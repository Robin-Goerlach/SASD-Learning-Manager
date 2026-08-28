namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>
/// Identifies the channel through which an application was initiated or submitted.
/// </summary>
public enum ApplicationChannel
{
    /// <summary>No specific channel is known yet.</summary>
    Unknown = 0,
    /// <summary>Application through an employer or job-board portal.</summary>
    Portal = 1,
    /// <summary>Application by e-mail.</summary>
    Email = 2,
    /// <summary>Application through LinkedIn.</summary>
    LinkedIn = 3,
    /// <summary>Application initiated through a recruiter or staffing intermediary.</summary>
    Recruiter = 4,
    /// <summary>Any other channel.</summary>
    Other = 5,
}
