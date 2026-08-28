using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>Coordinates contact use cases without moving contact-specific meaning into WinForms.</summary>
public sealed class ContactService(ITrackerDataStore store, IClock clock)
{
    /// <summary>Returns contacts for list and selection controls.</summary>
    public Task<IReadOnlyList<Contact>> ListAsync(bool includeArchived = false, CancellationToken cancellationToken = default)
        => store.ListContactsAsync(includeArchived, cancellationToken);

    /// <summary>Creates a professional contact.</summary>
    public async Task<Contact> CreateAsync(ContactInput input, CancellationToken cancellationToken = default)
    {
        await ValidateOrganizationAsync(input.OrganizationId, cancellationToken).ConfigureAwait(false);
        var now = clock.UtcNow;
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            OrganizationId = input.OrganizationId,
            FullName = Validation.Required(input.FullName, "Name", 200),
            Role = Validation.Optional(input.Role, "Rolle", 200),
            Email = Validation.Email(input.Email),
            Phone = Validation.Optional(input.Phone, "Telefon", 100),
            LinkedInUrl = Validation.Url(input.LinkedInUrl, "LinkedIn-URL"),
            Notes = Validation.Optional(input.Notes, "Notizen", 4000),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await store.AddContactAsync(contact, cancellationToken).ConfigureAwait(false);
        return contact;
    }

    /// <summary>Updates an existing professional contact.</summary>
    public async Task UpdateAsync(Guid id, ContactInput input, CancellationToken cancellationToken = default)
    {
        await ValidateOrganizationAsync(input.OrganizationId, cancellationToken).ConfigureAwait(false);
        var contact = await store.GetContactAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Der Kontakt wurde nicht gefunden.");

        contact.OrganizationId = input.OrganizationId;
        contact.FullName = Validation.Required(input.FullName, "Name", 200);
        contact.Role = Validation.Optional(input.Role, "Rolle", 200);
        contact.Email = Validation.Email(input.Email);
        contact.Phone = Validation.Optional(input.Phone, "Telefon", 100);
        contact.LinkedInUrl = Validation.Url(input.LinkedInUrl, "LinkedIn-URL");
        contact.Notes = Validation.Optional(input.Notes, "Notizen", 4000);
        contact.UpdatedAtUtc = clock.UtcNow;

        await store.UpdateContactAsync(contact, cancellationToken).ConfigureAwait(false);
    }

    private async Task ValidateOrganizationAsync(Guid? organizationId, CancellationToken cancellationToken)
    {
        if (organizationId is not null &&
            await store.GetOrganizationAsync(organizationId.Value, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Die zugeordnete Organisation wurde nicht gefunden.");
        }
    }
}
