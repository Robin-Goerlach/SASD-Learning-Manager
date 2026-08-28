using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.WinForms.Presentation;

/// <summary>Centralizes concise German labels used by the WinForms presentation layer.</summary>
public static class DisplayText
{
    /// <summary>Returns a user-facing label for an opportunity status.</summary>
    public static string OpportunityStatus(OpportunityStatus status) => status switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.OpportunityStatus.Identified => "Gefunden",
        SASD.Bewerbungsmanager.Domain.Enums.OpportunityStatus.Contacted => "Kontakt",
        SASD.Bewerbungsmanager.Domain.Enums.OpportunityStatus.ApplicationPlanned => "Bewerbung geplant",
        SASD.Bewerbungsmanager.Domain.Enums.OpportunityStatus.Applied => "Beworben",
        SASD.Bewerbungsmanager.Domain.Enums.OpportunityStatus.Interview => "Interview",
        SASD.Bewerbungsmanager.Domain.Enums.OpportunityStatus.Offer => "Angebot",
        SASD.Bewerbungsmanager.Domain.Enums.OpportunityStatus.Closed => "Abgeschlossen",
        _ => status.ToString(),
    };

    /// <summary>Returns a user-facing label for an application stage.</summary>
    public static string ApplicationStage(ApplicationStage stage) => stage switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationStage.Draft => "Entwurf",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationStage.Submitted => "Versendet",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationStage.Screening => "Prüfung",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationStage.Interview => "Interview",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationStage.Offer => "Angebot",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationStage.Rejected => "Absage",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationStage.Withdrawn => "Zurückgezogen",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationStage.Hired => "Eingestellt",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationStage.Closed => "Abgeschlossen",
        _ => stage.ToString(),
    };


    /// <summary>Returns a user-facing label for an application submission channel.</summary>
    public static string ApplicationChannel(ApplicationChannel channel) => channel switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationChannel.Unknown => "Unbekannt",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationChannel.Portal => "Portal",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationChannel.Email => "E-Mail",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationChannel.LinkedIn => "LinkedIn",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationChannel.Recruiter => "Recruiter",
        SASD.Bewerbungsmanager.Domain.Enums.ApplicationChannel.Other => "Sonstiges",
        _ => channel.ToString(),
    };

    /// <summary>Returns a user-facing label for an operational work-item kind.</summary>
    public static string WorkItemKind(WorkItemKind kind) => kind switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.WorkItemKind.Action => "ACTION",
        SASD.Bewerbungsmanager.Domain.Enums.WorkItemKind.WaitingFor => "WAITING_FOR",
        _ => kind.ToString(),
    };

    /// <summary>Returns a user-facing label for a work-item lifecycle state.</summary>
    public static string WorkItemStatus(WorkItemStatus status) => status switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.WorkItemStatus.Open => "Offen",
        SASD.Bewerbungsmanager.Domain.Enums.WorkItemStatus.Completed => "Erledigt",
        SASD.Bewerbungsmanager.Domain.Enums.WorkItemStatus.Cancelled => "Abgebrochen",
        _ => status.ToString(),
    };

    /// <summary>Returns a user-facing label for an activity type.</summary>
    public static string ActivityKind(ActivityKind kind) => kind switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.ActivityKind.Email => "E-Mail",
        SASD.Bewerbungsmanager.Domain.Enums.ActivityKind.PhoneCall => "Telefonat",
        SASD.Bewerbungsmanager.Domain.Enums.ActivityKind.LinkedIn => "LinkedIn",
        SASD.Bewerbungsmanager.Domain.Enums.ActivityKind.ApplicationSubmitted => "Bewerbung versendet",
        SASD.Bewerbungsmanager.Domain.Enums.ActivityKind.Interview => "Interview",
        SASD.Bewerbungsmanager.Domain.Enums.ActivityKind.Meeting => "Meeting",
        SASD.Bewerbungsmanager.Domain.Enums.ActivityKind.AuthorityAppointment => "Behördentermin",
        SASD.Bewerbungsmanager.Domain.Enums.ActivityKind.Note => "Notiz",
        SASD.Bewerbungsmanager.Domain.Enums.ActivityKind.Other => "Sonstiges",
        _ => kind.ToString(),
    };

    /// <summary>Returns a user-facing label for an activity lifecycle state.</summary>
    public static string ActivityStatus(ActivityStatus status) => status switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.ActivityStatus.Recorded => "Stattgefunden",
        SASD.Bewerbungsmanager.Domain.Enums.ActivityStatus.Planned => "Geplant",
        SASD.Bewerbungsmanager.Domain.Enums.ActivityStatus.Completed => "Erledigt",
        SASD.Bewerbungsmanager.Domain.Enums.ActivityStatus.Cancelled => "Abgesagt",
        _ => status.ToString(),
    };

    /// <summary>Returns a user-facing label for communication direction.</summary>
    public static string CommunicationDirection(CommunicationDirection direction) => direction switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.CommunicationDirection.Incoming => "Eingang",
        SASD.Bewerbungsmanager.Domain.Enums.CommunicationDirection.Outgoing => "Ausgang",
        _ => direction.ToString(),
    };

    /// <summary>Returns a user-facing label for communication classification.</summary>
    public static string CommunicationKind(CommunicationKind kind) => kind switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.CommunicationKind.Unclassified => "Unklassifiziert",
        SASD.Bewerbungsmanager.Domain.Enums.CommunicationKind.Recruiter => "Recruiter / HR",
        SASD.Bewerbungsmanager.Domain.Enums.CommunicationKind.ApplicationResponse => "Bewerbungsprozess",
        SASD.Bewerbungsmanager.Domain.Enums.CommunicationKind.JobAlert => "Job-Alert",
        SASD.Bewerbungsmanager.Domain.Enums.CommunicationKind.General => "Allgemein",
        _ => kind.ToString(),
    };

    /// <summary>Returns a user-facing label for communication processing state.</summary>
    public static string CommunicationStatus(CommunicationStatus status) => status switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.CommunicationStatus.Imported => "Importiert",
        SASD.Bewerbungsmanager.Domain.Enums.CommunicationStatus.Linked => "Zugeordnet",
        SASD.Bewerbungsmanager.Domain.Enums.CommunicationStatus.Ignored => "Ignoriert",
        _ => status.ToString(),
    };

    /// <summary>Returns a user-facing label for a discovered job result.</summary>
    public static string JobLeadStatus(JobLeadStatus status) => status switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.JobLeadStatus.New => "Neu",
        SASD.Bewerbungsmanager.Domain.Enums.JobLeadStatus.Reviewed => "Geprüft",
        SASD.Bewerbungsmanager.Domain.Enums.JobLeadStatus.Imported => "Als Stelle übernommen",
        SASD.Bewerbungsmanager.Domain.Enums.JobLeadStatus.Ignored => "Ignoriert",
        _ => status.ToString(),
    };

    /// <summary>Returns a user-facing label for an optional assistant task.</summary>
    public static string AssistantTaskKind(AssistantTaskKind kind) => kind switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.AssistantTaskKind.FitAnalysis => "Passungsanalyse",
        SASD.Bewerbungsmanager.Domain.Enums.AssistantTaskKind.NextSteps => "Nächste Schritte",
        SASD.Bewerbungsmanager.Domain.Enums.AssistantTaskKind.RecruiterReply => "Recruiter-Antwort",
        SASD.Bewerbungsmanager.Domain.Enums.AssistantTaskKind.InterviewPreparation => "Interviewvorbereitung",
        SASD.Bewerbungsmanager.Domain.Enums.AssistantTaskKind.JobPostingSummary => "Stellenanalyse",
        SASD.Bewerbungsmanager.Domain.Enums.AssistantTaskKind.ApplicationReview => "Bewerbungscheck",
        _ => kind.ToString(),
    };

    /// <summary>Returns a user-facing label for the assistant-session lifecycle.</summary>
    public static string AssistantSessionStatus(AssistantSessionStatus status) => status switch
    {
        SASD.Bewerbungsmanager.Domain.Enums.AssistantSessionStatus.Prepared => "Vorbereitet",
        SASD.Bewerbungsmanager.Domain.Enums.AssistantSessionStatus.Completed => "Antwort gespeichert",
        SASD.Bewerbungsmanager.Domain.Enums.AssistantSessionStatus.Discarded => "Verworfen",
        _ => status.ToString(),
    };

}
