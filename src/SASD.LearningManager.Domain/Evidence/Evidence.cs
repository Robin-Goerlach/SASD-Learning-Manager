using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.Evidence;

/// <summary>Describes the kind of proof represented by one Evidence record.</summary>
public enum EvidenceType
{
    CourseCompletion,
    Assessment,
    Quiz,
    Lab,
    Project,
    Certificate,
    PracticalUse,
    Documentation,
    Presentation,
    SelfAssessment,
    Other
}

/// <summary>Represents the lifecycle state of Evidence.</summary>
public enum EvidenceStatus
{
    Active,
    Archived
}

/// <summary>
/// User-recorded competency proof. Evidence can support a later Skill Assessment but never changes
/// mastery by itself; that separation prevents course completion or an uploaded artifact from being
/// mistaken for an explicit competency judgement.
/// </summary>
public sealed class EvidenceItem
{
    private EvidenceItem(
        Guid id,
        string title,
        string? description,
        EvidenceType type,
        DateTimeOffset occurredAtUtc,
        string? url,
        string? localPath,
        string? evaluation,
        EvidenceStatus status,
        DateTimeOffset created,
        DateTimeOffset updated,
        DateTimeOffset? archived)
    {
        Id = id;
        Title = title;
        Description = description;
        Type = type;
        OccurredAtUtc = occurredAtUtc;
        Url = url;
        LocalPath = localPath;
        Evaluation = evaluation;
        Status = status;
        CreatedAtUtc = created;
        UpdatedAtUtc = updated;
        ArchivedAtUtc = archived;
    }

    /// <summary>Gets the stable identifier of this Evidence record.</summary>
    public Guid Id { get; }

    /// <summary>Gets the user-facing title.</summary>
    public string Title { get; private set; }

    /// <summary>Gets an optional description of what the Evidence demonstrates.</summary>
    public string? Description { get; private set; }

    /// <summary>Gets the category of proof.</summary>
    public EvidenceType Type { get; private set; }

    /// <summary>Gets the UTC timestamp at which the evidenced activity occurred.</summary>
    public DateTimeOffset OccurredAtUtc { get; private set; }

    /// <summary>Gets an optional HTTP/HTTPS reference URL.</summary>
    public string? Url { get; private set; }

    /// <summary>Gets an optional local path to an externally stored artifact.</summary>
    public string? LocalPath { get; private set; }

    /// <summary>Gets an optional user evaluation of the Evidence quality or result.</summary>
    public string? Evaluation { get; private set; }

    /// <summary>Gets the current lifecycle state.</summary>
    public EvidenceStatus Status { get; private set; }

    /// <summary>Gets the UTC creation timestamp.</summary>
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>Gets the UTC timestamp of the most recent change.</summary>
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    /// <summary>Gets the archive timestamp, or <see langword="null"/> while active.</summary>
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    /// <summary>Creates a new active Evidence record after validating its user-provided values.</summary>
    public static EvidenceItem Create(
        string title,
        string? description,
        EvidenceType type,
        DateTimeOffset occurredAtUtc,
        string? url,
        string? localPath,
        string? evaluation,
        DateTimeOffset now)
    {
        EnsureOccurredAtIsNotInFuture(occurredAtUtc, now);
        return new EvidenceItem(
            Guid.NewGuid(),
            Guard.RequiredText(title, "Title", 300),
            Guard.OptionalText(description, "Description", 20_000),
            type,
            occurredAtUtc,
            Guard.OptionalHttpUrl(url),
            Guard.OptionalText(localPath, "Local path", 4096),
            Guard.OptionalText(evaluation, "Evaluation", 10_000),
            EvidenceStatus.Active,
            now,
            now,
            null);
    }

    /// <summary>Updates an active Evidence record without changing linked Skill mastery.</summary>
    public void Update(
        string title,
        string? description,
        EvidenceType type,
        DateTimeOffset occurredAtUtc,
        string? url,
        string? localPath,
        string? evaluation,
        DateTimeOffset now)
    {
        if (Status == EvidenceStatus.Archived)
        {
            throw new DomainValidationException("Archived evidence must be restored before editing.");
        }

        EnsureOccurredAtIsNotInFuture(occurredAtUtc, now);
        Title = Guard.RequiredText(title, "Title", 300);
        Description = Guard.OptionalText(description, "Description", 20_000);
        Type = type;
        OccurredAtUtc = occurredAtUtc;
        Url = Guard.OptionalHttpUrl(url);
        LocalPath = Guard.OptionalText(localPath, "Local path", 4096);
        Evaluation = Guard.OptionalText(evaluation, "Evaluation", 10_000);
        UpdatedAtUtc = now;
    }

    /// <summary>Archives Evidence without physically deleting historical proof.</summary>
    public void Archive(DateTimeOffset now)
    {
        Status = EvidenceStatus.Archived;
        ArchivedAtUtc = now;
        UpdatedAtUtc = now;
    }

    /// <summary>Restores archived Evidence to the active library.</summary>
    public void Restore(DateTimeOffset now)
    {
        Status = EvidenceStatus.Active;
        ArchivedAtUtc = null;
        UpdatedAtUtc = now;
    }

    /// <summary>Rehydrates a persisted Evidence record.</summary>
    public static EvidenceItem Rehydrate(
        Guid id,
        string title,
        string? description,
        EvidenceType type,
        DateTimeOffset occurred,
        string? url,
        string? localPath,
        string? evaluation,
        EvidenceStatus status,
        DateTimeOffset created,
        DateTimeOffset updated,
        DateTimeOffset? archived)
    {
        return new EvidenceItem(
            id,
            title,
            description,
            type,
            occurred,
            url,
            localPath,
            evaluation,
            status,
            created,
            updated,
            archived);
    }

    private static void EnsureOccurredAtIsNotInFuture(DateTimeOffset occurredAtUtc, DateTimeOffset now)
    {
        // A small tolerance avoids rejecting a freshly recorded artifact because two machines have
        // minimally different clocks, while still blocking clearly future-dated Evidence.
        if (occurredAtUtc > now.AddMinutes(5))
        {
            throw new DomainValidationException("Evidence date cannot be in the future.");
        }
    }
}
