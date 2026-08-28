namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>Classifies documents used during the application process.</summary>
public enum DocumentType
{
    /// <summary>Curriculum vitae / resume.</summary>
    Cv,

    /// <summary>Application or cover letter.</summary>
    CoverLetter,

    /// <summary>Certificate, reference, or testimonial.</summary>
    Certificate,

    /// <summary>Captured job advertisement or role description file.</summary>
    JobAdvertisement,

    /// <summary>Any other relevant document.</summary>
    Other,
}
