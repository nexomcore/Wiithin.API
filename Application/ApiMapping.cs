using System.Security.Claims;
using WithinAPI.Data;
using WithinAPI.Domain;
using WithinAPI.Models;

namespace WithinAPI.Application;

public static class ApiMapping
{
    private static readonly HashSet<string> IntensityOptions = new(StringComparer.OrdinalIgnoreCase) { "low", "medium", "high" };
    private static readonly HashSet<string> ExperienceLevelOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "beginner_friendly",
        "some_experience_recommended",
        "experienced_participants_only"
    };
    private static readonly HashSet<string> AgeRestrictionOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "all_ages",
        "13_plus",
        "16_plus",
        "18_plus",
        "seniors_focused",
        "family_kids_friendly"
    };

    public static Guid UserId(this ClaimsPrincipal principal) =>
        Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User id claim missing."));

    public static Guid? TryUserId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;

    public static UserSummaryDto ToDto(this User user) => new(user.Id, user.DisplayName, user.Email, user.Role, user.PreferredLens);

    public static DailyCheckInDto ToDto(this DailyCheckIn checkIn) => new()
    {
        Id = checkIn.Id.ToString(),
        CheckInDate = checkIn.CheckInDate.ToString("yyyy-MM-dd"),
        MoodScore = checkIn.MoodScore,
        EnergyScore = checkIn.EnergyScore,
        StressScore = checkIn.StressScore,
        ConnectionScore = checkIn.ConnectionScore,
        MeaningScore = checkIn.MeaningScore,
        Tags = checkIn.Tags,
        Note = checkIn.Note,
        DailyBalanceScore = checkIn.DailyBalanceScore
    };

    public static ProviderDto ToDto(this Provider provider) => new(
        provider.Id,
        provider.Name,
        provider.Slug,
        provider.Bio,
        provider.Lens,
        provider.Location,
        provider.WebsiteUrl,
        provider.InstagramUrl,
        provider.IsVerified);

    public static ProviderApplicationDto ToDto(this ProviderApplication application, string? temporaryPassword = null) => new(
        application.Id,
        application.Status,
        application.ProviderCategory,
        application.PrimaryLens,
        application.ServiceAreas,
        application.ContactName,
        application.ContactEmail,
        application.ContactPhone,
        application.PreferredContactMethod,
        application.ProviderName,
        application.BusinessType,
        application.Abn,
        application.WebsiteUrl,
        application.InstagramUrl,
        application.OtherSocialUrl,
        application.Location,
        application.DeliveryModes,
        application.VenueNames,
        application.ServicesOffered,
        application.YearsPracticing,
        application.TypicalAudience,
        application.Bio,
        application.JoinReason,
        application.Certifications,
        application.InsuranceStatus,
        application.WorkingWithChildrenCheck,
        application.FirstAidCpr,
        application.ProfessionalMemberships,
        application.CredentialLinks,
        application.HasEventsReady,
        application.ExpectedFirstEvent,
        application.BookingTools,
        application.AdminFacingNotes,
        application.DeclarationAccepted,
        application.AdminNotes,
        application.ReviewDecisionReason,
        application.SubmittedUtc,
        application.UpdatedUtc,
        application.ReviewedUtc,
        application.ApprovedProviderId,
        temporaryPassword);

    public static Event ToEntity(this UpsertEventDto request, Guid providerId)
    {
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            CreatedUtc = DateTimeOffset.UtcNow,
            Status = EventStatus.Published
        };
        return request.ApplyTo(evt);
    }

    public static Event ApplyTo(this UpsertEventDto request, Event evt)
    {
        evt.Title = request.Title.Trim();
        evt.Description = request.Description.Trim();
        evt.Lens = request.Lens;
        evt.LocationName = request.LocationName.Trim();
        evt.IsOnline = request.IsOnline;
        evt.StartUtc = request.StartUtc;
        evt.EndUtc = request.EndUtc;
        evt.PriceAmount = request.PriceAmount;
        evt.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "AUD" : request.Currency.Trim().ToUpperInvariant();
        evt.Capacity = request.Capacity;
        evt.SignupType = request.SignupType;
        evt.ExternalBookingUrl = request.ExternalBookingUrl;
        evt.ImageUrl = request.ImageUrl;
        evt.Tags = NormalizeList(request.Tags);
        evt.BringItems = NormalizeList(request.BringItems);
        evt.BringNotes = NormalizeNullable(request.BringNotes);
        evt.Facilities = NormalizeList(request.Facilities);
        evt.AccessibilityFeatures = NormalizeList(request.AccessibilityFeatures);
        evt.PhysicalIntensity = NormalizeSingle(request.PhysicalIntensity);
        evt.SocialInteractionLevel = NormalizeSingle(request.SocialInteractionLevel);
        evt.ExperienceLevel = NormalizeSingle(request.ExperienceLevel);
        evt.AtmosphereTags = NormalizeList(request.AtmosphereTags);
        evt.FoodProvided = request.FoodProvided;
        evt.DrinksProvided = request.DrinksProvided;
        evt.DietaryOptions = NormalizeList(request.DietaryOptions);
        evt.FoodNotes = NormalizeNullable(request.FoodNotes);
        evt.AgeRestriction = NormalizeSingle(request.AgeRestriction);
        evt.SafetyNotes = NormalizeNullable(request.SafetyNotes);
        return evt;
    }

    public static bool TryValidate(this UpsertEventDto request, out string message)
    {
        if (!IsAllowed(request.PhysicalIntensity, IntensityOptions))
        {
            message = "Physical intensity must be low, medium, or high.";
            return false;
        }

        if (!IsAllowed(request.SocialInteractionLevel, IntensityOptions))
        {
            message = "Social interaction level must be low, medium, or high.";
            return false;
        }

        if (!IsAllowed(request.ExperienceLevel, ExperienceLevelOptions))
        {
            message = "Experience level is not a supported option.";
            return false;
        }

        if (!IsAllowed(request.AgeRestriction, AgeRestrictionOptions))
        {
            message = "Age restriction is not a supported option.";
            return false;
        }

        message = "";
        return true;
    }

    public static decimal Average(DailyCheckIn[] items, Func<DailyCheckIn, int> selector) =>
        Math.Round(items.Average(item => (decimal)selector(item)), 1, MidpointRounding.AwayFromZero);

    public static IQueryable<EventDto> ProjectEvents(IQueryable<Event> query, WithinDbContext db, Guid? userId) =>
        from evt in query
        join provider in db.Providers on evt.ProviderId equals provider.Id
        select new EventDto(
            evt.Id,
            evt.ProviderId,
            provider.Name,
            evt.Title,
            evt.Description,
            evt.Lens,
            evt.LocationName,
            evt.IsOnline,
            evt.StartUtc,
            evt.EndUtc,
            evt.PriceAmount,
            evt.Currency,
            evt.Capacity,
            db.EventRegistrations.Count(reg => reg.EventId == evt.Id && reg.State == EventJoinState.Going),
            userId != null && db.SavedEvents.Any(saved => saved.EventId == evt.Id && saved.UserId == userId),
            userId == null ? null : db.EventRegistrations.Where(reg => reg.EventId == evt.Id && reg.UserId == userId).Select(reg => (EventJoinState?)reg.State).FirstOrDefault(),
            userId == null ? null : db.EventRegistrations.Where(reg => reg.EventId == evt.Id && reg.UserId == userId).Select(reg => (RsvpVisibility?)reg.Visibility).FirstOrDefault(),
            evt.SignupType,
            evt.ExternalBookingUrl,
            evt.ImageUrl,
            evt.Status,
            evt.Tags,
            evt.BringItems,
            evt.BringNotes,
            evt.Facilities,
            evt.AccessibilityFeatures,
            evt.PhysicalIntensity,
            evt.SocialInteractionLevel,
            evt.ExperienceLevel,
            evt.AtmosphereTags,
            evt.FoodProvided,
            evt.DrinksProvided,
            evt.DietaryOptions,
            evt.FoodNotes,
            evt.AgeRestriction,
            evt.SafetyNotes);

    public static IQueryable<CommunityDto> ProjectCommunities(IQueryable<Community> query, WithinDbContext db, Guid? userId) =>
        query.Select(item => new CommunityDto(
            item.Id,
            item.ProviderId,
            item.Name,
            item.Description,
            item.Lens,
            item.Location,
            db.CommunityMembers.Count(member => member.CommunityId == item.Id),
            userId != null && db.CommunityMembers.Any(member => member.CommunityId == item.Id && member.UserId == userId)));

    public static IQueryable<PostDto> ProjectPosts(IQueryable<Post> query, WithinDbContext db) =>
        from post in query
        join user in db.Users on post.AuthorUserId equals user.Id
        orderby post.CreatedUtc descending
        select new PostDto(
            post.Id,
            post.CommunityId,
            post.EventId,
            user.DisplayName,
            post.Body,
            db.Reactions.Count(reaction => reaction.PostId == post.Id),
            db.Comments.Count(comment => comment.PostId == post.Id && !comment.IsHidden),
            post.CreatedUtc);

    public static IQueryable<CommentDto> ProjectComments(IQueryable<Comment> query, WithinDbContext db) =>
        from comment in query
        join user in db.Users on comment.AuthorUserId equals user.Id
        orderby comment.CreatedUtc
        select new CommentDto(comment.Id, comment.ParentCommentId, user.DisplayName, comment.Body, comment.CreatedUtc);

    private static string[] NormalizeList(string[]? values) =>
        values is null
            ? []
            : values.Select(value => value.Trim().ToLowerInvariant())
                .Where(value => value.Length > 0)
                .Distinct()
                .ToArray();

    private static string? NormalizeNullable(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static string? NormalizeSingle(string? value)
    {
        var trimmed = value?.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static bool IsAllowed(string? value, HashSet<string> allowed)
    {
        var normalized = NormalizeSingle(value);
        return normalized is null || allowed.Contains(normalized);
    }
}
