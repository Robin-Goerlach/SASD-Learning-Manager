using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.LearningPaths;

/// <summary>Represents a typed non-hierarchical relation between two nodes of the same path.</summary>
public sealed class LearningPathNodeRelation
{
    private LearningPathNodeRelation(Guid id, Guid sourceNodeId, Guid targetNodeId,
        LearningPathNodeRelationType type, string? note, DateTimeOffset createdAtUtc)
    {
        Id = id;
        SourceNodeId = sourceNodeId;
        TargetNodeId = targetNodeId;
        Type = type;
        Note = note;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }
    public Guid SourceNodeId { get; }
    public Guid TargetNodeId { get; }
    public LearningPathNodeRelationType Type { get; }
    public string? Note { get; }
    public DateTimeOffset CreatedAtUtc { get; }

    public static LearningPathNodeRelation Create(Guid sourceNodeId, Guid targetNodeId,
        LearningPathNodeRelationType type, string? note, DateTimeOffset nowUtc)
    {
        if (sourceNodeId == Guid.Empty || targetNodeId == Guid.Empty)
        {
            throw new DomainValidationException("Relation node ids must not be empty.");
        }

        if (sourceNodeId == targetNodeId)
        {
            throw new DomainValidationException("A path node cannot relate to itself.");
        }

        return new LearningPathNodeRelation(Guid.NewGuid(), sourceNodeId, targetNodeId, type,
            Guard.OptionalText(note, "Relation note", 2_000), nowUtc);
    }

    public static LearningPathNodeRelation Rehydrate(Guid id, Guid sourceNodeId, Guid targetNodeId,
        LearningPathNodeRelationType type, string? note, DateTimeOffset createdAtUtc)
        => new(id, sourceNodeId, targetNodeId, type, note, createdAtUtc);
}
