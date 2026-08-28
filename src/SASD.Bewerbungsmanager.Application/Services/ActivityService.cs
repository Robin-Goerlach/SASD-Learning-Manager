using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>Coordinates timeline and appointment use cases.</summary>
public sealed class ActivityService(ITrackerDataStore store, IClock clock)
{
    /// <summary>Returns all activities in reverse chronological/appointment order.</summary>
    public Task<IReadOnlyList<Activity>> ListAsync(CancellationToken cancellationToken = default)
        => store.ListActivitiesAsync(cancellationToken);

    /// <summary>Creates a historical timeline entry or a planned appointment.</summary>
    public async Task<Activity> CreateAsync(ActivityInput input, CancellationToken cancellationToken = default)
    {
        await ValidateReferencesAsync(input, cancellationToken).ConfigureAwait(false);
        ValidateTiming(input);

        var now = clock.UtcNow;
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            OpportunityId = input.OpportunityId,
            ApplicationId = input.ApplicationId,
            ContactId = input.ContactId,
            OrganizationId = input.OrganizationId,
            Kind = input.Kind,
            Status = input.Status,
            Subject = Validation.Required(input.Subject, "Betreff", 250),
            Notes = Validation.Optional(input.Notes, "Notizen", 8000),
            OccurredAtUtc = input.OccurredAtUtc,
            ScheduledAtUtc = input.ScheduledAtUtc,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await store.AddActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        return activity;
    }

    /// <summary>Marks a planned activity as completed.</summary>
    public async Task CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await store.GetActivityAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Aktivität wurde nicht gefunden.");

        activity.Complete(clock.UtcNow);
        await store.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Marks a planned activity as cancelled.</summary>
    public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await store.GetActivityAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Aktivität wurde nicht gefunden.");

        activity.Cancel(clock.UtcNow);
        await store.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);
    }

    private async Task ValidateReferencesAsync(ActivityInput input, CancellationToken cancellationToken)
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

    private static void ValidateTiming(ActivityInput input)
    {
        if (input.Status is not ActivityStatus.Recorded and not ActivityStatus.Planned)
        {
            throw new ValidationException("Neue Aktivitäten müssen als stattgefunden oder geplant erfasst werden.");
        }

        if (input.Status == ActivityStatus.Recorded && input.OccurredAtUtc is null)
        {
            throw new ValidationException("Für eine stattgefundene Aktivität ist ein Zeitpunkt erforderlich.");
        }

        if (input.Status == ActivityStatus.Planned && input.ScheduledAtUtc is null)
        {
            throw new ValidationException("Für einen geplanten Termin ist ein Zeitpunkt erforderlich.");
        }
    }
}
