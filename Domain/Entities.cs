namespace WithinAPI.Domain;

public enum WithinRole
{
    User,
    Provider,
    Admin
}

public enum WithinLens
{
    Move,
    Feel,
    Seek
}

public enum EventJoinState
{
    Interested,
    Going,
    Attended,
    Declined
}

public enum EventStatus
{
    Draft,
    Published,
    Cancelled
}

public enum SignupType
{
    Internal,
    External
}

public enum NotificationKind
{
    DailyMotivation,
    EventReminder24h,
    EventReminder2h,
    EventUpdated,
    CommunitySummary,
    ProviderNewEvent,
    FriendRequestReceived,
    FriendRequestAccepted,
    EventInvite,
    PublicFriendRsvp,
    CircleThreadReply,
    CommentReply,
    UserMention,
    EventReminder,
    CircleJoinRequest,
    CircleInvite
}

public enum NotificationTargetType
{
    Event,
    Circle,
    CircleThread,
    CommunityPost,
    Profile,
    Connection,
    Comment
}

public enum NotificationMuteTargetType
{
    Circle,
    Event,
    User
}

public enum ProviderApplicationStatus
{
    Submitted,
    InReview,
    MoreInfoRequested,
    Approved,
    Rejected
}

public enum ProviderCategory
{
    BusinessStudio,
    IndividualPractitioner,
    CollectiveCommunityGroup,
    RetreatProgramOrganiser,
    VenueSpacePartner,
    CorporateWorkplaceWellness
}

public enum ProviderType
{
    Individual,
    Business
}

public enum ProviderVerificationStatus
{
    Unverified,
    Pending,
    Verified,
    Rejected
}

public enum ProviderPriceType
{
    Free,
    Fixed,
    FromPrice,
    ContactProvider
}

public enum ProviderServiceDeliveryMode
{
    InPerson,
    Online,
    Hybrid
}

public enum CommunityPostType
{
    AskCommunity,
    ShareExperience,
    FindBuddy,
    LocalRecommendation,
    Reflection
}

public enum CommunityContentStatus
{
    Active,
    Hidden,
    Removed,
    UnderReview
}

public enum CommunityReportReason
{
    SpamOrPromotion,
    HarassmentOrAbuse,
    MedicalMisinformation,
    InappropriateContent,
    SafetyConcern,
    Other
}

public enum CommunityReportStatus
{
    Pending,
    Reviewed,
    ActionTaken,
    Dismissed
}

public enum CircleType
{
    Platform,
    Provider,
    EventCohort,
    PrivateSupport
}

public enum CircleVisibility
{
    Public,
    Private,
    Hidden
}

public enum CircleStatus
{
    Active,
    Archived
}

public enum CircleMemberStatus
{
    Active,
    Left,
    Removed,
    Pending,
    Rejected,
    Blocked
}

public enum CircleRoleKind
{
    Moderator,
    Admin
}

public enum CircleMemberRole
{
    Admin,
    Moderator,
    Member
}

public enum CircleJoinRequestStatus
{
    Pending,
    Approved,
    Rejected
}

public enum CircleEventStatus
{
    Active,
    Removed
}

public enum ConnectionStatus
{
    Pending,
    Accepted,
    Rejected,
    Cancelled,
    Removed,
    Blocked
}

public enum ProfileVisibility
{
    Public,
    FriendsOnly,
    CircleMembersOnly,
    Private
}

public enum RsvpVisibility
{
    Public,
    FriendsOnly,
    CircleMembersOnly,
    Private
}

public enum TaggingPermission
{
    Everyone,
    FriendsOnly,
    CircleMembersOnly,
    NoOne
}

public enum FriendRequestPermission
{
    Everyone,
    FriendsOfFriends,
    SameCircleOrEvent,
    NoOne
}

public enum EventInviteStatus
{
    Pending,
    Accepted,
    Declined,
    Cancelled
}

public enum MentionSourceType
{
    EventComment,
    CirclePost,
    CircleComment
}

public enum CircleIdentityMode
{
    RealProfile,
    Pseudonym,
    HiddenProfile
}

public enum CirclePrivacyType
{
    Open,
    ApprovalRequired,
    PrivateInviteOnly,
    Sensitive
}

public enum MemberListVisibility
{
    Public,
    MembersOnly,
    AdminsOnly,
    Hidden
}

