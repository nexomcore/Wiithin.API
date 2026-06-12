namespace WithinAPI.Domain;

public sealed class ProviderFollow
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ProviderProfileView
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public Guid? ViewerUserId { get; set; }
    public DateOnly ViewDate { get; set; }
    public DateTimeOffset ViewedAt { get; set; }
}
