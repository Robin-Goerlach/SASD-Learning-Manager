using SASD.Bewerbungsmanager.WinForms.Presentation;
using JobApplication = SASD.Bewerbungsmanager.Domain.Entities.Application;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Displays the immutable stage history of a selected application.</summary>
public sealed class ApplicationHistoryForm : Form
{
    /// <summary>Creates a read-only history window for the supplied application.</summary>
    public ApplicationHistoryForm(JobApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        Text = "Bewerbungsverlauf";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(700, 380);
        Size = new Size(760, 460);

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            DataSource = application.StatusHistory
                .OrderByDescending(item => item.ChangedAtUtc)
                .Select(item => new
                {
                    Zeitpunkt = item.ChangedAtUtc.LocalDateTime,
                    Status = DisplayText.ApplicationStage(item.Stage),
                    Notiz = item.Note ?? string.Empty,
                })
                .ToList(),
        };

        var close = new Button
        {
            Text = "Schließen",
            DialogResult = DialogResult.OK,
            AutoSize = true,
            Anchor = AnchorStyles.Right,
        };
        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8),
        };
        bottom.Controls.Add(close);
        AcceptButton = close;

        Controls.Add(grid);
        Controls.Add(bottom);
    }
}
