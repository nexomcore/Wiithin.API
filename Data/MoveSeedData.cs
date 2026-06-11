using Microsoft.EntityFrameworkCore;
using WithinAPI.Domain;
using WithinAPI.Services;

namespace WithinAPI.Data;

/// <summary>
/// Idempotent starter content for the Move pillar: system workout/diet templates and a
/// public challenge catalogue. Keyed on Title so repeated startups never duplicate rows.
/// </summary>
public static class MoveSeedData
{
    // System-owned content has no human author; this fixed id avoids a FK to Users.
    public static readonly Guid SystemUserId = Guid.Parse("b0b0b0b0-0000-0000-0000-000000000001");

    public static async Task SeedAsync(WithinDbContext db)
    {
        await SeedWorkoutTemplatesAsync(db);
        await SeedDietTemplatesAsync(db);
        await SeedChallengeTemplatesAsync(db);
    }

    // ── Workout templates ──
    private static async Task SeedWorkoutTemplatesAsync(WithinDbContext db)
    {
        var existing = await db.WorkoutPlans.Where(p => p.IsTemplate).Select(p => p.Title).ToListAsync();
        var now = DateTimeOffset.UtcNow;

        void Add(string title, string desc, string goal, string type, string level, int weeks, int days, (string title, string[] exercises)[] dayList)
        {
            if (existing.Contains(title)) return;
            var plan = new WorkoutPlan
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = SystemUserId,
                AssignedToUserId = null,
                Title = title,
                Description = desc,
                Goal = goal,
                WorkoutType = type,
                ExperienceLevel = level,
                DurationWeeks = weeks,
                DaysPerWeek = days,
                IsTemplate = true,
                SourceType = MovePlanSourceType.SystemTemplate,
                Status = MovePlanStatus.Active,
                CreatedUtc = now,
                UpdatedUtc = now
            };
            db.WorkoutPlans.Add(plan);
            for (var d = 0; d < dayList.Length; d++)
            {
                var day = new WorkoutDay
                {
                    Id = Guid.NewGuid(),
                    WorkoutPlanId = plan.Id,
                    DayNumber = d + 1,
                    Title = dayList[d].title,
                    SortOrder = d
                };
                db.WorkoutDays.Add(day);
                for (var e = 0; e < dayList[d].exercises.Length; e++)
                {
                    db.WorkoutExercises.Add(new WorkoutExercise
                    {
                        Id = Guid.NewGuid(),
                        WorkoutDayId = day.Id,
                        ExerciseName = dayList[d].exercises[e],
                        Sets = 3,
                        Reps = "8-12",
                        RestSeconds = 90,
                        SortOrder = e
                    });
                }
            }
        }

        Add("Beginner 3-Day Gym Full Body", "A gentle full-body introduction to the gym, three sessions a week.",
            "general_health,lose_fat,body_recomposition", "gym", "beginner", 8, 3, new[]
            {
                ("Day A — Full Body", new[] { "Goblet Squat", "Chest Press Machine", "Lat Pulldown", "Seated Row", "Plank" }),
                ("Day B — Full Body", new[] { "Leg Press", "Incline Dumbbell Press", "Assisted Pull-up", "Dumbbell Shoulder Press", "Dead Bug" }),
                ("Day C — Full Body", new[] { "Romanian Deadlift", "Push-up", "Cable Row", "Lateral Raise", "Glute Bridge" })
            });

        Add("Beginner 4-Day Gym Upper Lower", "Upper/lower split building strength and muscle over four sessions.",
            "build_muscle,strength,body_recomposition", "gym", "beginner", 8, 4, new[]
            {
                ("Upper A", new[] { "Bench Press", "Bent-over Row", "Overhead Press", "Lat Pulldown", "Bicep Curl" }),
                ("Lower A", new[] { "Back Squat", "Romanian Deadlift", "Leg Press", "Calf Raise", "Plank" }),
                ("Upper B", new[] { "Incline Dumbbell Press", "Seated Row", "Lateral Raise", "Tricep Pushdown", "Face Pull" }),
                ("Lower B", new[] { "Deadlift", "Walking Lunge", "Leg Curl", "Hip Thrust", "Hanging Knee Raise" })
            });

