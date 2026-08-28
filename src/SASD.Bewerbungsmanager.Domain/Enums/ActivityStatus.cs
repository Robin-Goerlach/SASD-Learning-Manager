namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>Describes whether an activity is historical, planned, completed, or cancelled.</summary>
public enum ActivityStatus
{
    /// <summary>The activity is a historical record and needs no further completion step.</summary>
    Recorded,

    /// <summary>The activity is scheduled for the future.</summary>
    Planned,

    /// <summary>A previously planned activity was completed.</summary>
    Completed,

    /// <summary>A previously planned activity was cancelled.</summary>
    Cancelled,
}
