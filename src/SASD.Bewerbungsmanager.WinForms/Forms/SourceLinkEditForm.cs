using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Small editor for an external opportunity source.</summary>
public sealed class SourceLinkEditForm : Form
{
    private readonly TextBox _source = new();
    private readonly TextBox _url = new();
    private readonly TextBox _externalId = new();

    /// <summary>Creates an empty source-link editor.</summary>
    public SourceLinkEditForm()
    {
        Text = "Quelle hinzufügen";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(650, 270);
        BuildLayout();
    }

    /// <summary>Gets the current editor values as application input.</summary>
    public SourceLinkInput Input => new(_source.Text, _url.Text, _externalId.Text);

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Quelle *", _source);
        ControlFactory.AddEditorRow(table, "URL *", _url);
        ControlFactory.AddEditorRow(table, "Externe ID", _externalId);
        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var save = new Button { Text = "Speichern", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        table.Controls.Add(buttons, 1, table.RowCount++);
        AcceptButton = save;
        CancelButton = cancel;
        Controls.Add(table);
    }
}
