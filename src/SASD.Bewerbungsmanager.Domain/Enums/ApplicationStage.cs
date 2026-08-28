namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>
/// Represents the current stage of a concrete application.
/// </summary>
public enum ApplicationStage
{
    /// <summary>The application is being prepared.</summary>
    Draft = 0,
    /// <summary>The application has been submitted.</summary>
    Submitted = 1,
    /// <summary>The employer or intermediary is screening the application.</summary>
    Screening = 2,
    /// <summary>At least one interview is planned or taking place.</summary>
    Interview = 3,
    /// <summary>An offer has been received.</summary>
    Offer = 4,
    /// <summary>The application has been rejected.</summary>
    Rejected = 5,
    /// <summary>The application was withdrawn by the applicant.</summary>
    Withdrawn = 6,
    /// <summary>The application resulted in a hire.</summary>
    Hired = 7,
    /// <summary>The application is closed for another reason.</summary>
    Closed = 8,
}
