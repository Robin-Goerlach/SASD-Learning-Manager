using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Forms;
using SASD.Bewerbungsmanager.WinForms.Presentation;
using TrackerActivity = SASD.Bewerbungsmanager.Domain.Entities.Activity;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>Focused view of planned appointments, including interviews and authority appointments.</summary>
public sealed class AppointmentsControl : UserControl
{
    private readonly ActivityService _service;
    private readonly OpportunityService _opportunities;
    private readonly ApplicationService _applications;
    private readonly ContactService _contacts;
    private readonly OrganizationService _organizations;
    private readonly UiExceptionPresenter _errors;
    private readonly DataGridView _grid = ControlFactory.DataGrid();
    private IReadOnlyList<TrackerActivity> _items = [];

    /// <summary>Initializes the appointment view.</summary>
    public AppointmentsControl(
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
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Neuer Termin", async (_, _) => await CreateAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Erledigt", async (_, _) => await CompleteAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Absagen", async (_, _) => await CancelAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Aktualisieren", async (_, _) => await RefreshAsync()));
        Controls.Add(_grid);
        Controls.Add(toolbar);
    }

    private async Task RefreshAsync()
    {
        try
        {
            _items = (await _service.ListAsync())
                .Where(item => item.Status == ActivityStatus.Planned)
                .OrderBy(item => item.ScheduledAtUtc)
                .ToList();
            _grid.DataSource = _items.Select(item => new
            {
                item.Id,
                Termin = item.ScheduledAtUtc?.LocalDateTime.ToString("g") ?? string.Empty,
                Art = DisplayText.ActivityKind(item.Kind),
                Betreff = item.Subject,
                Notizen = item.Notes ?? string.Empty,
            }).ToList();
            if (_grid.Columns["Id"] is { } idColumn)
            {
                idColumn.Visible = false;
            }
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task CreateAsync()
    {
        try
        {
            var opportunities = await _opportunities.ListAsync();
            var applications = await _applications.ListAsync();
            var contacts = await _contacts.ListAsync(includeArchived: true);
            var organizations = await _organizations.ListAsync(includeArchived: true);
            using var dialog = new ActivityEditForm(opportunities, applications, contacts, organizations, plannedByDefault: true);
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
        if (item is null)
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
        if (item is null)
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
}
