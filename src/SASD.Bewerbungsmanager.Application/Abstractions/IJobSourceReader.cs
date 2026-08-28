using SASD.Bewerbungsmanager.Application.Models;

namespace SASD.Bewerbungsmanager.Application.Abstractions;

/// <summary>
/// Reads one normalized batch of discovered jobs from a local source-adapter file. Implementations
/// are intentionally file-format adapters; portal login, browser automation, and scraping stay outside
/// the application tracker.
/// </summary>
public interface IJobSourceReader
{
    /// <summary>Returns whether this reader supports the supplied local file path.</summary>
    bool CanRead(string path);

    /// <summary>Reads and validates one normalized job-search batch.</summary>
    Task<JobSourceBatch> ReadAsync(string path, CancellationToken cancellationToken = default);
}
