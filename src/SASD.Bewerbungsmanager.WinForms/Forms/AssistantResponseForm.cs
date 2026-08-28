using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Modal review dialog for text pasted back from an external or local assistant.</summary>
public sealed class AssistantResponseForm : Form
{
    private readonly TextBox _provider = new() { Text = "ChatGPT / externer Assistent" };
    private readonly TextBox _response = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Both,
        WordWrap = true,
        Height = 330,
        Font = new Font(FontFamily.GenericMonospace, 9),
    };

    /// <summary>Creates the response-review dialog and optionally pre-fills clipboard content.</summary>
    public AssistantResponseForm(string? initialResponse)
    {
        Text = "Assistenz-Antwort prüfen und speichern";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(820, 620);
        Size = new Size(900, 680);
        _response.Text = initialResponse ?? string.Empty;
        BuildLayout();
    }

    /// <summary>Gets the response after the user has reviewed it.</summary>
    public AssistantCompletionInput Input => new(_response.Text, _provider.Text);

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Provider", _provider);
        ControlFactory.AddEditorRow(table, "Antwort *", _response);

        var notice = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(660, 0),
            Text = "Die Antwort wird als untrusted Text gespeichert. Sie verändert keine Bewerbung, " +
                   "Aufgabe oder Stelle automatisch.",
        };
        ControlFactory.AddEditorRow(table, "Hinweis", notice);

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
        };
        var save = new Button { Text = "Antwort speichern", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        table.Controls.Add(buttons, 1, table.RowCount++);
        AcceptButton = save;
        CancelButton = cancel;
        Controls.Add(table);
    }
}
