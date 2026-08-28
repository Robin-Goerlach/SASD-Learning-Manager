using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Goals;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.Goals;
using SASD.LearningManager.Domain.Skills;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Goal editor including target date, next action and linked skill requirements.</summary>
public sealed class GoalEditForm : Form
{
    private readonly GoalService _goalService;
    private readonly SkillService _skillService;
    private readonly ILogger _logger;
    private readonly Guid? _goalId;
    private readonly TextBox _title = new() { MaxLength = 500 };
    private readonly TextBox _description = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = 20000 };
    private readonly TextBox _motivation = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = 10000 };
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _priority = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _targetDate = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly TextBox _nextAction = new() { MaxLength = 1000 };
    private readonly DateTimePicker _nextActionDue = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly CheckedListBox _skills = new() { CheckOnClick = true, IntegralHeight = false };
    private readonly Button _save = new() { Text = "Speichern", AutoSize = true };

    public GoalEditForm(GoalService goalService, SkillService skillService, ILogger logger, Guid? goalId = null)
    {
        _goalService = goalService;
        _skillService = skillService;
        _logger = logger;
        _goalId = goalId;
        Text = goalId is null ? "Lernziel anlegen" : "Lernziel bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(820, 700);
        Size = new Size(920, 800);
        AutoScaleMode = AutoScaleMode.Dpi;
        _type.DataSource = Enum.GetValues<GoalType>();
        _priority.DataSource = Enum.GetValues<GoalPriority>();
        _status.DataSource = Enum.GetValues<GoalStatus>().Where(static x => x != GoalStatus.Archived).ToArray();
        _priority.SelectedItem = GoalPriority.Normal;
        _status.SelectedItem = GoalStatus.Planned;
        _targetDate.Checked = false;
        _nextActionDue.Checked = false;
        Controls.Add(BuildLayout());
        AcceptButton = _save;
        Load += async (_, _) => await LoadDataAsync().ConfigureAwait(true);
        _save.Click += async (_, _) => await SaveAsync().ConfigureAwait(true);
    }

    private Control BuildLayout()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 11, AutoScroll = true };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        Add(panel, 0, "Titel *", _title);
        Add(panel, 1, "Typ", _type);
        Add(panel, 2, "Priorität", _priority);
        Add(panel, 3, "Status", _status);
        Add(panel, 4, "Zieldatum", _targetDate);
        Add(panel, 5, "Beschreibung", _description);
        Add(panel, 6, "Motivation", _motivation);
        Add(panel, 7, "Nächste Aktion", _nextAction);
        Add(panel, 8, "Aktion fällig", _nextActionDue);
        Add(panel, 9, "Benötigte Skills", _skills);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_save);
        panel.Controls.Add(buttons, 1, 10);
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
            var skills = await _skillService.ListLookupAsync(includeArchived: _goalId is not null).ConfigureAwait(true);
            var options = skills.Select(static x => new SkillOption(x.Id,
                x.Status == SkillStatus.Archived ? $"{x.Name} [Archiviert]" : $"{x.Name}  ({SkillLevelPresentation.Format(x.CurrentLevel)} → Ziel {x.TargetLevel?.ToString() ?? "–"})")).ToArray();
            _skills.DataSource = options;
            _skills.DisplayMember = nameof(SkillOption.Text);
            if (_goalId is null) return;

            var detail = await _goalService.GetDetailAsync(_goalId.Value).ConfigureAwait(true)
                ?? throw new KeyNotFoundException("Das Lernziel wurde nicht gefunden.");
            if (detail.Status == GoalStatus.Archived) throw new InvalidOperationException("Archivierte Ziele müssen zuerst wiederhergestellt werden.");
            _title.Text = detail.Title;
            _description.Text = detail.Description ?? string.Empty;
            _motivation.Text = detail.Motivation ?? string.Empty;
            _type.SelectedItem = detail.Type;
            _priority.SelectedItem = detail.Priority;
            _status.SelectedItem = detail.Status;
            SetDate(_targetDate, detail.TargetDate);
            _nextAction.Text = detail.NextActionText ?? string.Empty;
            SetDate(_nextActionDue, detail.NextActionDueDate);
            for (var index = 0; index < options.Length; index++)
                if (detail.SkillIds.Contains(options[index].Id)) _skills.SetItemChecked(index, true);
        }
        catch (Exception ex)
        {
            UiErrorHandler.Show(this, ex, _logger, "Lernziel laden");
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            _save.Enabled = false;
            var skillIds = _skills.CheckedItems.Cast<SkillOption>().Select(static x => x.Id).ToArray();
            var model = new GoalEditModel(_title.Text, NullIfWhiteSpace(_description.Text), (GoalType)_type.SelectedItem!,
                NullIfWhiteSpace(_motivation.Text), (GoalPriority)_priority.SelectedItem!, (GoalStatus)_status.SelectedItem!,
                GetDate(_targetDate), NullIfWhiteSpace(_nextAction.Text), GetDate(_nextActionDue), skillIds);
            if (_goalId is null) await _goalService.CreateAsync(model).ConfigureAwait(true);
            else await _goalService.UpdateAsync(_goalId.Value, model).ConfigureAwait(true);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { UiErrorHandler.Show(this, ex, _logger, "Lernziel speichern"); }
        finally { _save.Enabled = true; }
    }

    private static DateOnly? GetDate(DateTimePicker picker) => picker.Checked ? DateOnly.FromDateTime(picker.Value.Date) : null;
    private static void SetDate(DateTimePicker picker, DateOnly? date)
    {
        picker.Checked = date is not null;
        if (date is not null) picker.Value = date.Value.ToDateTime(TimeOnly.MinValue);
    }
    private static string? NullIfWhiteSpace(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private sealed record SkillOption(Guid Id, string Text);
}
