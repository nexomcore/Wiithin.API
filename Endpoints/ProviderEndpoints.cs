using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WithinAPI.Application;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class ProviderEndpoints
{
    public static IEndpointRouteBuilder MapProviderEndpoints(this IEndpointRouteBuilder app)
    {
        var providers = app.MapGroup("/api/providers");

        providers.MapGet("", async (
            WithinDbContext db,
            string? query,
            ProviderType? providerType,
            WithinLens? categoryGroup,
            WithinLens? lens,
            ProviderServiceDeliveryMode? deliveryMode,
            string? location,
            ProviderVerificationStatus? verificationStatus,
            string? sort) =>
        {
            return Results.Ok(await SearchProviders(db, query, providerType, categoryGroup ?? lens, deliveryMode, location, verificationStatus, sort));
        });

        providers.MapGet("/search", async (
            WithinDbContext db,
            string? query,
            ProviderType? providerType,
            WithinLens? categoryGroup,
            ProviderServiceDeliveryMode? deliveryMode,
            string? location,
            ProviderVerificationStatus? verificationStatus,
            string? sort) =>
        {
            return Results.Ok(await SearchProviders(db, query, providerType, categoryGroup, deliveryMode, location, verificationStatus, sort));
        });

        providers.MapGet("/me", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var provider = await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == principal.UserId());
            if (provider is null) return Results.Forbid();
            var serviceCount = await db.ProviderServices.CountAsync(item => item.ProviderId == provider.Id && item.IsActive);
            return Results.Ok(provider.ToDto(serviceCount, publicSafe: false));
        }).RequireAuthorization("ProviderOnly");

        providers.MapPut("/me", async (UpsertProviderDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var provider = await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == principal.UserId());
            if (provider is null) return Results.Forbid();
            ApplyProvider(request, provider, keepVerification: true);
            await db.SaveChangesAsync();
            var serviceCount = await db.ProviderServices.CountAsync(item => item.ProviderId == provider.Id && item.IsActive);
            return Results.Ok(provider.ToDto(serviceCount, publicSafe: false));
        }).RequireAuthorization("ProviderOnly");

        providers.MapGet("/me/events", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var provider = await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == userId);
            if (provider is null) return Results.Forbid();

            var events = await ApiMapping
                .ProjectEvents(db.Events.Where(item => item.ProviderId == provider.Id).OrderByDescending(item => item.StartUtc), db, userId)
                .ToArrayAsync();

            return Results.Ok(events);
        }).RequireAuthorization("ProviderOnly");

        providers.MapGet("/me/services", async (WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var provider = await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == principal.UserId());
            if (provider is null) return Results.Forbid();
            return Results.Ok(await db.ProviderServices
                .Where(item => item.ProviderId == provider.Id)
                .OrderByDescending(item => item.IsActive)
                .ThenBy(item => item.Name)
                .Select(item => item.ToDto())
                .ToArrayAsync());
        }).RequireAuthorization("ProviderOnly");

        providers.MapGet("/me/events/{eventId:guid}/engagement", async (Guid eventId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var provider = await db.Providers.FirstOrDefaultAsync(item => item.OwnerUserId == userId);
            if (provider is null) return Results.Forbid();

            var evt = await db.Events.FirstOrDefaultAsync(item => item.Id == eventId && item.ProviderId == provider.Id);
            if (evt is null) return Results.NotFound();

            var registrations = await (
                from registration in db.EventRegistrations
                join user in db.Users on registration.UserId equals user.Id
                where registration.EventId == eventId
                orderby registration.UpdatedUtc descending
                select new
                {
                    registration.State,
                    Participant = new ProviderEventParticipantDto(user.Id, user.DisplayName, registration.UpdatedUtc)
                }).ToArrayAsync();

            var saved = await (
                from savedEvent in db.SavedEvents
                join user in db.Users on savedEvent.UserId equals user.Id
                where savedEvent.EventId == eventId
                orderby savedEvent.CreatedUtc descending
                select new ProviderEventParticipantDto(user.Id, user.DisplayName, savedEvent.CreatedUtc)
            ).ToArrayAsync();

            var going = registrations.Where(item => item.State == EventJoinState.Going).Select(item => item.Participant).ToArray();
            var interested = registrations.Where(item => item.State == EventJoinState.Interested).Select(item => item.Participant).ToArray();
            var declined = registrations.Where(item => item.State == EventJoinState.Declined).Select(item => item.Participant).ToArray();

            return Results.Ok(new ProviderEventEngagementDto(
                evt.Id,
                evt.Title,
                going.Length,
                interested.Length,
                declined.Length,
                saved.Length,
                going,
                interested,
                declined,
                saved));
        }).RequireAuthorization("ProviderOnly");

        providers.MapGet("/{id:guid}", async (Guid id, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var provider = await db.Providers.FirstOrDefaultAsync(item => item.Id == id && item.IsActive);
            if (provider is null) return Results.NotFound();
            var userId = principal.TryUserId();
            return Results.Ok(await BuildProviderDetail(db, provider, userId));
        });

        providers.MapPost("", async (UpsertProviderDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var userId = principal.UserId();
            var provider = new Provider
            {
                Id = Guid.NewGuid(),
                OwnerUserId = userId,
                Slug = await CreateUniqueProviderSlug(db, request.Name),
                IsVerified = false,
                VerificationStatus = ProviderVerificationStatus.Unverified,
                CreatedUtc = DateTimeOffset.UtcNow,
                UpdatedUtc = DateTimeOffset.UtcNow
            };
            ApplyProvider(request, provider, keepVerification: true);
            db.Providers.Add(provider);
            await db.SaveChangesAsync();
            return Results.Created($"/api/providers/{provider.Id}", provider.ToDto(publicSafe: false));
        }).RequireAuthorization();

        providers.MapPut("/{id:guid}", async (Guid id, UpsertProviderDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var provider = await db.Providers.FindAsync(id);
            if (provider is null) return Results.NotFound();
            if (provider.OwnerUserId != principal.UserId()) return Results.Forbid();
            ApplyProvider(request, provider, keepVerification: true);
            await db.SaveChangesAsync();
            var serviceCount = await db.ProviderServices.CountAsync(item => item.ProviderId == provider.Id && item.IsActive);
            return Results.Ok(provider.ToDto(serviceCount, publicSafe: false));
        }).RequireAuthorization();

        providers.MapGet("/{providerId:guid}/services", async (Guid providerId, WithinDbContext db) =>
        {
            if (!await db.Providers.AnyAsync(item => item.Id == providerId && item.IsActive)) return Results.NotFound();
            return Results.Ok(await db.ProviderServices
                .Where(item => item.ProviderId == providerId && item.IsActive)
                .OrderBy(item => item.Name)
                .Select(item => item.ToDto())
                .ToArrayAsync());
        });

        providers.MapPost("/{providerId:guid}/services", async (Guid providerId, UpsertProviderServiceDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var provider = await db.Providers.FindAsync(providerId);
            if (provider is null) return Results.NotFound();
            if (provider.OwnerUserId != principal.UserId()) return Results.Forbid();
            var validation = ValidateService(request);
            if (validation is not null) return Results.BadRequest(new { message = validation });
            var now = DateTimeOffset.UtcNow;
            var service = new ProviderService { Id = Guid.NewGuid(), ProviderId = providerId, CreatedUtc = now, UpdatedUtc = now };
            ApplyService(request, service);
            db.ProviderServices.Add(service);
            await db.SaveChangesAsync();
            return Results.Created($"/api/provider-services/{service.Id}", service.ToDto());
        }).RequireAuthorization("ProviderOnly");

        app.MapPut("/api/provider-services/{serviceId:guid}", async (Guid serviceId, UpsertProviderServiceDto request, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var service = await db.ProviderServices.FindAsync(serviceId);
            if (service is null) return Results.NotFound();
            if (!await OwnsProvider(db, principal.UserId(), service.ProviderId)) return Results.Forbid();
            var validation = ValidateService(request);
            if (validation is not null) return Results.BadRequest(new { message = validation });
            ApplyService(request, service);
            service.UpdatedUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(service.ToDto());
        }).RequireAuthorization("ProviderOnly");

        app.MapDelete("/api/provider-services/{serviceId:guid}", async (Guid serviceId, WithinDbContext db, ClaimsPrincipal principal) =>
        {
            var service = await db.ProviderServices.FindAsync(serviceId);
            if (service is null) return Results.NotFound();
            if (!await OwnsProvider(db, principal.UserId(), service.ProviderId)) return Results.Forbid();
            service.IsActive = false;
            service.UpdatedUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("ProviderOnly");

        return app;
    }

    private static async Task<ProviderDto[]> SearchProviders(
        WithinDbContext db,
        string? query,
        ProviderType? providerType,
        WithinLens? lens,
        ProviderServiceDeliveryMode? deliveryMode,
        string? location,
        ProviderVerificationStatus? verificationStatus,
        string? sort)
    {
        var providers = db.Providers.Where(item => item.IsActive);
        if (providerType is not null) providers = providers.Where(item => item.ProviderType == providerType);
        if (lens is not null) providers = providers.Where(item => item.Lens == lens);
        if (verificationStatus is not null) providers = providers.Where(item => item.VerificationStatus == verificationStatus);
        if (!string.IsNullOrWhiteSpace(location))
        {
            var locationTerm = location.Trim().ToLowerInvariant();
            var locationPattern = $"%{locationTerm}%";
            providers = providers.Where(item =>
                EF.Functions.ILike(item.Location, locationPattern) ||
                (item.Suburb != null && EF.Functions.ILike(item.Suburb, locationPattern)) ||
                (item.City != null && EF.Functions.ILike(item.City, locationPattern)));
        }

        if (deliveryMode is not null)
        {
            providers = providers.Where(item => db.ProviderServices.Any(service =>
                service.ProviderId == item.Id &&
                service.IsActive &&
                (service.DeliveryMode == deliveryMode || service.DeliveryMode == ProviderServiceDeliveryMode.Hybrid)));
        }

        providers = sort?.Trim().ToLowerInvariant() switch
        {
            "alphabetical" => providers.OrderBy(item => item.Name),
            "recent" or "recentlyadded" => providers.OrderByDescending(item => item.CreatedUtc),
            _ => providers
                .OrderByDescending(item => item.VerificationStatus == ProviderVerificationStatus.Verified)
                .ThenBy(item => item.Name)
        };

        var providerItems = await providers.Take(250).ToArrayAsync();
        var providerIds = providerItems.Select(item => item.Id).ToArray();
        var activeServices = await db.ProviderServices
            .Where(item => providerIds.Contains(item.ProviderId) && item.IsActive)
            .ToArrayAsync();
        if (!string.IsNullOrWhiteSpace(query))
        {
            providerItems = providerItems
                .Where(item => ProviderRules.MatchesSearch(item, activeServices, query))
                .Take(100)
                .ToArray();
            providerIds = providerItems.Select(item => item.Id).ToArray();
            activeServices = activeServices.Where(item => providerIds.Contains(item.ProviderId)).ToArray();
        }

        var serviceCounts = activeServices
            .GroupBy(item => item.ProviderId)
            .ToDictionary(group => group.Key, group => group.Count());

        return providerItems.Select(item => item.ToDto(serviceCounts.GetValueOrDefault(item.Id))).ToArray();
    }

    private static async Task<ProviderDetailDto> BuildProviderDetail(WithinDbContext db, Provider provider, Guid? userId)
    {
        var services = await db.ProviderServices
            .Where(item => item.ProviderId == provider.Id && item.IsActive)
            .OrderBy(item => item.Name)
            .Select(item => item.ToDto())
            .ToArrayAsync();
        var events = await ApiMapping.ProjectEvents(
                db.Events.Where(item => item.ProviderId == provider.Id && item.Status == EventStatus.Published && item.StartUtc >= DateTimeOffset.UtcNow).OrderBy(item => item.StartUtc),
                db,
                userId)
            .Take(12)
            .ToArrayAsync();
        var communities = await ApiMapping.ProjectCommunities(db.Communities.Where(item => item.ProviderId == provider.Id).OrderBy(item => item.Name), db, userId).ToArrayAsync();
        var ownerUserId = provider.OwnerUserId;
        var circles = await db.Circles
            .Where(circle =>
                circle.Status == CircleStatus.Active &&
                circle.Visibility != CircleVisibility.Hidden &&
                (db.CircleMembers.Any(member => member.CircleId == circle.Id && member.UserId == ownerUserId && member.Status == CircleMemberStatus.Active && (member.Role == CircleMemberRole.Admin || member.Role == CircleMemberRole.Moderator)) ||
                 db.CircleEvents.Any(shared => shared.CircleId == circle.Id && shared.Status == CircleEventStatus.Active && db.Events.Any(evt => evt.Id == shared.EventId && evt.ProviderId == provider.Id))))
            .OrderBy(circle => circle.Name)
            .Select(circle => new CircleDto(
                circle.Id,
                circle.Name,
                circle.Slug,
                circle.Description,
                circle.Rules,
                circle.CreatedByUserId,
                circle.Type,
                circle.Visibility,
                circle.Status,
                circle.Lens,
                db.CircleMembers.Count(member => member.CircleId == circle.Id && member.Status == CircleMemberStatus.Active),
                db.CircleThreads.Count(thread => thread.CircleId == circle.Id && thread.Status == CommunityContentStatus.Active),
                db.CircleEvents.Count(evt => evt.CircleId == circle.Id && evt.Status == CircleEventStatus.Active),
                userId != null && db.CircleMembers.Any(member => member.CircleId == circle.Id && member.UserId == userId && member.Status == CircleMemberStatus.Active),
                userId != null && db.CircleMembers.Any(member => member.CircleId == circle.Id && member.UserId == userId && member.Status == CircleMemberStatus.Pending),
                userId == null ? null : db.CircleMembers.Where(member => member.CircleId == circle.Id && member.UserId == userId && member.Status == CircleMemberStatus.Active).Select(member => (CircleMemberRole?)member.Role).FirstOrDefault(),
                userId != null && db.CircleMembers.Any(member => member.CircleId == circle.Id && member.UserId == userId && member.Status == CircleMemberStatus.Active && member.Role == CircleMemberRole.Admin),
                circle.AllowAnonymousPosts))
            .ToArrayAsync();
        return new ProviderDetailDto(provider.ToDto(services.Length), services, events, communities, circles);
    }

    private static void ApplyProvider(UpsertProviderDto request, Provider provider, bool keepVerification)
    {
        provider.Name = request.Name.Trim();
        provider.Bio = request.Bio.Trim();
        provider.Lens = request.Lens;
        provider.ProviderType = request.ProviderType;
        provider.LegalName = CleanOptional(request.LegalName);
        provider.Categories = Clean(request.Categories);
        provider.ProfileImageUrl = CleanOptional(request.ProfileImageUrl);
        provider.CoverImageUrl = CleanOptional(request.CoverImageUrl);
        provider.Location = request.Location.Trim();
        provider.Suburb = CleanOptional(request.Suburb);
        provider.City = CleanOptional(request.City);
        provider.State = CleanOptional(request.State);
        provider.Country = CleanOptional(request.Country);
        provider.WebsiteUrl = CleanOptional(request.WebsiteUrl);
        provider.InstagramUrl = CleanOptional(request.InstagramUrl);
        provider.Phone = CleanOptional(request.Phone);
        provider.Email = CleanOptional(request.Email);
        provider.ShowEmailPublicly = request.ShowEmailPublicly;
        provider.ShowPhonePublicly = request.ShowPhonePublicly;
        provider.ShowWebsitePublicly = request.ShowWebsitePublicly;
        provider.PractitionerTitle = CleanOptional(request.PractitionerTitle);
        provider.YearsExperience = request.YearsExperience is < 0 ? null : request.YearsExperience;
        provider.Qualifications = CleanOptional(request.Qualifications);
        provider.ServicesOffered = Clean(request.ServicesOffered);
        provider.Languages = Clean(request.Languages);
        provider.OnlineAvailable = request.OnlineAvailable;
        provider.InPersonAvailable = request.InPersonAvailable;
        provider.BusinessType = CleanOptional(request.BusinessType);
        provider.Abn = CleanOptional(request.Abn);
        provider.Facilities = Clean(request.Facilities);
        provider.AccessibilityFeatures = Clean(request.AccessibilityFeatures);
        provider.TeamMembers = Clean(request.TeamMembers);
        provider.OpeningHours = CleanOptional(request.OpeningHours);
        provider.IsActive = request.IsActive;
        provider.UpdatedUtc = DateTimeOffset.UtcNow;
        if (!keepVerification)
        {
            provider.IsVerified = false;
            provider.VerificationStatus = ProviderVerificationStatus.Unverified;
        }
    }

    private static void ApplyService(UpsertProviderServiceDto request, ProviderService service)
    {
        service.Name = request.Name.Trim();
        service.Description = request.Description.Trim();
        service.Lens = request.Lens;
        service.Category = request.Category.Trim();
        service.DurationMinutes = request.DurationMinutes is <= 0 ? null : request.DurationMinutes;
        service.PriceType = request.PriceType;
        service.PriceAmount = request.PriceType is ProviderPriceType.Free or ProviderPriceType.ContactProvider ? null : request.PriceAmount;
        service.DeliveryMode = request.DeliveryMode;
        service.Location = CleanOptional(request.Location);
        service.IsActive = request.IsActive;
    }

    private static string? ValidateService(UpsertProviderServiceDto request)
    {
        return ProviderRules.CanSaveService(
            request.Name,
            request.Description,
            request.Category,
            request.DurationMinutes,
            request.PriceAmount,
            request.PriceType,
            out var message)
            ? null
            : message;
    }

    private static async Task<bool> OwnsProvider(WithinDbContext db, Guid userId, Guid providerId) =>
        await db.Providers.AnyAsync(item => item.Id == providerId && item.OwnerUserId == userId);

    private static string[] Clean(string[]? values) =>
        values is null ? [] : values.Select(item => item.Trim()).Where(item => item.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static string? CleanOptional(string? value)
    {
        var clean = value?.Trim();
        return string.IsNullOrWhiteSpace(clean) ? null : clean;
    }

    private static async Task<string> CreateUniqueProviderSlug(WithinDbContext db, string providerName)
    {
        var baseSlug = Slugs.From(providerName);
        var slug = baseSlug;
        var suffix = 2;
        while (await db.Providers.AnyAsync(item => item.Slug == slug))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }
}
