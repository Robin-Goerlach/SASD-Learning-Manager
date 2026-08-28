using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.WinForms.Forms;
using SASD.Bewerbungsmanager.WinForms.Presentation;
using TrackerDocument = SASD.Bewerbungsmanager.Domain.Entities.Document;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>Lists registered document versions and their SHA-256 fingerprints.</summary>
public sealed class DocumentsControl : UserControl
{
    private readonly DocumentService _service;
    private readonly UiExceptionPresenter _errors;
    private readonly DataGridView _grid = ControlFactory.DataGrid();
    private IReadOnlyList<TrackerDocument> _items = [];

    /// <summary>Initializes the document catalog view.</summary>
    public DocumentsControl(DocumentService service, UiExceptionPresenter errors)
    {
        _service = service;
        _errors = errors;
        BuildLayout();
        Load += async (_, _) => await RefreshAsync();
    }

    private void BuildLayout()
    {
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 0, 0, 8) };
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Dokument registrieren", async (_, _) => await RegisterAsync()));
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
                Typ = item.Type.ToString(),
                item.Label,
                item.Version,
                Sprache = item.Language,
                item.Tags,
                SHA256 = item.Sha256,
                Datei = item.OriginalPath,
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

    private async Task RegisterAsync()
    {
        using var dialog = new DocumentRegisterForm();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            // SHA-256 calculation uses asynchronous file I/O in the Infrastructure adapter. The UI
            // remains responsive even for larger PDF or Office files.
            await _service.RegisterAsync(dialog.Input);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }
}
