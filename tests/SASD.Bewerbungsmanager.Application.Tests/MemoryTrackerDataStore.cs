using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;
using TrackerActivity = SASD.Bewerbungsmanager.Domain.Entities.Activity;
using TrackerDocument = SASD.Bewerbungsmanager.Domain.Entities.Document;

namespace SASD.Bewerbungsmanager.Application.Tests;

/// <summary>Small in-memory persistence double shared by application-layer tests.</summary>
internal sealed class MemoryTrackerDataStore : ITrackerDataStore
{
    public List<Organization> Organizations { get; } = [];
    public List<Contact> Contacts { get; } = [];
    public List<Opportunity> Opportunities { get; } = [];
    public List<SourceLink> SourceLinks { get; } = [];
    public List<JobApplication> Applications { get; } = [];
    public List<TrackerActivity> Activities { get; } = [];
    public List<TrackerTask> Tasks { get; } = [];
    public List<SearchProfile> SearchProfiles { get; } = [];
    public List<TrackerDocument> Documents { get; } = [];
    public List<ApplicationDocumentSnapshot> ApplicationDocumentSnapshots { get; } = [];
    public List<CommunicationMessage> CommunicationMessages { get; } = [];
    public List<JobLead> JobLeads { get; } = [];
    public List<AssistantSession> AssistantSessions { get; } = [];

    public Task<IReadOnlyList<Organization>> ListOrganizationsAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Organization>>(Organizations.Where(item => includeArchived || !item.IsArchived).ToList());

