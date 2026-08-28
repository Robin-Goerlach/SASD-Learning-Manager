using SASD.LearningManager.Domain.Common;
using SASD.LearningManager.Domain.LearningPaths;

namespace SASD.LearningManager.Domain.Tests;

public sealed class LearningPathTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 28, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_RejectsTargetDateBeforePlannedStart()
    {
        Assert.Throws<DomainValidationException>(() => LearningPath.Create(
            "Docker Path", null, LearningPathStatus.Planned, LearningPathPriority.Normal,
            new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 1), null, null, Now));
    }

    [Fact]
    public void ChangeStatus_TracksStartAndCompletionWithoutChangingIdentity()
    {
        var path = LearningPath.Create("Docker Path", null, LearningPathStatus.Planned, LearningPathPriority.High,
            null, null, null, null, Now);

        path.ChangeStatus(LearningPathStatus.Active, Now.AddHours(1));
        path.ChangeStatus(LearningPathStatus.Completed, Now.AddHours(2));

        Assert.Equal(Now.AddHours(1), path.StartedAtUtc);
        Assert.Equal(Now.AddHours(2), path.CompletedAtUtc);
        Assert.Equal(LearningPathStatus.Completed, path.Status);
    }

    [Fact]
    public void Node_MoveToRejectsSelfParent()
    {
        var node = LearningPathNode.Create(Guid.NewGuid(), null, "CPU", null, LearningPathNodeType.Module, 0, true, Now);

        Assert.Throws<DomainValidationException>(() => node.MoveTo(node.Id, 0, Now));
    }

    [Fact]
    public void Progress_UsesRequiredNodesAsCoreDenominator()
    {
        var pathId = Guid.NewGuid();
        var requiredDone = LearningPathNode.Create(pathId, null, "Done", null, LearningPathNodeType.Topic, 0, true, Now);
        requiredDone.Update("Done", null, LearningPathNodeType.Topic, true, LearningPathNodeStatus.Completed, Now);
        var requiredOpen = LearningPathNode.Create(pathId, null, "Open", null, LearningPathNodeType.Topic, 1, true, Now);
        var optionalDone = LearningPathNode.Create(pathId, null, "Optional", null, LearningPathNodeType.Topic, 2, false, Now);
        optionalDone.Update("Optional", null, LearningPathNodeType.Topic, false, LearningPathNodeStatus.Completed, Now);

        var progress = LearningPathProgress.Calculate([requiredDone, requiredOpen, optionalDone]);

        Assert.Equal(1, progress.RequiredCompleted);
        Assert.Equal(2, progress.RequiredTotal);
        Assert.Equal(1, progress.OptionalCompleted);
        Assert.Equal(1, progress.OptionalTotal);
        Assert.Equal(50m, progress.CoreCompletionPercent);
    }

    [Fact]
    public void Progress_FallsBackToAllNodesWhenNoRequiredNodesExist()
    {
        var pathId = Guid.NewGuid();
        var done = LearningPathNode.Create(pathId, null, "Done", null, LearningPathNodeType.Activity, 0, false, Now);
        done.Update("Done", null, LearningPathNodeType.Activity, false, LearningPathNodeStatus.Completed, Now);
        var open = LearningPathNode.Create(pathId, null, "Open", null, LearningPathNodeType.Activity, 1, false, Now);

        var progress = LearningPathProgress.Calculate([done, open]);

        Assert.Equal(50m, progress.CoreCompletionPercent);
    }

    [Fact]
    public void Relation_RejectsSelfReference()
    {
        var nodeId = Guid.NewGuid();

        Assert.Throws<DomainValidationException>(() => LearningPathNodeRelation.Create(
            nodeId, nodeId, LearningPathNodeRelationType.Requires, null, Now));
    }
}
