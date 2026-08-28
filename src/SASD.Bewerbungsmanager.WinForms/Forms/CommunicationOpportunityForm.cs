using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Captures the small amount of confirmation required to create an opportunity from communication text.</summary>
public sealed class CommunicationOpportunityForm : Form
{
    private readonly TextBox _title = new();
    private readonly ComboBox _sourceUrl = new() { DropDownStyle = ComboBoxStyle.DropDown };

    /// <summary>Creates the dialog from deterministic local text-analysis suggestions.</summary>
    public CommunicationOpportunityForm(CommunicationTextAnalysis analysis)
    {
        Text = "Stelle aus Kommunikation übernehmen";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(760, 360);
        Size = new Size(820, 410);
        _title.Text = analysis.SuggestedTitle;
        _sourceUrl.Items.AddRange(analysis.Urls.Cast<object>().ToArray());
        if (_sourceUrl.Items.Count > 0)
        {
            _sourceUrl.SelectedIndex = 0;
        }

        BuildLayout();
    }

    /// <summary>Gets the confirmed opportunity title.</summary>
    public string OpportunityTitle => _title.Text;

    /// <summary>Gets an optional source URL selected or entered by the user.</summary>
    public string? SourceUrl => string.IsNullOrWhiteSpace(_sourceUrl.Text) ? null : _sourceUrl.Text.Trim();

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Position *", _title);
        ControlFactory.AddEditorRow(table, "Quell-URL", _sourceUrl);

        var note = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(620, 0),
            Text = "Der Nachrichtentext wird unverändert als Rollenbeschreibung-Snapshot gespeichert. " +
                   "Arbeitgeber, Standort und weitere Details können anschließend in der Stellenansicht ergänzt werden.",
        };
        ControlFactory.AddEditorRow(table, "Hinweis", note);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var save = new Button { Text = "Stelle anlegen", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        table.Controls.Add(buttons, 1, table.RowCount++);
        AcceptButton = save;
        CancelButton = cancel;
        Controls.Add(table);
    }
}
