using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Controls;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Modal editor for ACTION and WAITING_FOR next-step items.</summary>
public sealed class WorkItemEditForm : Form
{
    private readonly ComboBox _kind = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _title = new();
    private readonly TextBox _notes = new() { Multiline = true, Height = 100, ScrollBars = ScrollBars.Vertical };
    private readonly CheckBox _hasDue = new() { Text = "Fälligkeit verwenden", AutoSize = true };
    private readonly DateTimePicker _due = new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd.MM.yyyy HH:mm",
        Value = DateTime.Now.AddDays(1),
        Enabled = false,
    };
    private readonly ComboBox _opportunity = ChoiceBox();
    private readonly ComboBox _application = ChoiceBox();
    private readonly ComboBox _contact = ChoiceBox();
    private readonly ComboBox _organization = ChoiceBox();

    /// <summary>Creates a work-item editor with optional relation choices.</summary>
    public WorkItemEditForm(
        IReadOnlyList<Opportunity> opportunities,
        IReadOnlyList<JobApplication> applications,
        IReadOnlyList<Contact> contacts,
        IReadOnlyList<Organization> organizations,
        Guid? preselectedApplicationId = null)
    {
        Text = "Aufgabe / Warten auf erfassen";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(760, 590);
        Size = new Size(800, 630);

        _kind.DataSource = Enum.GetValues<WorkItemKind>();
        BindChoices(_opportunity, opportunities.Select(item => new ReferenceChoice(item.Id, item.Title)));
        BindChoices(_application, applications.Select(item => new ReferenceChoice(item.Id, $"{item.StartedAtUtc.LocalDateTime:d} — {item.Stage}")));
        BindChoices(_contact, contacts.Select(item => new ReferenceChoice(item.Id, item.FullName)));
        BindChoices(_organization, organizations.Select(item => new ReferenceChoice(item.Id, item.Name)));
        if (preselectedApplicationId is not null)
        {
            SelectChoice(_application, preselectedApplicationId);
        }

        _hasDue.CheckedChanged += (_, _) => _due.Enabled = _hasDue.Checked;
        BuildLayout();
    }

    /// <summary>Gets the current editor values as application-layer input.</summary>
    public WorkItemInput Input
        => new(
            SelectedId(_opportunity),
            SelectedId(_application),
            SelectedId(_contact),
            SelectedId(_organization),
            (WorkItemKind)_kind.SelectedItem!,
            _title.Text,
            _notes.Text,
            _hasDue.Checked ? ToUtc(_due.Value) : null);

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Typ", _kind);
        ControlFactory.AddEditorRow(table, "Aufgabe / Erwartung *", _title);
        ControlFactory.AddEditorRow(table, "Fälligkeit", _hasDue);
        ControlFactory.AddEditorRow(table, "Fällig am", _due);
        ControlFactory.AddEditorRow(table, "Stelle", _opportunity);
        ControlFactory.AddEditorRow(table, "Bewerbung", _application);
        ControlFactory.AddEditorRow(table, "Kontakt", _contact);
        ControlFactory.AddEditorRow(table, "Organisation", _organization);
        ControlFactory.AddEditorRow(table, "Notizen", _notes);

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

    private static ComboBox ChoiceBox() => new() { DropDownStyle = ComboBoxStyle.DropDownList };

    private static void BindChoices(ComboBox comboBox, IEnumerable<ReferenceChoice> values)
    {
        var choices = new List<ReferenceChoice> { new(null, "— keine Zuordnung —") };
        choices.AddRange(values);
        comboBox.DataSource = choices;
        comboBox.DisplayMember = nameof(ReferenceChoice.Text);
    }

    private static Guid? SelectedId(ComboBox comboBox)
        => ((ReferenceChoice)comboBox.SelectedItem!).Id;

    private static void SelectChoice(ComboBox comboBox, Guid? id)
    {
        var choice = ((IEnumerable<ReferenceChoice>)comboBox.DataSource!).FirstOrDefault(item => item.Id == id);
        if (choice is not null)
        {
            comboBox.SelectedItem = choice;
        }
    }

    private static DateTimeOffset ToUtc(DateTime localDateTime)
        => new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime)).ToUniversalTime();
}
