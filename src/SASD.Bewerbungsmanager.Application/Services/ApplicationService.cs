using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>Coordinates use cases around concrete applications and their stage history.</summary>
public sealed class ApplicationService(ITrackerDataStore store, IClock clock)
{
    /// <summary>Returns all concrete applications.</summary>
    public Task<IReadOnlyList<JobApplication>> ListAsync(CancellationToken cancellationToken = default)
        => store.ListApplicationsAsync(cancellationToken);

    /// <summary>Creates a concrete application and its initial immutable status-history entry.</summary>
    public async Task<JobApplication> CreateAsync(ApplicationInput input, CancellationToken cancellationToken = default)
    {
        if (await store.GetOpportunityAsync(input.OpportunityId, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Die zugehörige Stelle wurde nicht gefunden.");
        }

        if (input.SubmittedAtUtc is not null && input.SubmittedAtUtc < input.StartedAtUtc)
        {
            throw new ValidationException("Das Versanddatum darf nicht vor dem Start der Bewerbung liegen.");
        }

        var now = clock.UtcNow;
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            OpportunityId = input.OpportunityId,
            StartedAtUtc = input.StartedAtUtc,
            SubmittedAtUtc = input.SubmittedAtUtc,
            Channel = input.Channel,
            SalaryExpectation = Validation.Optional(input.SalaryExpectation, "Gehaltsvorstellung", 250),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        application.InitializeStage(input.Stage, now, "Bewerbung angelegt");

        await store.AddApplicationAsync(application, cancellationToken).ConfigureAwait(false);
        return application;
    }

    /// <summary>
    /// Updates the factual submission timestamp and channel used for evidence/export. A nullable
    /// timestamp allows a mistakenly entered submission to be cleared deliberately.
    /// </summary>
    public async Task UpdateSubmissionAsync(
        Guid applicationId,
        ApplicationSubmissionInput input,
        CancellationToken cancellationToken = default)
    {
        var application = await store.GetApplicationAsync(applicationId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Bewerbung wurde nicht gefunden.");
        if (input.SubmittedAtUtc is not null && input.SubmittedAtUtc < application.StartedAtUtc)
        {
            throw new ValidationException("Das Versanddatum darf nicht vor dem Start der Bewerbung liegen.");
        }

        await store.UpdateApplicationSubmissionAsync(
            applicationId,
            input.SubmittedAtUtc,
            input.Channel,
            clock.UtcNow,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Changes the application stage in one persistence operation so the current state and history
    /// entry cannot drift apart because of two separate saves.
    /// </summary>
    public Task ChangeStageAsync(Guid applicationId, ApplicationStage stage, string? note = null, CancellationToken cancellationToken = default)
        => store.ChangeApplicationStageAsync(applicationId, stage, clock.UtcNow, Validation.Optional(note, "Statusnotiz", 2000), cancellationToken);
}