        Add("Intermediate 5-Day Push Pull Legs", "Classic push/pull/legs rotation for steady muscle and strength gains.",
            "build_muscle,strength", "gym", "intermediate", 10, 5, new[]
            {
                ("Push", new[] { "Barbell Bench Press", "Overhead Press", "Incline Dumbbell Press", "Lateral Raise", "Tricep Pushdown" }),
                ("Pull", new[] { "Deadlift", "Pull-up", "Barbell Row", "Face Pull", "Barbell Curl" }),
                ("Legs", new[] { "Back Squat", "Romanian Deadlift", "Leg Press", "Leg Curl", "Standing Calf Raise" }),
                ("Push (Hypertrophy)", new[] { "Incline Bench Press", "Dumbbell Shoulder Press", "Cable Fly", "Lateral Raise", "Overhead Tricep Extension" }),
                ("Pull (Hypertrophy)", new[] { "Lat Pulldown", "Seated Cable Row", "Reverse Fly", "Hammer Curl", "Shrugs" })
            });

        Add("Beginner Home Bodyweight Plan", "No equipment needed — three calm home sessions a week.",
            "general_health,lose_fat", "home", "beginner", 6, 3, new[]
            {
                ("Full Body A", new[] { "Bodyweight Squat", "Incline Push-up", "Glute Bridge", "Superman Hold", "Plank" }),
                ("Full Body B", new[] { "Reverse Lunge", "Push-up", "Doorway Row", "Side Plank", "Bird Dog" }),
                ("Full Body C", new[] { "Wall Sit", "Pike Push-up", "Hip Hinge", "Dead Bug", "Mountain Climbers" })
            });

        Add("Beginner Calisthenics Plan", "Build relative strength with progressive bodyweight skills.",
            "strength,general_health", "calisthenics", "beginner", 8, 3, new[]
            {
                ("Push Focus", new[] { "Incline Push-up", "Pike Push-up", "Dip (assisted)", "Plank", "Hollow Hold" }),
                ("Pull Focus", new[] { "Australian Row", "Negative Pull-up", "Scapular Pull", "Superman", "Side Plank" }),
                ("Legs & Core", new[] { "Bodyweight Squat", "Split Squat", "Glute Bridge", "Calf Raise", "Leg Raise" })
            });

        Add("Fat Loss Circuit Plan", "Higher-tempo mixed circuits to support fat loss with minimal rest.",
            "lose_fat", "mixed", "beginner", 6, 4, new[]
            {
                ("Circuit A", new[] { "Goblet Squat", "Push-up", "Kettlebell Swing", "Mountain Climbers", "Plank" }),
                ("Circuit B", new[] { "Reverse Lunge", "Dumbbell Row", "Jumping Jacks", "Bicycle Crunch", "Wall Sit" }),
                ("Circuit C", new[] { "Step-up", "Incline Press", "High Knees", "Russian Twist", "Side Plank" }),
                ("Circuit D", new[] { "Sumo Deadlift", "Push Press", "Burpee (step-back)", "Dead Bug", "Hollow Hold" })
            });

