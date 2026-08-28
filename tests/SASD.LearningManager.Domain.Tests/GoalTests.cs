using SASD.LearningManager.Domain.Common;
using SASD.LearningManager.Domain.Goals;

namespace SASD.LearningManager.Domain.Tests;

public sealed class GoalTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 27, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_AchievedGoal_SetsAchievedTimestamp()
    {
        var goal = Goal.Create("Cloud Engineer", null, GoalType.Career, "Profile", GoalPriority.High,
            GoalStatus.Achieved, new DateOnly(2027, 1, 1), null, null, Now);

        Assert.Equal(GoalStatus.Achieved, goal.Status);
        Assert.Equal(Now, goal.AchievedAtUtc);
    }

    [Fact]
    public void ChangeStatus_ReopensAchievedGoal_WithoutDeletingIdentity()
    {
        var goal = Goal.Create("EX442", null, GoalType.Certification, null, GoalPriority.Normal,
            GoalStatus.Achieved, null, null, null, Now);
        var id = goal.Id;

        goal.ChangeStatus(GoalStatus.Active, Now.AddHours(1));

        Assert.Equal(id, goal.Id);
        Assert.Equal(GoalStatus.Active, goal.Status);
        Assert.Null(goal.AchievedAtUtc);
    }

    [Fact]
    public void Archive_BlocksMetadataChangesUntilRestore()
    {
        var goal = Goal.Create("Linux", null, GoalType.Learning, null, GoalPriority.Normal,
            GoalStatus.Active, null, null, null, Now);
        goal.Archive(Now.AddMinutes(1));

        Assert.Throws<DomainValidationException>(() => goal.Update(
            "Linux Advanced", null, GoalType.Learning, null, GoalPriority.Normal,
            null, null, null, Now.AddMinutes(2)));
    }
}
