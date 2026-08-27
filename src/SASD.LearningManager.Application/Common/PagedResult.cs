namespace SASD.LearningManager.Application.Common;

/// <summary>Represents a single page from a larger query result.</summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    /// <summary>Gets the number of available pages. Empty result sets still expose page one in the UI.</summary>
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
