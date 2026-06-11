using WithinAPI.Domain;

namespace WithinAPI.Services;

/// <summary>
/// Pure plan-generation rules for the Move Workouts/Diet tabs: choose the closest
/// system template for a user's preferences and decide which active plan a newly
/// activated plan supersedes. No EF/DI so it unit-tests in isolation. Generation is
/// strictly template-based (no AI), per the MVP brief.
/// </summary>
public static class MovePlanRules
{
    /// <summary>
    /// Score a workout template against requested preferences. Higher is better.
    /// Equal-token matches on type/goal/experience score highest; days-per-week
    /// closeness breaks ties so a 3-day request prefers a 3-day plan over a 5-day one.
    /// </summary>
    public static int ScoreWorkoutTemplate(WorkoutPlan template, string? goal, string? workoutType, string? experienceLevel, int? daysPerWeek)
    {
        var score = 0;
        if (TokenMatch(template.WorkoutType, workoutType)) score += 40;
        if (TokenMatch(template.ExperienceLevel, experienceLevel)) score += 20;
        if (GoalMatch(template.Goal, goal)) score += 25;
        if (daysPerWeek is int wanted)
        {
            var diff = Math.Abs(template.DaysPerWeek - wanted);
            score += Math.Max(0, 15 - diff * 5);
        }
        return score;
    }

    public static WorkoutPlan? MatchWorkoutTemplate(IEnumerable<WorkoutPlan> templates, string? goal, string? workoutType, string? experienceLevel, int? daysPerWeek)
    {
        WorkoutPlan? best = null;
        var bestScore = int.MinValue;
        foreach (var template in templates.Where(t => t.IsTemplate))
        {
            var score = ScoreWorkoutTemplate(template, goal, workoutType, experienceLevel, daysPerWeek);
            if (score > bestScore)
            {
                bestScore = score;
                best = template;
            }
        }
        return best;
    }

    public static int ScoreDietTemplate(DietPlan template, string? goal, string? dietPreference, int? calorieTarget)
    {
        var score = 0;
        if (TokenMatch(template.DietPreference, dietPreference)) score += 40;
        if (GoalMatch(template.Goal, goal)) score += 30;
        if (calorieTarget is int wanted && template.Calories > 0)
        {
            var diff = Math.Abs(template.Calories - wanted);
            score += Math.Max(0, 20 - diff / 100);
        }
        return score;
    }

    public static DietPlan? MatchDietTemplate(IEnumerable<DietPlan> templates, string? goal, string? dietPreference, int? calorieTarget)
    {
        DietPlan? best = null;
        var bestScore = int.MinValue;
        foreach (var template in templates.Where(t => t.IsTemplate))
        {
            var score = ScoreDietTemplate(template, goal, dietPreference, calorieTarget);
            if (score > bestScore)
            {
                bestScore = score;
                best = template;
            }
        }
        return best;
    }

    /// <summary>
    /// Single-active-plan rule: when a user activates a plan, any other plan of theirs that
    /// is currently Active should be paused. Returns the ids to pause (excludes the new one).
    /// </summary>
    public static IReadOnlyList<Guid> ActivePlansToPause(IEnumerable<(Guid Id, MovePlanStatus Status)> userPlans, Guid activatingPlanId) =>
        userPlans
            .Where(plan => plan.Id != activatingPlanId && plan.Status == MovePlanStatus.Active)
            .Select(plan => plan.Id)
            .ToArray();

    private static bool TokenMatch(string? a, string? b) =>
        !string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b) &&
        string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

    // A template's Goal column may list several supported goals (comma/pipe separated).
    private static bool GoalMatch(string? templateGoals, string? wanted)
    {
        if (string.IsNullOrWhiteSpace(templateGoals) || string.IsNullOrWhiteSpace(wanted)) return false;
        return templateGoals
            .Split([',', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(goal => string.Equals(goal, wanted.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
