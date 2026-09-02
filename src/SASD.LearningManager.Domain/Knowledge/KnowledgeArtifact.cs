using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.Knowledge;

/// <summary>Describes the intended use of one portable Markdown knowledge artifact.</summary>
public enum KnowledgeArtifactType
{
    Note,
    Summary,
    CheatSheet,
    CodeSnippet,
    LessonLearned,
    Question,
    CommandReference,
    Procedure,
    Other
}

/// <summary>Represents the lifecycle state of a knowledge artifact.</summary>
public enum KnowledgeArtifactStatus
{
    Active,
    Archived
}

/// <summary>
/// Portable Markdown knowledge captured independently from its source relations. Relationships to
/// resources, skills, topics, goals and learning paths are maintained by the application layer so
/// the knowledge itself stays reusable and provider-independent.
/// </summary>
public sealed class KnowledgeArtifact
{
    private KnowledgeArtifact(
        Guid id,
        string title,
        string markdown,
        KnowledgeArtifactType type,
        KnowledgeArtifactStatus status,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        DateTimeOffset? archivedAtUtc)
    {
        Id = id;
        Title = title;
        Markdown = markdown;
        Type = type;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        ArchivedAtUtc = archivedAtUtc;
    }

    /// <summary>Gets the stable identifier of the artifact.</summary>
    public Guid Id { get; }

    /// <summary>Gets the user-facing artifact title.</summary>
    public string Title { get; private set; }

    /// <summary>Gets the Markdown content that contains the captured knowledge.</summary>
    public string Markdown { get; private set; }

    /// <summary>Gets the semantic artifact type.</summary>
    public KnowledgeArtifactType Type { get; private set; }

    /// <summary>Gets the current lifecycle state.</summary>
    public KnowledgeArtifactStatus Status { get; private set; }

    /// <summary>Gets the UTC creation timestamp.</summary>
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>Gets the UTC timestamp of the most recent change.</summary>
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    /// <summary>Gets the UTC archive timestamp, or <see langword="null"/> while active.</summary>
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    /// <summary>Creates a new active artifact after validating title and Markdown content.</summary>
    public static KnowledgeArtifact Create(
        string title,
        string markdown,
        KnowledgeArtifactType type,
        DateTimeOffset now)
    {
        return new KnowledgeArtifact(
            Guid.NewGuid(),
            Guard.RequiredText(title, "Title", 300),
            Guard.RequiredText(markdown, "Markdown", 1_000_000),
            type,
            KnowledgeArtifactStatus.Active,
            now,
            now,
            null);
    }

    /// <summary>Updates an active artifact while preserving its identity and creation time.</summary>
    public void Update(string title, string markdown, KnowledgeArtifactType type, DateTimeOffset now)
    {
        if (Status == KnowledgeArtifactStatus.Archived)
        {
            throw new DomainValidationException("Archived knowledge must be restored before editing.");
        }

        Title = Guard.RequiredText(title, "Title", 300);
        Markdown = Guard.RequiredText(markdown, "Markdown", 1_000_000);
        Type = type;
        UpdatedAtUtc = now;
    }

    /// <summary>Archives the artifact without deleting content or relationships.</summary>
    public void Archive(DateTimeOffset now)
    {
        Status = KnowledgeArtifactStatus.Archived;
        ArchivedAtUtc = now;
        UpdatedAtUtc = now;
    }

    /// <summary>Restores an archived artifact to the active library.</summary>
    public void Restore(DateTimeOffset now)
    {
        Status = KnowledgeArtifactStatus.Active;
        ArchivedAtUtc = null;
        UpdatedAtUtc = now;
    }

    /// <summary>Rehydrates a persisted artifact without replaying creation-time validation.</summary>
    public static KnowledgeArtifact Rehydrate(
        Guid id,
        string title,
        string markdown,
        KnowledgeArtifactType type,
        KnowledgeArtifactStatus status,
        DateTimeOffset created,
        DateTimeOffset updated,
        DateTimeOffset? archived)
    {
        return new KnowledgeArtifact(id, title, markdown, type, status, created, updated, archived);
    }
}
