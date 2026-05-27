using Microsoft.EntityFrameworkCore;
using WithinAPI.Domain;

namespace WithinAPI.Data;

public sealed class WithinDbContext(DbContextOptions<WithinDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Provider> Providers => Set<Provider>();
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<WithinRole>();
        modelBuilder.HasPostgresEnum<WithinLens>();
        modelBuilder.HasPostgresEnum<EventJoinState>();
        modelBuilder.HasPostgresEnum<EventStatus>();
        modelBuilder.HasPostgresEnum<SignupType>();
        modelBuilder.HasPostgresEnum<NotificationKind>();

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

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasIndex(item => new { item.Lens, item.StartUtc });
            entity.HasIndex(item => item.ProviderId);
            entity.Property(item => item.Tags).HasColumnType("text[]");
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

        Seed(modelBuilder);
    }

    private static void Seed(ModelBuilder modelBuilder)
    {
        var now = new DateTimeOffset(2026, 5, 27, 0, 0, 0, TimeSpan.Zero);
        var demoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var trackOwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var pranaOwnerId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var trackProviderId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var pranaProviderId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var runEventId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var meditationEventId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        var moveCommunityId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        var seekCommunityId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        modelBuilder.Entity<User>().HasData(
            User.Seed(demoUserId, "Demo User", "demo@within.local", WithinRole.User, now),
            User.Seed(trackOwnerId, "TheTrack Provider", "provider@thetrack.local", WithinRole.Provider, now),
            User.Seed(pranaOwnerId, "Prana Provider", "provider@prana.local", WithinRole.Provider, now));

        modelBuilder.Entity<Provider>().HasData(
            new Provider
            {
                Id = trackProviderId,
                OwnerUserId = trackOwnerId,
                Name = "TheTrack Langley Park",
                Slug = "thetrack-langley-park",
                Bio = "Run club, HYROX conditioning, pilates, and outdoor fitness in Perth.",
                Lens = WithinLens.Move,
                Location = "Langley Park, Perth",
                WebsiteUrl = "https://example.com/thetrack",
                IsVerified = true,
                CreatedUtc = now
            },
            new Provider
            {
                Id = pranaProviderId,
                OwnerUserId = pranaOwnerId,
                Name = "Prana Wellness",
                Slug = "prana-wellness",
                Bio = "Meditation, spiritual healing, breathwork, retreats, and reflection circles.",
                Lens = WithinLens.Seek,
                Location = "North Perth",
                WebsiteUrl = "https://example.com/prana",
                IsVerified = true,
                CreatedUtc = now
            });

        modelBuilder.Entity<Event>().HasData(
            new Event
            {
                Id = runEventId,
                ProviderId = trackProviderId,
                Title = "Saturday Run Club",
                Description = "Beginner-friendly social run followed by coffee.",
                Lens = WithinLens.Move,
                LocationName = "Langley Park",
                IsOnline = false,
                StartUtc = now.AddDays(3).AddHours(23),
                EndUtc = now.AddDays(4).AddHours(1),
                PriceAmount = 0,
                Currency = "AUD",
                Capacity = 32,
                SignupType = SignupType.Internal,
                Status = EventStatus.Published,
                Tags = ["free", "weekend", "beginner-friendly"],
                CreatedUtc = now
            },
            new Event
            {
                Id = meditationEventId,
                ProviderId = pranaProviderId,
                Title = "Guided Meditation Circle",
                Description = "A calm circle for breath awareness, grounding, and reflection.",
                Lens = WithinLens.Seek,
                LocationName = "North Perth Wellness Studio",
                IsOnline = false,
                StartUtc = now.AddDays(4).AddHours(1),
                EndUtc = now.AddDays(4).AddHours(2),
                PriceAmount = 0,
                Currency = "AUD",
                Capacity = 18,
                SignupType = SignupType.Internal,
                Status = EventStatus.Published,
                Tags = ["free", "weekend", "meditation"],
                CreatedUtc = now
            });

        modelBuilder.Entity<Community>().HasData(
            new Community
            {
                Id = moveCommunityId,
                ProviderId = trackProviderId,
                Name = "TheTrack Community",
                Description = "Run club, HYROX, pilates, and outdoor training updates.",
                Lens = WithinLens.Move,
                Location = "Perth",
                CreatedUtc = now
            },
            new Community
            {
                Id = seekCommunityId,
                ProviderId = pranaProviderId,
                Name = "Prana Circle",
                Description = "Meditation, reflection, and spiritual growth discussions.",
                Lens = WithinLens.Seek,
                Location = "Perth",
                CreatedUtc = now
            });
    }
}
