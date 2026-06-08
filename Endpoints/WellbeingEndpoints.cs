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
            saved.Energy = energy;
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
}
