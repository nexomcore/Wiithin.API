namespace WithinAPI.Endpoints;

public static class EndpointRegistrationExtensions
{
    public static IEndpointRouteBuilder MapWithinApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthEndpoints();
        app.MapAuthEndpoints();
        app.MapUserEndpoints();
        app.MapUserPrivacyEndpoints();
        app.MapConnectionEndpoints();
        app.MapProfilePreviewEndpoints();
        app.MapAdminEndpoints();
        app.MapMarketFitEndpoints();
        app.MapProviderApplicationEndpoints();
        app.MapHomeEndpoints();
        app.MapProviderEndpoints();
        app.MapEventEndpoints();
        app.MapCircleEndpoints();
        app.MapCommunityEndpoints();
        app.MapPostEndpoints();
        app.MapNotificationEndpoints();
        app.MapWellbeingEndpoints();
        app.MapHabitEndpoints();
        return app;
    }
}

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", () => Results.Ok(new
        {
            status = "ok",
            service = "WithinAPI",
            utc = DateTimeOffset.UtcNow
        }));

        return app;
    }
}
