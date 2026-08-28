using System.Text.RegularExpressions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Controls;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>Collects one manually copied job result when no JSON/CSV source adapter is available.</summary>
public sealed partial class JobLeadClipboardImportForm : Form
{
    private readonly ComboBox _searchProfile = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _sourceSystem = new() { Text = "Clipboard" };
    private readonly TextBox _externalJobId = new();
    private readonly TextBox _title = new();
    private readonly TextBox _organization = new();
    private readonly TextBox _location = new();
    private readonly TextBox _remote = new();
    private readonly TextBox _salary = new();
    private readonly TextBox _url = new();
    private readonly TextBox _description = new() { Multiline = true, Height = 180, ScrollBars = ScrollBars.Vertical };
    private readonly CheckBox _hasPublishedDate = new() { Text = "bekannt", AutoSize = true };
    private readonly DateTimePicker _publishedDate = new() { Format = DateTimePickerFormat.Short, Value = DateTime.Today };

    /// <summary>Creates the manual import form and optionally initializes it from Windows clipboard text.</summary>
    public JobLeadClipboardImportForm(IReadOnlyList<SearchProfile> searchProfiles, string? clipboardText)
    {
        Text = "Job aus Zwischenablage erfassen";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(820, 650);
        Size = new Size(900, 720);

        _searchProfile.Items.Add(new ReferenceChoice(null, "(kein Suchprofil)"));
        foreach (var profile in searchProfiles.Where(item => item.IsActive).OrderBy(item => item.Name))
        {
            _searchProfile.Items.Add(new ReferenceChoice(profile.Id, $"{profile.Name} — {profile.Source}"));
        }
        _searchProfile.DisplayMember = nameof(ReferenceChoice.Text);
        _searchProfile.SelectedIndex = 0;

        if (!string.IsNullOrWhiteSpace(clipboardText))
        {
            _description.Text = clipboardText.Trim();
            var firstLine = clipboardText
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
            _title.Text = firstLine?.Length > 250 ? firstLine[..250] : firstLine ?? string.Empty;

            var urlMatch = HttpUrlRegex().Match(clipboardText);
            if (urlMatch.Success)
            {
                _url.Text = urlMatch.Value.TrimEnd('.', ',', ';', ')', ']');
            }
        }

        BuildLayout();
    }

    /// <summary>Gets the normalized application-layer input from the form values.</summary>
    public JobLeadClipboardInput Input
        => new(
            (_searchProfile.SelectedItem as ReferenceChoice)?.Id,
            _sourceSystem.Text,
            _externalJobId.Text,
            _title.Text,
            _organization.Text,
            _location.Text,
            _remote.Text,
            _salary.Text,
            _url.Text,
            _description.Text,
            _hasPublishedDate.Checked ? ToUtc(_publishedDate.Value.Date) : null);

    private void BuildLayout()
    {
        var table = ControlFactory.EditorTable();
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ControlFactory.AddEditorRow(table, "Suchprofil", _searchProfile);
        ControlFactory.AddEditorRow(table, "Quellsystem *", _sourceSystem);
        ControlFactory.AddEditorRow(table, "Externe ID", _externalJobId);
        ControlFactory.AddEditorRow(table, "Position *", _title);
        ControlFactory.AddEditorRow(table, "Organisation", _organization);
        ControlFactory.AddEditorRow(table, "Standort", _location);
        ControlFactory.AddEditorRow(table, "Remote/Hybrid", _remote);
        ControlFactory.AddEditorRow(table, "Gehalt", _salary);
        ControlFactory.AddEditorRow(table, "URL", _url);

        var publishedPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        publishedPanel.Controls.Add(_hasPublishedDate);
        publishedPanel.Controls.Add(_publishedDate);
        ControlFactory.AddEditorRow(table, "Veröffentlicht", publishedPanel);
        ControlFactory.AddEditorRow(table, "Stellenbeschreibung", _description);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
        var save = new Button { Text = "Importieren", DialogResult = DialogResult.OK, AutoSize = true };
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

    [GeneratedRegex("https?://[^\\s<>\"']+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex HttpUrlRegex();
}
