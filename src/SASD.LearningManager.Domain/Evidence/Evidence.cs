using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.Evidence;

public enum EvidenceType { CourseCompletion, Assessment, Quiz, Lab, Project, Certificate, PracticalUse, Documentation, Presentation, SelfAssessment, Other }
public enum EvidenceStatus { Active, Archived }

/// <summary>A user-recorded competency proof. It supports an assessment but never changes mastery itself.</summary>
public sealed class EvidenceItem
{
    private EvidenceItem(Guid id, string title, string? description, EvidenceType type, DateTimeOffset occurredAtUtc,
        string? url, string? localPath, string? evaluation, EvidenceStatus status, DateTimeOffset created, DateTimeOffset updated, DateTimeOffset? archived)
        => (Id, Title, Description, Type, OccurredAtUtc, Url, LocalPath, Evaluation, Status, CreatedAtUtc, UpdatedAtUtc, ArchivedAtUtc) =
            (id, title, description, type, occurredAtUtc, url, localPath, evaluation, status, created, updated, archived);

    public Guid Id { get; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public EvidenceType Type { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public string? Url { get; private set; }
    public string? LocalPath { get; private set; }
    public string? Evaluation { get; private set; }
    public EvidenceStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public static EvidenceItem Create(string title, string? description, EvidenceType type, DateTimeOffset occurredAtUtc,
        string? url, string? localPath, string? evaluation, DateTimeOffset now)
    {
        if (occurredAtUtc > now.AddMinutes(5)) throw new DomainValidationException("Evidence date cannot be in the future.");
        return new(Guid.NewGuid(), Guard.RequiredText(title, "Title", 300), Guard.OptionalText(description, "Description", 20_000), type,
            occurredAtUtc, Guard.OptionalHttpUrl(url), Guard.OptionalText(localPath, "Local path", 4096),
            Guard.OptionalText(evaluation, "Evaluation", 10_000), EvidenceStatus.Active, now, now, null);
    }

    public void Update(string title, string? description, EvidenceType type, DateTimeOffset occurredAtUtc,
        string? url, string? localPath, string? evaluation, DateTimeOffset now)
    {
        if (Status == EvidenceStatus.Archived) throw new DomainValidationException("Archived evidence must be restored before editing.");
        if (occurredAtUtc > now.AddMinutes(5)) throw new DomainValidationException("Evidence date cannot be in the future.");
        Title = Guard.RequiredText(title, "Title", 300); Description = Guard.OptionalText(description, "Description", 20_000);
        Type = type; OccurredAtUtc = occurredAtUtc; Url = Guard.OptionalHttpUrl(url);
        LocalPath = Guard.OptionalText(localPath, "Local path", 4096); Evaluation = Guard.OptionalText(evaluation, "Evaluation", 10_000); UpdatedAtUtc = now;
    }

    public void Archive(DateTimeOffset now) { Status = EvidenceStatus.Archived; ArchivedAtUtc = now; UpdatedAtUtc = now; }
    public void Restore(DateTimeOffset now) { Status = EvidenceStatus.Active; ArchivedAtUtc = null; UpdatedAtUtc = now; }
    public static EvidenceItem Rehydrate(Guid id, string title, string? description, EvidenceType type, DateTimeOffset occurred, string? url,
        string? localPath, string? evaluation, EvidenceStatus status, DateTimeOffset created, DateTimeOffset updated, DateTimeOffset? archived)
        => new(id, title, description, type, occurred, url, localPath, evaluation, status, created, updated, archived);
}
