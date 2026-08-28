using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>Coordinates manual search-source routines without introducing scraping.</summary>
public sealed class SearchProfileService(ITrackerDataStore store, IClock clock)
{
    /// <summary>Returns active search profiles by default.</summary>
    public Task<IReadOnlyList<SearchProfile>> ListAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        => store.ListSearchProfilesAsync(includeInactive, cancellationToken);

    /// <summary>Creates a new search routine.</summary>
    public async Task<SearchProfile> CreateAsync(SearchProfileInput input, CancellationToken cancellationToken = default)
    {
        ValidateInterval(input.CheckIntervalDays);
        var now = clock.UtcNow;
        var profile = new SearchProfile
        {
            Id = Guid.NewGuid(),
            Name = Validation.Required(input.Name, "Name", 200),
            Source = Validation.Required(input.Source, "Quelle", 100),
            Url = Validation.Url(input.Url, "URL")
                ?? throw new ValidationException("URL ist erforderlich."),
            CheckIntervalDays = input.CheckIntervalDays,
            NextCheckAtUtc = input.NextCheckAtUtc,
            IsActive = input.IsActive,
            Notes = Validation.Optional(input.Notes, "Notizen", 4000),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await store.AddSearchProfileAsync(profile, cancellationToken).ConfigureAwait(false);
        return profile;
    }

    /// <summary>Updates the editable definition of a search routine.</summary>
    public async Task UpdateAsync(Guid id, SearchProfileInput input, CancellationToken cancellationToken = default)
    {
        ValidateInterval(input.CheckIntervalDays);
        var profile = await store.GetSearchProfileAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Das Suchprofil wurde nicht gefunden.");

        profile.Name = Validation.Required(input.Name, "Name", 200);
        profile.Source = Validation.Required(input.Source, "Quelle", 100);
        profile.Url = Validation.Url(input.Url, "URL")
            ?? throw new ValidationException("URL ist erforderlich.");
        profile.CheckIntervalDays = input.CheckIntervalDays;
        profile.NextCheckAtUtc = input.NextCheckAtUtc;
        profile.IsActive = input.IsActive;
        profile.Notes = Validation.Optional(input.Notes, "Notizen", 4000);
        profile.UpdatedAtUtc = clock.UtcNow;

        await store.UpdateSearchProfileAsync(profile, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Marks a search source checked today and advances its next-check date.</summary>
    public async Task MarkCheckedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await store.GetSearchProfileAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Das Suchprofil wurde nicht gefunden.");

        profile.MarkChecked(clock.UtcNow);
        await store.UpdateSearchProfileAsync(profile, cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateInterval(int days)
    {
        if (days is < 1 or > 365)
        {
            throw new ValidationException("Das Prüfintervall muss zwischen 1 und 365 Tagen liegen.");
        }
    }
}
