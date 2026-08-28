namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>Classifies an entry in the professional activity timeline.</summary>
public enum ActivityKind
{
    /// <summary>An e-mail was sent or received.</summary>
    Email,

    /// <summary>A telephone conversation took place or is planned.</summary>
    PhoneCall,

    /// <summary>A LinkedIn message or interaction was relevant to the process.</summary>
    LinkedIn,

    /// <summary>A concrete application was submitted.</summary>
    ApplicationSubmitted,

    /// <summary>A job interview took place or is planned.</summary>
    Interview,

    /// <summary>A general meeting took place or is planned.</summary>
    Meeting,

    /// <summary>An appointment with the Agentur für Arbeit or another authority.</summary>
    AuthorityAppointment,

    /// <summary>A free-form note that belongs in the chronology.</summary>
    Note,

    /// <summary>Any other activity that is relevant to the job-search process.</summary>
    Other,
}
