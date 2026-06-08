using WithinAPI.Domain;

namespace WithinAPI.Services;

public static class ProviderRules
{
    public static ProviderType InferProviderType(ProviderCategory category) =>
        category == ProviderCategory.IndividualPractitioner ? ProviderType.Individual : ProviderType.Business;

    public static bool CanSaveService(
        string? name,
        string? description,
        string? category,
        int? durationMinutes,
        decimal? priceAmount,
        ProviderPriceType priceType,
        out string message)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            message = "Service name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            message = "Service description is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            message = "Service category is required.";
            return false;
        }

        if (durationMinutes is < 0)
        {
            message = "Duration cannot be negative.";
            return false;
        }

        if (priceAmount is < 0)
        {
            message = "Price cannot be negative.";
            return false;
        }

        if ((priceType is ProviderPriceType.Fixed or ProviderPriceType.FromPrice) && priceAmount is null)
        {
            message = "Price amount is required for fixed and from-price services.";
            return false;
        }

        message = "";
        return true;
    }

    public static bool MatchesSearch(Provider provider, IEnumerable<ProviderService> services, string term)
    {
        var query = term.Trim();
        if (query.Length == 0) return true;
        return Contains(provider.Name, query) ||
            Contains(provider.Bio, query) ||
            Contains(provider.Location, query) ||
            provider.Categories.Any(value => Contains(value, query)) ||
            provider.ServicesOffered.Any(value => Contains(value, query)) ||
            services.Any(service =>
                service.IsActive &&
                service.ProviderId == provider.Id &&
                (Contains(service.Name, query) || Contains(service.Description, query) || Contains(service.Category, query)));
    }

    private static bool Contains(string? value, string term) =>
        value?.Contains(term, StringComparison.OrdinalIgnoreCase) == true;
}
