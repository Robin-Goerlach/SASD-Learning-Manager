using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.Resources;

/// <summary>
/// Canonical learning resource. A resource is stored exactly once and may later be linked to
/// many skills, topics and learning paths. Completion belongs to this object and is deliberately
/// kept separate from future skill mastery assessments.
/// </summary>
public sealed class Resource
{
    private Resource(
        Guid id,
        string title,
        ResourceType type,
        Guid? providerId,
        string? url,
        string? normalizedUrl,
        string? localPath,
        string? description,
        string? whySaved,
        string? creator,
        string? languageCode,
        string? versionText,
        int? estimatedMinutes,
        ResourceDifficulty difficulty,
        ResourcePriority priority,
        ResourceStatus status,
        int? progressPercent,
        DateTimeOffset? startedAtUtc,
        DateTimeOffset? completedAtUtc,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        DateTimeOffset? archivedAtUtc)
    {
        Id = id;
        Title = title;
        Type = type;
        ProviderId = providerId;
        Url = url;
        NormalizedUrl = normalizedUrl;
        LocalPath = localPath;
        Description = description;
        WhySaved = whySaved;
        Creator = creator;
        LanguageCode = languageCode;
        VersionText = versionText;
        EstimatedMinutes = estimatedMinutes;
        Difficulty = difficulty;
        Priority = priority;
        Status = status;
        ProgressPercent = progressPercent;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        ArchivedAtUtc = archivedAtUtc;
    }

    public Guid Id { get; }
    public string Title { get; private set; }
    public ResourceType Type { get; private set; }
    public Guid? ProviderId { get; private set; }
    public string? Url { get; private set; }
    public string? NormalizedUrl { get; private set; }
    public string? LocalPath { get; private set; }
    public string? Description { get; private set; }
    public string? WhySaved { get; private set; }
    public string? Creator { get; private set; }
    public string? LanguageCode { get; private set; }
    public string? VersionText { get; private set; }
    public int? EstimatedMinutes { get; private set; }
    public ResourceDifficulty Difficulty { get; private set; }
    public ResourcePriority Priority { get; private set; }
    public ResourceStatus Status { get; private set; }
    public int? ProgressPercent { get; private set; }
    public DateTimeOffset? StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    /// <summary>Creates a new canonical resource.</summary>
    public static Resource Create(
        string title,
        ResourceType type,
        Guid? providerId,
        string? url,
        string? normalizedUrl,
        string? localPath,
        string? description,
        string? whySaved,
        string? creator,
        string? languageCode,
        string? versionText,
        int? estimatedMinutes,
        ResourceDifficulty difficulty,
        ResourcePriority priority,
        ResourceStatus status,
        DateTimeOffset nowUtc)
    {
        ValidateEstimatedMinutes(estimatedMinutes);
        ValidateNormalizedUrlPair(url, normalizedUrl);

        return new Resource(
            Guid.NewGuid(),
            Guard.RequiredText(title, "Resource title", 500),
            type,
            providerId,
            Guard.OptionalHttpUrl(url),
            Guard.OptionalText(normalizedUrl, "Normalized URL", 4096),
            Guard.OptionalText(localPath, "Local path", 4096),
            Guard.OptionalText(description, "Description", 20_000),
            Guard.OptionalText(whySaved, "Why saved", 10_000),
            Guard.OptionalText(creator, "Creator", 500),
            NormalizeLanguage(languageCode),
            Guard.OptionalText(versionText, "Version", 200),
            estimatedMinutes,
            difficulty,
            priority,
            status,
            status == ResourceStatus.Completed ? 100 : null,
            status is ResourceStatus.Started or ResourceStatus.Paused or ResourceStatus.Completed ? nowUtc : null,
            status == ResourceStatus.Completed ? nowUtc : null,
            nowUtc,
            nowUtc,
            status == ResourceStatus.Archived ? nowUtc : null);
    }

    /// <summary>Rehydrates a resource from trusted persistence data.</summary>
    public static Resource Rehydrate(
        Guid id,
        string title,
        ResourceType type,
        Guid? providerId,
        string? url,
        string? normalizedUrl,
        string? localPath,
        string? description,
        string? whySaved,
        string? creator,
        string? languageCode,
        string? versionText,
        int? estimatedMinutes,
        ResourceDifficulty difficulty,
        ResourcePriority priority,
        ResourceStatus status,
        int? progressPercent,
        DateTimeOffset? startedAtUtc,
        DateTimeOffset? completedAtUtc,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        DateTimeOffset? archivedAtUtc)
    {
        return new Resource(id, title, type, providerId, url, normalizedUrl, localPath, description, whySaved,
            creator, languageCode, versionText, estimatedMinutes, difficulty, priority, status, progressPercent,
            startedAtUtc, completedAtUtc, createdAtUtc, updatedAtUtc, archivedAtUtc);
    }

