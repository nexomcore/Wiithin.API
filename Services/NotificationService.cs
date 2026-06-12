using Microsoft.EntityFrameworkCore;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;

namespace WithinAPI.Services;

public sealed record NotificationCreateRequest(
    Guid UserId,
    NotificationKind Kind,
    string Title,
    string Body,
    NotificationTargetType? TargetType = null,
    Guid? TargetId = null,
    Guid? ActorUserId = null,
    Guid? CircleId = null,
    Guid? EventId = null,
    Guid? RelatedUserId = null);

public sealed class NotificationService(WithinDbContext db)
{
    public async Task<Notification?> CreateAsync(NotificationCreateRequest request)
    {
        if (request.ActorUserId == request.UserId) return null;
        if (!await PreferencesAllow(request.UserId, request.Kind)) return null;
        if (await IsMuted(request.UserId, request.CircleId, request.EventId, request.ActorUserId ?? request.RelatedUserId)) return null;

        var now = DateTimeOffset.UtcNow;
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ActorUserId = request.ActorUserId,
            Kind = request.Kind,
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            TargetType = request.TargetType,
            TargetId = request.TargetId,
            CircleId = request.CircleId,
            EventId = request.EventId,
            RelatedUserId = request.RelatedUserId,
            CreatedUtc = now
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
        return notification;
    }

    public async Task NotifyFriendRequestReceived(Guid receiverUserId, Guid requesterUserId, Guid connectionId)
    {
        var actorName = await UserName(requesterUserId);
        await CreateAsync(new NotificationCreateRequest(
            receiverUserId,
            NotificationKind.FriendRequestReceived,
            "New friend request",
            $"{actorName} sent you a friend request.",
            NotificationTargetType.Connection,
            connectionId,
            requesterUserId,
            RelatedUserId: requesterUserId));
    }

    public async Task NotifyFriendRequestAccepted(Guid requesterUserId, Guid receiverUserId, Guid connectionId)
    {
        var actorName = await UserName(receiverUserId);
        await CreateAsync(new NotificationCreateRequest(
            requesterUserId,
            NotificationKind.FriendRequestAccepted,
            "Friend request accepted",
            $"{actorName} accepted your friend request.",
            NotificationTargetType.Profile,
            receiverUserId,
            receiverUserId,
            RelatedUserId: receiverUserId));
    }

    public async Task NotifyEventInvite(Guid invitedUserId, Guid inviterUserId, Guid eventId, Guid inviteId)
    {
        var actorName = await UserName(inviterUserId);
        var eventTitle = await EventTitle(eventId);
        await CreateAsync(new NotificationCreateRequest(
            invitedUserId,
            NotificationKind.EventInvite,
            "Event invite",
            $"{actorName} invited you to {eventTitle}.",
            NotificationTargetType.Event,
            eventId,
            inviterUserId,
            EventId: eventId,
            RelatedUserId: inviterUserId));
    }

    public async Task NotifyPublicFriendRsvp(Guid actorUserId, Guid eventId)
    {
        var registration = await db.EventRegistrations.FirstOrDefaultAsync(item => item.EventId == eventId && item.UserId == actorUserId);
        if (registration is null || registration.State != EventJoinState.Going || registration.Visibility != RsvpVisibility.Public) return;

        var actorSettings = await db.UserPrivacySettings.FindAsync(actorUserId);
        if (actorSettings?.ShowActivityToFriends == false) return;

        var eventTitle = await EventTitle(eventId);
        var actorName = await UserName(actorUserId);
        var friendIds = await db.Connections
            .Where(item => item.Status == ConnectionStatus.Accepted && (item.RequesterUserId == actorUserId || item.ReceiverUserId == actorUserId))
            .Select(item => item.RequesterUserId == actorUserId ? item.ReceiverUserId : item.RequesterUserId)
            .ToArrayAsync();

        foreach (var friendId in friendIds)
        {
            await CreateAsync(new NotificationCreateRequest(
                friendId,
                NotificationKind.PublicFriendRsvp,
                "Friend RSVP",
                $"{actorName} is going to {eventTitle}.",
                NotificationTargetType.Event,
                eventId,
                actorUserId,
                EventId: eventId,
                RelatedUserId: actorUserId));
        }
    }

