namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>Defines who currently owns the next step in the job-search process.</summary>
public enum WorkItemKind
{
    /// <summary>The user must perform the next action.</summary>
    Action,

    /// <summary>The next response is expected from another person or organization.</summary>
    WaitingFor,
}
