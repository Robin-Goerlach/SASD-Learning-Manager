using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Captures a deliberate application-stage transition and an optional history note.</summary>
public sealed class ApplicationStageForm : Form
{
    private readonly ComboBox _stage = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _note = new() { Multiline = true, Height = 90, ScrollBars = ScrollBars.Vertical };

    /// <summary>Creates the status editor with the current stage selected.</summary>
    public ApplicationStageForm(ApplicationStage currentStage)
    {
        Text = "Bewerbungsstatus ändern";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(600, 320);
        _stage.DataSource = Enum.GetValues<ApplicationStage>();
        _stage.SelectedItem = currentStage;
        BuildLayout();
    }

    /// <summary>Gets the selected stage.</summary>
    public ApplicationStage Stage => (ApplicationStage)_stage.SelectedItem!;

    /// <summary>Gets the optional history note.</summary>
    public string Note => _note.Text;

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Neuer Status", _stage);
        ControlFactory.AddEditorRow(table, "Notiz", _note);
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
}
