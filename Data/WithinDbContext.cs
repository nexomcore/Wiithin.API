using Microsoft.EntityFrameworkCore;
using WithinAPI.Domain;

namespace WithinAPI.Data;

public sealed class WithinDbContext(DbContextOptions<WithinDbContext> options) : DbContext(options)
{
    public const string Schema = "within";

    public DbSet<User> Users => Set<User>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ProviderApplication> ProviderApplications => Set<ProviderApplication>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<SavedEvent> SavedEvents => Set<SavedEvent>();
    public DbSet<Community> Communities => Set<Community>();
    public DbSet<CommunityMember> CommunityMembers => Set<CommunityMember>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<CommunityPost> CommunityPosts => Set<CommunityPost>();
    public DbSet<CommunityTopic> CommunityTopics => Set<CommunityTopic>();
    public DbSet<CommunityPostTopic> CommunityPostTopics => Set<CommunityPostTopic>();
    public DbSet<CommunityComment> CommunityComments => Set<CommunityComment>();
    public DbSet<CommunityHelpfulReaction> CommunityHelpfulReactions => Set<CommunityHelpfulReaction>();
    public DbSet<SavedCommunityPost> SavedCommunityPosts => Set<SavedCommunityPost>();
    public DbSet<CommunityReport> CommunityReports => Set<CommunityReport>();
    public DbSet<Circle> Circles => Set<Circle>();
    public DbSet<CircleMember> CircleMembers => Set<CircleMember>();
    public DbSet<CircleJoinRequest> CircleJoinRequests => Set<CircleJoinRequest>();
    public DbSet<CircleRole> CircleRoles => Set<CircleRole>();
    public DbSet<CircleThread> CircleThreads => Set<CircleThread>();
    public DbSet<CircleThreadComment> CircleThreadComments => Set<CircleThreadComment>();
    public DbSet<CircleEvent> CircleEvents => Set<CircleEvent>();
    public DbSet<CircleHelpfulReaction> CircleHelpfulReactions => Set<CircleHelpfulReaction>();
    public DbSet<CircleReaction> CircleReactions => Set<CircleReaction>();
    public DbSet<CirclePoll> CirclePolls => Set<CirclePoll>();
    public DbSet<CirclePollOption> CirclePollOptions => Set<CirclePollOption>();
    public DbSet<CirclePollVote> CirclePollVotes => Set<CirclePollVote>();
    public DbSet<CircleWeeklyCheckInResponse> CircleWeeklyCheckInResponses => Set<CircleWeeklyCheckInResponse>();
    public DbSet<CircleInvite> CircleInvites => Set<CircleInvite>();
    public DbSet<CircleReport> CircleReports => Set<CircleReport>();
    public DbSet<CircleGuideline> CircleGuidelines => Set<CircleGuideline>();
    public DbSet<CircleAnnouncement> CircleAnnouncements => Set<CircleAnnouncement>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<UserPrivacySettings> UserPrivacySettings => Set<UserPrivacySettings>();
    public DbSet<EventInvite> EventInvites => Set<EventInvite>();
    public DbSet<Mention> Mentions => Set<Mention>();
    public DbSet<UserReport> UserReports => Set<UserReport>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<PushToken> PushTokens => Set<PushToken>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<NotificationMute> NotificationMutes => Set<NotificationMute>();
    public DbSet<NotificationSchedule> NotificationSchedules => Set<NotificationSchedule>();
    public DbSet<DailyCheckIn> DailyCheckIns => Set<DailyCheckIn>();
    public DbSet<MonthlyProfile> MonthlyProfiles => Set<MonthlyProfile>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<MarketFitSubmission> MarketFitSubmissions => Set<MarketFitSubmission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.HasPostgresEnum<WithinRole>();
        modelBuilder.HasPostgresEnum<WithinLens>();
        modelBuilder.HasPostgresEnum<EventJoinState>();
        modelBuilder.HasPostgresEnum<EventStatus>();
        modelBuilder.HasPostgresEnum<SignupType>();
        modelBuilder.HasPostgresEnum<NotificationKind>();
        modelBuilder.HasPostgresEnum<NotificationTargetType>();
        modelBuilder.HasPostgresEnum<NotificationMuteTargetType>();
        modelBuilder.HasPostgresEnum<ProviderApplicationStatus>();
        modelBuilder.HasPostgresEnum<ProviderCategory>();
        modelBuilder.HasPostgresEnum<CommunityPostType>();
        modelBuilder.HasPostgresEnum<CommunityContentStatus>();
        modelBuilder.HasPostgresEnum<CommunityReportReason>();
        modelBuilder.HasPostgresEnum<CommunityReportStatus>();
        modelBuilder.HasPostgresEnum<CircleType>();
        modelBuilder.HasPostgresEnum<CircleVisibility>();
        modelBuilder.HasPostgresEnum<CircleStatus>();
        modelBuilder.HasPostgresEnum<CircleMemberStatus>();
        modelBuilder.HasPostgresEnum<CircleMemberRole>();
        modelBuilder.HasPostgresEnum<CircleJoinRequestStatus>();
        modelBuilder.HasPostgresEnum<CircleRoleKind>();
        modelBuilder.HasPostgresEnum<CircleEventStatus>();
        modelBuilder.HasPostgresEnum<ConnectionStatus>();
        modelBuilder.HasPostgresEnum<ProfileVisibility>();
        modelBuilder.HasPostgresEnum<RsvpVisibility>();
        modelBuilder.HasPostgresEnum<TaggingPermission>();
        modelBuilder.HasPostgresEnum<FriendRequestPermission>();
        modelBuilder.HasPostgresEnum<EventInviteStatus>();
        modelBuilder.HasPostgresEnum<MentionSourceType>();
        modelBuilder.HasPostgresEnum<CircleIdentityMode>();
        modelBuilder.HasPostgresEnum<CirclePrivacyType>();
        modelBuilder.HasPostgresEnum<MemberListVisibility>();
        modelBuilder.HasPostgresEnum<CirclePostVisibility>();
        modelBuilder.HasPostgresEnum<CirclePostType>();
        modelBuilder.HasPostgresEnum<CircleReactionType>();
        modelBuilder.HasPostgresEnum<WeeklyCheckInMood>();
        modelBuilder.HasPostgresEnum<CircleInviteStatus>();
        modelBuilder.HasPostgresEnum<UserReportStatus>();
        modelBuilder.HasPostgresEnum<UserReportReason>();

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(item => item.Email).IsUnique();
            entity.Property(item => item.Email).HasMaxLength(320);
            entity.Property(item => item.DisplayName).HasMaxLength(160);
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.HasIndex(item => item.OwnerUserId);
            entity.Property(item => item.Name).HasMaxLength(180);
            entity.Property(item => item.Slug).HasMaxLength(180);
            entity.HasIndex(item => item.Slug).IsUnique();
        });

        modelBuilder.Entity<ProviderApplication>(entity =>
        {
            entity.HasIndex(item => new { item.Status, item.SubmittedUtc });
            entity.HasIndex(item => item.ContactEmail);
            entity.Property(item => item.ContactName).HasMaxLength(180);
            entity.Property(item => item.ContactEmail).HasMaxLength(320);
            entity.Property(item => item.ContactPhone).HasMaxLength(80);
            entity.Property(item => item.PreferredContactMethod).HasMaxLength(80);
            entity.Property(item => item.ProviderName).HasMaxLength(180);
            entity.Property(item => item.BusinessType).HasMaxLength(120);
            entity.Property(item => item.Abn).HasMaxLength(40);
            entity.Property(item => item.Location).HasMaxLength(180);
            entity.Property(item => item.YearsPracticing).HasMaxLength(80);
            entity.Property(item => item.InsuranceStatus).HasMaxLength(120);
            entity.Property(item => item.WorkingWithChildrenCheck).HasMaxLength(120);
            entity.Property(item => item.FirstAidCpr).HasMaxLength(120);
            entity.Property(item => item.HasEventsReady).HasMaxLength(80);
            entity.Property(item => item.ServiceAreas).HasColumnType("text[]");
            entity.Property(item => item.DeliveryModes).HasColumnType("text[]");
            entity.Property(item => item.ServicesOffered).HasColumnType("text[]");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasIndex(item => new { item.Lens, item.StartUtc });
            entity.HasIndex(item => item.ProviderId);
            entity.Property(item => item.Tags).HasColumnType("text[]");
            entity.Property(item => item.BringItems).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(item => item.Facilities).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(item => item.AccessibilityFeatures).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(item => item.AtmosphereTags).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(item => item.DietaryOptions).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(item => item.PhysicalIntensity).HasMaxLength(20);
            entity.Property(item => item.SocialInteractionLevel).HasMaxLength(20);
            entity.Property(item => item.ExperienceLevel).HasMaxLength(80);
            entity.Property(item => item.AgeRestriction).HasMaxLength(40);
            entity.Property(item => item.BringNotes).HasMaxLength(1000);
            entity.Property(item => item.FoodNotes).HasMaxLength(1000);
            entity.Property(item => item.SafetyNotes).HasMaxLength(1500);
        });

        modelBuilder.Entity<EventRegistration>(entity =>
        {
            entity.Property(item => item.Visibility).HasDefaultValue(RsvpVisibility.FriendsOnly);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasIndex(item => item.ParentCommentId);
        });

        modelBuilder.Entity<CommunityPost>(entity =>
        {
            entity.HasIndex(item => new { item.Status, item.CreatedAt });
            entity.HasIndex(item => item.LinkedEventId);
            entity.HasIndex(item => item.UserId);
            entity.Property(item => item.Title).HasMaxLength(120);
            entity.Property(item => item.Body).HasMaxLength(3000);
        });

        modelBuilder.Entity<CommunityTopic>(entity =>
        {
            entity.HasIndex(item => item.Slug).IsUnique();
            entity.Property(item => item.Name).HasMaxLength(80);
            entity.Property(item => item.Slug).HasMaxLength(80);
            entity.Property(item => item.Description).HasMaxLength(240);
        });

        modelBuilder.Entity<CommunityPostTopic>(entity =>
        {
            entity.HasKey(item => new { item.PostId, item.TopicId });
            entity.HasIndex(item => item.TopicId);
        });

        modelBuilder.Entity<CommunityComment>(entity =>
        {
            entity.HasIndex(item => new { item.PostId, item.CreatedAt });
            entity.HasIndex(item => item.UserId);
            entity.Property(item => item.Body).HasMaxLength(1000);
        });

        modelBuilder.Entity<CommunityHelpfulReaction>(entity =>
        {
            entity.HasIndex(item => new { item.PostId, item.UserId })
                .IsUnique()
                .HasFilter("\"PostId\" IS NOT NULL");
            entity.HasIndex(item => new { item.CommentId, item.UserId })
                .IsUnique()
                .HasFilter("\"CommentId\" IS NOT NULL");
        });

        modelBuilder.Entity<SavedCommunityPost>(entity =>
        {
            entity.HasIndex(item => new { item.PostId, item.UserId }).IsUnique();
        });

        modelBuilder.Entity<CommunityReport>(entity =>
        {
            entity.HasIndex(item => new { item.Status, item.CreatedAt });
            entity.HasIndex(item => item.PostId);
            entity.HasIndex(item => item.CommentId);
            entity.Property(item => item.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<Circle>(entity =>
        {
            entity.HasIndex(item => item.Slug).IsUnique();
            entity.HasIndex(item => new { item.Visibility, item.Status });
            entity.HasIndex(item => item.CreatedByUserId);
            entity.Property(item => item.Name).HasMaxLength(120);
            entity.Property(item => item.Slug).HasMaxLength(120);
            entity.Property(item => item.Description).HasMaxLength(600);
            entity.Property(item => item.Rules).HasMaxLength(2000);
            entity.Property(item => item.AllowPseudonyms).HasDefaultValue(true);
            entity.Property(item => item.AllowHiddenProfiles).HasDefaultValue(true);
            entity.Property(item => item.AllowAnonymousPosts).HasDefaultValue(false);
            entity.Property(item => item.MemberListVisibility).HasDefaultValue(MemberListVisibility.MembersOnly);
            entity.Property(item => item.DefaultPostVisibility).HasDefaultValue(CirclePostVisibility.MembersOnly);
            entity.Property(item => item.DefaultEventRsvpVisibility).HasDefaultValue(RsvpVisibility.FriendsOnly);
        });

        modelBuilder.Entity<CircleMember>(entity =>
        {
            entity.HasIndex(item => new { item.CircleId, item.UserId }).IsUnique();
            entity.HasIndex(item => new { item.UserId, item.Status });
            entity.HasIndex(item => new { item.CircleId, item.Role, item.Status });
            entity.Property(item => item.Role).HasDefaultValue(CircleMemberRole.Member);
            entity.Property(item => item.DisplayNameOverride).HasMaxLength(40);
        });

        modelBuilder.Entity<CircleJoinRequest>(entity =>
        {
            entity.HasIndex(item => new { item.CircleId, item.UserId }).IsUnique();
            entity.HasIndex(item => new { item.CircleId, item.Status, item.RequestedAt });
        });

        modelBuilder.Entity<CircleRole>(entity =>
        {
            entity.HasIndex(item => new { item.CircleId, item.UserId, item.Role }).IsUnique();
            entity.HasIndex(item => item.UserId);
        });

        modelBuilder.Entity<CircleThread>(entity =>
        {
            entity.HasIndex(item => new { item.CircleId, item.Status, item.CreatedAt });
            entity.HasIndex(item => new { item.CircleId, item.IsPinned, item.PostType, item.CreatedAt });
            entity.HasIndex(item => new { item.CircleId, item.PostType, item.WeeklyCheckInWeekStart });
            entity.HasIndex(item => item.LinkedEventId);
            entity.HasIndex(item => item.UserId);
            entity.Property(item => item.PostType).HasDefaultValue(CirclePostType.Standard);
            entity.Property(item => item.IsPinned).HasDefaultValue(false);
            entity.Property(item => item.IsAnonymous).HasDefaultValue(false);
            entity.Property(item => item.Title).HasMaxLength(140);
            entity.Property(item => item.Body).HasMaxLength(4000);
            entity.Property(item => item.ImageUrl).HasMaxLength(1000);
        });

        modelBuilder.Entity<CircleThreadComment>(entity =>
        {
            entity.HasIndex(item => new { item.ThreadId, item.CreatedAt });
            entity.HasIndex(item => item.UserId);
            entity.Property(item => item.IsAnonymous).HasDefaultValue(false);
            entity.Property(item => item.Body).HasMaxLength(1200);
        });

        modelBuilder.Entity<CircleEvent>(entity =>
        {
            entity.HasIndex(item => new { item.CircleId, item.EventId }).IsUnique();
            entity.HasIndex(item => item.EventId);
            entity.Property(item => item.OptionalNote).HasMaxLength(500);
        });

        modelBuilder.Entity<CircleHelpfulReaction>(entity =>
        {
            entity.HasIndex(item => new { item.ThreadId, item.UserId })
                .IsUnique()
                .HasFilter("\"ThreadId\" IS NOT NULL");
            entity.HasIndex(item => new { item.CommentId, item.UserId })
                .IsUnique()
                .HasFilter("\"CommentId\" IS NOT NULL");
        });

        modelBuilder.Entity<CircleReaction>(entity =>
        {
            entity.HasIndex(item => new { item.ThreadId, item.UserId, item.ReactionType })
                .IsUnique()
                .HasFilter("\"ThreadId\" IS NOT NULL");
            entity.HasIndex(item => new { item.CommentId, item.UserId, item.ReactionType })
                .IsUnique()
                .HasFilter("\"CommentId\" IS NOT NULL");
        });

        modelBuilder.Entity<CirclePoll>(entity =>
        {
            entity.HasIndex(item => item.ThreadId).IsUnique();
            entity.Property(item => item.Question).HasMaxLength(240);
        });

        modelBuilder.Entity<CirclePollOption>(entity =>
        {
            entity.HasIndex(item => new { item.PollId, item.SortOrder });
            entity.Property(item => item.Text).HasMaxLength(120);
        });

        modelBuilder.Entity<CirclePollVote>(entity =>
        {
            entity.HasIndex(item => new { item.PollId, item.UserId }).IsUnique();
            entity.HasIndex(item => item.OptionId);
        });

        modelBuilder.Entity<CircleWeeklyCheckInResponse>(entity =>
        {
            entity.HasIndex(item => new { item.ThreadId, item.UserId }).IsUnique();
        });

        modelBuilder.Entity<CircleInvite>(entity =>
        {
            entity.HasIndex(item => new { item.CircleId, item.InvitedUserId, item.Status });
            entity.HasIndex(item => new { item.InvitedUserId, item.Status });
        });

        modelBuilder.Entity<CircleReport>(entity =>
        {
            entity.HasIndex(item => new { item.Status, item.CreatedAt });
            entity.HasIndex(item => item.CircleId);
            entity.HasIndex(item => item.ThreadId);
            entity.HasIndex(item => item.CommentId);
            entity.HasIndex(item => item.CircleEventId);
            entity.Property(item => item.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<CircleGuideline>(entity =>
        {
            entity.HasIndex(item => new { item.CircleId, item.SortOrder });
            entity.Property(item => item.Title).HasMaxLength(120);
            entity.Property(item => item.Body).HasMaxLength(1000);
        });

        modelBuilder.Entity<CircleAnnouncement>(entity =>
        {
            entity.HasIndex(item => new { item.CircleId, item.IsPinned, item.CreatedAt });
            entity.Property(item => item.Body).HasMaxLength(1000);
        });

        modelBuilder.Entity<Connection>(entity =>
        {
            entity.HasIndex(item => new { item.RequesterUserId, item.ReceiverUserId });
            entity.HasIndex(item => new { item.ReceiverUserId, item.Status });
            entity.HasIndex(item => new { item.RequesterUserId, item.Status });
        });

        modelBuilder.Entity<UserPrivacySettings>(entity =>
        {
            entity.HasKey(item => item.UserId);
        });

        modelBuilder.Entity<EventInvite>(entity =>
        {
            entity.HasIndex(item => new { item.EventId, item.InvitedUserId, item.Status });
            entity.HasIndex(item => new { item.InvitedUserId, item.Status });
            entity.Property(item => item.Message).HasMaxLength(500);
        });

        modelBuilder.Entity<Mention>(entity =>
        {
            entity.HasIndex(item => new { item.MentionedUserId, item.CreatedAt });
            entity.HasIndex(item => new { item.SourceType, item.SourceId });
        });

        modelBuilder.Entity<UserReport>(entity =>
        {
            entity.HasIndex(item => new { item.ReportedUserId, item.Status });
            entity.HasIndex(item => new { item.ReportedByUserId, item.CreatedAt });
            entity.Property(item => item.Details).HasMaxLength(1000);
        });

        modelBuilder.Entity<DailyCheckIn>(entity =>
        {
            entity.HasIndex(item => new { item.UserId, item.CheckInDate }).IsUnique();
            entity.Property(item => item.Tags).HasColumnType("text[]");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasIndex(item => new { item.UserId, item.IsRead, item.CreatedUtc });
            entity.HasIndex(item => new { item.TargetType, item.TargetId });
            entity.Property(item => item.Title).HasMaxLength(160);
            entity.Property(item => item.Body).HasMaxLength(500);
        });

        modelBuilder.Entity<PushToken>(entity =>
        {
            entity.ToTable("push_tokens");
            entity.HasIndex(item => item.Token).IsUnique();
            entity.HasIndex(item => item.UserId);
            entity.Property(item => item.Platform).HasMaxLength(40);
            entity.Property(item => item.Token).HasMaxLength(512);
        });

        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.ToTable("notification_preferences");
            entity.HasIndex(item => item.UserId).IsUnique();
            entity.Property(item => item.DailyMotivationEnabled).HasDefaultValue(true);
            entity.Property(item => item.EventRemindersEnabled).HasDefaultValue(true);
            entity.Property(item => item.CommunitySummariesEnabled).HasDefaultValue(true);
            entity.Property(item => item.ProviderNewEventsEnabled).HasDefaultValue(true);
            entity.Property(item => item.FriendRequestsEnabled).HasDefaultValue(true);
            entity.Property(item => item.EventInvitesEnabled).HasDefaultValue(true);
            entity.Property(item => item.FriendActivityEnabled).HasDefaultValue(true);
            entity.Property(item => item.CircleRepliesEnabled).HasDefaultValue(true);
            entity.Property(item => item.CommentRepliesEnabled).HasDefaultValue(true);
            entity.Property(item => item.MentionsEnabled).HasDefaultValue(true);
        });

        modelBuilder.Entity<NotificationMute>(entity =>
        {
            entity.ToTable("notification_mutes");
            entity.HasIndex(item => new { item.UserId, item.TargetType, item.TargetId }).IsUnique();
        });

        modelBuilder.Entity<MarketFitSubmission>(entity =>
        {
            entity.HasIndex(item => new { item.Audience, item.CreatedUtc });
            entity.Property(item => item.Audience).HasMaxLength(40);
            entity.Property(item => item.Name).HasMaxLength(180);
            entity.Property(item => item.Contact).HasMaxLength(320);
            entity.Property(item => item.Source).HasMaxLength(120);
            entity.Property(item => item.AnswersJson).HasColumnType("jsonb");
        });

        modelBuilder.Entity<EventRegistration>()
            .HasIndex(item => new { item.EventId, item.UserId })
            .IsUnique();

        modelBuilder.Entity<SavedEvent>()
            .HasIndex(item => new { item.EventId, item.UserId })
            .IsUnique();

        modelBuilder.Entity<CommunityMember>()
            .HasIndex(item => new { item.CommunityId, item.UserId })
            .IsUnique();

        modelBuilder.Entity<Reaction>()
            .HasIndex(item => new { item.PostId, item.CommentId, item.UserId, item.Kind })
            .IsUnique();

        modelBuilder.Entity<Review>()
            .HasIndex(item => new { item.EventId, item.UserId })
            .IsUnique();

        modelBuilder.Entity<DeviceToken>()
            .HasIndex(item => item.Token)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(item => item.TokenHash)
            .IsUnique();
    }
}
