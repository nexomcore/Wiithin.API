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
    Attended
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
