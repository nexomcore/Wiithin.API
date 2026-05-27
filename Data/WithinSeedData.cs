using Microsoft.EntityFrameworkCore;
using WithinAPI.Domain;

namespace WithinAPI.Data;

public static class WithinSeedData
{
    public static async Task EnsureAsync(WithinDbContext db)
    {
        var now = new DateTimeOffset(2026, 5, 27, 0, 0, 0, TimeSpan.Zero);
        var demoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var trackOwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var pranaOwnerId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var trackProviderId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var pranaProviderId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        await UpsertUser(db, User.Seed(demoUserId, "Demo User", "demo@within.local", WithinRole.User, now));
        await UpsertUser(db, User.Seed(trackOwnerId, "TheTrack Provider", "provider@thetrack.local", WithinRole.Provider, now));
        await UpsertUser(db, User.Seed(pranaOwnerId, "Prana Provider", "provider@prana.local", WithinRole.Provider, now));

        await UpsertProvider(db, new Provider
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
        });

        await UpsertProvider(db, new Provider
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

        var events = new[]
        {
            new Event
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                ProviderId = trackProviderId,
                Title = "HIIT Strength",
                Description = "Strength, conditioning, and balance.",
                Lens = WithinLens.Move,
                LocationName = "Northbridge",
                IsOnline = false,
                StartUtc = now.AddDays(3).AddHours(23),
                EndUtc = now.AddDays(4).AddHours(1),
                PriceAmount = 0,
                Currency = "AUD",
                Capacity = 32,
                SignupType = SignupType.Internal,
                ImageUrl = "https://images.unsplash.com/photo-1518611012118-696072aa579a?auto=format&fit=crop&w=900&q=80",
                Status = EventStatus.Published,
                Tags = ["strength", "evening", "beginner-friendly"],
                CreatedUtc = now
            },
            new Event
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
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
                ImageUrl = "https://images.unsplash.com/photo-1506126613408-eca07ce68773?auto=format&fit=crop&w=900&q=80",
                Status = EventStatus.Published,
                Tags = ["meditation", "weekend"],
                CreatedUtc = now
            },
            new Event
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProviderId = pranaProviderId,
                Title = "Yoga Flow",
                Description = "Slow flow, mobility, and breathwork.",
                Lens = WithinLens.Feel,
                LocationName = "Subiaco",
                IsOnline = false,
                StartUtc = now.AddDays(5).AddHours(0),
                EndUtc = now.AddDays(5).AddHours(1),
                PriceAmount = 0,
                Currency = "AUD",
                Capacity = 20,
                SignupType = SignupType.Internal,
                ImageUrl = "https://images.unsplash.com/photo-1599901860904-17e6ed7083a0?auto=format&fit=crop&w=900&q=80",
                Status = EventStatus.Published,
                Tags = ["yoga", "mobility"],
                CreatedUtc = now
            },
            new Event
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                ProviderId = trackProviderId,
                Title = "Run Club",
                Description = "A relaxed social run through Kings Park.",
                Lens = WithinLens.Move,
                LocationName = "Kings Park",
                IsOnline = false,
                StartUtc = now.AddDays(6).AddHours(22),
                EndUtc = now.AddDays(7),
                PriceAmount = 0,
                Currency = "AUD",
                Capacity = 40,
                SignupType = SignupType.Internal,
                ImageUrl = "https://images.unsplash.com/photo-1552674605-db6ffd4facb5?auto=format&fit=crop&w=900&q=80",
                Status = EventStatus.Published,
                Tags = ["run", "social"],
                CreatedUtc = now
            }
        };

        foreach (var evt in events)
        {
            await UpsertEvent(db, evt);
        }

        await UpsertRegistration(db, new EventRegistration
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            EventId = events[2].Id,
            UserId = demoUserId,
            State = EventJoinState.Going,
            CreatedUtc = now,
            UpdatedUtc = now
        });

        await UpsertRegistration(db, new EventRegistration
        {
            Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            EventId = events[3].Id,
            UserId = demoUserId,
            State = EventJoinState.Going,
            CreatedUtc = now,
            UpdatedUtc = now
        });

        await UpsertDailyCheckIn(db, new DailyCheckIn
        {
            Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            UserId = demoUserId,
            CheckInDate = DateOnly.FromDateTime(DateTime.UtcNow),
            MoodScore = 4,
            EnergyScore = 4,
            StressScore = 2,
            ConnectionScore = 3,
            MeaningScore = 4,
            DailyBalanceScore = 75
        });

        await db.SaveChangesAsync();
    }

    private static async Task UpsertUser(WithinDbContext db, User seed)
    {
        var existing = await db.Users.FindAsync(seed.Id);
        if (existing is null) db.Users.Add(seed);
    }

    private static async Task UpsertProvider(WithinDbContext db, Provider seed)
    {
        var existing = await db.Providers.FindAsync(seed.Id);
        if (existing is null)
        {
            db.Providers.Add(seed);
            return;
        }

        existing.Name = seed.Name;
        existing.Slug = seed.Slug;
        existing.Bio = seed.Bio;
        existing.Lens = seed.Lens;
        existing.Location = seed.Location;
        existing.WebsiteUrl = seed.WebsiteUrl;
        existing.InstagramUrl = seed.InstagramUrl;
        existing.IsVerified = seed.IsVerified;
    }

    private static async Task UpsertEvent(WithinDbContext db, Event seed)
    {
        var existing = await db.Events.FindAsync(seed.Id);
        if (existing is null)
        {
            db.Events.Add(seed);
            return;
        }

        existing.Title = seed.Title;
        existing.Description = seed.Description;
        existing.Lens = seed.Lens;
        existing.LocationName = seed.LocationName;
        existing.IsOnline = seed.IsOnline;
        existing.StartUtc = seed.StartUtc;
        existing.EndUtc = seed.EndUtc;
        existing.PriceAmount = seed.PriceAmount;
        existing.Currency = seed.Currency;
        existing.Capacity = seed.Capacity;
        existing.SignupType = seed.SignupType;
        existing.ExternalBookingUrl = seed.ExternalBookingUrl;
        existing.ImageUrl = seed.ImageUrl;
        existing.Status = seed.Status;
        existing.Tags = seed.Tags;
    }

    private static async Task UpsertRegistration(WithinDbContext db, EventRegistration seed)
    {
        var existing = await db.EventRegistrations.FirstOrDefaultAsync(item => item.EventId == seed.EventId && item.UserId == seed.UserId);
        if (existing is null)
        {
            db.EventRegistrations.Add(seed);
            return;
        }

        existing.State = seed.State;
        existing.UpdatedUtc = seed.UpdatedUtc;
    }

    private static async Task UpsertDailyCheckIn(WithinDbContext db, DailyCheckIn seed)
    {
        var existing = await db.DailyCheckIns.FirstOrDefaultAsync(item => item.UserId == seed.UserId && item.CheckInDate == seed.CheckInDate);
        if (existing is null)
        {
            db.DailyCheckIns.Add(seed);
            return;
        }

        existing.MoodScore = seed.MoodScore;
        existing.EnergyScore = seed.EnergyScore;
        existing.StressScore = seed.StressScore;
        existing.ConnectionScore = seed.ConnectionScore;
        existing.MeaningScore = seed.MeaningScore;
        existing.DailyBalanceScore = seed.DailyBalanceScore;
        existing.Note = seed.Note;
    }
}
