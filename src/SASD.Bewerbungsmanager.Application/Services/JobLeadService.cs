using System.Security.Cryptography;
using System.Text;
using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>
/// Coordinates the v0.4 job-search inbox. Source adapters hand normalized results to this service;
/// it performs validation and deterministic deduplication before a user decides whether a result
/// should become a durable <see cref="Opportunity"/>.
/// </summary>
public sealed class JobLeadService(
    ITrackerDataStore store,
    IClock clock,
    IEnumerable<IJobSourceReader> readers,
    OpportunityService opportunityService,
    SearchProfileService searchProfileService)
{
    /// <summary>Returns imported job leads, newest first. Ignored results are hidden by default.</summary>
    public async Task<IReadOnlyList<JobLead>> ListAsync(
        bool includeIgnored = false,
        CancellationToken cancellationToken = default)
    {
        var items = await store.ListJobLeadsAsync(cancellationToken).ConfigureAwait(false);
        return items
            .Where(item => includeIgnored || item.Status != JobLeadStatus.Ignored)
            .ToList();
    }

    /// <summary>Imports a local JSON/CSV handoff using the first registered adapter that supports the file.</summary>
    public Task<JobLeadBatchImportResult> ImportFileAsync(
        string path,
        CancellationToken cancellationToken = default)
        => ImportFileAsync(path, searchProfileOverride: null, cancellationToken: cancellationToken);

    /// <summary>
    /// Imports a local JSON/CSV handoff and optionally assigns all imported records to a user-selected
    /// search profile. The explicit UI choice takes precedence over an id embedded in the file.
    /// </summary>
    public async Task<JobLeadBatchImportResult> ImportFileAsync(
        string path,
        Guid? searchProfileOverride,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ValidationException("Eine Importdatei ist erforderlich.");
        }

        var reader = readers.FirstOrDefault(item => item.CanRead(path))
            ?? throw new ValidationException("Für dieses Dateiformat ist kein Quellenadapter registriert.");
        var batch = await reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (searchProfileOverride is not null)
        {
            batch = batch with { SearchProfileId = searchProfileOverride };
        }
        return await ImportBatchAsync(batch, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Imports one versioned normalized batch and advances its search profile when supplied.</summary>
    public async Task<JobLeadBatchImportResult> ImportBatchAsync(
        JobSourceBatch batch,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);
        if (batch.SchemaVersion != 1)
        {
            throw new ValidationException($"Job-Quellen-Handoff-Version {batch.SchemaVersion} wird nicht unterstützt.");
        }

        var sourceSystem = Validation.Required(batch.SourceSystem, "Quellsystem", 100);
        if (batch.SearchProfileId is Guid profileId &&
            await store.GetSearchProfileAsync(profileId, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Das im Quellenpaket angegebene Suchprofil wurde nicht gefunden.");
        }

        if (batch.Items.Count == 0)
        {
            if (batch.SearchProfileId is Guid emptyProfileId)
            {
                await searchProfileService.MarkCheckedAsync(emptyProfileId, cancellationToken).ConfigureAwait(false);
            }
            return new JobLeadBatchImportResult(0, 0, []);
        }

        var foundAtUtc = batch.CapturedAtUtc == default ? clock.UtcNow : batch.CapturedAtUtc;
        var results = new List<JobLeadImportItemResult>(batch.Items.Count);
        foreach (var item in batch.Items)
        {
            results.Add(await ImportItemAsync(
                batch.SearchProfileId,
                sourceSystem,
                item,
                foundAtUtc,
                cancellationToken).ConfigureAwait(false));
        }

        // A successful source import counts as a completed check even when all records were duplicates.
        // This prevents a repeatedly idempotent feed from remaining permanently overdue in "Heute".
        if (batch.SearchProfileId is Guid checkedProfileId)
        {
            await searchProfileService.MarkCheckedAsync(checkedProfileId, cancellationToken).ConfigureAwait(false);
        }

        return new JobLeadBatchImportResult(
            results.Count(item => !item.WasDuplicate),
            results.Count(item => item.WasDuplicate),
            results);
    }

    /// <summary>Imports one manually copied job result without requiring a source-adapter file.</summary>
    public async Task<JobLeadImportItemResult> ImportClipboardAsync(
        JobLeadClipboardInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.SearchProfileId is Guid profileId &&
            await store.GetSearchProfileAsync(profileId, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Das ausgewählte Suchprofil wurde nicht gefunden.");
        }

        var item = new JobSourceItem(
            input.ExternalJobId,
            input.Title,
            input.OrganizationName,
            input.Location,
            input.RemoteText,
            input.SalaryText,
            input.Url,
            input.DescriptionText,
            input.PublishedAtUtc);

        var result = await ImportItemAsync(
            input.SearchProfileId,
            Validation.Required(input.SourceSystem, "Quellsystem", 100),
            item,
            clock.UtcNow,
            cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>Marks a lead as reviewed without promoting it.</summary>
    public async Task MarkReviewedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await RequireLeadAsync(id, cancellationToken).ConfigureAwait(false);
        lead.MarkReviewed(clock.UtcNow);
        await store.UpdateJobLeadAsync(lead, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Dismisses a discovered job from the active job-search inbox.</summary>
    public async Task IgnoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await RequireLeadAsync(id, cancellationToken).ConfigureAwait(false);
        lead.Ignore(clock.UtcNow);
        await store.UpdateJobLeadAsync(lead, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Promotes a discovered job into the durable opportunity workflow and preserves the original
    /// source URL as a <see cref="SourceLink"/> when one was captured.
    /// </summary>
    public async Task<Opportunity> PromoteAsync(
        Guid id,
        JobLeadOpportunityInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var lead = await RequireLeadAsync(id, cancellationToken).ConfigureAwait(false);
        if (lead.Status == JobLeadStatus.Imported && lead.OpportunityId is Guid existingOpportunityId)
        {
            return await store.GetOpportunityAsync(existingOpportunityId, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Die verknüpfte Stelle wurde nicht gefunden.");
        }

        var opportunity = await opportunityService.CreateAsync(
            new OpportunityInput(
                input.EmployerOrganizationId,
                input.IntermediaryOrganizationId,
                lead.Title,
                BuildDescriptionSnapshot(lead),
                lead.Location,
                lead.RemoteText,
                lead.SalaryText,
                OpportunityStatus.Identified,
                lead.FoundAtUtc,
                lead.PublishedAtUtc,
                DeadlineAtUtc: null),
            cancellationToken).ConfigureAwait(false);

        if (lead.SourceUrl is not null)
        {
            await opportunityService.AddSourceLinkAsync(
                opportunity.Id,
                new SourceLinkInput(lead.SourceSystem, lead.SourceUrl, lead.ExternalJobId),
                cancellationToken).ConfigureAwait(false);
        }

        lead.LinkOpportunity(opportunity.Id, clock.UtcNow);
        await store.UpdateJobLeadAsync(lead, cancellationToken).ConfigureAwait(false);
        return opportunity;
    }

    private async Task<JobLeadImportItemResult> ImportItemAsync(
        Guid? searchProfileId,
        string sourceSystem,
        JobSourceItem input,
        DateTimeOffset foundAtUtc,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var title = Validation.Required(input.Title, "Position", 250);
        var externalJobId = Validation.Optional(input.ExternalJobId, "Externe Stellen-ID", 250);
        var organizationName = Validation.Optional(input.OrganizationName, "Organisation", 250);
        var location = Validation.Optional(input.Location, "Standort", 250);
        var remoteText = Validation.Optional(input.RemoteText, "Remote/Hybrid", 250);
        var salaryText = Validation.Optional(input.SalaryText, "Gehalt", 250);
        var sourceUrl = Validation.Url(input.Url, "Stellen-URL");
        var description = Validation.Optional(input.DescriptionText, "Stellenbeschreibung", 100_000);
        var canonicalUrl = CanonicalizeUrl(sourceUrl);
        var fingerprint = ComputeFingerprint(sourceSystem, externalJobId, title, organizationName, canonicalUrl);

        JobLead? duplicate = null;
        if (externalJobId is not null)
        {
            duplicate = await store.FindJobLeadByExternalIdentityAsync(
                sourceSystem,
                externalJobId,
                cancellationToken).ConfigureAwait(false);
        }

        if (duplicate is null && canonicalUrl is not null)
        {
            duplicate = await store.FindJobLeadBySourceUrlAsync(canonicalUrl, cancellationToken).ConfigureAwait(false);
        }

        duplicate ??= await store.FindJobLeadByFingerprintAsync(fingerprint, cancellationToken).ConfigureAwait(false);
        if (duplicate is not null)
        {
            return new JobLeadImportItemResult(duplicate, WasDuplicate: true);
        }

        var now = clock.UtcNow;
        var lead = new JobLead
        {
            Id = Guid.NewGuid(),
            SearchProfileId = searchProfileId,
            SourceSystem = sourceSystem,
            ExternalJobId = externalJobId,
            FingerprintSha256 = fingerprint,
            Title = title,
            OrganizationName = organizationName,
            Location = location,
            RemoteText = remoteText,
            SalaryText = salaryText,
            SourceUrl = canonicalUrl,
            DescriptionText = description,
            PublishedAtUtc = input.PublishedAtUtc,
            FoundAtUtc = foundAtUtc,
            Status = JobLeadStatus.New,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await store.AddJobLeadAsync(lead, cancellationToken).ConfigureAwait(false);
        return new JobLeadImportItemResult(lead, WasDuplicate: false);
    }

    private async Task<JobLead> RequireLeadAsync(Guid id, CancellationToken cancellationToken)
        => await store.GetJobLeadAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Der gefundene Job wurde nicht gefunden.");

    private static string BuildDescriptionSnapshot(JobLead lead)
    {
        if (!string.IsNullOrWhiteSpace(lead.DescriptionText))
        {
            return lead.DescriptionText;
        }

        var builder = new StringBuilder();
        builder.AppendLine(lead.Title);
        if (!string.IsNullOrWhiteSpace(lead.OrganizationName))
        {
            builder.AppendLine($"Organisation: {lead.OrganizationName}");
        }
        builder.AppendLine($"Quelle: {lead.SourceSystem}");
        if (!string.IsNullOrWhiteSpace(lead.SourceUrl))
        {
            builder.AppendLine($"URL: {lead.SourceUrl}");
        }
        builder.AppendLine();
        builder.AppendLine("Die Quelle enthielt beim Import keine ausführliche Stellenbeschreibung.");
        return builder.ToString().Trim();
    }

    private static string ComputeFingerprint(
        string sourceSystem,
        string? externalJobId,
        string title,
        string? organizationName,
        string? canonicalUrl)
    {
        var normalized = string.Join("\n",
            sourceSystem.Trim().ToUpperInvariant(),
            externalJobId?.Trim().ToUpperInvariant() ?? string.Empty,
            title.Trim().ToUpperInvariant(),
            organizationName?.Trim().ToUpperInvariant() ?? string.Empty,
            canonicalUrl?.Trim().ToUpperInvariant() ?? string.Empty);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized))).ToLowerInvariant();
    }

    private static string? CanonicalizeUrl(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var uri = new Uri(value, UriKind.Absolute);
        var builder = new UriBuilder(uri)
        {
            Fragment = string.Empty,
            Host = uri.Host.ToLowerInvariant(),
        };

        var keptQuery = uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(part =>
            {
                var key = part.Split('=', 2)[0];
                return !key.StartsWith("utm_", StringComparison.OrdinalIgnoreCase) &&
                       !key.Equals("ref", StringComparison.OrdinalIgnoreCase) &&
                       !key.Equals("trackingId", StringComparison.OrdinalIgnoreCase);
            });
        builder.Query = string.Join("&", keptQuery);

        var canonical = builder.Uri.AbsoluteUri;
        return canonical.EndsWith("/", StringComparison.Ordinal) && builder.Path.Length > 1
            ? canonical.TrimEnd('/')
            : canonical;
    }
}
