using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.Competencies;

/// <summary>
/// Broad competency domain such as Linux, Cloud or Cyber Security. A competency area groups
/// topics and skills but is not itself mastery-rated.
/// </summary>
public sealed class CompetencyArea
{
    private CompetencyArea(Guid id, string name, string? description, CatalogStatus status,
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

    public static CompetencyArea Create(string name, string? description, DateTimeOffset nowUtc)
        => new(Guid.NewGuid(), Guard.RequiredText(name, "Competency area name", 200),
            Guard.OptionalText(description, "Competency area description", 4000), CatalogStatus.Active,
            nowUtc, nowUtc, null);

    public static CompetencyArea Rehydrate(Guid id, string name, string? description, CatalogStatus status,
        DateTimeOffset createdAtUtc, DateTimeOffset updatedAtUtc, DateTimeOffset? archivedAtUtc)
        => new(id, name, description, status, createdAtUtc, updatedAtUtc, archivedAtUtc);

    public void Update(string name, string? description, DateTimeOffset nowUtc)
    {
        EnsureNotArchived();
        Name = Guard.RequiredText(name, "Competency area name", 200);
        Description = Guard.OptionalText(description, "Competency area description", 4000);
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
            throw new DomainValidationException("An archived competency area must be restored before it can be edited.");
        }
    }
}
