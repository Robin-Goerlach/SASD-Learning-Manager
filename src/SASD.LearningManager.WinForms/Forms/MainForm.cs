using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Infrastructure.Configuration;
using SASD.LearningManager.WinForms.Views;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>
/// Main application shell. Milestone 3 adds active Goals and Skills workspaces while retaining the
/// Resource Library, Inbox and global Quick Capture from the previous milestones.
/// </summary>
public sealed class MainForm : Form
{
    private readonly GoalsView _goalsView;
    private readonly SkillsView _skillsView;
    private readonly ResourcesView _resourcesView;
    private readonly InboxView _inboxView;
    private readonly ResourceService _resourceService;
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
    private Button? _resourcesButton;
    private Button? _inboxButton;

    public MainForm(
        GoalsView goalsView,
        SkillsView skillsView,
        ResourcesView resourcesView,
        InboxView inboxView,
        ResourceService resourceService,
        ILogger<MainForm> logger,
        ApplicationPaths paths)
    {
        _goalsView = goalsView;
        _skillsView = skillsView;
        _resourcesView = resourcesView;
        _inboxView = inboxView;
        _resourceService = resourceService;
        _logger = logger;

        Text = "SASD Learning Manager";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 700);
        Size = new Size(1400, 850);
        Font = new Font("Segoe UI", 9F);
        AutoScaleMode = AutoScaleMode.Dpi;
        KeyPreview = true;

        Controls.Add(BuildContent());
        Controls.Add(BuildSidebar());
        Controls.Add(BuildStatus(paths));

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

        nav.Controls.Add(CreateNavButton("Lernpfade", enabled: false));

        _skillsButton = CreateNavButton("Skills", enabled: true);
        _skillsButton.Click += (_, _) => ShowSkills();
        nav.Controls.Add(_skillsButton);

        _resourcesButton = CreateNavButton("Ressourcen", enabled: true);
        _resourcesButton.Click += (_, _) => ShowResources();
        nav.Controls.Add(_resourcesButton);

        _inboxButton = CreateNavButton("Inbox", enabled: true);
        _inboxButton.Click += (_, _) => ShowInbox();
        nav.Controls.Add(_inboxButton);

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
        _contentPanel.Controls.Clear();
        _goalsView.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(_goalsView);
        _ = _goalsView.RefreshAsync();
    }

    private void ShowSkills()
    {
        SetSelectedNavigation(_skillsButton);
        _titleLabel.Text = "Skills & Kompetenzlücken";
        _contentPanel.Controls.Clear();
        _skillsView.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(_skillsView);
        _ = _skillsView.RefreshAsync();
    }

    private void ShowResources()
    {
        SetSelectedNavigation(_resourcesButton);
        _titleLabel.Text = "Ressourcenbibliothek";
        _contentPanel.Controls.Clear();
        _resourcesView.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(_resourcesView);
        _ = _resourcesView.RefreshAsync();
    }

    private void ShowInbox()
    {
        SetSelectedNavigation(_inboxButton);
        _titleLabel.Text = "Inbox";
        _contentPanel.Controls.Clear();
        _inboxView.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(_inboxView);
        _ = _inboxView.RefreshAsync();
    }

    private void SetSelectedNavigation(Button? selected)
    {
        foreach (var button in new[] { _goalsButton, _skillsButton, _resourcesButton, _inboxButton })
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
}
