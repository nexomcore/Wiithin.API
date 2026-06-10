using Microsoft.EntityFrameworkCore;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("/register", async (RegisterDto request, WithinDbContext db, AuthTokenService tokens) =>
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (await db.Users.AnyAsync(user => user.Email == email))
            {
                return Results.Conflict(new { message = "Email is already registered." });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                DisplayName = ResolveInitialDisplayName(request.DisplayName, email),
                Email = email,
                PasswordHash = Passwords.Hash(request.Password),
                Role = request.Role,
                CreatedUtc = DateTimeOffset.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Results.Ok(await tokens.CreateResponse(user));
        });

        auth.MapPost("/login", async (LoginDto request, WithinDbContext db, AuthTokenService tokens) =>
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(item => item.Email == email);
            if (user is null || user.IsDeleted || !Passwords.Verify(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(await tokens.CreateResponse(user));
        });

        return app;
    }

    // Signup collects only email + password; the user sets their real names during onboarding.
    // Until then, default the display name to the email's local part so it is never blank.
    private static string ResolveInitialDisplayName(string? displayName, string email)
    {
        var trimmed = displayName?.Trim();
        if (!string.IsNullOrWhiteSpace(trimmed)) return trimmed;
        var localPart = email.Split('@')[0].Trim();
        return string.IsNullOrWhiteSpace(localPart) ? "New member" : localPart;
    }
}
