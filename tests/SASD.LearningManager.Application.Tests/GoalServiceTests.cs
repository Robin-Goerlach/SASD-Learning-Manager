using SASD.LearningManager.Application.Goals;
using SASD.LearningManager.Domain.Goals;
using SASD.LearningManager.Domain.Skills;

namespace SASD.LearningManager.Application.Tests;

public sealed class GoalServiceTests
{
    [Fact]
    public async Task CreateAsync_LinksSkillWithoutChangingMastery()
    {
        var clock = new FakeClock();
        var skills = new FakeSkillRepository();
        var skill = Skill.Create("Docker Networking", null, 4, clock.UtcNow);
        skill.ApplyAssessment(2, clock.UtcNow);
        skills.Items.Add(skill.Id, skill);
        skills.Areas[skill.Id] = [];
        skills.Topics[skill.Id] = [];
        skills.Assessments[skill.Id] = [];
        var goals = new FakeGoalRepository();
        var service = new GoalService(goals, skills, clock);

        var id = await service.CreateAsync(new GoalEditModel(
            "Cloud Engineer", null, GoalType.Career, "Profile", GoalPriority.High, GoalStatus.Active,
            new DateOnly(2027, 1, 1), "Docker Lab", null, [skill.Id]),
            TestContext.Current.CancellationToken);

        Assert.Contains(skill.Id, goals.SkillLinks[id]);
        Assert.Equal(2, skills.Items[skill.Id].CurrentLevel);
    }

    [Fact]
    public async Task CreateAsync_RejectsUnknownSkillRelationship()
    {
        var service = new GoalService(new FakeGoalRepository(), new FakeSkillRepository(), new FakeClock());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new GoalEditModel(
            "Goal", null, GoalType.Learning, null, GoalPriority.Normal, GoalStatus.Planned,
            null, null, null, [Guid.NewGuid()]), TestContext.Current.CancellationToken));
    }
}
