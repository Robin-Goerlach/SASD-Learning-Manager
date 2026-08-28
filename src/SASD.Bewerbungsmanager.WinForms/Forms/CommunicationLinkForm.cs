using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Controls;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Lets the user confirm or correct the job-search context of an imported communication.</summary>
public sealed class CommunicationLinkForm : Form
{
    private readonly ComboBox _opportunity = ChoiceBox();
    private readonly ComboBox _application = ChoiceBox();
    private readonly ComboBox _contact = ChoiceBox();
    private readonly ComboBox _organization = ChoiceBox();

    /// <summary>Creates the relation editor and preselects the currently inferred context.</summary>
    public CommunicationLinkForm(
        IReadOnlyList<Opportunity> opportunities,
        IReadOnlyList<JobApplication> applications,
        IReadOnlyList<Contact> contacts,
        IReadOnlyList<Organization> organizations,
        CommunicationMessage message)
    {
        Text = "Kommunikation zuordnen";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(700, 420);
        Size = new Size(760, 470);

        BindChoices(_opportunity, opportunities.Select(item => new ReferenceChoice(item.Id, item.Title)));
        BindChoices(_application, applications.Select(item => new ReferenceChoice(item.Id, $"{item.StartedAtUtc.LocalDateTime:d} — {item.Stage}")));
        BindChoices(_contact, contacts.Select(item => new ReferenceChoice(item.Id, item.FullName)));
        BindChoices(_organization, organizations.Select(item => new ReferenceChoice(item.Id, item.Name)));
        SelectChoice(_opportunity, message.OpportunityId);
        SelectChoice(_application, message.ApplicationId);
        SelectChoice(_contact, message.ContactId);
        SelectChoice(_organization, message.OrganizationId);
        BuildLayout();
    }

    /// <summary>Gets the user-confirmed relation values.</summary>
    public CommunicationLinkInput Input
        => new(SelectedId(_opportunity), SelectedId(_application), SelectedId(_contact), SelectedId(_organization));

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Stelle", _opportunity);
        ControlFactory.AddEditorRow(table, "Bewerbung", _application);
        ControlFactory.AddEditorRow(table, "Kontakt", _contact);
        ControlFactory.AddEditorRow(table, "Organisation", _organization);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var save = new Button { Text = "Übernehmen", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        table.Controls.Add(buttons, 1, table.RowCount++);
        AcceptButton = save;
        CancelButton = cancel;
        Controls.Add(table);
    }

    private static ComboBox ChoiceBox() => new() { DropDownStyle = ComboBoxStyle.DropDownList };

    private static void BindChoices(ComboBox comboBox, IEnumerable<ReferenceChoice> values)
    {
        var choices = new List<ReferenceChoice> { new(null, "— keine Zuordnung —") };
        choices.AddRange(values);
        comboBox.DataSource = choices;
        comboBox.DisplayMember = nameof(ReferenceChoice.Text);
    }

    private static Guid? SelectedId(ComboBox comboBox) => ((ReferenceChoice)comboBox.SelectedItem!).Id;

    private static void SelectChoice(ComboBox comboBox, Guid? id)
    {
        var choice = ((IEnumerable<ReferenceChoice>)comboBox.DataSource!).FirstOrDefault(item => item.Id == id);
        if (choice is not null)
        {
            comboBox.SelectedItem = choice;
        }
    }
}
