using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class EngagementEndpoints
{
    public static IEndpointRouteBuilder MapEngagementEndpoints(this IEndpointRouteBuilder app)
    {
        var engagement = app.MapGroup("/api/engagement").RequireAuthorization();

        engagement.MapGet("/dashboard", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var forYou = await BuildForYou(db, userId);
            var streaks = await BuildStreaks(db, userId);
            var weekly = await BuildWeeklySummary(db, userId, DateOnly.FromDateTime(DateTime.UtcNow));
            var circleDigest = await BuildCircleDigest(db, userId);
            var suggestions = await BuildSuggestions(db, userId);
            var badges = await BuildUserBadges(db, userId, streaks);
            return Results.Ok(new EngagementDashboardDto(
                forYou,
                streaks,
                weekly,
                circleDigest,
                suggestions,
                badges,
                DailyWisdom(DateTime.UtcNow.DayOfYear)));
        });

        engagement.MapGet("/streaks", async (WithinDbContext db, ClaimsPrincipal principal) =>
            Results.Ok(await BuildStreaks(db, principal.UserId())));

        engagement.MapGet("/weekly-summary", async (WithinDbContext db, ClaimsPrincipal principal) =>
            Results.Ok(await BuildWeeklySummary(db, principal.UserId(), DateOnly.FromDateTime(DateTime.UtcNow))));

        engagement.MapGet("/badges", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            return Results.Ok(await BuildUserBadges(db, userId, await BuildStreaks(db, userId)));
        });

        engagement.MapPost("/reminders/run", async (WithinDbContext db, NotificationService notifications, ClaimsPrincipal principal) =>
        {
            if (!principal.IsInRole(nameof(WithinRole.Admin))) return Results.Forbid();
            var count = await SendReminderBatch(db, notifications);
            return Results.Ok(new { notificationsCreated = count });
        });

        app.MapGet("/api/providers/{providerId:guid}/follow-state", async (Guid providerId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.TryUserId();
            var count = await db.ProviderFollows.CountAsync(item => item.ProviderId == providerId);
            var following = userId is not null && await db.ProviderFollows.AnyAsync(item => item.ProviderId == providerId && item.UserId == userId);
            return Results.Ok(new ProviderFollowStateDto(providerId, following, count));
        });

        app.MapPost("/api/providers/{providerId:guid}/follow", async (Guid providerId, WithinDbContext db, NotificationService notifications, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var provider = await db.Providers.FirstOrDefaultAsync(item => item.Id == providerId && item.IsActive);
            if (provider is null) return Results.NotFound();
            if (!await db.ProviderFollows.AnyAsync(item => item.ProviderId == providerId && item.UserId == userId))
            {
                db.ProviderFollows.Add(new ProviderFollow { Id = Guid.NewGuid(), ProviderId = providerId, UserId = userId, CreatedAt = DateTimeOffset.UtcNow });
                await db.SaveChangesAsync();
                await notifications.NotifyProviderNewFollower(provider.OwnerUserId, userId, providerId);
            }
            var count = await db.ProviderFollows.CountAsync(item => item.ProviderId == providerId);
            return Results.Ok(new ProviderFollowStateDto(providerId, true, count));
        }).RequireAuthorization();

        app.MapDelete("/api/providers/{providerId:guid}/follow", async (Guid providerId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            await db.ProviderFollows.Where(item => item.ProviderId == providerId && item.UserId == userId).ExecuteDeleteAsync();
            var count = await db.ProviderFollows.CountAsync(item => item.ProviderId == providerId);
            return Results.Ok(new ProviderFollowStateDto(providerId, false, count));
        }).RequireAuthorization();

        var providerQuickWins = app.MapGroup("/api/providers/me/quick-wins").RequireAuthorization("ProviderOnly");

        providerQuickWins.MapGet("/profile-completeness", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var provider = await CurrentProvider(db, principal);
            if (provider is null) return Results.Forbid();
            return Results.Ok(await BuildProfileCompleteness(db, provider));
        });

        providerQuickWins.MapGet("/analytics", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var provider = await CurrentProvider(db, principal);
            if (provider is null) return Results.Forbid();
            return Results.Ok(await BuildProviderAnalytics(db, provider));
        });

        providerQuickWins.MapGet("/client-activity", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var provider = await CurrentProvider(db, principal);
            if (provider is null) return Results.Forbid();
            return Results.Ok(await BuildProviderClientActivity(db, provider.Id));
        });

        providerQuickWins.MapGet("/badges", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var provider = await CurrentProvider(db, principal);
            if (provider is null) return Results.Forbid();
            return Results.Ok(await BuildProviderBadges(db, provider));
        });

        return app;
    }

    public static async Task TrackProviderProfileView(WithinDbContext db, Guid providerId, Guid? viewerUserId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.ProviderProfileViews.Add(new ProviderProfileView
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            ViewerUserId = viewerUserId,
            ViewDate = today,
            ViewedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task<ForYouFeedItemDto[]> BuildForYou(WithinDbContext db, Guid userId)
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var items = new List<ForYouFeedItemDto>();

        var programIds = await db.AssignedPrograms
            .Where(item => item.ClientUserId == userId && item.Status == AssignedProgramStatus.Active)
            .Select(item => item.Id)
            .ToArrayAsync();
        var tasks = await db.AssignedProgramTasks
            .Where(item => programIds.Contains(item.AssignedProgramId) && item.ScheduledDate == today && item.Status == AssignedProgramTaskStatus.Pending)
            .OrderBy(item => item.SortOrder)
            .Take(4)
            .ToArrayAsync();
        items.AddRange(tasks.Select(task => new ForYouFeedItemDto(
            task.Id.ToString(),
            "TodayTask",
            task.Title,
            $"{TaskTypeLabel(task.TaskType)} due today",
            LensForTask(task.TaskType),
            now.AddMinutes(task.SortOrder),
            "/programs",
            task.Id,
            100)));

        var weekEnd = now.AddDays(7);
        var followedProviderIds = await db.ProviderFollows.Where(item => item.UserId == userId).Select(item => item.ProviderId).ToArrayAsync();
        var circleIds = await db.CircleMembers.Where(item => item.UserId == userId && item.Status == CircleMemberStatus.Active).Select(item => item.CircleId).ToArrayAsync();
        var circleEventIds = await db.CircleEvents.Where(item => circleIds.Contains(item.CircleId) && item.Status == CircleEventStatus.Active).Select(item => item.EventId).ToArrayAsync();
        var events = await db.Events
            .Where(item => item.Status == EventStatus.Published && item.StartUtc >= now && item.StartUtc <= weekEnd && (followedProviderIds.Contains(item.ProviderId) || circleEventIds.Contains(item.Id)))
            .OrderBy(item => item.StartUtc)
            .Take(4)
            .ToArrayAsync();
        items.AddRange(events.Select(evt => new ForYouFeedItemDto(evt.Id.ToString(), "UpcomingEvent", evt.Title, $"{evt.StartUtc.LocalDateTime:g} · {evt.LocationName}", evt.Lens, evt.StartUtc, $"/events/{evt.Id}", evt.Id, 80)));

        var threadItems = await (
            from thread in db.CircleThreads
            join circle in db.Circles on thread.CircleId equals circle.Id
            where circleIds.Contains(thread.CircleId) && thread.CreatedAt >= now.AddDays(-7) && thread.Status == CommunityContentStatus.Active
            orderby thread.CreatedAt descending
            select new { thread.Id, thread.CircleId, thread.Title, thread.Body, circle.Lens, thread.CreatedAt })
            .Take(3)
            .ToArrayAsync();
        items.AddRange(threadItems.Select(thread => new ForYouFeedItemDto(thread.Id.ToString(), "CircleActivity", thread.Title, thread.Body, thread.Lens, thread.CreatedAt, $"/circles/{thread.CircleId}", thread.CircleId, 60)));

        var activeChallenges = await (
            from participant in db.ChallengeParticipants
            join challenge in db.Challenges on participant.ChallengeId equals challenge.Id
            where participant.UserId == userId && (participant.Status == ChallengeParticipantStatus.Active || participant.Status == ChallengeParticipantStatus.Joined)
            orderby participant.JoinedAt descending
            select new { challenge.Id, challenge.Title, challenge.DurationDays, challenge.EndDate })
            .Take(2)
            .ToArrayAsync();
        foreach (var challenge in activeChallenges)
        {
            var completed = await db.ChallengeProgresses.CountAsync(item => item.ChallengeId == challenge.Id && item.UserId == userId && item.Status == ChallengeProgressStatus.Completed);
            var pct = challenge.DurationDays <= 0 ? 0 : Math.Min(100, (int)Math.Round((double)completed / challenge.DurationDays * 100));
            var daysLeft = challenge.EndDate is null ? challenge.DurationDays - completed : Math.Max(0, challenge.EndDate.Value.DayNumber - today.DayNumber);
            items.Add(new ForYouFeedItemDto(challenge.Id.ToString(), "ChallengeProgress", challenge.Title, $"{pct}% complete · {daysLeft} days remaining", WithinLens.Move, now.AddHours(1), "/move/challenges", challenge.Id, 55));
        }

        items.Add(new ForYouFeedItemDto("daily-wisdom", "DailyWisdom", "Daily wisdom", DailyWisdom(DateTime.UtcNow.DayOfYear), WithinLens.Feel, now.AddHours(2), null, null, 20));
        items.Add(new ForYouFeedItemDto("journey-progress", "JourneyProgress", "Continue journey", "Pick up your latest Feel or Seek journey.", WithinLens.Seek, now.AddHours(3), "/(tabs)/journey", null, 15));

        return items.OrderByDescending(item => item.Priority).ThenBy(item => item.SortAt).Take(12).ToArray();
    }

    private static async Task<StreakSummaryDto> BuildStreaks(WithinDbContext db, Guid userId)
    {
        var checkIns = await db.DailyCheckIns.Where(item => item.UserId == userId).ToArrayAsync();
        var workoutDates = await db.WorkoutLogs.Where(item => item.UserId == userId && item.Status == WorkoutLogStatus.Completed).Select(item => item.LogDate).ToArrayAsync();
        var programReflectionDates = await (
            from task in db.AssignedProgramTasks
            join program in db.AssignedPrograms on task.AssignedProgramId equals program.Id
            where program.ClientUserId == userId && task.Status == AssignedProgramTaskStatus.Completed && (task.TaskType == ProgramTaskType.Reflection || task.TaskType == ProgramTaskType.Meditation)
            select new { task.ScheduledDate, task.TaskType })
            .ToArrayAsync();

        var daily = Streak(checkIns.Select(item => item.CheckInDate));
        var journal = Streak(checkIns.Where(item => !string.IsNullOrWhiteSpace(item.JournalEntry) || !string.IsNullOrWhiteSpace(item.Note)).Select(item => item.CheckInDate));
        var meditation = Streak(checkIns.Where(item => item.DidMeditateToday).Select(item => item.CheckInDate).Concat(programReflectionDates.Where(item => item.TaskType == ProgramTaskType.Meditation).Select(item => item.ScheduledDate)));
        var workout = Streak(workoutDates);
        var reflection = Streak(programReflectionDates.Where(item => item.TaskType == ProgramTaskType.Reflection).Select(item => item.ScheduledDate));
        var best = new[] { daily, journal, meditation, workout, reflection }.Max();
        return new StreakSummaryDto(daily, journal, meditation, workout, reflection, best, best == 1 ? "1 Day Streak" : $"{best} Day Streak", best > 0 && !checkIns.Any(item => item.CheckInDate == DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    private static async Task<WeeklyProgressSummaryDto> BuildWeeklySummary(WithinDbContext db, Guid userId, DateOnly today)
    {
        var weekStart = today.AddDays(-((int)today.DayOfWeek));
        var weekEnd = weekStart.AddDays(6);
        var startUtc = new DateTimeOffset(weekStart.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endUtc = new DateTimeOffset(weekEnd.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
        var eventsAttended = await db.EventRegistrations.CountAsync(item => item.UserId == userId && item.State == EventJoinState.Attended && db.Events.Any(evt => evt.Id == item.EventId && evt.StartUtc >= startUtc && evt.StartUtc <= endUtc));
        var challengesCompleted = await db.ChallengeParticipants.CountAsync(item => item.UserId == userId && item.Status == ChallengeParticipantStatus.Completed && item.CompletedAt >= startUtc && item.CompletedAt <= endUtc);
        var checkIns = await db.DailyCheckIns.Where(item => item.UserId == userId && item.CheckInDate >= weekStart && item.CheckInDate <= weekEnd).ToArrayAsync();
        var programTasksCompleted = await (
            from task in db.AssignedProgramTasks
            join program in db.AssignedPrograms on task.AssignedProgramId equals program.Id
            where program.ClientUserId == userId && task.Status == AssignedProgramTaskStatus.Completed && task.ScheduledDate >= weekStart && task.ScheduledDate <= weekEnd
            select task.Id)
            .CountAsync();
        return new WeeklyProgressSummaryDto(
            weekStart,
            weekEnd,
            eventsAttended,
            challengesCompleted,
            checkIns.Length,
            checkIns.Count(item => !string.IsNullOrWhiteSpace(item.JournalEntry)),
            checkIns.Count(item => item.DidMeditateToday) * 10,
            programTasksCompleted);
    }

    private static async Task<CircleActivityDigestDto> BuildCircleDigest(WithinDbContext db, Guid userId)
    {
        var since = DateTimeOffset.UtcNow.AddDays(-7);
        var circleIds = await db.CircleMembers.Where(item => item.UserId == userId && item.Status == CircleMemberStatus.Active).Select(item => item.CircleId).ToArrayAsync();
        var threads = await (
            from thread in db.CircleThreads
            join circle in db.Circles on thread.CircleId equals circle.Id
            where circleIds.Contains(thread.CircleId) && thread.CreatedAt >= since && thread.Status == CommunityContentStatus.Active
            orderby thread.CreatedAt descending
            select new { thread.Id, thread.CircleId, thread.Title, thread.Body, circle.Lens, thread.CreatedAt })
            .Take(5)
            .ToArrayAsync();
        var threadIds = await db.CircleThreads.Where(item => circleIds.Contains(item.CircleId)).Select(item => item.Id).ToArrayAsync();
        var comments = await db.CircleThreadComments.CountAsync(item => threadIds.Contains(item.ThreadId) && item.CreatedAt >= since && item.Status == CommunityContentStatus.Active);
        var upcoming = await db.CircleEvents.CountAsync(item => circleIds.Contains(item.CircleId) && item.Status == CircleEventStatus.Active && db.Events.Any(evt => evt.Id == item.EventId && evt.StartUtc >= DateTimeOffset.UtcNow));
        var items = threads.Select(item => new ForYouFeedItemDto(item.Id.ToString(), "CircleDiscussion", item.Title, item.Body, item.Lens, item.CreatedAt, $"/circles/{item.CircleId}", item.CircleId, 50)).ToArray();
        return new CircleActivityDigestDto(threads.Length, comments, upcoming, items);
    }

    private static async Task<SuggestedContentDto> BuildSuggestions(WithinDbContext db, Guid userId)
    {
        var interests = await db.UserWellbeingInterests.Where(item => item.UserId == userId).Select(item => item.InterestKey).ToArrayAsync();
        var followedProviderIds = await db.ProviderFollows.Where(item => item.UserId == userId).Select(item => item.ProviderId).ToArrayAsync();
        var attendedProviderIds = await (
            from registration in db.EventRegistrations
            join evt in db.Events on registration.EventId equals evt.Id
            where registration.UserId == userId && (registration.State == EventJoinState.Going || registration.State == EventJoinState.Attended)
            select evt.ProviderId)
            .Distinct()
            .ToArrayAsync();
        var providerIds = followedProviderIds.Concat(attendedProviderIds).Distinct().ToArray();
        var eventCandidates = db.Events.Where(item => item.Status == EventStatus.Published && item.StartUtc >= DateTimeOffset.UtcNow);
        if (providerIds.Length > 0 || interests.Length > 0)
        {
            eventCandidates = eventCandidates.Where(item => providerIds.Contains(item.ProviderId) || item.Tags.Any(tag => interests.Contains(tag)));
        }
        var events = await ApiMapping.ProjectEvents(eventCandidates.OrderBy(item => item.StartUtc).Take(6), db, userId).ToArrayAsync();

        var joinedCircleIds = await db.CircleMembers.Where(item => item.UserId == userId).Select(item => item.CircleId).ToArrayAsync();
        var circles = await db.Circles
            .Where(item => !joinedCircleIds.Contains(item.Id) && item.Status == CircleStatus.Active && item.Visibility == CircleVisibility.Public)
            .OrderByDescending(item => db.CircleMembers.Count(member => member.CircleId == item.Id && member.Status == CircleMemberStatus.Active))
            .Take(6)
            .Select(circle => new CircleDto(circle.Id, circle.Name, circle.Slug, circle.Description, circle.Rules, circle.CreatedByUserId, circle.Type, circle.Visibility, circle.Status, circle.Lens, db.CircleMembers.Count(member => member.CircleId == circle.Id && member.Status == CircleMemberStatus.Active), db.CircleThreads.Count(thread => thread.CircleId == circle.Id && thread.Status == CommunityContentStatus.Active), db.CircleEvents.Count(evt => evt.CircleId == circle.Id && evt.Status == CircleEventStatus.Active), false, false, null, false, circle.AllowAnonymousPosts, circle.RequiresApproval))
            .ToArrayAsync();

        return new SuggestedContentDto(events, circles);
    }

    private static async Task<AchievementBadgeDto[]> BuildUserBadges(WithinDbContext db, Guid userId, StreakSummaryDto streaks)
    {
        return new[]
        {
            new AchievementBadgeDto("first_event", "First Event", "RSVP to your first event.", await db.EventRegistrations.AnyAsync(item => item.UserId == userId && item.State != EventJoinState.Declined)),
            new AchievementBadgeDto("first_circle", "First Circle", "Join your first circle.", await db.CircleMembers.AnyAsync(item => item.UserId == userId && item.Status == CircleMemberStatus.Active)),
            new AchievementBadgeDto("first_program", "First Program", "Start your first provider program.", await db.AssignedPrograms.AnyAsync(item => item.ClientUserId == userId)),
            new AchievementBadgeDto("streak_7", "7 Day Streak", "Show up for seven days.", streaks.Best >= 7),
            new AchievementBadgeDto("streak_30", "30 Day Streak", "Show up for thirty days.", streaks.Best >= 30)
        };
    }

    private static async Task<ProviderProfileCompletenessDto> BuildProfileCompleteness(WithinDbContext db, Provider provider)
    {
        var checks = new List<(string Label, bool Done)>
        {
            ("Profile photo", !string.IsNullOrWhiteSpace(provider.ProfileImageUrl)),
            ("Bio", provider.Bio.Trim().Length >= 80),
            ("Services", await db.ProviderServices.AnyAsync(item => item.ProviderId == provider.Id && item.IsActive)),
            ("Certifications", !string.IsNullOrWhiteSpace(provider.Qualifications)),
            ("Programs", await db.ProgramTemplates.AnyAsync(item => item.ProviderId == provider.Id)),
            ("Events", await db.Events.AnyAsync(item => item.ProviderId == provider.Id))
        };
        var completed = checks.Where(item => item.Done).Select(item => item.Label).ToArray();
        var recommendations = checks.Where(item => !item.Done).Select(item => $"Add {item.Label.ToLowerInvariant()}.").ToArray();
        return new ProviderProfileCompletenessDto((int)Math.Round((double)completed.Length / checks.Count * 100), completed, recommendations);
    }

    private static async Task<ProviderAnalyticsDto> BuildProviderAnalytics(WithinDbContext db, Provider provider)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var views = await db.ProviderProfileViews.CountAsync(item => item.ProviderId == provider.Id && item.ViewDate >= since);
        var followers = await db.ProviderFollows.CountAsync(item => item.ProviderId == provider.Id);
        var providerCircleIds = await db.Circles.Where(item => item.CreatedByUserId == provider.OwnerUserId).Select(item => item.Id).ToArrayAsync();
        var circleMembers = await db.CircleMembers.CountAsync(member => providerCircleIds.Contains(member.CircleId) && member.Status == CircleMemberStatus.Active);
        var eventsCreated = await db.Events.CountAsync(item => item.ProviderId == provider.Id);
        var registrations = await db.EventRegistrations.CountAsync(item => db.Events.Any(evt => evt.Id == item.EventId && evt.ProviderId == provider.Id));
        var programs = await db.AssignedPrograms.Where(item => item.ProviderId == provider.Id).ToArrayAsync();
        var completionRate = programs.Length == 0 ? 0 : (int)Math.Round((double)programs.Count(item => item.Status == AssignedProgramStatus.Completed) / programs.Length * 100);
        return new ProviderAnalyticsDto(views, views * 3, followers, circleMembers, eventsCreated, registrations, programs.Count(item => item.Status == AssignedProgramStatus.Active), programs.Length, completionRate);
    }

    private static async Task<ProviderClientActivityDto> BuildProviderClientActivity(WithinDbContext db, Guid providerId)
    {
        var programs = await db.AssignedPrograms.Where(item => item.ProviderId == providerId).OrderByDescending(item => item.UpdatedAt).ToArrayAsync();
        var clientIds = programs.Select(item => item.ClientUserId).Distinct().ToArray();
        var users = await db.Users.Where(item => clientIds.Contains(item.Id)).ToDictionaryAsync(item => item.Id, item => item.DisplayName);
        var recent = new List<ProviderProgramActivityDto>();
        foreach (var program in programs.Take(6))
        {
            var total = await db.AssignedProgramTasks.CountAsync(item => item.AssignedProgramId == program.Id);
            var complete = await db.AssignedProgramTasks.CountAsync(item => item.AssignedProgramId == program.Id && item.Status == AssignedProgramTaskStatus.Completed);
            recent.Add(new ProviderProgramActivityDto(program.Id, program.ClientUserId, users.GetValueOrDefault(program.ClientUserId, "Client"), program.Title, program.Status, total == 0 ? 0 : (int)Math.Round((double)complete / total * 100), program.UpdatedAt));
        }

        var pendingCheckIns = await db.ClientCheckIns
            .Where(item => item.ProviderId == providerId && item.ProviderFeedback == null)
            .OrderByDescending(item => item.CheckInDate)
            .Take(8)
            .Select(item => new ProviderCheckInActivityDto(item.Id, item.AssignedProgramId, item.ClientUserId, users.GetValueOrDefault(item.ClientUserId, "Client"), item.CheckInDate, item.Mood, item.ClientNotes))
            .ToArrayAsync();

        var attention = new List<ProviderClientAttentionDto>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var program in programs.Where(item => item.Status == AssignedProgramStatus.Active || item.Status == AssignedProgramStatus.Paused).Take(20))
        {
            var name = users.GetValueOrDefault(program.ClientUserId, "Client");
            if (program.Status == AssignedProgramStatus.Paused) attention.Add(new ProviderClientAttentionDto(program.ClientUserId, name, "Program paused", program.Id));
            var lastCheckIn = await db.ClientCheckIns.Where(item => item.AssignedProgramId == program.Id).MaxAsync(item => (DateOnly?)item.CheckInDate);
            if (lastCheckIn is null || lastCheckIn.Value <= today.AddDays(-7)) attention.Add(new ProviderClientAttentionDto(program.ClientUserId, name, "No check-in submitted in 7 days", program.Id));
        }

        return new ProviderClientActivityDto(programs.Count(item => item.Status == AssignedProgramStatus.Active), recent.ToArray(), pendingCheckIns, attention.Take(10).ToArray());
    }

    private static async Task<AchievementBadgeDto[]> BuildProviderBadges(WithinDbContext db, Provider provider)
    {
        var attendeeCount = await db.EventRegistrations.CountAsync(item => db.Events.Any(evt => evt.Id == item.EventId && evt.ProviderId == provider.Id) && item.State != EventJoinState.Declined);
        return new[]
        {
            new AchievementBadgeDto("first_client", "First Client", "Assign your first client program.", await db.AssignedPrograms.AnyAsync(item => item.ProviderId == provider.Id)),
            new AchievementBadgeDto("first_program", "First Program", "Create your first program template.", await db.ProgramTemplates.AnyAsync(item => item.ProviderId == provider.Id)),
            new AchievementBadgeDto("first_event", "First Event", "Create your first event.", await db.Events.AnyAsync(item => item.ProviderId == provider.Id)),
            new AchievementBadgeDto("hundred_attendees", "100 Attendees", "Reach 100 event registrations.", attendeeCount >= 100),
            new AchievementBadgeDto("top_rated", "Top Rated Provider", "Maintain a 4.5+ average rating.", await db.Reviews.AnyAsync(item => db.Events.Any(evt => evt.Id == item.EventId && evt.ProviderId == provider.Id)) && await db.Reviews.Where(item => db.Events.Any(evt => evt.Id == item.EventId && evt.ProviderId == provider.Id)).AverageAsync(item => item.Rating) >= 4.5)
        };
    }

    private static async Task<int> SendReminderBatch(WithinDbContext db, NotificationService notifications)
    {
        var created = 0;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var users = await db.Users.Where(item => !item.IsDeleted).ToArrayAsync();
        foreach (var user in users)
        {
            var lastCheckIn = await db.DailyCheckIns.Where(item => item.UserId == user.Id).MaxAsync(item => (DateOnly?)item.CheckInDate);
            if (lastCheckIn is not null && lastCheckIn.Value == today.AddDays(-1) && !await db.DailyCheckIns.AnyAsync(item => item.UserId == user.Id && item.CheckInDate == today))
            {
                await notifications.CreateAsync(new NotificationCreateRequest(user.Id, NotificationKind.StreakAtRisk, "Your streak is at risk", "Check in today to keep your streak alive.", NotificationTargetType.Profile, user.Id));
                created++;
            }
            if (lastCheckIn is null || lastCheckIn.Value <= today.AddDays(-7))
            {
                await notifications.CreateAsync(new NotificationCreateRequest(user.Id, NotificationKind.ReengagementReminder, "New things are waiting", "We've found new events, circles and tasks you may enjoy.", NotificationTargetType.Profile, user.Id));
                created++;
            }
            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                var summary = await BuildWeeklySummary(db, user.Id, today);
                await notifications.CreateAsync(new NotificationCreateRequest(
                    user.Id,
                    NotificationKind.WeeklyProgressSummary,
                    "Your weekly progress is ready",
                    $"{summary.CheckInsCompleted} check-ins, {summary.ProgramTasksCompleted} program tasks and {summary.MeditationMinutes} meditation minutes this week.",
                    NotificationTargetType.Profile,
                    user.Id));
                created++;
            }
        }

        var overduePrograms = await db.AssignedPrograms.Where(item => item.Status == AssignedProgramStatus.Active).ToArrayAsync();
        foreach (var program in overduePrograms)
        {
            var hasOverdue = await db.AssignedProgramTasks.AnyAsync(item => item.AssignedProgramId == program.Id && item.ScheduledDate <= today && item.Status == AssignedProgramTaskStatus.Pending);
            if (!hasOverdue) continue;
            await notifications.CreateAsync(new NotificationCreateRequest(program.ClientUserId, NotificationKind.ProgramOverdue, "Today's tasks are waiting", $"{program.Title} has tasks ready for you.", NotificationTargetType.Program, program.Id));
            created++;
        }

        var providers = await db.Providers.Where(item => item.IsActive).ToArrayAsync();
        foreach (var provider in providers)
        {
            var pendingCheckIns = await db.ClientCheckIns.CountAsync(item => item.ProviderId == provider.Id && item.ProviderFeedback == null);
            if (pendingCheckIns == 0) continue;
            await notifications.CreateAsync(new NotificationCreateRequest(
                provider.OwnerUserId,
                NotificationKind.ReengagementReminder,
                "Pending client check-ins",
                $"You have {pendingCheckIns} pending client check-ins.",
                NotificationTargetType.Profile,
                provider.Id));
            created++;
        }
        return created;
    }

    private static async Task<Provider?> CurrentProvider(WithinDbContext db, ClaimsPrincipal principal) =>
        await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == principal.UserId());

    private static int Streak(IEnumerable<DateOnly> dates)
    {
        var set = dates.Distinct().ToHashSet();
        var day = DateOnly.FromDateTime(DateTime.UtcNow);
        var streak = 0;
        while (set.Contains(day))
        {
            streak++;
            day = day.AddDays(-1);
        }
        return streak;
    }

    private static string TaskTypeLabel(ProgramTaskType type) => type switch
    {
        ProgramTaskType.Exercise => "Workout",
        ProgramTaskType.Meditation => "Meditation",
        ProgramTaskType.Reflection => "Reflection",
        ProgramTaskType.Meal => "Meal",
        _ => "Task"
    };

    private static WithinLens LensForTask(ProgramTaskType type) => type switch
    {
        ProgramTaskType.Exercise or ProgramTaskType.Meal or ProgramTaskType.YogaPose => WithinLens.Move,
        ProgramTaskType.Meditation or ProgramTaskType.Reading => WithinLens.Seek,
        _ => WithinLens.Feel
    };

    private static string DailyWisdom(int day) => (day % 3) switch
    {
        0 => "Move: a small walk still counts.",
        1 => "Feel: name what is true before trying to fix it.",
        _ => "Seek: one quiet question can change the shape of the day."
    };
}
