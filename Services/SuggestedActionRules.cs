using WithinAPI.Domain;

namespace WithinAPI.Services;

/// <summary>
/// Deterministic, rule-based suggested action for a daily check-in (Sprint 1, plan section 3.5).
/// Pure logic only — no EF, no DI — so the test project can compile it directly without the
/// running WithinAPI assembly. Rules are evaluated top to bottom and the first match wins.
/// </summary>
public static class SuggestedActionRules
{
    public sealed record SuggestedAction(string Key, string Text);

    public static SuggestedAction Resolve(CheckInMood mood, CheckInEnergy energy, DailyIntention intention)
    {
        if (mood is CheckInMood.Stressed or CheckInMood.Anxious)
        {
            return new("breathing_reset", "Try a 2-minute breathing reset or browse meditation circles.");
        }

        if (mood is CheckInMood.Tired || energy is CheckInEnergy.Exhausted)
        {
            return new("go_gentle", "Go gentle today. Try rest, stretching or an early night.");
        }

        if (mood is CheckInMood.Low)
        {
            return new("small_manageable", "Write one small thing that felt manageable today or check supportive circles.");
        }

        if (energy is CheckInEnergy.High && intention is DailyIntention.MoveMyBody)
        {
            return new("move_event", "Find a fitness, walking, yoga or outdoor event near you.");
        }

        if (intention is DailyIntention.ConnectWithSomeone)
        {
            return new("connect", "Check new posts in your circles or attend a community event.");
        }

        if (intention is DailyIntention.PracticeGratitude)
        {
            return new("gratitude", "Add one gratitude note for today.");
        }

        return new("small_action", "Take one small wellbeing action today. Even 2 minutes counts.");
    }

    /// <summary>Resolve only the human-readable text for a previously stored suggestion key.</summary>
    public static string? TextForKey(string? key) => key switch
    {
        "breathing_reset" => "Try a 2-minute breathing reset or browse meditation circles.",
        "go_gentle" => "Go gentle today. Try rest, stretching or an early night.",
        "small_manageable" => "Write one small thing that felt manageable today or check supportive circles.",
        "move_event" => "Find a fitness, walking, yoga or outdoor event near you.",
        "connect" => "Check new posts in your circles or attend a community event.",
        "gratitude" => "Add one gratitude note for today.",
        "small_action" => "Take one small wellbeing action today. Even 2 minutes counts.",
        _ => null
    };
}
