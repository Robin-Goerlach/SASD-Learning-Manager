namespace SASD.LearningManager.Domain.Goals;

/// <summary>Describes the purpose of a learning goal.</summary>
public enum GoalType
{
    Learning,
    Career,
    Certification,
    Project,
    Interest,
    Other
}

/// <summary>Lifecycle state of a goal.</summary>
public enum GoalStatus
{
    Planned,
    Active,
    Paused,
    Achieved,
    Archived
}

/// <summary>Relative importance of a goal.</summary>
public enum GoalPriority
{
    Low,
    Normal,
    High,
    VeryHigh
}
