using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.WinForms.Presentation;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>
/// Operational Today cockpit. It deliberately prioritizes overdue actions, waiting states,
/// appointments, and due search checks over management-style charts.
/// </summary>
public sealed class DashboardControl : UserControl
{
    private readonly DashboardService _dashboardService;
    private readonly TodayService _todayService;
    private readonly UiExceptionPresenter _errors;
    private readonly Label _active = MetricLabel();
    private readonly Label _applications = MetricLabel();
    private readonly Label _interviews = MetricLabel();
    private readonly Label _offers = MetricLabel();
    private readonly DataGridView _overdue = SmallGrid();
    private readonly DataGridView _actions = SmallGrid();
    private readonly DataGridView _waiting = SmallGrid();
    private readonly DataGridView _appointments = SmallGrid();
    private readonly DataGridView _searches = SmallGrid();

    /// <summary>Initializes the operational dashboard.</summary>
    public DashboardControl(DashboardService dashboardService, TodayService todayService, UiExceptionPresenter errors)
    {
        _dashboardService = dashboardService;
        _todayService = todayService;
        _errors = errors;
        BuildLayout();
        Load += async (_, _) => await RefreshAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        header.Controls.Add(new Label
        {
            Text = "Heute / Übersicht",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 18, FontStyle.Bold),
            Margin = new Padding(0, 0, 18, 12),
        });
        header.Controls.Add(ControlFactory.ToolbarButton("Aktualisieren", async (_, _) => await RefreshAsync()));
        root.Controls.Add(header, 0, 0);

        var metrics = new TableLayoutPanel { AutoSize = true, ColumnCount = 4, Dock = DockStyle.Top };
        metrics.Controls.Add(MetricPanel("Aktive Stellen", _active), 0, 0);
        metrics.Controls.Add(MetricPanel("Bewerbungen", _applications), 1, 0);
        metrics.Controls.Add(MetricPanel("Interviews", _interviews), 2, 0);
        metrics.Controls.Add(MetricPanel("Angebote", _offers), 3, 0);
        root.Controls.Add(metrics, 0, 1);

        root.Controls.Add(new Label
        {
            Text = "Der nächste Schritt ist wichtiger als der aktuelle Status.",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 10, FontStyle.Italic),
            Margin = new Padding(0, 6, 0, 12),
        }, 0, 2);

        var operational = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
        };
        operational.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        operational.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        operational.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
        operational.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
        operational.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
        operational.Controls.Add(Section("Überfällige ACTIONs", _overdue), 0, 0);
        operational.Controls.Add(Section("Heute / ohne Termin", _actions), 1, 0);
        operational.Controls.Add(Section("WAITING_FOR", _waiting), 0, 1);
        operational.Controls.Add(Section("Nächste Termine", _appointments), 1, 1);
        operational.Controls.Add(Section("Suchquellen prüfen", _searches), 0, 2);
        operational.SetColumnSpan(operational.GetControlFromPosition(0, 2)!, 2);
        root.Controls.Add(operational, 0, 3);

        Controls.Add(root);
    }

    private async Task RefreshAsync()
    {
        try
        {
            var summary = await _dashboardService.GetSummaryAsync();
            var today = await _todayService.GetOverviewAsync();
            Apply(summary, today);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private void Apply(DashboardSummary summary, TodayOverview today)
    {
        _active.Text = summary.ActiveOpportunities.ToString(System.Globalization.CultureInfo.CurrentCulture);
        _applications.Text = summary.Applications.ToString(System.Globalization.CultureInfo.CurrentCulture);
        _interviews.Text = summary.Interviews.ToString(System.Globalization.CultureInfo.CurrentCulture);
        _offers.Text = summary.Offers.ToString(System.Globalization.CultureInfo.CurrentCulture);

        _overdue.DataSource = today.OverdueActions.Select(item => new
        {
            Aufgabe = item.Title,
            Fällig = item.DueAtUtc?.LocalDateTime.ToString("g") ?? string.Empty,
        }).ToList();
        _actions.DataSource = today.DueActions.Select(item => new
        {
            Aufgabe = item.Title,
            Fällig = item.DueAtUtc?.LocalDateTime.ToString("g") ?? "ohne Termin",
        }).ToList();
        _waiting.DataSource = today.WaitingFor.Select(item => new
        {
            Erwartung = item.Title,
            Prüfen = item.DueAtUtc?.LocalDateTime.ToString("g") ?? string.Empty,
        }).ToList();
        _appointments.DataSource = today.UpcomingAppointments.Select(item => new
        {
            Termin = item.ScheduledAtUtc?.LocalDateTime.ToString("g") ?? string.Empty,
            Art = DisplayText.ActivityKind(item.Kind),
            Betreff = item.Subject,
        }).ToList();
        _searches.DataSource = today.DueSearchProfiles.Select(item => new
        {
            Suche = item.Name,
            Quelle = item.Source,
            Fällig = item.NextCheckAtUtc.LocalDateTime.ToShortDateString(),
        }).ToList();
    }

    private static Label MetricLabel() => new()
    {
        AutoSize = true,
        Font = new Font(SystemFonts.DefaultFont.FontFamily, 20, FontStyle.Bold),
    };

    private static Control MetricPanel(string title, Label value)
    {
        var panel = new TableLayoutPanel
        {
            AutoSize = true,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 12, 0),
        };
        panel.Controls.Add(new Label { Text = title, AutoSize = true }, 0, 0);
        panel.Controls.Add(value, 0, 1);
        return panel;
    }

    private static Control Section(string title, DataGridView grid)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(4),
        };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 10, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 5),
        }, 0, 0);
        panel.Controls.Add(grid, 0, 1);
        return panel;
    }

    private static DataGridView SmallGrid()
    {
        var grid = ControlFactory.DataGrid();
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        return grid;
    }
}
