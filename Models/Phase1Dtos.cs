using WithinAPI.Domain;
using System.Text.Json;

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

public sealed record ProviderApplicationDto(
    Guid Id,
    ProviderApplicationStatus Status,
    ProviderCategory ProviderCategory,
    WithinLens PrimaryLens,
    string[] ServiceAreas,
    string ContactName,
    string ContactEmail,
    string ContactPhone,
    string PreferredContactMethod,
    string ProviderName,
    string BusinessType,
    string? Abn,
    string? WebsiteUrl,
    string? InstagramUrl,
    string? OtherSocialUrl,
    string Location,
    string[] DeliveryModes,
    string? VenueNames,
    string[] ServicesOffered,
    string YearsPracticing,
    string TypicalAudience,
    string Bio,
    string JoinReason,
    string Certifications,
    string InsuranceStatus,
    string WorkingWithChildrenCheck,
    string FirstAidCpr,
    string? ProfessionalMemberships,
    string? CredentialLinks,
    string HasEventsReady,
    string ExpectedFirstEvent,
    string BookingTools,
    string? AdminFacingNotes,
    bool DeclarationAccepted,
    string AdminNotes,
    string ReviewDecisionReason,
    DateTimeOffset SubmittedUtc,
    DateTimeOffset UpdatedUtc,
    DateTimeOffset? ReviewedUtc,
    Guid? ApprovedProviderId,
    string? TemporaryPassword = null);

public sealed record CreateProviderApplicationDto(
    ProviderCategory ProviderCategory,
    WithinLens PrimaryLens,
    string[] ServiceAreas,
    string ContactName,
    string ContactEmail,
    string ContactPhone,
    string PreferredContactMethod,
    string ProviderName,
    string BusinessType,
    string? Abn,
    string? WebsiteUrl,
    string? InstagramUrl,
    string? OtherSocialUrl,
    string Location,
    string[] DeliveryModes,
    string? VenueNames,
    string[] ServicesOffered,
    string YearsPracticing,
    string TypicalAudience,
    string Bio,
    string JoinReason,
    string Certifications,
    string InsuranceStatus,
    string WorkingWithChildrenCheck,
    string FirstAidCpr,
    string? ProfessionalMemberships,
    string? CredentialLinks,
    string HasEventsReady,
    string ExpectedFirstEvent,
    string BookingTools,
    string? AdminFacingNotes,
    bool DeclarationAccepted);

public sealed record ProviderApplicationStatusUpdateDto(ProviderApplicationStatus Status, string? Reason);

public sealed record ProviderApplicationNotesDto(string AdminNotes);

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
    RsvpVisibility? RsvpVisibility,
    SignupType SignupType,
    string? ExternalBookingUrl,
    string? ImageUrl,
    EventStatus Status,
    string[] Tags);

public sealed record ProviderEventEngagementDto(
    Guid EventId,
    string EventTitle,
    int GoingCount,
    int InterestedCount,
    int DeclinedCount,
    int SavedCount,
    ProviderEventParticipantDto[] Going,
    ProviderEventParticipantDto[] Interested,
    ProviderEventParticipantDto[] Declined,
    ProviderEventParticipantDto[] Saved);

public sealed record ProviderEventParticipantDto(
    Guid UserId,
    string DisplayName,
    DateTimeOffset UpdatedUtc);

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

public sealed record EventRsvpDto(EventJoinState State, RsvpVisibility? Visibility = null);

public sealed record RsvpVisibilityDto(RsvpVisibility Visibility);

public sealed record EventAttendeeDto(
    Guid UserId,
    string DisplayName,
    EventJoinState State,
    RsvpVisibility Visibility,
    bool IsPrivate,
    DateTimeOffset UpdatedUtc);

public sealed record FriendsGoingDto(int Count, EventAttendeeDto[] Friends);

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

public sealed record CommentDto(Guid Id, Guid? ParentCommentId, string AuthorName, string Body, DateTimeOffset CreatedUtc);

public sealed record UpsertCommentDto(string Body, Guid? ParentCommentId = null);

public sealed record CommunityAuthorDto(
    Guid Id,
    string DisplayName,
    WithinRole Role,
    bool IsVerifiedProvider);

public sealed record CommunityTopicDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive);

public sealed record CommunityEventSummaryDto(
    Guid Id,
    string Title,
    string ProviderName,
    DateTimeOffset StartUtc,
    string LocationName);

