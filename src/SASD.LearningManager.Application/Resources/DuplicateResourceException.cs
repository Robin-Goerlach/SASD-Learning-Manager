namespace SASD.LearningManager.Application.Resources;

/// <summary>
/// Signals that a normalized resource URL is already known. This is intentionally an advisory
/// application-level conflict rather than a database uniqueness constraint because rare cases may
/// legitimately require two resource records for the same address.
/// </summary>
public sealed class DuplicateResourceException : InvalidOperationException
{
    /// <summary>Initializes the conflict with the already existing resource identity.</summary>
    public DuplicateResourceException(Guid existingResourceId, string existingResourceTitle)
        : base($"This URL is already stored as '{existingResourceTitle}'.")
    {
        ExistingResourceId = existingResourceId;
        ExistingResourceTitle = existingResourceTitle;
    }

    public Guid ExistingResourceId { get; }
    public string ExistingResourceTitle { get; }
}
