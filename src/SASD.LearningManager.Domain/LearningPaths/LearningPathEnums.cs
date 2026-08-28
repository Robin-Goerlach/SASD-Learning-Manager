namespace SASD.LearningManager.Domain.LearningPaths;

/// <summary>Lifecycle state of a learning path.</summary>
public enum LearningPathStatus
{
    Planned,
    Active,
    Paused,
    Completed,
    Archived
}

/// <summary>Relative importance of a learning path.</summary>
public enum LearningPathPriority
{
    Low,
    Normal,
    High,
    VeryHigh
}

/// <summary>Describes the semantic role of a node inside a learning path.</summary>
public enum LearningPathNodeType
{
    Module,
    Topic,
    SkillCheckpoint,
    Activity,
    Project,
    Milestone,
    Other
}

/// <summary>Lifecycle state of an individual learning-path node.</summary>
public enum LearningPathNodeStatus
{
    Planned,
    Active,
    Completed,
    Skipped,
    Archived
}

/// <summary>
/// Describes a graph-like relationship between two nodes. These relationships intentionally live
/// next to, rather than inside, the parent/child tree so hierarchy and dependency remain separate.
/// </summary>
public enum LearningPathNodeRelationType
{
    Requires,
    AlternativeTo,
    RecommendedBefore,
    RecommendedAfter,
    Deepens,
    Related
}
