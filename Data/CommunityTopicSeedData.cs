using Microsoft.EntityFrameworkCore;
using WithinAPI.Domain;

namespace WithinAPI.Data;

public static class CommunityTopicSeedData
{
    private static readonly string[] TopicNames =
    [
        "Meditation",
        "Yoga",
        "Fitness",
        "Mindfulness",
        "Breathwork",
        "Retreats",
        "Mental Wellbeing",
        "Nutrition",
        "Walking & Outdoors",
        "Spirituality",
        "Beginner Friendly",
        "Perth Recommendations",
        "General Wellbeing"
    ];

    public static async Task EnsureSeededAsync(WithinDbContext db)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var name in TopicNames)
        {
            var slug = Slugify(name);
            if (await db.CommunityTopics.AnyAsync(item => item.Slug == slug))
            {
                continue;
            }

            db.CommunityTopics.Add(new CommunityTopic
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = slug,
                IsActive = true,
                CreatedAt = now
            });
        }

        await db.SaveChangesAsync();
    }

    private static string Slugify(string value)
    {
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();
        return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}
