using SASD.LearningManager.Domain.Common;
using SASD.LearningManager.Domain.Competencies;

namespace SASD.LearningManager.Domain.Tests;

public sealed class CompetencyTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 27, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CompetencyArea_Create_TrimsName()
    {
        var area = CompetencyArea.Create("  Linux  ", null, Now);
        Assert.Equal("Linux", area.Name);
    }

    [Fact]
    public void Topic_ArchiveRequiresRestoreBeforeUpdate()
    {
        var topic = Topic.Create("systemd", null, Now);
        topic.Archive(Now.AddMinutes(1));

        Assert.Throws<DomainValidationException>(() => topic.Update("systemd services", null, Now.AddMinutes(2)));
    }
}
