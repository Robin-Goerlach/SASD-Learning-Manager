using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.WinForms.Forms;
using SASD.Bewerbungsmanager.WinForms.Presentation;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>
/// Displays the local communication inbox. The view imports normalized Mail Workbench handoffs or
/// clipboard text, shows deterministic analysis results and lets the user confirm workflow context.
/// </summary>
public sealed class CommunicationsControl : UserControl
{
    private readonly CommunicationImportService _service;
    private readonly OpportunityService _opportunities;
    private readonly ApplicationService _applications;
    private readonly ContactService _contacts;
    private readonly OrganizationService _organizations;
    private readonly JobLeadService _jobLeads;
    private readonly UiExceptionPresenter _errors;
    private readonly DataGridView _grid = ControlFactory.DataGrid();
    private readonly TextBox _details = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Both,
        WordWrap = false,
        Font = new Font(FontFamily.GenericMonospace, 9),
    };
    private IReadOnlyList<CommunicationMessage> _items = [];

    /// <summary>Initializes the communication integration view.</summary>
    public CommunicationsControl(
        CommunicationImportService service,
        OpportunityService opportunities,
        ApplicationService applications,
        ContactService contacts,
        OrganizationService organizations,
        JobLeadService jobLeads,
        UiExceptionPresenter errors)
    {
        _service = service;
        _opportunities = opportunities;
        _applications = applications;
        _contacts = contacts;
        _organizations = organizations;
        _jobLeads = jobLeads;
        _errors = errors;
        BuildLayout();
        Load += async (_, _) => await RefreshAsync();
        _grid.SelectionChanged += (_, _) => ShowSelectedDetails();
    }

    private void BuildLayout()
    {
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8),
        };
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Mail-Workbench JSON...", async (_, _) => await ImportHandoffAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Zwischenablage...", async (_, _) => await ImportClipboardAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Zuordnen...", async (_, _) => await LinkSelectedAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("ACTION erzeugen...", async (_, _) => await CreateActionAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Als Job-Fund", async (_, _) => await CreateJobLeadAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Als Stelle übernehmen...", async (_, _) => await CreateOpportunityAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Ignorieren", async (_, _) => await IgnoreAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Aktualisieren", async (_, _) => await RefreshAsync()));

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
        };
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        content.Controls.Add(_grid, 0, 0);
        content.Controls.Add(_details, 0, 1);
        Controls.Add(content);
        Controls.Add(toolbar);
    }

    private async Task RefreshAsync()
    {
        try
        {
            _items = await _service.ListAsync();
            _grid.DataSource = _items.Select(item => new
            {
                item.Id,
                Zeitpunkt = item.MessageAtUtc.LocalDateTime.ToString("g"),
                Richtung = DisplayText.CommunicationDirection(item.Direction),
                Art = DisplayText.CommunicationKind(item.Kind),
                Status = DisplayText.CommunicationStatus(item.Status),
                Absender = FormatSender(item),
                Betreff = item.Subject,
                Zugeordnet = item.ApplicationId is not null || item.OpportunityId is not null || item.ContactId is not null ? "Ja" : string.Empty,
                Timeline = item.ActivityId is not null ? "Ja" : string.Empty,
            }).ToList();
            if (_grid.Columns["Id"] is { } idColumn)
            {
                idColumn.Visible = false;
            }

            ShowSelectedDetails();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task ImportHandoffAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "SASD Mail Workbench Handoff importieren",
            Filter = "SASD Communication JSON (*.json)|*.json|Alle Dateien (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var result = await _service.ImportHandoffFileAsync(dialog.FileName);
            await RefreshAsync();
            MessageBox.Show(
                this,
                $"Importiert: {result.Imported}\nDuplikate: {result.Duplicates}\nTimeline-Aktivitäten erzeugt: {result.ActivitiesCreated}",
                "Kommunikationsimport",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task ImportClipboardAsync()
    {
        using var dialog = new CommunicationClipboardImportForm();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var result = await _service.ImportAsync(dialog.Input);
            await RefreshAsync();
            var text = result.WasDuplicate
                ? "Die Kommunikation war bereits vorhanden und wurde nicht erneut angelegt."
                : result.ActivityCreatedAutomatically
                    ? "Kommunikation importiert und automatisch als E-Mail-Aktivität in die Timeline übernommen."
                    : "Kommunikation importiert.";
            MessageBox.Show(this, text, "Kommunikationsimport", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task LinkSelectedAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        try
        {
            var opportunities = await _opportunities.ListAsync();
            var applications = await _applications.ListAsync();
            var contacts = await _contacts.ListAsync(includeArchived: true);
            var organizations = await _organizations.ListAsync(includeArchived: true);
            using var dialog = new CommunicationLinkForm(opportunities, applications, contacts, organizations, selected);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            await _service.LinkAsync(selected.Id, dialog.Input);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task CreateActionAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        using var dialog = new CommunicationActionForm(selected.Subject);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _service.CreateActionAsync(selected.Id, dialog.ActionTitle, dialog.DueAtUtc);
            MessageBox.Show(this, "ACTION wurde erzeugt.", "Kommunikation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task CreateJobLeadAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        try
        {
            var analysis = _service.Analyze(selected);
            var result = await _jobLeads.ImportClipboardAsync(new SASD.Bewerbungsmanager.Application.Models.JobLeadClipboardInput(
                SearchProfileId: null,
                SourceSystem: selected.SourceSystem,
                ExternalJobId: null,
                Title: analysis.SuggestedTitle,
                OrganizationName: null,
                Location: null,
                RemoteText: null,
                SalaryText: null,
                Url: analysis.Urls.FirstOrDefault(),
                DescriptionText: selected.BodyText,
                PublishedAtUtc: null));
            MessageBox.Show(
                this,
                result.WasDuplicate ? "Der Job-Fund war bereits vorhanden." : "Die Nachricht wurde in die Jobsuche übernommen.",
                "Kommunikation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task CreateOpportunityAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        var analysis = _service.Analyze(selected);
        using var dialog = new CommunicationOpportunityForm(analysis);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var opportunity = await _service.CreateOpportunityFromMessageAsync(selected.Id, dialog.OpportunityTitle, dialog.SourceUrl);
            await RefreshAsync();
            MessageBox.Show(
                this,
                $"Stelle '{opportunity.Title}' wurde angelegt. Weitere Details können in der Stellenansicht ergänzt werden.",
                "Kommunikation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task IgnoreAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        try
        {
            await _service.IgnoreAsync(selected.Id);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private void ShowSelectedDetails()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            _details.Clear();
            return;
        }

        var analysis = _service.Analyze(selected);
        var urls = analysis.Urls.Count == 0 ? "—" : string.Join(Environment.NewLine, analysis.Urls.Select(item => $"- {item}"));
        _details.Text =
            $"Quelle: {selected.SourceSystem}{Environment.NewLine}" +
            $"Externe ID: {selected.ExternalMessageId ?? "—"}{Environment.NewLine}" +
            $"Von: {FormatSender(selected)}{Environment.NewLine}" +
            $"An: {selected.ToAddresses ?? "—"}{Environment.NewLine}" +
            $"Zeit: {selected.MessageAtUtc.LocalDateTime:g}{Environment.NewLine}" +
            $"Art: {DisplayText.CommunicationKind(selected.Kind)}{Environment.NewLine}" +
            $"Status: {DisplayText.CommunicationStatus(selected.Status)}{Environment.NewLine}" +
            $"Betreff: {selected.Subject}{Environment.NewLine}" +
            $"Gefundene Links:{Environment.NewLine}{urls}{Environment.NewLine}{Environment.NewLine}" +
            selected.BodyText;
    }

    private CommunicationMessage? SelectedItem()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is Guid id)
        {
            return _items.SingleOrDefault(item => item.Id == id);
        }

        return null;
    }

    private static string FormatSender(CommunicationMessage item)
    {
        if (string.IsNullOrWhiteSpace(item.FromAddress))
        {
            return item.FromName ?? string.Empty;
        }

        return string.IsNullOrWhiteSpace(item.FromName)
            ? item.FromAddress
            : $"{item.FromName} <{item.FromAddress}>";
    }
}
