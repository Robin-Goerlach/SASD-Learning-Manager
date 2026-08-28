using SASD.Bewerbungsmanager.Application.Models;

namespace SASD.Bewerbungsmanager.Application.Abstractions;

/// <summary>
/// Reads normalized communication handoff files produced by the SASD Mail Workbench or another
/// trusted local adapter. The application layer depends only on this format-level port and never on
/// IMAP, POP3, SMTP, or provider-specific APIs.
/// </summary>
public interface ICommunicationHandoffReader
{
    /// <summary>Reads and validates one local communication handoff file.</summary>
    Task<CommunicationHandoffBatch> ReadAsync(string path, CancellationToken cancellationToken = default);
}
