namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>
/// Represents the coarse lifecycle of an interesting job opportunity.
/// </summary>
public enum OpportunityStatus
{
    /// <summary>The opportunity has been found but no contact has been made yet.</summary>
    Identified = 0,
    /// <summary>There has been contact about the opportunity.</summary>
    Contacted = 1,
    /// <summary>An application is being prepared.</summary>
    ApplicationPlanned = 2,
    /// <summary>An application has been submitted.</summary>
    Applied = 3,
    /// <summary>The process is currently in an interview phase.</summary>
    Interview = 4,
    /// <summary>An offer has been received.</summary>
    Offer = 5,
    /// <summary>The opportunity is no longer active.</summary>
    Closed = 6,
}
