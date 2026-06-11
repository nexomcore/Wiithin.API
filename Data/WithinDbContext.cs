using Microsoft.EntityFrameworkCore;
using WithinAPI.Domain;

namespace WithinAPI.Data;

public sealed class WithinDbContext(DbContextOptions<WithinDbContext> options) : DbContext(options)
{
    public const string Schema = "within";

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ProviderService> ProviderServices => Set<ProviderService>();
    public DbSet<ProviderApplication> ProviderApplications => Set<ProviderApplication>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<SavedEvent> SavedEvents => Set<SavedEvent>();
    public DbSet<Community> Communities => Set<Community>();
    public DbSet<CommunityMember> CommunityMembers => Set<CommunityMember>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
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
    public DbSet<UserWellbeingProfile> UserWellbeingProfiles => Set<UserWellbeingProfile>();
    public DbSet<UserWellbeingInterest> UserWellbeingInterests => Set<UserWellbeingInterest>();
    public DbSet<UserWellbeingGoal> UserWellbeingGoals => Set<UserWellbeingGoal>();
    public DbSet<DailyCheckIn> DailyCheckIns => Set<DailyCheckIn>();
    public DbSet<MonthlyProfile> MonthlyProfiles => Set<MonthlyProfile>();
    public DbSet<HabitTemplate> HabitTemplates => Set<HabitTemplate>();
    public DbSet<UserHabit> UserHabits => Set<UserHabit>();
    public DbSet<HabitCompletion> HabitCompletions => Set<HabitCompletion>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<MarketFitSubmission> MarketFitSubmissions => Set<MarketFitSubmission>();

