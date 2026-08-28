using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Modal editor for one organization.</summary>
public sealed class OrganizationEditForm : Form
{
    private readonly TextBox _name = new();
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _website = new();
    private readonly TextBox _notes = new() { Multiline = true, Height = 100, ScrollBars = ScrollBars.Vertical };

    /// <summary>Creates an empty organization editor.</summary>
    public OrganizationEditForm(Organization? organization = null)
    {
        Text = organization is null ? "Organisation anlegen" : "Organisation bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        MinimumSize = new Size(600, 420);
        Size = new Size(650, 460);
        _type.DataSource = Enum.GetValues<OrganizationType>();
        BuildLayout();

        if (organization is not null)
        {
            _name.Text = organization.Name;
            _type.SelectedItem = organization.Type;
            _website.Text = organization.Website ?? string.Empty;
            _notes.Text = organization.Notes ?? string.Empty;
        }
    }

    /// <summary>Gets the current editor values as application input.</summary>
    public OrganizationInput Input => new(_name.Text, (OrganizationType)_type.SelectedItem!, _website.Text, _notes.Text);

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Name *", _name);
        ControlFactory.AddEditorRow(table, "Typ", _type);
        ControlFactory.AddEditorRow(table, "Website", _website);
        ControlFactory.AddEditorRow(table, "Notizen", _notes);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8) };
        buttons.Controls.Add(new Button { Text = "Speichern", DialogResult = DialogResult.OK, AutoSize = true });
        buttons.Controls.Add(new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true });
        AcceptButton = (IButtonControl)buttons.Controls[0];
        CancelButton = (IButtonControl)buttons.Controls[1];
        Controls.Add(table);
        Controls.Add(buttons);
    }
}