    public async Task NotifyCircleThreadReply(Guid threadId, Guid commentId, Guid actorUserId)
    {
        var thread = await db.CircleThreads.FindAsync(threadId);
        if (thread is null || thread.UserId == actorUserId) return;
        var actorName = await CircleDisplayName(thread.CircleId, actorUserId);
        await CreateAsync(new NotificationCreateRequest(
            thread.UserId,
            NotificationKind.CircleThreadReply,
            "New circle reply",
            $"{actorName} replied to your thread.",
            NotificationTargetType.CircleThread,
            threadId,
            actorUserId,
            CircleId: thread.CircleId,
            RelatedUserId: actorUserId));
    }

    public async Task NotifyCircleCommentReply(Guid threadId, Guid parentCommentId, Guid replyCommentId, Guid actorUserId)
    {
        var parent = await db.CircleThreadComments.FindAsync(parentCommentId);
        if (parent is null || parent.UserId == actorUserId) return;
        var thread = await db.CircleThreads.FindAsync(threadId);
        if (thread is null) return;
        var actorName = await CircleDisplayName(thread.CircleId, actorUserId);
        await CreateAsync(new NotificationCreateRequest(
            parent.UserId,
            NotificationKind.CommentReply,
            "New comment reply",
            $"{actorName} replied to your comment.",
            NotificationTargetType.CircleThread,
            threadId,
            actorUserId,
            CircleId: thread.CircleId,
            RelatedUserId: actorUserId));
    }

    public async Task NotifyCommentReply(Guid parentCommentId, Guid replyCommentId, Guid actorUserId, Guid eventId)
    {
        var parent = await db.Comments.FindAsync(parentCommentId);
        if (parent is null || parent.AuthorUserId == actorUserId) return;
        var actorName = await UserName(actorUserId);
        await CreateAsync(new NotificationCreateRequest(
            parent.AuthorUserId,
            NotificationKind.CommentReply,
            "New comment reply",
            $"{actorName} replied to your comment.",
            NotificationTargetType.Event,
            eventId,
            actorUserId,
            EventId: eventId,
            RelatedUserId: actorUserId));
    }

    public async Task NotifyEventReminder(Guid userId, Guid eventId, DateTimeOffset startUtc)
    {
        await CreateAsync(new NotificationCreateRequest(
            userId,
            NotificationKind.EventReminder,
            "Event reminder",
            $"{await EventTitle(eventId)} starts {startUtc.LocalDateTime:g}.",
            NotificationTargetType.Event,
            eventId,
            EventId: eventId));
    }

    public async Task NotifyCircleJoinApproved(Guid circleId, Guid memberUserId, Guid approvedByUserId)
    {
        var circleName = await db.Circles.Where(item => item.Id == circleId).Select(item => item.Name).FirstOrDefaultAsync() ?? "Circle";
        await CreateAsync(new NotificationCreateRequest(
            memberUserId,
            NotificationKind.CircleJoinApproved,
            "Circle request approved",
            $"You've been approved to join {circleName}. Tap to open the circle.",
            NotificationTargetType.Circle,
            circleId,
            approvedByUserId,
            CircleId: circleId));
    }

    public async Task NotifyCircleJoinRequest(Guid circleId, Guid requesterUserId)
    {
        var circleName = await db.Circles.Where(item => item.Id == circleId).Select(item => item.Name).FirstOrDefaultAsync() ?? "Circle";
        var requesterName = await UserName(requesterUserId);
        var adminIds = await db.CircleMembers
            .Where(item => item.CircleId == circleId && item.Status == CircleMemberStatus.Active && item.Role == CircleMemberRole.Admin)
            .Select(item => item.UserId)
            .Distinct()
            .ToArrayAsync();

        foreach (var adminId in adminIds)
        {
            await CreateAsync(new NotificationCreateRequest(
                adminId,
                NotificationKind.CircleJoinRequest,
                "Circle join request",
                $"{requesterName} requested to join {circleName}.",
                NotificationTargetType.Circle,
                circleId,
                requesterUserId,
                CircleId: circleId,
                RelatedUserId: requesterUserId));
        }
    }

    public async Task NotifyCircleInvite(Guid invitedUserId, Guid inviterUserId, Guid circleId, Guid inviteId)
    {
        var actorName = await UserName(inviterUserId);
        var circleName = await db.Circles.Where(item => item.Id == circleId).Select(item => item.Name).FirstOrDefaultAsync() ?? "Circle";
        await CreateAsync(new NotificationCreateRequest(
            invitedUserId,
            NotificationKind.CircleInvite,
            "Circle invite",
            $"{actorName} invited you to join {circleName}.",
            NotificationTargetType.Circle,
            circleId,
            inviterUserId,
            CircleId: circleId,
            RelatedUserId: inviterUserId));
    }

