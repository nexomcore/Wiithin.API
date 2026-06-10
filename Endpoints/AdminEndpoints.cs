using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/admin");

        admin.MapPost("/login", async (LoginDto request, WithinDbContext db, AuthTokenService tokens) =>
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(item => item.Email == email);
            if (user is null || user.IsDeleted || !Passwords.Verify(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            if (user.Role != WithinRole.Admin)
            {
                return Results.Json(new { message = "Admin access required." }, statusCode: StatusCodes.Status403Forbidden);
            }

            return Results.Ok(await tokens.CreateResponse(user));
        });

        admin.MapGet("/submissions", async (IMarketFitSubmissionService service, string? audience, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetSubmissionsAsync(audience, cancellationToken))).RequireAuthorization("AdminOnly");

        admin.MapGet("/stats", async (IMarketFitSubmissionService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetStatsAsync(cancellationToken))).RequireAuthorization("AdminOnly");

        admin.MapGet("/users", async (WithinDbContext db) =>
        {
            var users = await db.Users
                .OrderByDescending(item => item.CreatedUtc)
                .Select(item => new AdminUserDto(item.Id, item.DisplayName, item.Email, item.Role, item.CreatedUtc))
                .ToArrayAsync();
            return Results.Ok(users);
        }).RequireAuthorization("AdminOnly");

        admin.MapDelete("/submissions/{id:guid}", async (Guid id, IMarketFitSubmissionService service, CancellationToken cancellationToken) =>
            await service.DeleteSubmissionAsync(id, cancellationToken) ? Results.NoContent() : Results.NotFound()).RequireAuthorization("AdminOnly");

        admin.MapDelete("/users/{id:guid}", async (Guid id, WithinDbContext db, AccountDeletionService deletion) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(item => item.Id == id);
            if (user is null || user.IsDeleted) return Results.NotFound();
            if (user.Role == WithinRole.Admin)
            {
                var activeAdmins = await db.Users.CountAsync(item => item.Role == WithinRole.Admin && !item.IsDeleted);
                if (activeAdmins <= 1)
                {
                    return Results.Json(new { message = "Cannot delete the last remaining admin account." }, statusCode: StatusCodes.Status409Conflict);
                }
            }

            var result = await deletion.DeleteAccountAsync(id);
            return result.Status switch
            {
                AccountDeletionStatus.Deleted => Results.NoContent(),
                AccountDeletionStatus.Blocked => Results.Json(new { message = result.Message }, statusCode: StatusCodes.Status409Conflict),
                _ => Results.NotFound()
            };
        }).RequireAuthorization("AdminOnly");

        return app;
    }
}