    // ── Move pillar ──
    public DbSet<MoveProfile> MoveProfiles => Set<MoveProfile>();
    public DbSet<BodyMetricLog> BodyMetricLogs => Set<BodyMetricLog>();
    public DbSet<CalculatorResult> CalculatorResults => Set<CalculatorResult>();
    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
    public DbSet<WorkoutDay> WorkoutDays => Set<WorkoutDay>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();
    public DbSet<WorkoutExerciseLog> WorkoutExerciseLogs => Set<WorkoutExerciseLog>();
    public DbSet<DietPlan> DietPlans => Set<DietPlan>();
    public DbSet<DietMeal> DietMeals => Set<DietMeal>();
    public DbSet<DietMealItem> DietMealItems => Set<DietMealItem>();
    public DbSet<DietLog> DietLogs => Set<DietLog>();
    public DbSet<TrainerClient> TrainerClients => Set<TrainerClient>();
    public DbSet<TrainerNote> TrainerNotes => Set<TrainerNote>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<ChallengeTask> ChallengeTasks => Set<ChallengeTask>();
    public DbSet<ChallengeParticipant> ChallengeParticipants => Set<ChallengeParticipant>();
    public DbSet<ChallengeProgress> ChallengeProgresses => Set<ChallengeProgress>();

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
        modelBuilder.HasPostgresEnum<ProviderType>();
        modelBuilder.HasPostgresEnum<ProviderVerificationStatus>();
        modelBuilder.HasPostgresEnum<ProviderPriceType>();
        modelBuilder.HasPostgresEnum<ProviderServiceDeliveryMode>();
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

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(item => item.Key).IsUnique();
            entity.Property(item => item.Key).HasMaxLength(40);
            entity.Property(item => item.Name).HasMaxLength(80);
            entity.Property(item => item.Description).HasMaxLength(240);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(item => item.Email).IsUnique();
            entity.HasIndex(item => item.RoleId);
            entity.Property(item => item.Email).HasMaxLength(320);
            entity.Property(item => item.DisplayName).HasMaxLength(160);
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.HasIndex(item => item.OwnerUserId);
            entity.HasIndex(item => new { item.ProviderType, item.Lens, item.IsActive });
            entity.HasIndex(item => new { item.VerificationStatus, item.CreatedUtc });
            entity.Property(item => item.ProviderType).HasDefaultValue(ProviderType.Business);
            entity.Property(item => item.VerificationStatus).HasDefaultValue(ProviderVerificationStatus.Unverified);
            entity.Property(item => item.Name).HasMaxLength(180);
            entity.Property(item => item.Slug).HasMaxLength(180);
            entity.HasIndex(item => item.Slug).IsUnique();
            entity.Property(item => item.LegalName).HasMaxLength(180);
            entity.Property(item => item.Categories).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(item => item.ProfileImageUrl).HasMaxLength(1000);
            entity.Property(item => item.CoverImageUrl).HasMaxLength(1000);
            entity.Property(item => item.Location).HasMaxLength(180);
            entity.Property(item => item.Suburb).HasMaxLength(120);
            entity.Property(item => item.City).HasMaxLength(120);
            entity.Property(item => item.State).HasMaxLength(80);
            entity.Property(item => item.Country).HasMaxLength(80);
            entity.Property(item => item.Phone).HasMaxLength(80);
            entity.Property(item => item.Email).HasMaxLength(320);
            entity.Property(item => item.IsActive).HasDefaultValue(true);
            entity.Property(item => item.ShowWebsitePublicly).HasDefaultValue(true);
            entity.Property(item => item.PractitionerTitle).HasMaxLength(120);
            entity.Property(item => item.ServicesOffered).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(item => item.Languages).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(item => item.InPersonAvailable).HasDefaultValue(true);
            entity.Property(item => item.BusinessType).HasMaxLength(120);
            entity.Property(item => item.Abn).HasMaxLength(40);
            entity.Property(item => item.Facilities).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(item => item.AccessibilityFeatures).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(item => item.TeamMembers).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
        });

        modelBuilder.Entity<ProviderService>(entity =>
        {
            entity.HasIndex(item => new { item.ProviderId, item.IsActive });
            entity.HasIndex(item => new { item.Lens, item.DeliveryMode, item.IsActive });
            entity.Property(item => item.Name).HasMaxLength(160);
            entity.Property(item => item.Category).HasMaxLength(120);
            entity.Property(item => item.Location).HasMaxLength(180);
        });

        modelBuilder.Entity<ProviderApplication>(entity =>
        {
            entity.HasIndex(item => new { item.Status, item.SubmittedUtc });
            entity.HasIndex(item => item.ProviderType);
            entity.Property(item => item.ProviderType).HasDefaultValue(ProviderType.Business);
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
            entity.HasIndex(item => item.ProviderServiceId);
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
            entity.Property(item => item.EventType).HasMaxLength(40).HasDefaultValue("class");
            entity.Property(item => item.RetreatDuration).HasMaxLength(120);
            entity.Property(item => item.RetreatFocus).HasMaxLength(60);
            entity.Property(item => item.DifficultyLevel).HasMaxLength(40);
            entity.Property(item => item.WhatsIncluded).HasMaxLength(2000);
            entity.Property(item => item.WhatToBring).HasMaxLength(2000);
            entity.Property(item => item.FacilitiesAvailable).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
        });

        modelBuilder.Entity<EventRegistration>(entity =>
        {
            entity.Property(item => item.Visibility).HasDefaultValue(RsvpVisibility.FriendsOnly);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasIndex(item => item.ParentCommentId);
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
            entity.HasIndex(item => item.ParentCommentId);
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
            entity.Property(item => item.Mood).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.MoodScore);
            entity.Property(item => item.Energy).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.EnergyLevel);
            entity.Property(item => item.StressLevel);
            entity.Property(item => item.DidMoveToday).HasDefaultValue(false);
            entity.Property(item => item.DidMeditateToday).HasDefaultValue(false);
            entity.Property(item => item.SleepQuality).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Intention).HasConversion<string>().HasMaxLength(48);
            entity.Property(item => item.SleepHours).HasColumnType("numeric(4,1)");
            entity.Property(item => item.Note).HasMaxLength(500);
            entity.Property(item => item.JournalEntry).HasMaxLength(1000);
            entity.Property(item => item.SuggestedActionKey).HasMaxLength(64);
        });

        modelBuilder.Entity<UserWellbeingProfile>(entity =>
        {
            entity.HasIndex(item => item.UserId).IsUnique();
            entity.Property(item => item.FirstName).HasMaxLength(80);
            entity.Property(item => item.DisplayName).HasMaxLength(120);
            entity.Property(item => item.UsePseudonym).HasDefaultValue(false);
            entity.Property(item => item.Pseudonym).HasMaxLength(80);
            entity.Property(item => item.AgeRange).HasMaxLength(40);
            entity.Property(item => item.Gender).HasMaxLength(80);
            entity.Property(item => item.LocationCountry).HasMaxLength(80);
            entity.Property(item => item.LocationCity).HasMaxLength(120);
            entity.Property(item => item.LocationSuburb).HasMaxLength(120);
            entity.Property(item => item.ProfilePhotoUrl).HasMaxLength(1000);
            entity.Property(item => item.HeightCm).HasColumnType("numeric(5,1)");
            entity.Property(item => item.WeightKg).HasColumnType("numeric(5,1)");
            entity.Property(item => item.ActivityLevel).HasMaxLength(40);
            entity.Property(item => item.AverageSleepHours).HasColumnType("numeric(3,1)");
            entity.Property(item => item.WaterIntakeLitres).HasColumnType("numeric(3,1)");
            entity.Property(item => item.MeditationFrequency).HasMaxLength(80);
            entity.Property(item => item.BodyFatPercentage).HasColumnType("numeric(4,1)");
            entity.Property(item => item.Vo2Max).HasColumnType("numeric(5,1)");
            entity.Property(item => item.WearableProvider).HasMaxLength(80);
            entity.Property(item => item.WearableConnected).HasDefaultValue(false);
            entity.Property(item => item.OnboardingCompleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<UserWellbeingInterest>(entity =>
        {
            entity.HasIndex(item => new { item.UserId, item.InterestKey }).IsUnique();
            entity.Property(item => item.Category).HasMaxLength(20);
            entity.Property(item => item.InterestKey).HasMaxLength(80);
            entity.Property(item => item.InterestLabel).HasMaxLength(120);
        });

        modelBuilder.Entity<UserWellbeingGoal>(entity =>
        {
            entity.HasIndex(item => new { item.UserId, item.GoalKey }).IsUnique();
            entity.Property(item => item.GoalKey).HasMaxLength(80);
            entity.Property(item => item.GoalLabel).HasMaxLength(120);
        });

        modelBuilder.Entity<HabitTemplate>(entity =>
        {
            entity.Property(item => item.Category).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Name).HasMaxLength(60);
            entity.Property(item => item.Description).HasMaxLength(240);
            entity.Property(item => item.IconKey).HasMaxLength(48);
            entity.HasIndex(item => item.Name).IsUnique();
        });

        modelBuilder.Entity<UserHabit>(entity =>
        {
            entity.Property(item => item.Category).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Name).HasMaxLength(60);
            entity.HasIndex(item => new { item.UserId, item.IsActive });
        });

        modelBuilder.Entity<HabitCompletion>(entity =>
        {
            entity.HasIndex(item => new { item.UserHabitId, item.CompletionDate }).IsUnique();
            entity.HasIndex(item => new { item.UserId, item.CompletionDate });
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

        ConfigureMoveModule(modelBuilder);

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

    // Move enums are stored as text (HasConversion<string>) so the module's migration is a
    // plain set of CreateTable calls and never touches the shared Postgres native-enum block.
    private static void ConfigureMoveModule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MoveProfile>(entity =>
        {
            entity.HasIndex(item => item.UserId).IsUnique();
            entity.Property(item => item.HeightCm).HasColumnType("numeric(5,1)");
            entity.Property(item => item.WeightKg).HasColumnType("numeric(5,1)");
            entity.Property(item => item.GenderForCalculation).HasMaxLength(40);
            entity.Property(item => item.ActivityLevel).HasMaxLength(40);
            entity.Property(item => item.FitnessGoal).HasMaxLength(40);
            entity.Property(item => item.ExperienceLevel).HasMaxLength(40);
            entity.Property(item => item.PreferredWorkoutType).HasMaxLength(40);
            entity.Property(item => item.DietPreference).HasMaxLength(40);
            entity.Property(item => item.EquipmentAvailable).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(item => item.InjuriesOrLimitations).HasMaxLength(1000);
            entity.Property(item => item.FoodAllergies).HasMaxLength(1000);
        });

        modelBuilder.Entity<BodyMetricLog>(entity =>
        {
            entity.HasIndex(item => new { item.UserId, item.LoggedAt });
            foreach (var column in new[] { "WeightKg", "BodyFatPercentage", "WaistCm", "HipCm", "ChestCm", "ArmCm", "ThighCm", "Bmi", "Bmr", "Tdee" })
            {
                entity.Property(column).HasColumnType("numeric(6,1)");
            }
        });

        modelBuilder.Entity<CalculatorResult>(entity =>
        {
            entity.HasIndex(item => new { item.UserId, item.CreatedUtc });
            entity.Property(item => item.CalculatorType).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.InputJson).HasColumnType("jsonb");
            entity.Property(item => item.ResultJson).HasColumnType("jsonb");
        });

        modelBuilder.Entity<WorkoutPlan>(entity =>
        {
            entity.HasIndex(item => new { item.AssignedToUserId, item.Status });
            entity.HasIndex(item => new { item.IsTemplate, item.WorkoutType });
            entity.Property(item => item.SourceType).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Title).HasMaxLength(160);
            entity.Property(item => item.Description).HasMaxLength(2000);
            entity.Property(item => item.Goal).HasMaxLength(120);
            entity.Property(item => item.WorkoutType).HasMaxLength(40);
            entity.Property(item => item.ExperienceLevel).HasMaxLength(40);
        });

        modelBuilder.Entity<WorkoutDay>(entity =>
        {
            entity.HasIndex(item => new { item.WorkoutPlanId, item.SortOrder });
            entity.Property(item => item.Title).HasMaxLength(160);
            entity.Property(item => item.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<WorkoutExercise>(entity =>
        {
            entity.HasIndex(item => new { item.WorkoutDayId, item.SortOrder });
            entity.Property(item => item.ExerciseName).HasMaxLength(160);
            entity.Property(item => item.Reps).HasMaxLength(40);
            entity.Property(item => item.Weight).HasColumnType("numeric(6,1)");
            entity.Property(item => item.Tempo).HasMaxLength(40);
            entity.Property(item => item.Notes).HasMaxLength(500);
        });

        modelBuilder.Entity<WorkoutLog>(entity =>
        {
            entity.HasIndex(item => new { item.UserId, item.LogDate });
            // One completion per user / workout day / date (repeated sessions create a new date).
            entity.HasIndex(item => new { item.UserId, item.WorkoutDayId, item.LogDate })
                .IsUnique()
                .HasFilter("\"WorkoutDayId\" IS NOT NULL");
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Title).HasMaxLength(160);
            entity.Property(item => item.Notes).HasMaxLength(1000);
        });

        modelBuilder.Entity<WorkoutExerciseLog>(entity =>
        {
            entity.HasIndex(item => item.WorkoutLogId);
            entity.Property(item => item.ActualReps).HasMaxLength(40);
            entity.Property(item => item.ActualWeight).HasColumnType("numeric(6,1)");
            entity.Property(item => item.Notes).HasMaxLength(500);
        });

        modelBuilder.Entity<DietPlan>(entity =>
        {
            entity.HasIndex(item => new { item.AssignedToUserId, item.Status });
            entity.HasIndex(item => new { item.IsTemplate, item.DietPreference });
            entity.Property(item => item.SourceType).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Title).HasMaxLength(160);
            entity.Property(item => item.Description).HasMaxLength(2000);
            entity.Property(item => item.Goal).HasMaxLength(120);
            entity.Property(item => item.DietPreference).HasMaxLength(40);
        });

        modelBuilder.Entity<DietMeal>(entity =>
        {
            entity.HasIndex(item => new { item.DietPlanId, item.SortOrder });
            entity.Property(item => item.MealName).HasMaxLength(120);
            entity.Property(item => item.MealTime).HasMaxLength(40);
        });

        modelBuilder.Entity<DietMealItem>(entity =>
        {
            entity.HasIndex(item => item.DietMealId);
            entity.Property(item => item.FoodName).HasMaxLength(160);
            entity.Property(item => item.Quantity).HasMaxLength(60);
            entity.Property(item => item.Unit).HasMaxLength(40);
            entity.Property(item => item.SubstitutionNotes).HasMaxLength(500);
        });

        modelBuilder.Entity<DietLog>(entity =>
        {
            entity.HasIndex(item => new { item.UserId, item.LogDate });
            entity.HasIndex(item => new { item.UserId, item.DietMealId, item.LogDate }).IsUnique();
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Notes).HasMaxLength(1000);
        });

        modelBuilder.Entity<TrainerClient>(entity =>
        {
            entity.HasIndex(item => new { item.TrainerUserId, item.ClientUserId }).IsUnique();
            entity.HasIndex(item => new { item.ClientUserId, item.Status });
            entity.HasIndex(item => new { item.TrainerUserId, item.Status });
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<TrainerNote>(entity =>
        {
            entity.HasIndex(item => new { item.ClientUserId, item.CreatedUtc });
            entity.HasIndex(item => new { item.TrainerUserId, item.ClientUserId });
            entity.Property(item => item.Visibility).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Note).HasMaxLength(2000);
        });

        modelBuilder.Entity<Challenge>(entity =>
        {
            entity.HasIndex(item => new { item.Visibility, item.IsTemplate });
            entity.HasIndex(item => item.CircleId);
            entity.Property(item => item.ChallengeType).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Visibility).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Title).HasMaxLength(160);
            entity.Property(item => item.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<ChallengeTask>(entity =>
        {
            entity.HasIndex(item => new { item.ChallengeId, item.SortOrder });
            entity.Property(item => item.Title).HasMaxLength(160);
            entity.Property(item => item.Description).HasMaxLength(1000);
            entity.Property(item => item.TargetValue).HasColumnType("numeric(8,2)");
            entity.Property(item => item.TargetUnit).HasMaxLength(40);
        });

        modelBuilder.Entity<ChallengeParticipant>(entity =>
        {
            entity.HasIndex(item => new { item.ChallengeId, item.UserId }).IsUnique();
            entity.HasIndex(item => new { item.UserId, item.Status });
            entity.Property(item => item.DisplayMode).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<ChallengeProgress>(entity =>
        {
            entity.HasIndex(item => new { item.ChallengeId, item.UserId, item.LogDate });
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(item => item.ValueCompleted).HasColumnType("numeric(8,2)");
            entity.Property(item => item.ProofImageUrl).HasMaxLength(1000);
            entity.Property(item => item.Notes).HasMaxLength(1000);
        });
    }
}
