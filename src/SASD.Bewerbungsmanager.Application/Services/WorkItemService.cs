using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>Coordinates ACTION and WAITING_FOR work items.</summary>
public sealed class WorkItemService(ITrackerDataStore store, IClock clock)
{
    /// <summary>Returns all work items, including completed history.</summary>
    public Task<IReadOnlyList<TrackerTask>> ListAsync(CancellationToken cancellationToken = default)
        => store.ListTasksAsync(cancellationToken);

    /// <summary>Creates a new open next-step item.</summary>
    public async Task<TrackerTask> CreateAsync(WorkItemInput input, CancellationToken cancellationToken = default)
    {
        await ValidateReferencesAsync(input, cancellationToken).ConfigureAwait(false);
        var now = clock.UtcNow;
        var item = new TrackerTask
        {
            Id = Guid.NewGuid(),
            OpportunityId = input.OpportunityId,
            ApplicationId = input.ApplicationId,
            ContactId = input.ContactId,
            OrganizationId = input.OrganizationId,
            Kind = input.Kind,
            Status = WorkItemStatus.Open,
            Title = Validation.Required(input.Title, "Aufgabe", 250),
            Notes = Validation.Optional(input.Notes, "Notizen", 8000),
            DueAtUtc = input.DueAtUtc,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await store.AddTaskAsync(item, cancellationToken).ConfigureAwait(false);
        return item;
    }

    /// <summary>Completes an open ACTION or WAITING_FOR item.</summary>
    public async Task CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await store.GetTaskAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Aufgabe wurde nicht gefunden.");

        item.Complete(clock.UtcNow);
        await store.UpdateTaskAsync(item, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Cancels an open work item that is no longer relevant.</summary>
    public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await store.GetTaskAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Aufgabe wurde nicht gefunden.");

        item.Cancel(clock.UtcNow);
        await store.UpdateTaskAsync(item, cancellationToken).ConfigureAwait(false);
    }

    private async Task ValidateReferencesAsync(WorkItemInput input, CancellationToken cancellationToken)
    {
        if (input.OpportunityId is Guid opportunityId &&
            await store.GetOpportunityAsync(opportunityId, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Die zugeordnete Stelle wurde nicht gefunden.");
        }

        if (input.ApplicationId is Guid applicationId)
        {
            var application = await store.GetApplicationAsync(applicationId, cancellationToken).ConfigureAwait(false)
                ?? throw new KeyNotFoundException("Die zugeordnete Bewerbung wurde nicht gefunden.");
            if (input.OpportunityId is Guid selectedOpportunityId && application.OpportunityId != selectedOpportunityId)
            {
                throw new ValidationException("Die ausgewählte Bewerbung gehört nicht zur ausgewählten Stelle.");
            }
        }

        if (input.ContactId is Guid contactId &&
            await store.GetContactAsync(contactId, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Der zugeordnete Kontakt wurde nicht gefunden.");
        }

        if (input.OrganizationId is Guid organizationId &&
            await store.GetOrganizationAsync(organizationId, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Die zugeordnete Organisation wurde nicht gefunden.");
        }
    }
}
