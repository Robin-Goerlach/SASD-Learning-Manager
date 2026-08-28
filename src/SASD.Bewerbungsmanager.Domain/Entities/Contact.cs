namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Represents a real professional contact. Contacts are modeled independently so that
/// conversations and responsibilities are not reduced to text fields on an application.
/// </summary>
public sealed class Contact
{
    /// <summary>Gets or sets the stable technical identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the optional organization this person belongs to.</summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>Gets or sets the person's full display name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the professional role or function, for example Recruiter.</summary>
    public string? Role { get; set; }

    /// <summary>Gets or sets the e-mail address when it is relevant to the job-search process.</summary>
    public string? Email { get; set; }

    /// <summary>Gets or sets the telephone number when it is relevant to the process.</summary>
    public string? Phone { get; set; }

    /// <summary>Gets or sets the optional LinkedIn profile URL.</summary>
    public string? LinkedInUrl { get; set; }

    /// <summary>Gets or sets concise professional notes about this contact.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets whether the contact is hidden from normal active lists.</summary>
    public bool IsArchived { get; set; }

    /// <summary>Gets or sets when this record was first created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets when this record was most recently changed.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
