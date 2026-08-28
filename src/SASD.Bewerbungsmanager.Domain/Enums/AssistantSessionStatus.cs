namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>Represents the local lifecycle of one optional assistant handoff session.</summary>
public enum AssistantSessionStatus
{
    /// <summary>The prompt was prepared locally and is waiting for an external/manual response.</summary>
    Prepared = 0,

    /// <summary>A response was deliberately pasted back and stored by the user.</summary>
    Completed = 1,

    /// <summary>The user decided not to continue using the prepared session.</summary>
    Discarded = 2,
}
