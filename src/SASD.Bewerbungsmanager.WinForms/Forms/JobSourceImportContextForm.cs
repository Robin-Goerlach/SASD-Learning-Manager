using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Lets the user optionally associate a local adapter file with one saved search profile.</summary>
public sealed class JobSourceImportContextForm : Form
{
    private readonly ComboBox _searchProfile = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    /// <summary>Creates the small context dialog for the selected import file.</summary>
    public JobSourceImportContextForm(string path, IReadOnlyList<SearchProfile> profiles)
    {
        Text = "Job-Quellen-Import";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(760, 300);
        Size = new Size(820, 340);

        _searchProfile.Items.Add(new ReferenceChoice(null, "(Zuordnung aus Datei verwenden / keine Zuordnung)"));
        foreach (var profile in profiles.Where(item => item.IsActive).OrderBy(item => item.Name))
        {
            _searchProfile.Items.Add(new ReferenceChoice(profile.Id, $"{profile.Name} — {profile.Source}"));
        }
        _searchProfile.DisplayMember = nameof(ReferenceChoice.Text);
        _searchProfile.SelectedIndex = 0;
        BuildLayout(path);
    }

    /// <summary>Gets the optional search-profile override selected by the user.</summary>
    public Guid? SearchProfileId => (_searchProfile.SelectedItem as ReferenceChoice)?.Id;

    private void BuildLayout(string path)
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Datei", new TextBox { Text = path, ReadOnly = true });
        ControlFactory.AddEditorRow(table, "Suchprofil", _searchProfile);

        var note = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(620, 0),
            Text = "Wenn ein Suchprofil gewählt wird, überschreibt diese bewusste Zuordnung eine optionale SearchProfileId aus der Datei. " +
                   "Nach erfolgreichem Import wird das Profil als geprüft markiert.",
        };
        ControlFactory.AddEditorRow(table, "Hinweis", note);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var import = new Button { Text = "Importieren", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(import);
        buttons.Controls.Add(cancel);
        table.Controls.Add(buttons, 1, table.RowCount++);
        AcceptButton = import;
        CancelButton = cancel;
        Controls.Add(table);
    }
}
