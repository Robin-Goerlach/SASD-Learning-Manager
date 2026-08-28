namespace SASD.LearningManager.Domain.Skills;

/// <summary>Lifecycle status of a skill definition.</summary>
public enum SkillStatus
{
    Active,
    Inactive,
    Archived
}

/// <summary>Source/context of a skill assessment.</summary>
public enum SkillAssessmentType
{
    SelfAssessment,
    PracticalReview,
    Exam,
    ProjectReview,
    MentorReview,
    Other
}
