using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.WinForms.Forms;
using SASD.Bewerbungsmanager.WinForms.Presentation;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>
/// Lists concrete applications and exposes status history, factual submission metadata, immutable
/// document assignment, and the manual "context for ChatGPT" handoff without embedded generative AI.
/// </summary>
public sealed class ApplicationsControl : UserControl
{
    private readonly ApplicationService _service;
    private readonly OpportunityService _opportunities;
    private readonly DocumentService _documents;
    private readonly ApplicationContextService _context;
    private readonly UiExceptionPresenter _errors;
    private readonly DataGridView _grid = ControlFactory.DataGrid();
    private IReadOnlyList<JobApplication> _items = [];

    /// <summary>Initializes the applications view.</summary>
    public ApplicationsControl(
        ApplicationService service,
        OpportunityService opportunities,
        DocumentService documents,
        ApplicationContextService context,
        UiExceptionPresenter errors)
    {
        _service = service;
        _opportunities = opportunities;
        _documents = documents;
        _context = context;
        _errors = errors;
        BuildLayout();
        Load += async (_, _) => await RefreshAsync();
    }

    private void BuildLayout()
    {
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8),
            WrapContents = true,
        };
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Neue Bewerbung", async (_, _) => await CreateAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Status ändern", async (_, _) => await ChangeStageAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Versanddaten", async (_, _) => await EditSubmissionAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Historie", (_, _) => ShowHistory()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Dokument zuordnen", async (_, _) => await AttachDocumentAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Verwendete Dokumente", async (_, _) => await ShowDocumentsAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Kontext für ChatGPT kopieren", async (_, _) => await CopyContextAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Aktualisieren", async (_, _) => await RefreshAsync()));
        Controls.Add(_grid);
        Controls.Add(toolbar);
    }

    private async Task RefreshAsync()
    {
        try
        {
            _items = await _service.ListAsync();
            var opportunities = await _opportunities.ListAsync();
            var titles = opportunities.ToDictionary(item => item.Id, item => item.Title);
            _grid.DataSource = _items.Select(item => new
            {
                item.Id,
                Position = titles.TryGetValue(item.OpportunityId, out var title) ? title : "(Stelle nicht gefunden)",
                Status = DisplayText.ApplicationStage(item.Stage),
                Kanal = DisplayText.ApplicationChannel(item.Channel),
                Gestartet = item.StartedAtUtc.LocalDateTime.ToShortDateString(),
                Versendet = item.SubmittedAtUtc?.LocalDateTime.ToShortDateString() ?? string.Empty,
                Historie = item.StatusHistory.Count,
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
            using var dialog = new ApplicationEditForm(opportunities);
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

    private async Task ChangeStageAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        using var dialog = new ApplicationStageForm(selected.Stage);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _service.ChangeStageAsync(selected.Id, dialog.Stage, dialog.Note);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task EditSubmissionAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        using var dialog = new ApplicationSubmissionForm(selected);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _service.UpdateSubmissionAsync(selected.Id, dialog.Input);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private void ShowHistory()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        using var dialog = new ApplicationHistoryForm(selected);
        dialog.ShowDialog(this);
    }

    private async Task AttachDocumentAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        try
        {
            var documents = await _documents.ListAsync();
            using var dialog = new ApplicationDocumentAttachForm(documents);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            await _documents.AttachToApplicationAsync(selected.Id, dialog.SelectedDocumentId);
            MessageBox.Show(
                this,
                "Die konkrete Dokumentversion wurde geprüft und als unveränderlicher Snapshot der Bewerbung zugeordnet.",
                "SASD Bewerbungsmanager",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task ShowDocumentsAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        try
        {
            var snapshots = await _documents.ListApplicationSnapshotsAsync(selected.Id);
            using var dialog = new ApplicationDocumentsForm(snapshots);
            dialog.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task CopyContextAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        try
        {
            var text = await _context.BuildAsync(selected.Id);
            Clipboard.SetText(text);
            MessageBox.Show(
                this,
                "Der Bewerbungskontext wurde in die Zwischenablage kopiert.",
                "SASD Bewerbungsmanager",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private JobApplication? SelectedItem()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is Guid id)
        {
            return _items.SingleOrDefault(item => item.Id == id);
        }

        return null;
    }
}
