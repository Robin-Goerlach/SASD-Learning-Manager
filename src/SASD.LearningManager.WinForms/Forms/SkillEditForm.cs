using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.Competencies;
using SASD.LearningManager.Domain.Skills;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Editor for skill metadata, target level and taxonomy assignments.</summary>
public sealed class SkillEditForm : Form
{
    private readonly SkillService _skillService;
    private readonly CompetencyCatalogService _catalogService;
    private readonly ILogger _logger;
    private readonly Guid? _skillId;
    private readonly TextBox _name = new() { MaxLength = 300 };
    private readonly TextBox _description = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = 10000 };
    private readonly Label _currentLevel = new() { AutoSize = true };
    private readonly ComboBox _targetLevel = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckedListBox _areas = new() { CheckOnClick = true, IntegralHeight = false };
    private readonly CheckedListBox _topics = new() { CheckOnClick = true, IntegralHeight = false };
    private readonly Button _save = new() { Text = "Speichern", AutoSize = true };

    public SkillEditForm(SkillService skillService, CompetencyCatalogService catalogService, ILogger logger, Guid? skillId = null)
    {
        _skillService = skillService;
        _catalogService = catalogService;
        _logger = logger;
        _skillId = skillId;
        Text = skillId is null ? "Skill anlegen" : "Skill bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(800, 650);
        Size = new Size(900, 760);
        AutoScaleMode = AutoScaleMode.Dpi;

        _targetLevel.DataSource = SkillLevelPresentation.Options(includeEmpty: true).ToList();
        _targetLevel.DisplayMember = nameof(SkillLevelOption.Text);
        _status.DataSource = new[] { SkillStatus.Active, SkillStatus.Inactive };
        _currentLevel.Text = SkillLevelPresentation.Format(null);

        Controls.Add(BuildLayout());
        AcceptButton = _save;
        Load += async (_, _) => await LoadDataAsync().ConfigureAwait(true);
        _save.Click += async (_, _) => await SaveAsync().ConfigureAwait(true);
    }

    private Control BuildLayout()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 8 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 28));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        Add(panel, 0, "Name *", _name);
        Add(panel, 1, "Beschreibung", _description);
        Add(panel, 2, "Aktuelles Level", _currentLevel);
        Add(panel, 3, "Ziel-Level", _targetLevel);
        Add(panel, 4, "Status", _status);
        Add(panel, 5, "Kompetenzbereiche", _areas);
        Add(panel, 6, "Topics", _topics);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_save);
        panel.Controls.Add(buttons, 1, 7);
        CancelButton = cancel;
        return panel;
    }

    private static void Add(TableLayoutPanel panel, int row, string label, Control control)
    {
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 3, 0, 3);
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 8, 8, 0) }, 0, row);
        panel.Controls.Add(control, 1, row);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var includeArchived = _skillId is not null;
            var areas = await _catalogService.ListAreasAsync(includeArchived).ConfigureAwait(true);
            var topics = await _catalogService.ListTopicsAsync(includeArchived).ConfigureAwait(true);
            var areaOptions = areas.Select(static x => new CatalogOption(x.Id, x.Status == CatalogStatus.Archived ? $"{x.Name} [Archiviert]" : x.Name)).ToArray();
            var topicOptions = topics.Select(static x => new CatalogOption(x.Id, x.Status == CatalogStatus.Archived ? $"{x.Name} [Archiviert]" : x.Name)).ToArray();
            _areas.DataSource = areaOptions;
            _areas.DisplayMember = nameof(CatalogOption.Name);
            _topics.DataSource = topicOptions;
            _topics.DisplayMember = nameof(CatalogOption.Name);

            if (_skillId is null) return;
            var detail = await _skillService.GetDetailAsync(_skillId.Value).ConfigureAwait(true)
                ?? throw new KeyNotFoundException("Der Skill wurde nicht gefunden.");
            if (detail.Status == SkillStatus.Archived) throw new InvalidOperationException("Archivierte Skills müssen zuerst wiederhergestellt werden.");

            _name.Text = detail.Name;
            _description.Text = detail.Description ?? string.Empty;
            _currentLevel.Text = SkillLevelPresentation.Format(detail.CurrentLevel);
            _targetLevel.SelectedItem = ((IEnumerable<SkillLevelOption>)_targetLevel.DataSource!).First(x => x.Level == detail.TargetLevel);
            _status.SelectedItem = detail.Status;

            for (var index = 0; index < areaOptions.Length; index++)
                if (detail.CompetencyAreaIds.Contains(areaOptions[index].Id)) _areas.SetItemChecked(index, true);
            for (var index = 0; index < topicOptions.Length; index++)
                if (detail.TopicIds.Contains(topicOptions[index].Id)) _topics.SetItemChecked(index, true);
        }
        catch (Exception ex)
        {
            UiErrorHandler.Show(this, ex, _logger, "Skill laden");
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            _save.Enabled = false;
            var areaIds = _areas.CheckedItems.Cast<CatalogOption>().Select(static x => x.Id).ToArray();
            var topicIds = _topics.CheckedItems.Cast<CatalogOption>().Select(static x => x.Id).ToArray();
            var target = (_targetLevel.SelectedItem as SkillLevelOption)?.Level;
            var model = new SkillEditModel(_name.Text, NullIfWhiteSpace(_description.Text), target,
                (SkillStatus)_status.SelectedItem!, areaIds, topicIds);
            if (_skillId is null) await _skillService.CreateAsync(model).ConfigureAwait(true);
            else await _skillService.UpdateAsync(_skillId.Value, model).ConfigureAwait(true);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { UiErrorHandler.Show(this, ex, _logger, "Skill speichern"); }
        finally { _save.Enabled = true; }
    }

    private static string? NullIfWhiteSpace(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private sealed record CatalogOption(Guid Id, string Name);
}
