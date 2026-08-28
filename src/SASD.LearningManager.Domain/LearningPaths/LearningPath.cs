using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.LearningPaths;

/// <summary>
/// Represents a structured personal learning roadmap. A path owns its lifecycle metadata while
/// its hierarchical nodes are persisted as separate entities so tree operations stay manageable.
/// </summary>
public sealed class LearningPath
{
    private LearningPath(
        Guid id,
        string title,
        string? description,
        LearningPathStatus status,
        LearningPathPriority priority,
        DateOnly? plannedStartDate,
        DateOnly? targetDate,
        string? nextActionText,
        DateOnly? nextActionDueDate,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        DateTimeOffset? startedAtUtc,
        DateTimeOffset? completedAtUtc,
        DateTimeOffset? archivedAtUtc)
    {
        Id = id;
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        PlannedStartDate = plannedStartDate;
        TargetDate = targetDate;
        NextActionText = nextActionText;
        NextActionDueDate = nextActionDueDate;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        ArchivedAtUtc = archivedAtUtc;
    }

    public Guid Id { get; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public LearningPathStatus Status { get; private set; }
    public LearningPathPriority Priority { get; private set; }
    public DateOnly? PlannedStartDate { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public string? NextActionText { get; private set; }
    public DateOnly? NextActionDueDate { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    /// <summary>Creates a new path after validating its user-editable metadata.</summary>
    public static LearningPath Create(
        string title,
        string? description,
        LearningPathStatus status,
        LearningPathPriority priority,
        DateOnly? plannedStartDate,
        DateOnly? targetDate,
        string? nextActionText,
        DateOnly? nextActionDueDate,
        DateTimeOffset nowUtc)
    {
        if (status == LearningPathStatus.Archived)
        {
            throw new DomainValidationException("A new learning path cannot start archived.");
        }

        ValidateDates(plannedStartDate, targetDate);
        var path = new LearningPath(
            Guid.NewGuid(),
            Guard.RequiredText(title, "Learning path title", 500),
            Guard.OptionalText(description, "Learning path description", 20_000),
            LearningPathStatus.Planned,
            priority,
            plannedStartDate,
            targetDate,
            Guard.OptionalText(nextActionText, "Next action", 1_000),
            nextActionDueDate,
            nowUtc,
            nowUtc,
            null,
            null,
            null);

        if (status != LearningPathStatus.Planned)
        {
            path.ChangeStatus(status, nowUtc);
        }

        return path;
    }

    /// <summary>Rehydrates a path from persistence without re-running lifecycle transitions.</summary>
    public static LearningPath Rehydrate(
        Guid id,
        string title,
        string? description,
        LearningPathStatus status,
        LearningPathPriority priority,
        DateOnly? plannedStartDate,
        DateOnly? targetDate,
        string? nextActionText,
        DateOnly? nextActionDueDate,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        DateTimeOffset? startedAtUtc,
        DateTimeOffset? completedAtUtc,
        DateTimeOffset? archivedAtUtc)
        => new(id, title, description, status, priority, plannedStartDate, targetDate, nextActionText,
            nextActionDueDate, createdAtUtc, updatedAtUtc, startedAtUtc, completedAtUtc, archivedAtUtc);

    /// <summary>Updates path metadata without changing lifecycle state.</summary>
    public void Update(
        string title,
        string? description,
        LearningPathPriority priority,
        DateOnly? plannedStartDate,
        DateOnly? targetDate,
        string? nextActionText,
        DateOnly? nextActionDueDate,
        DateTimeOffset nowUtc)
    {
        EnsureNotArchived();
        ValidateDates(plannedStartDate, targetDate);
        Title = Guard.RequiredText(title, "Learning path title", 500);
        Description = Guard.OptionalText(description, "Learning path description", 20_000);
        Priority = priority;
        PlannedStartDate = plannedStartDate;
        TargetDate = targetDate;
        NextActionText = Guard.OptionalText(nextActionText, "Next action", 1_000);
        NextActionDueDate = nextActionDueDate;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Changes lifecycle state and maintains start/completion timestamps.</summary>
    public void ChangeStatus(LearningPathStatus status, DateTimeOffset nowUtc)
    {
        EnsureNotArchived();
        if (status == LearningPathStatus.Archived)
        {
            throw new DomainValidationException("Use the archive operation to archive a learning path.");
        }

        if (status == Status)
        {
            return;
        }

        Status = status;
        if (status == LearningPathStatus.Active)
        {
            StartedAtUtc ??= nowUtc;
            CompletedAtUtc = null;
        }
        else if (status == LearningPathStatus.Completed)
        {
            StartedAtUtc ??= nowUtc;
            CompletedAtUtc = nowUtc;
        }
        else if (status is LearningPathStatus.Planned or LearningPathStatus.Paused)
        {
            CompletedAtUtc = null;
        }

        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Archives the path while preserving its nodes and relationships.</summary>
    public void Archive(DateTimeOffset nowUtc)
    {
        Status = LearningPathStatus.Archived;
        ArchivedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Restores an archived path to a conservative paused state.</summary>
    public void Restore(DateTimeOffset nowUtc)
    {
        if (Status != LearningPathStatus.Archived)
        {
            return;
        }

        Status = StartedAtUtc is null ? LearningPathStatus.Planned : LearningPathStatus.Paused;
        ArchivedAtUtc = null;
        UpdatedAtUtc = nowUtc;
    }

    private static void ValidateDates(DateOnly? plannedStartDate, DateOnly? targetDate)
    {
        if (plannedStartDate is not null && targetDate is not null && targetDate < plannedStartDate)
        {
            throw new DomainValidationException("Target date must not be before planned start date.");
        }
    }

    private void EnsureNotArchived()
    {
        if (Status == LearningPathStatus.Archived)
        {
            throw new DomainValidationException("An archived learning path must be restored before it can be changed.");
        }
    }
}
