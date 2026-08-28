using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Represents an organization involved in the job-search process, such as an employer,
/// recruiting company, or public authority.
/// </summary>
public sealed class Organization
{
    /// <summary>Gets or sets the stable technical identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the display name of the organization.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the role the organization normally has in the process.</summary>
    public OrganizationType Type { get; set; }

    /// <summary>Gets or sets the optional website used as a human reference.</summary>
    public string? Website { get; set; }

    /// <summary>Gets or sets free-form notes that belong to the organization itself.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets whether the organization is hidden from normal active lists.</summary>
    public bool IsArchived { get; set; }

    /// <summary>Gets or sets when this record was first created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets when this record was most recently changed.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
