using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Stores one optional, reviewable assistant handoff. The tracker prepares the prompt locally and
/// never applies assistant output to job-search data automatically. A user may copy the prompt into
/// ChatGPT, another hosted model, or a local model and paste the response back deliberately.
/// </summary>
public sealed class AssistantSession
{
    /// <summary>Gets or sets the stable local identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the related opportunity, when the session is scoped to a job opportunity.</summary>
    public Guid? OpportunityId { get; set; }

    /// <summary>Gets or sets the related concrete application, when available.</summary>
    public Guid? ApplicationId { get; set; }

    /// <summary>Gets or sets the business purpose selected for this assistant request.</summary>
    public AssistantTaskKind TaskKind { get; set; }

    /// <summary>Gets or sets the current local lifecycle state.</summary>
    public AssistantSessionStatus Status { get; set; }

    /// <summary>Gets or sets a short user-facing title that remains useful in session history.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SHA-256 hash of the context that was used when the prompt was prepared. This
    /// makes the request reproducible without pretending that later tracker edits changed old prompts.
    /// </summary>
    public string ContextSha256 { get; set; } = string.Empty;

    /// <summary>Gets or sets the complete, reviewable prompt prepared by the application.</summary>
    public string PromptText { get; set; } = string.Empty;

    /// <summary>Gets or sets the response pasted back by the user, if the session was completed.</summary>
    public string? ResponseText { get; set; }

    /// <summary>
    /// Gets or sets an optional provider label supplied by the user, for example "ChatGPT" or
    /// "Local model". No API key, token, or credential is stored here.
    /// </summary>
    public string? ProviderLabel { get; set; }

    /// <summary>Gets or sets optional user instructions that were added to the standard task template.</summary>
    public string? AdditionalInstructions { get; set; }

    /// <summary>Gets or sets when the prompt session was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets when a response was accepted into the local tracker.</summary>
    public DateTimeOffset? CompletedAtUtc { get; set; }

    /// <summary>Gets or sets the most recent local state-change timestamp.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>
    /// Completes this session with explicitly user-supplied assistant output. The method does not
    /// interpret the response and therefore cannot mutate opportunities, applications, or tasks.
    /// </summary>
    /// <param name="responseText">Assistant response pasted or otherwise supplied by the user.</param>
    /// <param name="providerLabel">Optional descriptive provider/model label.</param>
    /// <param name="completedAtUtc">Completion timestamp.</param>
    public void Complete(string responseText, string? providerLabel, DateTimeOffset completedAtUtc)
    {
        if (Status != AssistantSessionStatus.Prepared)
        {
            throw new InvalidOperationException("Nur eine vorbereitete Assistenz-Sitzung kann abgeschlossen werden.");
        }

        if (string.IsNullOrWhiteSpace(responseText))
        {
            throw new ArgumentException("Eine Assistenz-Antwort ist erforderlich.", nameof(responseText));
        }

        ResponseText = responseText.Trim();
        ProviderLabel = string.IsNullOrWhiteSpace(providerLabel) ? null : providerLabel.Trim();
        Status = AssistantSessionStatus.Completed;
        CompletedAtUtc = completedAtUtc;
        UpdatedAtUtc = completedAtUtc;
    }

    /// <summary>Marks a prepared session deliberately discarded without deleting its audit trail.</summary>
    /// <param name="changedAtUtc">Timestamp of the discard action.</param>
    public void Discard(DateTimeOffset changedAtUtc)
    {
        if (Status == AssistantSessionStatus.Completed)
        {
            throw new InvalidOperationException("Eine abgeschlossene Assistenz-Sitzung kann nicht verworfen werden.");
        }

        Status = AssistantSessionStatus.Discarded;
        UpdatedAtUtc = changedAtUtc;
    }
}