public enum CirclePostVisibility
{
    Public,
    MembersOnly,
    Private
}

public enum CirclePostType
{
    Standard,
    System,
    Announcement,
    EventShare,
    WeeklyCheckIn,
    Poll
}

public enum CircleReactionType
{
    Support,
    Grateful,
    Inspired,
    Motivated,
    Growing
}

public enum WeeklyCheckInMood
{
    Great,
    Good,
    Okay,
    Struggling
}

public enum CircleInviteStatus
{
    Pending,
    Accepted,
    Declined,
    Cancelled
}

public enum UserReportStatus
{
    Open,
    UnderReview,
    Resolved,
    Dismissed
}

public enum UserReportReason
{
    Harassment,
    Spam,
    HateOrAbuse,
    Impersonation,
    PrivacyConcern,
    Other
}

// API-facing only: identifies the surface a displayed identity was clicked from.
// Not persisted, so it is intentionally absent from the Postgres enum registration.
public enum ProfileContextType
{
    EventAttendee,
    EventFriendGoing,
    EventComment,
    CircleMember,
    CirclePost,
    CircleComment
}

// The viewer-relative connection state shown on a profile preview (spec §8.2).
// Distinct from the persisted ConnectionStatus.
public enum ProfileConnectionState
{
    None,
    PendingSent,
    PendingReceived,
    Connected,
    Blocked
}

public sealed class User
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public WithinRole Role { get; set; }
    public WithinLens PreferredLens { get; set; } = WithinLens.Feel;
    public DateTimeOffset CreatedUtc { get; set; }

    public static User Seed(Guid id, string name, string email, WithinRole role, DateTimeOffset createdUtc) => new()
    {
        Id = id,
        DisplayName = name,
        Email = email,
        PasswordHash = "pbkdf2:AQIDBAUGBwgJCgsMDQ4PEA==:+8XHFlhvxuo21D9qorz3lLT5BMobw/7nT57cxsiviS8=",
        Role = role,
        CreatedUtc = createdUtc
    };
}

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = "";
    public DateTimeOffset ExpiresUtc { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset? RevokedUtc { get; set; }
}

