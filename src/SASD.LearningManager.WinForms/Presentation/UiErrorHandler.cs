using Microsoft.Extensions.Logging;
using SASD.LearningManager.Domain.Common;

namespace SASD.LearningManager.WinForms.Presentation;

/// <summary>Centralizes friendly desktop error messages while retaining a technical correlation ID in logs.</summary>
internal static class UiErrorHandler
{
    public static void Show(Form owner, Exception exception, ILogger logger, string operation)
    {
        if (exception is DomainValidationException or ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            MessageBox.Show(owner, exception.Message, "Eingabe prüfen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var errorId = $"ERR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
        logger.LogError(exception, "Unexpected UI failure in {Operation}. ErrorId={ErrorId}", operation, errorId);
        MessageBox.Show(
            owner,
            $"Die Aktion konnte nicht abgeschlossen werden. Details wurden protokolliert.\n\nFehler-ID: {errorId}",
            "Unerwarteter Fehler",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
