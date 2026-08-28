using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Captures normalized communication text from the Windows clipboard or manual paste.</summary>
public sealed class CommunicationClipboardImportForm : Form
{
    private readonly ComboBox _direction = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _kind = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _fromName = new();
    private readonly TextBox _fromAddress = new();
    private readonly TextBox _subject = new();
    private readonly TextBox _body = new() { Multiline = true, ScrollBars = ScrollBars.Both, Height = 260, AcceptsReturn = true };
    private readonly DateTimePicker _messageAt = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy HH:mm", Value = DateTime.Now };

    /// <summary>Creates a clipboard import dialog and pre-fills the body with current clipboard text when available.</summary>
    public CommunicationClipboardImportForm()
    {
        Text = "Kommunikation aus Zwischenablage";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(820, 650);
        Size = new Size(900, 720);
        _direction.DataSource = Enum.GetValues<CommunicationDirection>();
        _kind.DataSource = Enum.GetValues<CommunicationKind>();
        _kind.SelectedItem = CommunicationKind.Unclassified;
        if (Clipboard.ContainsText())
        {
            _body.Text = Clipboard.GetText();
        }

        BuildLayout();
    }

    /// <summary>Gets the entered values as normalized application-layer input.</summary>
    public CommunicationImportInput Input
        => new(
            SourceSystem: "Clipboard",
            ExternalMessageId: null,
            Direction: (CommunicationDirection)_direction.SelectedItem!,
            Kind: (CommunicationKind)_kind.SelectedItem!,
            FromName: _fromName.Text,
            FromAddress: _fromAddress.Text,
            ToAddresses: null,
            Subject: _subject.Text,
            BodyText: _body.Text,
            MessageAtUtc: ToUtc(_messageAt.Value),
            SourceReference: "Windows-Zwischenablage");

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Richtung", _direction);
        ControlFactory.AddEditorRow(table, "Einordnung", _kind);
        ControlFactory.AddEditorRow(table, "Absendername", _fromName);
        ControlFactory.AddEditorRow(table, "Absender E-Mail", _fromAddress);
        ControlFactory.AddEditorRow(table, "Zeitpunkt", _messageAt);
        ControlFactory.AddEditorRow(table, "Betreff *", _subject);
        ControlFactory.AddEditorRow(table, "Text", _body);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var save = new Button { Text = "Importieren", DialogResult = DialogResult.OK, AutoSize = true };
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
