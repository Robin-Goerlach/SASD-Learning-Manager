using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Domain.Entities;

/// <summary>
/// Represents a concrete application for an <see cref="Opportunity"/>.
/// </summary>
public sealed class Application
{
    private readonly List<ApplicationStatusHistory> _statusHistory = [];

    /// <summary>Gets or sets the stable technical identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the opportunity for which this application exists.</summary>
    public Guid OpportunityId { get; set; }

    /// <summary>Gets or sets when preparation of the application started.</summary>
    public DateTimeOffset StartedAtUtc { get; set; }

    /// <summary>Gets or sets when the application was actually submitted.</summary>
    public DateTimeOffset? SubmittedAtUtc { get; set; }

    /// <summary>Gets the current application stage.</summary>
    public ApplicationStage Stage { get; private set; }

    /// <summary>Gets or sets the channel used for the application.</summary>
    public ApplicationChannel Channel { get; set; }

    /// <summary>Gets or sets the salary expectation communicated for this application.</summary>
    public string? SalaryExpectation { get; set; }

    /// <summary>Gets or sets when the process was closed.</summary>
    public DateTimeOffset? ClosedAtUtc { get; set; }

    /// <summary>Gets or sets a concise human-readable outcome.</summary>
    public string? Outcome { get; set; }

    /// <summary>Gets or sets when this record was first created.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>Gets or sets when this record was most recently changed.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>Gets the chronological status history maintained by the application.</summary>
    public IReadOnlyCollection<ApplicationStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    /// <summary>
    /// Initializes the first stage and writes the first history item. This method is intended for
    /// newly created applications and must not be used to overwrite an existing process history.
    /// </summary>
    /// <param name="stage">Initial stage.</param>
    /// <param name="changedAtUtc">Time at which the stage became effective.</param>
    /// <param name="note">Optional explanation.</param>
    public void InitializeStage(ApplicationStage stage, DateTimeOffset changedAtUtc, string? note = null)
    {
        if (_statusHistory.Count != 0)
        {
            throw new InvalidOperationException("The application stage has already been initialized.");
        }

        Stage = stage;
        _statusHistory.Add(CreateHistory(stage, changedAtUtc, note));
    }

    /// <summary>
    /// Changes the current stage and appends a history entry. Re-applying the same stage is treated
    /// as a no-op to avoid meaningless duplicate history records.
    /// </summary>
    /// <param name="stage">New stage.</param>
    /// <param name="changedAtUtc">Time of the transition.</param>
    /// <param name="note">Optional explanation.</param>
    public void ChangeStage(ApplicationStage stage, DateTimeOffset changedAtUtc, string? note = null)
    {
        if (_statusHistory.Count == 0)
        {
            throw new InvalidOperationException("Initialize the application stage before changing it.");
        }

        if (Stage == stage)
        {
            return;
        }

        Stage = stage;
        UpdatedAtUtc = changedAtUtc;
        _statusHistory.Add(CreateHistory(stage, changedAtUtc, note));
    }

    /// <summary>
    /// Restores persisted history when an application is materialized outside an ORM tracking graph.
    /// The method is intentionally internal to the domain assembly's collaborators through EF field mapping.
    /// </summary>
    /// <param name="history">Persisted history items.</param>
    internal void ReplaceStatusHistory(IEnumerable<ApplicationStatusHistory> history)
    {
        _statusHistory.Clear();
        _statusHistory.AddRange(history.OrderBy(item => item.ChangedAtUtc));
    }

    private ApplicationStatusHistory CreateHistory(ApplicationStage stage, DateTimeOffset changedAtUtc, string? note)
        => new()
        {
            Id = Guid.NewGuid(),
            ApplicationId = Id,
            Stage = stage,
            ChangedAtUtc = changedAtUtc,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
        };
}
