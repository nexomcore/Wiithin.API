using Microsoft.EntityFrameworkCore;
using WithinAPI.Domain;

namespace WithinAPI.Data;

public static class HabitTemplateSeedData
{
    private static readonly (string Name, HabitCategory Category)[] Templates =
    [
        // Mind
        ("Meditate", HabitCategory.Mind),
        ("Practice gratitude", HabitCategory.Mind),
        ("Journal", HabitCategory.Mind),
        ("Do breathing exercise", HabitCategory.Mind),
        // Body
        ("Walk", HabitCategory.Body),
        ("Stretch", HabitCategory.Body),
        ("Exercise", HabitCategory.Body),
        ("Drink water", HabitCategory.Body),
        // Lifestyle
        ("Sleep on time", HabitCategory.Lifestyle),
        ("Eat mindfully", HabitCategory.Lifestyle),
        ("Reduce screen time before bed", HabitCategory.Lifestyle),
        // Social
        ("Connect with someone", HabitCategory.Social),
        ("Attend a wellbeing event", HabitCategory.Social),
        ("Check in with a circle", HabitCategory.Social),
        // Nature
        ("Spend time outdoors", HabitCategory.Nature),
        ("Get sunlight", HabitCategory.Nature)
    ];

    public static async Task EnsureSeededAsync(WithinDbContext db)
    {
        var sortOrder = 0;
        foreach (var (name, category) in Templates)
        {
            sortOrder++;
            if (await db.HabitTemplates.AnyAsync(item => item.Name == name))
            {
                continue;
            }

            db.HabitTemplates.Add(new HabitTemplate
            {
                Id = Guid.NewGuid(),
                Name = name,
                Category = category,
                SortOrder = sortOrder,
                IsActive = true
            });
        }

        await db.SaveChangesAsync();
    }
}
