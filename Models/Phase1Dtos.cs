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
    ProviderType ProviderType,
    string? LegalName,
    string Bio,
    WithinLens Lens,
    string[] Categories,
    string? ProfileImageUrl,
    string? CoverImageUrl,
    string Location,
    string? Suburb,
    string? City,
    string? State,
    string? Country,
    string? WebsiteUrl,
    string? InstagramUrl,
    string? Phone,
    string? Email,
    bool IsVerified,
    ProviderVerificationStatus VerificationStatus,
    bool IsActive,
    bool ShowEmailPublicly,
    bool ShowPhonePublicly,
    bool ShowWebsitePublicly,
    string? PractitionerTitle,
    int? YearsExperience,
    string? Qualifications,
    string[] ServicesOffered,
    string[] Languages,
    bool OnlineAvailable,
    bool InPersonAvailable,
    string? BusinessType,
    string? Abn,
    string[] Facilities,
    string[] AccessibilityFeatures,
    string[] TeamMembers,
    string? OpeningHours,
    int ServiceCount,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc);

public sealed record ProviderServiceDto(
    Guid Id,
    Guid ProviderId,
    string Name,
    string Description,
    WithinLens Lens,
    string Category,
    int? DurationMinutes,
    decimal? PriceAmount,
    ProviderPriceType PriceType,
    ProviderServiceDeliveryMode DeliveryMode,
    string? Location,
    bool IsActive,
    DateTimeOffset CreatedUtc,
    DateTimeOffset UpdatedUtc);

public sealed record ProviderSummaryDto(
    Guid Id,
    string Name,
    ProviderType ProviderType,
    WithinLens Lens,
    string Location,
    bool IsVerified,
    ProviderVerificationStatus VerificationStatus);

public sealed record ProviderDetailDto(
    ProviderDto Provider,
    ProviderServiceDto[] Services,
    EventDto[] UpcomingEvents,
    CommunityDto[] Communities,
    CircleDto[] Circles);

public sealed record UpsertProviderDto(
    string Name,
    string Bio,
    WithinLens Lens,
    string Location,
    string? WebsiteUrl = null,
    string? InstagramUrl = null,
    ProviderType ProviderType = ProviderType.Business,
    string? LegalName = null,
    string[]? Categories = null,
    string? ProfileImageUrl = null,
    string? CoverImageUrl = null,
    string? Suburb = null,
    string? City = null,
    string? State = null,
    string? Country = null,
    string? Phone = null,
    string? Email = null,
    bool ShowEmailPublicly = false,
    bool ShowPhonePublicly = false,
    bool ShowWebsitePublicly = true,
    string? PractitionerTitle = null,
    int? YearsExperience = null,
    string? Qualifications = null,
    string[]? ServicesOffered = null,
    string[]? Languages = null,
    bool OnlineAvailable = false,
    bool InPersonAvailable = true,
    string? BusinessType = null,
    string? Abn = null,
    string[]? Facilities = null,
    string[]? AccessibilityFeatures = null,
    string[]? TeamMembers = null,
    string? OpeningHours = null,
    bool IsActive = true);

public sealed record UpsertProviderServiceDto(
    string Name,
    string Description,
    WithinLens Lens,
    string Category,
    int? DurationMinutes,
    decimal? PriceAmount,
    ProviderPriceType PriceType,
    ProviderServiceDeliveryMode DeliveryMode,
    string? Location,
    bool IsActive = true);

public sealed record ProviderApplicationDto(
    Guid Id,
    ProviderApplicationStatus Status,
    ProviderType ProviderType,
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
    ProviderType ProviderType,
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
    Guid? ProviderServiceId,
    string ProviderName,
    ProviderSummaryDto Provider,
    ProviderServiceDto? ProviderService,
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
    string[] Tags,
    string[] BringItems,
    string? BringNotes,
    string[] Facilities,
    string[] AccessibilityFeatures,
    string? PhysicalIntensity,
    string? SocialInteractionLevel,
    string? ExperienceLevel,
    string[] AtmosphereTags,
    bool FoodProvided,
    bool DrinksProvided,
    string[] DietaryOptions,
    string? FoodNotes,
    string? AgeRestriction,
    string? SafetyNotes);

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
    string[] Tags,
    Guid? ProviderServiceId = null,
    string[]? BringItems = null,
    string? BringNotes = null,
    string[]? Facilities = null,
    string[]? AccessibilityFeatures = null,
    string? PhysicalIntensity = null,
    string? SocialInteractionLevel = null,
    string? ExperienceLevel = null,
    string[]? AtmosphereTags = null,
    bool FoodProvided = false,
    bool DrinksProvided = false,
    string[]? DietaryOptions = null,
    string? FoodNotes = null,
    string? AgeRestriction = null,
    string? SafetyNotes = null);

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
    string? Rules,
    Guid CreatedByUserId,
    CircleType Type,
    CircleVisibility Visibility,
    CircleStatus Status,
    WithinLens Lens,
    int MemberCount,
    int ThreadCount,
    int EventCount,
    bool IsMember,
    bool IsPendingMember,
    CircleMemberRole? ViewerRole,
    bool CanManage,
    bool AllowAnonymousPosts);

