using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/api/users").RequireAuthorization();

        users.MapGet("/search", async (string? q, WithinDbContext db, PrivacyService privacy, ClaimsPrincipal principal) =>
        {
            var currentUserId = principal.UserId();
            var search = q?.Trim().ToLowerInvariant() ?? "";
            if (search.Length < 2) return Results.Ok(Array.Empty<UserSearchResultDto>());

            var candidates = await db.Users
                .Where(item => item.Id != currentUserId && item.DisplayName.ToLower().Contains(search))
                .OrderBy(item => item.DisplayName)
                .Take(20)
                .ToArrayAsync();

            var response = new List<UserSearchResultDto>(candidates.Length);
            foreach (var user in candidates)
            {
                if (await privacy.IsBlocked(currentUserId, user.Id)) continue;
                var connection = await db.Connections.FirstOrDefaultAsync(item =>
                    (item.RequesterUserId == currentUserId && item.ReceiverUserId == user.Id) ||
                    (item.RequesterUserId == user.Id && item.ReceiverUserId == currentUserId));
                if (connection?.Status == ConnectionStatus.Blocked) continue;
                response.Add(new UserSearchResultDto(
                    user.Id,
                    user.DisplayName,
                    user.Role,
                    connection?.Status,
                    connection?.RequesterUserId == currentUserId));
            }

            return Results.Ok(response.ToArray());
        });

        return app;
    }
}
