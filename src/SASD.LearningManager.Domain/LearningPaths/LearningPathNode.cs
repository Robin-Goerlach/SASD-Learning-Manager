using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.LearningPaths;

/// <summary>
/// Represents one hierarchical step inside a learning path. Tree-cycle detection is deliberately
/// handled by the application service because it requires knowledge of other nodes in the path.
/// </summary>
public sealed class LearningPathNode
{
    private LearningPathNode(
        Guid id,
        Guid learningPathId,
        Guid? parentNodeId,
        string title,
        string? description,
        LearningPathNodeType type,
        int sortOrder,
        bool isRequired,
        LearningPathNodeStatus status,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        DateTimeOffset? archivedAtUtc)
    {
        Id = id;
        LearningPathId = learningPathId;
        ParentNodeId = parentNodeId;
        Title = title;
        Description = description;
        Type = type;
        SortOrder = sortOrder;
        IsRequired = isRequired;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        ArchivedAtUtc = archivedAtUtc;
    }

    public Guid Id { get; }
    public Guid LearningPathId { get; }
    public Guid? ParentNodeId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public LearningPathNodeType Type { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsRequired { get; private set; }
    public LearningPathNodeStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public static LearningPathNode Create(
        Guid learningPathId,
        Guid? parentNodeId,
        string title,
        string? description,
        LearningPathNodeType type,
        int sortOrder,
        bool isRequired,
        DateTimeOffset nowUtc)
    {
        if (learningPathId == Guid.Empty)
        {
            throw new DomainValidationException("Learning path id must not be empty.");
        }

        ValidateSortOrder(sortOrder);
        return new LearningPathNode(Guid.NewGuid(), learningPathId, parentNodeId,
            Guard.RequiredText(title, "Learning path node title", 500),
            Guard.OptionalText(description, "Learning path node description", 20_000),
            type, sortOrder, isRequired, LearningPathNodeStatus.Planned, nowUtc, nowUtc, null);
    }

    public static LearningPathNode Rehydrate(
        Guid id,
        Guid learningPathId,
        Guid? parentNodeId,
        string title,
        string? description,
        LearningPathNodeType type,
        int sortOrder,
        bool isRequired,
        LearningPathNodeStatus status,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        DateTimeOffset? archivedAtUtc)
        => new(id, learningPathId, parentNodeId, title, description, type, sortOrder, isRequired,
            status, createdAtUtc, updatedAtUtc, archivedAtUtc);

    /// <summary>Updates user-editable node metadata but not its parent/order.</summary>
    public void Update(
        string title,
        string? description,
        LearningPathNodeType type,
        bool isRequired,
        LearningPathNodeStatus status,
        DateTimeOffset nowUtc)
    {
        EnsureNotArchived();
        if (status == LearningPathNodeStatus.Archived)
        {
            throw new DomainValidationException("Use the archive operation to archive a path node.");
        }

        Title = Guard.RequiredText(title, "Learning path node title", 500);
        Description = Guard.OptionalText(description, "Learning path node description", 20_000);
        Type = type;
        IsRequired = isRequired;
        Status = status;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Moves the node after the application layer has verified tree-cycle constraints.</summary>
    public void MoveTo(Guid? parentNodeId, int sortOrder, DateTimeOffset nowUtc)
    {
        EnsureNotArchived();
        if (parentNodeId == Id)
        {
            throw new DomainValidationException("A path node cannot be its own parent.");
        }

        ValidateSortOrder(sortOrder);
        ParentNodeId = parentNodeId;
        SortOrder = sortOrder;
        UpdatedAtUtc = nowUtc;
    }

    public void Archive(DateTimeOffset nowUtc)
    {
        Status = LearningPathNodeStatus.Archived;
        ArchivedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void Restore(DateTimeOffset nowUtc)
    {
        if (Status != LearningPathNodeStatus.Archived)
        {
            return;
        }

        Status = LearningPathNodeStatus.Planned;
        ArchivedAtUtc = null;
        UpdatedAtUtc = nowUtc;
    }

    private static void ValidateSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new DomainValidationException("Sort order must not be negative.");
        }
    }

    private void EnsureNotArchived()
    {
        if (Status == LearningPathNodeStatus.Archived)
        {
            throw new DomainValidationException("An archived path node must be restored before it can be changed.");
        }
    }
}
