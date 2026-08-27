namespace SASD.LearningManager.Application.Resources;

/// <summary>Produces a conservative canonical representation used for duplicate detection.</summary>
public interface IUrlNormalizer
{
    string? Normalize(string? url);
}
