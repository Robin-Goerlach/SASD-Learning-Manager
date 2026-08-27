using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.Providers;

/// <summary>
/// Represents the source or publisher of one or more learning resources.
/// A provider is deliberately data, not an integration boundary: O'Reilly, YouTube and an
/// internal document repository are handled by the same domain concept.
/// </summary>
public sealed class Provider
{
    private Provider(
        Guid id,
        string name,
        string? websiteUrl,
        string? description,
        ProviderType type,
        ProviderStatus status,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        DateTimeOffset? archivedAtUtc)
    {
        Id = id;
        Name = name;
        WebsiteUrl = websiteUrl;
        Description = description;
        Type = type;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        ArchivedAtUtc = archivedAtUtc;
    }

    public Guid Id { get; }
    public string Name { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public string? Description { get; private set; }
    public ProviderType Type { get; private set; }
    public ProviderStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    /// <summary>Creates a new active provider.</summary>
    public static Provider Create(
        string name,
        string? websiteUrl,
        string? description,
        ProviderType type,
        DateTimeOffset nowUtc)
    {
        return new Provider(
            Guid.NewGuid(),
            Guard.RequiredText(name, "Provider name", 200),
            Guard.OptionalHttpUrl(websiteUrl),
            Guard.OptionalText(description, "Provider description", 4000),
            type,
            ProviderStatus.Active,
            nowUtc,
            nowUtc,
            null);
    }

    /// <summary>Rehydrates a provider from trusted persistence data.</summary>
    public static Provider Rehydrate(
        Guid id,
        string name,
        string? websiteUrl,
        string? description,
        ProviderType type,
        ProviderStatus status,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        DateTimeOffset? archivedAtUtc)
    {
        return new Provider(id, name, websiteUrl, description, type, status, createdAtUtc, updatedAtUtc, archivedAtUtc);
    }

    /// <summary>Updates editable provider metadata.</summary>
    public void Update(string name, string? websiteUrl, string? description, ProviderType type, DateTimeOffset nowUtc)
    {
        if (Status == ProviderStatus.Archived)
        {
            throw new DomainValidationException("An archived provider must be restored before it can be edited.");
        }

        Name = Guard.RequiredText(name, "Provider name", 200);
        WebsiteUrl = Guard.OptionalHttpUrl(websiteUrl);
        Description = Guard.OptionalText(description, "Provider description", 4000);
        Type = type;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Marks the provider inactive without removing historical references.</summary>
    public void SetInactive(DateTimeOffset nowUtc)
    {
        if (Status == ProviderStatus.Archived)
        {
            throw new DomainValidationException("An archived provider must be restored before it can be changed.");
        }

        Status = ProviderStatus.Inactive;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Marks the provider active.</summary>
    public void SetActive(DateTimeOffset nowUtc)
    {
        if (Status == ProviderStatus.Archived)
        {
            throw new DomainValidationException("An archived provider must be restored before it can be activated.");
        }

        Status = ProviderStatus.Active;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Archives the provider while preserving resource history.</summary>
    public void Archive(DateTimeOffset nowUtc)
    {
        Status = ProviderStatus.Archived;
        ArchivedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Restores a previously archived provider as inactive.</summary>
    public void Restore(DateTimeOffset nowUtc)
    {
        if (Status != ProviderStatus.Archived)
        {
            return;
        }

        Status = ProviderStatus.Inactive;
        ArchivedAtUtc = null;
        UpdatedAtUtc = nowUtc;
    }
}
