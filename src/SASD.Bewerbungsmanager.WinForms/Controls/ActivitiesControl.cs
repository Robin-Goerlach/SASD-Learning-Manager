using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Forms;
using SASD.Bewerbungsmanager.WinForms.Presentation;
using TrackerActivity = SASD.Bewerbungsmanager.Domain.Entities.Activity;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>Displays the chronological activity timeline and planned interactions.</summary>
public sealed class ActivitiesControl : UserControl
{
    private readonly ActivityService _service;
    private readonly OpportunityService _opportunities;
    private readonly ApplicationService _applications;
    private readonly ContactService _contacts;
    private readonly OrganizationService _organizations;
    private readonly UiExceptionPresenter _errors;
    private readonly DataGridView _grid = ControlFactory.DataGrid();
    private IReadOnlyList<TrackerActivity> _items = [];

    /// <summary>Initializes the timeline view.</summary>
    public ActivitiesControl(
        ActivityService service,
        OpportunityService opportunities,
        ApplicationService applications,
        ContactService contacts,
        OrganizationService organizations,
        UiExceptionPresenter errors)
    {
        _service = service;
        _opportunities = opportunities;
        _applications = applications;
        _contacts = contacts;
        _organizations = organizations;
        _errors = errors;
        BuildLayout();
        Load += async (_, _) => await RefreshAsync();
    }

    private void BuildLayout()
    {
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 8) };
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Neue Aktivität", async (_, _) => await CreateAsync(planned: false)));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Neuer Termin", async (_, _) => await CreateAsync(planned: true)));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Termin erledigt", async (_, _) => await CompleteAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Termin absagen", async (_, _) => await CancelAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Aktualisieren", async (_, _) => await RefreshAsync()));
        Controls.Add(_grid);
        Controls.Add(toolbar);
    }

    private async Task RefreshAsync()
    {
        try
        {
            _items = await _service.ListAsync();
            _grid.DataSource = _items.Select(item => new
            {
                item.Id,
                Zeitpunkt = FormatWhen(item),
                Art = DisplayText.ActivityKind(item.Kind),
                Status = DisplayText.ActivityStatus(item.Status),
                Betreff = item.Subject,
                Notizen = item.Notes ?? string.Empty,
            }).ToList();
            HideIdColumn();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task CreateAsync(bool planned)
    {
        try
        {
            var opportunities = await _opportunities.ListAsync();
            var applications = await _applications.ListAsync();
            var contacts = await _contacts.ListAsync(includeArchived: true);
            var organizations = await _organizations.ListAsync(includeArchived: true);
            using var dialog = new ActivityEditForm(opportunities, applications, contacts, organizations, plannedByDefault: planned);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            await _service.CreateAsync(dialog.Input);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task CompleteAsync()
    {
        var item = SelectedItem();
        if (item is null || item.Status != ActivityStatus.Planned)
        {
            return;
        }

        try
        {
            await _service.CompleteAsync(item.Id);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task CancelAsync()
    {
        var item = SelectedItem();
        if (item is null || item.Status != ActivityStatus.Planned)
        {
            return;
        }

        try
        {
            await _service.CancelAsync(item.Id);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private TrackerActivity? SelectedItem()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is Guid id)
        {
            return _items.SingleOrDefault(item => item.Id == id);
        }

        return null;
    }

    private void HideIdColumn()
    {
        if (_grid.Columns["Id"] is { } idColumn)
        {
            idColumn.Visible = false;
        }
    }

    private static string FormatWhen(TrackerActivity item)
        => (item.OccurredAtUtc ?? item.ScheduledAtUtc ?? item.CreatedAtUtc).LocalDateTime.ToString("g");
}
