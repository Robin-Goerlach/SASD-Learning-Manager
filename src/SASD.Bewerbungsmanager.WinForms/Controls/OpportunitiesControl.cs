using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Forms;
using SASD.Bewerbungsmanager.WinForms.Presentation;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>Lists and edits opportunities and preserves their captured description snapshots.</summary>
public sealed class OpportunitiesControl : UserControl
{
    private readonly OpportunityService _service;
    private readonly OrganizationService _organizations;
    private readonly UiExceptionPresenter _errors;
    private readonly DataGridView _grid = ControlFactory.DataGrid();
    private IReadOnlyList<Opportunity> _items = [];

    /// <summary>Initializes the opportunities view.</summary>
    public OpportunitiesControl(OpportunityService service, OrganizationService organizations, UiExceptionPresenter errors)
    {
        _service = service;
        _organizations = organizations;
        _errors = errors;
        BuildLayout();
        Load += async (_, _) => await RefreshAsync();
        _grid.CellDoubleClick += async (_, _) => await EditSelectedAsync();
    }

    private void BuildLayout()
    {
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 8) };
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Neu", async (_, _) => await CreateAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Bearbeiten", async (_, _) => await EditSelectedAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Quelle hinzufügen", async (_, _) => await AddSourceAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Aktualisieren", async (_, _) => await RefreshAsync()));
        Controls.Add(_grid);
        Controls.Add(toolbar);
    }

    private async Task RefreshAsync()
    {
        try
        {
            _items = await _service.ListAsync();
            var orgs = await _organizations.ListAsync(true);
            var names = orgs.ToDictionary(item => item.Id, item => item.Name);
            _grid.DataSource = _items.Select(item => new
            {
                item.Id,
                Position = item.Title,
                Arbeitgeber = item.EmployerOrganizationId is Guid id && names.TryGetValue(id, out var name) ? name : string.Empty,
                Status = DisplayText.OpportunityStatus(item.Status),
                Standort = item.Location ?? string.Empty,
                Gefunden = item.FoundAtUtc.LocalDateTime.ToShortDateString(),
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
            var orgs = await _organizations.ListAsync();
            using var dialog = new OpportunityEditForm(orgs);
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

    private async Task EditSelectedAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }
        try
        {
            var orgs = await _organizations.ListAsync(true);
            using var dialog = new OpportunityEditForm(orgs, selected);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            await _service.UpdateAsync(selected.Id, dialog.Input);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task AddSourceAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }
        using var dialog = new SourceLinkEditForm();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        try
        {
            await _service.AddSourceLinkAsync(selected.Id, dialog.Input);
            MessageBox.Show(this, "Quelle wurde gespeichert.", "SASD Bewerbungsmanager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private Opportunity? SelectedItem()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is Guid id)
        {
            return _items.SingleOrDefault(item => item.Id == id);
        }
        return null;
    }
}
