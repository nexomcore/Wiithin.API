using WithinAPI.Models;

namespace WithinAPI.Services;

public sealed class InMemoryWithinStore
{
    private readonly List<UserRecord> _users =
    [
        new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Demo User", "demo@within.local")
    ];

    private readonly List<DailyCheckInDto> _dailyCheckIns;
    private readonly List<MonthlyHolisticProfileDto> _monthlyProfiles = [];

    public InMemoryWithinStore(WellbeingScoringService scoring)
    {
        _dailyCheckIns =
        [
            SeedDaily(scoring, "2026-05-20", 3, 3, 3, 3, 3, ["Work"], "A steady day."),
            SeedDaily(scoring, "2026-05-21", 4, 3, 2, 4, 4, ["Walk", "Nature"], "Felt better after time outside."),
            SeedDaily(scoring, "2026-05-22", 3, 2, 4, 3, 3, ["Tired", "Sleep"], "Energy needed some care."),
            SeedDaily(scoring, "2026-05-23", 4, 4, 2, 4, 5, ["Yoga", "Grateful"], "Movement helped the day feel grounded."),
            SeedDaily(scoring, "2026-05-24", 3, 3, 3, 4, 3, ["Family"], "Connected with family."),
            SeedDaily(scoring, "2026-05-25", 4, 3, 2, 5, 4, ["Social", "Meditation"], "A connected and calm evening.")
        ];

        var now = DateTime.Today;
        var preview = CreatePreviewMonthlyProfile(now.Month, now.Year);
        var score = scoring.CalculateMonthlyProfile(preview);
        _monthlyProfiles.Add(preview with
        {
            MoveRawScore = score.MoveRaw,
            MoveScorePercent = score.MovePercent,
            FeelRawScore = score.FeelRaw,
            FeelScorePercent = score.FeelPercent,
            SeekRawScore = score.SeekRaw,
            SeekScorePercent = score.SeekPercent,
            HolisticProfileScore = score.HolisticProfileScore
        });
    }

