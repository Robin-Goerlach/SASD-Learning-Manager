using Microsoft.Extensions.DependencyInjection;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>
/// Main application shell for the current desktop product. Navigation follows a classic Windows business
/// application pattern and keeps the frequently used operational areas one click away.
/// </summary>
public sealed class MainForm : Form
{
    private readonly IServiceProvider _services;
    private readonly Panel _contentPanel = new() { Dock = DockStyle.Fill, Padding = new Padding(12) };
    private Control? _currentView;

    /// <summary>Initializes the main application window.</summary>
    public MainForm(IServiceProvider services)
    {
        _services = services;
        Text = "SASD Bewerbungsmanager — v0.7.0 Releasehärtung II / RC-Vorbereitung";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1150, 720);
        Size = new Size(1320, 840);
        AutoScaleMode = AutoScaleMode.Dpi;

        var navigation = BuildNavigation();
        Controls.Add(_contentPanel);
        Controls.Add(navigation);
        Shown += (_, _) => ShowView<DashboardControl>();
    }

    private Control BuildNavigation()
    {
        var navigation = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 205,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10, 16, 10, 10),
            AutoScroll = true,
        };

        var title = new Label
        {
            Text = "SASD\nBewerbungsmanager",
            AutoSize = false,
            Width = 178,
            Height = 56,
            Font = new Font(Font.FontFamily, 11, FontStyle.Bold),
        };
        navigation.Controls.Add(title);
        navigation.Controls.Add(NavButton("Heute", () => ShowView<DashboardControl>()));
        navigation.Controls.Add(NavButton("Aufgaben", () => ShowView<TasksControl>()));
        navigation.Controls.Add(NavButton("Termine", () => ShowView<AppointmentsControl>()));
        navigation.Controls.Add(NavButton("Verlauf", () => ShowView<ActivitiesControl>()));
        navigation.Controls.Add(NavButton("Kommunikation", () => ShowView<CommunicationsControl>()));
        navigation.Controls.Add(NavButton("Jobsuche", () => ShowView<JobLeadsControl>()));
        navigation.Controls.Add(NavButton("Assistenz", () => ShowView<AssistantControl>()));
        navigation.Controls.Add(NavButton("Sicherung / Diagnose", () => ShowView<BackupDiagnosticsControl>()));
        navigation.Controls.Add(NavButton("Suchquellen", () => ShowView<SearchProfilesControl>()));
        navigation.Controls.Add(NavButton("Nachweise / Export", () => ShowView<EvidenceExportControl>()));
        navigation.Controls.Add(NavButton("Bewerbungen", () => ShowView<ApplicationsControl>()));
        navigation.Controls.Add(NavButton("Stellen", () => ShowView<OpportunitiesControl>()));
        navigation.Controls.Add(NavButton("Kontakte", () => ShowView<ContactsControl>()));
        navigation.Controls.Add(NavButton("Organisationen", () => ShowView<OrganizationsControl>()));
        navigation.Controls.Add(NavButton("Dokumente", () => ShowView<DocumentsControl>()));
        return navigation;
    }

    private static Button NavButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            Width = 178,
            Height = 38,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 0, 5),
        };
        button.Click += (_, _) => action();
        return button;
    }

    private void ShowView<TControl>() where TControl : Control
    {
        // ActivatorUtilities creates the disposable UserControl without making the root DI
        // container retain every transient view until application shutdown. Constructor
        // dependencies still come from DI, while MainForm remains responsible for view disposal.
        var next = ActivatorUtilities.CreateInstance<TControl>(_services);
        next.Dock = DockStyle.Fill;

        _contentPanel.SuspendLayout();
        try
        {
            _contentPanel.Controls.Clear();
            _currentView?.Dispose();
            _currentView = next;
            _contentPanel.Controls.Add(next);
        }
        finally
        {
            _contentPanel.ResumeLayout();
        }
    }
}
