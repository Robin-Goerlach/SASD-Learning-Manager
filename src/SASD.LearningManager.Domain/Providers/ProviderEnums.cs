namespace SASD.LearningManager.Domain.Providers;

/// <summary>Describes the general kind of a learning-resource provider.</summary>
public enum ProviderType
{
    LearningPlatform,
    Publisher,
    Vendor,
    University,
    Community,
    Personal,
    Other
}

/// <summary>Represents the lifecycle state of a provider.</summary>
public enum ProviderStatus
{
    Active,
    Inactive,
    Archived
}
