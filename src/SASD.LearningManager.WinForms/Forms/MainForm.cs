using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.ImportExport;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Infrastructure.Configuration;
using SASD.LearningManager.WinForms.Views;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>
/// Main application shell. It owns global navigation, Quick Capture and application-wide commands
/// such as portable CSV import/export while delegating business work to application services.
/// </summary>
public sealed class MainForm : Form
{
    private readonly GoalsView _goalsView;
    private readonly SkillsView _skillsView;
    private readonly LearningPathsView _learningPathsView;
    private readonly ResourcesView _resourcesView;
    private readonly InboxView _inboxView;
    private readonly ResourceService _resourceService;
    private readonly ResourceCsvTransferService _csvTransferService;
    private readonly ILogger<MainForm> _logger;
    private readonly Panel _contentPanel = new() { Dock = DockStyle.Fill };
    private readonly Label _titleLabel = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 16F, FontStyle.Bold),
        Text = "Ressourcen"
    };

    private Button? _goalsButton;
    private Button? _skillsButton;
    private Button? _learningPathsButton;
    private Button? _resourcesButton;
    private Button? _inboxButton;

    /// <summary>Creates the main shell and wires navigation plus global application commands.</summary>
    public MainForm(
        GoalsView goalsView,
        SkillsView skillsView,
        LearningPathsView learningPathsView,
        ResourcesView resourcesView,
        InboxView inboxView,
        ResourceService resourceService,
        ResourceCsvTransferService csvTransferService,
        ILogger<MainForm> logger,
        ApplicationPaths paths)
    {
        _goalsView = goalsView;
        _skillsView = skillsView;
        _learningPathsView = learningPathsView;
        _resourcesView = resourcesView;
        _inboxView = inboxView;
        _resourceService = resourceService;
        _csvTransferService = csvTransferService;
        _logger = logger;

        Text = "SASD Learning Manager";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 700);
        Size = new Size(1400, 850);
        Font = new Font("Segoe UI", 9F);
        AutoScaleMode = AutoScaleMode.Dpi;
        KeyPreview = true;

        var menu = BuildMenu();
        MainMenuStrip = menu;
        Controls.Add(BuildContent());
        Controls.Add(BuildSidebar());
        Controls.Add(BuildStatus(paths));
        Controls.Add(menu);

        KeyDown += async (_, args) =>
        {
            if (args.Control && args.Shift && args.KeyCode == Keys.N)
            {
                args.SuppressKeyPress = true;
                await ShowQuickCaptureAsync().ConfigureAwait(true);
            }
        };

        ShowGoals();
    }

    private MenuStrip BuildMenu()
    {
        var menu = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("Datei");
        var dataMenu = new ToolStripMenuItem("Daten");

        var importResources = new ToolStripMenuItem("Ressourcen aus CSV importieren …");
        importResources.Click += async (_, _) => await ImportResourcesAsync().ConfigureAwait(true);

        var exportResources = new ToolStripMenuItem("Ressourcen als CSV exportieren …");
        exportResources.Click += async (_, _) => await ExportResourcesAsync().ConfigureAwait(true);

        var exit = new ToolStripMenuItem("Beenden");
        exit.Click += (_, _) => Close();

        dataMenu.DropDownItems.Add(importResources);
        dataMenu.DropDownItems.Add(exportResources);
        fileMenu.DropDownItems.Add(exit);
        menu.Items.Add(fileMenu);
        menu.Items.Add(dataMenu);
        return menu;
    }

    private Control BuildSidebar()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 185,
            Padding = new Padding(12, 18, 12, 12),
            BackColor = Color.FromArgb(245, 247, 250)
        };

        var brand = new Label
        {
            Text = "SASD\nLearning Manager",
            AutoSize = false,
            Height = 58,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        panel.Controls.Add(brand);

        var nav = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 12, 0, 0)
        };
        panel.Controls.Add(nav);
        panel.Controls.SetChildIndex(nav, 0);

        nav.Controls.Add(CreateNavButton("Heute", enabled: false));

        _goalsButton = CreateNavButton("Ziele", enabled: true);
        _goalsButton.Click += (_, _) => ShowGoals();
        nav.Controls.Add(_goalsButton);

        _learningPathsButton = CreateNavButton("Lernpfade", enabled: true);
        _learningPathsButton.Click += (_, _) => ShowLearningPaths();
        nav.Controls.Add(_learningPathsButton);

        _skillsButton = CreateNavButton("Skills", enabled: true);
        _skillsButton.Click += (_, _) => ShowSkills();
        nav.Controls.Add(_skillsButton);

        _resourcesButton = CreateNavButton("Ressourcen", enabled: true);
        _resourcesButton.Click += (_, _) => ShowResources();
        nav.Controls.Add(_resourcesButton);

        _inboxButton = CreateNavButton("Inbox", enabled: true);
        _inboxButton.Click += (_, _) => ShowInbox();
        nav.Controls.Add(_inboxButton);

        // M5 backend services exist, while their dedicated WinForms workspaces are still pending.
        nav.Controls.Add(CreateNavButton("Wissen", enabled: false));
        nav.Controls.Add(CreateNavButton("Evidence", enabled: false));
        nav.Controls.Add(CreateNavButton("Datenpflege", enabled: false));

        return panel;
    }

    private Control BuildContent()
    {
        var outer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(22, 14, 22, 18) };
        var header = new Panel { Dock = DockStyle.Top, Height = 58 };
        _titleLabel.Location = new Point(0, 10);
        header.Controls.Add(_titleLabel);

        var capture = new Button
        {
            Text = "+ Ressource erfassen  (Ctrl+Shift+N)",
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Height = 34
        };
        capture.Location = new Point(720, 8);
        capture.Click += async (_, _) => await ShowQuickCaptureAsync().ConfigureAwait(true);
        header.Resize += (_, _) => capture.Left = Math.Max(360, header.ClientSize.Width - capture.Width);
        header.Controls.Add(capture);

        outer.Controls.Add(_contentPanel);
        outer.Controls.Add(header);
        return outer;
    }

    private static Control BuildStatus(ApplicationPaths paths)
    {
        var status = new StatusStrip { Dock = DockStyle.Bottom };
        status.Items.Add(new ToolStripStatusLabel("Bereit"));
        status.Items.Add(new ToolStripStatusLabel { Spring = true });
        status.Items.Add(new ToolStripStatusLabel($"DB: {Path.GetFileName(paths.DatabasePath)}"));
        status.Items.Add(new ToolStripStatusLabel("Local-first / offline-fähig"));
        return status;
    }

    private static Button CreateNavButton(string text, bool enabled)
    {
        return new Button
        {
            Text = text,
            Enabled = enabled,
            Width = 150,
            Height = 38,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 3, 0, 3),
            BackColor = Color.White
        };
    }

    private void ShowGoals()
    {
        SetSelectedNavigation(_goalsButton);
        _titleLabel.Text = "Lernziele";
        ShowContent(_goalsView);
        _ = _goalsView.RefreshAsync();
    }

    private void ShowLearningPaths()
    {
        SetSelectedNavigation(_learningPathsButton);
        _titleLabel.Text = "Learning Paths";
        ShowContent(_learningPathsView);
        _ = _learningPathsView.RefreshAsync();
    }

    private void ShowSkills()
    {
        SetSelectedNavigation(_skillsButton);
        _titleLabel.Text = "Skills & Kompetenzlücken";
        ShowContent(_skillsView);
        _ = _skillsView.RefreshAsync();
    }

    private void ShowResources()
    {
        SetSelectedNavigation(_resourcesButton);
        _titleLabel.Text = "Ressourcenbibliothek";
        ShowContent(_resourcesView);
        _ = _resourcesView.RefreshAsync();
    }

    private void ShowInbox()
    {
        SetSelectedNavigation(_inboxButton);
        _titleLabel.Text = "Inbox";
        ShowContent(_inboxView);
        _ = _inboxView.RefreshAsync();
    }

    private void ShowContent(Control control)
    {
        _contentPanel.Controls.Clear();
        control.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(control);
    }

    private void SetSelectedNavigation(Button? selected)
    {
        foreach (var button in new[] { _goalsButton, _learningPathsButton, _skillsButton, _resourcesButton, _inboxButton })
        {
            if (button is null)
            {
                continue;
            }

            button.Font = new Font(button.Font, button == selected ? FontStyle.Bold : FontStyle.Regular);
            button.BackColor = button == selected ? Color.FromArgb(225, 233, 244) : Color.White;
        }
    }

    private async Task ShowQuickCaptureAsync()
    {
        using var form = new QuickCaptureForm(_resourceService, _logger);
        if (form.ShowDialog(this) != DialogResult.OK || form.ResourceId is null)
        {
            return;
        }

        if (form.Outcome == QuickCaptureOutcome.OpenExisting)
        {
            ShowResources();
            await _resourcesView.OpenResourceAsync(form.ResourceId.Value).ConfigureAwait(true);
            return;
        }

        // A newly captured item belongs to the Inbox by design. Navigating there immediately gives
        // positive feedback without forcing the user to classify the entry right away.
        ShowInbox();
        await _inboxView.RefreshAsync().ConfigureAwait(true);
    }

    private async Task ImportResourcesAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Ressourcen aus CSV importieren",
            Filter = "CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var report = await _csvTransferService.ImportAsync(dialog.FileName).ConfigureAwait(true);
            var diagnostics = report.Errors.Count == 0
                ? "Keine Importhinweise."
                : string.Join(Environment.NewLine, report.Errors.Take(10).Select(error => $"Zeile {error.RowNumber}: {error.Message}"));

            if (report.Errors.Count > 10)
            {
                diagnostics += $"{Environment.NewLine}… und {report.Errors.Count - 10} weitere Hinweise.";
            }

            MessageBox.Show(
                this,
                $"CSV-Import abgeschlossen.\n\nZeilen: {report.TotalRows}\nNeu angelegt: {report.Created}\nURL-Dubletten übersprungen: {report.SkippedDuplicates}\nHinweise/Fehler: {report.Errors.Count}\n\n{diagnostics}",
                "Ressourcen importieren",
                MessageBoxButtons.OK,
                report.Errors.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            ShowResources();
            await _resourcesView.RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or FormatException)
        {
            _logger.LogWarning(exception, "Resource CSV import failed for {FilePath}", dialog.FileName);
            MessageBox.Show(this, exception.Message, "CSV-Import fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ExportResourcesAsync()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Ressourcen als CSV exportieren",
            Filter = "CSV-Dateien (*.csv)|*.csv",
            AddExtension = true,
            DefaultExt = "csv",
            FileName = $"sasd-learning-resources-{DateTime.Now:yyyyMMdd}.csv",
            OverwritePrompt = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _csvTransferService.ExportAsync(dialog.FileName).ConfigureAwait(true);
            MessageBox.Show(this, $"Export erfolgreich:\n{dialog.FileName}", "Ressourcen exportieren", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(exception, "Resource CSV export failed for {FilePath}", dialog.FileName);
            MessageBox.Show(this, exception.Message, "CSV-Export fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