    /// <summary>Updates metadata without altering progress or lifecycle history.</summary>
    public void UpdateMetadata(
        string title,
        ResourceType type,
        Guid? providerId,
        string? url,
        string? normalizedUrl,
        string? localPath,
        string? description,
        string? whySaved,
        string? creator,
        string? languageCode,
        string? versionText,
        int? estimatedMinutes,
        ResourceDifficulty difficulty,
        ResourcePriority priority,
        DateTimeOffset nowUtc)
    {
        if (Status == ResourceStatus.Archived)
        {
            throw new DomainValidationException("An archived resource must be restored before it can be edited.");
        }

        ValidateEstimatedMinutes(estimatedMinutes);
        ValidateNormalizedUrlPair(url, normalizedUrl);

        Title = Guard.RequiredText(title, "Resource title", 500);
        Type = type;
        ProviderId = providerId;
        Url = Guard.OptionalHttpUrl(url);
        NormalizedUrl = Guard.OptionalText(normalizedUrl, "Normalized URL", 4096);
        LocalPath = Guard.OptionalText(localPath, "Local path", 4096);
        Description = Guard.OptionalText(description, "Description", 20_000);
        WhySaved = Guard.OptionalText(whySaved, "Why saved", 10_000);
        Creator = Guard.OptionalText(creator, "Creator", 500);
        LanguageCode = NormalizeLanguage(languageCode);
        VersionText = Guard.OptionalText(versionText, "Version", 200);
        EstimatedMinutes = estimatedMinutes;
        Difficulty = difficulty;
        Priority = priority;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Changes the learning status and maintains the associated timestamps.</summary>
    public void ChangeStatus(ResourceStatus newStatus, DateTimeOffset nowUtc)
    {
        if (newStatus == ResourceStatus.Archived)
        {
            Archive(nowUtc);
            return;
        }

        if (Status == ResourceStatus.Archived)
        {
            throw new DomainValidationException("An archived resource must be restored before changing its learning status.");
        }

        Status = newStatus;
        if (newStatus is ResourceStatus.Started or ResourceStatus.Paused or ResourceStatus.Completed)
        {
            StartedAtUtc ??= nowUtc;
        }

        if (newStatus == ResourceStatus.Completed)
        {
            CompletedAtUtc = nowUtc;
            ProgressPercent = 100;
        }
        else if (CompletedAtUtc is not null)
        {
            // Re-opening a resource keeps StartedAt but clears the previous current completion date.
            // Completion history will later be available through ActivityLog.
            CompletedAtUtc = null;
        }

        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Sets the user's processing progress for resources where a percentage is meaningful.</summary>
    public void SetProgress(int? progressPercent, DateTimeOffset nowUtc)
    {
        if (Status == ResourceStatus.Archived)
        {
            throw new DomainValidationException("Progress cannot be changed while a resource is archived.");
        }

        if (progressPercent is < 0 or > 100)
        {
            throw new DomainValidationException("Progress must be between 0 and 100 percent.");
        }

        ProgressPercent = progressPercent;
        if (progressPercent > 0)
        {
            StartedAtUtc ??= nowUtc;
        }

        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Archives the resource while preserving all metadata and relationships.</summary>
    public void Archive(DateTimeOffset nowUtc)
    {
        Status = ResourceStatus.Archived;
        ArchivedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Restores an archived resource to a neutral planned state.</summary>
    public void Restore(DateTimeOffset nowUtc)
    {
        if (Status != ResourceStatus.Archived)
        {
            return;
        }

        Status = ResourceStatus.Planned;
        ArchivedAtUtc = null;
        UpdatedAtUtc = nowUtc;
    }

    private static void ValidateEstimatedMinutes(int? estimatedMinutes)
    {
        if (estimatedMinutes is < 0)
        {
            throw new DomainValidationException("Estimated learning time cannot be negative.");
        }
    }

    private static void ValidateNormalizedUrlPair(string? url, string? normalizedUrl)
    {
        if (string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(normalizedUrl))
        {
            throw new DomainValidationException("A normalized URL cannot exist without an original URL.");
        }
    }

    private static string? NormalizeLanguage(string? languageCode)
    {
        var value = Guard.OptionalText(languageCode, "Language", 20);
        return value?.ToLowerInvariant();
    }
}
