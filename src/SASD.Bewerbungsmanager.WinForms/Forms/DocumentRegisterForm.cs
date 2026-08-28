using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Modal editor for registering an existing file as a known document version.</summary>
public sealed class DocumentRegisterForm : Form
{
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _label = new();
    private readonly TextBox _version = new();
    private readonly TextBox _language = new() { Text = "DE" };
    private readonly TextBox _tags = new();
    private readonly TextBox _path = new();

    /// <summary>Creates the document-registration dialog.</summary>
    public DocumentRegisterForm()
    {
        Text = "Dokumentversion registrieren";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 430);
        _type.DataSource = Enum.GetValues<DocumentType>();
        BuildLayout();
    }

    /// <summary>Gets the values entered by the user.</summary>
    public DocumentInput Input
        => new((DocumentType)_type.SelectedItem!, _label.Text, _version.Text, _language.Text, _tags.Text, _path.Text);

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Typ", _type);
        ControlFactory.AddEditorRow(table, "Bezeichnung *", _label);
        ControlFactory.AddEditorRow(table, "Version *", _version);
        ControlFactory.AddEditorRow(table, "Sprache *", _language);
        ControlFactory.AddEditorRow(table, "Tags", _tags);

        var pathPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
        pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        pathPanel.Controls.Add(_path, 0, 0);
        var browse = new Button { Text = "…", AutoSize = true };
        browse.Click += (_, _) => Browse();
        pathPanel.Controls.Add(browse, 1, 0);
        ControlFactory.AddEditorRow(table, "Datei *", pathPanel);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var save = new Button { Text = "Registrieren", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        table.Controls.Add(buttons, 1, table.RowCount++);
        AcceptButton = save;
        CancelButton = cancel;
        Controls.Add(table);
    }

    private void Browse()
    {
        using var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Multiselect = false,
            Title = "Dokument auswählen",
            Filter = "Dokumente|*.pdf;*.doc;*.docx;*.odt;*.txt;*.rtf|Alle Dateien|*.*",
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _path.Text = dialog.FileName;
        }
    }
}
