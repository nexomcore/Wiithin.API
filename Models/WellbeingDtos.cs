namespace WithinAPI.Models;

public sealed record DailyCheckInDto
{
    public string? Id { get; init; }
    public string CheckInDate { get; init; } = "";
    public int MoodScore { get; init; }
    public int EnergyScore { get; init; }
    public int StressScore { get; init; }
    public int ConnectionScore { get; init; }
    public int MeaningScore { get; init; }
    public string[] Tags { get; init; } = [];
    public string? Note { get; init; }
    public int? DailyBalanceScore { get; init; }
}

public sealed record MonthlyHolisticProfileDto
{
    public string? Id { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public ProfileItemDto[] MoveItems { get; init; } = [];
    public ProfileItemDto[] FeelItems { get; init; } = [];
    public ProfileItemDto[] SeekItems { get; init; } = [];
    public int? MoveRawScore { get; init; }
    public int? MoveScorePercent { get; init; }
    public int? FeelRawScore { get; init; }
    public int? FeelScorePercent { get; init; }
    public int? SeekRawScore { get; init; }
    public int? SeekScorePercent { get; init; }
    public int? HolisticProfileScore { get; init; }
    public string? ReflectionNote { get; init; }
}

public sealed record ProfileItemDto
{
    public string QuestionKey { get; init; } = "";
    public string Domain { get; init; } = "";
    public int Score { get; init; }
    public int MaxScore { get; init; }
}

public sealed record TrendItemDto(string Date, int DailyBalanceScore);

public sealed record DomainScoreDto(string Domain, int RawScore, int MaxScore, int Percent, string Band);

public sealed record RecommendationDto(
    string Id,
    string Domain,
    string RecommendationType,
    string Title,
    string Description);

public sealed record ReflectionDto(string Id, string Date, string Type, string Note);

public sealed record WeeklyAveragesDto(decimal Mood, decimal Energy, decimal Stress, decimal Connection, decimal Meaning);

public sealed record WellbeingDashboardDto
{
    public bool TodayCheckInCompleted { get; init; }
    public DailyCheckInDto? Today { get; init; }
    public int? DailyBalanceScore { get; init; }
    public WeeklyAveragesDto WeeklyAverages { get; init; } = new(0, 0, 0, 0, 0);
    public string? StrongestArea { get; init; }
    public string? SupportArea { get; init; }
    public TrendItemDto[] TrendItems { get; init; } = [];
    public bool MonthlyProfileCompleted { get; init; }
    public MonthlyProfileSummaryDto? MonthlyProfile { get; init; }
    public RecommendationDto[] Recommendations { get; init; } = [];
    public ReflectionDto[] RecentReflections { get; init; } = [];
}

public sealed record MonthlyProfileSummaryDto(int HolisticProfileScore, DomainScoreDto[] Domains);

public sealed record MonthlyScoreResult(
    int MoveRaw,
    int MovePercent,
    int FeelRaw,
    int FeelPercent,
    int SeekRaw,
    int SeekPercent,
    int HolisticProfileScore);
