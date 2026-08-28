using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Common;
using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.Skills;
using SASD.LearningManager.WinForms.Forms;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Views;

/// <summary>Milestone-3 skill workspace with target gaps, assessments and taxonomy management.</summary>
public sealed class SkillsView : UserControl
{
    private readonly SkillService _skillService;
    private readonly CompetencyCatalogService _catalogService;
    private readonly ILogger<SkillsView> _logger;
    private readonly TextBox _searchBox = new() { Width = 260, PlaceholderText = "Skill suchen" };
    private readonly ComboBox _statusFilter = new() { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _includeArchived = new() { Text = "Archiv anzeigen", AutoSize = true };
    private readonly DataGridView _grid = new();
    private readonly Label _pageLabel = new() { AutoSize = true };
    private readonly Button _previous = new() { Text = "‹ Zurück", AutoSize = true };
    private readonly Button _next = new() { Text = "Weiter ›", AutoSize = true };
    private int _page = 1;
    private const int PageSize = 100;

    public SkillsView(SkillService skillService, CompetencyCatalogService catalogService, ILogger<SkillsView> logger)
    {
        _skillService = skillService;
        _catalogService = catalogService;
        _logger = logger;
        Dock = DockStyle.Fill;
        AutoScaleMode = AutoScaleMode.Dpi;
        ConfigureGrid();
        Controls.Add(_grid);
        Controls.Add(BuildPaging());
        Controls.Add(BuildToolbar());
        ConfigureFilters();
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditSelectedAsync().ConfigureAwait(true); };
    }

    public async Task RefreshAsync()
    {
        try { await LoadPageAsync().ConfigureAwait(true); }
        catch (Exception ex) { UiErrorHandler.Show(FindForm() ?? new Form(), ex, _logger, "Skills laden"); }
    }

