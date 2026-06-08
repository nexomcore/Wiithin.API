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
        var mayaOwnerId = Guid.Parse("12121212-1212-1212-1212-121212121212");
        var ariOwnerId = Guid.Parse("13131313-1313-1313-1313-131313131313");
        var adminUserId = Guid.Parse("a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a0");

        var trackProviderId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var pranaProviderId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var mayaProviderId = Guid.Parse("14141414-1414-1414-1414-141414141414");
        var ariProviderId = Guid.Parse("15151515-1515-1515-1515-151515151515");

        await UpsertUser(db, User.Seed(demoUserId, "Demo User", "demo@within.local", WithinRole.User, now));
        await UpsertUser(db, User.Seed(trackOwnerId, "TheTrack Provider", "provider@thetrack.local", WithinRole.Provider, now));
        await UpsertUser(db, User.Seed(pranaOwnerId, "Prana Provider", "provider@prana.local", WithinRole.Provider, now));
        await UpsertUser(db, User.Seed(mayaOwnerId, "Maya Rivers", "maya@within.local", WithinRole.Provider, now));
        await UpsertUser(db, User.Seed(ariOwnerId, "Ari Sol", "ari@within.local", WithinRole.Provider, now));
        await UpsertAdmin(db, User.Seed(adminUserId, "Within Admin", "admin@within.local", WithinRole.Admin, now));

        var providers = new[]
        {
            new Provider
            {
                Id = trackProviderId,
                OwnerUserId = trackOwnerId,
                Name = "TheTrack Langley Park",
                Slug = "thetrack-langley-park",
                ProviderType = ProviderType.Business,
                Bio = "Run club, HYROX conditioning, pilates, and outdoor fitness in Perth. Built for people who want structure, sweat, and a social finish.",
                Lens = WithinLens.Move,
                Categories = ["fitness", "run club", "pilates", "strength"],
                Location = "Langley Park, Perth",
                City = "Perth",
                State = "WA",
                Country = "Australia",
                WebsiteUrl = "https://example.com/thetrack",
                InstagramUrl = "https://instagram.com/thetrack",
                BusinessType = "Outdoor fitness studio",
                Facilities = ["toilets", "drinking_water", "parking"],
                OnlineAvailable = false,
                InPersonAvailable = true,
                IsVerified = true,
                VerificationStatus = ProviderVerificationStatus.Verified,
                CreatedUtc = now,
                UpdatedUtc = now
            },
            new Provider
            {
                Id = pranaProviderId,
                OwnerUserId = pranaOwnerId,
                Name = "Prana Wellness",
                Slug = "prana-wellness",
                ProviderType = ProviderType.Business,
                Bio = "Meditation, spiritual healing, breathwork, retreats, and reflection circles for grounded inner work.",
                Lens = WithinLens.Seek,
                Categories = ["meditation", "breathwork", "retreats", "spiritual practice"],
                Location = "North Perth",
                City = "Perth",
                State = "WA",
                Country = "Australia",
                WebsiteUrl = "https://example.com/prana",
                InstagramUrl = "https://instagram.com/pranawellness",
                BusinessType = "Wellness centre",
                Facilities = ["toilets", "quiet_room"],
                OnlineAvailable = true,
                InPersonAvailable = true,
                IsVerified = true,
                VerificationStatus = ProviderVerificationStatus.Verified,
                CreatedUtc = now,
                UpdatedUtc = now
            },
            new Provider
            {
                Id = mayaProviderId,
                OwnerUserId = mayaOwnerId,
                Name = "Maya Rivers Fitness",
                Slug = "maya-rivers-fitness",
                ProviderType = ProviderType.Individual,
                Bio = "Individual strength coach and hiking guide helping small groups build confidence outdoors and under load.",
                Lens = WithinLens.Move,
                Categories = ["strength", "hiking", "fitness coaching"],
                Location = "Scarborough",
                City = "Perth",
                State = "WA",
                Country = "Australia",
                WebsiteUrl = "https://example.com/maya-rivers",
                InstagramUrl = "https://instagram.com/mayariversfit",
                PractitionerTitle = "Fitness Trainer",
                YearsExperience = 7,
                Qualifications = "Strength coaching and outdoor leadership certifications.",
                ServicesOffered = ["Small-group strength coaching", "Guided hikes"],
                Languages = ["English"],
                OnlineAvailable = false,
                InPersonAvailable = true,
                IsVerified = true,
                VerificationStatus = ProviderVerificationStatus.Verified,
                CreatedUtc = now,
                UpdatedUtc = now
            },
            new Provider
            {
                Id = ariProviderId,
                OwnerUserId = ariOwnerId,
                Name = "Ari Sol Healing",
                Slug = "ari-sol-healing",
                ProviderType = ProviderType.Individual,
                Bio = "Spiritual healer and breathwork facilitator offering quiet, intimate sessions for release, reflection, and renewal.",
                Lens = WithinLens.Seek,
                Categories = ["breathwork", "spiritual guidance", "sound healing"],
                Location = "Fremantle",
                City = "Fremantle",
                State = "WA",
                Country = "Australia",
                WebsiteUrl = "https://example.com/ari-sol",
                InstagramUrl = "https://instagram.com/arisolhealing",
                PractitionerTitle = "Breathwork Facilitator",
                YearsExperience = 5,
                Qualifications = "Breathwork facilitation and sound meditation training.",
                ServicesOffered = ["1:1 breathwork", "Spiritual guidance", "Sound healing"],
                Languages = ["English"],
                OnlineAvailable = true,
                InPersonAvailable = true,
                IsVerified = true,
                VerificationStatus = ProviderVerificationStatus.Verified,
                CreatedUtc = now,
                UpdatedUtc = now
            }
        };

        foreach (var provider in providers)
        {
            await UpsertProvider(db, provider);
        }

        var providerServices = new[]
        {
            Service(Guid.Parse("40404040-4040-4040-4040-404040404040"), trackProviderId, "Group Fitness Class", "Outdoor functional fitness for mixed experience levels.", WithinLens.Move, "fitness", ProviderServiceDeliveryMode.InPerson, 60, 28, ProviderPriceType.Fixed, "Langley Park"),
            Service(Guid.Parse("41414141-4141-4141-4141-414141414141"), trackProviderId, "Run Club", "Beginner-friendly social running with pace groups.", WithinLens.Move, "run club", ProviderServiceDeliveryMode.InPerson, 75, 0, ProviderPriceType.Free, "Langley Park"),
            Service(Guid.Parse("42424242-4242-4242-4242-424242424242"), pranaProviderId, "Meditation Coaching", "Guided meditation and reflection support.", WithinLens.Seek, "meditation", ProviderServiceDeliveryMode.Hybrid, 60, null, ProviderPriceType.ContactProvider, "North Perth"),
            Service(Guid.Parse("43434343-4343-4343-4343-434343434343"), pranaProviderId, "Breathwork Workshop", "Group breathwork sessions for reset and reflection.", WithinLens.Seek, "breathwork", ProviderServiceDeliveryMode.Hybrid, 90, 16, ProviderPriceType.FromPrice, "North Perth / Online"),
            Service(Guid.Parse("44444444-5555-5555-5555-555555555555"), mayaProviderId, "1:1 Fitness Coaching", "Personal strength coaching with clear progression.", WithinLens.Move, "fitness coaching", ProviderServiceDeliveryMode.InPerson, 60, null, ProviderPriceType.ContactProvider, "Scarborough"),
            Service(Guid.Parse("45454545-4545-4545-4545-454545454545"), mayaProviderId, "Guided Hike", "Small-group mindful hikes around Perth trails.", WithinLens.Move, "hiking", ProviderServiceDeliveryMode.InPerson, 120, 22, ProviderPriceType.FromPrice, "Perth"),
            Service(Guid.Parse("46464646-4646-4646-4646-464646464646"), ariProviderId, "1:1 Breathwork", "Private breathwork support for grounding and release.", WithinLens.Seek, "breathwork", ProviderServiceDeliveryMode.Hybrid, 60, null, ProviderPriceType.ContactProvider, "Fremantle / Online"),
            Service(Guid.Parse("47474747-4747-4747-4747-474747474747"), ariProviderId, "Sound Healing", "Small-group sound meditation and reflection.", WithinLens.Seek, "sound healing", ProviderServiceDeliveryMode.InPerson, 75, 35, ProviderPriceType.FromPrice, "Fremantle")
        };

        foreach (var service in providerServices)
        {
            await UpsertProviderService(db, service);
        }

        var saturdayRunId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var meditationId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        var hyroxId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var pilatesId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var hikeId = Guid.Parse("16161616-1616-1616-1616-161616161616");
        var hiitId = Guid.Parse("17171717-1717-1717-1717-171717171717");
        var breathworkId = Guid.Parse("18181818-1818-1818-1818-181818181818");
        var moonId = Guid.Parse("19191919-1919-1919-1919-191919191919");
        var healingId = Guid.Parse("20202020-2020-2020-2020-202020202020");

        var events = new[]
        {
            new Event
            {
                Id = saturdayRunId,
                ProviderId = trackProviderId,
                Title = "Saturday Run Club",
                Description = "Beginner-friendly social run along the river followed by coffee. Expect pace groups, warm-up drills, and a relaxed community finish.",
                Lens = WithinLens.Move,
                LocationName = "Langley Park",
                IsOnline = false,
                StartUtc = now.AddDays(3).AddHours(23),
                EndUtc = now.AddDays(4).AddHours(1),
                PriceAmount = 0,
                Currency = "AUD",
                Capacity = 32,
                SignupType = SignupType.Internal,
                ImageUrl = "https://images.unsplash.com/photo-1552674605-db6ffd4facb5?auto=format&fit=crop&w=1200&q=80",
                Status = EventStatus.Published,
                Tags = ["run", "free", "weekend", "beginner-friendly"],
                CreatedUtc = now
            },
            new Event
            {
                Id = hyroxId,
                ProviderId = trackProviderId,
                Title = "HYROX Conditioning Session",
                Description = "A focused conditioning block using sled pushes, carries, rowing, and functional intervals. Scaled options available for first-timers.",
                Lens = WithinLens.Move,
                LocationName = "Langley Park Training Zone",
                IsOnline = false,
                StartUtc = now.AddDays(4).AddHours(9),
                EndUtc = now.AddDays(4).AddHours(10),
                PriceAmount = 28,
                Currency = "AUD",
                Capacity = 24,
                SignupType = SignupType.Internal,
                ImageUrl = "https://images.unsplash.com/photo-1518611012118-696072aa579a?auto=format&fit=crop&w=1200&q=80",
                Status = EventStatus.Published,
                Tags = ["hyrox", "strength", "conditioning", "paid"],
                CreatedUtc = now
            },
            new Event
            {
                Id = pilatesId,
                ProviderId = trackProviderId,
                Title = "Outdoor Pilates Flow",
                Description = "A low-impact mat pilates session outdoors with mobility, core activation, breath, and a slow cooldown.",
                Lens = WithinLens.Move,
                LocationName = "South Perth Foreshore",
                IsOnline = false,
                StartUtc = now.AddDays(5).AddHours(0),
                EndUtc = now.AddDays(5).AddHours(1),
                PriceAmount = 18,
                Currency = "AUD",
                Capacity = 20,
                SignupType = SignupType.External,
                ExternalBookingUrl = "https://example.com/book/outdoor-pilates-flow",
                ImageUrl = "https://images.unsplash.com/photo-1599901860904-17e6ed7083a0?auto=format&fit=crop&w=1200&q=80",
                Status = EventStatus.Published,
                Tags = ["pilates", "mobility", "outdoor", "paid"],
                CreatedUtc = now
            },
            new Event
            {
                Id = hikeId,
                ProviderId = mayaProviderId,
                Title = "Sunrise Hike",
                Description = "A small-group sunrise hike with gentle strength stops, mindful pacing, and a simple breakfast lookout pause.",
                Lens = WithinLens.Move,
                LocationName = "Bold Park",
                IsOnline = false,
                StartUtc = now.AddDays(6).AddHours(22),
                EndUtc = now.AddDays(7).AddMinutes(30),
                PriceAmount = 22,
                Currency = "AUD",
                Capacity = 16,
                SignupType = SignupType.Internal,
                ImageUrl = "https://images.unsplash.com/photo-1551632811-561732d1e306?auto=format&fit=crop&w=1200&q=80",
                Status = EventStatus.Published,
                Tags = ["hike", "sunrise", "small-group", "paid"],
                CreatedUtc = now
            },
            new Event
            {
                Id = hiitId,
                ProviderId = mayaProviderId,
                Title = "HIIT Strength",
                Description = "Compact strength and cardio intervals for people who want a simple, high-energy session before work.",
                Lens = WithinLens.Move,
                LocationName = "Northbridge",
                IsOnline = false,
                StartUtc = now.AddDays(8).AddHours(23),
                EndUtc = now.AddDays(9),
                PriceAmount = 0,
                Currency = "AUD",
                Capacity = 30,
                SignupType = SignupType.Internal,
                ImageUrl = "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?auto=format&fit=crop&w=1200&q=80",
                Status = EventStatus.Published,
                Tags = ["hiit", "strength", "free", "morning"],
                CreatedUtc = now
            },
            new Event
            {
                Id = meditationId,
                ProviderId = pranaProviderId,
                Title = "Guided Meditation Circle",
                Description = "A calm circle for breath awareness, grounding, and reflection. Arrive as you are; cushions and tea are provided.",
                Lens = WithinLens.Seek,
                LocationName = "North Perth Wellness Studio",
                IsOnline = false,
                StartUtc = now.AddDays(4).AddHours(1),
                EndUtc = now.AddDays(4).AddHours(2),
                PriceAmount = 0,
                Currency = "AUD",
                Capacity = 18,
                SignupType = SignupType.Internal,
                ImageUrl = "https://images.unsplash.com/photo-1506126613408-eca07ce68773?auto=format&fit=crop&w=1200&q=80",
                Status = EventStatus.Published,
                Tags = ["meditation", "free", "weekend", "circle"],
                CreatedUtc = now
            },
            new Event
            {
                Id = breathworkId,
                ProviderId = pranaProviderId,
                Title = "Breathwork Reset",
                Description = "A guided breathwork session for nervous-system downshift, release, and spacious reflection.",
                Lens = WithinLens.Seek,
                LocationName = "Online",
                IsOnline = true,
                StartUtc = now.AddDays(5).AddHours(11),
                EndUtc = now.AddDays(5).AddHours(12),
                PriceAmount = 16,
                Currency = "AUD",
                Capacity = 40,
                SignupType = SignupType.External,
                ExternalBookingUrl = "https://example.com/book/breathwork-reset",
                ImageUrl = "https://images.unsplash.com/photo-1545389336-cf090694435e?auto=format&fit=crop&w=1200&q=80",
                Status = EventStatus.Published,
                Tags = ["breathwork", "online", "reset", "paid"],
                CreatedUtc = now
            },
            new Event
            {
                Id = moonId,
                ProviderId = ariProviderId,
                Title = "Full Moon Reflection Gathering",
                Description = "A candlelit reflection circle with journaling, intention setting, and a closing sound meditation.",
                Lens = WithinLens.Seek,
                LocationName = "Fremantle Studio",
                IsOnline = false,
                StartUtc = now.AddDays(10).AddHours(11),
                EndUtc = now.AddDays(10).AddHours(13),
                PriceAmount = 35,
                Currency = "AUD",
                Capacity = 22,
                SignupType = SignupType.Internal,
                ImageUrl = "https://images.unsplash.com/photo-1532767153582-b1a0e5145009?auto=format&fit=crop&w=1200&q=80",
                Status = EventStatus.Published,
                Tags = ["full-moon", "reflection", "journaling", "paid"],
                CreatedUtc = now
            },
            new Event
            {
                Id = healingId,
                ProviderId = ariProviderId,
                Title = "Spiritual Healing Workshop",
                Description = "A gentle workshop for energy clearing, guided visualisation, and practical grounding rituals to take home.",
                Lens = WithinLens.Seek,
                LocationName = "Fremantle Studio",
                IsOnline = false,
                StartUtc = now.AddDays(12).AddHours(2),
                EndUtc = now.AddDays(12).AddHours(4),
                PriceAmount = 48,
                Currency = "AUD",
                Capacity = 14,
                SignupType = SignupType.External,
                ExternalBookingUrl = "https://example.com/book/spiritual-healing-workshop",
                ImageUrl = "https://images.unsplash.com/photo-1519681393784-d120267933ba?auto=format&fit=crop&w=1200&q=80",
                Status = EventStatus.Published,
                Tags = ["healing", "workshop", "grounding", "paid"],
                CreatedUtc = now
            }
        };

        foreach (var evt in events)
        {
            await UpsertEvent(db, evt);
        }

        var moveCommunityId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        var seekCommunityId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var hikeCommunityId = Guid.Parse("21212121-2121-2121-2121-212121212121");
        var healingCommunityId = Guid.Parse("22222222-3333-4444-5555-666666666666");

        var communities = new[]
        {
            new Community { Id = moveCommunityId, ProviderId = trackProviderId, Name = "TheTrack Community", Description = "Run club, HYROX, pilates, and outdoor training updates.", Lens = WithinLens.Move, Location = "Perth", CreatedUtc = now },
            new Community { Id = seekCommunityId, ProviderId = pranaProviderId, Name = "Prana Circle", Description = "Meditation, reflection, and spiritual growth discussions.", Lens = WithinLens.Seek, Location = "Perth", CreatedUtc = now },
            new Community { Id = hikeCommunityId, ProviderId = mayaProviderId, Name = "Maya's Move Crew", Description = "Small-group strength, hikes, and practical training accountability.", Lens = WithinLens.Move, Location = "Coastal Perth", CreatedUtc = now },
            new Community { Id = healingCommunityId, ProviderId = ariProviderId, Name = "Ari's Reflection Room", Description = "A slower space for ritual prompts, moon reflections, and grounding practices.", Lens = WithinLens.Seek, Location = "Fremantle", CreatedUtc = now }
        };

        foreach (var community in communities)
        {
            await UpsertCommunity(db, community);
            await UpsertCommunityMember(db, new CommunityMember { Id = Guid.NewGuid(), CommunityId = community.Id, UserId = demoUserId, JoinedUtc = now.AddDays(1) });
        }

        await UpsertRegistration(db, new EventRegistration { Id = Guid.Parse("34343434-3434-3434-3434-343434343434"), EventId = pilatesId, UserId = demoUserId, State = EventJoinState.Going, CreatedUtc = now, UpdatedUtc = now });
        await UpsertRegistration(db, new EventRegistration { Id = Guid.Parse("35353535-3535-3535-3535-353535353535"), EventId = hikeId, UserId = demoUserId, State = EventJoinState.Going, CreatedUtc = now, UpdatedUtc = now });
        await UpsertRegistration(db, new EventRegistration { Id = Guid.Parse("23232323-2323-2323-2323-232323232323"), EventId = meditationId, UserId = demoUserId, State = EventJoinState.Interested, CreatedUtc = now, UpdatedUtc = now });

        await UpsertSavedEvent(db, new SavedEvent { Id = Guid.Parse("24242424-2424-2424-2424-242424242424"), EventId = breathworkId, UserId = demoUserId, CreatedUtc = now });

        await UpsertPost(db, new Post { Id = Guid.Parse("25252525-2525-2525-2525-252525252525"), CommunityId = moveCommunityId, EventId = saturdayRunId, AuthorUserId = trackOwnerId, Body = "Saturday Run Club is mapped and ready. We will split into relaxed 4km and steady 7km groups before coffee.", CreatedUtc = now.AddHours(8) });
        await UpsertPost(db, new Post { Id = Guid.Parse("26262626-2626-2626-2626-262626262626"), CommunityId = seekCommunityId, EventId = meditationId, AuthorUserId = pranaOwnerId, Body = "For this week's circle, bring a small notebook. The theme is breath, attention, and the places where we soften.", CreatedUtc = now.AddHours(9) });
        await UpsertPost(db, new Post { Id = Guid.Parse("27272727-2727-2727-2727-272727272727"), CommunityId = hikeCommunityId, EventId = hikeId, AuthorUserId = mayaOwnerId, Body = "Sunrise Hike note: wear layers and bring water. We will pause halfway for a short grounding drill.", CreatedUtc = now.AddHours(10) });
        await UpsertPost(db, new Post { Id = Guid.Parse("28282828-2828-2828-2828-282828282828"), CommunityId = healingCommunityId, EventId = moonId, AuthorUserId = ariOwnerId, Body = "Full Moon gathering spots are open. The reflection prompt this month is: what am I ready to stop carrying?", CreatedUtc = now.AddHours(11) });

        await UpsertComment(db, new Comment { Id = Guid.Parse("29292929-2929-2929-2929-292929292929"), EventId = saturdayRunId, AuthorUserId = demoUserId, Body = "Is this okay for someone getting back into running?", CreatedUtc = now.AddHours(12) });
        await UpsertComment(db, new Comment { Id = Guid.Parse("30303030-3030-3030-3030-303030303030"), EventId = saturdayRunId, AuthorUserId = trackOwnerId, Body = "Yes. Start with the relaxed group and we will keep the pace conversational.", CreatedUtc = now.AddHours(13) });
        await UpsertComment(db, new Comment { Id = Guid.Parse("31313131-3131-3131-3131-313131313131"), EventId = meditationId, AuthorUserId = demoUserId, Body = "Do I need meditation experience?", CreatedUtc = now.AddHours(14) });
        await UpsertComment(db, new Comment { Id = Guid.Parse("32323232-3232-3232-3232-323232323232"), PostId = Guid.Parse("26262626-2626-2626-2626-262626262626"), AuthorUserId = demoUserId, Body = "Love this prompt. I will bring my journal.", CreatedUtc = now.AddHours(15) });

        await UpsertReaction(db, new Reaction { Id = Guid.Parse("33333333-4444-5555-6666-777777777777"), PostId = Guid.Parse("25252525-2525-2525-2525-252525252525"), UserId = demoUserId, Kind = "like", CreatedUtc = now.AddHours(16) });

        await UpsertDailyCheckIn(db, new DailyCheckIn
        {
            Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
            UserId = demoUserId,
            CheckInDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Mood = CheckInMood.Good,
            Energy = CheckInEnergy.Balanced,
            SleepQuality = CheckInSleepQuality.Okay,
            Intention = DailyIntention.StayCalm,
            Tags = ["energised", "grounded"],
            SuggestedActionKey = "small_action",
            DailyBalanceScore = 75,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        await db.SaveChangesAsync();
    }

    private static async Task UpsertUser(WithinDbContext db, User seed)
    {
        var existing = await db.Users.FindAsync(seed.Id);
        if (existing is null) db.Users.Add(seed);
    }

    private static async Task UpsertAdmin(WithinDbContext db, User seed)
    {
        var existing = await db.Users.FindAsync(seed.Id);
        if (existing is null)
        {
            db.Users.Add(seed);
            return;
        }

        existing.Role = seed.Role;
        existing.DisplayName = seed.DisplayName;
        existing.Email = seed.Email;
        existing.PasswordHash = seed.PasswordHash;
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
        existing.ProviderType = seed.ProviderType;
        existing.Categories = seed.Categories;
        existing.City = seed.City;
        existing.State = seed.State;
        existing.Country = seed.Country;
        existing.VerificationStatus = seed.VerificationStatus;
        existing.IsActive = seed.IsActive;
        existing.BusinessType = seed.BusinessType;
        existing.PractitionerTitle = seed.PractitionerTitle;
        existing.YearsExperience = seed.YearsExperience;
        existing.Qualifications = seed.Qualifications;
        existing.ServicesOffered = seed.ServicesOffered;
        existing.Languages = seed.Languages;
        existing.OnlineAvailable = seed.OnlineAvailable;
        existing.InPersonAvailable = seed.InPersonAvailable;
        existing.Facilities = seed.Facilities;
        existing.UpdatedUtc = seed.UpdatedUtc;
    }

    private static ProviderService Service(Guid id, Guid providerId, string name, string description, WithinLens lens, string category, ProviderServiceDeliveryMode deliveryMode, int? duration, decimal? price, ProviderPriceType priceType, string location) => new()
    {
        Id = id,
        ProviderId = providerId,
        Name = name,
        Description = description,
        Lens = lens,
        Category = category,
        DeliveryMode = deliveryMode,
        DurationMinutes = duration,
        PriceAmount = price,
        PriceType = priceType,
        Location = location,
        IsActive = true,
        CreatedUtc = new DateTimeOffset(2026, 5, 27, 0, 0, 0, TimeSpan.Zero),
        UpdatedUtc = new DateTimeOffset(2026, 5, 27, 0, 0, 0, TimeSpan.Zero)
    };

    private static async Task UpsertProviderService(WithinDbContext db, ProviderService seed)
    {
        var existing = await db.ProviderServices.FindAsync(seed.Id);
        if (existing is null)
        {
            db.ProviderServices.Add(seed);
            return;
        }

        existing.ProviderId = seed.ProviderId;
        existing.Name = seed.Name;
        existing.Description = seed.Description;
        existing.Lens = seed.Lens;
        existing.Category = seed.Category;
        existing.DurationMinutes = seed.DurationMinutes;
        existing.PriceAmount = seed.PriceAmount;
        existing.PriceType = seed.PriceType;
        existing.DeliveryMode = seed.DeliveryMode;
        existing.Location = seed.Location;
        existing.IsActive = seed.IsActive;
        existing.UpdatedUtc = seed.UpdatedUtc;
    }

    private static async Task UpsertEvent(WithinDbContext db, Event seed)
    {
        var existing = await db.Events.FindAsync(seed.Id);
        if (existing is null)
        {
            db.Events.Add(seed);
            return;
        }

        existing.ProviderId = seed.ProviderId;
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

    private static async Task UpsertCommunity(WithinDbContext db, Community seed)
    {
        var existing = await db.Communities.FindAsync(seed.Id);
        if (existing is null)
        {
            db.Communities.Add(seed);
            return;
        }

        existing.ProviderId = seed.ProviderId;
        existing.Name = seed.Name;
        existing.Description = seed.Description;
        existing.Lens = seed.Lens;
        existing.Location = seed.Location;
    }

    private static async Task UpsertCommunityMember(WithinDbContext db, CommunityMember seed)
    {
        if (!await db.CommunityMembers.AnyAsync(item => item.CommunityId == seed.CommunityId && item.UserId == seed.UserId))
        {
            db.CommunityMembers.Add(seed);
        }
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

    private static async Task UpsertSavedEvent(WithinDbContext db, SavedEvent seed)
    {
        if (!await db.SavedEvents.AnyAsync(item => item.EventId == seed.EventId && item.UserId == seed.UserId))
        {
            db.SavedEvents.Add(seed);
        }
    }

    private static async Task UpsertPost(WithinDbContext db, Post seed)
    {
        var existing = await db.Posts.FindAsync(seed.Id);
        if (existing is null)
        {
            db.Posts.Add(seed);
            return;
        }

        existing.CommunityId = seed.CommunityId;
        existing.EventId = seed.EventId;
        existing.AuthorUserId = seed.AuthorUserId;
        existing.Body = seed.Body;
        existing.IsHidden = seed.IsHidden;
        existing.CreatedUtc = seed.CreatedUtc;
    }

    private static async Task UpsertComment(WithinDbContext db, Comment seed)
    {
        var existing = await db.Comments.FindAsync(seed.Id);
        if (existing is null)
        {
            db.Comments.Add(seed);
            return;
        }

        existing.PostId = seed.PostId;
        existing.EventId = seed.EventId;
        existing.AuthorUserId = seed.AuthorUserId;
        existing.Body = seed.Body;
        existing.IsHidden = seed.IsHidden;
        existing.CreatedUtc = seed.CreatedUtc;
    }

    private static async Task UpsertReaction(WithinDbContext db, Reaction seed)
    {
        if (!await db.Reactions.AnyAsync(item => item.PostId == seed.PostId && item.UserId == seed.UserId && item.Kind == seed.Kind))
        {
            db.Reactions.Add(seed);
        }
    }

    private static async Task UpsertDailyCheckIn(WithinDbContext db, DailyCheckIn seed)
    {
        var existing = await db.DailyCheckIns.FirstOrDefaultAsync(item => item.UserId == seed.UserId && item.CheckInDate == seed.CheckInDate);
        if (existing is null)
        {
            existing = await db.DailyCheckIns.FindAsync(seed.Id);
            if (existing is null)
            {
                db.DailyCheckIns.Add(seed);
                return;
            }

            existing.UserId = seed.UserId;
            existing.CheckInDate = seed.CheckInDate;
        }

        existing.Mood = seed.Mood;
        existing.Energy = seed.Energy;
        existing.SleepQuality = seed.SleepQuality;
        existing.SleepHours = seed.SleepHours;
        existing.Intention = seed.Intention;
        existing.Tags = seed.Tags;
        existing.SuggestedActionKey = seed.SuggestedActionKey;
        existing.DailyBalanceScore = seed.DailyBalanceScore;
        existing.Note = seed.Note;
        existing.CreatedAtUtc = seed.CreatedAtUtc;
        existing.UpdatedAtUtc = seed.UpdatedAtUtc;
    }
}
