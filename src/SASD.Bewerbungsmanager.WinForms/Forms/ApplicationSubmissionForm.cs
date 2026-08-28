using SASD.Bewerbungsmanager.Application.Models;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>
/// Edits the factual submission metadata used by evidence reports. This is intentionally separate
/// from the workflow stage because a status can be corrected later without inventing a submission
/// date from the correction timestamp.
/// </summary>
public sealed class ApplicationSubmissionForm : Form
{
    private readonly CheckBox _submittedCheck = new() { Text = "Versanddatum erfasst", AutoSize = true };
    private readonly DateTimePicker _submitted = new() { Format = DateTimePickerFormat.Short };
    private readonly ComboBox _channel = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    /// <summary>Creates the editor initialized from the selected application.</summary>
    public ApplicationSubmissionForm(JobApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);
        Text = "Versanddaten bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(600, 300);

        _submittedCheck.Checked = application.SubmittedAtUtc is not null;
        _submitted.Value = application.SubmittedAtUtc?.LocalDateTime.Date ?? DateTime.Today;
        _submitted.Enabled = _submittedCheck.Checked;
        _submittedCheck.CheckedChanged += (_, _) => _submitted.Enabled = _submittedCheck.Checked;
        _channel.DataSource = Enum.GetValues<ApplicationChannel>();
        _channel.SelectedItem = application.Channel;
        BuildLayout();
    }

    /// <summary>Gets the corrected submission metadata.</summary>
    public ApplicationSubmissionInput Input
        => new(
            _submittedCheck.Checked ? ToUtc(_submitted.Value.Date) : null,
            (ApplicationChannel)_channel.SelectedItem!);

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Versand", _submittedCheck);
        ControlFactory.AddEditorRow(table, "Versanddatum", _submitted);
        ControlFactory.AddEditorRow(table, "Kanal", _channel);

        table.Controls.Add(new Label
        {
            Text = "Der Bewerbungsnachweis verwendet dieses Versanddatum. Ein leerer Wert entfernt die Bewerbung aus periodischen Nachweisen.",
            AutoSize = true,
            MaximumSize = new Size(430, 0),
            Margin = new Padding(0, 10, 0, 10),
        }, 1, table.RowCount++);

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
        };
        var save = new Button { Text = "Übernehmen", DialogResult = DialogResult.OK, AutoSize = true };
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
}
