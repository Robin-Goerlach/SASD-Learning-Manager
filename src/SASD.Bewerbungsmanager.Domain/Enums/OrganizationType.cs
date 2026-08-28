namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>
/// Describes the business role an organization has in the job-search process.
/// </summary>
public enum OrganizationType
{
    /// <summary>The organization is or may become the actual employer.</summary>
    Employer = 0,

    /// <summary>The organization acts as recruiter, staffing company, or intermediary.</summary>
    Recruiter = 1,

    /// <summary>The organization is a public authority, for example the Agentur für Arbeit.</summary>
    PublicAgency = 2,

    /// <summary>The organization does not fit one of the more specific categories.</summary>
    Other = 3,
}
