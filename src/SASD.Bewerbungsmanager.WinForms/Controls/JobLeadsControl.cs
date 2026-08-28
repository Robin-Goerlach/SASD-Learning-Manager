using System.Diagnostics;
using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Forms;
using SASD.Bewerbungsmanager.WinForms.Presentation;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>
/// Operational job-search inbox for normalized portal/source results. The control does not scrape
/// websites; it consumes local JSON/CSV adapters or manually copied clipboard content.
/// </summary>
public sealed class JobLeadsControl : UserControl
{
    private readonly JobLeadService _service;
    private readonly SearchProfileService _searchProfiles;
    private readonly OrganizationService _organizations;
    private readonly UiExceptionPresenter _errors;
    private readonly DataGridView _grid = ControlFactory.DataGrid();
    private readonly CheckBox _includeIgnored = new() { Text = "Ignorierte anzeigen", AutoSize = true };
    private IReadOnlyList<JobLead> _items = [];

    /// <summary>Initializes the job-search inbox.</summary>
    public JobLeadsControl(
        JobLeadService service,
        SearchProfileService searchProfiles,
        OrganizationService organizations,
        UiExceptionPresenter errors)
    {
        _service = service;
        _searchProfiles = searchProfiles;
        _organizations = organizations;
        _errors = errors;
        BuildLayout();
        Load += async (_, _) => await RefreshAsync();
        _grid.CellDoubleClick += (_, _) => OpenSelectedUrl();
        _includeIgnored.CheckedChanged += async (_, _) => await RefreshAsync();
    }

    private void BuildLayout()
    {
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 8) };
        toolbar.Controls.Add(ControlFactory.ToolbarButton("JSON/CSV importieren", async (_, _) => await ImportFileAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Zwischenablage", async (_, _) => await ImportClipboardAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("URL öffnen", (_, _) => OpenSelectedUrl()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Geprüft", async (_, _) => await MarkReviewedAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Als Stelle übernehmen", async (_, _) => await PromoteAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Ignorieren", async (_, _) => await IgnoreAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Aktualisieren", async (_, _) => await RefreshAsync()));
        toolbar.Controls.Add(_includeIgnored);
        Controls.Add(_grid);
        Controls.Add(toolbar);
    }

    private async Task RefreshAsync()
    {
        try
        {
            _items = await _service.ListAsync(_includeIgnored.Checked);
            var profiles = await _searchProfiles.ListAsync(includeInactive: true);
            var profileNames = profiles.ToDictionary(item => item.Id, item => item.Name);
            _grid.DataSource = _items.Select(item => new
            {
                item.Id,
                Status = DisplayText.JobLeadStatus(item.Status),
                Quelle = item.SourceSystem,
                Suchprofil = item.SearchProfileId is Guid id && profileNames.TryGetValue(id, out var profileName) ? profileName : string.Empty,
                Position = item.Title,
                Organisation = item.OrganizationName ?? string.Empty,
                Standort = item.Location ?? string.Empty,
                Remote = item.RemoteText ?? string.Empty,
                Gefunden = item.FoundAtUtc.LocalDateTime.ToShortDateString(),
                item.SourceUrl,
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

    private async Task ImportFileAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Job-Quellen-Handoff importieren",
            Filter = "Job-Quellen (*.json;*.csv)|*.json;*.csv|JSON (*.json)|*.json|CSV (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var profiles = await _searchProfiles.ListAsync(includeInactive: true);
            using var contextDialog = new JobSourceImportContextForm(dialog.FileName, profiles);
            if (contextDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var result = await _service.ImportFileAsync(dialog.FileName, contextDialog.SearchProfileId);
            MessageBox.Show(
                this,
                $"Importiert: {result.Imported}\nDuplikate übersprungen: {result.Duplicates}",
                "Jobsuche",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task ImportClipboardAsync()
    {
        try
        {
            var profiles = await _searchProfiles.ListAsync(includeInactive: true);
            var clipboardText = Clipboard.ContainsText() ? Clipboard.GetText(TextDataFormat.UnicodeText) : null;
            using var dialog = new JobLeadClipboardImportForm(profiles, clipboardText);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var result = await _service.ImportClipboardAsync(dialog.Input);
            MessageBox.Show(
                this,
                result.WasDuplicate ? "Der Treffer war bereits vorhanden." : "Der Treffer wurde in die Jobsuche übernommen.",
                "Jobsuche",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task MarkReviewedAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        try
        {
            await _service.MarkReviewedAsync(selected.Id);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task PromoteAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        try
        {
            var organizations = await _organizations.ListAsync(includeArchived: true);
            using var dialog = new JobLeadOpportunityForm(selected, organizations);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var opportunity = await _service.PromoteAsync(selected.Id, dialog.Input);
            MessageBox.Show(
                this,
                $"Die Stelle '{opportunity.Title}' wurde angelegt.",
                "Jobsuche",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task IgnoreAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        if (MessageBox.Show(
                this,
                $"'{selected.Title}' wirklich ignorieren?",
                "Jobsuche",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _service.IgnoreAsync(selected.Id);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private void OpenSelectedUrl()
    {
        var selected = SelectedItem();
        if (selected?.SourceUrl is null)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(selected.SourceUrl) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private JobLead? SelectedItem()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is Guid id)
        {
            return _items.SingleOrDefault(item => item.Id == id);
        }

        return null;
    }
}