    public UserRecord? FindUserByEmail(string email)
    {
        return _users.FirstOrDefault(user => string.Equals(user.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public UserRecord UpsertUser(string name, string email)
    {
        var existing = FindUserByEmail(email);
        if (existing is not null)
        {
            return existing;
        }

        var user = new UserRecord(Guid.NewGuid(), name.Trim(), email.Trim().ToLowerInvariant());
        _users.Add(user);
        return user;
    }

    public DailyCheckInDto UpsertDailyCheckIn(DailyCheckInDto input)
    {
        var id = input.Id ?? $"daily_{input.CheckInDate}";
        var saved = input with { Id = id };
        _dailyCheckIns.RemoveAll(item => item.CheckInDate == input.CheckInDate);
        _dailyCheckIns.Add(saved);
        return saved;
    }

    public DailyCheckInDto? GetDailyCheckIn(string checkInDate)
    {
        return _dailyCheckIns.FirstOrDefault(item => item.CheckInDate == checkInDate);
    }

    public TrendItemDto[] GetTrend(int days)
    {
        return _dailyCheckIns
            .OrderBy(item => item.CheckInDate)
            .TakeLast(days)
            .Select(item => new TrendItemDto(item.CheckInDate, item.DailyBalanceScore ?? 0))
            .ToArray();
    }

    public MonthlyHolisticProfileDto UpsertMonthlyProfile(MonthlyHolisticProfileDto input)
    {
        var id = input.Id ?? $"profile_{input.Year}_{input.Month}";
        var saved = input with { Id = id };
        _monthlyProfiles.RemoveAll(item => item.Month == input.Month && item.Year == input.Year);
        _monthlyProfiles.Add(saved);
        return saved;
    }

    public MonthlyHolisticProfileDto? GetMonthlyProfile(int month, int year)
    {
        return _monthlyProfiles.FirstOrDefault(item => item.Month == month && item.Year == year);
    }

    public MonthlyHolisticProfileDto[] GetMonthlyHistory(int months)
    {
        return _monthlyProfiles
            .OrderByDescending(item => item.Year)
            .ThenByDescending(item => item.Month)
            .Take(months)
            .ToArray();
    }

    public MonthlyHolisticProfileDto GetCurrentOrPreviewMonthlyProfile()
    {
        var now = DateTime.Today;
        return GetMonthlyProfile(now.Month, now.Year) ?? CreatePreviewMonthlyProfile(now.Month, now.Year);
    }

    public WellbeingDashboardDto GetDashboard(WellbeingScoringService scoring)
    {
        var today = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
        var todayCheckIn = GetDailyCheckIn(today);
        var weeklyAverages = GetWeeklyAverages();
        var strongestSupport = scoring.GetStrongestAndSupport(weeklyAverages);
        var profile = GetCurrentOrPreviewMonthlyProfile();
        var domains = scoring.GetDomainScores(profile);

        return new WellbeingDashboardDto
        {
            TodayCheckInCompleted = todayCheckIn is not null,
            Today = todayCheckIn,
            DailyBalanceScore = todayCheckIn?.DailyBalanceScore,
            WeeklyAverages = weeklyAverages,
            StrongestArea = strongestSupport.StrongestArea,
            SupportArea = strongestSupport.SupportArea,
            TrendItems = GetTrend(7),
            MonthlyProfileCompleted = GetMonthlyProfile(DateTime.Today.Month, DateTime.Today.Year) is not null,
            MonthlyProfile = new MonthlyProfileSummaryDto(profile.HolisticProfileScore ?? 0, domains),
            Recommendations = scoring.GetRecommendations(domains),
            RecentReflections = GetRecentReflections()
        };
    }

    private WeeklyAveragesDto GetWeeklyAverages()
    {
        var latest = _dailyCheckIns.OrderBy(item => item.CheckInDate).TakeLast(7).ToArray();
        if (latest.Length == 0)
        {
            return new WeeklyAveragesDto(0, 0, 0, 0, 0);
        }

        return new WeeklyAveragesDto(
            Average(latest, item => item.MoodScore),
            Average(latest, item => item.EnergyScore),
            Average(latest, item => item.StressScore),
            Average(latest, item => item.ConnectionScore),
            Average(latest, item => item.MeaningScore));
    }

    private ReflectionDto[] GetRecentReflections()
    {
        var daily = _dailyCheckIns
            .Where(item => !string.IsNullOrWhiteSpace(item.Note))
            .OrderByDescending(item => item.CheckInDate)
            .Take(3)
            .Select(item => new ReflectionDto(item.Id ?? item.CheckInDate, item.CheckInDate, "daily", item.Note ?? ""));

        var monthly = _monthlyProfiles
            .Where(item => !string.IsNullOrWhiteSpace(item.ReflectionNote))
            .OrderByDescending(item => item.Year)
            .ThenByDescending(item => item.Month)
            .Take(1)
            .Select(item => new ReflectionDto(item.Id ?? $"{item.Month}/{item.Year}", $"{item.Month}/{item.Year}", "monthly", item.ReflectionNote ?? ""));

        return monthly.Concat(daily).ToArray();
    }

    private static DailyCheckInDto SeedDaily(
        WellbeingScoringService scoring,
        string date,
        int mood,
        int energy,
        int stress,
        int connection,
        int meaning,
        string[] tags,
        string note)
    {
        var dto = new DailyCheckInDto
        {
            Id = $"daily_{date}",
            CheckInDate = date,
            MoodScore = mood,
            EnergyScore = energy,
            StressScore = stress,
            ConnectionScore = connection,
            MeaningScore = meaning,
            Tags = tags,
            Note = note
        };

        return dto with { DailyBalanceScore = scoring.CalculateDailyBalance(dto) };
    }

    private static MonthlyHolisticProfileDto CreatePreviewMonthlyProfile(int month, int year)
    {
        var moveItems = new[]
        {
            Item("move_overall", "Move", 4, 5),
            Item("move_cardio", "Move", 3, 5),
            Item("move_strength", "Move", 3, 5),
            Item("move_agility", "Move", 3, 5),
            Item("move_flexibility", "Move", 4, 5)
        };
        var feelItems = new[]
        {
            Item("feel_optimistic_calm", "Feel", 4, 5),
            Item("feel_energy", "Feel", 3, 5),
            Item("feel_clear_thinking", "Feel", 4, 5),
            Item("feel_confidence", "Feel", 3, 5),
            Item("feel_supported", "Feel", 4, 5),
            Item("feel_open_interest", "Feel", 3, 5)
        };
        var seekItems = new[]
        {
            Item("seek_purpose", "Seek", 4, 6),
            Item("seek_values", "Seek", 5, 6),
            Item("seek_future", "Seek", 4, 6),
            Item("seek_identity", "Seek", 4, 6),
            Item("seek_beyond_tasks", "Seek", 5, 6),
            Item("seek_life_direction", "Seek", 4, 6)
        };

        return new MonthlyHolisticProfileDto
        {
            Id = $"profile_{year}_{month}",
            Month = month,
            Year = year,
            MoveItems = moveItems,
            FeelItems = feelItems,
            SeekItems = seekItems,
            ReflectionNote = "This month felt more balanced when movement and connection were part of the week."
        };
    }

    private static ProfileItemDto Item(string key, string domain, int score, int maxScore)
    {
        return new ProfileItemDto
        {
            QuestionKey = key,
            Domain = domain,
            Score = score,
            MaxScore = maxScore
        };
    }

    private static decimal Average(DailyCheckInDto[] items, Func<DailyCheckInDto, int> selector)
    {
        return Math.Round((decimal)items.Average(selector), 1, MidpointRounding.AwayFromZero);
    }
}
