using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Controls;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Modal editor for a historical timeline entry or a planned appointment.</summary>
public sealed class ActivityEditForm : Form
{
    private readonly ComboBox _kind = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _subject = new();
    private readonly TextBox _notes = new() { Multiline = true, Height = 100, ScrollBars = ScrollBars.Vertical };
    private readonly CheckBox _planned = new() { Text = "geplanter Termin", AutoSize = true };
    private readonly DateTimePicker _when = DateTimeEditor();
    private readonly ComboBox _opportunity = ChoiceBox();
    private readonly ComboBox _application = ChoiceBox();
    private readonly ComboBox _contact = ChoiceBox();
    private readonly ComboBox _organization = ChoiceBox();

    /// <summary>Creates an editor with the current optional reference choices.</summary>
    public ActivityEditForm(
        IReadOnlyList<Opportunity> opportunities,
        IReadOnlyList<JobApplication> applications,
        IReadOnlyList<Contact> contacts,
        IReadOnlyList<Organization> organizations,
        Guid? preselectedApplicationId = null,
        bool plannedByDefault = false)
    {
        Text = "Aktivität / Termin erfassen";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(760, 600);
        Size = new Size(800, 640);

        _kind.DataSource = Enum.GetValues<ActivityKind>();
        _planned.Checked = plannedByDefault;
        BindChoices(_opportunity, opportunities.Select(item => new ReferenceChoice(item.Id, item.Title)));
        BindChoices(_application, applications.Select(item => new ReferenceChoice(item.Id, $"{item.StartedAtUtc.LocalDateTime:d} — {item.Stage}")));
        BindChoices(_contact, contacts.Select(item => new ReferenceChoice(item.Id, item.FullName)));
        BindChoices(_organization, organizations.Select(item => new ReferenceChoice(item.Id, item.Name)));

        if (preselectedApplicationId is not null)
        {
            SelectChoice(_application, preselectedApplicationId);
        }

        _planned.CheckedChanged += (_, _) => Text = _planned.Checked ? "Termin erfassen" : "Aktivität erfassen";
        BuildLayout();
    }

    /// <summary>Gets the editor values as application-layer input.</summary>
    public ActivityInput Input
    {
        get
        {
            var timestamp = ToUtc(_when.Value);
            return new ActivityInput(
                SelectedId(_opportunity),
                SelectedId(_application),
                SelectedId(_contact),
                SelectedId(_organization),
                (ActivityKind)_kind.SelectedItem!,
                _planned.Checked ? ActivityStatus.Planned : ActivityStatus.Recorded,
                _subject.Text,
                _notes.Text,
                _planned.Checked ? null : timestamp,
                _planned.Checked ? timestamp : null);
        }
    }

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Art", _kind);
        ControlFactory.AddEditorRow(table, "Betreff *", _subject);
        ControlFactory.AddEditorRow(table, "Planung", _planned);
        ControlFactory.AddEditorRow(table, "Zeitpunkt", _when);
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

    private static DateTimePicker DateTimeEditor() => new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd.MM.yyyy HH:mm",
        Value = DateTime.Now,
    };

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
