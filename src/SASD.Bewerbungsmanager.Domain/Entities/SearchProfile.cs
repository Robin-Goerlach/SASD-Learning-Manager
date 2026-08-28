namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Represents one manually checked job-search source or saved search. Milestone 2 intentionally
/// does not scrape portals; it only remembers when the user should check the source again.
/// </summary>
public sealed class SearchProfile
{
    /// <summary>Gets or sets the stable technical identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets a human-readable name for this search routine.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the source/portal name, for example LinkedIn or BA Jobsuche.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Gets or sets the saved search or careers-page URL.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Gets or sets the normal interval between manual checks.</summary>
    public int CheckIntervalDays { get; set; }

    /// <summary>Gets or sets when the source was last checked.</summary>
    public DateTimeOffset? LastCheckedAtUtc { get; set; }

    /// <summary>Gets or sets when the source should next be checked.</summary>
    public DateTimeOffset NextCheckAtUtc { get; set; }

    /// <summary>Gets or sets whether this search routine should appear in operational views.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets optional notes about search terms or filters.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets when this record was first created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets when this record was most recently changed.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>Records that this source was checked and schedules the next regular check.</summary>
    /// <param name="checkedAtUtc">Time at which the user completed the check.</param>
    public void MarkChecked(DateTimeOffset checkedAtUtc)
    {
        if (CheckIntervalDays <= 0)
        {
            throw new InvalidOperationException("Das Prüfintervall muss größer als null sein.");
        }

        LastCheckedAtUtc = checkedAtUtc;
        NextCheckAtUtc = checkedAtUtc.AddDays(CheckIntervalDays);
        UpdatedAtUtc = checkedAtUtc;
    }
}
