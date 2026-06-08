using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;

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

            var recommended = await ApiMapping.ProjectEvents(
                    db.Events
                        .Where(item => item.Status == EventStatus.Published)
                        .OrderBy(item => item.StartUtc),
                    db,
                    userId)
                .Take(5)
                .ToArrayAsync();
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
                todayCheckInEntity?.ToDto(),
                recommended,
                communities,
                $"Choose one {user.PreferredLens} action that supports your wellbeing today.",
                upcoming));
        });

        return app;
    }
}
