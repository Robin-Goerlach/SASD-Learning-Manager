using SASD.LearningManager.Domain.Common;
using SASD.LearningManager.Domain.Skills;

namespace SASD.LearningManager.Domain.Tests;

public sealed class SkillTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 27, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_SkillStartsWithoutCurrentLevel()
    {
        var skill = Skill.Create("Docker Networking", "Bridge diagnostics", 4, Now);

        Assert.Null(skill.CurrentLevel);
        Assert.Equal(4, skill.TargetLevel);
        Assert.Null(skill.Gap);
    }

    [Fact]
    public void ApplyAssessment_UpdatesCurrentLevelAndGap()
    {
        var skill = Skill.Create("Docker Networking", null, 4, Now);

        skill.ApplyAssessment(2, Now.AddMinutes(1));

        Assert.Equal(2, skill.CurrentLevel);
        Assert.Equal(2, skill.Gap);
    }

    [Fact]
    public void Create_RejectsTargetLevelOutsideScale()
    {
        Assert.Throws<DomainValidationException>(() => Skill.Create("Invalid", null, 6, Now));
    }

    [Fact]
    public void Assessment_RejectsFutureTimestamp()
    {
        var skill = Skill.Create("Ceph", null, 3, Now);

        Assert.Throws<DomainValidationException>(() => SkillAssessment.Create(
            skill.Id, 2, SkillAssessmentType.SelfAssessment, null, Now.AddHours(1), Now));
    }
}
