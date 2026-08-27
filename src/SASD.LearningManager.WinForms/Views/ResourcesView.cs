using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Domain.Resources;
using SASD.LearningManager.WinForms.Forms;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Views;

/// <summary>Milestone-1 resource-library work area with search, paging and resource/provider management.</summary>
public sealed class ResourcesView : UserControl
{
    private readonly ResourceService _resourceService;
    private readonly ProviderService _providerService;
    private readonly ILogger<ResourcesView> _logger;

    private readonly TextBox _searchBox = new() { Width = 270, PlaceholderText = "Titel, URL, Beschreibung oder Provider" };
    private readonly ComboBox _providerFilter = new() { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _statusFilter = new() { Width = 135, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _includeArchived = new() { Text = "Archiv anzeigen", AutoSize = true };
    private readonly DataGridView _grid = new();
    private readonly Label _pageLabel = new() { AutoSize = true, TextAlign = ContentAlignment.MiddleCenter };
    private readonly Button _previousButton = new() { Text = "‹ Zurück", AutoSize = true };
    private readonly Button _nextButton = new() { Text = "Weiter ›", AutoSize = true };
    private int _currentPage = 1;
    private bool _suppressFilterEvents;
    private const int PageSize = 100;

    public ResourcesView(ResourceService resourceService, ProviderService providerService, ILogger<ResourcesView> logger)
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

        _searchBox.KeyDown += async (_, args) =>
        {
            if (args.KeyCode == Keys.Enter)
            {
                _currentPage = 1;
                await RefreshAsync().ConfigureAwait(true);
                args.SuppressKeyPress = true;
            }
        };
        _providerFilter.SelectedIndexChanged += async (_, _) =>
        {
            if (_suppressFilterEvents) return;
            _currentPage = 1;
            await SafeLoadPageAsync().ConfigureAwait(true);
        };
        _statusFilter.SelectedIndexChanged += async (_, _) =>
        {
            if (_suppressFilterEvents) return;
            _currentPage = 1;
            await SafeLoadPageAsync().ConfigureAwait(true);
        };
        _includeArchived.CheckedChanged += async (_, _) => { _currentPage = 1; await RefreshAsync().ConfigureAwait(true); };
        _grid.CellDoubleClick += async (_, args) => { if (args.RowIndex >= 0) await EditSelectedAsync().ConfigureAwait(true); };
    }

    /// <summary>Opens a specific resource in the standard editor, used for duplicate-resolution navigation.</summary>
    public async Task OpenResourceAsync(Guid resourceId)
    {
        try
        {
            using var form = new ResourceEditForm(_resourceService, _providerService, _logger, resourceId);
            if (form.ShowDialog(FindForm()) == DialogResult.OK)
            {
                await RefreshAsync().ConfigureAwait(true);
            }
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Ressource öffnen");
        }
    }

    /// <summary>Reloads provider filters and the current resource page.</summary>
    public async Task RefreshAsync()
    {
        try
        {
            await LoadProviderFilterAsync().ConfigureAwait(true);
            await LoadPageAsync().ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Ressourcen laden");
        }
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

        var searchButton = new Button { Text = "Suchen", AutoSize = true };
        searchButton.Click += async (_, _) => { _currentPage = 1; await RefreshAsync().ConfigureAwait(true); };
        var newButton = new Button { Text = "+ Ressource", AutoSize = true };
        newButton.Click += async (_, _) => await CreateResourceAsync().ConfigureAwait(true);
        var editButton = new Button { Text = "Bearbeiten", AutoSize = true };
        editButton.Click += async (_, _) => await EditSelectedAsync().ConfigureAwait(true);
        var openButton = new Button { Text = "URL öffnen", AutoSize = true };
        openButton.Click += async (_, _) => await OpenSelectedUrlAsync().ConfigureAwait(true);
        var archiveButton = new Button { Text = "Archivieren", AutoSize = true };
        archiveButton.Click += async (_, _) => await ArchiveSelectedAsync().ConfigureAwait(true);
        var restoreButton = new Button { Text = "Wiederherstellen", AutoSize = true };
        restoreButton.Click += async (_, _) => await RestoreSelectedAsync().ConfigureAwait(true);
        var providersButton = new Button { Text = "Provider …", AutoSize = true };
        providersButton.Click += async (_, _) => await ManageProvidersAsync().ConfigureAwait(true);

        toolbar.Controls.AddRange([
            new Label { Text = "Suche:", AutoSize = true, Margin = new Padding(0, 9, 3, 0) },
            _searchBox,
            searchButton,
            new Label { Text = "Provider:", AutoSize = true, Margin = new Padding(10, 9, 3, 0) },
            _providerFilter,
            new Label { Text = "Status:", AutoSize = true, Margin = new Padding(10, 9, 3, 0) },
            _statusFilter,
            _includeArchived,
            newButton,
            editButton,
            openButton,
            archiveButton,
            restoreButton,
            providersButton
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
            if (_currentPage > 1)
            {
                _currentPage--;
                await SafeLoadPageAsync().ConfigureAwait(true);
            }
        };
        _nextButton.Click += async (_, _) =>
        {
            _currentPage++;
            await SafeLoadPageAsync().ConfigureAwait(true);
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
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Titel", DataPropertyName = nameof(ResourceListItemDto.Title), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 260 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Provider", DataPropertyName = nameof(ResourceListItemDto.ProviderName), Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Typ", DataPropertyName = nameof(ResourceListItemDto.Type), Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = nameof(ResourceListItemDto.Status), Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fortschritt", DataPropertyName = nameof(ResourceListItemDto.ProgressPercent), Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Priorität", DataPropertyName = nameof(ResourceListItemDto.Priority), Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Schwierigkeit", DataPropertyName = nameof(ResourceListItemDto.Difficulty), Width = 105 });
        _grid.CellFormatting += (_, args) =>
        {
            if (args.ColumnIndex == 4 && args.Value is int progress)
            {
                args.Value = $"{progress} %";
                args.FormattingApplied = true;
            }
        };
    }

    private async Task LoadProviderFilterAsync()
    {
        _suppressFilterEvents = true;
        try
        {
            var selectedId = (_providerFilter.SelectedItem as ProviderFilterOption)?.Id;
            var selectedStatus = (_statusFilter.SelectedItem as StatusFilterOption)?.Status;
            var providers = await _providerService.ListAsync(includeArchived: false).ConfigureAwait(true);
            var options = new List<ProviderFilterOption> { new(null, "Alle") };
            options.AddRange(providers.Select(static p => new ProviderFilterOption(p.Id, p.Name)));
            _providerFilter.DataSource = options;
            _providerFilter.DisplayMember = nameof(ProviderFilterOption.Name);
            _providerFilter.SelectedItem = options.FirstOrDefault(x => x.Id == selectedId) ?? options[0];

            if (_statusFilter.Items.Count == 0)
            {
                _statusFilter.Items.Add(new StatusFilterOption(null, "Alle"));
                foreach (var status in Enum.GetValues<ResourceStatus>())
                {
                    _statusFilter.Items.Add(new StatusFilterOption(status, status.ToString()));
                }
                _statusFilter.DisplayMember = nameof(StatusFilterOption.Name);
            }

            _statusFilter.SelectedItem = _statusFilter.Items
                .Cast<StatusFilterOption>()
                .FirstOrDefault(x => x.Status == selectedStatus) ?? _statusFilter.Items[0];
        }
        finally
        {
            _suppressFilterEvents = false;
        }
    }

    /// <summary>Loads the current page and converts unexpected failures into a recoverable UI error.</summary>
    private async Task SafeLoadPageAsync()
    {
        try
        {
            await LoadPageAsync().ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Ressourcen laden");
        }
    }

    private async Task LoadPageAsync()
    {
        var providerId = (_providerFilter.SelectedItem as ProviderFilterOption)?.Id;
        var status = (_statusFilter.SelectedItem as StatusFilterOption)?.Status;
        var result = await _resourceService.SearchAsync(new ResourceSearchCriteria(
            _searchBox.Text,
            providerId,
            null,
            status,
            null,
            _includeArchived.Checked,
            _currentPage,
            PageSize)).ConfigureAwait(true);

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
        _pageLabel.Text = $"Seite {result.PageNumber} / {result.TotalPages} · {result.TotalCount} Ressourcen";
        _previousButton.Enabled = result.PageNumber > 1;
        _nextButton.Enabled = result.PageNumber < result.TotalPages;
    }

    private ResourceListItemDto? SelectedResource => _grid.CurrentRow?.DataBoundItem as ResourceListItemDto;

    private async Task CreateResourceAsync()
    {
        using var form = new ResourceEditForm(_resourceService, _providerService, _logger, null);
        if (form.ShowDialog(FindForm()) == DialogResult.OK)
        {
            await RefreshAsync().ConfigureAwait(true);
        }
    }

    private async Task EditSelectedAsync()
    {
        var selected = SelectedResource;
        if (selected is null)
        {
            return;
        }

        if (selected.Status == ResourceStatus.Archived)
        {
            MessageBox.Show(FindForm(), "Archivierte Ressourcen müssen vor dem Bearbeiten wiederhergestellt werden.", "Ressource", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new ResourceEditForm(_resourceService, _providerService, _logger, selected.Id);
        if (form.ShowDialog(FindForm()) == DialogResult.OK)
        {
            await RefreshAsync().ConfigureAwait(true);
        }
    }

    private async Task OpenSelectedUrlAsync()
    {
        var selected = SelectedResource;
        if (selected is null) return;
        try
        {
            await _resourceService.OpenUrlAsync(selected.Id).ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "URL öffnen");
        }
    }

    private async Task ArchiveSelectedAsync()
    {
        var selected = SelectedResource;
        if (selected is null || selected.Status == ResourceStatus.Archived) return;
        if (MessageBox.Show(FindForm(), $"'{selected.Title}' archivieren?\n\nDie Ressource bleibt historisch erhalten.", "Archivieren", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
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
            UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Ressource archivieren");
        }
    }

    private async Task RestoreSelectedAsync()
    {
        var selected = SelectedResource;
        if (selected is null || selected.Status != ResourceStatus.Archived) return;
        try
        {
            await _resourceService.RestoreAsync(selected.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Ressource wiederherstellen");
        }
    }

    private async Task ManageProvidersAsync()
    {
        using var form = new ProviderManagementForm(_providerService, _logger);
        form.ShowDialog(FindForm());
        await RefreshAsync().ConfigureAwait(true);
    }

    private sealed record ProviderFilterOption(Guid? Id, string Name);
    private sealed record StatusFilterOption(ResourceStatus? Status, string Name);
}
