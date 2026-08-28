using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Domain.Competencies;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Editor for a topic and its many-to-many competency-area assignments.</summary>
public sealed class TopicEditForm : Form
{
    private readonly CompetencyCatalogService _service;
    private readonly ILogger _logger;
    private readonly Guid? _id;
    private readonly TextBox _name = new() { MaxLength = 200 };
    private readonly TextBox _description = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = 4000 };
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckedListBox _areas = new() { CheckOnClick = true, IntegralHeight = false };
    private readonly Button _save = new() { Text = "Speichern", AutoSize = true };

    public TopicEditForm(CompetencyCatalogService service, ILogger logger, Guid? id = null)
    {
        _service = service;
        _logger = logger;
        _id = id;
        Text = id is null ? "Topic anlegen" : "Topic bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(680, 520);
        Size = new Size(760, 600);
        AutoScaleMode = AutoScaleMode.Dpi;
        _status.DataSource = new[] { CatalogStatus.Active, CatalogStatus.Inactive };
        Controls.Add(BuildLayout());
        AcceptButton = _save;
        Load += async (_, _) => await LoadDataAsync().ConfigureAwait(true);
        _save.Click += async (_, _) => await SaveAsync().ConfigureAwait(true);
    }

    private Control BuildLayout()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 5 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        Add(panel, 0, "Name *", _name);
        Add(panel, 1, "Status", _status);
        Add(panel, 2, "Beschreibung", _description);
        Add(panel, 3, "Kompetenzbereiche", _areas);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var cancel = new Button { Text = "Abbrechen", AutoSize = true, DialogResult = DialogResult.Cancel };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_save);
        panel.Controls.Add(buttons, 1, 4);
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
        try
        {
            var includeArchived = _id is not null;
            var areas = await _service.ListAreasAsync(includeArchived).ConfigureAwait(true);
            var options = areas.Select(static x => new AreaOption(x.Id, x.Status == CatalogStatus.Archived ? $"{x.Name} [Archiviert]" : x.Name)).ToArray();
            _areas.DataSource = options;
            _areas.DisplayMember = nameof(AreaOption.Name);

            if (_id is null) return;
            var detail = await _service.GetTopicDetailAsync(_id.Value).ConfigureAwait(true)
                ?? throw new KeyNotFoundException("Das Topic wurde nicht gefunden.");
            if (detail.Status == CatalogStatus.Archived) throw new InvalidOperationException("Archivierte Topics müssen zuerst wiederhergestellt werden.");
            _name.Text = detail.Name;
            _description.Text = detail.Description ?? string.Empty;
            _status.SelectedItem = detail.Status;
            for (var index = 0; index < options.Length; index++)
            {
                if (detail.CompetencyAreaIds.Contains(options[index].Id)) _areas.SetItemChecked(index, true);
            }
        }
        catch (Exception ex)
        {
            UiErrorHandler.Show(this, ex, _logger, "Topic laden");
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            _save.Enabled = false;
            var areaIds = _areas.CheckedItems.Cast<AreaOption>().Select(static x => x.Id).ToArray();
            var model = new TopicEditModel(_name.Text, NullIfWhiteSpace(_description.Text), (CatalogStatus)_status.SelectedItem!, areaIds);
            if (_id is null) await _service.CreateTopicAsync(model).ConfigureAwait(true);
            else await _service.UpdateTopicAsync(_id.Value, model).ConfigureAwait(true);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { UiErrorHandler.Show(this, ex, _logger, "Topic speichern"); }
        finally { _save.Enabled = true; }
    }

    private static string? NullIfWhiteSpace(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private sealed record AreaOption(Guid Id, string Name);
}
