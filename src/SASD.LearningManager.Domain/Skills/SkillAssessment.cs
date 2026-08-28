using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.Domain.Skills;

/// <summary>
/// Immutable point-in-time assessment of one skill. Assessments are appended instead of updated so
/// the learning history remains transparent and later evidence can be related to a specific review.
/// </summary>
public sealed class SkillAssessment
{
    private SkillAssessment(Guid id, Guid skillId, int level, SkillAssessmentType type,
        string? reason, DateTimeOffset assessedAtUtc, DateTimeOffset createdAtUtc)
    {
        Id = id;
        SkillId = skillId;
        Level = level;
        Type = type;
        Reason = reason;
        AssessedAtUtc = assessedAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }
    public Guid SkillId { get; }
    public int Level { get; }
    public SkillAssessmentType Type { get; }
    public string? Reason { get; }
    public DateTimeOffset AssessedAtUtc { get; }
    public DateTimeOffset CreatedAtUtc { get; }

    public static SkillAssessment Create(Guid skillId, int level, SkillAssessmentType type,
        string? reason, DateTimeOffset assessedAtUtc, DateTimeOffset createdAtUtc)
    {
        if (skillId == Guid.Empty)
        {
            throw new DomainValidationException("Skill assessment requires a valid skill identifier.");
        }

        Skill.ValidateLevel(level, "Assessment level");
        if (assessedAtUtc > createdAtUtc.AddMinutes(5))
        {
            throw new DomainValidationException("Assessment time cannot be in the future.");
        }

        return new SkillAssessment(Guid.NewGuid(), skillId, level, type,
            Guard.OptionalText(reason, "Assessment reason", 10_000), assessedAtUtc, createdAtUtc);
    }

    public static SkillAssessment Rehydrate(Guid id, Guid skillId, int level, SkillAssessmentType type,
        string? reason, DateTimeOffset assessedAtUtc, DateTimeOffset createdAtUtc)
        => new(id, skillId, level, type, reason, assessedAtUtc, createdAtUtc);
}
