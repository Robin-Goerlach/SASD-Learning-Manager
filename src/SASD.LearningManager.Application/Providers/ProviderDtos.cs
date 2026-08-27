using SASD.LearningManager.Domain.Providers;

namespace SASD.LearningManager.Application.Providers;

/// <summary>Compact representation used by provider lists and lookup controls.</summary>
public sealed record ProviderListItemDto(
    Guid Id,
    string Name,
    ProviderType Type,
    ProviderStatus Status,
    string? WebsiteUrl);

/// <summary>Editable provider input used by create and update operations.</summary>
public sealed record ProviderEditModel(
    string Name,
    string? WebsiteUrl,
    string? Description,
    ProviderType Type);