    public async Task NotifyProgramAssigned(Guid clientUserId, Guid providerOwnerUserId, Guid assignedProgramId, string programTitle)
    {
        var providerName = await UserName(providerOwnerUserId);
        await CreateAsync(new NotificationCreateRequest(
            clientUserId,
            NotificationKind.ProgramAssigned,
            "New program assigned",
            $"{providerName} assigned you {programTitle}.",
            NotificationTargetType.Program,
            assignedProgramId,
            providerOwnerUserId,
            RelatedUserId: providerOwnerUserId));
    }

    public async Task NotifyProgramUpdated(Guid clientUserId, Guid providerOwnerUserId, Guid assignedProgramId, string programTitle)
    {
        await CreateAsync(new NotificationCreateRequest(
            clientUserId,
            NotificationKind.ProgramUpdated,
            "Program updated",
            $"{programTitle} has been updated.",
            NotificationTargetType.Program,
            assignedProgramId,
            providerOwnerUserId,
            RelatedUserId: providerOwnerUserId));
    }

    public async Task NotifyProgramTaskDue(Guid clientUserId, Guid taskId, string taskTitle)
    {
        await CreateAsync(new NotificationCreateRequest(
            clientUserId,
            NotificationKind.ProgramTaskDue,
            "Task due today",
            $"{taskTitle} is due today.",
            NotificationTargetType.ProgramTask,
            taskId));
    }

    public async Task NotifyProgramFeedbackAdded(Guid clientUserId, Guid providerOwnerUserId, Guid targetId, string title)
    {
        await CreateAsync(new NotificationCreateRequest(
            clientUserId,
            NotificationKind.ProgramFeedbackAdded,
            "Provider feedback added",
            title,
            NotificationTargetType.Program,
            targetId,
            providerOwnerUserId,
            RelatedUserId: providerOwnerUserId));
    }

    public async Task NotifyProgramCompleted(Guid providerOwnerUserId, Guid clientUserId, Guid assignedProgramId, string programTitle)
    {
        var clientName = await UserName(clientUserId);
        await CreateAsync(new NotificationCreateRequest(
            providerOwnerUserId,
            NotificationKind.ProgramCompleted,
            "Program completed",
            $"{clientName} completed {programTitle}.",
            NotificationTargetType.Program,
            assignedProgramId,
            clientUserId,
            RelatedUserId: clientUserId));
    }

    public async Task ScheduleEventReminders(Guid userId, Event evt, EventJoinState state)
    {
        await db.NotificationSchedules
            .Where(item => item.UserId == userId &&
                           item.EventId == evt.Id &&
                           item.SentUtc == null &&
                           (item.Kind == NotificationKind.EventReminder24h || item.Kind == NotificationKind.EventReminder2h || item.Kind == NotificationKind.EventReminder))
            .ExecuteDeleteAsync();

        if (state != EventJoinState.Going) return;
        if (!await PreferencesAllow(userId, NotificationKind.EventReminder)) return;
        if (await IsMuted(userId, null, evt.Id, null)) return;

        var now = DateTimeOffset.UtcNow;
        AddReminder(userId, evt, NotificationKind.EventReminder24h, evt.StartUtc.AddHours(-24), now);
        AddReminder(userId, evt, NotificationKind.EventReminder2h, evt.StartUtc.AddHours(-2), now);
        await db.SaveChangesAsync();
    }

    public async Task NotifyMentions(Guid actorUserId, string body, MentionSourceType sourceType, Guid sourceId, Guid? circleId, Guid? eventId)
    {
        var users = await db.Users.Where(item => item.Id != actorUserId).ToArrayAsync();
        foreach (var user in users)
        {
            if (!body.Contains("@" + user.DisplayName, StringComparison.OrdinalIgnoreCase)) continue;
            if (await db.Mentions.AnyAsync(item => item.MentionedUserId == user.Id && item.SourceType == sourceType && item.SourceId == sourceId)) continue;

            db.Mentions.Add(new Mention
            {
                Id = Guid.NewGuid(),
                MentionedUserId = user.Id,
                MentionedByUserId = actorUserId,
                SourceType = sourceType,
                SourceId = sourceId,
                CreatedAt = DateTimeOffset.UtcNow
            });

            var actorName = circleId is null ? await UserName(actorUserId) : await CircleDisplayName(circleId.Value, actorUserId);
            await CreateAsync(new NotificationCreateRequest(
                user.Id,
                NotificationKind.UserMention,
                "You were mentioned",
                $"{actorName} mentioned you.",
                sourceType == MentionSourceType.CirclePost ? NotificationTargetType.CircleThread : eventId is not null ? NotificationTargetType.Event : NotificationTargetType.CommunityPost,
                sourceId,
                actorUserId,
                CircleId: circleId,
                EventId: eventId,
                RelatedUserId: actorUserId));
        }
    }

