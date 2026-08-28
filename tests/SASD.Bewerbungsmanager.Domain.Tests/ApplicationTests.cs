using Xunit;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Tests;

public sealed class ApplicationTests
{
    [Fact]
    public void ChangeStage_AppendsHistoryAndUpdatesCurrentStage()
    {
        var application = new Application
        {
            Id = Guid.NewGuid(),
            StartedAtUtc = new DateTimeOffset(2026, 8, 20, 8, 0, 0, TimeSpan.Zero),
            UpdatedAtUtc = new DateTimeOffset(2026, 8, 20, 8, 0, 0, TimeSpan.Zero),
        };
        application.InitializeStage(ApplicationStage.Draft, application.StartedAtUtc, "Angelegt");

        var changedAt = new DateTimeOffset(2026, 8, 21, 9, 30, 0, TimeSpan.Zero);
        application.ChangeStage(ApplicationStage.Submitted, changedAt, "Per Portal versendet");

        Assert.Equal(ApplicationStage.Submitted, application.Stage);
        Assert.Equal(2, application.StatusHistory.Count);
        Assert.Equal(ApplicationStage.Submitted, application.StatusHistory.Last().Stage);
        Assert.Equal(changedAt, application.UpdatedAtUtc);
    }

    [Fact]
    public void ChangeStage_WhenStageIsUnchanged_DoesNotCreateDuplicateHistory()
    {
        var application = new Application { Id = Guid.NewGuid() };
        var time = new DateTimeOffset(2026, 8, 20, 8, 0, 0, TimeSpan.Zero);
        application.InitializeStage(ApplicationStage.Draft, time);

        application.ChangeStage(ApplicationStage.Draft, time.AddHours(1), "Keine echte Änderung");

        Assert.Single(application.StatusHistory);
    }
}
