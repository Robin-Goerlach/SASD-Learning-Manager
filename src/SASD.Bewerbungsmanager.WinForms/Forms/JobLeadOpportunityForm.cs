using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Confirms optional organization relations before a discovered job becomes an opportunity.</summary>
public sealed class JobLeadOpportunityForm : Form
{
    private readonly ComboBox _employer = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _intermediary = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    /// <summary>Creates the promotion dialog for one selected job lead.</summary>
    public JobLeadOpportunityForm(JobLead lead, IReadOnlyList<Organization> organizations)
    {
        ArgumentNullException.ThrowIfNull(lead);
        Text = "Gefundenen Job als Stelle übernehmen";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(760, 430);
        Size = new Size(820, 500);

        PopulateOrganizations(_employer, organizations, "(Arbeitgeber später zuordnen)");
        PopulateOrganizations(_intermediary, organizations, "(kein Vermittler)");
        BuildLayout(lead);
    }

    /// <summary>Gets the confirmed promotion options.</summary>
    public JobLeadOpportunityInput Input
        => new(
            (_employer.SelectedItem as ReferenceChoice)?.Id,
            (_intermediary.SelectedItem as ReferenceChoice)?.Id);

    private void BuildLayout(JobLead lead)
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Position", ReadOnlyText(lead.Title));
        ControlFactory.AddEditorRow(table, "Quelle", ReadOnlyText(lead.SourceSystem));
        ControlFactory.AddEditorRow(table, "Quelle nennt", ReadOnlyText(lead.OrganizationName ?? string.Empty));
        ControlFactory.AddEditorRow(table, "Arbeitgeber", _employer);
        ControlFactory.AddEditorRow(table, "Vermittler", _intermediary);

        var note = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(600, 0),
            Text = "Der importierte Beschreibungstext wird als Rollenbeschreibung-Snapshot übernommen. " +
                   "Die Quell-URL wird als SourceLink gespeichert. Fehlende Details können anschließend in 'Stellen' ergänzt werden.",
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

    private static TextBox ReadOnlyText(string text) => new() { ReadOnly = true, Text = text };

    private static void PopulateOrganizations(ComboBox combo, IReadOnlyList<Organization> organizations, string emptyLabel)
    {
        combo.Items.Add(new ReferenceChoice(null, emptyLabel));
        foreach (var organization in organizations.Where(item => !item.IsArchived).OrderBy(item => item.Name))
        {
            combo.Items.Add(new ReferenceChoice(organization.Id, organization.Name));
        }
        combo.DisplayMember = nameof(ReferenceChoice.Text);
        combo.SelectedIndex = 0;
    }
}
