using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Goals;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.Goals;
using SASD.LearningManager.WinForms.Forms;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Views;

/// <summary>Goal workspace linking desired outcomes to the skills needed to reach them.</summary>
public sealed class GoalsView : UserControl
{
    private readonly GoalService _goalService;
    private readonly SkillService _skillService;
    private readonly ILogger<GoalsView> _logger;
    private readonly TextBox _searchBox = new() { Width = 260, PlaceholderText = "Ziel, Motivation oder nächste Aktion" };
    private readonly ComboBox _statusFilter = new() { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _includeArchived = new() { Text = "Archiv anzeigen", AutoSize = true };
    private readonly DataGridView _grid = new();
    private readonly Label _pageLabel = new() { AutoSize = true };
    private readonly Button _previous = new() { Text = "‹ Zurück", AutoSize = true };
    private readonly Button _next = new() { Text = "Weiter ›", AutoSize = true };
    private int _page = 1;
    private const int PageSize = 100;

    public GoalsView(GoalService goalService, SkillService skillService, ILogger<GoalsView> logger)
    {
        _goalService = goalService;
        _skillService = skillService;
        _logger = logger;
        Dock = DockStyle.Fill;
        AutoScaleMode = AutoScaleMode.Dpi;
        ConfigureGrid();
        ConfigureFilters();
        Controls.Add(_grid);
        Controls.Add(BuildPaging());
        Controls.Add(BuildToolbar());
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditSelectedAsync().ConfigureAwait(true); };
    }

    public async Task RefreshAsync()
    {
        try { await LoadPageAsync().ConfigureAwait(true); }
        catch (Exception ex) { UiErrorHandler.Show(FindForm() ?? new Form(), ex, _logger, "Ziele laden"); }
    }

    private Control BuildToolbar()
    {
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, WrapContents = false, Padding = new Padding(0, 4, 0, 4) };
        var search = new Button { Text = "Suchen", AutoSize = true };
        var add = new Button { Text = "+ Ziel", AutoSize = true };
        var edit = new Button { Text = "Bearbeiten", AutoSize = true };
        var archive = new Button { Text = "Archivieren", AutoSize = true };
        var restore = new Button { Text = "Wiederherstellen", AutoSize = true };
        search.Click += async (_, _) => { _page = 1; await RefreshAsync().ConfigureAwait(true); };
        add.Click += async (_, _) => await CreateAsync().ConfigureAwait(true);
        edit.Click += async (_, _) => await EditSelectedAsync().ConfigureAwait(true);
        archive.Click += async (_, _) => await ArchiveSelectedAsync().ConfigureAwait(true);
        restore.Click += async (_, _) => await RestoreSelectedAsync().ConfigureAwait(true);
        bar.Controls.AddRange([
            new Label { Text = "Suche:", AutoSize = true, Margin = new Padding(0, 9, 3, 0) }, _searchBox, search,
            new Label { Text = "Status:", AutoSize = true, Margin = new Padding(10, 9, 3, 0) }, _statusFilter,
            _includeArchived, add, edit, archive, restore]);
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
        foreach (var status in Enum.GetValues<GoalStatus>().Where(static x => x != GoalStatus.Archived))
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
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ziel", DataPropertyName = nameof(GoalListItemDto.Title), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 250 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Typ", DataPropertyName = nameof(GoalListItemDto.Type), Width = 105 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = nameof(GoalListItemDto.Status), Width = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Priorität", DataPropertyName = nameof(GoalListItemDto.Priority), Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Zieldatum", DataPropertyName = nameof(GoalListItemDto.TargetDate), Width = 95 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Skills", DataPropertyName = nameof(GoalListItemDto.SkillCount), Width = 60 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nächste Aktion", DataPropertyName = nameof(GoalListItemDto.NextActionText), Width = 240 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fällig", DataPropertyName = nameof(GoalListItemDto.NextActionDueDate), Width = 95 });
    }

    private async Task LoadPageAsync()
    {
        var status = (_statusFilter.SelectedItem as StatusOption)?.Status;
        var result = await _goalService.SearchAsync(new GoalSearchCriteria(
            string.IsNullOrWhiteSpace(_searchBox.Text) ? null : _searchBox.Text.Trim(), status,
            _includeArchived.Checked, _page, PageSize)).ConfigureAwait(true);
        if (_page > 1 && result.Items.Count == 0 && result.TotalCount > 0)
        {
            _page--;
            await LoadPageAsync().ConfigureAwait(true);
            return;
        }

        _grid.DataSource = result.Items.ToList();
        _pageLabel.Text = $"Seite {result.PageNumber} / {Math.Max(1, result.TotalPages)}   ({result.TotalCount} Ziele)";
        _previous.Enabled = result.PageNumber > 1;
        _next.Enabled = result.PageNumber < result.TotalPages;
    }

    private async Task CreateAsync()
    {
        using var form = new GoalEditForm(_goalService, _skillService, _logger);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task EditSelectedAsync()
    {
        if (Selected is not { } item) return;
        if (item.Status == GoalStatus.Archived)
        {
            MessageBox.Show(FindForm(), "Archivierte Ziele müssen vor dem Bearbeiten wiederhergestellt werden.", "Lernziel", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var form = new GoalEditForm(_goalService, _skillService, _logger, item.Id);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task ArchiveSelectedAsync()
    {
        if (Selected is not { } item || item.Status == GoalStatus.Archived) return;
        try
        {
            await _goalService.ArchiveAsync(item.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex) { UiErrorHandler.Show(FindForm() ?? new Form(), ex, _logger, "Lernziel archivieren"); }
    }

    private async Task RestoreSelectedAsync()
    {
        if (Selected is not { Status: GoalStatus.Archived } item) return;
        try
        {
            await _goalService.RestoreAsync(item.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex) { UiErrorHandler.Show(FindForm() ?? new Form(), ex, _logger, "Lernziel wiederherstellen"); }
    }

    private GoalListItemDto? Selected => _grid.CurrentRow?.DataBoundItem as GoalListItemDto;
    private sealed record StatusOption(GoalStatus? Status, string Text);
}
