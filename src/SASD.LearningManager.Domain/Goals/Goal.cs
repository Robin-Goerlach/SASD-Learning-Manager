using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.Goals;

/// <summary>
/// Represents a desired learning, career, certification or project outcome. A goal may be linked
/// to multiple skills, but it never owns their mastery state. Skill assessment remains a separate
/// concern so achieving a goal cannot silently rewrite competence history.
/// </summary>
public sealed class Goal
{
    private Goal(
        Guid id,
        string title,
        string? description,
        GoalType type,
        string? motivation,
        GoalPriority priority,
        GoalStatus status,
        DateOnly? targetDate,
        string? nextActionText,
        DateOnly? nextActionDueDate,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        DateTimeOffset? achievedAtUtc,
        DateTimeOffset? archivedAtUtc)
    {
        Id = id;
        Title = title;
        Description = description;
        Type = type;
        Motivation = motivation;
        Priority = priority;
        Status = status;
        TargetDate = targetDate;
        NextActionText = nextActionText;
        NextActionDueDate = nextActionDueDate;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        AchievedAtUtc = achievedAtUtc;
        ArchivedAtUtc = archivedAtUtc;
    }

    public Guid Id { get; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public GoalType Type { get; private set; }
    public string? Motivation { get; private set; }
    public GoalPriority Priority { get; private set; }
    public GoalStatus Status { get; private set; }
    public DateOnly? TargetDate { get; private set; }
    public string? NextActionText { get; private set; }
    public DateOnly? NextActionDueDate { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? AchievedAtUtc { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    /// <summary>Creates a new goal using validated, normalized text values.</summary>
    public static Goal Create(
        string title,
        string? description,
        GoalType type,
        string? motivation,
        GoalPriority priority,
        GoalStatus status,
        DateOnly? targetDate,
        string? nextActionText,
        DateOnly? nextActionDueDate,
        DateTimeOffset nowUtc)
    {
        if (status == GoalStatus.Archived)
        {
            throw new DomainValidationException("A new goal cannot start in the archived state.");
        }

        return new Goal(
            Guid.NewGuid(),
            Guard.RequiredText(title, "Goal title", 500),
            Guard.OptionalText(description, "Goal description", 20_000),
            type,
            Guard.OptionalText(motivation, "Goal motivation", 10_000),
            priority,
            status,
            targetDate,
            Guard.OptionalText(nextActionText, "Next action", 1000),
            nextActionDueDate,
            nowUtc,
            nowUtc,
            status == GoalStatus.Achieved ? nowUtc : null,
            null);
    }

    /// <summary>Rehydrates a goal from trusted persistence data.</summary>
    public static Goal Rehydrate(
        Guid id,
        string title,
        string? description,
        GoalType type,
        string? motivation,
        GoalPriority priority,
        GoalStatus status,
        DateOnly? targetDate,
        string? nextActionText,
        DateOnly? nextActionDueDate,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        DateTimeOffset? achievedAtUtc,
        DateTimeOffset? archivedAtUtc)
        => new(id, title, description, type, motivation, priority, status, targetDate, nextActionText,
            nextActionDueDate, createdAtUtc, updatedAtUtc, achievedAtUtc, archivedAtUtc);

    /// <summary>Updates editable goal metadata while preserving lifecycle history.</summary>
    public void Update(
        string title,
        string? description,
        GoalType type,
        string? motivation,
        GoalPriority priority,
        DateOnly? targetDate,
        string? nextActionText,
        DateOnly? nextActionDueDate,
        DateTimeOffset nowUtc)
    {
        EnsureNotArchived();
        Title = Guard.RequiredText(title, "Goal title", 500);
        Description = Guard.OptionalText(description, "Goal description", 20_000);
        Type = type;
        Motivation = Guard.OptionalText(motivation, "Goal motivation", 10_000);
        Priority = priority;
        TargetDate = targetDate;
        NextActionText = Guard.OptionalText(nextActionText, "Next action", 1000);
        NextActionDueDate = nextActionDueDate;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Changes the lifecycle state and keeps achieved timestamps consistent.</summary>
    public void ChangeStatus(GoalStatus newStatus, DateTimeOffset nowUtc)
    {
        if (newStatus == GoalStatus.Archived)
        {
            Archive(nowUtc);
            return;
        }

        EnsureNotArchived();
        Status = newStatus;
        AchievedAtUtc = newStatus == GoalStatus.Achieved ? AchievedAtUtc ?? nowUtc : null;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Archives the goal without deleting its linked skills or history.</summary>
    public void Archive(DateTimeOffset nowUtc)
    {
        Status = GoalStatus.Archived;
        ArchivedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Restores an archived goal as planned, leaving historical links intact.</summary>
    public void Restore(DateTimeOffset nowUtc)
    {
        if (Status != GoalStatus.Archived)
        {
            return;
        }

        Status = GoalStatus.Planned;
        ArchivedAtUtc = null;
        UpdatedAtUtc = nowUtc;
    }

    private void EnsureNotArchived()
    {
        if (Status == GoalStatus.Archived)
        {
            throw new DomainValidationException("An archived goal must be restored before it can be edited.");
        }
    }
}
