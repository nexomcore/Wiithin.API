using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
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

        // Self-service account deletion. Requires the current password (re-authentication).
        users.MapDelete("/me", async ([FromBody] DeleteAccountRequest request, WithinDbContext db, AccountDeletionService deletion, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var user = await db.Users.FirstOrDefaultAsync(item => item.Id == userId);
            if (user is null || user.IsDeleted) return Results.NotFound();
            if (string.IsNullOrEmpty(request.Password) || !Passwords.Verify(request.Password, user.PasswordHash))
            {
                return Results.Json(new { message = "Password is incorrect." }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await deletion.DeleteAccountAsync(userId);
            return result.Status switch
            {
                AccountDeletionStatus.Deleted => Results.NoContent(),
                AccountDeletionStatus.Blocked => Results.Json(new { message = result.Message }, statusCode: StatusCodes.Status409Conflict),
                _ => Results.NotFound()
            };
        });

        return app;
    }
}
