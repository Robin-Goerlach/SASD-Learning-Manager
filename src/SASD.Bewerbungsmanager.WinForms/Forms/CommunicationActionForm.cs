using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Small dialog for creating an ACTION directly from an imported communication.</summary>
public sealed class CommunicationActionForm : Form
{
    private readonly TextBox _title = new();
    private readonly CheckBox _hasDue = new() { Text = "Fälligkeit verwenden", AutoSize = true };
    private readonly DateTimePicker _due = new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd.MM.yyyy HH:mm",
        Value = DateTime.Now.AddDays(1),
        Enabled = false,
    };

    /// <summary>Creates the action dialog with a useful default title derived from the message subject.</summary>
    public CommunicationActionForm(string subject)
    {
        Text = "ACTION aus Kommunikation";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(700, 330);
        Size = new Size(760, 380);
        var defaultTitle = $"Auf Nachricht reagieren: {subject}";
        _title.Text = defaultTitle.Length <= 250 ? defaultTitle : defaultTitle[..250];
        _hasDue.CheckedChanged += (_, _) => _due.Enabled = _hasDue.Checked;
        BuildLayout();
    }

    /// <summary>Gets the action title.</summary>
    public string ActionTitle => _title.Text;

    /// <summary>Gets the optional due time converted to UTC.</summary>
    public DateTimeOffset? DueAtUtc => _hasDue.Checked ? ToUtc(_due.Value) : null;

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "ACTION *", _title);
        ControlFactory.AddEditorRow(table, "Fälligkeit", _hasDue);
        ControlFactory.AddEditorRow(table, "Fällig am", _due);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var save = new Button { Text = "Erzeugen", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        table.Controls.Add(buttons, 1, table.RowCount++);
        AcceptButton = save;
        CancelButton = cancel;
        Controls.Add(table);
    }

    private static DateTimeOffset ToUtc(DateTime localDateTime)
        => new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime)).ToUniversalTime();
}
