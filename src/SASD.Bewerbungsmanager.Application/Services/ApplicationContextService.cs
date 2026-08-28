using System.Globalization;
using System.Text;
using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using TrackerActivity = SASD.Bewerbungsmanager.Domain.Entities.Activity;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>
/// Creates a compact, deterministic text context for manually transferring one application into a
/// separate ChatGPT or advisory conversation. No generative AI is called from the application.
/// </summary>
public sealed class ApplicationContextService(ITrackerDataStore store, IClock clock)
{
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>Builds the context text for one concrete application.</summary>
    public async Task<string> BuildAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var application = await store.GetApplicationAsync(applicationId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die Bewerbung wurde nicht gefunden.");
        var opportunity = await store.GetOpportunityAsync(application.OpportunityId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Die zugehörige Stelle wurde nicht gefunden.");

        var organizations = await store.ListOrganizationsAsync(includeArchived: true, cancellationToken).ConfigureAwait(false);
        var contacts = await store.ListContactsAsync(includeArchived: true, cancellationToken).ConfigureAwait(false);
        var activities = await store.ListActivitiesAsync(cancellationToken).ConfigureAwait(false);
        var tasks = await store.ListTasksAsync(cancellationToken).ConfigureAwait(false);
        var documents = await store.ListApplicationDocumentSnapshotsAsync(applicationId, cancellationToken).ConfigureAwait(false);

        var relevantActivities = activities
            .Where(item => item.ApplicationId == applicationId || item.OpportunityId == opportunity.Id)
            .OrderBy(item => item.OccurredAtUtc ?? item.ScheduledAtUtc ?? item.CreatedAtUtc)
            .ToList();
        var relevantTasks = tasks
            .Where(item => item.ApplicationId == applicationId || item.OpportunityId == opportunity.Id)
            .ToList();
        var contactIds = relevantActivities.Select(item => item.ContactId)
            .Concat(relevantTasks.Select(item => item.ContactId))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToHashSet();

        var builder = new StringBuilder(4096);
        AddSection(builder, "Position", opportunity.Title);
        AddSection(builder, "Unternehmen", ResolveOrganization(opportunity.EmployerOrganizationId, organizations));
        AddSection(builder, "Vermittler", ResolveOrganization(opportunity.IntermediaryOrganizationId, organizations));
        AddSection(
            builder,
            "Kontakte",
            JoinLines(contacts.Where(item => contactIds.Contains(item.Id)).Select(FormatContact)));
        AddSection(builder, "Status", application.Stage.ToString());
        AddSection(builder, "Rollenbeschreibung", opportunity.DescriptionSnapshot);
        AddSection(builder, "Bisheriger Verlauf", JoinLines(relevantActivities.Select(FormatActivity)));
        AddSection(
            builder,
            "Offene Aufgaben",
            JoinLines(relevantTasks
                .Where(item => item.Status == WorkItemStatus.Open && item.Kind == WorkItemKind.Action)
                .Select(FormatTask)));
        AddSection(
            builder,
            "Warten auf",
            JoinLines(relevantTasks
                .Where(item => item.Status == WorkItemStatus.Open && item.Kind == WorkItemKind.WaitingFor)
                .Select(FormatTask)));
        AddSection(builder, "Verwendete Dokumente", JoinLines(documents.Select(FormatDocument)));

        var nextAppointment = relevantActivities
            .Where(item => item.Status == ActivityStatus.Planned && item.ScheduledAtUtc is not null && item.ScheduledAtUtc >= clock.UtcNow)
            .OrderBy(item => item.ScheduledAtUtc)
            .FirstOrDefault();
        AddSection(builder, "Nächster Termin", nextAppointment is null ? string.Empty : FormatActivity(nextAppointment));

        return builder.ToString().TrimEnd();
    }

    private static string ResolveOrganization(Guid? id, IReadOnlyList<Organization> organizations)
        => id is Guid organizationId
            ? organizations.SingleOrDefault(item => item.Id == organizationId)?.Name ?? string.Empty
            : string.Empty;

    private static string FormatContact(Contact contact)
        => string.IsNullOrWhiteSpace(contact.Role)
            ? contact.FullName
            : $"{contact.FullName} — {contact.Role}";

    private static string FormatActivity(TrackerActivity activity)
    {
        var timestamp = activity.OccurredAtUtc ?? activity.ScheduledAtUtc ?? activity.CreatedAtUtc;
        return $"{FormatDate(timestamp)} — {activity.Kind}: {activity.Subject}";
    }

    private static string FormatTask(TrackerTask item)
    {
        var due = item.DueAtUtc is null ? string.Empty : $" (fällig {FormatDate(item.DueAtUtc.Value)})";
        return $"{item.Title}{due}";
    }

    private static string FormatDocument(ApplicationDocumentSnapshot snapshot)
        => $"{snapshot.Type}: {snapshot.Label} / {snapshot.Version} / {snapshot.Language} / SHA-256 {snapshot.Sha256}";

    private static string FormatDate(DateTimeOffset value)
        => value.ToLocalTime().ToString("dd.MM.yyyy HH:mm", GermanCulture);

    private static string JoinLines(IEnumerable<string> values)
    {
        var materialized = values.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        return materialized.Count == 0 ? string.Empty : string.Join(Environment.NewLine, materialized.Select(value => $"- {value}"));
    }

    private static void AddSection(StringBuilder builder, string title, string? content)
    {
        builder.AppendLine($"{title}:");
        builder.AppendLine(string.IsNullOrWhiteSpace(content) ? "-" : content.Trim());
        builder.AppendLine();
    }
}
