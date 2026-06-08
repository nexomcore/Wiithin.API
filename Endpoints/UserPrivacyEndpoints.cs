using System.Security.Claims;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class UserPrivacyEndpoints
{
    public static IEndpointRouteBuilder MapUserPrivacyEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/api/users/me").RequireAuthorization();

        users.MapGet("/privacy-settings", async (PrivacyService privacy, ClaimsPrincipal principal) =>
        {
            var settings = await privacy.GetOrCreateSettings(principal.UserId());
            return Results.Ok(ToDto(settings));
        });

        users.MapPut("/privacy-settings", async (UserPrivacySettingsDto request, PrivacyService privacy, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var settings = await privacy.GetOrCreateSettings(principal.UserId());
            settings.ProfileVisibility = request.ProfileVisibility;
            settings.DefaultRsvpVisibility = request.DefaultRsvpVisibility;
            settings.TaggingPermission = request.TaggingPermission;
            settings.FriendRequestPermission = request.FriendRequestPermission;
            settings.ShowActivityToFriends = request.ShowActivityToFriends;
            settings.AllowEventInviteFromFriends = request.AllowEventInviteFromFriends;
            settings.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(settings));
        });

        return app;
    }

    private static UserPrivacySettingsDto ToDto(Domain.UserPrivacySettings settings) => new(
        settings.ProfileVisibility,
        settings.DefaultRsvpVisibility,
        settings.TaggingPermission,
        settings.FriendRequestPermission,
        settings.ShowActivityToFriends,
        settings.AllowEventInviteFromFriends);
}
