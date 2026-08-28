using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Modal editor for a manually checked job-search source.</summary>
public sealed class SearchProfileEditForm : Form
{
    private readonly TextBox _name = new();
    private readonly TextBox _source = new();
    private readonly TextBox _url = new();
    private readonly NumericUpDown _interval = new() { Minimum = 1, Maximum = 365, Value = 1 };
    private readonly DateTimePicker _nextCheck = new() { Format = DateTimePickerFormat.Short, Value = DateTime.Today };
    private readonly CheckBox _active = new() { Text = "aktiv", AutoSize = true, Checked = true };
    private readonly TextBox _notes = new() { Multiline = true, Height = 100, ScrollBars = ScrollBars.Vertical };

    /// <summary>Creates an editor for a new or existing search profile.</summary>
    public SearchProfileEditForm(SearchProfile? profile = null)
    {
        Text = profile is null ? "Suchprofil anlegen" : "Suchprofil bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(720, 520);
        BuildLayout();

        if (profile is not null)
        {
            _name.Text = profile.Name;
            _source.Text = profile.Source;
            _url.Text = profile.Url;
            _interval.Value = Math.Clamp(profile.CheckIntervalDays, 1, 365);
            _nextCheck.Value = profile.NextCheckAtUtc.LocalDateTime.Date;
            _active.Checked = profile.IsActive;
            _notes.Text = profile.Notes ?? string.Empty;
        }
    }

    /// <summary>Gets the current editor values as application-layer input.</summary>
    public SearchProfileInput Input
        => new(
            _name.Text,
            _source.Text,
            _url.Text,
            Decimal.ToInt32(_interval.Value),
            ToUtc(_nextCheck.Value.Date),
            _active.Checked,
            _notes.Text);

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Name *", _name);
        ControlFactory.AddEditorRow(table, "Quelle *", _source);
        ControlFactory.AddEditorRow(table, "URL *", _url);
        ControlFactory.AddEditorRow(table, "Prüfintervall (Tage)", _interval);
        ControlFactory.AddEditorRow(table, "Nächste Prüfung", _nextCheck);
        ControlFactory.AddEditorRow(table, "Status", _active);
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

    private static DateTimeOffset ToUtc(DateTime localDate)
        => new DateTimeOffset(localDate, TimeZoneInfo.Local.GetUtcOffset(localDate)).ToUniversalTime();
}