    private Control BuildToolbar()
    {
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, WrapContents = false, Padding = new Padding(0, 4, 0, 4) };
        var search = new Button { Text = "Suchen", AutoSize = true };
        var add = new Button { Text = "+ Skill", AutoSize = true };
        var edit = new Button { Text = "Bearbeiten", AutoSize = true };
        var assess = new Button { Text = "Bewerten …", AutoSize = true };
        var archive = new Button { Text = "Archivieren", AutoSize = true };
        var restore = new Button { Text = "Wiederherstellen", AutoSize = true };
        var catalog = new Button { Text = "Kompetenzkatalog …", AutoSize = true };
        search.Click += async (_, _) => { _page = 1; await RefreshAsync().ConfigureAwait(true); };
        add.Click += async (_, _) => await CreateAsync().ConfigureAwait(true);
        edit.Click += async (_, _) => await EditSelectedAsync().ConfigureAwait(true);
        assess.Click += async (_, _) => await AssessSelectedAsync().ConfigureAwait(true);
        archive.Click += async (_, _) => await ArchiveSelectedAsync().ConfigureAwait(true);
        restore.Click += async (_, _) => await RestoreSelectedAsync().ConfigureAwait(true);
        catalog.Click += async (_, _) => await ManageCatalogAsync().ConfigureAwait(true);
        bar.Controls.AddRange([
            new Label { Text = "Suche:", AutoSize = true, Margin = new Padding(0, 9, 3, 0) }, _searchBox, search,
            new Label { Text = "Status:", AutoSize = true, Margin = new Padding(10, 9, 3, 0) }, _statusFilter,
            _includeArchived, add, edit, assess, archive, restore, catalog]);
        return bar;
    }

    private Control BuildPaging()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 5, 0, 0) };
        _previous.Click += async (_, _) => { if (_page > 1) { _page--; await RefreshAsync().ConfigureAwait(true); } };
        _next.Click += async (_, _) => { _page++; await RefreshAsync().ConfigureAwait(true); };
        panel.Controls.Add(_next);
        panel.Controls.Add(_pageLabel);
        panel.Controls.Add(_previous);
        return panel;
    }

    private void ConfigureFilters()
    {
        _statusFilter.Items.Add(new StatusOption(null, "Alle"));
        foreach (var status in Enum.GetValues<SkillStatus>().Where(static x => x != SkillStatus.Archived))
            _statusFilter.Items.Add(new StatusOption(status, status.ToString()));
        _statusFilter.DisplayMember = nameof(StatusOption.Text);
        _statusFilter.SelectedIndex = 0;
        _statusFilter.SelectedIndexChanged += async (_, _) => { _page = 1; await RefreshAsync().ConfigureAwait(true); };
        _includeArchived.CheckedChanged += async (_, _) => { _page = 1; await RefreshAsync().ConfigureAwait(true); };
        _searchBox.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) { _page = 1; await RefreshAsync().ConfigureAwait(true); e.SuppressKeyPress = true; } };
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
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Skill", DataPropertyName = nameof(SkillListItemDto.Name), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 240 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ist", DataPropertyName = nameof(SkillListItemDto.CurrentLevel), Width = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ziel", DataPropertyName = nameof(SkillListItemDto.TargetLevel), Width = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Gap", DataPropertyName = nameof(SkillListItemDto.Gap), Width = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kompetenzbereiche", DataPropertyName = nameof(SkillListItemDto.CompetencyAreas), Width = 190 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Topics", DataPropertyName = nameof(SkillListItemDto.Topics), Width = 190 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = nameof(SkillListItemDto.Status), Width = 100 });
        _grid.CellFormatting += (_, e) =>
        {
            if (e.ColumnIndex is 1 or 2 && e.Value is null)
            {
                e.Value = "–";
                e.FormattingApplied = true;
            }
            else if (e.ColumnIndex == 3)
            {
                e.Value = e.Value is int gap ? (gap > 0 ? $"+{gap}" : gap.ToString(System.Globalization.CultureInfo.InvariantCulture)) : "–";
                e.FormattingApplied = true;
            }
        };
    }

    private async Task LoadPageAsync()
    {
        var status = (_statusFilter.SelectedItem as StatusOption)?.Status;
        var result = await _skillService.SearchAsync(new SkillSearchCriteria(
            string.IsNullOrWhiteSpace(_searchBox.Text) ? null : _searchBox.Text.Trim(), status,
            _includeArchived.Checked, _page, PageSize)).ConfigureAwait(true);
        if (_page > 1 && result.Items.Count == 0 && result.TotalCount > 0)
        {
            _page--;
            await LoadPageAsync().ConfigureAwait(true);
            return;
        }

        _grid.DataSource = result.Items.ToList();
        _pageLabel.Text = $"Seite {result.PageNumber} / {Math.Max(1, result.TotalPages)}   ({result.TotalCount} Skills)";
        _previous.Enabled = result.PageNumber > 1;
        _next.Enabled = result.PageNumber < result.TotalPages;
    }

    private async Task CreateAsync()
    {
        using var form = new SkillEditForm(_skillService, _catalogService, _logger);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task EditSelectedAsync()
    {
        if (Selected is not { } item) return;
        if (item.Status == SkillStatus.Archived)
        {
            MessageBox.Show(FindForm(), "Archivierte Skills müssen vor dem Bearbeiten wiederhergestellt werden.", "Skill", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var form = new SkillEditForm(_skillService, _catalogService, _logger, item.Id);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task AssessSelectedAsync()
    {
        if (Selected is not { } item) return;
        if (item.Status == SkillStatus.Archived)
        {
            MessageBox.Show(FindForm(), "Archivierte Skills müssen vor einer Bewertung wiederhergestellt werden.", "Skill", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var form = new SkillAssessmentForm(_skillService, _logger, item.Id);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task ArchiveSelectedAsync()
    {
        if (Selected is not { } item || item.Status == SkillStatus.Archived) return;
        try
        {
            await _skillService.ArchiveAsync(item.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex) { UiErrorHandler.Show(FindForm() ?? new Form(), ex, _logger, "Skill archivieren"); }
    }

    private async Task RestoreSelectedAsync()
    {
        if (Selected is not { Status: SkillStatus.Archived } item) return;
        try
        {
            await _skillService.RestoreAsync(item.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex) { UiErrorHandler.Show(FindForm() ?? new Form(), ex, _logger, "Skill wiederherstellen"); }
    }

    private async Task ManageCatalogAsync()
    {
        using var form = new CompetencyCatalogForm(_catalogService, _logger);
        form.ShowDialog(FindForm());
        await RefreshAsync().ConfigureAwait(true);
    }

    private SkillListItemDto? Selected => _grid.CurrentRow?.DataBoundItem as SkillListItemDto;
    private sealed record StatusOption(SkillStatus? Status, string Text);
}
