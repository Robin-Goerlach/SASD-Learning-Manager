namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>Represents the lifecycle of an operational work item.</summary>
public enum WorkItemStatus
{
    /// <summary>The work item is still relevant.</summary>
    Open,

    /// <summary>The work item was completed.</summary>
    Completed,

    /// <summary>The work item was deliberately cancelled without completion.</summary>
    Cancelled,
}