        Add("Mobility and Flexibility Plan", "Gentle daily-friendly mobility and flexibility flows.",
            "mobility", "yoga", "beginner", 4, 3, new[]
            {
                ("Lower Body Mobility", new[] { "Cat-Cow", "World's Greatest Stretch", "90/90 Hip Switch", "Hamstring Flow", "Child's Pose" }),
                ("Upper Body Mobility", new[] { "Thoracic Rotation", "Wall Angel", "Doorway Chest Stretch", "Neck Release", "Downward Dog" }),
                ("Full Body Flow", new[] { "Sun Salutation", "Low Lunge Flow", "Pigeon Pose", "Seated Forward Fold", "Supine Twist" })
            });
    }

    // ── Diet templates ──
    private static async Task SeedDietTemplatesAsync(WithinDbContext db)
    {
        var existing = await db.DietPlans.Where(p => p.IsTemplate).Select(p => p.Title).ToListAsync();
        var now = DateTimeOffset.UtcNow;

        void Add(string title, string desc, string goal, string preference, int calories, (string name, string time, (string food, string qty)[] items)[] meals)
        {
            if (existing.Contains(title)) return;
            var macros = MoveCalculators.Macros(calories, goal);
            var plan = new DietPlan
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = SystemUserId,
                AssignedToUserId = null,
                Title = title,
                Description = desc,
                Goal = goal,
                Calories = calories,
                ProteinGrams = macros.ProteinGrams,
                CarbsGrams = macros.CarbsGrams,
                FatGrams = macros.FatGrams,
                DietPreference = preference,
                DurationWeeks = 4,
                IsTemplate = true,
                SourceType = MovePlanSourceType.SystemTemplate,
                Status = MovePlanStatus.Active,
                CreatedUtc = now,
                UpdatedUtc = now
            };
            db.DietPlans.Add(plan);
            var perMeal = meals.Length == 0 ? 0 : calories / meals.Length;
            for (var m = 0; m < meals.Length; m++)
            {
                var meal = new DietMeal
                {
                    Id = Guid.NewGuid(),
                    DietPlanId = plan.Id,
                    MealName = meals[m].name,
                    MealTime = meals[m].time,
                    Calories = perMeal,
                    ProteinGrams = macros.ProteinGrams / Math.Max(meals.Length, 1),
                    CarbsGrams = macros.CarbsGrams / Math.Max(meals.Length, 1),
                    FatGrams = macros.FatGrams / Math.Max(meals.Length, 1),
                    SortOrder = m
                };
                db.DietMeals.Add(meal);
                foreach (var (food, qty) in meals[m].items)
                {
                    db.DietMealItems.Add(new DietMealItem
                    {
                        Id = Guid.NewGuid(),
                        DietMealId = meal.Id,
                        FoodName = food,
                        Quantity = qty
                    });
                }
            }
        }

        Add("Indian Vegetarian Fat Loss Plan", "Balanced vegetarian Indian meals at a gentle calorie deficit.",
            "lose_fat", "vegetarian", 1600, new[]
            {
                ("Breakfast", "8:00", new[] { ("Vegetable poha", "1 bowl"), ("Greek yoghurt", "150 g") }),
                ("Lunch", "13:00", new[] { ("Dal", "1 bowl"), ("Roti", "2"), ("Mixed vegetable sabzi", "1 cup"), ("Salad", "1 plate") }),
                ("Snack", "17:00", new[] { ("Roasted chana", "30 g"), ("Fruit", "1 piece") }),
                ("Dinner", "20:00", new[] { ("Paneer bhurji", "100 g"), ("Roti", "1"), ("Sautéed greens", "1 cup") })
            });

        Add("Indian Non-Vegetarian Fat Loss Plan", "Protein-forward Indian meals for fat loss with lean meats.",
            "lose_fat", "no_preference", 1700, new[]
            {
                ("Breakfast", "8:00", new[] { ("Egg white omelette", "3 eggs"), ("Multigrain toast", "1 slice") }),
                ("Lunch", "13:00", new[] { ("Grilled chicken", "120 g"), ("Brown rice", "1 cup"), ("Salad", "1 plate") }),
                ("Snack", "17:00", new[] { ("Buttermilk", "1 glass"), ("Sprouts", "1 cup") }),
                ("Dinner", "20:00", new[] { ("Fish curry (light)", "120 g"), ("Roti", "1"), ("Vegetables", "1 cup") })
            });

        Add("High Protein Muscle Gain Plan", "A calorie surplus with plenty of protein to support muscle growth.",
            "build_muscle", "no_preference", 2600, new[]
            {
                ("Breakfast", "8:00", new[] { ("Oats", "80 g"), ("Whole eggs", "3"), ("Banana", "1") }),
                ("Lunch", "13:00", new[] { ("Chicken breast", "180 g"), ("Rice", "1.5 cups"), ("Vegetables", "1 cup") }),
                ("Snack", "17:00", new[] { ("Whey protein", "1 scoop"), ("Peanut butter toast", "2 slices") }),
                ("Dinner", "20:30", new[] { ("Lean beef or paneer", "150 g"), ("Sweet potato", "200 g"), ("Salad", "1 plate") })
            });

        Add("Simple Busy Professional Plan", "Fast, repeatable meals for a busy week with minimal prep.",
            "maintain", "no_preference", 2000, new[]
            {
                ("Breakfast", "8:00", new[] { ("Greek yoghurt", "200 g"), ("Granola", "40 g"), ("Berries", "1 cup") }),
                ("Lunch", "13:00", new[] { ("Chicken & quinoa bowl", "1 bowl"), ("Mixed salad", "1 plate") }),
                ("Snack", "16:30", new[] { ("Protein bar", "1"), ("Apple", "1") }),
                ("Dinner", "20:00", new[] { ("Salmon or tofu", "150 g"), ("Roasted vegetables", "1.5 cups") })
            });

        Add("Budget High Protein Plan", "Affordable, high-protein staples that keep costs down.",
            "build_muscle", "no_preference", 2200, new[]
            {
                ("Breakfast", "8:00", new[] { ("Eggs", "3"), ("Oats", "70 g") }),
                ("Lunch", "13:00", new[] { ("Lentils (dal)", "1.5 cups"), ("Rice", "1 cup") }),
                ("Snack", "17:00", new[] { ("Peanuts", "30 g"), ("Milk", "1 glass") }),
                ("Dinner", "20:00", new[] { ("Chicken thigh or soya chunks", "150 g"), ("Roti", "2"), ("Vegetables", "1 cup") })
            });

        Add("General Balanced Diet Plan", "A flexible, balanced plate for everyday wellbeing.",
            "general_health", "no_preference", 2000, new[]
            {
                ("Breakfast", "8:00", new[] { ("Wholegrain toast", "2 slices"), ("Eggs or tofu", "2"), ("Fruit", "1 piece") }),
                ("Lunch", "13:00", new[] { ("Grain bowl with protein", "1 bowl"), ("Salad", "1 plate") }),
                ("Snack", "16:30", new[] { ("Nuts", "30 g"), ("Yoghurt", "150 g") }),
                ("Dinner", "20:00", new[] { ("Protein of choice", "150 g"), ("Vegetables", "1.5 cups"), ("Rice or potato", "1 cup") })
            });
    }

    // ── Challenge templates (joinable public catalogue) ──
    private static async Task SeedChallengeTemplatesAsync(WithinDbContext db)
    {
        var existing = await db.Challenges.Where(c => c.IsTemplate).Select(c => c.Title).ToListAsync();
        var now = DateTimeOffset.UtcNow;

        void Add(string title, string desc, ChallengeType type, int durationDays, bool leaderboard)
        {
            if (existing.Contains(title)) return;
            db.Challenges.Add(new Challenge
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = SystemUserId,
                Title = title,
                Description = desc,
                ChallengeType = type,
                DurationDays = durationDays,
                Visibility = ChallengeVisibility.Public,
                IsPublic = true,
                IsTemplate = true,
                AllowAnonymousParticipation = true,
                LeaderboardEnabled = leaderboard,
                CreatedUtc = now,
                UpdatedUtc = now
            });
        }

        Add("30-Day Push-Up Challenge", "Build pushing strength with a gradually rising daily push-up target.", ChallengeType.PushUp, 30, true);
        Add("30-Day Plank Challenge", "Grow core endurance by holding a plank a little longer each day.", ChallengeType.Mobility, 30, true);
        Add("7-Day Morning Walk Challenge", "Start each day with a calm morning walk for a week.", ChallengeType.Steps, 7, false);
        Add("90-Day Transformation Challenge", "A longer, holistic challenge blending movement, nutrition and rest.", ChallengeType.Custom, 90, true);
        Add("14-Day Mobility Challenge", "Two weeks of short daily mobility flows to move and feel better.", ChallengeType.Mobility, 14, false);
        Add("10,000 Steps Challenge", "Reach 10,000 steps each day and keep a gentle streak going.", ChallengeType.Steps, 30, true);
    }
}