public sealed record CommunityPostDto(
    Guid Id,
    CommunityPostType PostType,
    string Title,
    string Body,
    CommunityContentStatus Status,
    CommunityAuthorDto Author,
    CommunityTopicDto[] Topics,
    CommunityEventSummaryDto? LinkedEvent,
    int HelpfulCount,
    int CommentCount,
    int SavedCount,
    bool IsHelpful,
    bool IsSaved,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CommunityPostDetailDto(
    CommunityPostDto Post,
    CommunityCommentDto[] Comments);

public sealed record CommunityCommentDto(
    Guid Id,
    Guid PostId,
    string Body,
    CommunityContentStatus Status,
    CommunityAuthorDto Author,
    int HelpfulCount,
    bool IsHelpful,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CommunityCreatePostDto(
    CommunityPostType PostType,
    string Title,
    string Body,
    Guid[] TopicIds,
    Guid? LinkedEventId);

public sealed record CommunityUpdatePostDto(
    CommunityPostType PostType,
    string Title,
    string Body,
    Guid[] TopicIds,
    Guid? LinkedEventId);

public sealed record CommunityCreateCommentDto(string Body);

public sealed record CommunityReportRequestDto(
    Guid? PostId,
    Guid? CommentId,
    CommunityReportReason Reason,
    string? Description);

public sealed record CommunityReportDto(
    Guid Id,
    CommunityReportReason Reason,
    string? Description,
    CommunityReportStatus Status,
    CommunityPostDto? Post,
    CommunityCommentDto? Comment,
    CommunityAuthorDto Reporter,
    CommunityAuthorDto? Reviewer,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReviewedAt);

public sealed record CommunityReviewReportDto(CommunityReportStatus Status);

public sealed record CircleDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    CircleType Type,
    CircleVisibility Visibility,
    CircleStatus Status,
    WithinLens Lens,
    int MemberCount,
    int ThreadCount,
    int EventCount,
    bool IsMember);

public sealed record CircleGuidelineDto(Guid Id, string Title, string Body, int SortOrder);

public sealed record CircleDetailDto(
    CircleDto Circle,
    CircleGuidelineDto[] Guidelines,
    CircleThreadDto[] LatestThreads,
    EventDto[] SharedEvents);

public sealed record CircleThreadDto(
    Guid Id,
    Guid CircleId,
    string CircleName,
    CommunityPostType ThreadType,
    string Title,
    string Body,
    CommunityContentStatus Status,
    CommunityAuthorDto Author,
    CommunityEventSummaryDto? LinkedEvent,
    int HelpfulCount,
    int CommentCount,
    bool IsHelpful,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CircleThreadDetailDto(CircleThreadDto Thread, CircleThreadCommentDto[] Comments);

public sealed record CircleThreadCommentDto(
    Guid Id,
    Guid ThreadId,
    string Body,
    CommunityContentStatus Status,
    CommunityAuthorDto Author,
    int HelpfulCount,
    bool IsHelpful,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CircleCreateThreadDto(
    CommunityPostType ThreadType,
    string Title,
    string Body,
    Guid? LinkedEventId);

public sealed record CircleUpdateThreadDto(
    CommunityPostType ThreadType,
    string Title,
    string Body,
    Guid? LinkedEventId);

public sealed record CircleCreateCommentDto(string Body);

public sealed record CircleShareEventDto(Guid EventId, string? OptionalNote);

public sealed record CircleReportRequestDto(
    Guid? ThreadId,
    Guid? CommentId,
    Guid? CircleEventId,
    CommunityReportReason Reason,
    string? Description);

public sealed record CircleReportDto(
    Guid Id,
    Guid CircleId,
    Guid? CircleEventId,
    string CircleName,
    CommunityReportReason Reason,
    string? Description,
    CommunityReportStatus Status,
    CircleThreadDto? Thread,
    CircleThreadCommentDto? Comment,
    EventDto? SharedEvent,
    CommunityAuthorDto Reporter,
    CommunityAuthorDto? Reviewer,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReviewedAt);

public sealed record CircleReviewReportDto(CommunityReportStatus Status);

public sealed record ConnectionDto(
    Guid Id,
    Guid OtherUserId,
    string OtherDisplayName,
    ConnectionStatus Status,
    bool IsRequester,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UserSearchResultDto(
    Guid Id,
    string DisplayName,
    WithinRole Role,
    ConnectionStatus? ConnectionStatus,
    bool IsRequester);

public sealed record ConnectionRequestDto(Guid ReceiverUserId);

public sealed record BlockUserDto(Guid UserId);

public sealed record UserReportRequestDto(
    Guid ReportedUserId,
    UserReportReason Reason,
    string? Details,
    MentionSourceType? SourceType,
    Guid? SourceId);

public sealed record UserPrivacySettingsDto(
    ProfileVisibility ProfileVisibility,
    RsvpVisibility DefaultRsvpVisibility,
    TaggingPermission TaggingPermission,
    FriendRequestPermission FriendRequestPermission,
    bool ShowActivityToFriends,
    bool AllowEventInviteFromFriends);

public sealed record EventInviteDto(
    Guid Id,
    Guid EventId,
    string EventTitle,
    CommunityAuthorDto InvitedBy,
    CommunityAuthorDto InvitedUser,
    EventInviteStatus Status,
    string? Message,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateEventInvitesDto(Guid[] InvitedUserIds, string? Message);

public sealed record CircleIdentityDto(
    Guid CircleId,
    CircleIdentityMode IdentityMode,
    string? DisplayNameOverride,
    string DisplayName,
    bool ProfileLinkAllowed);

public sealed record UpdateCircleIdentityDto(CircleIdentityMode IdentityMode, string? DisplayNameOverride);

public sealed record CircleMemberDto(
    Guid UserId,
    string DisplayName,
    CircleIdentityMode IdentityMode,
    bool ProfileLinkAllowed,
    CircleMemberStatus Status,
    DateTimeOffset JoinedAt);

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

public sealed record MarketFitSubmissionDto(
    string Audience,
    string Name,
    string Contact,
    string Source,
    JsonElement Answers);

public sealed record MarketFitSubmissionResponseDto(
    Guid Id,
    string Audience,
    string Name,
    string Contact,
    string Source,
    DateTimeOffset CreatedUtc);

public sealed record AdminSubmissionDto(
    Guid Id,
    string Audience,
    string Name,
    string Contact,
    string Source,
    JsonElement Answers,
    DateTimeOffset CreatedUtc);

public sealed record AdminStatsDto(
    int TotalSubmissions,
    int UserSubmissions,
    int ProviderSubmissions,
    int TotalUsers,
    int ProviderUsers,
    int AdminUsers,
    DateTimeOffset? LatestSubmissionUtc);

public sealed record AdminUserDto(
    Guid Id,
    string DisplayName,
    string Email,
    WithinRole Role,
    DateTimeOffset CreatedUtc);
