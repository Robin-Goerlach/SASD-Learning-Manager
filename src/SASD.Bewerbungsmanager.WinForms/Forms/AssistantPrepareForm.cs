using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Controls;
using SASD.Bewerbungsmanager.WinForms.Presentation;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>
/// Modal editor used to choose the scope and purpose of a new optional assistant handoff. No
/// external provider is contacted from this dialog.
/// </summary>
public sealed class AssistantPrepareForm : Form
{
    private readonly ComboBox _target = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _task = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _additionalInstructions = new()
    {
        Multiline = true,
        Height = 120,
        ScrollBars = ScrollBars.Vertical,
    };

    /// <summary>Creates the preparation dialog for the supplied application/opportunity targets.</summary>
    public AssistantPrepareForm(IReadOnlyList<AssistantTarget> targets)
    {
        Text = "Assistenz-Prompt vorbereiten";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(760, 460);
        Size = new Size(820, 500);

        _target.DataSource = targets.ToList();
        _target.DisplayMember = nameof(AssistantTarget.DisplayText);
        _task.DataSource = Enum.GetValues<AssistantTaskKind>()
            .Select(item => new AssistantTaskChoice(item, DisplayText.AssistantTaskKind(item)))
            .ToList();
        _task.DisplayMember = nameof(AssistantTaskChoice.Text);
        BuildLayout();
    }

    /// <summary>Gets the currently selected assistant preparation values.</summary>
    public AssistantPreparationInput Input
    {
        get
        {
            var target = (AssistantTarget)_target.SelectedItem!;
            var task = ((AssistantTaskChoice)_task.SelectedItem!).Kind;
            return target.IsApplication
                ? new AssistantPreparationInput(target.OpportunityId, target.Id, task, _additionalInstructions.Text)
                : new AssistantPreparationInput(target.Id, null, task, _additionalInstructions.Text);
        }
    }

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Kontext *", _target);
        ControlFactory.AddEditorRow(table, "Aufgabe *", _task);
        ControlFactory.AddEditorRow(table, "Zusatzanweisung", _additionalInstructions);

        var notice = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(620, 0),
            Text = "Der Bewerbungsmanager erzeugt nur einen lokalen, prüfbaren Prompt. " +
                   "Eine Übertragung an ChatGPT oder einen anderen Assistenten erfolgt ausschließlich durch dich.",
        };
        ControlFactory.AddEditorRow(table, "Hinweis", notice);

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
        };
        var create = new Button { Text = "Prompt erzeugen", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(create);
        buttons.Controls.Add(cancel);
        table.Controls.Add(buttons, 1, table.RowCount++);
        AcceptButton = create;
        CancelButton = cancel;
        Controls.Add(table);
    }

    private sealed record AssistantTaskChoice(AssistantTaskKind Kind, string Text);
}
