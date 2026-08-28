using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Editor used when a real application is created from an existing opportunity.</summary>
public sealed class ApplicationEditForm : Form
{
    private readonly ComboBox _opportunity = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _started = new() { Format = DateTimePickerFormat.Short };
    private readonly CheckBox _submittedCheck = new() { Text = "bereits versendet", AutoSize = true };
    private readonly DateTimePicker _submitted = new() { Format = DateTimePickerFormat.Short, Enabled = false };
    private readonly ComboBox _stage = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _channel = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _salary = new();

    /// <summary>Creates an application editor for the current opportunities.</summary>
    public ApplicationEditForm(IReadOnlyList<Opportunity> opportunities)
    {
        Text = "Bewerbung anlegen";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(700, 440);

        _opportunity.DataSource = opportunities.Select(item => new OpportunityChoice(item.Id, item.Title)).ToList();
        _opportunity.DisplayMember = nameof(OpportunityChoice.Title);
        _stage.DataSource = Enum.GetValues<ApplicationStage>();
        _channel.DataSource = Enum.GetValues<ApplicationChannel>();
        _submittedCheck.CheckedChanged += (_, _) => _submitted.Enabled = _submittedCheck.Checked;
        BuildLayout();
    }

    /// <summary>Gets the current editor values as application input.</summary>
    public ApplicationInput Input
    {
        get
        {
            var choice = (OpportunityChoice)_opportunity.SelectedItem!;
            var started = ToUtc(_started.Value.Date);
            DateTimeOffset? submitted = _submittedCheck.Checked ? ToUtc(_submitted.Value.Date) : null;
            return new ApplicationInput(
                choice.Id,
                started,
                submitted,
                (ApplicationStage)_stage.SelectedItem!,
                (ApplicationChannel)_channel.SelectedItem!,
                _salary.Text);
        }
    }

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Stelle *", _opportunity);
        ControlFactory.AddEditorRow(table, "Gestartet", _started);
        ControlFactory.AddEditorRow(table, "Versand", _submittedCheck);
        ControlFactory.AddEditorRow(table, "Versanddatum", _submitted);
        ControlFactory.AddEditorRow(table, "Status", _stage);
        ControlFactory.AddEditorRow(table, "Kanal", _channel);
        ControlFactory.AddEditorRow(table, "Gehaltsvorstellung", _salary);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var save = new Button { Text = "Speichern", DialogResult = DialogResult.OK, AutoSize = true, Enabled = _opportunity.Items.Count > 0 };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        table.Controls.Add(buttons, 1, table.RowCount++);
        AcceptButton = save;
        CancelButton = cancel;
        Controls.Add(table);
    }

    private static DateTimeOffset ToUtc(DateTime date)
        => new DateTimeOffset(date, TimeZoneInfo.Local.GetUtcOffset(date)).ToUniversalTime();

    private sealed record OpportunityChoice(Guid Id, string Title);
}
