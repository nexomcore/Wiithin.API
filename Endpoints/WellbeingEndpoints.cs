using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class WellbeingEndpoints
{
    public static IEndpointRouteBuilder MapWellbeingEndpoints(this IEndpointRouteBuilder app)
    {
        var wellbeing = app.MapGroup("/api/wellbeing").RequireAuthorization();

        wellbeing.MapGet("/options", () => Results.Ok(WellbeingProfileOptions.ToDto()));

        wellbeing.MapGet("/profile", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var profile = await db.UserWellbeingProfiles.FirstOrDefaultAsync(item => item.UserId == userId);
            var interests = await db.UserWellbeingInterests
                .Where(item => item.UserId == userId)
                .OrderBy(item => item.Category)
                .ThenBy(item => item.InterestLabel)
                .Select(item => new WellbeingInterestDto(item.Category, item.InterestKey, item.InterestLabel))
                .ToArrayAsync();
            var goals = await db.UserWellbeingGoals
                .Where(item => item.UserId == userId)
                .OrderBy(item => item.CreatedAt)
                .Select(item => new WellbeingGoalDto(item.GoalKey, item.GoalLabel))
                .ToArrayAsync();

            return Results.Ok(new WellbeingProfileResponseDto
            {
                Profile = profile?.ToDto(),
                Interests = interests,
                Goals = goals
            });
        });

        wellbeing.MapPost("/onboarding", async (WellbeingOnboardingRequest request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return Results.BadRequest(new { message = "First name and display name are required." });
            }

            if (request.UsePseudonym && string.IsNullOrWhiteSpace(request.Pseudonym))
            {
                return Results.BadRequest(new { message = "Pseudonym is required when pseudonym mode is enabled." });
            }

            if (request.GoalKeys.Distinct(StringComparer.OrdinalIgnoreCase).Count() > 3)
            {
                return Results.BadRequest(new { message = "Choose up to 3 goals." });
            }

            if (!TryParseDateOnly(request.DateOfBirth, out var dateOfBirth))
            {
                return Results.BadRequest(new { message = "Date of birth must use yyyy-MM-dd format." });
            }

            if (request.Age is < 13 or > 120)
            {
                return Results.BadRequest(new { message = "Age must be between 13 and 120." });
            }

            var selectedInterests = new List<WellbeingInterestOptionDto>();
            foreach (var key in request.InterestKeys.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!WellbeingProfileOptions.TryGetInterest(key, out var option))
                {
                    return Results.BadRequest(new { message = $"Interest '{key}' is not supported." });
                }
                selectedInterests.Add(option);
            }

            var selectedGoals = new List<WellbeingOptionDto>();
            foreach (var key in request.GoalKeys.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!WellbeingProfileOptions.TryGetGoal(key, out var option))
                {
                    return Results.BadRequest(new { message = $"Goal '{key}' is not supported." });
                }
                selectedGoals.Add(option);
            }

            var now = DateTimeOffset.UtcNow;
            var profile = await db.UserWellbeingProfiles.FirstOrDefaultAsync(item => item.UserId == userId);
            if (profile is null)
            {
                profile = new UserWellbeingProfile { Id = Guid.NewGuid(), UserId = userId, CreatedAt = now };
                db.UserWellbeingProfiles.Add(profile);
            }

            profile.FirstName = request.FirstName.Trim();
            profile.DisplayName = request.DisplayName.Trim();
            profile.UsePseudonym = request.UsePseudonym;
            profile.Pseudonym = NormalizeNullable(request.Pseudonym);
            profile.DateOfBirth = dateOfBirth;
            profile.Age = request.Age;
            profile.AgeRange = NormalizeNullable(request.AgeRange);
            profile.Gender = NormalizeNullable(request.Gender);
            profile.LocationCountry = "Australia";
            profile.LocationCity = NormalizeNullable(request.LocationCity) ?? "Perth";
            profile.LocationSuburb = NormalizeNullable(request.LocationSuburb);
            profile.ProfilePhotoUrl = NormalizeNullable(request.ProfilePhotoUrl);
            profile.OnboardingCompleted = true;
            profile.UpdatedAt = now;

            var user = await db.Users.FindAsync(userId);
            if (user is not null)
            {
                user.DisplayName = profile.UsePseudonym && !string.IsNullOrWhiteSpace(profile.Pseudonym)
                    ? profile.Pseudonym
                    : profile.DisplayName;
            }

            await db.UserWellbeingInterests.Where(item => item.UserId == userId).ExecuteDeleteAsync();
            await db.UserWellbeingGoals.Where(item => item.UserId == userId).ExecuteDeleteAsync();
            db.UserWellbeingInterests.AddRange(selectedInterests.Select(item => new UserWellbeingInterest
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = item.Category,
                InterestKey = item.Key,
                InterestLabel = item.Label,
                CreatedAt = now
            }));
            db.UserWellbeingGoals.AddRange(selectedGoals.Select(item => new UserWellbeingGoal
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GoalKey = item.Key,
                GoalLabel = item.Label,
                CreatedAt = now
            }));

            await db.SaveChangesAsync();
            return Results.Ok(new { onboardingCompleted = true });
        });

        wellbeing.MapPut("/profile", async (UpdateWellbeingProfileRequest request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!TryValidateOptionalProfile(request, out var message))
            {
                return Results.BadRequest(new { message });
            }

            var now = DateTimeOffset.UtcNow;
            var profile = await db.UserWellbeingProfiles.FirstOrDefaultAsync(item => item.UserId == userId);
            if (profile is null)
            {
                profile = new UserWellbeingProfile { Id = Guid.NewGuid(), UserId = userId, CreatedAt = now };
                db.UserWellbeingProfiles.Add(profile);
            }

            profile.HeightCm = request.HeightCm ?? profile.HeightCm;
            profile.WeightKg = request.WeightKg ?? profile.WeightKg;
            profile.ActivityLevel = string.IsNullOrWhiteSpace(request.ActivityLevel) ? profile.ActivityLevel : request.ActivityLevel.Trim();
            profile.AverageSleepHours = request.AverageSleepHours ?? profile.AverageSleepHours;
            profile.WaterIntakeLitres = request.WaterIntakeLitres ?? profile.WaterIntakeLitres;
            profile.ExerciseDaysPerWeek = request.ExerciseDaysPerWeek ?? profile.ExerciseDaysPerWeek;
            profile.MeditationFrequency = string.IsNullOrWhiteSpace(request.MeditationFrequency) ? profile.MeditationFrequency : request.MeditationFrequency.Trim();
            profile.StressLevelBaseline = request.StressLevelBaseline ?? profile.StressLevelBaseline;
            profile.EnergyLevelBaseline = request.EnergyLevelBaseline ?? profile.EnergyLevelBaseline;
            profile.MoodLevelBaseline = request.MoodLevelBaseline ?? profile.MoodLevelBaseline;
            profile.UpdatedAt = now;

            await db.SaveChangesAsync();
            return Results.Ok(profile.ToDto());
        });

        wellbeing.MapPost("/checkins", async (UpsertMvpDailyCheckInRequest request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!TryValidateCheckIn(request, out var message))
            {
                return Results.BadRequest(new { message });
            }

            if (!Enum.TryParse<CheckInMood>(request.Mood, ignoreCase: true, out var mood) ||
                !WellbeingProfileOptions.MoodOptions.Any(item => item.Key.Equals(mood.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                return Results.BadRequest(new { message = "Please choose a supported mood." });
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var now = DateTimeOffset.UtcNow;
            var checkIn = await db.DailyCheckIns.FirstOrDefaultAsync(item => item.UserId == userId && item.CheckInDate == today);
            if (checkIn is null)
            {
                checkIn = new DailyCheckIn
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CheckInDate = today,
                    CreatedAtUtc = now
                };
                db.DailyCheckIns.Add(checkIn);
            }

            checkIn.Mood = mood;
            checkIn.MoodScore = request.MoodScore;
            checkIn.Energy = ToLegacyEnergy(request.EnergyLevel);
            checkIn.EnergyLevel = request.EnergyLevel;
            checkIn.StressLevel = request.StressLevel;
            checkIn.DidMoveToday = request.DidMoveToday;
            checkIn.DidMeditateToday = request.DidMeditateToday;
            checkIn.Intention = request.DidMeditateToday ? DailyIntention.BeMindful : DailyIntention.StayCalm;
            checkIn.JournalEntry = NormalizeNullable(request.JournalEntry);
            checkIn.Note = checkIn.JournalEntry is null ? checkIn.Note : checkIn.JournalEntry;
            checkIn.DailyBalanceScore = CalculateMvpBalance(request.MoodScore, request.EnergyLevel, request.StressLevel);
            checkIn.UpdatedAtUtc = now;

            await db.SaveChangesAsync();
            return Results.Ok(checkIn.ToMvpDto());
        });

        wellbeing.MapGet("/checkins", async (WithinDbContext db, ClaimsPrincipal principal, string? fromDate, string? toDate) =>
        {
            var userId = principal.UserId();
            if (!TryParseDateOnly(fromDate, out var from) || !TryParseDateOnly(toDate, out var to))
            {
                return Results.BadRequest(new { message = "Dates must use yyyy-MM-dd format." });
            }

            var end = to ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var start = from ?? end.AddDays(-30);
            if (start > end)
            {
                return Results.BadRequest(new { message = "fromDate must be before toDate." });
            }

            var checkIns = await db.DailyCheckIns
                .Where(item => item.UserId == userId && item.CheckInDate >= start && item.CheckInDate <= end)
                .OrderByDescending(item => item.CheckInDate)
                .ToArrayAsync();

            return Results.Ok(checkIns.Select(item => item.ToMvpDto()).ToArray());
        });

        wellbeing.MapGet("/summary", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var checkIns = await db.DailyCheckIns
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.CheckInDate)
                .ToArrayAsync();

            if (checkIns.Length == 0)
            {
                return Results.Ok(new WellbeingSummaryDto());
            }

            return Results.Ok(new WellbeingSummaryDto
            {
                CurrentStreak = CurrentStreak(checkIns),
                TotalCheckIns = checkIns.Length,
                AverageMoodScore = Round(checkIns.Average(item => item.MoodScore ?? 3)),
                AverageEnergyLevel = Round(checkIns.Average(item => item.EnergyLevel ?? 5)),
                AverageStressLevel = Round(checkIns.Average(item => item.StressLevel ?? 5)),
                MoveCompletionCount = checkIns.Count(item => item.DidMoveToday),
                MeditationCompletionCount = checkIns.Count(item => item.DidMeditateToday),
                LatestCheckIn = checkIns[0].ToMvpDto()
            });
        });

        wellbeing.MapPost("/daily-checkin", async (DailyCheckInDto request, WithinDbContext db, WellbeingScoringService scoring, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            if (!DateOnly.TryParse(request.CheckInDate, out var date))
            {
                return Results.BadRequest(new { message = "A valid check-in date is required." });
            }

            if (!Enum.TryParse<CheckInMood>(request.Mood, ignoreCase: true, out var mood))
            {
                return Results.BadRequest(new { message = "Please choose how you are feeling today." });
            }

            if (!Enum.TryParse<CheckInEnergy>(request.Energy, ignoreCase: true, out var energy))
            {
                return Results.BadRequest(new { message = "Please choose your energy for today." });
            }

            if (!Enum.TryParse<DailyIntention>(request.Intention, ignoreCase: true, out var intention))
            {
                return Results.BadRequest(new { message = "Please choose one intention for today." });
            }

            CheckInSleepQuality? sleepQuality = null;
            if (!string.IsNullOrWhiteSpace(request.SleepQuality))
            {
                if (!Enum.TryParse<CheckInSleepQuality>(request.SleepQuality, ignoreCase: true, out var parsedSleep))
                {
                    return Results.BadRequest(new { message = "That sleep quality option is not recognised." });
                }
                sleepQuality = parsedSleep;
            }

            if (request.SleepHours is < 0 or > 16)
            {
                return Results.BadRequest(new { message = "Sleep hours must be between 0 and 16." });
            }

            if (request.Note?.Length > 500)
            {
                return Results.BadRequest(new { message = "Daily note must be 500 characters or fewer." });
            }

            var now = DateTimeOffset.UtcNow;
            var saved = await db.DailyCheckIns.FirstOrDefaultAsync(item => item.UserId == userId && item.CheckInDate == date);
            if (saved is null)
            {
                saved = new DailyCheckIn { Id = Guid.NewGuid(), UserId = userId, CheckInDate = date, CreatedAtUtc = now };
                db.DailyCheckIns.Add(saved);
            }

            saved.Mood = mood;
            saved.MoodScore ??= mood switch
            {
                CheckInMood.Great => 5,
                CheckInMood.Good => 4,
                CheckInMood.Okay => 3,
                CheckInMood.Low => 2,
                CheckInMood.Struggling => 1,
                _ => 3
            };
            saved.Energy = energy;
            saved.EnergyLevel ??= energy switch
            {
                CheckInEnergy.High => 9,
                CheckInEnergy.Balanced => 6,
                CheckInEnergy.Low => 3,
                CheckInEnergy.Exhausted => 1,
                _ => 5
            };
            saved.SleepQuality = sleepQuality;
            saved.SleepHours = request.SleepHours;
            saved.Intention = intention;
            saved.Tags = request.Tags.Select(tag => tag.Trim().ToLowerInvariant()).Where(tag => tag.Length > 0).Distinct().ToArray();
            saved.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            saved.SuggestedActionKey = SuggestedActionRules.Resolve(mood, energy, intention).Key;
            saved.DailyBalanceScore = scoring.CalculateDailyBalance(mood, energy, sleepQuality);
            saved.UpdatedAtUtc = now;
            await db.SaveChangesAsync();
            return Results.Ok(saved.ToDto());
        });

        wellbeing.MapGet("/daily-checkin/today", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var userId = principal.UserId();
            var checkIn = await db.DailyCheckIns.FirstOrDefaultAsync(item => item.UserId == userId && item.CheckInDate == today);
            return checkIn is null ? Results.NotFound() : Results.Ok(checkIn.ToDto());
        });

        wellbeing.MapGet("/daily-checkin/trend", async (WithinDbContext db, ClaimsPrincipal principal, int? days) =>
        {
            var range = Math.Clamp(days ?? 7, 1, 30);
            var end = DateOnly.FromDateTime(DateTime.UtcNow);
            var start = end.AddDays(-(range - 1));
            var userId = principal.UserId();
            var trend = await db.DailyCheckIns
                .Where(item => item.UserId == userId && item.CheckInDate >= start && item.CheckInDate <= end)
                .OrderBy(item => item.CheckInDate)
                .Select(item => new TrendItemDto(item.CheckInDate.ToString("yyyy-MM-dd"), item.DailyBalanceScore))
                .ToArrayAsync();
            return Results.Ok(trend);
        });

        wellbeing.MapGet("/dashboard", async (WithinDbContext db, ClaimsPrincipal principal, WellbeingScoringService scoring) =>
        {
            var userId = principal.UserId();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var weekStart = today.AddDays(-6);
            var recent = await db.DailyCheckIns
                .Where(item => item.UserId == userId && item.CheckInDate >= weekStart && item.CheckInDate <= today)
                .OrderBy(item => item.CheckInDate)
                .ToArrayAsync();
            var todayCheckIn = recent.FirstOrDefault(item => item.CheckInDate == today);
            var sleepValues = recent.Select(item => WellbeingScoringService.SleepValence(item.SleepQuality)).Where(value => value is not null).Select(value => value!.Value).ToArray();
            var weeklyAverages = recent.Length == 0
                ? new WeeklyAveragesDto(0, 0, 0)
                : new WeeklyAveragesDto(
                    Round(recent.Average(item => WellbeingScoringService.MoodValence(item.Mood))),
                    Round(recent.Average(item => WellbeingScoringService.EnergyValence(item.Energy))),
                    sleepValues.Length == 0 ? 0 : Round(sleepValues.Average()));
            var areas = recent.Length == 0 ? ((string?)null, (string?)null) : scoring.GetStrongestAndSupport(weeklyAverages);

            return Results.Ok(new WellbeingDashboardDto
            {
                TodayCheckInCompleted = todayCheckIn is not null,
                Today = todayCheckIn?.ToDto(),
                DailyBalanceScore = todayCheckIn?.DailyBalanceScore,
                WeeklyAverages = weeklyAverages,
                StrongestArea = areas.Item1,
                SupportArea = areas.Item2,
                TrendItems = recent.Select(item => new TrendItemDto(item.CheckInDate.ToString("yyyy-MM-dd"), item.DailyBalanceScore)).ToArray(),
                MonthlyProfileCompleted = false,
                Recommendations = [],
                RecentReflections = recent
                    .Where(item => !string.IsNullOrWhiteSpace(item.Note))
                    .OrderByDescending(item => item.CheckInDate)
                    .Take(3)
                    .Select(item => new ReflectionDto(item.Id.ToString(), item.CheckInDate.ToString("yyyy-MM-dd"), "Daily Pulse", item.Note!))
                    .ToArray()
            });
        });

        return app;
    }

    private static decimal Round(decimal value) => Math.Round(value, 1, MidpointRounding.AwayFromZero);

    private static decimal Round(double value) => Math.Round((decimal)value, 1, MidpointRounding.AwayFromZero);

    private static bool TryParseDateOnly(string? value, out DateOnly? date)
    {
        date = null;
        if (string.IsNullOrWhiteSpace(value)) return true;
        if (!DateOnly.TryParse(value, out var parsed)) return false;
        date = parsed;
        return true;
    }

    private static string? NormalizeNullable(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static bool TryValidateOptionalProfile(UpdateWellbeingProfileRequest request, out string message)
    {
        if (request.HeightCm is < 80 or > 250)
        {
            message = "Height must be between 80 and 250 cm.";
            return false;
        }
        if (request.WeightKg is < 25 or > 300)
        {
            message = "Weight must be between 25 and 300 kg.";
            return false;
        }
        if (!WellbeingProfileOptions.IsActivityLevelAllowed(request.ActivityLevel))
        {
            message = "Activity level is not supported.";
            return false;
        }
        if (request.AverageSleepHours is < 0 or > 24)
        {
            message = "Sleep hours must be between 0 and 24.";
            return false;
        }
        if (request.ExerciseDaysPerWeek is < 0 or > 7)
        {
            message = "Exercise days must be between 0 and 7.";
            return false;
        }
        if (request.WaterIntakeLitres is < 0 or > 10)
        {
            message = "Water intake must be between 0 and 10 litres.";
            return false;
        }
        if (request.StressLevelBaseline is < 1 or > 10 || request.EnergyLevelBaseline is < 1 or > 10 || request.MoodLevelBaseline is < 1 or > 10)
        {
            message = "Baseline mood, energy and stress must be between 1 and 10.";
            return false;
        }

        message = "";
        return true;
    }

    private static bool TryValidateCheckIn(UpsertMvpDailyCheckInRequest request, out string message)
    {
        if (request.MoodScore is < 1 or > 5)
        {
            message = "Mood score must be between 1 and 5.";
            return false;
        }
        if (request.EnergyLevel is < 1 or > 10)
        {
            message = "Energy level must be between 1 and 10.";
            return false;
        }
        if (request.StressLevel is < 1 or > 10)
        {
            message = "Stress level must be between 1 and 10.";
            return false;
        }
        if (request.JournalEntry?.Length > 1000)
        {
            message = "Journal entry must be 1000 characters or fewer.";
            return false;
        }

        message = "";
        return true;
    }

    private static CheckInEnergy ToLegacyEnergy(int energyLevel) => energyLevel switch
    {
        >= 8 => CheckInEnergy.High,
        >= 5 => CheckInEnergy.Balanced,
        >= 3 => CheckInEnergy.Low,
        _ => CheckInEnergy.Exhausted
    };

    private static int CalculateMvpBalance(int moodScore, int energyLevel, int stressLevel)
    {
        var mood = moodScore / 5m;
        var energy = energyLevel / 10m;
        var calm = (11 - stressLevel) / 10m;
        return (int)Math.Round((mood + energy + calm) / 3m * 100m, MidpointRounding.AwayFromZero);
    }

    private static int CurrentStreak(DailyCheckIn[] checkIns)
    {
        var dates = checkIns.Select(item => item.CheckInDate).ToHashSet();
        var cursor = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!dates.Contains(cursor))
        {
            cursor = cursor.AddDays(-1);
        }

        var streak = 0;
        while (dates.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }
}
