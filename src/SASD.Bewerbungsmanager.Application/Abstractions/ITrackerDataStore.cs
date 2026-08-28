using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;

namespace SASD.Bewerbungsmanager.Application.Abstractions;

/// <summary>
/// Defines the persistence operations needed by the early application layer. It is deliberately a
/// single pragmatic port instead of a generic repository hierarchy for every entity.
/// </summary>
public interface ITrackerDataStore
{
    Task<IReadOnlyList<Organization>> ListOrganizationsAsync(bool includeArchived, CancellationToken cancellationToken = default);
    Task<Organization?> GetOrganizationAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddOrganizationAsync(Organization organization, CancellationToken cancellationToken = default);
    Task UpdateOrganizationAsync(Organization organization, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Contact>> ListContactsAsync(bool includeArchived, CancellationToken cancellationToken = default);
    Task<Contact?> GetContactAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddContactAsync(Contact contact, CancellationToken cancellationToken = default);
    Task UpdateContactAsync(Contact contact, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Opportunity>> ListOpportunitiesAsync(CancellationToken cancellationToken = default);
    Task<Opportunity?> GetOpportunityAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddOpportunityAsync(Opportunity opportunity, CancellationToken cancellationToken = default);
    Task UpdateOpportunityAsync(Opportunity opportunity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SourceLink>> ListSourceLinksAsync(Guid opportunityId, CancellationToken cancellationToken = default);
    Task AddSourceLinkAsync(SourceLink sourceLink, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobApplication>> ListApplicationsAsync(CancellationToken cancellationToken = default);
    Task<JobApplication?> GetApplicationAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddApplicationAsync(JobApplication application, CancellationToken cancellationToken = default);
    Task UpdateApplicationSubmissionAsync(Guid applicationId, DateTimeOffset? submittedAtUtc, ApplicationChannel channel, DateTimeOffset updatedAtUtc, CancellationToken cancellationToken = default);
    Task ChangeApplicationStageAsync(Guid applicationId, ApplicationStage stage, DateTimeOffset changedAtUtc, string? note, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Activity>> ListActivitiesAsync(CancellationToken cancellationToken = default);
    Task<Activity?> GetActivityAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddActivityAsync(Activity activity, CancellationToken cancellationToken = default);
    Task UpdateActivityAsync(Activity activity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TrackerTask>> ListTasksAsync(CancellationToken cancellationToken = default);
    Task<TrackerTask?> GetTaskAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddTaskAsync(TrackerTask task, CancellationToken cancellationToken = default);
    Task UpdateTaskAsync(TrackerTask task, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchProfile>> ListSearchProfilesAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<SearchProfile?> GetSearchProfileAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddSearchProfileAsync(SearchProfile profile, CancellationToken cancellationToken = default);
    Task UpdateSearchProfileAsync(SearchProfile profile, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Document>> ListDocumentsAsync(bool includeArchived, CancellationToken cancellationToken = default);
    Task<Document?> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddDocumentAsync(Document document, CancellationToken cancellationToken = default);
    Task UpdateDocumentAsync(Document document, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationDocumentSnapshot>> ListApplicationDocumentSnapshotsAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task AddApplicationDocumentSnapshotAsync(ApplicationDocumentSnapshot snapshot, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CommunicationMessage>> ListCommunicationMessagesAsync(CancellationToken cancellationToken = default);
    Task<CommunicationMessage?> GetCommunicationMessageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CommunicationMessage?> FindCommunicationMessageByFingerprintAsync(string fingerprintSha256, CancellationToken cancellationToken = default);
    Task<CommunicationMessage?> FindCommunicationMessageByExternalIdentityAsync(string sourceSystem, string externalMessageId, CancellationToken cancellationToken = default);
    Task AddCommunicationMessageAsync(CommunicationMessage message, CancellationToken cancellationToken = default);
    Task UpdateCommunicationMessageAsync(CommunicationMessage message, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobLead>> ListJobLeadsAsync(CancellationToken cancellationToken = default);
    Task<JobLead?> GetJobLeadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobLead?> FindJobLeadByFingerprintAsync(string fingerprintSha256, CancellationToken cancellationToken = default);
    Task<JobLead?> FindJobLeadByExternalIdentityAsync(string sourceSystem, string externalJobId, CancellationToken cancellationToken = default);
    Task<JobLead?> FindJobLeadBySourceUrlAsync(string sourceUrl, CancellationToken cancellationToken = default);
    Task AddJobLeadAsync(JobLead lead, CancellationToken cancellationToken = default);
    Task UpdateJobLeadAsync(JobLead lead, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssistantSession>> ListAssistantSessionsAsync(CancellationToken cancellationToken = default);
    Task<AssistantSession?> GetAssistantSessionAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAssistantSessionAsync(AssistantSession session, CancellationToken cancellationToken = default);
    Task UpdateAssistantSessionAsync(AssistantSession session, CancellationToken cancellationToken = default);
}