public sealed class Provider
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public ProviderType ProviderType { get; set; } = ProviderType.Business;
    public string? LegalName { get; set; }
    public string Bio { get; set; } = "";
    public WithinLens Lens { get; set; }
    public string[] Categories { get; set; } = [];
    public string? ProfileImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Location { get; set; } = "";
    public string? Suburb { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsVerified { get; set; }
    public ProviderVerificationStatus VerificationStatus { get; set; } = ProviderVerificationStatus.Unverified;
    public bool IsActive { get; set; } = true;
    public bool ShowEmailPublicly { get; set; }
    public bool ShowPhonePublicly { get; set; }
    public bool ShowWebsitePublicly { get; set; } = true;
    public string? PractitionerTitle { get; set; }
    public int? YearsExperience { get; set; }
    public string? Qualifications { get; set; }
    public string[] ServicesOffered { get; set; } = [];
    public string[] Languages { get; set; } = [];
    public bool OnlineAvailable { get; set; }
    public bool InPersonAvailable { get; set; } = true;
    public string? BusinessType { get; set; }
    public string? Abn { get; set; }
    public string[] Facilities { get; set; } = [];
    public string[] AccessibilityFeatures { get; set; } = [];
    public string[] TeamMembers { get; set; } = [];
    public string? OpeningHours { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

public sealed class ProviderService
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public WithinLens Lens { get; set; }
    public string Category { get; set; } = "";
    public int? DurationMinutes { get; set; }
    public decimal? PriceAmount { get; set; }
    public ProviderPriceType PriceType { get; set; } = ProviderPriceType.ContactProvider;
    public ProviderServiceDeliveryMode DeliveryMode { get; set; } = ProviderServiceDeliveryMode.InPerson;
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

public sealed class ProviderApplication
{
    public Guid Id { get; set; }
    public ProviderApplicationStatus Status { get; set; } = ProviderApplicationStatus.Submitted;
    public ProviderType ProviderType { get; set; } = ProviderType.Business;
    public ProviderCategory ProviderCategory { get; set; }
    public WithinLens PrimaryLens { get; set; }
    public string[] ServiceAreas { get; set; } = [];
    public string ContactName { get; set; } = "";
    public string ContactEmail { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string PreferredContactMethod { get; set; } = "";
    public string ProviderName { get; set; } = "";
    public string BusinessType { get; set; } = "";
    public string? Abn { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? OtherSocialUrl { get; set; }
    public string Location { get; set; } = "";
    public string[] DeliveryModes { get; set; } = [];
    public string? VenueNames { get; set; }
    public string[] ServicesOffered { get; set; } = [];
    public string YearsPracticing { get; set; } = "";
    public string TypicalAudience { get; set; } = "";
    public string Bio { get; set; } = "";
    public string JoinReason { get; set; } = "";
    public string Certifications { get; set; } = "";
    public string InsuranceStatus { get; set; } = "";
    public string WorkingWithChildrenCheck { get; set; } = "";
    public string FirstAidCpr { get; set; } = "";
    public string? ProfessionalMemberships { get; set; }
    public string? CredentialLinks { get; set; }
    public string HasEventsReady { get; set; } = "";
    public string ExpectedFirstEvent { get; set; } = "";
    public string BookingTools { get; set; } = "";
    public string? AdminFacingNotes { get; set; }
    public bool DeclarationAccepted { get; set; }
    public string AdminNotes { get; set; } = "";
    public string ReviewDecisionReason { get; set; } = "";
    public DateTimeOffset SubmittedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
    public DateTimeOffset? ReviewedUtc { get; set; }
    public Guid? ApprovedProviderId { get; set; }
}

public sealed class Event
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public Guid? ProviderServiceId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public WithinLens Lens { get; set; }
    public string LocationName { get; set; } = "";
    public bool IsOnline { get; set; }
    public DateTimeOffset StartUtc { get; set; }
    public DateTimeOffset EndUtc { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "AUD";
    public int Capacity { get; set; }
    public SignupType SignupType { get; set; }
    public string? ExternalBookingUrl { get; set; }
    public string? ImageUrl { get; set; }
    public EventStatus Status { get; set; }
    public string[] Tags { get; set; } = [];
    public string[] BringItems { get; set; } = [];
    public string? BringNotes { get; set; }
    public string[] Facilities { get; set; } = [];
    public string[] AccessibilityFeatures { get; set; } = [];
    public string? PhysicalIntensity { get; set; }
    public string? SocialInteractionLevel { get; set; }
    public string? ExperienceLevel { get; set; }
    public string[] AtmosphereTags { get; set; } = [];
    public bool FoodProvided { get; set; }
    public bool DrinksProvided { get; set; }
    public string[] DietaryOptions { get; set; } = [];
    public string? FoodNotes { get; set; }
    public string? AgeRestriction { get; set; }
    public string? SafetyNotes { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class EventRegistration
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public EventJoinState State { get; set; }
    public RsvpVisibility Visibility { get; set; } = RsvpVisibility.FriendsOnly;
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

public sealed class SavedEvent
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class Community
{
    public Guid Id { get; set; }
    public Guid? ProviderId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public WithinLens Lens { get; set; }
    public string Location { get; set; } = "";
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class CommunityMember
{
    public Guid Id { get; set; }
    public Guid CommunityId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset JoinedUtc { get; set; }
}

public sealed class Post
{
    public Guid Id { get; set; }
    public Guid CommunityId { get; set; }
    public Guid? EventId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Body { get; set; } = "";
    public bool IsHidden { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class Comment
{
    public Guid Id { get; set; }
    public Guid? PostId { get; set; }
    public Guid? EventId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Body { get; set; } = "";
    public bool IsHidden { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class Reaction
{
    public Guid Id { get; set; }
    public Guid? PostId { get; set; }
    public Guid? CommentId { get; set; }
    public Guid UserId { get; set; }
    public string Kind { get; set; } = "like";
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class CommunityPost
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public CommunityPostType PostType { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public Guid? LinkedEventId { get; set; }
    public CommunityContentStatus Status { get; set; } = CommunityContentStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class CommunityTopic
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CommunityPostTopic
{
    public Guid PostId { get; set; }
    public Guid TopicId { get; set; }
}

public sealed class CommunityComment
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Body { get; set; } = "";
    public CommunityContentStatus Status { get; set; } = CommunityContentStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class CommunityHelpfulReaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? PostId { get; set; }
    public Guid? CommentId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class SavedCommunityPost
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CommunityReport
{
    public Guid Id { get; set; }
    public Guid ReportedByUserId { get; set; }
    public Guid? PostId { get; set; }
    public Guid? CommentId { get; set; }
    public CommunityReportReason Reason { get; set; }
    public string? Description { get; set; }
    public CommunityReportStatus Status { get; set; } = CommunityReportStatus.Pending;
    public Guid? ReviewedByUserId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class Circle
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Rules { get; set; }
    public Guid CreatedByUserId { get; set; }
    public CircleType Type { get; set; } = CircleType.Platform;
    public CircleVisibility Visibility { get; set; } = CircleVisibility.Public;
    public CircleStatus Status { get; set; } = CircleStatus.Active;
    public CirclePrivacyType PrivacyType { get; set; } = CirclePrivacyType.Open;
    public bool AllowPseudonyms { get; set; } = true;
    public bool AllowHiddenProfiles { get; set; } = true;
    public bool AllowAnonymousPosts { get; set; }
    public MemberListVisibility MemberListVisibility { get; set; } = MemberListVisibility.MembersOnly;
    public CirclePostVisibility DefaultPostVisibility { get; set; } = CirclePostVisibility.MembersOnly;
    public RsvpVisibility DefaultEventRsvpVisibility { get; set; } = RsvpVisibility.FriendsOnly;
    public WithinLens Lens { get; set; } = WithinLens.Feel;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CircleMember
{
    public Guid Id { get; set; }
    public Guid CircleId { get; set; }
    public Guid UserId { get; set; }
    public CircleMemberRole Role { get; set; } = CircleMemberRole.Member;
    public CircleMemberStatus Status { get; set; } = CircleMemberStatus.Active;
    public CircleIdentityMode IdentityMode { get; set; } = CircleIdentityMode.RealProfile;
    public string? DisplayNameOverride { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LeftAt { get; set; }
}

public sealed class CircleJoinRequest
{
    public Guid Id { get; set; }
    public Guid CircleId { get; set; }
    public Guid UserId { get; set; }
    public CircleJoinRequestStatus Status { get; set; } = CircleJoinRequestStatus.Pending;
    public DateTimeOffset RequestedAt { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
}

public sealed class CircleRole
{
    public Guid Id { get; set; }
    public Guid CircleId { get; set; }
    public Guid UserId { get; set; }
    public CircleRoleKind Role { get; set; }
    public Guid? AssignedByUserId { get; set; }
    public DateTimeOffset AssignedAt { get; set; }
}

public sealed class CircleThread
{
    public Guid Id { get; set; }
    public Guid CircleId { get; set; }
    public Guid UserId { get; set; }
    public CommunityPostType ThreadType { get; set; } = CommunityPostType.AskCommunity;
    public CirclePostType PostType { get; set; } = CirclePostType.Standard;
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public Guid? LinkedEventId { get; set; }
    public bool IsPinned { get; set; }
    public bool IsAnonymous { get; set; }
    public string? ImageUrl { get; set; }
    public DateOnly? WeeklyCheckInWeekStart { get; set; }
    public CommunityContentStatus Status { get; set; } = CommunityContentStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public sealed class CircleThreadComment
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    // One-level replies only: a reply points at a top-level comment; replies cannot be replied to.
    public Guid? ParentCommentId { get; set; }
    public Guid UserId { get; set; }
    public string Body { get; set; } = "";
    public bool IsAnonymous { get; set; }
    public CommunityContentStatus Status { get; set; } = CommunityContentStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public sealed class CircleEvent
{
    public Guid Id { get; set; }
    public Guid CircleId { get; set; }
    public Guid EventId { get; set; }
    public Guid SharedByUserId { get; set; }
    public string? OptionalNote { get; set; }
    public CircleEventStatus Status { get; set; } = CircleEventStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CircleHelpfulReaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ThreadId { get; set; }
    public Guid? CommentId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CircleReaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ThreadId { get; set; }
    public Guid? CommentId { get; set; }
    public CircleReactionType ReactionType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CirclePoll
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public string Question { get; set; } = "";
    public DateTimeOffset? ClosesAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CirclePollOption
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public string Text { get; set; } = "";
    public int SortOrder { get; set; }
}

public sealed class CirclePollVote
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public Guid OptionId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CircleWeeklyCheckInResponse
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public Guid UserId { get; set; }
    public WeeklyCheckInMood Mood { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class CircleInvite
{
    public Guid Id { get; set; }
    public Guid CircleId { get; set; }
    public Guid InvitedByUserId { get; set; }
    public Guid InvitedUserId { get; set; }
    public CircleInviteStatus Status { get; set; } = CircleInviteStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }
}

public sealed class CircleReport
{
    public Guid Id { get; set; }
    public Guid ReporterUserId { get; set; }
    public Guid CircleId { get; set; }
    public Guid? ThreadId { get; set; }
    public Guid? CommentId { get; set; }
    public Guid? CircleEventId { get; set; }
    public CommunityReportReason Reason { get; set; }
    public string? Description { get; set; }
    public CommunityReportStatus Status { get; set; } = CommunityReportStatus.Pending;
    public Guid? ReviewedByUserId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CircleGuideline
{
    public Guid Id { get; set; }
    public Guid CircleId { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CircleAnnouncement
{
    public Guid Id { get; set; }
    public Guid CircleId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Body { get; set; } = "";
    public bool IsPinned { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class Connection
{
    public Guid Id { get; set; }
    public Guid RequesterUserId { get; set; }
    public Guid ReceiverUserId { get; set; }
    public ConnectionStatus Status { get; set; } = ConnectionStatus.Pending;
    public Guid? BlockedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }
    public DateTimeOffset? BlockedAt { get; set; }
}

public sealed class UserPrivacySettings
{
    public Guid UserId { get; set; }
    public ProfileVisibility ProfileVisibility { get; set; } = ProfileVisibility.FriendsOnly;
    public RsvpVisibility DefaultRsvpVisibility { get; set; } = RsvpVisibility.FriendsOnly;
    public TaggingPermission TaggingPermission { get; set; } = TaggingPermission.FriendsOnly;
    public FriendRequestPermission FriendRequestPermission { get; set; } = FriendRequestPermission.SameCircleOrEvent;
    public bool ShowActivityToFriends { get; set; } = true;
    public bool AllowEventInviteFromFriends { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class EventInvite
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid InvitedByUserId { get; set; }
    public Guid InvitedUserId { get; set; }
    public EventInviteStatus Status { get; set; } = EventInviteStatus.Pending;
    public string? Message { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? RespondedAt { get; set; }
}

public sealed class Mention
{
    public Guid Id { get; set; }
    public Guid MentionedUserId { get; set; }
    public Guid MentionedByUserId { get; set; }
    public MentionSourceType SourceType { get; set; }
    public Guid SourceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class UserReport
{
    public Guid Id { get; set; }
    public Guid ReportedByUserId { get; set; }
    public Guid ReportedUserId { get; set; }
    public MentionSourceType? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public UserReportReason Reason { get; set; }
    public string? Details { get; set; }
    public UserReportStatus Status { get; set; } = UserReportStatus.Open;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class Review
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string Body { get; set; } = "";
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class NotificationPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool DailyMotivationEnabled { get; set; } = true;
    public bool EventRemindersEnabled { get; set; } = true;
    public bool CommunitySummariesEnabled { get; set; } = true;
    public bool ProviderNewEventsEnabled { get; set; } = true;
    public bool FriendRequestsEnabled { get; set; } = true;
    public bool EventInvitesEnabled { get; set; } = true;
    public bool FriendActivityEnabled { get; set; } = true;
    public bool CircleRepliesEnabled { get; set; } = true;
    public bool CommentRepliesEnabled { get; set; } = true;
    public bool MentionsEnabled { get; set; } = true;
    public WithinLens PreferredLens { get; set; } = WithinLens.Feel;
}

public sealed class PushToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = "";
    public string Platform { get; set; } = "";
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

public sealed class DeviceToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = "";
    public string Platform { get; set; } = "";
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ActorUserId { get; set; }
    public NotificationKind Kind { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public NotificationTargetType? TargetType { get; set; }
    public Guid? TargetId { get; set; }
    public Guid? CircleId { get; set; }
    public Guid? EventId { get; set; }
    public Guid? RelatedUserId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset? ReadUtc { get; set; }
}

public sealed class NotificationMute
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationMuteTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class NotificationSchedule
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? EventId { get; set; }
    public NotificationKind Kind { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTimeOffset SendAtUtc { get; set; }
    public DateTimeOffset? SentUtc { get; set; }
}

public enum CheckInMood
{
    Great,
    Good,
    Okay,
    Low,
    Struggling,
    Stressed,
    Anxious,
    Tired,
    Angry,
    Grateful,
    Peaceful
}

public enum CheckInEnergy
{
    High,
    Balanced,
    Low,
    Exhausted
}

public enum CheckInSleepQuality
{
    Great,
    Okay,
    Poor,
    VeryPoor,
    NotSure
}

public enum DailyIntention
{
    StayCalm,
    MoveMyBody,
    EatBetter,
    BeProductive,
    RestAndRecover,
    ConnectWithSomeone,
    PracticeGratitude,
    SpendTimeOutdoors,
    BeMindful,
    ReduceStress
}

public sealed class DailyCheckIn
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public CheckInMood Mood { get; set; }
    public int? MoodScore { get; set; }
    public CheckInEnergy Energy { get; set; }
    public int? EnergyLevel { get; set; }
    public int? StressLevel { get; set; }
    public bool DidMoveToday { get; set; }
    public bool DidMeditateToday { get; set; }
    public CheckInSleepQuality? SleepQuality { get; set; }
    public decimal? SleepHours { get; set; }
    public DailyIntention Intention { get; set; }
    public string[] Tags { get; set; } = [];
    public string? Note { get; set; }
    public string? JournalEntry { get; set; }
    public string? SuggestedActionKey { get; set; }
    public int DailyBalanceScore { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class UserWellbeingProfile
{
    // Wellbeing and lifestyle profile data is not medical diagnosis data.
    // Do not expose height, weight, mood, stress, journal entries, health metrics, or check-ins on public profiles.
    // Future AI insights over this data must avoid medical diagnosis or clinical advice.
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public bool UsePseudonym { get; set; }
    public string? Pseudonym { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? AgeRange { get; set; }
    public string? Gender { get; set; }
    public string? LocationCountry { get; set; }
    public string? LocationCity { get; set; }
    public string? LocationSuburb { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public string? ActivityLevel { get; set; }
    public decimal? AverageSleepHours { get; set; }
    public decimal? WaterIntakeLitres { get; set; }
    public int? ExerciseDaysPerWeek { get; set; }
    public string? MeditationFrequency { get; set; }
    public int? StressLevelBaseline { get; set; }
    public int? EnergyLevelBaseline { get; set; }
    public int? MoodLevelBaseline { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public int? RestingHeartRate { get; set; }
    public decimal? Vo2Max { get; set; }
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public string? WearableProvider { get; set; }
    public bool WearableConnected { get; set; }
    public DateTimeOffset? LastWearableSyncAt { get; set; }
    public bool OnboardingCompleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class UserWellbeingInterest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Category { get; set; } = "";
    public string InterestKey { get; set; } = "";
    public string InterestLabel { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class UserWellbeingGoal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string GoalKey { get; set; } = "";
    public string GoalLabel { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class MonthlyProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int MoveScorePercent { get; set; }
    public int FeelScorePercent { get; set; }
    public int SeekScorePercent { get; set; }
    public int HolisticProfileScore { get; set; }
    public string? ReflectionNote { get; set; }
}

public enum HabitCategory
{
    Mind,
    Body,
    Lifestyle,
    Social,
    Nature
}

public sealed class HabitTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public HabitCategory Category { get; set; }
    public string? Description { get; set; }
    public string? IconKey { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UserHabit
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? HabitTemplateId { get; set; }
    public string Name { get; set; } = "";
    public HabitCategory? Category { get; set; }
    public bool IsCustom { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class HabitCompletion
{
    public Guid Id { get; set; }
    public Guid UserHabitId { get; set; }
    public Guid UserId { get; set; }
    public DateOnly CompletionDate { get; set; }
    public DateTimeOffset CompletedAtUtc { get; set; }
}

public sealed class MarketFitSubmission
{
    public Guid Id { get; set; }
    public string Audience { get; set; } = "";
    public string Name { get; set; } = "";
    public string Contact { get; set; } = "";
    public string Source { get; set; } = "";
    public string AnswersJson { get; set; } = "{}";
    public DateTimeOffset CreatedUtc { get; set; }
}
