namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>Classifies imported communication for the job-search workflow.</summary>
public enum CommunicationKind
{
    /// <summary>No reliable classification was supplied or inferred yet.</summary>
    Unclassified = 0,

    /// <summary>A direct recruiter, HR, or hiring-manager communication.</summary>
    Recruiter = 1,

    /// <summary>A response that directly belongs to an existing application process.</summary>
    ApplicationResponse = 2,

    /// <summary>A job-alert or automated job recommendation containing possible opportunities.</summary>
    JobAlert = 3,

    /// <summary>Other communication that is still useful to keep in the local communication inbox.</summary>
    General = 4,
}
