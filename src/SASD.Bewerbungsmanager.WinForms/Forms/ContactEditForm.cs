using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Modal editor for a professional contact.</summary>
public sealed class ContactEditForm : Form
{
    private readonly ComboBox _organization = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _name = new();
    private readonly TextBox _role = new();
    private readonly TextBox _email = new();
    private readonly TextBox _phone = new();
    private readonly TextBox _linkedin = new();
    private readonly TextBox _notes = new() { Multiline = true, Height = 90, ScrollBars = ScrollBars.Vertical };

    /// <summary>Creates a contact editor using the supplied organization choices.</summary>
    public ContactEditForm(IReadOnlyList<Organization> organizations, Contact? contact = null)
    {
        Text = contact is null ? "Kontakt anlegen" : "Kontakt bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(650, 540);
        Size = new Size(700, 580);

        var options = new List<OrganizationChoice> { new(null, "— keine Zuordnung —") };
        options.AddRange(organizations.Select(item => new OrganizationChoice(item.Id, item.Name)));
        _organization.DataSource = options;
        _organization.DisplayMember = nameof(OrganizationChoice.Name);

        BuildLayout();
        if (contact is not null)
        {
            _organization.SelectedItem = options.FirstOrDefault(item => item.Id == contact.OrganizationId) ?? options[0];
            _name.Text = contact.FullName;
            _role.Text = contact.Role ?? string.Empty;
            _email.Text = contact.Email ?? string.Empty;
            _phone.Text = contact.Phone ?? string.Empty;
            _linkedin.Text = contact.LinkedInUrl ?? string.Empty;
            _notes.Text = contact.Notes ?? string.Empty;
        }
    }

    /// <summary>Gets the current editor values as application input.</summary>
    public ContactInput Input
    {
        get
        {
            var choice = (OrganizationChoice)_organization.SelectedItem!;
            return new ContactInput(choice.Id, _name.Text, _role.Text, _email.Text, _phone.Text, _linkedin.Text, _notes.Text);
        }
    }

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Organisation", _organization);
        ControlFactory.AddEditorRow(table, "Name *", _name);
        ControlFactory.AddEditorRow(table, "Rolle", _role);
        ControlFactory.AddEditorRow(table, "E-Mail", _email);
        ControlFactory.AddEditorRow(table, "Telefon", _phone);
        ControlFactory.AddEditorRow(table, "LinkedIn", _linkedin);
        ControlFactory.AddEditorRow(table, "Notizen", _notes);
        AddButtons(table);
        Controls.Add(table);
    }

    private void AddButtons(TableLayoutPanel table)
    {
        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var save = new Button { Text = "Speichern", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        table.Controls.Add(buttons, 1, table.RowCount++);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private sealed record OrganizationChoice(Guid? Id, string Name);
}
