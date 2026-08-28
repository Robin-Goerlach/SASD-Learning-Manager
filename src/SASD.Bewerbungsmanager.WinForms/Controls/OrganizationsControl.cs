using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Forms;
using SASD.Bewerbungsmanager.WinForms.Presentation;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>Lists and edits organizations through application-layer use cases.</summary>
public sealed class OrganizationsControl : UserControl
{
    private readonly OrganizationService _service;
    private readonly UiExceptionPresenter _errors;
    private readonly DataGridView _grid = ControlFactory.DataGrid();
    private IReadOnlyList<Organization> _items = [];

    /// <summary>Initializes the organizations view.</summary>
    public OrganizationsControl(OrganizationService service, UiExceptionPresenter errors)
    {
        _service = service;
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
                Name = item.Name,
                Typ = item.Type.ToString(),
                Website = item.Website ?? string.Empty,
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
        using var dialog = new OrganizationEditForm();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        try
        {
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
        using var dialog = new OrganizationEditForm(selected);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        try
        {
            await _service.UpdateAsync(selected.Id, dialog.Input);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private Organization? SelectedItem()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is Guid id)
        {
            return _items.SingleOrDefault(item => item.Id == id);
        }
        return null;
    }
}
