using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Read-only view of immutable document snapshots used by one application.</summary>
public sealed class ApplicationDocumentsForm : Form
{
    /// <summary>Creates the snapshot list.</summary>
    public ApplicationDocumentsForm(IReadOnlyList<ApplicationDocumentSnapshot> snapshots)
    {
        Text = "Verwendete Dokumentversionen";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(920, 430);

        var grid = ControlFactory.DataGrid();
        grid.DataSource = snapshots.Select(item => new
        {
            Typ = item.Type.ToString(),
            item.Label,
            item.Version,
            Sprache = item.Language,
            SHA256 = item.Sha256,
            Snapshot = item.StoredPath,
        }).ToList();
        Controls.Add(grid);
    }
}
