using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>
/// Builds the operational Today view. Date filtering intentionally happens after materialization
/// because SQLite cannot reliably compare the domain's <see cref="DateTimeOffset"/> values in SQL.
/// </summary>
public sealed class TodayService(ITrackerDataStore store, IClock clock)
{
    /// <summary>Returns the due and upcoming work that should drive the current day.</summary>
    public async Task<TodayOverview> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var localNow = now.ToLocalTime();
        var localToday = localNow.Date;
        var localTomorrow = localToday.AddDays(1);
        var startUtc = ToUtc(localToday);
        var endUtc = ToUtc(localTomorrow);
        var appointmentHorizonUtc = ToUtc(localToday.AddDays(14));

        var tasks = await store.ListTasksAsync(cancellationToken).ConfigureAwait(false);
        var activities = await store.ListActivitiesAsync(cancellationToken).ConfigureAwait(false);
        var searches = await store.ListSearchProfilesAsync(includeInactive: false, cancellationToken).ConfigureAwait(false);

        var openActions = tasks
            .Where(item => item.Status == WorkItemStatus.Open && item.Kind == WorkItemKind.Action)
            .ToList();

        var overdue = openActions
            .Where(item => item.DueAtUtc is not null && item.DueAtUtc < startUtc)
            .OrderBy(item => item.DueAtUtc)
            .ThenBy(item => item.Title)
            .ToList();

        var overdueIds = overdue.Select(item => item.Id).ToHashSet();
        var due = openActions
            .Where(item => !overdueIds.Contains(item.Id))
            .Where(item => item.DueAtUtc is null || item.DueAtUtc < endUtc)
            .OrderBy(item => item.DueAtUtc ?? DateTimeOffset.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();

        var waitingFor = tasks
            .Where(item => item.Status == WorkItemStatus.Open && item.Kind == WorkItemKind.WaitingFor)
            .OrderBy(item => item.DueAtUtc ?? DateTimeOffset.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();

        var appointments = activities
            .Where(item =>
                item.Status == ActivityStatus.Planned &&
                item.ScheduledAtUtc is not null &&
                item.ScheduledAtUtc >= now &&
                item.ScheduledAtUtc < appointmentHorizonUtc)
            .OrderBy(item => item.ScheduledAtUtc)
            .ThenBy(item => item.Subject)
            .ToList();

        var dueSearches = searches
            .Where(item => item.IsActive && item.NextCheckAtUtc < endUtc)
            .OrderBy(item => item.NextCheckAtUtc)
            .ThenBy(item => item.Name)
            .ToList();

        return new TodayOverview(overdue, due, waitingFor, appointments, dueSearches);
    }

    private static DateTimeOffset ToUtc(DateTime localDateTime)
        => new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime)).ToUniversalTime();
}
