using WithinAPI.Domain;
using WithinAPI.Models;

namespace WithinAPI.Services;

public sealed class WellbeingScoringService
{
    /// <summary>
    /// Maps the qualitative mood label to a 1-5 wellbeing valence so daily balance and trends remain
    /// comparable over time. Positive/settled moods score high; distressed moods score low.
    /// </summary>
    public static decimal MoodValence(CheckInMood mood) => mood switch
    {
        CheckInMood.Great => 5m,
        CheckInMood.Good => 4m,
        CheckInMood.Grateful => 4m,
        CheckInMood.Peaceful => 4m,
        CheckInMood.Okay => 3m,
        CheckInMood.Tired => 2m,
        CheckInMood.Low => 2m,
        CheckInMood.Stressed => 2m,
        CheckInMood.Anxious => 2m,
        CheckInMood.Angry => 1m,
        _ => 3m
    };

    public static decimal EnergyValence(CheckInEnergy energy) => energy switch
    {
        CheckInEnergy.High => 5m,
        CheckInEnergy.Balanced => 4m,
        CheckInEnergy.Low => 2m,
        CheckInEnergy.Exhausted => 1m,
        _ => 3m
    };

    public static decimal? SleepValence(CheckInSleepQuality? sleep) => sleep switch
    {
        CheckInSleepQuality.Great => 5m,
        CheckInSleepQuality.Okay => 3m,
        CheckInSleepQuality.Poor => 2m,
        CheckInSleepQuality.VeryPoor => 1m,
        CheckInSleepQuality.NotSure => null,
        null => null,
        _ => null
    };

    public int CalculateDailyBalance(CheckInMood mood, CheckInEnergy energy, CheckInSleepQuality? sleep)
    {
        var values = new List<decimal> { MoodValence(mood), EnergyValence(energy) };
        var sleepValence = SleepValence(sleep);
        if (sleepValence is not null)
        {
            values.Add(sleepValence.Value);
        }

        var average = values.Average();
        return (int)Math.Round((average / 5m) * 100m, MidpointRounding.AwayFromZero);
    }

    public MonthlyScoreResult CalculateMonthlyProfile(MonthlyHolisticProfileDto input)
    {
        var moveRaw = input.MoveItems.Sum(item => item.Score);
        var moveMax = input.MoveItems.Sum(item => item.MaxScore);
        var feelRaw = input.FeelItems.Sum(item => item.Score);
        var feelMax = input.FeelItems.Sum(item => item.MaxScore);
        var seekRaw = input.SeekItems.Sum(item => item.Score);
        var seekMax = input.SeekItems.Sum(item => item.MaxScore);

        var movePercent = Percent(moveRaw, moveMax);
        var feelPercent = Percent(feelRaw, feelMax);
        var seekPercent = Percent(seekRaw, seekMax);

        return new MonthlyScoreResult(
            moveRaw,
            movePercent,
            feelRaw,
            feelPercent,
            seekRaw,
            seekPercent,
            (int)Math.Round((movePercent + feelPercent + seekPercent) / 3m, MidpointRounding.AwayFromZero));
    }

    public DomainScoreDto[] GetDomainScores(MonthlyHolisticProfileDto profile)
    {
        var moveMax = profile.MoveItems.Sum(item => item.MaxScore);
        var feelMax = profile.FeelItems.Sum(item => item.MaxScore);
        var seekMax = profile.SeekItems.Sum(item => item.MaxScore);

        return
        [
            new("Move", profile.MoveRawScore ?? 0, moveMax, profile.MoveScorePercent ?? 0, GetBand(profile.MoveScorePercent ?? 0)),
            new("Feel", profile.FeelRawScore ?? 0, feelMax, profile.FeelScorePercent ?? 0, GetBand(profile.FeelScorePercent ?? 0)),
            new("Seek", profile.SeekRawScore ?? 0, seekMax, profile.SeekScorePercent ?? 0, GetBand(profile.SeekScorePercent ?? 0))
        ];
    }

    public RecommendationDto[] GetRecommendations(DomainScoreDto[] domains)
    {
        return domains
            .Where(domain => domain.Percent < 70)
            .SelectMany(domain => RecommendationCatalog[domain.Domain].Select((item, index) =>
                new RecommendationDto(
                    $"{domain.Domain.ToLowerInvariant()}_{index}",
                    domain.Domain,
                    index < 2 ? "event" : "service",
                    item.Title,
                    item.Description)))
            .ToArray();
    }

    public (string StrongestArea, string SupportArea) GetStrongestAndSupport(WeeklyAveragesDto weeklyAverages)
    {
        var areas = new Dictionary<string, decimal>
        {
            ["Mood"] = weeklyAverages.Mood,
            ["Energy"] = weeklyAverages.Energy,
            ["Sleep"] = weeklyAverages.Sleep
        };

        return (
            areas.MaxBy(pair => pair.Value).Key,
            areas.MinBy(pair => pair.Value).Key);
    }

    private static int Percent(int raw, int max)
    {
        if (max <= 0)
        {
            return 0;
        }

        return (int)Math.Round((raw / (decimal)max) * 100m, MidpointRounding.AwayFromZero);
    }

    private static string GetBand(int percent)
    {
        if (percent >= 85) return "Strong";
        if (percent >= 70) return "Steady";
        if (percent >= 55) return "Support area";
        return "Needs care";
    }

    private static readonly Dictionary<string, (string Title, string Description)[]> RecommendationCatalog = new()
    {
        ["Move"] =
        [
            ("Yoga class", "A gentle session to support mobility and daily rhythm."),
            ("Walking group", "A low-pressure way to move and connect."),
            ("Beginner gym session", "A structured entry point for strength and confidence."),
            ("Cycling/running club", "A social movement option for stamina and momentum.")
        ],
        ["Feel"] =
        [
            ("Mindfulness class", "A quiet practice for steadiness and attention."),
            ("Journaling circle", "A reflective group format for emotional clarity."),
            ("Nature walk", "A restorative activity for mood and stress care."),
            ("Community social event", "A simple way to strengthen connection.")
        ],
        ["Seek"] =
        [
            ("Meditation retreat", "Dedicated time for meaning, reflection, and purpose."),
            ("Volunteering", "A grounded way to connect values with action."),
            ("Personal development workshop", "A structured setting for growth and direction."),
            ("Spiritual/philosophy group", "A shared space for curiosity and deeper questions.")
        ]
    };
}