public sealed record CircleCreateDto(
    string Name,
    string Description,
    WithinLens Lens,
    CircleVisibility Visibility,
    string? Rules = null,
    bool AllowAnonymousPosts = false);

public sealed record CircleUpdateDto(
    string Name,
    string Description,
    WithinLens Lens,
    CircleVisibility Visibility,
    string? Rules = null,
    bool AllowAnonymousPosts = false);

public sealed record CircleGuidelineDto(Guid Id, string Title, string Body, int SortOrder);

public sealed record CircleAnnouncementDto(
    Guid Id,
    string Body,
    bool IsPinned,
    CommunityAuthorDto Author,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CircleDetailDto(
    CircleDto Circle,
    CircleGuidelineDto[] Guidelines,
    CircleAnnouncementDto[] Announcements,
    CircleThreadDto[] LatestThreads,
    EventDto[] SharedEvents);

public sealed record CircleThreadDto(
    Guid Id,
    Guid CircleId,
    string CircleName,
    CommunityPostType ThreadType,
    CirclePostType PostType,
    string Title,
    string Body,
    CommunityContentStatus Status,
    CommunityAuthorDto Author,
    CommunityEventSummaryDto? LinkedEvent,
    int HelpfulCount,
    int CommentCount,
    bool IsHelpful,
    bool IsPinned,
    bool IsAnonymous,
    string? ImageUrl,
    CircleReactionSummaryDto[] Reactions,
    CirclePollDto? Poll,
    CircleWeeklyCheckInDto? WeeklyCheckIn,
    int CircleGoingCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    // Circle-identity-safe display metadata. Tap the author via contextType=CirclePost,
    // contextId=CircleId, targetContextProfileId=Id. Author.Id is Guid.Empty for non-real identities.
    CircleIdentityMode AuthorIdentityMode = CircleIdentityMode.RealProfile,
    bool AuthorIsClickable = false);

public sealed record CircleThreadDetailDto(CircleThreadDto Thread, CircleThreadCommentDto[] Comments);

public sealed record CircleThreadCommentDto(
    Guid Id,
    Guid ThreadId,
    string Body,
    CommunityContentStatus Status,
    CommunityAuthorDto Author,
    int HelpfulCount,
    bool IsHelpful,
    bool IsAnonymous,
    CircleReactionSummaryDto[] Reactions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    // Tap the author via contextType=CircleComment, contextId=CircleId, targetContextProfileId=Id.
    Guid CircleId = default,
    CircleIdentityMode AuthorIdentityMode = CircleIdentityMode.RealProfile,
    bool AuthorIsClickable = false);

public sealed record CircleCreateThreadDto(
    CommunityPostType ThreadType,
    string Title,
    string Body,
    Guid? LinkedEventId,
    CirclePostType? PostType = null,
    bool IsPinned = false,
    bool IsAnonymous = false,
    string? ImageUrl = null,
    CirclePollCreateDto? Poll = null);

public sealed record CircleUpdateThreadDto(
    CommunityPostType ThreadType,
    string Title,
    string Body,
    Guid? LinkedEventId,
    bool IsPinned = false,
    string? ImageUrl = null);

public sealed record CircleCreateCommentDto(string Body, bool IsAnonymous = false);

public sealed record CircleShareEventDto(Guid EventId, string? OptionalNote);

public sealed record CircleJoinRequestDto(
    Guid Id,
    Guid CircleId,
    string CircleName,
    CommunityAuthorDto User,
    CircleJoinRequestStatus Status,
    DateTimeOffset RequestedAt,
    CommunityAuthorDto? ReviewedBy,
    DateTimeOffset? ReviewedAt);

public sealed record CircleRoleUpdateDto(CircleMemberRole Role);

public sealed record CircleAnnouncementCreateDto(string Body, bool IsPinned);

public sealed record CircleReactionSummaryDto(CircleReactionType ReactionType, int Count, bool HasReacted);

public sealed record CircleReactionDto(CircleReactionType ReactionType);

public sealed record CirclePollCreateDto(string Question, string[] Options, DateTimeOffset? ClosesAt = null);

public sealed record CirclePollOptionDto(Guid Id, string Text, int VoteCount, bool HasVoted);

public sealed record CirclePollDto(Guid Id, string Question, DateTimeOffset? ClosesAt, bool HasVoted, CirclePollOptionDto[] Options);

public sealed record CirclePollVoteDto(Guid OptionId);

public sealed record CircleWeeklyCheckInDto(Guid ThreadId, bool HasResponded, WeeklyCheckInMood? MyMood, IReadOnlyDictionary<WeeklyCheckInMood, int> Counts);

public sealed record CircleWeeklyCheckInResponseDto(WeeklyCheckInMood Mood);

public sealed record CircleInviteCreateDto(Guid[] UserIds);

public sealed record CircleInviteDto(
    Guid Id,
    Guid CircleId,
    string CircleName,
    CommunityAuthorDto InvitedBy,
    CommunityAuthorDto InvitedUser,
    CircleInviteStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CircleAttendanceDto(Guid EventId, Guid CircleId, int Count);

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

// Context-safe action requests. The frontend may not hold the real target userId
// (pseudonym/hidden identities), so it references the displayed identity by context.
// The backend resolves the real user internally. Spec §9.
public sealed record ConnectionFromContextDto(
    ProfileContextType ContextType,
    Guid ContextId,
    Guid TargetContextProfileId);

public sealed record BlockFromContextDto(
    ProfileContextType ContextType,
    Guid ContextId,
    Guid TargetContextProfileId);

public sealed record ReportFromContextDto(
    ProfileContextType ContextType,
    Guid ContextId,
    Guid TargetContextProfileId,
    UserReportReason Reason,
    string? Details);

// Context-safe display identity (spec §8 getDisplayIdentityForContext).
public sealed record DisplayIdentityDto(
    string DisplayName,
    CircleIdentityMode IdentityMode,
    bool IsClickable,
    bool ProfileLinkAllowed);

// The privacy-safe card returned when a displayed name is tapped (spec §8.2).
// Note: never carries the real userId for Pseudonym/HiddenProfile identities.
public sealed record ProfilePreviewDto(
    string DisplayName,
    string? AvatarUrl,
    string? BioPreview,
    string? LocationPreview,
    string? SharedContextSummary,
    CircleIdentityMode IdentityMode,
    bool CanViewFullProfile,
    bool CanRequestConnection,
    ProfileConnectionState ConnectionStatus,
    bool CanReport,
    bool CanBlock,
    string? SafeProfileRoute,
    Guid? ConnectionId);

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
    // Null for Pseudonym/HiddenProfile members so the real identity never reaches the client.
    Guid? UserId,
    string DisplayName,
    CircleIdentityMode IdentityMode,
    bool ProfileLinkAllowed,
    CircleMemberRole Role,
    CircleMemberStatus Status,
    DateTimeOffset JoinedAt,
    // Safe handle to pass to /api/profile-preview as targetContextProfileId (the membership id).
    Guid ContextProfileId = default,
    bool IsClickable = false,
    string[]? Badges = null);

public sealed record ReviewDto(Guid Id, string AuthorName, int Rating, string Body, DateTimeOffset CreatedUtc);

public sealed record UpsertReviewDto(int Rating, string Body);

public sealed record DeviceTokenDto(string Token, string Platform);

public sealed record NotificationPreferencesDto(
    bool DailyMotivationEnabled,
    bool EventRemindersEnabled,
    bool CommunitySummariesEnabled,
    bool ProviderNewEventsEnabled,
    bool FriendRequestsEnabled,
    bool EventInvitesEnabled,
    bool FriendActivityEnabled,
    bool CircleRepliesEnabled,
    bool CommentRepliesEnabled,
    bool MentionsEnabled,
    WithinLens PreferredLens);

public sealed record NotificationDto(
    Guid Id,
    NotificationKind Kind,
    string Title,
    string Body,
    NotificationTargetType? TargetType,
    Guid? TargetId,
    Guid? ActorUserId,
    Guid? CircleId,
    Guid? EventId,
    Guid? RelatedUserId,
    bool IsRead,
    DateTimeOffset CreatedUtc,
    DateTimeOffset? ReadUtc);

public sealed record CreateNotificationDto(
    Guid UserId,
    NotificationKind Kind,
    string Title,
    string Body,
    NotificationTargetType? TargetType,
    Guid? TargetId,
    Guid? ActorUserId,
    Guid? CircleId,
    Guid? EventId,
    Guid? RelatedUserId);

public sealed record PushTokenDto(string Token, string Platform);

public sealed record NotificationMuteDto(NotificationMuteTargetType TargetType, Guid TargetId);

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
