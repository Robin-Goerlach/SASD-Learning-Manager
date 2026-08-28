using System.Security.Cryptography;
using System.Text;
using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Exceptions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>
/// Imports normalized mail/communication handoffs into the job-search workflow. Matching is deliberately
/// conservative: explicit identifiers and unique existing relations may be applied automatically, while
/// ambiguous situations remain visible for user confirmation instead of silently linking the wrong process.
/// </summary>
public sealed class CommunicationImportService(
    ITrackerDataStore store,
    IClock clock,
    ICommunicationHandoffReader handoffReader,
    ActivityService activityService,
    WorkItemService workItemService,
    OpportunityService opportunityService)
{
    private static readonly string[] JobAlertTerms =
    [
        "job alert", "job-alert", "stellenalarm", "stellen alert", "jobalarm",
        "neue stellen", "new jobs", "job recommendations", "jobs für sie", "jobs for you",
    ];

    private static readonly string[] ApplicationTerms =
    [
        "bewerbung", "application", "interview", "einladung", "candidate", "candidature",
        "vorstellungsgespräch", "recruiting process", "application process",
    ];

    /// <summary>Returns all imported communication items, newest message first.</summary>
    public Task<IReadOnlyList<CommunicationMessage>> ListAsync(CancellationToken cancellationToken = default)
        => store.ListCommunicationMessagesAsync(cancellationToken);

    /// <summary>Returns one imported communication by id.</summary>
    public Task<CommunicationMessage?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => store.GetCommunicationMessageAsync(id, cancellationToken);

    /// <summary>
    /// Imports one normalized message, performs deterministic deduplication, conservatively matches known
    /// contacts/context and creates an e-mail activity for direct recruiter/application communication.
    /// </summary>
    public async Task<CommunicationImportResult> ImportAsync(
        CommunicationImportInput input,
        CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(input);
        var fingerprint = ComputeFingerprint(normalized);

        CommunicationMessage? duplicate = null;
        if (!string.IsNullOrWhiteSpace(normalized.ExternalMessageId))
        {
            duplicate = await store.FindCommunicationMessageByExternalIdentityAsync(
                normalized.SourceSystem,
                normalized.ExternalMessageId!,
                cancellationToken).ConfigureAwait(false);
        }

        duplicate ??= await store.FindCommunicationMessageByFingerprintAsync(fingerprint, cancellationToken).ConfigureAwait(false);
        if (duplicate is not null)
        {
            return new CommunicationImportResult(
                duplicate,
                WasDuplicate: true,
                ContactMatchedAutomatically: false,
                ContextMatchedAutomatically: false,
                ActivityCreatedAutomatically: false,
                AnalyzeText(duplicate.Subject, duplicate.BodyText, duplicate.Kind).Urls);
        }

        var match = await MatchContextAsync(normalized, cancellationToken).ConfigureAwait(false);
        var kind = Classify(normalized, match.Contact is not null);
        var now = clock.UtcNow;
        var message = new CommunicationMessage
        {
            Id = Guid.NewGuid(),
            SourceSystem = normalized.SourceSystem,
            ExternalMessageId = normalized.ExternalMessageId,
            FingerprintSha256 = fingerprint,
            Direction = normalized.Direction,
            Kind = kind,
            Status = CommunicationStatus.Imported,
            FromName = normalized.FromName,
            FromAddress = normalized.FromAddress,
            ToAddresses = normalized.ToAddresses,
            Subject = normalized.Subject,
            BodyText = normalized.BodyText,
            MessageAtUtc = normalized.MessageAtUtc,
            SourceReference = normalized.SourceReference,
            OpportunityId = match.OpportunityId,
            ApplicationId = match.ApplicationId,
            ContactId = match.Contact?.Id,
            OrganizationId = match.OrganizationId,
            ImportedAtUtc = now,
            UpdatedAtUtc = now,
        };

        if (match.HasAnyContext)
        {
            message.Status = CommunicationStatus.Linked;
        }

        await store.AddCommunicationMessageAsync(message, cancellationToken).ConfigureAwait(false);

        var activityCreated = await EnsureTimelineActivityAsync(message, cancellationToken).ConfigureAwait(false);
        var analysis = AnalyzeText(message.Subject, message.BodyText, message.Kind);
        return new CommunicationImportResult(
            message,
            WasDuplicate: false,
            ContactMatchedAutomatically: match.ContactMatchedAutomatically,
            ContextMatchedAutomatically: match.ContextMatchedAutomatically,
            ActivityCreatedAutomatically: activityCreated,
            analysis.Urls);
    }

    /// <summary>Imports every message from a versioned SASD Mail Workbench handoff JSON file.</summary>
    public async Task<CommunicationBatchImportResult> ImportHandoffFileAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var batch = await handoffReader.ReadAsync(path, cancellationToken).ConfigureAwait(false);
        if (batch.SchemaVersion != 1)
        {
            throw new ValidationException($"Nicht unterstützte Kommunikations-Handoff-Version: {batch.SchemaVersion}.");
        }

        var sourceSystem = Validation.Required(batch.SourceSystem, "Quellsystem", 100);
        var results = new List<CommunicationImportResult>(batch.Messages.Count);
        foreach (var item in batch.Messages)
        {
            var result = await ImportAsync(
                new CommunicationImportInput(
                    sourceSystem,
                    item.ExternalMessageId,
                    item.Direction,
                    item.Kind,
                    item.FromName,
                    item.FromAddress,
                    item.ToAddresses,
                    item.Subject,
                    item.BodyText,
                    item.MessageAtUtc,
                    item.SourceReference,
                    item.OpportunityId,
                    item.ApplicationId,
                    item.ContactId,
                    item.OrganizationId),
                cancellationToken).ConfigureAwait(false);
            results.Add(result);
        }

        return new CommunicationBatchImportResult(
            Imported: results.Count(item => !item.WasDuplicate),
            Duplicates: results.Count(item => item.WasDuplicate),
            ActivitiesCreated: results.Count(item => item.ActivityCreatedAutomatically),
            Results: results);
    }

    /// <summary>
    /// Replaces the automatically inferred relations with explicit user-confirmed context. If the message
    /// represents direct professional communication and has no activity yet, a timeline entry is created.
    /// </summary>
    public async Task LinkAsync(
        Guid communicationId,
        CommunicationLinkInput input,
        CancellationToken cancellationToken = default)
    {
        var message = await store.GetCommunicationMessageAsync(communicationId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Kommunikation wurde nicht gefunden.");

        var validated = await ValidateExplicitContextAsync(input, cancellationToken).ConfigureAwait(false);
        message.LinkContext(
            validated.OpportunityId,
            validated.ApplicationId,
            validated.ContactId,
            validated.OrganizationId,
            clock.UtcNow);
        await store.UpdateCommunicationMessageAsync(message, cancellationToken).ConfigureAwait(false);
        await EnsureTimelineActivityAsync(message, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Marks one communication as intentionally irrelevant to the application tracker.</summary>
    public async Task IgnoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await store.GetCommunicationMessageAsync(id, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Kommunikation wurde nicht gefunden.");
        message.Ignore(clock.UtcNow);
        await store.UpdateCommunicationMessageAsync(message, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Creates an ACTION linked to the same context as the selected communication message.</summary>
    public async Task<TrackerTask> CreateActionAsync(
        Guid communicationId,
        string title,
        DateTimeOffset? dueAtUtc,
        CancellationToken cancellationToken = default)
    {
        var message = await store.GetCommunicationMessageAsync(communicationId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Kommunikation wurde nicht gefunden.");

        return await workItemService.CreateAsync(
            new WorkItemInput(
                message.OpportunityId,
                message.ApplicationId,
                message.ContactId,
                message.OrganizationId,
                WorkItemKind.Action,
                title,
                $"Aus Kommunikation: {message.Subject}",
                dueAtUtc),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new opportunity from the normalized communication body. This is primarily intended for
    /// job-alert messages and clipboard imports and keeps the captured body as the durable role snapshot.
    /// </summary>
    public async Task<Opportunity> CreateOpportunityFromMessageAsync(
        Guid communicationId,
        string title,
        string? sourceUrl,
        CancellationToken cancellationToken = default)
    {
        var message = await store.GetCommunicationMessageAsync(communicationId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Kommunikation wurde nicht gefunden.");

        var validatedSourceUrl = string.IsNullOrWhiteSpace(sourceUrl)
            ? null
            : Validation.Url(sourceUrl, "Quell-URL");
        var description = string.IsNullOrWhiteSpace(message.BodyText) ? message.Subject : message.BodyText;
        var opportunity = await opportunityService.CreateAsync(
            new OpportunityInput(
                EmployerOrganizationId: null,
                IntermediaryOrganizationId: null,
                Title: title,
                DescriptionSnapshot: description,
                Location: null,
                RemoteText: null,
                SalaryText: null,
                Status: OpportunityStatus.Identified,
                FoundAtUtc: clock.UtcNow,
                PublishedAtUtc: null,
                DeadlineAtUtc: null),
            cancellationToken).ConfigureAwait(false);

        if (validatedSourceUrl is not null)
        {
            // SourceLink supports shorter portal ids than CommunicationMessage. Do not mutate an
            // oversized mail id into a different id; omit it and retain the exact id on the message.
            var sourceExternalId = message.ExternalMessageId is { Length: <= 250 }
                ? message.ExternalMessageId
                : null;
            await opportunityService.AddSourceLinkAsync(
                opportunity.Id,
                new SourceLinkInput(message.SourceSystem, validatedSourceUrl, sourceExternalId),
                cancellationToken).ConfigureAwait(false);
        }

        message.LinkContext(opportunity.Id, null, message.ContactId, message.OrganizationId, clock.UtcNow);
        await store.UpdateCommunicationMessageAsync(message, cancellationToken).ConfigureAwait(false);
        return opportunity;
    }

    /// <summary>Runs deterministic, local text analysis without sending content to any external service.</summary>
    public CommunicationTextAnalysis Analyze(CommunicationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return AnalyzeText(message.Subject, message.BodyText, message.Kind);
    }

    /// <summary>Runs deterministic, local text analysis for clipboard content before it is imported.</summary>
    public CommunicationTextAnalysis AnalyzeClipboard(string subject, string bodyText)
        => AnalyzeText(subject, bodyText, CommunicationKind.Unclassified);

    private async Task<bool> EnsureTimelineActivityAsync(
        CommunicationMessage message,
        CancellationToken cancellationToken)
    {
        if (message.ActivityId is not null || message.Status == CommunicationStatus.Ignored)
        {
            return false;
        }

        var shouldCreate = message.Kind is CommunicationKind.Recruiter or CommunicationKind.ApplicationResponse ||
                           (message.Kind == CommunicationKind.General &&
                            (message.ContactId is not null || message.OpportunityId is not null || message.ApplicationId is not null));
        if (!shouldCreate)
        {
            return false;
        }

        var notes = BuildActivityNotes(message);
        var activitySubject = message.Subject.Length <= 250 ? message.Subject : message.Subject[..250];
        var activity = await activityService.CreateAsync(
            new ActivityInput(
                message.OpportunityId,
                message.ApplicationId,
                message.ContactId,
                message.OrganizationId,
                ActivityKind.Email,
                ActivityStatus.Recorded,
                activitySubject,
                notes,
                message.MessageAtUtc,
                ScheduledAtUtc: null),
            cancellationToken).ConfigureAwait(false);

        message.AttachActivity(activity.Id, clock.UtcNow);
        await store.UpdateCommunicationMessageAsync(message, cancellationToken).ConfigureAwait(false);
        return true;
    }

    private async Task<ContextMatch> MatchContextAsync(
        CommunicationImportInput input,
        CancellationToken cancellationToken)
    {
        if (input.ApplicationId is not null || input.OpportunityId is not null || input.ContactId is not null || input.OrganizationId is not null)
        {
            var explicitContext = await ValidateExplicitContextAsync(
                new CommunicationLinkInput(input.OpportunityId, input.ApplicationId, input.ContactId, input.OrganizationId),
                cancellationToken).ConfigureAwait(false);
            var explicitContact = explicitContext.ContactId is Guid contactId
                ? await store.GetContactAsync(contactId, cancellationToken).ConfigureAwait(false)
                : null;
            return new ContextMatch(
                explicitContext.OpportunityId,
                explicitContext.ApplicationId,
                explicitContact,
                explicitContext.OrganizationId,
                ContactMatchedAutomatically: false,
                ContextMatchedAutomatically: false);
        }

        Contact? contact = null;
        var counterpartyAddresses = GetCounterpartyAddresses(input);
        if (counterpartyAddresses.Count > 0)
        {
            var contacts = await store.ListContactsAsync(includeArchived: false, cancellationToken).ConfigureAwait(false);
            var candidates = contacts.Where(item =>
                    !string.IsNullOrWhiteSpace(item.Email) &&
                    counterpartyAddresses.Contains(item.Email.Trim(), StringComparer.OrdinalIgnoreCase))
                .ToList();
            contact = candidates.Count == 1 ? candidates[0] : null;
        }

        var organizationId = contact?.OrganizationId;
        Guid? applicationId = null;
        Guid? opportunityId = null;
        var contextMatched = false;

        if (contact is not null)
        {
            var activities = await store.ListActivitiesAsync(cancellationToken).ConfigureAwait(false);
            var threshold = input.MessageAtUtc.AddDays(-365);
            var related = activities
                .Where(item => item.ContactId == contact.Id)
                .Where(item => (item.OccurredAtUtc ?? item.ScheduledAtUtc ?? item.CreatedAtUtc) >= threshold)
                .ToList();

            var applicationIds = related.Where(item => item.ApplicationId is not null)
                .Select(item => item.ApplicationId!.Value)
                .Distinct()
                .ToList();
            if (applicationIds.Count == 1)
            {
                var application = await store.GetApplicationAsync(applicationIds[0], cancellationToken).ConfigureAwait(false);
                if (application is not null && !IsClosed(application))
                {
                    applicationId = application.Id;
                    opportunityId = application.OpportunityId;
                    contextMatched = true;
                }
            }

            if (opportunityId is null)
            {
                var opportunityIds = related.Where(item => item.OpportunityId is not null)
                    .Select(item => item.OpportunityId!.Value)
                    .Distinct()
                    .ToList();
                if (opportunityIds.Count == 1)
                {
                    var opportunity = await store.GetOpportunityAsync(opportunityIds[0], cancellationToken).ConfigureAwait(false);
                    if (opportunity is not null && opportunity.Status != OpportunityStatus.Closed)
                    {
                        opportunityId = opportunity.Id;
                        contextMatched = true;
                    }
                }
            }
        }

        if (opportunityId is null && organizationId is Guid knownOrganizationId)
        {
            var opportunities = await store.ListOpportunitiesAsync(cancellationToken).ConfigureAwait(false);
            var candidates = opportunities
                .Where(item => item.Status != OpportunityStatus.Closed)
                .Where(item => item.EmployerOrganizationId == knownOrganizationId || item.IntermediaryOrganizationId == knownOrganizationId)
                .ToList();
            if (candidates.Count == 1)
            {
                opportunityId = candidates[0].Id;
                contextMatched = true;
            }
        }

        if (applicationId is null && opportunityId is Guid knownOpportunityId)
        {
            var applications = await store.ListApplicationsAsync(cancellationToken).ConfigureAwait(false);
            var candidates = applications
                .Where(item => item.OpportunityId == knownOpportunityId)
                .Where(item => !IsClosed(item))
                .ToList();
            if (candidates.Count == 1)
            {
                applicationId = candidates[0].Id;
                contextMatched = true;
            }
        }

        return new ContextMatch(
            opportunityId,
            applicationId,
            contact,
            organizationId,
            ContactMatchedAutomatically: contact is not null,
            ContextMatchedAutomatically: contextMatched);
    }

    private async Task<CommunicationLinkInput> ValidateExplicitContextAsync(
        CommunicationLinkInput input,
        CancellationToken cancellationToken)
    {
        Opportunity? opportunity = null;
        JobApplication? application = null;
        Contact? contact = null;
        Organization? organization = null;

        if (input.OpportunityId is Guid opportunityId)
        {
            opportunity = await store.GetOpportunityAsync(opportunityId, cancellationToken).ConfigureAwait(false)
                ?? throw new KeyNotFoundException("Die zugeordnete Stelle wurde nicht gefunden.");
        }

        if (input.ApplicationId is Guid applicationId)
        {
            application = await store.GetApplicationAsync(applicationId, cancellationToken).ConfigureAwait(false)
                ?? throw new KeyNotFoundException("Die zugeordnete Bewerbung wurde nicht gefunden.");
            if (opportunity is not null && application.OpportunityId != opportunity.Id)
            {
                throw new ValidationException("Die ausgewählte Bewerbung gehört nicht zur ausgewählten Stelle.");
            }

            opportunity ??= await store.GetOpportunityAsync(application.OpportunityId, cancellationToken).ConfigureAwait(false);
        }

        if (input.ContactId is Guid contactId)
        {
            contact = await store.GetContactAsync(contactId, cancellationToken).ConfigureAwait(false)
                ?? throw new KeyNotFoundException("Der zugeordnete Kontakt wurde nicht gefunden.");
        }

        if (input.OrganizationId is Guid organizationId)
        {
            organization = await store.GetOrganizationAsync(organizationId, cancellationToken).ConfigureAwait(false)
                ?? throw new KeyNotFoundException("Die zugeordnete Organisation wurde nicht gefunden.");
        }

        organization ??= contact?.OrganizationId is Guid contactOrganizationId
            ? await store.GetOrganizationAsync(contactOrganizationId, cancellationToken).ConfigureAwait(false)
            : null;

        return new CommunicationLinkInput(opportunity?.Id, application?.Id, contact?.Id, organization?.Id);
    }

    private static CommunicationImportInput Normalize(CommunicationImportInput input)
    {
        var sourceSystem = Validation.Required(input.SourceSystem, "Quellsystem", 100);
        var externalId = Validation.Optional(input.ExternalMessageId, "Externe Nachrichten-ID", 512);
        var fromName = Validation.Optional(input.FromName, "Absendername", 250);
        var fromAddress = Validation.Email(input.FromAddress);
        var toAddresses = Validation.Optional(input.ToAddresses, "Empfänger", 2000);
        var subject = Validation.Required(input.Subject, "Betreff", 500);
        var body = Validation.Optional(input.BodyText, "Nachrichtentext", 100_000) ?? string.Empty;
        var sourceReference = Validation.Optional(input.SourceReference, "Quellreferenz", 2048);

        return input with
        {
            SourceSystem = sourceSystem,
            ExternalMessageId = externalId,
            FromName = fromName,
            FromAddress = fromAddress,
            ToAddresses = toAddresses,
            Subject = subject,
            BodyText = body,
            SourceReference = sourceReference,
        };
    }

    private static CommunicationKind Classify(CommunicationImportInput input, bool contactMatched)
    {
        if (input.Kind != CommunicationKind.Unclassified)
        {
            return input.Kind;
        }

        var searchable = $"{input.Subject}\n{input.BodyText}";
        if (ContainsAny(searchable, JobAlertTerms))
        {
            return CommunicationKind.JobAlert;
        }

        if (input.ApplicationId is not null || ContainsAny(searchable, ApplicationTerms))
        {
            return CommunicationKind.ApplicationResponse;
        }

        if (contactMatched)
        {
            return CommunicationKind.Recruiter;
        }

        return CommunicationKind.General;
    }

    private static CommunicationTextAnalysis AnalyzeText(string subject, string bodyText, CommunicationKind currentKind)
    {
        var suggestedKind = currentKind == CommunicationKind.Unclassified
            ? (ContainsAny($"{subject}\n{bodyText}", JobAlertTerms) ? CommunicationKind.JobAlert : CommunicationKind.General)
            : currentKind;
        return new CommunicationTextAnalysis(
            suggestedKind,
            GuessTitle(subject, bodyText),
            ExtractUrls(bodyText));
    }

    private static IReadOnlyList<string> ExtractUrls(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var urls = new List<string>();
        var separators = new[] { ' ', '\t', '\r', '\n', '<', '>', '"', '\'' };
        foreach (var token in text.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var candidate = token.TrimEnd('.', ',', ';', ':', ')', ']', '}', '!', '?');
            if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                var normalized = uri.AbsoluteUri;
                if (!urls.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                {
                    urls.Add(normalized);
                }
            }
        }

        return urls;
    }

    private static string GuessTitle(string subject, string bodyText)
    {
        var cleanedSubject = subject.Trim();
        foreach (var prefix in new[] { "Re:", "Fwd:", "Fw:", "AW:", "WG:" })
        {
            while (cleanedSubject.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                cleanedSubject = cleanedSubject[prefix.Length..].Trim();
            }
        }

        if (!string.IsNullOrWhiteSpace(cleanedSubject))
        {
            return cleanedSubject.Length <= 250 ? cleanedSubject : cleanedSubject[..250];
        }

        var firstLine = bodyText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? "Neue Stelle aus Kommunikation";
        return firstLine.Length <= 250 ? firstLine : firstLine[..250];
    }

    private static string BuildActivityNotes(CommunicationMessage message)
    {
        var header = $"Importiert aus: {message.SourceSystem}";
        if (!string.IsNullOrWhiteSpace(message.FromAddress))
        {
            header += $"\nVon: {message.FromName} <{message.FromAddress}>";
        }

        var body = message.BodyText;
        var available = Math.Max(0, 8000 - header.Length - 2);
        if (body.Length > available)
        {
            body = body[..available];
        }

        return string.IsNullOrWhiteSpace(body) ? header : $"{header}\n\n{body}";
    }

    private static IReadOnlyList<string> GetCounterpartyAddresses(CommunicationImportInput input)
    {
        if (input.Direction == CommunicationDirection.Incoming)
        {
            return string.IsNullOrWhiteSpace(input.FromAddress) ? [] : [input.FromAddress.Trim()];
        }

        if (string.IsNullOrWhiteSpace(input.ToAddresses))
        {
            return [];
        }

        var addresses = new List<string>();
        foreach (var part in input.ToAddresses.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var candidate = part;
            var open = candidate.LastIndexOf('<');
            var close = candidate.LastIndexOf('>');
            if (open >= 0 && close > open)
            {
                candidate = candidate[(open + 1)..close].Trim();
            }

            if (candidate.Contains('@') &&
                !addresses.Contains(candidate, StringComparer.OrdinalIgnoreCase))
            {
                addresses.Add(candidate);
            }
        }

        return addresses;
    }

    private static string ComputeFingerprint(CommunicationImportInput input)
    {
        var canonical = string.Join(
            "\n",
            input.SourceSystem.Trim().ToUpperInvariant(),
            input.Direction.ToString(),
            input.FromAddress?.Trim().ToUpperInvariant() ?? string.Empty,
            input.ToAddresses?.Trim().ToUpperInvariant() ?? string.Empty,
            input.Subject.Trim(),
            input.MessageAtUtc.ToUniversalTime().ToString("O"),
            input.BodyText.Trim());
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static bool ContainsAny(string text, IEnumerable<string> terms)
        => terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));

    private static bool IsClosed(JobApplication application)
        => application.Stage is ApplicationStage.Rejected or ApplicationStage.Withdrawn or ApplicationStage.Hired or ApplicationStage.Closed;

    private sealed record ContextMatch(
        Guid? OpportunityId,
        Guid? ApplicationId,
        Contact? Contact,
        Guid? OrganizationId,
        bool ContactMatchedAutomatically,
        bool ContextMatchedAutomatically)
    {
        public bool HasAnyContext =>
            OpportunityId is not null || ApplicationId is not null || Contact is not null || OrganizationId is not null;
    }
}
