using SASD.Bewerbungsmanager.Application.Models;

namespace SASD.Bewerbungsmanager.Application.Abstractions;

/// <summary>
/// Writes read-only evidence reports and exchange dossiers to user-selected files. The application
/// layer owns the semantic export models; concrete file formats remain an infrastructure concern.
/// </summary>
public interface IApplicationExportWriter
{
    /// <summary>Writes a semicolon-separated UTF-8 CSV application evidence report.</summary>
    Task WriteEvidenceCsvAsync(ApplicationEvidenceReport report, string path, CancellationToken cancellationToken = default);

    /// <summary>Writes a compact printable PDF application evidence report.</summary>
    Task WriteEvidencePdfAsync(ApplicationEvidenceReport report, string path, CancellationToken cancellationToken = default);

    /// <summary>Writes a privacy-conscious JSON dossier for one concrete application.</summary>
    Task WriteDossierJsonAsync(ApplicationExchangeDossier dossier, string path, CancellationToken cancellationToken = default);

    /// <summary>Writes a human-readable Markdown dossier for one concrete application.</summary>
    Task WriteDossierMarkdownAsync(ApplicationExchangeDossier dossier, string path, CancellationToken cancellationToken = default);
}