    public Task<Organization?> GetOrganizationAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Organizations.SingleOrDefault(item => item.Id == id));

    public Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        Organizations.Add(organization);
        return Task.CompletedTask;
    }

    public Task UpdateOrganizationAsync(Organization organization, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<Contact>> ListContactsAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Contact>>(Contacts.Where(item => includeArchived || !item.IsArchived).ToList());

    public Task<Contact?> GetContactAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Contacts.SingleOrDefault(item => item.Id == id));

    public Task AddContactAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        Contacts.Add(contact);
        return Task.CompletedTask;
    }

    public Task UpdateContactAsync(Contact contact, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<Opportunity>> ListOpportunitiesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Opportunity>>(Opportunities.ToList());

    public Task<Opportunity?> GetOpportunityAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Opportunities.SingleOrDefault(item => item.Id == id));

    public Task AddOpportunityAsync(Opportunity opportunity, CancellationToken cancellationToken = default)
    {
        Opportunities.Add(opportunity);
        return Task.CompletedTask;
    }

    public Task UpdateOpportunityAsync(Opportunity opportunity, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<SourceLink>> ListSourceLinksAsync(Guid opportunityId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<SourceLink>>(SourceLinks.Where(item => item.OpportunityId == opportunityId).ToList());

    public Task AddSourceLinkAsync(SourceLink sourceLink, CancellationToken cancellationToken = default)
    {
        SourceLinks.Add(sourceLink);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<JobApplication>> ListApplicationsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<JobApplication>>(Applications.ToList());

    public Task<JobApplication?> GetApplicationAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Applications.SingleOrDefault(item => item.Id == id));

    public Task AddApplicationAsync(JobApplication application, CancellationToken cancellationToken = default)
    {
        Applications.Add(application);
        return Task.CompletedTask;
    }

    public Task UpdateApplicationSubmissionAsync(
        Guid applicationId,
        DateTimeOffset? submittedAtUtc,
        ApplicationChannel channel,
        DateTimeOffset updatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var application = Applications.Single(item => item.Id == applicationId);
        application.SubmittedAtUtc = submittedAtUtc;
        application.Channel = channel;
        application.UpdatedAtUtc = updatedAtUtc;
        return Task.CompletedTask;
    }

    public Task ChangeApplicationStageAsync(Guid applicationId, ApplicationStage stage, DateTimeOffset changedAtUtc, string? note, CancellationToken cancellationToken = default)
    {
        Applications.Single(item => item.Id == applicationId).ChangeStage(stage, changedAtUtc, note);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TrackerActivity>> ListActivitiesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TrackerActivity>>(Activities.ToList());

    public Task<TrackerActivity?> GetActivityAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Activities.SingleOrDefault(item => item.Id == id));

    public Task AddActivityAsync(TrackerActivity activity, CancellationToken cancellationToken = default)
    {
        Activities.Add(activity);
        return Task.CompletedTask;
    }

    public Task UpdateActivityAsync(TrackerActivity activity, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<TrackerTask>> ListTasksAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TrackerTask>>(Tasks.ToList());

    public Task<TrackerTask?> GetTaskAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Tasks.SingleOrDefault(item => item.Id == id));

    public Task AddTaskAsync(TrackerTask task, CancellationToken cancellationToken = default)
    {
        Tasks.Add(task);
        return Task.CompletedTask;
    }

    public Task UpdateTaskAsync(TrackerTask task, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<SearchProfile>> ListSearchProfilesAsync(bool includeInactive, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<SearchProfile>>(SearchProfiles.Where(item => includeInactive || item.IsActive).ToList());

    public Task<SearchProfile?> GetSearchProfileAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(SearchProfiles.SingleOrDefault(item => item.Id == id));

    public Task AddSearchProfileAsync(SearchProfile profile, CancellationToken cancellationToken = default)
    {
        SearchProfiles.Add(profile);
        return Task.CompletedTask;
    }

    public Task UpdateSearchProfileAsync(SearchProfile profile, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<TrackerDocument>> ListDocumentsAsync(bool includeArchived, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TrackerDocument>>(Documents.Where(item => includeArchived || !item.IsArchived).ToList());

    public Task<TrackerDocument?> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Documents.SingleOrDefault(item => item.Id == id));

    public Task AddDocumentAsync(TrackerDocument document, CancellationToken cancellationToken = default)
    {
        Documents.Add(document);
        return Task.CompletedTask;
    }

    public Task UpdateDocumentAsync(TrackerDocument document, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<ApplicationDocumentSnapshot>> ListApplicationDocumentSnapshotsAsync(Guid applicationId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ApplicationDocumentSnapshot>>(
            ApplicationDocumentSnapshots.Where(item => item.ApplicationId == applicationId).ToList());

    public Task AddApplicationDocumentSnapshotAsync(ApplicationDocumentSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ApplicationDocumentSnapshots.Add(snapshot);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<CommunicationMessage>> ListCommunicationMessagesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<CommunicationMessage>>(CommunicationMessages.ToList());

    public Task<CommunicationMessage?> GetCommunicationMessageAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(CommunicationMessages.SingleOrDefault(item => item.Id == id));

    public Task<CommunicationMessage?> FindCommunicationMessageByFingerprintAsync(string fingerprintSha256, CancellationToken cancellationToken = default)
        => Task.FromResult(CommunicationMessages.SingleOrDefault(item => item.FingerprintSha256 == fingerprintSha256));

    public Task<CommunicationMessage?> FindCommunicationMessageByExternalIdentityAsync(string sourceSystem, string externalMessageId, CancellationToken cancellationToken = default)
        => Task.FromResult(CommunicationMessages.SingleOrDefault(item => item.SourceSystem == sourceSystem && item.ExternalMessageId == externalMessageId));

    public Task AddCommunicationMessageAsync(CommunicationMessage message, CancellationToken cancellationToken = default)
    {
        CommunicationMessages.Add(message);
        return Task.CompletedTask;
    }

    public Task UpdateCommunicationMessageAsync(CommunicationMessage message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<JobLead>> ListJobLeadsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<JobLead>>(JobLeads.ToList());

    public Task<JobLead?> GetJobLeadAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(JobLeads.SingleOrDefault(item => item.Id == id));

    public Task<JobLead?> FindJobLeadByFingerprintAsync(string fingerprintSha256, CancellationToken cancellationToken = default)
        => Task.FromResult(JobLeads.SingleOrDefault(item => item.FingerprintSha256 == fingerprintSha256));

    public Task<JobLead?> FindJobLeadByExternalIdentityAsync(string sourceSystem, string externalJobId, CancellationToken cancellationToken = default)
        => Task.FromResult(JobLeads.FirstOrDefault(item => item.SourceSystem == sourceSystem && item.ExternalJobId == externalJobId));

    public Task<JobLead?> FindJobLeadBySourceUrlAsync(string sourceUrl, CancellationToken cancellationToken = default)
        => Task.FromResult(JobLeads.FirstOrDefault(item => item.SourceUrl == sourceUrl));

    public Task AddJobLeadAsync(JobLead lead, CancellationToken cancellationToken = default)
    {
        JobLeads.Add(lead);
        return Task.CompletedTask;
    }

    public Task UpdateJobLeadAsync(JobLead lead, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<AssistantSession>> ListAssistantSessionsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<AssistantSession>>(AssistantSessions.ToList());

    public Task<AssistantSession?> GetAssistantSessionAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(AssistantSessions.SingleOrDefault(item => item.Id == id));

    public Task AddAssistantSessionAsync(AssistantSession session, CancellationToken cancellationToken = default)
    {
        AssistantSessions.Add(session);
        return Task.CompletedTask;
    }

    public Task UpdateAssistantSessionAsync(AssistantSession session, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

}
