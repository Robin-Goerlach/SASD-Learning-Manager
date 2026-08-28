using Microsoft.Extensions.Logging;
using SASD.Bewerbungsmanager.Application.Exceptions;

namespace SASD.Bewerbungsmanager.WinForms.Presentation;

/// <summary>
/// Converts application failures into concise user-visible messages while persisting complete
/// technical detail to a local diagnostic log. Sensitive business data is deliberately not added
/// to log messages by this class; the original exception is nevertheless retained for debugging.
/// </summary>
public sealed class UiExceptionPresenter(ILogger<UiExceptionPresenter> logger)
{
    /// <summary>Shows a suitable error message for a failure raised by a UI operation.</summary>
    public void Show(Exception exception, IWin32Window? owner = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        logger.LogError(exception, "UI operation failed with {ExceptionType}.", exception.GetType().Name);
        var logPath = LocalDiagnosticLog.TryAppend(exception);

        var message = exception switch
        {
            ValidationException => exception.Message,
            KeyNotFoundException => exception.Message,
            _ => BuildTechnicalMessage(exception, logPath),
        };

        MessageBox.Show(owner, message, "SASD Bewerbungsmanager", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static string BuildTechnicalMessage(Exception exception, string? logPath)
    {
        // Show the base exception because wrapper exceptions (for example TypeInitializationException
        // or AggregateException) often hide the actionable message one level deeper.
        var root = exception.GetBaseException();
        var location = string.IsNullOrWhiteSpace(logPath)
            ? "Die Logdatei konnte nicht geschrieben werden."
            : $"Vollständige Details:\n{logPath}";

        return $"Die Aktion konnte nicht abgeschlossen werden.\n\n"
            + $"Fehler: {root.GetType().Name}\n"
            + $"{root.Message}\n\n"
            + location;
    }
}
