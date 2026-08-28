using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Presentation;

namespace SASD.Bewerbungsmanager.Presentation.Tests;

public sealed class DisplayTextTests
{
    [Theory]
    [InlineData(ApplicationStage.Draft, "Entwurf")]
    [InlineData(ApplicationStage.Submitted, "Versendet")]
    [InlineData(ApplicationStage.Interview, "Interview")]
    [InlineData(ApplicationStage.Rejected, "Absage")]
    public void ApplicationStage_ReturnsStableGermanUiLabel(ApplicationStage stage, string expected)
    {
        Assert.Equal(expected, DisplayText.ApplicationStage(stage));
    }

    [Theory]
    [InlineData(ApplicationChannel.Email, "E-Mail")]
    [InlineData(ApplicationChannel.Portal, "Portal")]
    [InlineData(ApplicationChannel.Recruiter, "Recruiter")]
    public void ApplicationChannel_ReturnsGermanLabel(ApplicationChannel channel, string expected)
    {
        Assert.Equal(expected, DisplayText.ApplicationChannel(channel));
    }

    [Theory]
    [InlineData(WorkItemKind.Action, "ACTION")]
    [InlineData(WorkItemKind.WaitingFor, "WAITING_FOR")]
    public void WorkItemKind_ReturnsOperationalLabel(WorkItemKind kind, string expected)
    {
        Assert.Equal(expected, DisplayText.WorkItemKind(kind));
    }

    [Theory]
    [InlineData(ActivityKind.Interview, "Interview")]
    [InlineData(ActivityKind.AuthorityAppointment, "Behördentermin")]
    [InlineData(ActivityKind.PhoneCall, "Telefonat")]
    public void ActivityKind_ReturnsGermanLabel(ActivityKind kind, string expected)
    {
        Assert.Equal(expected, DisplayText.ActivityKind(kind));
    }

    [Theory]
    [InlineData(CommunicationKind.Recruiter, "Recruiter / HR")]
    [InlineData(CommunicationKind.ApplicationResponse, "Bewerbungsprozess")]
    [InlineData(CommunicationKind.JobAlert, "Job-Alert")]
    public void CommunicationKind_ReturnsGermanLabel(CommunicationKind kind, string expected)
    {
        Assert.Equal(expected, DisplayText.CommunicationKind(kind));
    }

    [Theory]
    [InlineData(JobLeadStatus.New, "Neu")]
    [InlineData(JobLeadStatus.Reviewed, "Geprüft")]
    [InlineData(JobLeadStatus.Imported, "Als Stelle übernommen")]
    [InlineData(JobLeadStatus.Ignored, "Ignoriert")]
    public void JobLeadStatus_ReturnsGermanLabel(JobLeadStatus status, string expected)
        => Assert.Equal(expected, DisplayText.JobLeadStatus(status));

    [Theory]
    [InlineData(AssistantTaskKind.FitAnalysis, "Passungsanalyse")]
    [InlineData(AssistantTaskKind.InterviewPreparation, "Interviewvorbereitung")]
    [InlineData(AssistantTaskKind.RecruiterReply, "Recruiter-Antwort")]
    public void AssistantTaskKind_ReturnsGermanLabel(AssistantTaskKind kind, string expected)
        => Assert.Equal(expected, DisplayText.AssistantTaskKind(kind));

    [Theory]
    [InlineData(AssistantSessionStatus.Prepared, "Vorbereitet")]
    [InlineData(AssistantSessionStatus.Completed, "Antwort gespeichert")]
    [InlineData(AssistantSessionStatus.Discarded, "Verworfen")]
    public void AssistantSessionStatus_ReturnsGermanLabel(AssistantSessionStatus status, string expected)
        => Assert.Equal(expected, DisplayText.AssistantSessionStatus(status));

}
