namespace SASD.Bewerbungsmanager.Domain.Enums;

/// <summary>
/// Identifies the user-selected purpose of one assistant session. The enum describes the business
/// intent only; it deliberately does not bind the tracker to a particular AI vendor or model.
/// </summary>
public enum AssistantTaskKind
{
    /// <summary>Assess how well the opportunity matches the supplied application context.</summary>
    FitAnalysis = 0,

    /// <summary>Suggest concrete next steps without changing tracker data automatically.</summary>
    NextSteps = 1,

    /// <summary>Prepare a draft response to a recruiter or other application contact.</summary>
    RecruiterReply = 2,

    /// <summary>Prepare interview topics, questions, and talking points.</summary>
    InterviewPreparation = 3,

    /// <summary>Summarize and structure the captured job description.</summary>
    JobPostingSummary = 4,

    /// <summary>Review the current application package and identify gaps or open questions.</summary>
    ApplicationReview = 5,
}
