using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>Coordinates organization use cases and their validation rules.</summary>
public sealed class OrganizationService(ITrackerDataStore store, IClock clock)
{
    /// <summary>Returns organizations for list and selection controls.</summary>
    public Task<IReadOnlyList<Organization>> ListAsync(bool includeArchived = false, CancellationToken cancellationToken = default)
        => store.ListOrganizationsAsync(includeArchived, cancellationToken);

    /// <summary>Creates a new organization with normalized user input.</summary>
    public async Task<Organization> CreateAsync(OrganizationInput input, CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = Validation.Required(input.Name, "Name", 200),
            Type = input.Type,
            Website = Validation.Url(input.Website, "Website"),
            Notes = Validation.Optional(input.Notes, "Notizen", 4000),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await store.AddOrganizationAsync(organization, cancellationToken).ConfigureAwait(false);
        return organization;
    }

    /// <summary>Updates an existing organization while preserving its identity and creation time.</summary>
    public async Task UpdateAsync(Guid id, OrganizationInput input, CancellationToken cancellationToken = default)
    {
        var organization = await store.GetOrganizationAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Organisation wurde nicht gefunden.");

        organization.Name = Validation.Required(input.Name, "Name", 200);
        organization.Type = input.Type;
        organization.Website = Validation.Url(input.Website, "Website");
        organization.Notes = Validation.Optional(input.Notes, "Notizen", 4000);
        organization.UpdatedAtUtc = clock.UtcNow;

        await store.UpdateOrganizationAsync(organization, cancellationToken).ConfigureAwait(false);
    }
}
