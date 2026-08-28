using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.Skills;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Explicit mastery review dialog with assessment history and qualitative level labels.</summary>
public sealed class SkillAssessmentForm : Form
{
    private readonly SkillService _service;
    private readonly ILogger _logger;
    private readonly Guid _skillId;
    private readonly Label _skillName = new() { AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
    private readonly Label _current = new() { AutoSize = true };
    private readonly ComboBox _level = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _reason = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = 10000 };
    private readonly DataGridView _history = new();
    private readonly Button _save = new() { Text = "Bewertung speichern", AutoSize = true };

    public SkillAssessmentForm(SkillService service, ILogger logger, Guid skillId)
    {
        _service = service;
        _logger = logger;
        _skillId = skillId;
        Text = "Skill bewerten";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(760, 650);
        Size = new Size(860, 730);
        AutoScaleMode = AutoScaleMode.Dpi;
        _level.DataSource = SkillLevelPresentation.Options(includeEmpty: false).ToList();
        _level.DisplayMember = nameof(SkillLevelOption.Text);
        _type.DataSource = Enum.GetValues<SkillAssessmentType>();
        ConfigureHistory();
        Controls.Add(BuildLayout());
        AcceptButton = _save;
        Load += async (_, _) => await LoadDataAsync().ConfigureAwait(true);
        _save.Click += async (_, _) => await SaveAsync().ConfigureAwait(true);
    }

    private Control BuildLayout()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 7 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 68));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        panel.Controls.Add(_skillName, 1, 0);
        Add(panel, 1, "Aktuelles Level", _current);
        Add(panel, 2, "Neue Bewertung", _level);
        Add(panel, 3, "Art", _type);
        Add(panel, 4, "Begründung", _reason);
        Add(panel, 5, "Historie", _history);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_save);
        panel.Controls.Add(buttons, 1, 6);
        CancelButton = cancel;
        return panel;
    }

    private static void Add(TableLayoutPanel panel, int row, string label, Control control)
    {
        control.Dock = DockStyle.Fill;
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 8, 8, 0) }, 0, row);
        panel.Controls.Add(control, 1, row);
    }

    private void ConfigureHistory()
    {
        _history.ReadOnly = true;
        _history.AllowUserToAddRows = false;
        _history.AllowUserToDeleteRows = false;
        _history.AutoGenerateColumns = false;
        _history.RowHeadersVisible = false;
        _history.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _history.BackgroundColor = SystemColors.Window;
        _history.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Datum", DataPropertyName = nameof(SkillAssessmentListItemDto.AssessedAtUtc), Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
        _history.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Level", DataPropertyName = nameof(SkillAssessmentListItemDto.Level), Width = 70 });
        _history.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Art", DataPropertyName = nameof(SkillAssessmentListItemDto.Type), Width = 130 });
        _history.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Begründung", DataPropertyName = nameof(SkillAssessmentListItemDto.Reason), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var detail = await _service.GetDetailAsync(_skillId).ConfigureAwait(true)
                ?? throw new KeyNotFoundException("Der Skill wurde nicht gefunden.");
            _skillName.Text = detail.Name;
            _current.Text = SkillLevelPresentation.Format(detail.CurrentLevel);
            if (detail.CurrentLevel is not null)
                _level.SelectedItem = ((IEnumerable<SkillLevelOption>)_level.DataSource!).First(x => x.Level == detail.CurrentLevel);
            _history.DataSource = (await _service.ListAssessmentsAsync(_skillId).ConfigureAwait(true)).ToList();
        }
        catch (Exception ex)
        {
            UiErrorHandler.Show(this, ex, _logger, "Skillbewertung laden");
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            _save.Enabled = false;
            var level = (_level.SelectedItem as SkillLevelOption)?.Level
                ?? throw new InvalidOperationException("Bitte ein Skill-Level auswählen.");
            var model = new SkillAssessmentModel(level, (SkillAssessmentType)_type.SelectedItem!, NullIfWhiteSpace(_reason.Text));
            await _service.AssessAsync(_skillId, model).ConfigureAwait(true);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { UiErrorHandler.Show(this, ex, _logger, "Skill bewerten"); }
        finally { _save.Enabled = true; }
    }

    private static string? NullIfWhiteSpace(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
