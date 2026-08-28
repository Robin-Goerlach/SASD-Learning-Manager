using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Forms;
using SASD.Bewerbungsmanager.WinForms.Presentation;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>Lists operational ACTION and WAITING_FOR items and allows quick completion.</summary>
public sealed class TasksControl : UserControl
{
    private readonly WorkItemService _service;
    private readonly OpportunityService _opportunities;
    private readonly ApplicationService _applications;
    private readonly ContactService _contacts;
    private readonly OrganizationService _organizations;
    private readonly UiExceptionPresenter _errors;
    private readonly DataGridView _grid = ControlFactory.DataGrid();
    private IReadOnlyList<TrackerTask> _items = [];

    /// <summary>Initializes the task view.</summary>
    public TasksControl(
        WorkItemService service,
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
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Neu", async (_, _) => await CreateAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Erledigt", async (_, _) => await CompleteAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Abbrechen", async (_, _) => await CancelAsync()));
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
                Typ = DisplayText.WorkItemKind(item.Kind),
                Status = DisplayText.WorkItemStatus(item.Status),
                Aufgabe = item.Title,
                Fällig = item.DueAtUtc?.LocalDateTime.ToString("g") ?? string.Empty,
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
            using var dialog = new WorkItemEditForm(opportunities, applications, contacts, organizations);
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

    private TrackerTask? SelectedItem()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is Guid id)
        {
            return _items.SingleOrDefault(item => item.Id == id);
        }

        return null;
    }
}
