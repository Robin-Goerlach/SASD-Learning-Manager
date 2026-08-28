using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>Coordinates creation, editing, and source capture for job opportunities.</summary>
public sealed class OpportunityService(ITrackerDataStore store, IClock clock)
{
    /// <summary>Returns all opportunities ordered by the persistence adapter.</summary>
    public Task<IReadOnlyList<Opportunity>> ListAsync(CancellationToken cancellationToken = default)
        => store.ListOpportunitiesAsync(cancellationToken);

    /// <summary>Creates an opportunity including a durable snapshot of its role description.</summary>
    public async Task<Opportunity> CreateAsync(OpportunityInput input, CancellationToken cancellationToken = default)
    {
        await ValidateOrganizationsAsync(input.EmployerOrganizationId, input.IntermediaryOrganizationId, cancellationToken).ConfigureAwait(false);
        ValidateDates(input);
        var now = clock.UtcNow;
        var opportunity = new Opportunity
        {
            Id = Guid.NewGuid(),
            EmployerOrganizationId = input.EmployerOrganizationId,
            IntermediaryOrganizationId = input.IntermediaryOrganizationId,
            Title = Validation.Required(input.Title, "Position", 250),
            DescriptionSnapshot = Validation.Required(input.DescriptionSnapshot, "Rollenbeschreibung", 100_000),
            Location = Validation.Optional(input.Location, "Standort", 250),
            RemoteText = Validation.Optional(input.RemoteText, "Remote/Hybrid", 250),
            SalaryText = Validation.Optional(input.SalaryText, "Gehalt", 250),
            Status = input.Status,
            FoundAtUtc = input.FoundAtUtc,
            PublishedAtUtc = input.PublishedAtUtc,
            DeadlineAtUtc = input.DeadlineAtUtc,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await store.AddOpportunityAsync(opportunity, cancellationToken).ConfigureAwait(false);
        return opportunity;
    }

    /// <summary>Updates the editable fields of an opportunity.</summary>
    public async Task UpdateAsync(Guid id, OpportunityInput input, CancellationToken cancellationToken = default)
    {
        await ValidateOrganizationsAsync(input.EmployerOrganizationId, input.IntermediaryOrganizationId, cancellationToken).ConfigureAwait(false);
        ValidateDates(input);
        var opportunity = await store.GetOpportunityAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Stelle wurde nicht gefunden.");

        opportunity.EmployerOrganizationId = input.EmployerOrganizationId;
        opportunity.IntermediaryOrganizationId = input.IntermediaryOrganizationId;
        opportunity.Title = Validation.Required(input.Title, "Position", 250);
        opportunity.DescriptionSnapshot = Validation.Required(input.DescriptionSnapshot, "Rollenbeschreibung", 100_000);
        opportunity.Location = Validation.Optional(input.Location, "Standort", 250);
        opportunity.RemoteText = Validation.Optional(input.RemoteText, "Remote/Hybrid", 250);
        opportunity.SalaryText = Validation.Optional(input.SalaryText, "Gehalt", 250);
        opportunity.Status = input.Status;
        opportunity.FoundAtUtc = input.FoundAtUtc;
        opportunity.PublishedAtUtc = input.PublishedAtUtc;
        opportunity.DeadlineAtUtc = input.DeadlineAtUtc;
        opportunity.UpdatedAtUtc = clock.UtcNow;

        await store.UpdateOpportunityAsync(opportunity, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Adds a source URL without making that URL the historical source of truth for the role description.</summary>
    public async Task<SourceLink> AddSourceLinkAsync(Guid opportunityId, SourceLinkInput input, CancellationToken cancellationToken = default)
    {
        if (await store.GetOpportunityAsync(opportunityId, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Die Stelle wurde nicht gefunden.");
        }

        var sourceLink = new SourceLink
        {
            Id = Guid.NewGuid(),
            OpportunityId = opportunityId,
            Source = Validation.Required(input.Source, "Quelle", 100),
            Url = Validation.Url(input.Url, "URL")
                ?? throw new ValidationException("URL ist erforderlich."),
            ExternalId = Validation.Optional(input.ExternalId, "Externe ID", 250),
            CapturedAtUtc = clock.UtcNow,
        };

        await store.AddSourceLinkAsync(sourceLink, cancellationToken).ConfigureAwait(false);
        return sourceLink;
    }

    /// <summary>Returns source references for a selected opportunity.</summary>
    public Task<IReadOnlyList<SourceLink>> ListSourceLinksAsync(Guid opportunityId, CancellationToken cancellationToken = default)
        => store.ListSourceLinksAsync(opportunityId, cancellationToken);

    private async Task ValidateOrganizationsAsync(Guid? employerId, Guid? intermediaryId, CancellationToken cancellationToken)
    {
        if (employerId is not null && await store.GetOrganizationAsync(employerId.Value, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Der Arbeitgeber wurde nicht gefunden.");
        }

        if (intermediaryId is not null && await store.GetOrganizationAsync(intermediaryId.Value, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Der Vermittler wurde nicht gefunden.");
        }

        if (employerId is not null && employerId == intermediaryId)
        {
            throw new ValidationException("Arbeitgeber und Vermittler sollen getrennte Organisationen sein.");
        }
    }

    private static void ValidateDates(OpportunityInput input)
    {
        if (input.DeadlineAtUtc is not null && input.DeadlineAtUtc < input.FoundAtUtc)
        {
            throw new ValidationException("Die Bewerbungsfrist darf nicht vor dem Funddatum liegen.");
        }
    }
}
