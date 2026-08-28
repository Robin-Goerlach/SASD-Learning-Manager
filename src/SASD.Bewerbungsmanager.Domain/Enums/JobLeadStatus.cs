namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>Describes the review state of one discovered job result before or after promotion to an opportunity.</summary>
public enum JobLeadStatus
{
    /// <summary>The result was imported and has not yet been reviewed.</summary>
    New = 0,

    /// <summary>The result was reviewed and remains potentially interesting.</summary>
    Reviewed = 1,

    /// <summary>The result was promoted to a durable opportunity.</summary>
    Imported = 2,

    /// <summary>The result was deliberately dismissed and should normally stay out of the active inbox.</summary>
    Ignored = 3,
}
