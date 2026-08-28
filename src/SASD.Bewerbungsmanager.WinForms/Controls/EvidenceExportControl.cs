using System.Globalization;
using SASD.Bewerbungsmanager.Application.Abstractions;
using SASD.Bewerbungsmanager.Application.Models;
using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.WinForms.Presentation;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>
/// Provides period-based application evidence plus privacy-conscious application dossier exports.
/// The control only orchestrates use cases and file dialogs; report semantics and file generation
/// remain outside the presentation layer.
/// </summary>
public sealed class EvidenceExportControl : UserControl
{
    private readonly ApplicationEvidenceService _evidenceService;
    private readonly ApplicationDossierService _dossierService;
    private readonly IApplicationExportWriter _writer;
    private readonly ApplicationService _applicationService;
    private readonly OpportunityService _opportunityService;
    private readonly OrganizationService _organizationService;
    private readonly UiExceptionPresenter _errors;

    private readonly DateTimePicker _fromDate = new() { Format = DateTimePickerFormat.Short, Width = 110 };
    private readonly DateTimePicker _toDate = new() { Format = DateTimePickerFormat.Short, Width = 110 };
    private readonly Label _countLabel = new() { AutoSize = true, Margin = new Padding(12, 7, 0, 0) };
    private readonly DataGridView _preview = ControlFactory.DataGrid();
    private readonly ComboBox _applicationChoice = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 520 };

    /// <summary>Initializes the evidence and exchange view.</summary>
    public EvidenceExportControl(
        ApplicationEvidenceService evidenceService,
        ApplicationDossierService dossierService,
        IApplicationExportWriter writer,
        ApplicationService applicationService,
        OpportunityService opportunityService,
        OrganizationService organizationService,
        UiExceptionPresenter errors)
    {
        _evidenceService = evidenceService;
        _dossierService = dossierService;
        _writer = writer;
        _applicationService = applicationService;
        _opportunityService = opportunityService;
        _organizationService = organizationService;
        _errors = errors;

        var today = DateTime.Today;
        _fromDate.Value = new DateTime(today.Year, today.Month, 1);
        _toDate.Value = today;

        BuildLayout();
        Load += async (_, _) => await InitialLoadAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(new Label
        {
            Text = "Nachweise, Export und Austausch",
            AutoSize = true,
            Font = new Font(Font.FontFamily, 18, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 12),
        }, 0, 0);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildEvidenceTab());
        tabs.TabPages.Add(BuildExchangeTab());
        root.Controls.Add(tabs, 0, 1);
        Controls.Add(root);
    }

    private TabPage BuildEvidenceTab()
    {
        var page = new TabPage("Bewerbungsnachweis") { Padding = new Padding(10) };
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
        };
        toolbar.Controls.Add(new Label { Text = "Von", AutoSize = true, Margin = new Padding(0, 7, 6, 0) });
        toolbar.Controls.Add(_fromDate);
        toolbar.Controls.Add(new Label { Text = "Bis", AutoSize = true, Margin = new Padding(10, 7, 6, 0) });
        toolbar.Controls.Add(_toDate);
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Vorschau", async (_, _) => await RefreshEvidenceAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("CSV exportieren", async (_, _) => await ExportCsvAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("PDF exportieren", async (_, _) => await ExportPdfAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("CSV + PDF", async (_, _) => await ExportBothAsync()));
        toolbar.Controls.Add(_countLabel);
        root.Controls.Add(toolbar, 0, 0);

        root.Controls.Add(new Label
        {
            Text = "Es werden nur Bewerbungen mit tatsächlich erfasstem Versanddatum berücksichtigt. Der Zeitraum bezieht sich auf lokale Kalendertage.",
            AutoSize = true,
            MaximumSize = new Size(1000, 0),
            Margin = new Padding(0, 8, 0, 10),
        }, 0, 1);
        root.Controls.Add(_preview, 0, 2);
        page.Controls.Add(root);
        return page;
    }

    private TabPage BuildExchangeTab()
    {
        var page = new TabPage("Austauschdossier") { Padding = new Padding(10) };
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(new Label
        {
            Text = "Ein Austauschdossier bündelt den gespeicherten Kontext einer Bewerbung für andere Werkzeuge oder Personen. Lokale Dateipfade und Dokumentinhalte werden absichtlich nicht exportiert.",
            AutoSize = true,
            MaximumSize = new Size(1000, 0),
            Margin = new Padding(0, 0, 0, 12),
        }, 0, 0);

        var selector = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
        selector.Controls.Add(new Label { Text = "Bewerbung", AutoSize = true, Margin = new Padding(0, 7, 8, 0) });
        selector.Controls.Add(_applicationChoice);
        root.Controls.Add(selector, 0, 1);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 10, 0, 0) };
        buttons.Controls.Add(ControlFactory.ToolbarButton("JSON exportieren", async (_, _) => await ExportDossierJsonAsync()));
        buttons.Controls.Add(ControlFactory.ToolbarButton("Markdown exportieren", async (_, _) => await ExportDossierMarkdownAsync()));
        root.Controls.Add(buttons, 0, 2);

        root.Controls.Add(new Label
        {
            Text = "JSON eignet sich für maschinellen Austausch; Markdown ist für Menschen, Dokumentation und bewusste Übergabe an einen separaten Beratungs- oder KI-Chat gedacht.",
            AutoSize = true,
            MaximumSize = new Size(1000, 0),
            Margin = new Padding(0, 14, 0, 0),
            Font = new Font(Font.FontFamily, 9, FontStyle.Italic),
        }, 0, 3);
        page.Controls.Add(root);
        return page;
    }

    private async Task InitialLoadAsync()
    {
        await RefreshEvidenceAsync();
        await RefreshApplicationChoicesAsync();
    }

    private async Task RefreshEvidenceAsync()
    {
        try
        {
            UseWaitCursor = true;
            var report = await BuildReportAsync();
            _countLabel.Text = $"{report.Items.Count.ToString(CultureInfo.CurrentCulture)} versendet";
            _preview.DataSource = report.Items.Select(item => new
            {
                Versanddatum = item.SubmittedAtUtc.ToLocalTime().ToString("d", CultureInfo.CurrentCulture),
                Unternehmen = item.Employer,
                Position = item.Position,
                Standort = item.Location ?? string.Empty,
                Kanal = DisplayText.ApplicationChannel(item.Channel),
                Status = DisplayText.ApplicationStage(item.Stage),
                Quellen = item.Sources,
            }).ToList();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private async Task RefreshApplicationChoicesAsync()
    {
        try
        {
            var applications = await _applicationService.ListAsync();
            var opportunities = await _opportunityService.ListAsync();
            var organizations = await _organizationService.ListAsync(includeArchived: true);
            var opportunityById = opportunities.ToDictionary(item => item.Id);
            var organizationById = organizations.ToDictionary(item => item.Id);

            var choices = applications
                .Select(application =>
                {
                    opportunityById.TryGetValue(application.OpportunityId, out var opportunity);
                    var employer = opportunity?.EmployerOrganizationId is Guid employerId
                        && organizationById.TryGetValue(employerId, out var organization)
                            ? organization.Name
                            : string.Empty;
                    var submitted = application.SubmittedAtUtc?.ToLocalTime().ToString("d", CultureInfo.CurrentCulture) ?? "noch nicht versendet";
                    var text = $"{opportunity?.Title ?? "(Stelle fehlt)"} — {employer} — {submitted}";
                    return new ApplicationChoice(application.Id, text);
                })
                .OrderBy(item => item.Text, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            _applicationChoice.DataSource = choices;
            _applicationChoice.DisplayMember = nameof(ApplicationChoice.Text);
            _applicationChoice.ValueMember = nameof(ApplicationChoice.Id);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task ExportCsvAsync()
    {
        try
        {
            var report = await GetCurrentReportAsync();
            using var dialog = new SaveFileDialog
            {
                Filter = "CSV-Datei (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
                DefaultExt = "csv",
                AddExtension = true,
                FileName = EvidenceBaseName(report) + ".csv",
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            await _writer.WriteEvidenceCsvAsync(report, dialog.FileName);
            ShowExportCompleted(dialog.FileName);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task ExportPdfAsync()
    {
        try
        {
            var report = await GetCurrentReportAsync();
            using var dialog = new SaveFileDialog
            {
                Filter = "PDF-Datei (*.pdf)|*.pdf|Alle Dateien (*.*)|*.*",
                DefaultExt = "pdf",
                AddExtension = true,
                FileName = EvidenceBaseName(report) + ".pdf",
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            await _writer.WriteEvidencePdfAsync(report, dialog.FileName);
            ShowExportCompleted(dialog.FileName);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task ExportBothAsync()
    {
        try
        {
            var report = await GetCurrentReportAsync();
            using var dialog = new FolderBrowserDialog
            {
                Description = "Ordner für CSV- und PDF-Bewerbungsnachweis auswählen",
                UseDescriptionForTitle = true,
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var baseName = EvidenceBaseName(report);
            var csvPath = Path.Combine(dialog.SelectedPath, baseName + ".csv");
            var pdfPath = Path.Combine(dialog.SelectedPath, baseName + ".pdf");
            await _writer.WriteEvidenceCsvAsync(report, csvPath);
            await _writer.WriteEvidencePdfAsync(report, pdfPath);
            MessageBox.Show(
                this,
                $"CSV und PDF wurden exportiert nach:{Environment.NewLine}{dialog.SelectedPath}",
                "SASD Bewerbungsmanager",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private Task ExportDossierJsonAsync()
        => ExportDossierAsync("JSON-Datei (*.json)|*.json|Alle Dateien (*.*)|*.*", "json", _writer.WriteDossierJsonAsync);

    private Task ExportDossierMarkdownAsync()
        => ExportDossierAsync("Markdown-Datei (*.md)|*.md|Textdatei (*.txt)|*.txt|Alle Dateien (*.*)|*.*", "md", _writer.WriteDossierMarkdownAsync);

    private async Task ExportDossierAsync(
        string filter,
        string extension,
        Func<ApplicationExchangeDossier, string, CancellationToken, Task> writeAsync)
    {
        try
        {
            if (_applicationChoice.SelectedItem is not ApplicationChoice choice)
            {
                MessageBox.Show(
                    this,
                    "Bitte zuerst eine Bewerbung auswählen.",
                    "SASD Bewerbungsmanager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var dossier = await _dossierService.BuildAsync(choice.Id);
            using var dialog = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = extension,
                AddExtension = true,
                FileName = $"Bewerbungsdossier_{SanitizeFileName(dossier.Position)}_{DateTime.Today:yyyy-MM-dd}.{extension}",
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            await writeAsync(dossier, dialog.FileName, CancellationToken.None);
            ShowExportCompleted(dialog.FileName);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private Task<ApplicationEvidenceReport> BuildReportAsync()
        => _evidenceService.BuildAsync(DateOnly.FromDateTime(_fromDate.Value.Date), DateOnly.FromDateTime(_toDate.Value.Date));

    private async Task<ApplicationEvidenceReport> GetCurrentReportAsync()
    {
        // Rebuild before writing so a preview cannot become stale when the user changes the dates or
        // edits applications in another view and then returns to this tab.
        var report = await BuildReportAsync();
        return report;
    }

    private static string EvidenceBaseName(ApplicationEvidenceReport report)
        => $"Bewerbungsnachweis_{report.FromDate:yyyy-MM-dd}_bis_{report.ToDate:yyyy-MM-dd}";

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var builder = new System.Text.StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(invalid.Contains(character) ? '_' : character);
        }

        var result = builder.ToString().Trim();
        return result.Length == 0 ? "Bewerbung" : result.Length <= 80 ? result : result[..80];
    }

    private void ShowExportCompleted(string path)
        => MessageBox.Show(
            this,
            $"Export erfolgreich erstellt:{Environment.NewLine}{path}",
            "SASD Bewerbungsmanager",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

    private sealed record ApplicationChoice(Guid Id, string Text);
}
