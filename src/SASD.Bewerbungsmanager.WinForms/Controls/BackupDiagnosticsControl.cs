using SASD.Bewerbungsmanager.Infrastructure.Operations;
using SASD.Bewerbungsmanager.WinForms.Forms;
using SASD.Bewerbungsmanager.WinForms.Presentation;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>
/// User-facing entry point for local backup, restore staging and privacy-conscious diagnostics.
/// The control contains no persistence rules; all authoritative work is delegated to infrastructure services.
/// </summary>
public sealed class BackupDiagnosticsControl : UserControl
{
    private readonly TrackerBackupCoordinator _backupCoordinator;
    private readonly TrackerDiagnosticsService _diagnosticsService;
    private readonly ReleaseReadinessService _releaseReadinessService;
    private readonly UiExceptionPresenter _errors;
    private readonly TextBox _status = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        WordWrap = true,
        Font = new Font(FontFamily.GenericMonospace, 9),
    };

    /// <summary>Initializes the backup, diagnostics and release-readiness workspace.</summary>
    public BackupDiagnosticsControl(
        TrackerBackupCoordinator backupCoordinator,
        TrackerDiagnosticsService diagnosticsService,
        ReleaseReadinessService releaseReadinessService,
        UiExceptionPresenter errors)
    {
        _backupCoordinator = backupCoordinator;
        _diagnosticsService = diagnosticsService;
        _releaseReadinessService = releaseReadinessService;
        _errors = errors;
        BuildLayout();
    }

    private void BuildLayout()
    {
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8),
        };
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Backup erstellen...", async (_, _) => await CreateBackupAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Backup prüfen...", async (_, _) => await ValidateBackupAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Wiederherstellen...", async (_, _) => await StageRestoreAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("RC-Check", async (_, _) => await RunReleaseReadinessAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Diagnose speichern...", async (_, _) => await WriteDiagnosticsAsync()));

        var notice = new Label
        {
            Dock = DockStyle.Bottom,
            AutoSize = false,
            Height = 82,
            Padding = new Padding(4, 8, 4, 4),
            Text = "Backups enthalten die lokale SQLite-Datenbank und private Dokument-Snapshots. " +
                   "v0.7.0 erstellt standardmäßig passwortgeschützte .sasdbak-Dateien; unverschlüsselte ZIP-Backups bleiben für Kompatibilität lesbar. " +
                   "Ein Restore wird nur vorgemerkt und beim nächsten Programmstart vor dem Öffnen der Datenbank angewendet.",
        };

        _status.Text = "Bereit.\r\n\r\nEmpfehlung vor RC1: verschlüsseltes Backup erstellen, prüfen, Restore praktisch testen und RC-Check ausführen.";
        Controls.Add(_status);
        Controls.Add(notice);
        Controls.Add(toolbar);
    }

    private async Task CreateBackupAsync()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Tracker-Backup speichern",
            Filter = "Verschlüsseltes SASD Backup (*.sasdbak)|*.sasdbak|Unverschlüsseltes ZIP-Backup (*.zip)|*.zip",
            FilterIndex = 1,
            DefaultExt = "sasdbak",
            AddExtension = true,
            FileName = $"SASD-Bewerbungsmanager-Backup-{DateTime.Now:yyyyMMdd-HHmm}.sasdbak",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var encrypted = !string.Equals(Path.GetExtension(dialog.FileName), ".zip", StringComparison.OrdinalIgnoreCase);
        string? password = null;
        if (encrypted)
        {
            using var passwordDialog = new BackupPasswordDialog(confirmPassword: true);
            if (passwordDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            password = passwordDialog.Password;
        }
        else if (MessageBox.Show(
                     this,
                     "Ein unverschlüsseltes ZIP-Backup enthält sensible Bewerbungsdaten im Klartext. Wirklich unverschlüsselt speichern?",
                     "Unverschlüsseltes Backup",
                     MessageBoxButtons.YesNo,
                     MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            UseWaitCursor = true;
            var result = await _backupCoordinator.CreateBackupAsync(dialog.FileName, password);
            _status.Text = $"Backup erstellt.\r\nSchutz: {(encrypted ? "Passwortgeschützt" : "Unverschlüsselt")}\r\n" +
                           $"Dateien: {result.FileCount}\r\nNutzdaten: {result.TotalBytes:N0} Bytes\r\n\r\n{result.Path}";
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task ValidateBackupAsync()
    {
        using var dialog = BackupOpenDialog("Backup prüfen");
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var password = AskForPasswordWhenRequired(dialog.FileName);
        if (password.RequiredButCancelled)
        {
            return;
        }

        try
        {
            UseWaitCursor = true;
            var result = await _backupCoordinator.ValidateBackupAsync(dialog.FileName, password.Value);
            var details = result.Errors.Count == 0 ? "Keine Fehler gefunden." : string.Join("\r\n- ", result.Errors.Prepend("Fehler:"));
            _status.Text = $"{result.Summary}\r\n\r\n{details}";
            MessageBox.Show(
                this,
                result.Summary,
                "Backup-Prüfung",
                MessageBoxButtons.OK,
                result.IsValid ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task StageRestoreAsync()
    {
        using var dialog = BackupOpenDialog("Backup zur Wiederherstellung auswählen");
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var password = AskForPasswordWhenRequired(dialog.FileName);
        if (password.RequiredButCancelled)
        {
            return;
        }

        if (MessageBox.Show(
                this,
                "Das ausgewählte Backup wird vollständig geprüft und für den NÄCHSTEN Programmstart vorgemerkt. " +
                "Vor dem tatsächlichen Austausch wird automatisch eine Recovery-Kopie des aktuellen Zustands angelegt. Fortfahren?",
                "Wiederherstellung vorbereiten",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            UseWaitCursor = true;
            var result = await _backupCoordinator.StageRestoreAsync(dialog.FileName, password.Value);
            _status.Text = $"Restore erfolgreich vorgemerkt.\r\nGeprüfte Dateien: {result.FileCount}\r\n\r\n" +
                           "Bitte die Anwendung vollständig schließen und neu starten. Erst dann wird der Datenbestand ersetzt.";
            MessageBox.Show(
                this,
                "Restore wurde vorgemerkt. Bitte die Anwendung jetzt schließen und neu starten.",
                "Wiederherstellung",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task RunReleaseReadinessAsync()
    {
        try
        {
            UseWaitCursor = true;
            var report = await _releaseReadinessService.CreateReportAsync();
            _status.Text = report.ToDisplayText();
            MessageBox.Show(
                this,
                report.IsReadyForRc
                    ? "Die lokalen technischen RC-Gates sind erfüllt."
                    : "Mindestens ein lokales technisches RC-Gate ist noch nicht erfüllt.",
                "Release-Readiness",
                MessageBoxButtons.OK,
                report.IsReadyForRc ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task WriteDiagnosticsAsync()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Technische Diagnose speichern",
            Filter = "JSON-Datei (*.json)|*.json",
            DefaultExt = "json",
            AddExtension = true,
            FileName = $"SASD-Bewerbungsmanager-Diagnose-{DateTime.Now:yyyyMMdd-HHmm}.json",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            UseWaitCursor = true;
            var report = await _diagnosticsService.CreateReportAsync();
            await _diagnosticsService.WriteReportAsync(dialog.FileName);
            _status.Text = $"Diagnose gespeichert.\r\nSQLite quick_check: {report.QuickCheck}\r\n" +
                           $"Foreign-Key-Verletzungen: {report.ForeignKeyViolationCount}\r\n" +
                           $"Offene Migrationen: {report.PendingMigrations.Count}\r\n\r\n{dialog.FileName}";
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private PasswordResult AskForPasswordWhenRequired(string path)
    {
        if (!_backupCoordinator.RequiresPassword(path))
        {
            return new PasswordResult(null, false);
        }

        using var passwordDialog = new BackupPasswordDialog(confirmPassword: false);
        return passwordDialog.ShowDialog(this) == DialogResult.OK
            ? new PasswordResult(passwordDialog.Password, false)
            : new PasswordResult(null, true);
    }

    private static OpenFileDialog BackupOpenDialog(string title) => new()
    {
        Title = title,
        Filter = "SASD Backups (*.sasdbak;*.zip)|*.sasdbak;*.zip|Verschlüsseltes SASD Backup (*.sasdbak)|*.sasdbak|ZIP-Backup (*.zip)|*.zip",
        CheckFileExists = true,
        Multiselect = false,
    };

    private sealed record PasswordResult(string? Value, bool RequiredButCancelled);
}
