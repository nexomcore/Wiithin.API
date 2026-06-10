using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class HomeEndpoints
{
    public static IEndpointRouteBuilder MapHomeEndpoints(this IEndpointRouteBuilder app)
    {
        var home = app.MapGroup("/api/home").RequireAuthorization();

        home.MapGet("", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var user = await db.Users.FindAsync(userId);
            if (user is null) return Results.Unauthorized();
            var firstName = await db.UserWellbeingProfiles
                .Where(item => item.UserId == userId)
                .Select(item => item.FirstName)
                .FirstOrDefaultAsync();

            var interests = await db.UserWellbeingInterests
                .Where(item => item.UserId == userId)
                .Select(item => item.InterestKey)
                .ToArrayAsync();
            var goals = await db.UserWellbeingGoals
                .Where(item => item.UserId == userId)
                .Select(item => item.GoalKey)
                .ToArrayAsync();
            var recommendationCategories = WellbeingRecommendationRules.BuildRecommendedCategories(interests, goals);

            var candidateEvents = await ApiMapping.ProjectEvents(
                    db.Events
                        .Where(item => item.Status == EventStatus.Published)
                        .OrderBy(item => item.StartUtc),
                    db,
                    userId)
                .Take(20)
                .ToArrayAsync();
            var recommended = candidateEvents
                .OrderByDescending(item => RecommendationScore(item.Tags, item.Title, item.Description, recommendationCategories))
                .ThenBy(item => item.StartUtc)
                .Take(5)
                .ToArray();
            var communities = Array.Empty<CommunityDto>();
            var upcoming = await ApiMapping.ProjectEvents(
                    from evt in db.Events
                    join reg in db.EventRegistrations on evt.Id equals reg.EventId
                    where reg.UserId == userId && reg.State == EventJoinState.Going
                    orderby evt.StartUtc
                    select evt,
                    db,
                    userId)
                .Take(3)
                .ToArrayAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var todayCheckInEntity = await db.DailyCheckIns
                .FirstOrDefaultAsync(item => item.UserId == userId && item.CheckInDate == today);

            return Results.Ok(new HomeDashboardDto(
                user.ToDto(),
                string.IsNullOrWhiteSpace(firstName) ? null : firstName,
                todayCheckInEntity?.ToDto(),
                recommended,
                communities,
                $"Choose one {user.PreferredLens} action that supports your wellbeing today.",
                upcoming));
        });

        return app;
    }

    private static int RecommendationScore(string[] tags, string title, string description, string[] categories)
    {
        if (categories.Length == 0) return 0;
        var score = 0;
        var text = $"{title} {description}".ToLowerInvariant();
        foreach (var category in categories)
        {
            if (tags.Any(tag => tag.Equals(category, StringComparison.OrdinalIgnoreCase))) score += 3;
            if (text.Contains(category.Replace('_', ' '), StringComparison.OrdinalIgnoreCase)) score += 1;
        }

        return score;
    }
}
