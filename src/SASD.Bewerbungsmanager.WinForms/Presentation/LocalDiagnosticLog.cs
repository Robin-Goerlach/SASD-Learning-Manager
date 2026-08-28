using System.Text;
using SASD.Bewerbungsmanager.Infrastructure.Persistence;

namespace SASD.Bewerbungsmanager.WinForms.Presentation;

/// <summary>
/// Writes a small local diagnostic log for UI/runtime failures. The log deliberately lives beside
/// the application's local data rather than inside the Git repository, so technical diagnostics
/// cannot accidentally become source-controlled project files.
/// </summary>
internal static class LocalDiagnosticLog
{
    private const string LogDirectoryName = "Logs";
    private const string LogFileName = "application.log";

    /// <summary>Returns the full path of the local diagnostic log file.</summary>
    public static string GetLogPath()
    {
        var databasePath = AppDataPath.GetDefaultDatabasePath();
        var dataDirectory = Path.GetDirectoryName(databasePath)
            ?? throw new InvalidOperationException("Das lokale Anwendungsdatenverzeichnis konnte nicht ermittelt werden.");

        var logDirectory = Path.Combine(dataDirectory, LogDirectoryName);
        Directory.CreateDirectory(logDirectory);
        return Path.Combine(logDirectory, LogFileName);
    }

    /// <summary>
    /// Appends a complete exception record to the local log and returns the log path. Logging is
    /// best-effort: callers should never lose the original application error merely because the
    /// diagnostic file itself cannot be written.
    /// </summary>
    public static string? TryAppend(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        try
        {
            var path = GetLogPath();
            var entry = new StringBuilder()
                .AppendLine(new string('=', 88))
                .Append("UTC: ").AppendLine(DateTimeOffset.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture))
                .Append("Process: ").AppendLine(Environment.ProcessPath ?? "<unknown>")
                .Append("Runtime: ").AppendLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription)
                .Append("OS: ").AppendLine(System.Runtime.InteropServices.RuntimeInformation.OSDescription)
                .AppendLine()
                .AppendLine(exception.ToString())
                .AppendLine()
                .ToString();

            File.AppendAllText(path, entry, Encoding.UTF8);
            return path;
        }
        catch
        {
            // The application's original exception has priority over diagnostics. In particular,
            // do not recurse into the UI exception presenter when the log directory is unwritable.
            return null;
        }
    }
}
