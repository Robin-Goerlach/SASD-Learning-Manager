using System.Diagnostics;
using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Forms;
using SASD.Bewerbungsmanager.WinForms.Presentation;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>Manages manually checked job-search sources without scraping them.</summary>
public sealed class SearchProfilesControl : UserControl
{
    private readonly SearchProfileService _service;
    private readonly UiExceptionPresenter _errors;
    private readonly DataGridView _grid = ControlFactory.DataGrid();
    private IReadOnlyList<SearchProfile> _items = [];

    /// <summary>Initializes the search-profile view.</summary>
    public SearchProfilesControl(SearchProfileService service, UiExceptionPresenter errors)
    {
        _service = service;
        _errors = errors;
        BuildLayout();
        Load += async (_, _) => await RefreshAsync();
        _grid.CellDoubleClick += async (_, _) => await EditAsync();
    }

    private void BuildLayout()
    {
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 8) };
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Neu", async (_, _) => await CreateAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Bearbeiten", async (_, _) => await EditAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Suche öffnen", (_, _) => OpenSelected()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Heute geprüft", async (_, _) => await MarkCheckedAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Aktualisieren", async (_, _) => await RefreshAsync()));
        Controls.Add(_grid);
        Controls.Add(toolbar);
    }

    private async Task RefreshAsync()
    {
        try
        {
            _items = await _service.ListAsync(includeInactive: true);
            _grid.DataSource = _items.Select(item => new
            {
                item.Id,
                item.Name,
                Quelle = item.Source,
                NächstePrüfung = item.NextCheckAtUtc.LocalDateTime.ToShortDateString(),
                LetztePrüfung = item.LastCheckedAtUtc?.LocalDateTime.ToShortDateString() ?? string.Empty,
                Intervall = $"{item.CheckIntervalDays} Tag(e)",
                Aktiv = item.IsActive ? "Ja" : "Nein",
                item.Url,
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
        using var dialog = new SearchProfileEditForm();
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

    private async Task EditAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        using var dialog = new SearchProfileEditForm(selected);
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

    private async Task MarkCheckedAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        try
        {
            await _service.MarkCheckedAsync(selected.Id);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private void OpenSelected()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(selected.Url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private SearchProfile? SelectedItem()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is Guid id)
        {
            return _items.SingleOrDefault(item => item.Id == id);
        }

        return null;
    }
}
