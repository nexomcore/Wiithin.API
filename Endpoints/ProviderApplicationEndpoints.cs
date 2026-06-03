using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;

namespace WithinAPI.Endpoints;

public static class ProviderApplicationEndpoints
{
    public static IEndpointRouteBuilder MapProviderApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var applications = app.MapGroup("/api/provider-applications");

        applications.MapPost("", async (CreateProviderApplicationDto request, WithinDbContext db) =>
        {
            var validation = Validate(request);
            if (validation is not null) return validation;

            var now = DateTimeOffset.UtcNow;
            var application = new ProviderApplication
            {
                Id = Guid.NewGuid(),
                Status = ProviderApplicationStatus.Submitted,
                ProviderCategory = request.ProviderCategory,
                PrimaryLens = request.PrimaryLens,
                ServiceAreas = Clean(request.ServiceAreas),
                ContactName = request.ContactName.Trim(),
                ContactEmail = request.ContactEmail.Trim().ToLowerInvariant(),
                ContactPhone = request.ContactPhone.Trim(),
                PreferredContactMethod = request.PreferredContactMethod.Trim(),
                ProviderName = request.ProviderName.Trim(),
                BusinessType = request.BusinessType.Trim(),
                Abn = CleanOptional(request.Abn),
                WebsiteUrl = CleanOptional(request.WebsiteUrl),
                InstagramUrl = CleanOptional(request.InstagramUrl),
                OtherSocialUrl = CleanOptional(request.OtherSocialUrl),
                Location = request.Location.Trim(),
                DeliveryModes = Clean(request.DeliveryModes),
                VenueNames = CleanOptional(request.VenueNames),
                ServicesOffered = Clean(request.ServicesOffered),
                YearsPracticing = request.YearsPracticing.Trim(),
                TypicalAudience = request.TypicalAudience.Trim(),
                Bio = request.Bio.Trim(),
                JoinReason = request.JoinReason.Trim(),
                Certifications = request.Certifications.Trim(),
                InsuranceStatus = request.InsuranceStatus.Trim(),
                WorkingWithChildrenCheck = request.WorkingWithChildrenCheck.Trim(),
                FirstAidCpr = request.FirstAidCpr.Trim(),
                ProfessionalMemberships = CleanOptional(request.ProfessionalMemberships),
                CredentialLinks = CleanOptional(request.CredentialLinks),
                HasEventsReady = request.HasEventsReady.Trim(),
                ExpectedFirstEvent = request.ExpectedFirstEvent.Trim(),
                BookingTools = request.BookingTools.Trim(),
                AdminFacingNotes = CleanOptional(request.AdminFacingNotes),
                DeclarationAccepted = request.DeclarationAccepted,
                SubmittedUtc = now,
                UpdatedUtc = now
            };

            db.ProviderApplications.Add(application);
            await db.SaveChangesAsync();
            return Results.Created($"/api/provider-applications/{application.Id}", application.ToDto());
        });

        var admin = app.MapGroup("/api/admin/provider-applications").RequireAuthorization("AdminOnly");

        admin.MapGet("", async (
            WithinDbContext db,
            ProviderApplicationStatus? status,
            string? search) =>
        {
            var query = db.ProviderApplications.AsQueryable();
            if (status is not null) query = query.Where(item => item.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                query = query.Where(item =>
                    item.ProviderName.ToLower().Contains(term) ||
                    item.ContactEmail.ToLower().Contains(term) ||
                    item.ContactName.ToLower().Contains(term) ||
                    item.Location.ToLower().Contains(term));
            }

            var result = await query
                .OrderByDescending(item => item.SubmittedUtc)
                .Select(item => item.ToDto())
                .ToArrayAsync();

            return Results.Ok(result);
        });

        admin.MapGet("/{id:guid}", async (Guid id, WithinDbContext db) =>
        {
            var application = await db.ProviderApplications.FindAsync(id);
            return application is null ? Results.NotFound() : Results.Ok(application.ToDto());
        });

        admin.MapPost("/{id:guid}/status", async (Guid id, ProviderApplicationStatusUpdateDto request, WithinDbContext db) =>
        {
            var application = await db.ProviderApplications.FindAsync(id);
            if (application is null) return Results.NotFound();

            var reason = request.Reason?.Trim() ?? "";
            if ((request.Status is ProviderApplicationStatus.Rejected or ProviderApplicationStatus.MoreInfoRequested) && reason.Length == 0)
            {
                return Results.BadRequest(new { message = "A reason is required for rejected or more-info decisions." });
            }

            application.Status = request.Status;
            application.ReviewDecisionReason = reason;
            application.UpdatedUtc = DateTimeOffset.UtcNow;
            application.ReviewedUtc = request.Status is ProviderApplicationStatus.Approved or ProviderApplicationStatus.Rejected
                ? application.UpdatedUtc
                : null;

            await db.SaveChangesAsync();
            return Results.Ok(application.ToDto());
        });

        admin.MapPost("/{id:guid}/notes", async (Guid id, ProviderApplicationNotesDto request, WithinDbContext db) =>
        {
            var application = await db.ProviderApplications.FindAsync(id);
            if (application is null) return Results.NotFound();

            application.AdminNotes = request.AdminNotes.Trim();
            application.UpdatedUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(application.ToDto());
        });

        return app;
    }

    private static IResult? Validate(CreateProviderApplicationDto request)
    {
        var missing = new List<string>();
        AddMissing(missing, request.ContactName, "contactName");
        AddMissing(missing, request.ContactEmail, "contactEmail");
        AddMissing(missing, request.ProviderName, "providerName");
        AddMissing(missing, request.BusinessType, "businessType");
        AddMissing(missing, request.Location, "location");
        AddMissing(missing, request.YearsPracticing, "yearsPracticing");
        AddMissing(missing, request.TypicalAudience, "typicalAudience");
        AddMissing(missing, request.Bio, "bio");
        AddMissing(missing, request.JoinReason, "joinReason");
        AddMissing(missing, request.Certifications, "certifications");
        AddMissing(missing, request.InsuranceStatus, "insuranceStatus");
        AddMissing(missing, request.WorkingWithChildrenCheck, "workingWithChildrenCheck");
        AddMissing(missing, request.FirstAidCpr, "firstAidCpr");
        AddMissing(missing, request.HasEventsReady, "hasEventsReady");
        AddMissing(missing, request.ExpectedFirstEvent, "expectedFirstEvent");
        AddMissing(missing, request.BookingTools, "bookingTools");

        if (request.ServiceAreas.Length == 0) missing.Add("serviceAreas");
        if (request.DeliveryModes.Length == 0) missing.Add("deliveryModes");
        if (request.ServicesOffered.Length == 0) missing.Add("servicesOffered");
        if (!request.DeclarationAccepted) missing.Add("declarationAccepted");

        return missing.Count == 0 ? null : Results.BadRequest(new { message = "Required fields are missing.", fields = missing });
    }

    private static void AddMissing(List<string> missing, string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) missing.Add(field);
    }

    private static string[] Clean(string[] values) =>
        values.Select(item => item.Trim()).Where(item => item.Length > 0).Distinct().ToArray();

    private static string? CleanOptional(string? value)
    {
        var clean = value?.Trim();
        return string.IsNullOrWhiteSpace(clean) ? null : clean;
    }
}
