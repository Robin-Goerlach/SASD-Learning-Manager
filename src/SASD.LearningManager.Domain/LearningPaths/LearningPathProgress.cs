namespace SASD.LearningManager.Domain.LearningPaths;

/// <summary>Immutable progress result for one learning path.</summary>
public sealed record LearningPathProgress(
    int RequiredCompleted,
    int RequiredTotal,
    int OptionalCompleted,
    int OptionalTotal,
    decimal? CoreCompletionPercent)
{
    /// <summary>
    /// Calculates path progress from active nodes. Optional nodes never reduce core completion;
    /// when a path contains no required nodes, all non-archived nodes become the denominator.
    /// </summary>
    public static LearningPathProgress Calculate(IEnumerable<LearningPathNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        var active = nodes.Where(static node => node.Status != LearningPathNodeStatus.Archived).ToArray();
        var required = active.Where(static node => node.IsRequired).ToArray();
        var optional = active.Where(static node => !node.IsRequired).ToArray();
        var requiredCompleted = required.Count(static node => node.Status == LearningPathNodeStatus.Completed);
        var optionalCompleted = optional.Count(static node => node.Status == LearningPathNodeStatus.Completed);

        decimal? percentage;
        if (required.Length > 0)
        {
            percentage = decimal.Round(requiredCompleted * 100m / required.Length, 1);
        }
        else if (active.Length > 0)
        {
            var completed = active.Count(static node => node.Status == LearningPathNodeStatus.Completed);
            percentage = decimal.Round(completed * 100m / active.Length, 1);
        }
        else
        {
            percentage = null;
        }

        return new LearningPathProgress(requiredCompleted, required.Length, optionalCompleted, optional.Length, percentage);
    }
}
