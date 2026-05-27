using WithinAPI.Domain;

namespace WithinAPI.Models;

public sealed record RegisterDto(string DisplayName, string Email, string Password, WithinRole Role = WithinRole.User);

public sealed record LoginDto(string Email, string Password);

public sealed record TokenResponseDto(string AccessToken, string RefreshToken, UserSummaryDto User);

public sealed record UserSummaryDto(Guid Id, string DisplayName, string Email, WithinRole Role, WithinLens PreferredLens);

public sealed record ProviderDto(
    Guid Id,
    string Name,
    string Slug,
    string Bio,
    WithinLens Lens,
    string Location,
    string? WebsiteUrl,
    string? InstagramUrl,
    bool IsVerified);

public sealed record UpsertProviderDto(
    string Name,
    string Bio,
    WithinLens Lens,
    string Location,
    string? WebsiteUrl,
    string? InstagramUrl);

public sealed record EventDto(
    Guid Id,
    Guid ProviderId,
    string ProviderName,
    string Title,
    string Description,
    WithinLens Lens,
    string LocationName,
    bool IsOnline,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    decimal PriceAmount,
    string Currency,
    int Capacity,
    int GoingCount,
    bool IsSaved,
    EventJoinState? JoinState,
    SignupType SignupType,
    string? ExternalBookingUrl,
    string? ImageUrl,
    EventStatus Status,
    string[] Tags);

public sealed record EventFilterDto(
    WithinLens? Lens,
    bool? Free,
    bool? Online,
    bool? Weekend,
    string? Search,
    string? Tag,
    Guid? ProviderId);

public sealed record UpsertEventDto(
    string Title,
    string Description,
    WithinLens Lens,
    string LocationName,
    bool IsOnline,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    decimal PriceAmount,
    string Currency,
    int Capacity,
    SignupType SignupType,
    string? ExternalBookingUrl,
    string? ImageUrl,
    string[] Tags);

public sealed record JoinEventDto(EventJoinState State);

public sealed record CommunityDto(
    Guid Id,
    Guid? ProviderId,
    string Name,
    string Description,
    WithinLens Lens,
    string Location,
    int MemberCount,
    bool IsMember);

public sealed record UpsertCommunityPostDto(string Body, Guid? EventId);

public sealed record PostDto(
    Guid Id,
    Guid CommunityId,
    Guid? EventId,
    string AuthorName,
    string Body,
    int ReactionCount,
    int CommentCount,
    DateTimeOffset CreatedUtc);

public sealed record CommentDto(Guid Id, string AuthorName, string Body, DateTimeOffset CreatedUtc);

public sealed record UpsertCommentDto(string Body);

public sealed record ReviewDto(Guid Id, string AuthorName, int Rating, string Body, DateTimeOffset CreatedUtc);

public sealed record UpsertReviewDto(int Rating, string Body);

public sealed record DeviceTokenDto(string Token, string Platform);

public sealed record NotificationPreferencesDto(
    bool DailyMotivationEnabled,
    bool EventRemindersEnabled,
    bool CommunitySummariesEnabled,
    bool ProviderNewEventsEnabled,
    WithinLens PreferredLens);

public sealed record HomeDashboardDto(
    UserSummaryDto User,
    DailyCheckInDto? TodayCheckIn,
    EventDto[] RecommendedEvents,
    CommunityDto[] CommunityPulse,
    string DailyMotivation,
    EventDto[] UpcomingJoinedEvents);
