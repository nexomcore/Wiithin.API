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
    ProviderNewEvent
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
    Removed
}

public enum CircleRoleKind
{
    Moderator,
    Admin
}

public enum CircleEventStatus
{
    Active,
    Removed
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
    public string Bio { get; set; } = "";
    public WithinLens Lens { get; set; }
    public string Location { get; set; } = "";
    public string? WebsiteUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public bool IsVerified { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class ProviderApplication
{
    public Guid Id { get; set; }
    public ProviderApplicationStatus Status { get; set; } = ProviderApplicationStatus.Submitted;
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
    public DateTimeOffset CreatedUtc { get; set; }
}

public sealed class EventRegistration
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public EventJoinState State { get; set; }
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
    public CircleType Type { get; set; } = CircleType.Platform;
    public CircleVisibility Visibility { get; set; } = CircleVisibility.Public;
    public CircleStatus Status { get; set; } = CircleStatus.Active;
    public WithinLens Lens { get; set; } = WithinLens.Feel;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CircleMember
{
    public Guid Id { get; set; }
    public Guid CircleId { get; set; }
    public Guid UserId { get; set; }
    public CircleMemberStatus Status { get; set; } = CircleMemberStatus.Active;
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? LeftAt { get; set; }
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
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public Guid? LinkedEventId { get; set; }
    public CommunityContentStatus Status { get; set; } = CommunityContentStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

public sealed class CircleThreadComment
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public Guid UserId { get; set; }
    public string Body { get; set; } = "";
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
    public WithinLens PreferredLens { get; set; } = WithinLens.Feel;
}

public sealed class DeviceToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = "";
    public string Platform { get; set; } = "";
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

public sealed class DailyCheckIn
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public int MoodScore { get; set; }
    public int EnergyScore { get; set; }
    public int StressScore { get; set; }
    public int ConnectionScore { get; set; }
    public int MeaningScore { get; set; }
    public string[] Tags { get; set; } = [];
    public string? Note { get; set; }
    public int DailyBalanceScore { get; set; }
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
