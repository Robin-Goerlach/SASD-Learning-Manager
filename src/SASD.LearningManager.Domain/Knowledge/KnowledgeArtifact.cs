using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.Knowledge;

public enum KnowledgeArtifactType { Note, Summary, CheatSheet, CodeSnippet, LessonLearned, Question, CommandReference, Procedure, Other }
public enum KnowledgeArtifactStatus { Active, Archived }

/// <summary>Portable Markdown knowledge captured independently from its source relations.</summary>
public sealed class KnowledgeArtifact
{
    private KnowledgeArtifact(Guid id, string title, string markdown, KnowledgeArtifactType type, KnowledgeArtifactStatus status,
        DateTimeOffset createdAtUtc, DateTimeOffset updatedAtUtc, DateTimeOffset? archivedAtUtc)
        => (Id, Title, Markdown, Type, Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc) =
            (id, title, markdown, type, status, createdAtUtc, updatedAtUtc, archivedAtUtc);

    public Guid Id { get; }
    public string Title { get; private set; }
    public string Markdown { get; private set; }
    public KnowledgeArtifactType Type { get; private set; }
    public KnowledgeArtifactStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public static KnowledgeArtifact Create(string title, string markdown, KnowledgeArtifactType type, DateTimeOffset now)
        => new(Guid.NewGuid(), Guard.RequiredText(title, "Title", 300), Guard.RequiredText(markdown, "Markdown", 1_000_000),
            type, KnowledgeArtifactStatus.Active, now, now, null);

    public void Update(string title, string markdown, KnowledgeArtifactType type, DateTimeOffset now)
    {
        if (Status == KnowledgeArtifactStatus.Archived) throw new DomainValidationException("Archived knowledge must be restored before editing.");
        Title = Guard.RequiredText(title, "Title", 300);
        Markdown = Guard.RequiredText(markdown, "Markdown", 1_000_000);
        Type = type;
        UpdatedAtUtc = now;
    }

    public void Archive(DateTimeOffset now) { Status = KnowledgeArtifactStatus.Archived; ArchivedAtUtc = now; UpdatedAtUtc = now; }
    public void Restore(DateTimeOffset now) { Status = KnowledgeArtifactStatus.Active; ArchivedAtUtc = null; UpdatedAtUtc = now; }
    public static KnowledgeArtifact Rehydrate(Guid id, string title, string markdown, KnowledgeArtifactType type, KnowledgeArtifactStatus status,
        DateTimeOffset created, DateTimeOffset updated, DateTimeOffset? archived) => new(id, title, markdown, type, status, created, updated, archived);
}
