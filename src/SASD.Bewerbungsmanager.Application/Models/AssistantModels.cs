using SASD.Bewerbungsmanager.Domain.Enums;

namespace SASD.Bewerbungsmanager.Application.Models;

/// <summary>Input used to prepare one optional assistant handoff session.</summary>
public sealed record AssistantPreparationInput(
    Guid? OpportunityId,
    Guid? ApplicationId,
    AssistantTaskKind TaskKind,
    string? AdditionalInstructions);

/// <summary>Input used when a user deliberately stores an assistant response.</summary>
public sealed record AssistantCompletionInput(string ResponseText, string? ProviderLabel);

/// <summary>
/// Read model used by the preparation dialog so the user can select an opportunity or application
/// without exposing presentation code to persistence details.
/// </summary>
public sealed record AssistantTarget(Guid Id, Guid OpportunityId, string DisplayText, bool IsApplication);
