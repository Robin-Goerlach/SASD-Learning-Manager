using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Goals;
using SASD.LearningManager.Application.LearningPaths;
using SASD.LearningManager.Domain.Goals;
using SASD.LearningManager.Domain.LearningPaths;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Edits learning-path metadata and optional Goal relationships.</summary>
public sealed class LearningPathEditForm : Form
{
    private readonly LearningPathService _pathService;
    private readonly GoalService _goalService;
    private readonly ILogger _logger;
    private readonly Guid? _pathId;
    private readonly TextBox _title = new() { MaxLength = 500 };
    private readonly TextBox _description = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = 20000 };
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _priority = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _plannedStart = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly DateTimePicker _targetDate = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly TextBox _nextAction = new() { MaxLength = 1000 };
    private readonly DateTimePicker _nextActionDue = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly CheckedListBox _goals = new() { CheckOnClick = true, IntegralHeight = false };
    private readonly Button _save = new() { Text = "Speichern", AutoSize = true };

    public LearningPathEditForm(LearningPathService pathService, GoalService goalService, ILogger logger, Guid? pathId = null)
    {
        _pathService = pathService;
        _goalService = goalService;
        _logger = logger;
        _pathId = pathId;
        Text = pathId is null ? "Learning Path anlegen" : "Learning Path bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(800, 650);
        Size = new Size(900, 760);
        AutoScaleMode = AutoScaleMode.Dpi;
        _status.DataSource = Enum.GetValues<LearningPathStatus>().Where(static value => value != LearningPathStatus.Archived).ToArray();
        _priority.DataSource = Enum.GetValues<LearningPathPriority>();
        _status.SelectedItem = LearningPathStatus.Planned;
        _priority.SelectedItem = LearningPathPriority.Normal;
        _plannedStart.Checked = false;
        _targetDate.Checked = false;
        _nextActionDue.Checked = false;
        Controls.Add(BuildLayout());
        AcceptButton = _save;
        Load += async (_, _) => await LoadDataAsync().ConfigureAwait(true);
        _save.Click += async (_, _) => await SaveAsync().ConfigureAwait(true);
    }

    private Control BuildLayout()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 10, AutoScroll = true };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        Add(panel, 0, "Titel *", _title);
        Add(panel, 1, "Status", _status);
        Add(panel, 2, "Priorität", _priority);
        Add(panel, 3, "Geplanter Start", _plannedStart);
        Add(panel, 4, "Zieldatum", _targetDate);
        Add(panel, 5, "Beschreibung", _description);
        Add(panel, 6, "Nächste Aktion", _nextAction);
        Add(panel, 7, "Aktion fällig", _nextActionDue);
        Add(panel, 8, "Zugehörige Ziele", _goals);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_save);
        panel.Controls.Add(buttons, 1, 9);
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
            var goals = await _goalService.ListLookupAsync(includeArchived: _pathId is not null).ConfigureAwait(true);
            var options = goals.Select(static goal => new GoalOption(goal.Id,
                goal.Status == GoalStatus.Archived ? $"{goal.Title} [Archiviert]" : goal.Title)).ToArray();
            _goals.DataSource = options;
            _goals.DisplayMember = nameof(GoalOption.Text);
            if (_pathId is null)
            {
                return;
            }

            var detail = await _pathService.GetDetailAsync(_pathId.Value).ConfigureAwait(true)
                ?? throw new KeyNotFoundException("Der Learning Path wurde nicht gefunden.");
            if (detail.Status == LearningPathStatus.Archived)
            {
                throw new InvalidOperationException("Archivierte Learning Paths müssen zuerst wiederhergestellt werden.");
            }

            _title.Text = detail.Title;
            _description.Text = detail.Description ?? string.Empty;
            _status.SelectedItem = detail.Status;
            _priority.SelectedItem = detail.Priority;
            SetDate(_plannedStart, detail.PlannedStartDate);
            SetDate(_targetDate, detail.TargetDate);
            _nextAction.Text = detail.NextActionText ?? string.Empty;
            SetDate(_nextActionDue, detail.NextActionDueDate);
            for (var index = 0; index < options.Length; index++)
            {
                if (detail.GoalIds.Contains(options[index].Id))
                {
                    _goals.SetItemChecked(index, true);
                }
            }
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Learning Path laden");
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            _save.Enabled = false;
            var goalIds = _goals.CheckedItems.Cast<GoalOption>().Select(static item => item.Id).ToArray();
            var model = new LearningPathEditModel(_title.Text, NullIfWhiteSpace(_description.Text),
                (LearningPathStatus)_status.SelectedItem!, (LearningPathPriority)_priority.SelectedItem!,
                GetDate(_plannedStart), GetDate(_targetDate), NullIfWhiteSpace(_nextAction.Text), GetDate(_nextActionDue), goalIds);
            if (_pathId is null)
            {
                await _pathService.CreateAsync(model).ConfigureAwait(true);
            }
            else
            {
                await _pathService.UpdateAsync(_pathId.Value, model).ConfigureAwait(true);
            }
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Learning Path speichern");
        }
        finally
        {
            _save.Enabled = true;
        }
    }

    private static DateOnly? GetDate(DateTimePicker picker) => picker.Checked ? DateOnly.FromDateTime(picker.Value.Date) : null;
    private static void SetDate(DateTimePicker picker, DateOnly? date)
    {
        picker.Checked = date is not null;
        if (date is not null) picker.Value = date.Value.ToDateTime(TimeOnly.MinValue);
    }
    private static string? NullIfWhiteSpace(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private sealed record GoalOption(Guid Id, string Text);
}
