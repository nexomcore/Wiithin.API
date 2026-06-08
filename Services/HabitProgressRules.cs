namespace WithinAPI.Services;

/// <summary>
/// Gentle, no-guilt progress language for the tiny habit tracker (Sprint 2, plan sections 4.5 and 12).
/// Pure logic only so it can be unit tested without EF/DI. The guiding rule: never shame a missed day,
/// always offer a fresh start, and celebrate small wins.
/// </summary>
public static class HabitProgressRules
{
    /// <summary>Daily completion line, e.g. "You completed 2 of 4 habits today. Nice little win."</summary>
    public static string DailyProgressLabel(int completed, int total)
    {
        if (total <= 0)
        {
            return "Choose a few tiny habits to support your wellbeing journey.";
        }

        if (completed <= 0)
        {
            return "A fresh start is available today. Even one small step counts.";
        }

        if (completed >= total)
        {
            return $"You completed all {total} habits today. Beautiful consistency.";
        }

        return $"You completed {completed} of {total} habits today. Nice little win.";
    }

    /// <summary>Weekly encouragement based on how many days the user showed up (no streak-loss framing).</summary>
    public static string WeeklyShowUpLabel(int daysShownUp)
    {
        if (daysShownUp <= 0)
        {
            return "Welcome back. Today is a fresh start.";
        }

        var dayWord = daysShownUp == 1 ? "day" : "days";
        return $"You showed up {daysShownUp} {dayWord} this week.";
    }

    /// <summary>Returning-after-a-gap line. Always warm, never "you missed yesterday".</summary>
    public static string ComebackLabel() => "Welcome back. Today is a fresh start.";
}
