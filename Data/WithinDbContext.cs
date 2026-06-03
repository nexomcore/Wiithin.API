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
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
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
        modelBuilder.HasPostgresEnum<ProviderApplicationStatus>();
        modelBuilder.HasPostgresEnum<ProviderCategory>();

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
        });

        modelBuilder.Entity<DailyCheckIn>(entity =>
        {
            entity.HasIndex(item => new { item.UserId, item.CheckInDate }).IsUnique();
            entity.Property(item => item.Tags).HasColumnType("text[]");
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