    public NotificationDto ToDto(Notification notification) => new(
        notification.Id,
        notification.Kind,
        notification.Title,
        notification.Body,
        notification.TargetType,
        notification.TargetId,
        notification.ActorUserId,
        notification.CircleId,
        notification.EventId,
        notification.RelatedUserId,
        notification.IsRead,
        notification.CreatedUtc,
        notification.ReadUtc);

    private async Task<bool> PreferencesAllow(Guid userId, NotificationKind kind)
    {
        var prefs = await db.NotificationPreferences.FirstOrDefaultAsync(item => item.UserId == userId);
        if (prefs is null) return true;

        return kind switch
        {
            NotificationKind.DailyMotivation => prefs.DailyMotivationEnabled,
            NotificationKind.EventReminder or NotificationKind.EventReminder24h or NotificationKind.EventReminder2h => prefs.EventRemindersEnabled,
            NotificationKind.CommunitySummary => prefs.CommunitySummariesEnabled,
            NotificationKind.ProviderNewEvent => prefs.ProviderNewEventsEnabled,
            NotificationKind.FriendRequestReceived or NotificationKind.FriendRequestAccepted => prefs.FriendRequestsEnabled,
            NotificationKind.EventInvite => prefs.EventInvitesEnabled,
            NotificationKind.PublicFriendRsvp => prefs.FriendActivityEnabled,
            NotificationKind.CircleThreadReply or NotificationKind.CircleInvite => prefs.CircleRepliesEnabled,
            NotificationKind.CommentReply => prefs.CommentRepliesEnabled,
            NotificationKind.UserMention => prefs.MentionsEnabled,
            _ => true
        };
    }

    private async Task<bool> IsMuted(Guid userId, Guid? circleId, Guid? eventId, Guid? actorUserId)
    {
        if (circleId is not null && await db.NotificationMutes.AnyAsync(item => item.UserId == userId && item.TargetType == NotificationMuteTargetType.Circle && item.TargetId == circleId)) return true;
        if (eventId is not null && await db.NotificationMutes.AnyAsync(item => item.UserId == userId && item.TargetType == NotificationMuteTargetType.Event && item.TargetId == eventId)) return true;
        if (actorUserId is not null && await db.NotificationMutes.AnyAsync(item => item.UserId == userId && item.TargetType == NotificationMuteTargetType.User && item.TargetId == actorUserId)) return true;
        return false;
    }

    private void AddReminder(Guid userId, Event evt, NotificationKind kind, DateTimeOffset sendAtUtc, DateTimeOffset now)
    {
        if (sendAtUtc <= now) return;
        var label = kind == NotificationKind.EventReminder24h ? "tomorrow" : "soon";
        db.NotificationSchedules.Add(new NotificationSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventId = evt.Id,
            Kind = kind,
            Title = "Event reminder",
            Body = $"{evt.Title} starts {label}.",
            SendAtUtc = sendAtUtc
        });
    }

    private async Task<string> UserName(Guid userId) =>
        await db.Users.Where(item => item.Id == userId).Select(item => item.DisplayName).FirstOrDefaultAsync() ?? "Within user";

    private async Task<string> EventTitle(Guid eventId) =>
        await db.Events.Where(item => item.Id == eventId).Select(item => item.Title).FirstOrDefaultAsync() ?? "an event";

    private async Task<string> CircleDisplayName(Guid circleId, Guid userId)
    {
        var userName = await UserName(userId);
        var member = await db.CircleMembers.FirstOrDefaultAsync(item => item.CircleId == circleId && item.UserId == userId);
        return ProfileAccessRules.ResolveCircleIdentity(userName, member?.IdentityMode, member?.DisplayNameOverride, false).DisplayName;
    }
}
