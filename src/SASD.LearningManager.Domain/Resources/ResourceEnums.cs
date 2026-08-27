namespace SASD.LearningManager.Domain.Resources;

/// <summary>Describes the medium or learning format of a resource.</summary>
public enum ResourceType
{
    Course,
    Video,
    Book,
    Article,
    Document,
    Documentation,
    Lab,
    Project,
    Podcast,
    PracticeExam,
    Event,
    Repository,
    Other
}

/// <summary>Represents the learning lifecycle of a resource.</summary>
public enum ResourceStatus
{
    Inbox,
    Planned,
    Started,
    Paused,
    Deferred,
    Completed,
    Abandoned,
    Archived
}

/// <summary>Represents the user's priority for a resource.</summary>
public enum ResourcePriority
{
    Low,
    Normal,
    High,
    VeryHigh
}

/// <summary>Represents the difficulty of the learning material, not the user's skill level.</summary>
public enum ResourceDifficulty
{
    Unknown,
    Beginner,
    Intermediate,
    Advanced,
    Expert
}
