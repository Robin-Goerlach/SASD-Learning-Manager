using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.Skills;

/// <summary>
/// A demonstrable capability that can be assessed independently of learning-resource completion.
/// The current level is a snapshot of the newest accepted assessment; the assessment history is
/// preserved separately and therefore remains the authoritative record of development over time.
/// </summary>
public sealed class Skill
{
    private Skill(Guid id, string name, string? description, int? currentLevel, int? targetLevel,
        SkillStatus status, DateTimeOffset createdAtUtc, DateTimeOffset updatedAtUtc,
        DateTimeOffset? archivedAtUtc)
    {
        Id = id;
        Name = name;
        Description = description;
        CurrentLevel = currentLevel;
        TargetLevel = targetLevel;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        ArchivedAtUtc = archivedAtUtc;
    }

    public Guid Id { get; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int? CurrentLevel { get; private set; }
    public int? TargetLevel { get; private set; }
    public SkillStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    /// <summary>Returns the target gap when both current and target levels are known.</summary>
    public int? Gap => CurrentLevel is null || TargetLevel is null ? null : TargetLevel.Value - CurrentLevel.Value;

    public static Skill Create(string name, string? description, int? targetLevel, DateTimeOffset nowUtc)
    {
        ValidateLevel(targetLevel, "Target level");
        return new Skill(Guid.NewGuid(), Guard.RequiredText(name, "Skill name", 300),
            Guard.OptionalText(description, "Skill description", 10_000), null, targetLevel,
            SkillStatus.Active, nowUtc, nowUtc, null);
    }

    public static Skill Rehydrate(Guid id, string name, string? description, int? currentLevel,
        int? targetLevel, SkillStatus status, DateTimeOffset createdAtUtc, DateTimeOffset updatedAtUtc,
        DateTimeOffset? archivedAtUtc)
        => new(id, name, description, currentLevel, targetLevel, status, createdAtUtc, updatedAtUtc, archivedAtUtc);

    /// <summary>Updates descriptive metadata and desired target level.</summary>
    public void Update(string name, string? description, int? targetLevel, SkillStatus status, DateTimeOffset nowUtc)
    {
        EnsureNotArchived();
        if (status == SkillStatus.Archived)
        {
            throw new DomainValidationException("Use the archive operation to archive a skill.");
        }

        ValidateLevel(targetLevel, "Target level");
        Name = Guard.RequiredText(name, "Skill name", 300);
        Description = Guard.OptionalText(description, "Skill description", 10_000);
        TargetLevel = targetLevel;
        Status = status;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>
    /// Applies a new assessment snapshot. Only the SkillService may call this together with the
    /// immutable SkillAssessment insert so current level and history remain transactionally aligned.
    /// </summary>
    public void ApplyAssessment(int level, DateTimeOffset nowUtc)
    {
        EnsureNotArchived();
        ValidateLevel(level, "Assessment level");
        CurrentLevel = level;
        UpdatedAtUtc = nowUtc;
    }

    public void Archive(DateTimeOffset nowUtc)
    {
        Status = SkillStatus.Archived;
        ArchivedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void Restore(DateTimeOffset nowUtc)
    {
        if (Status != SkillStatus.Archived) return;
        Status = SkillStatus.Inactive;
        ArchivedAtUtc = null;
        UpdatedAtUtc = nowUtc;
    }

    internal static void ValidateLevel(int? level, string fieldName)
    {
        if (level is < 1 or > 5)
        {
            throw new DomainValidationException($"{fieldName} must be between 1 and 5.");
        }
    }

    private void EnsureNotArchived()
    {
        if (Status == SkillStatus.Archived)
        {
            throw new DomainValidationException("An archived skill must be restored before it can be changed.");
        }
    }
}
