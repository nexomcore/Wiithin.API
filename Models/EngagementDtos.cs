using WithinAPI.Domain;

namespace WithinAPI.Models;

public sealed record ForYouFeedItemDto(
    string Id,
    string Kind,
    string Title,
    string Body,
    WithinLens Lens,
    DateTimeOffset SortAt,
    string? ActionRoute,
    Guid? TargetId,
    int Priority);

public sealed record StreakSummaryDto(
    int DailyCheckIn,
    int Journal,
    int Meditation,
    int Workout,
    int Reflection,
    int Best,
    string Label,
    bool AtRisk);

public sealed record WeeklyProgressSummaryDto(
    DateOnly WeekStart,
    DateOnly WeekEnd,
    int EventsAttended,
    int ChallengesCompleted,
    int CheckInsCompleted,
    int JournalEntriesWritten,
    int MeditationMinutes,
    int ProgramTasksCompleted);

public sealed record CircleActivityDigestDto(
    int NewDiscussions,
    int NewComments,
    int UpcomingCircleEvents,
    ForYouFeedItemDto[] Items);

public sealed record SuggestedContentDto(EventDto[] Events, CircleDto[] Circles);

public sealed record AchievementBadgeDto(string Key, string Label, string Description, bool Earned);

public sealed record EngagementDashboardDto(
    ForYouFeedItemDto[] ForYou,
    StreakSummaryDto Streaks,
    WeeklyProgressSummaryDto WeeklySummary,
    CircleActivityDigestDto CircleDigest,
    SuggestedContentDto Suggestions,
    AchievementBadgeDto[] Badges,
    string DailyWisdom);

public sealed record ProviderFollowStateDto(Guid ProviderId, bool IsFollowing, int FollowerCount);

public sealed record ProviderProfileCompletenessDto(int Score, string[] Completed, string[] Recommendations);

public sealed record ProviderAnalyticsDto(
    int ProfileViews30d,
    int SearchAppearances30d,
    int Followers,
    int CircleMembers,
    int EventsCreated,
    int EventRegistrations,
    int ActiveClients,
    int ProgramsAssigned,
    int ProgramCompletionRate);

public sealed record ProviderClientActivityDto(
    int ActiveClients,
    ProviderProgramActivityDto[] RecentlyAssignedPrograms,
    ProviderCheckInActivityDto[] PendingCheckIns,
    ProviderClientAttentionDto[] ClientsRequiringAttention);

public sealed record ProviderClientAttentionDto(Guid ClientUserId, string ClientName, string Reason, Guid? AssignedProgramId);

public sealed record ProviderProgramActivityDto(Guid AssignedProgramId, Guid ClientUserId, string ClientName, string Title, AssignedProgramStatus Status, int ProgressPercent, DateTimeOffset UpdatedAt);

public sealed record ProviderCheckInActivityDto(Guid CheckInId, Guid AssignedProgramId, Guid ClientUserId, string ClientName, DateOnly CheckInDate, string? Mood, string? ClientNotes);
