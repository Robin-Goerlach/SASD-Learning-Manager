using SASD.Bewerbungsmanager.WinForms.Controls;
using TrackerDocument = SASD.Bewerbungsmanager.Domain.Entities.Document;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Selects one registered document version for immutable assignment to an application.</summary>
public sealed class ApplicationDocumentAttachForm : Form
{
    private readonly ComboBox _document = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    /// <summary>Creates the selection dialog using active document catalog entries.</summary>
    public ApplicationDocumentAttachForm(IReadOnlyList<TrackerDocument> documents)
    {
        Text = "Dokumentversion zuordnen";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(680, 220);
        _document.DataSource = documents
            .Select(item => new DocumentChoice(item.Id, $"{item.Type} — {item.Label} — {item.Version} — {item.Language}"))
            .ToList();
        _document.DisplayMember = nameof(DocumentChoice.Text);
        BuildLayout(documents.Count > 0);
    }

    /// <summary>Gets the selected catalog document identifier.</summary>
    public Guid SelectedDocumentId => ((DocumentChoice)_document.SelectedItem!).Id;

    private void BuildLayout(bool hasDocuments)
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Dokument", _document);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var save = new Button { Text = "Zuordnen", DialogResult = DialogResult.OK, AutoSize = true, Enabled = hasDocuments };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        table.Controls.Add(buttons, 1, table.RowCount++);
        AcceptButton = save;
        CancelButton = cancel;
        Controls.Add(table);
    }

    private sealed record DocumentChoice(Guid Id, string Text);
}
