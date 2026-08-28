using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Domain.Competencies;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Small editor for broad competency areas such as Linux, Cloud or Cyber Security.</summary>
public sealed class CompetencyAreaEditForm : Form
{
    private readonly CompetencyCatalogService _service;
    private readonly ILogger _logger;
    private readonly Guid? _id;
    private readonly TextBox _name = new() { MaxLength = 200 };
    private readonly TextBox _description = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = 4000 };
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button _save = new() { Text = "Speichern", AutoSize = true };

    public CompetencyAreaEditForm(CompetencyCatalogService service, ILogger logger, Guid? id = null)
    {
        _service = service;
        _logger = logger;
        _id = id;
        Text = id is null ? "Kompetenzbereich anlegen" : "Kompetenzbereich bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(620, 390);
        Size = new Size(700, 450);
        AutoScaleMode = AutoScaleMode.Dpi;
        _status.DataSource = new[] { CatalogStatus.Active, CatalogStatus.Inactive };
        Controls.Add(BuildLayout());
        AcceptButton = _save;
        Load += async (_, _) => await LoadDataAsync().ConfigureAwait(true);
        _save.Click += async (_, _) => await SaveAsync().ConfigureAwait(true);
    }

    private Control BuildLayout()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 4 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        Add(panel, 0, "Name *", _name);
        Add(panel, 1, "Status", _status);
        Add(panel, 2, "Beschreibung", _description);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var cancel = new Button { Text = "Abbrechen", AutoSize = true, DialogResult = DialogResult.Cancel };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_save);
        panel.Controls.Add(buttons, 1, 3);
        CancelButton = cancel;
        return panel;
    }

    private static void Add(TableLayoutPanel panel, int row, string label, Control control)
    {
        control.Dock = DockStyle.Fill;
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 8, 8, 0) }, 0, row);
        panel.Controls.Add(control, 1, row);
    }

    private async Task LoadDataAsync()
    {
        if (_id is null) return;
        try
        {
            var area = (await _service.ListAreasAsync(includeArchived: true).ConfigureAwait(true)).FirstOrDefault(x => x.Id == _id.Value)
                ?? throw new KeyNotFoundException("Der Kompetenzbereich wurde nicht gefunden.");
            _name.Text = area.Name;
            _description.Text = area.Description ?? string.Empty;
            if (area.Status == CatalogStatus.Archived) throw new InvalidOperationException("Archivierte Kompetenzbereiche müssen zuerst wiederhergestellt werden.");
            _status.SelectedItem = area.Status;
        }
        catch (Exception ex)
        {
            UiErrorHandler.Show(this, ex, _logger, "Kompetenzbereich laden");
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            _save.Enabled = false;
            var model = new CompetencyAreaEditModel(_name.Text, NullIfWhiteSpace(_description.Text), (CatalogStatus)_status.SelectedItem!);
            if (_id is null) await _service.CreateAreaAsync(model).ConfigureAwait(true);
            else await _service.UpdateAreaAsync(_id.Value, model).ConfigureAwait(true);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { UiErrorHandler.Show(this, ex, _logger, "Kompetenzbereich speichern"); }
        finally { _save.Enabled = true; }
    }

    private static string? NullIfWhiteSpace(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
