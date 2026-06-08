using WithinAPI.Domain;

namespace WithinAPI.Data;

public static class CircleSeedData
{
    private static readonly (string Name, string Description, WithinLens Lens)[] PlatformCircles =
    [
        ("Meditation Circle", "A calm space for meditation questions, reflections, and event discovery.", WithinLens.Feel),
        ("Yoga Circle", "Discuss yoga practice, classes, beginner questions, and local sessions.", WithinLens.Move),
        ("Breathwork Circle", "Share breathwork experiences, preparation questions, and guided events.", WithinLens.Feel),
        ("Walking & Outdoors Circle", "Find mindful walks, outdoor wellbeing events, and gentle movement buddies.", WithinLens.Move),
        ("Retreat Seekers Circle", "Discover retreats, ask preparation questions, and share post-retreat reflections.", WithinLens.Seek),
        ("Mindfulness Circle", "Practice mindfulness with grounded discussion and practical support.", WithinLens.Feel),
        ("Mental Wellbeing Circle", "Supportive wellbeing discussion with safety-minded community guidelines.", WithinLens.Feel),
        ("Nutrition Circle", "Talk about nourishment, wellbeing habits, and local recommendations.", WithinLens.Move),
        ("Spirituality Circle", "Explore meaning, purpose, and spiritual wellbeing respectfully.", WithinLens.Seek),
        ("Beginner Friendly Circle", "A low-pressure space for first steps into wellbeing activities.", WithinLens.Feel),
        ("Perth Recommendations Circle", "Share Perth-based events, providers, venues, and local tips.", WithinLens.Seek),
        ("General Wellbeing Circle", "Open wellbeing discussion for topics that do not fit another Circle.", WithinLens.Feel)
    ];

    public static async Task EnsureSeededAsync(WithinDbContext db)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var item in PlatformCircles)
        {
            var slug = Slugify(item.Name);
            var circle = db.Circles.FirstOrDefault(circle => circle.Slug == slug);
            if (circle is null)
            {
                circle = new Circle
                {
                    Id = Guid.NewGuid(),
                    Name = item.Name,
                    Slug = slug,
                    Description = item.Description,
                    Lens = item.Lens,
                    Type = CircleType.Platform,
                    Visibility = CircleVisibility.Public,
                    Status = CircleStatus.Active,
                    CreatedAt = now
                };
                db.Circles.Add(circle);
            }
            else
            {
                circle.Type = CircleType.Platform;
                circle.Visibility = CircleVisibility.Public;
                circle.Status = CircleStatus.Active;
                circle.PrivacyType = CirclePrivacyType.Open;
                circle.AllowPseudonyms = true;
                circle.AllowHiddenProfiles = true;
                circle.AllowAnonymousPosts = false;
                circle.MemberListVisibility = MemberListVisibility.MembersOnly;
                circle.DefaultPostVisibility = CirclePostVisibility.MembersOnly;
                circle.DefaultEventRsvpVisibility = RsvpVisibility.FriendsOnly;
            }

            if (!db.CircleGuidelines.Any(guideline => guideline.CircleId == circle.Id))
            {
                db.CircleGuidelines.AddRange(
                    new CircleGuideline
                    {
                        Id = Guid.NewGuid(),
                        CircleId = circle.Id,
                        Title = "Keep it kind",
                        Body = "Share personal experiences and practical support without judgement.",
                        SortOrder = 1
                    },
                    new CircleGuideline
                    {
                        Id = Guid.NewGuid(),
                        CircleId = circle.Id,
                        Title = "No medical advice",
                        Body = "Avoid diagnosing or prescribing. Encourage professional help for medical or safety concerns.",
                        SortOrder = 2
                    },
                    new CircleGuideline
                    {
                        Id = Guid.NewGuid(),
                        CircleId = circle.Id,
                        Title = "Relevant events only",
                        Body = "Event shares should clearly fit the Circle and remain helpful rather than promotional.",
                        SortOrder = 3
                    });
            }
        }

        await db.SaveChangesAsync();
    }

    private static string Slugify(string value) =>
        value.ToLowerInvariant()
            .Replace("&", "and")
            .Replace(" circle", "")
            .Replace(" ", "-")
            .Replace("/", "-");
}
