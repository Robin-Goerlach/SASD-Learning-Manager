using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.Competencies;

/// <summary>
/// A subject-oriented knowledge grouping such as Docker Networking. Topics organize knowledge;
/// unlike skills they do not carry a mastery level.
/// </summary>
public sealed class Topic
{
    private Topic(Guid id, string name, string? description, CatalogStatus status,
        DateTimeOffset createdAtUtc, DateTimeOffset updatedAtUtc, DateTimeOffset? archivedAtUtc)
    {
        Id = id;
        Name = name;
        Description = description;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        ArchivedAtUtc = archivedAtUtc;
    }

    public Guid Id { get; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public CatalogStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public static Topic Create(string name, string? description, DateTimeOffset nowUtc)
        => new(Guid.NewGuid(), Guard.RequiredText(name, "Topic name", 200),
            Guard.OptionalText(description, "Topic description", 4000), CatalogStatus.Active,
            nowUtc, nowUtc, null);

    public static Topic Rehydrate(Guid id, string name, string? description, CatalogStatus status,
        DateTimeOffset createdAtUtc, DateTimeOffset updatedAtUtc, DateTimeOffset? archivedAtUtc)
        => new(id, name, description, status, createdAtUtc, updatedAtUtc, archivedAtUtc);

    public void Update(string name, string? description, DateTimeOffset nowUtc)
    {
        EnsureNotArchived();
        Name = Guard.RequiredText(name, "Topic name", 200);
        Description = Guard.OptionalText(description, "Topic description", 4000);
        UpdatedAtUtc = nowUtc;
    }

    public void SetActive(bool active, DateTimeOffset nowUtc)
    {
        EnsureNotArchived();
        Status = active ? CatalogStatus.Active : CatalogStatus.Inactive;
        UpdatedAtUtc = nowUtc;
    }

    public void Archive(DateTimeOffset nowUtc)
    {
        Status = CatalogStatus.Archived;
        ArchivedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void Restore(DateTimeOffset nowUtc)
    {
        if (Status != CatalogStatus.Archived) return;
        Status = CatalogStatus.Inactive;
        ArchivedAtUtc = null;
        UpdatedAtUtc = nowUtc;
    }

    private void EnsureNotArchived()
    {
        if (Status == CatalogStatus.Archived)
        {
            throw new DomainValidationException("An archived topic must be restored before it can be edited.");
        }
    }
}
