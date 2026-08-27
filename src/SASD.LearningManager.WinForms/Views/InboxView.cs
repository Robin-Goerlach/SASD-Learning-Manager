using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.WinForms.Forms;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Views;

/// <summary>
/// Dedicated Milestone-2 work area for captured but not yet classified resources. The view keeps
/// the Inbox workflow intentionally small: search, classify, open the URL or archive an irrelevant entry.
/// </summary>
public sealed class InboxView : UserControl
{
    private const int PageSize = 100;
    private readonly ResourceService _resourceService;
    private readonly SASD.LearningManager.Application.Providers.ProviderService _providerService;
    private readonly ILogger<InboxView> _logger;

    private readonly TextBox _searchBox = new() { Width = 320, PlaceholderText = "Titel, URL oder Capture-Notiz" };
    private readonly DataGridView _grid = new();
    private readonly Label _pageLabel = new() { AutoSize = true, TextAlign = ContentAlignment.MiddleCenter };
    private readonly Button _previousButton = new() { Text = "‹ Zurück", AutoSize = true };
    private readonly Button _nextButton = new() { Text = "Weiter ›", AutoSize = true };
    private int _currentPage = 1;

    public InboxView(
        ResourceService resourceService,
        SASD.LearningManager.Application.Providers.ProviderService providerService,
        ILogger<InboxView> logger)
    {
        _resourceService = resourceService;
        _providerService = providerService;
        _logger = logger;

        Dock = DockStyle.Fill;
        AutoScaleMode = AutoScaleMode.Dpi;
        ConfigureGrid();
        Controls.Add(_grid);
        Controls.Add(BuildPagingBar());
        Controls.Add(BuildToolbar());
        Controls.Add(BuildHint());

        _searchBox.KeyDown += async (_, args) =>
        {
            if (args.KeyCode != Keys.Enter)
            {
                return;
            }

            args.SuppressKeyPress = true;
            _currentPage = 1;
            await RefreshAsync().ConfigureAwait(true);
        };
        _grid.CellDoubleClick += async (_, args) =>
        {
            if (args.RowIndex >= 0)
            {
                await ClassifySelectedAsync().ConfigureAwait(true);
            }
        };
    }

    /// <summary>Reloads the current Inbox page.</summary>
    public async Task RefreshAsync()
    {
        try
        {
            await LoadPageAsync().ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Inbox laden");
        }
    }

    private Control BuildHint()
    {
        return new Label
        {
            Dock = DockStyle.Top,
            Height = 42,
            Text = "Inbox = schnell erfasste Ressourcen. Klassifiziere sie später mit Provider, Typ, Tags und Lernstatus – oder archiviere irrelevante Einträge.",
            AutoEllipsis = true,
            ForeColor = SystemColors.GrayText,
            Padding = new Padding(0, 8, 0, 4)
        };
    }

    private Control BuildToolbar()
    {
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 46,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 4)
        };

        var search = new Button { Text = "Suchen", AutoSize = true };
        search.Click += async (_, _) => { _currentPage = 1; await RefreshAsync().ConfigureAwait(true); };
        var classify = new Button { Text = "Klassifizieren …", AutoSize = true };
        classify.Click += async (_, _) => await ClassifySelectedAsync().ConfigureAwait(true);
        var open = new Button { Text = "URL öffnen", AutoSize = true };
        open.Click += async (_, _) => await OpenSelectedAsync().ConfigureAwait(true);
        var archive = new Button { Text = "Verwerfen / archivieren", AutoSize = true };
        archive.Click += async (_, _) => await ArchiveSelectedAsync().ConfigureAwait(true);

        toolbar.Controls.AddRange([
            new Label { Text = "Suche:", AutoSize = true, Margin = new Padding(0, 9, 3, 0) },
            _searchBox,
            search,
            classify,
            open,
            archive
        ]);
        return toolbar;
    }

    private Control BuildPagingBar()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 5, 0, 0)
        };
        _previousButton.Click += async (_, _) =>
        {
            if (_currentPage <= 1) return;
            _currentPage--;
            await RefreshAsync().ConfigureAwait(true);
        };
        _nextButton.Click += async (_, _) =>
        {
            _currentPage++;
            await RefreshAsync().ConfigureAwait(true);
        };
        panel.Controls.Add(_nextButton);
        panel.Controls.Add(_pageLabel);
        panel.Controls.Add(_previousButton);
        return panel;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.BackgroundColor = SystemColors.Window;
        _grid.BorderStyle = BorderStyle.Fixed3D;
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Titel",
            DataPropertyName = nameof(InboxListItemDto.Title),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 240
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "URL",
            DataPropertyName = nameof(InboxListItemDto.Url),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            MinimumWidth = 260
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Notiz",
            DataPropertyName = nameof(InboxListItemDto.Note),
            Width = 240
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Erfasst",
            DataPropertyName = nameof(InboxListItemDto.CapturedAtUtc),
            Width = 145
        });
        _grid.CellFormatting += (_, args) =>
        {
            if (args.Value is DateTimeOffset captured)
            {
                args.Value = captured.ToLocalTime().ToString("g");
                args.FormattingApplied = true;
            }
        };
    }

    private InboxListItemDto? Selected => _grid.CurrentRow?.DataBoundItem as InboxListItemDto;

    private async Task LoadPageAsync()
    {
        var result = await _resourceService.SearchInboxAsync(
            new InboxSearchCriteria(_searchBox.Text, _currentPage, PageSize)).ConfigureAwait(true);

        if (_currentPage > result.TotalPages)
        {
            _currentPage = result.TotalPages;
            if (_currentPage != result.PageNumber)
            {
                await LoadPageAsync().ConfigureAwait(true);
                return;
            }
        }

        _grid.DataSource = result.Items.ToList();
        _pageLabel.Text = $"Seite {result.PageNumber} / {result.TotalPages} · {result.TotalCount} Inbox-Einträge";
        _previousButton.Enabled = result.PageNumber > 1;
        _nextButton.Enabled = result.PageNumber < result.TotalPages;
    }

    private async Task ClassifySelectedAsync()
    {
        var selected = Selected;
        if (selected is null) return;

        using var form = new ResourceEditForm(_resourceService, _providerService, _logger, selected.Id, classificationMode: true);
        if (form.ShowDialog(FindForm()) == DialogResult.OK)
        {
            await RefreshAsync().ConfigureAwait(true);
        }
    }

    private async Task OpenSelectedAsync()
    {
        var selected = Selected;
        if (selected is null) return;
        try
        {
            await _resourceService.OpenUrlAsync(selected.Id).ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Inbox-URL öffnen");
        }
    }

    private async Task ArchiveSelectedAsync()
    {
        var selected = Selected;
        if (selected is null) return;
        if (MessageBox.Show(
                FindForm(),
                $"'{selected.Title}' aus der Inbox verwerfen und archivieren?\n\nDie Ressource bleibt historisch erhalten und kann später wiederhergestellt werden.",
                "Inbox-Eintrag archivieren",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _resourceService.ArchiveAsync(selected.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Inbox-Eintrag archivieren");
        }
    }
}
