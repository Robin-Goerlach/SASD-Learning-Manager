using SASD.LearningManager.Domain.Competencies;

namespace SASD.LearningManager.Application.Competencies;

/// <summary>List/read model for competency areas.</summary>
public sealed record CompetencyAreaListItemDto(
    Guid Id,
    string Name,
    string? Description,
    CatalogStatus Status);

/// <summary>Editable competency-area input.</summary>
public sealed record CompetencyAreaEditModel(string Name, string? Description, CatalogStatus Status);

/// <summary>List/read model for topics including their competency-area labels.</summary>
public sealed record TopicListItemDto(
    Guid Id,
    string Name,
    string? Description,
    string CompetencyAreas,
    CatalogStatus Status);

/// <summary>Full editable topic state.</summary>
public sealed record TopicDetailDto(
    Guid Id,
    string Name,
    string? Description,
    CatalogStatus Status,
    IReadOnlyList<Guid> CompetencyAreaIds);

/// <summary>Editable topic input including area relationships.</summary>
public sealed record TopicEditModel(
    string Name,
    string? Description,
    CatalogStatus Status,
    IReadOnlyCollection<Guid> CompetencyAreaIds);
