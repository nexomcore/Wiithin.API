namespace WithinAPI.Models;

public sealed record WellbeingOptionDto(string Key, string Label);

public sealed record WellbeingInterestOptionDto(string Category, string Key, string Label);

public sealed record WellbeingOptionsDto
{
    public Dictionary<string, WellbeingOptionDto[]> Interests { get; init; } = [];
    public WellbeingOptionDto[] Goals { get; init; } = [];
    public string[] ActivityLevels { get; init; } = [];
    public WellbeingOptionDto[] MoodOptions { get; init; } = [];
}

public sealed record WellbeingProfileDto
{
    public string? Id { get; init; }
    public string UserId { get; init; } = "";
    public string FirstName { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public bool UsePseudonym { get; init; }
    public string? Pseudonym { get; init; }
    public string? DateOfBirth { get; init; }
    public int? Age { get; init; }
    public string? AgeRange { get; init; }
    public string? Gender { get; init; }
    public string? LocationCountry { get; init; }
    public string? LocationCity { get; init; }
    public string? LocationSuburb { get; init; }
    public string? ProfilePhotoUrl { get; init; }
    public decimal? HeightCm { get; init; }
    public decimal? WeightKg { get; init; }
    public string? ActivityLevel { get; init; }
    public decimal? AverageSleepHours { get; init; }
    public decimal? WaterIntakeLitres { get; init; }
    public int? ExerciseDaysPerWeek { get; init; }
    public string? MeditationFrequency { get; init; }
    public int? StressLevelBaseline { get; init; }
    public int? EnergyLevelBaseline { get; init; }
    public int? MoodLevelBaseline { get; init; }
    public decimal? BodyFatPercentage { get; init; }
    public int? RestingHeartRate { get; init; }
    public decimal? Vo2Max { get; init; }
    public int? BloodPressureSystolic { get; init; }
    public int? BloodPressureDiastolic { get; init; }
    public string? WearableProvider { get; init; }
    public bool WearableConnected { get; init; }
    public string? LastWearableSyncAt { get; init; }
    public bool OnboardingCompleted { get; init; }
    public string? CreatedAt { get; init; }
    public string? UpdatedAt { get; init; }
}

public sealed record WellbeingInterestDto(string Category, string InterestKey, string InterestLabel);

public sealed record WellbeingGoalDto(string GoalKey, string GoalLabel);

public sealed record WellbeingProfileResponseDto
{
    public WellbeingProfileDto? Profile { get; init; }
    public WellbeingInterestDto[] Interests { get; init; } = [];
    public WellbeingGoalDto[] Goals { get; init; } = [];
}

public sealed record WellbeingOnboardingRequest
{
    public string FirstName { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public bool UsePseudonym { get; init; }
    public string? Pseudonym { get; init; }
    public string? DateOfBirth { get; init; }
    public int? Age { get; init; }
    public string? AgeRange { get; init; }
    public string? Gender { get; init; }
    public string? LocationCountry { get; init; }
    public string? LocationCity { get; init; }
    public string? LocationSuburb { get; init; }
    public string? ProfilePhotoUrl { get; init; }
    public string[] InterestKeys { get; init; } = [];
    public string[] GoalKeys { get; init; } = [];
}

public sealed record UpdateWellbeingProfileRequest
{
    public decimal? HeightCm { get; init; }
    public decimal? WeightKg { get; init; }
    public string? ActivityLevel { get; init; }
    public decimal? AverageSleepHours { get; init; }
    public decimal? WaterIntakeLitres { get; init; }
    public int? ExerciseDaysPerWeek { get; init; }
    public string? MeditationFrequency { get; init; }
    public int? StressLevelBaseline { get; init; }
    public int? EnergyLevelBaseline { get; init; }
    public int? MoodLevelBaseline { get; init; }
}

public sealed record UpsertMvpDailyCheckInRequest
{
    public string? Mood { get; init; }
    public int MoodScore { get; init; }
    public int EnergyLevel { get; init; }
    public int StressLevel { get; init; }
    public bool DidMoveToday { get; init; }
    public bool DidMeditateToday { get; init; }
    public string? JournalEntry { get; init; }
}

public sealed record MvpDailyCheckInDto
{
    public string Id { get; init; } = "";
    public string CheckInDate { get; init; } = "";
    public string Mood { get; init; } = "";
    public int MoodScore { get; init; }
    public int EnergyLevel { get; init; }
    public int StressLevel { get; init; }
    public bool DidMoveToday { get; init; }
    public bool DidMeditateToday { get; init; }
    public string? JournalEntry { get; init; }
    public string CreatedAt { get; init; } = "";
    public string UpdatedAt { get; init; } = "";
}

public sealed record WellbeingSummaryDto
{
    public int CurrentStreak { get; init; }
    public int TotalCheckIns { get; init; }
    public decimal AverageMoodScore { get; init; }
    public decimal AverageEnergyLevel { get; init; }
    public decimal AverageStressLevel { get; init; }
    public int MoveCompletionCount { get; init; }
    public int MeditationCompletionCount { get; init; }
    public MvpDailyCheckInDto? LatestCheckIn { get; init; }
}

public sealed record DailyCheckInDto
{
    public string? Id { get; init; }
    public string CheckInDate { get; init; } = "";
    public string Mood { get; init; } = "";
    public string Energy { get; init; } = "";
    public string? SleepQuality { get; init; }
    public decimal? SleepHours { get; init; }
    public string Intention { get; init; } = "";
    public string[] Tags { get; init; } = [];
    public string? Note { get; init; }
    public string? SuggestedActionKey { get; init; }
    public string? SuggestedAction { get; init; }
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

public sealed record WeeklyAveragesDto(decimal Mood, decimal Energy, decimal Sleep);

public sealed record WellbeingDashboardDto
{
    public bool TodayCheckInCompleted { get; init; }
    public DailyCheckInDto? Today { get; init; }
    public int? DailyBalanceScore { get; init; }
    public WeeklyAveragesDto WeeklyAverages { get; init; } = new(0, 0, 0);
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
