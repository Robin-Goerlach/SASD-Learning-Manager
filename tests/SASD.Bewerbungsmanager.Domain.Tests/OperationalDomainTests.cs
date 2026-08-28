using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using TrackerActivity = SASD.Bewerbungsmanager.Domain.Entities.Activity;

namespace SASD.Bewerbungsmanager.Domain.Tests;

public sealed class OperationalDomainTests
{
    [Fact]
    public void WorkItem_Complete_ChangesLifecycleAndTimestamp()
    {
        var completedAt = new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);
        var item = new TrackerTask
        {
            Id = Guid.NewGuid(),
            Kind = WorkItemKind.Action,
            Status = WorkItemStatus.Open,
            Title = "Synthetische Aufgabe",
        };

        item.Complete(completedAt);

        Assert.Equal(WorkItemStatus.Completed, item.Status);
        Assert.Equal(completedAt, item.CompletedAtUtc);
        Assert.Equal(completedAt, item.UpdatedAtUtc);
    }

    [Fact]
    public void SearchProfile_MarkChecked_SchedulesNextRegularCheck()
    {
        var checkedAt = new DateTimeOffset(2026, 8, 26, 8, 0, 0, TimeSpan.Zero);
        var profile = new SearchProfile
        {
            CheckIntervalDays = 3,
            NextCheckAtUtc = checkedAt,
        };

        profile.MarkChecked(checkedAt);

        Assert.Equal(checkedAt, profile.LastCheckedAtUtc);
        Assert.Equal(checkedAt.AddDays(3), profile.NextCheckAtUtc);
    }

    [Fact]
    public void PlannedActivity_Complete_PreservesScheduledTimeAsOccurredTime()
    {
        var scheduled = new DateTimeOffset(2026, 8, 27, 10, 0, 0, TimeSpan.Zero);
        var completed = scheduled.AddHours(1);
        var activity = new TrackerActivity
        {
            Status = ActivityStatus.Planned,
            Kind = ActivityKind.Interview,
            Subject = "Synthetisches Interview",
            ScheduledAtUtc = scheduled,
        };

        activity.Complete(completed);

        Assert.Equal(ActivityStatus.Completed, activity.Status);
        Assert.Equal(scheduled, activity.OccurredAtUtc);
        Assert.Equal(completed, activity.CompletedAtUtc);
    }
    [Fact]
    public void JobLead_LinkOpportunity_ChangesStatusAndStoresRelation()
    {
        var now = new DateTimeOffset(2026, 8, 27, 11, 0, 0, TimeSpan.Zero);
        var opportunityId = Guid.NewGuid();
        var lead = new JobLead
        {
            Id = Guid.NewGuid(),
            SourceSystem = "Example Portal",
            FingerprintSha256 = new string('a', 64),
            Title = "Synthetic Engineer",
            Status = JobLeadStatus.New,
        };

        lead.LinkOpportunity(opportunityId, now);

        Assert.Equal(JobLeadStatus.Imported, lead.Status);
        Assert.Equal(opportunityId, lead.OpportunityId);
        Assert.Equal(now, lead.UpdatedAtUtc);
    }

    [Fact]
    public void JobLead_Imported_CannotBeIgnored()
    {
        var lead = new JobLead { Status = JobLeadStatus.Imported, OpportunityId = Guid.NewGuid() };

        Assert.Throws<InvalidOperationException>(() => lead.Ignore(DateTimeOffset.UtcNow));
    }

}
