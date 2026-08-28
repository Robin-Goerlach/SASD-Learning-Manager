using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Application.Services;

/// <summary>
/// Builds the intentionally small Milestone-1 dashboard. Operational due items are added in the
/// next MVP milestone together with Activity and Task/WAITING_FOR.
/// </summary>
public sealed class DashboardService(ITrackerDataStore store)
{
    /// <summary>Builds current high-level counts from persisted opportunities and applications.</summary>
    public async Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var opportunities = await store.ListOpportunitiesAsync(cancellationToken).ConfigureAwait(false);
        var applications = await store.ListApplicationsAsync(cancellationToken).ConfigureAwait(false);

        return new DashboardSummary(
            opportunities.Count(item => item.Status != OpportunityStatus.Closed),
            applications.Count,
            applications.Count(item => item.Stage == ApplicationStage.Interview),
            applications.Count(item => item.Stage == ApplicationStage.Offer));
    }
}
