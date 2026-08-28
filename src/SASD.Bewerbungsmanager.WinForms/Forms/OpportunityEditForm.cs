using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Modal editor for an opportunity and its role-description snapshot.</summary>
public sealed class OpportunityEditForm : Form
{
    private readonly ComboBox _employer = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _intermediary = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _title = new();
    private readonly TextBox _location = new();
    private readonly TextBox _remote = new();
    private readonly TextBox _salary = new();
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _foundAt = new() { Format = DateTimePickerFormat.Short };
    private readonly TextBox _description = new() { Multiline = true, Height = 220, ScrollBars = ScrollBars.Both, AcceptsReturn = true };

    /// <summary>Creates an opportunity editor using the supplied organization choices.</summary>
    public OpportunityEditForm(IReadOnlyList<Organization> organizations, Opportunity? opportunity = null)
    {
        Text = opportunity is null ? "Stelle anlegen" : "Stelle bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(760, 680);
        Size = new Size(820, 740);

        var choices = new List<OrganizationChoice> { new(null, "— keine Zuordnung —") };
        choices.AddRange(organizations.Select(item => new OrganizationChoice(item.Id, item.Name)));
        _employer.DataSource = choices.ToList();
        _employer.DisplayMember = nameof(OrganizationChoice.Name);
        _intermediary.DataSource = choices.ToList();
        _intermediary.DisplayMember = nameof(OrganizationChoice.Name);
        _status.DataSource = Enum.GetValues<OpportunityStatus>();
        _foundAt.Value = DateTime.Today;
        BuildLayout();

        if (opportunity is not null)
        {
            _employer.SelectedItem = ((IEnumerable<OrganizationChoice>)_employer.DataSource!).First(item => item.Id == opportunity.EmployerOrganizationId);
            _intermediary.SelectedItem = ((IEnumerable<OrganizationChoice>)_intermediary.DataSource!).First(item => item.Id == opportunity.IntermediaryOrganizationId);
            _title.Text = opportunity.Title;
            _location.Text = opportunity.Location ?? string.Empty;
            _remote.Text = opportunity.RemoteText ?? string.Empty;
            _salary.Text = opportunity.SalaryText ?? string.Empty;
            _status.SelectedItem = opportunity.Status;
            _foundAt.Value = opportunity.FoundAtUtc.LocalDateTime.Date;
            _description.Text = opportunity.DescriptionSnapshot;
        }
    }

    /// <summary>Gets the current editor values as application input.</summary>
    public OpportunityInput Input
    {
        get
        {
            var employer = (OrganizationChoice)_employer.SelectedItem!;
            var intermediary = (OrganizationChoice)_intermediary.SelectedItem!;
            var foundAt = new DateTimeOffset(_foundAt.Value.Date, TimeZoneInfo.Local.GetUtcOffset(_foundAt.Value.Date)).ToUniversalTime();
            return new OpportunityInput(
                employer.Id,
                intermediary.Id,
                _title.Text,
                _description.Text,
                _location.Text,
                _remote.Text,
                _salary.Text,
                (OpportunityStatus)_status.SelectedItem!,
                foundAt,
                null,
                null);
        }
    }

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Position *", _title);
        ControlFactory.AddEditorRow(table, "Arbeitgeber", _employer);
        ControlFactory.AddEditorRow(table, "Vermittler", _intermediary);
        ControlFactory.AddEditorRow(table, "Status", _status);
        ControlFactory.AddEditorRow(table, "Gefunden", _foundAt);
        ControlFactory.AddEditorRow(table, "Standort", _location);
        ControlFactory.AddEditorRow(table, "Remote / Hybrid", _remote);
        ControlFactory.AddEditorRow(table, "Gehalt", _salary);
        ControlFactory.AddEditorRow(table, "Rollenbeschreibung *", _description);

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

    private sealed record OrganizationChoice(Guid? Id, string Name);
}
