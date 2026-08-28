using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Goals;
using SASD.LearningManager.Application.LearningPaths;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.LearningPaths;
using SASD.LearningManager.WinForms.Forms;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Views;

/// <summary>
/// Milestone-4 workspace for learning paths. The left side manages path metadata while the right
/// side presents a real hierarchical TreeView with explicit node actions and relationship editing.
/// </summary>
public sealed class LearningPathsView : UserControl
{
    private readonly LearningPathService _pathService;
    private readonly GoalService _goalService;
    private readonly SkillService _skillService;
    private readonly ResourceService _resourceService;
    private readonly ILogger<LearningPathsView> _logger;
    private readonly TextBox _search = new() { Width = 210, PlaceholderText = "Learning Path suchen" };
    private readonly ComboBox _status = new() { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _includeArchivedPaths = new() { Text = "Path-Archiv", AutoSize = true };
    private readonly CheckBox _includeArchivedNodes = new() { Text = "Knoten-Archiv", AutoSize = true };
    private readonly DataGridView _pathGrid = new();
    private readonly TreeView _tree = new() { Dock = DockStyle.Fill, HideSelection = false };
    private readonly Label _progressLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
    private readonly Label _nodeInfo = new() { AutoSize = true };
    private readonly Label _pageLabel = new() { AutoSize = true };
    private readonly Button _previous = new() { Text = "‹ Zurück", AutoSize = true };
    private readonly Button _next = new() { Text = "Weiter ›", AutoSize = true };
    private int _page = 1;
    private const int PageSize = 100;

    public LearningPathsView(LearningPathService pathService, GoalService goalService, SkillService skillService,
        ResourceService resourceService, ILogger<LearningPathsView> logger)
    {
        _pathService = pathService;
        _goalService = goalService;
        _skillService = skillService;
        _resourceService = resourceService;
        _logger = logger;
        Dock = DockStyle.Fill;
        AutoScaleMode = AutoScaleMode.Dpi;
        ConfigurePathGrid();
        ConfigureFilters();
        Controls.Add(BuildSplit());
        _pathGrid.SelectionChanged += async (_, _) => await RefreshTreeAsync().ConfigureAwait(true);
        _tree.AfterSelect += (_, _) => UpdateNodeInfo();
        _tree.NodeMouseDoubleClick += async (_, _) => await EditNodeAsync().ConfigureAwait(true);
    }

    public async Task RefreshAsync()
    {
        try
        {
            await LoadPathsAsync().ConfigureAwait(true);
            await RefreshTreeAsync().ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Learning Paths laden");
        }
    }

    private Control BuildSplit()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            Size = new Size(1150, 650),
            SplitterDistance = 470,
            Panel1MinSize = 320,
            Panel2MinSize = 420
        };
        split.Panel1.Controls.Add(_pathGrid);
        split.Panel1.Controls.Add(BuildPathPaging());
        split.Panel1.Controls.Add(BuildPathToolbar());
        split.Panel2.Controls.Add(_tree);
        split.Panel2.Controls.Add(BuildNodeStatus());
        split.Panel2.Controls.Add(BuildNodeToolbar());
        return split;
    }

    private Control BuildPathToolbar()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 86, WrapContents = true, Padding = new Padding(0, 4, 0, 4) };
        var searchButton = new Button { Text = "Suchen", AutoSize = true };
        var add = new Button { Text = "+ Path", AutoSize = true };
        var edit = new Button { Text = "Bearbeiten", AutoSize = true };
        var archive = new Button { Text = "Archivieren", AutoSize = true };
        var restore = new Button { Text = "Wiederherstellen", AutoSize = true };
        searchButton.Click += async (_, _) => { _page = 1; await RefreshAsync().ConfigureAwait(true); };
        add.Click += async (_, _) => await CreatePathAsync().ConfigureAwait(true);
        edit.Click += async (_, _) => await EditPathAsync().ConfigureAwait(true);
        archive.Click += async (_, _) => await ArchivePathAsync().ConfigureAwait(true);
        restore.Click += async (_, _) => await RestorePathAsync().ConfigureAwait(true);
        panel.Controls.AddRange([
            new Label { Text = "Suche:", AutoSize = true, Margin = new Padding(0, 8, 3, 0) }, _search, searchButton,
            new Label { Text = "Status:", AutoSize = true, Margin = new Padding(8, 8, 3, 0) }, _status,
            _includeArchivedPaths, add, edit, archive, restore]);
        return panel;
    }


    private Control BuildPathPaging()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 38, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 5, 0, 0) };
        _previous.Click += async (_, _) => { if (_page > 1) { _page--; await RefreshAsync().ConfigureAwait(true); } };
        _next.Click += async (_, _) => { _page++; await RefreshAsync().ConfigureAwait(true); };
        panel.Controls.Add(_next);
        panel.Controls.Add(_pageLabel);
        panel.Controls.Add(_previous);
        return panel;
    }

    private Control BuildNodeToolbar()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 76, WrapContents = true, Padding = new Padding(0, 4, 0, 4) };
        var addRoot = new Button { Text = "+ Wurzel", AutoSize = true };
        var addChild = new Button { Text = "+ Unterknoten", AutoSize = true };
        var edit = new Button { Text = "Bearbeiten", AutoSize = true };
        var up = new Button { Text = "↑ Hoch", AutoSize = true };
        var down = new Button { Text = "↓ Runter", AutoSize = true };
        var relations = new Button { Text = "Beziehungen …", AutoSize = true };
        var archive = new Button { Text = "Teilbaum archivieren", AutoSize = true };
        var restore = new Button { Text = "Knoten wiederherstellen", AutoSize = true };
        addRoot.Click += async (_, _) => await CreateNodeAsync(null).ConfigureAwait(true);
        addChild.Click += async (_, _) => await CreateNodeAsync(SelectedNode?.Id).ConfigureAwait(true);
        edit.Click += async (_, _) => await EditNodeAsync().ConfigureAwait(true);
        up.Click += async (_, _) => await MoveNodeAsync(upwards: true).ConfigureAwait(true);
        down.Click += async (_, _) => await MoveNodeAsync(upwards: false).ConfigureAwait(true);
        relations.Click += async (_, _) => await ManageRelationsAsync().ConfigureAwait(true);
        archive.Click += async (_, _) => await ArchiveNodeAsync().ConfigureAwait(true);
        restore.Click += async (_, _) => await RestoreNodeAsync().ConfigureAwait(true);
        _includeArchivedNodes.CheckedChanged += async (_, _) => await RefreshTreeAsync().ConfigureAwait(true);
        panel.Controls.AddRange([addRoot, addChild, edit, up, down, relations, archive, restore, _includeArchivedNodes]);
        return panel;
    }

    private Control BuildNodeStatus()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(4, 9, 4, 4) };
        panel.Controls.Add(_progressLabel);
        panel.Controls.Add(new Label { Text = "   |   ", AutoSize = true });
        panel.Controls.Add(_nodeInfo);
        return panel;
    }

    private void ConfigureFilters()
    {
        _status.Items.Add(new StatusOption(null, "Alle"));
        foreach (var value in Enum.GetValues<LearningPathStatus>().Where(static value => value != LearningPathStatus.Archived))
        {
            _status.Items.Add(new StatusOption(value, value.ToString()));
        }
        _status.DisplayMember = nameof(StatusOption.Text);
        _status.SelectedIndex = 0;
        _status.SelectedIndexChanged += async (_, _) => { _page = 1; await RefreshAsync().ConfigureAwait(true); };
        _includeArchivedPaths.CheckedChanged += async (_, _) => { _page = 1; await RefreshAsync().ConfigureAwait(true); };
        _search.KeyDown += async (_, args) =>
        {
            if (args.KeyCode == Keys.Enter)
            {
                _page = 1;
                args.SuppressKeyPress = true;
                await RefreshAsync().ConfigureAwait(true);
            }
        };
    }

    private void ConfigurePathGrid()
    {
        _pathGrid.Dock = DockStyle.Fill;
        _pathGrid.ReadOnly = true;
        _pathGrid.AllowUserToAddRows = false;
        _pathGrid.AllowUserToDeleteRows = false;
        _pathGrid.AutoGenerateColumns = false;
        _pathGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _pathGrid.MultiSelect = false;
        _pathGrid.RowHeadersVisible = false;
        _pathGrid.BackgroundColor = SystemColors.Window;
        _pathGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Learning Path", DataPropertyName = nameof(LearningPathListItemDto.Title), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 180 });
        _pathGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = nameof(LearningPathListItemDto.Status), Width = 90 });
        _pathGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fortschritt", DataPropertyName = nameof(LearningPathListItemDto.CoreCompletionPercent), Width = 90 });
        _pathGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nodes", DataPropertyName = nameof(LearningPathListItemDto.NodeCount), Width = 60 });
        _pathGrid.CellFormatting += (_, args) =>
        {
            if (args.ColumnIndex == 2)
            {
                args.Value = args.Value is decimal percent ? $"{percent:0.#} %" : "–";
                args.FormattingApplied = true;
            }
        };
        _pathGrid.CellDoubleClick += async (_, args) => { if (args.RowIndex >= 0) await EditPathAsync().ConfigureAwait(true); };
    }

    private async Task LoadPathsAsync()
    {
        var selectedId = SelectedPath?.Id;
        var status = (_status.SelectedItem as StatusOption)?.Status;
        var result = await _pathService.SearchAsync(new LearningPathSearchCriteria(
            string.IsNullOrWhiteSpace(_search.Text) ? null : _search.Text.Trim(), status,
            _includeArchivedPaths.Checked, _page, PageSize)).ConfigureAwait(true);
        if (_page > 1 && result.Items.Count == 0 && result.TotalCount > 0)
        {
            _page--;
            await LoadPathsAsync().ConfigureAwait(true);
            return;
        }

        _pathGrid.DataSource = result.Items.ToList();
        _pageLabel.Text = $"Seite {result.PageNumber} / {Math.Max(1, result.TotalPages)}   ({result.TotalCount} Paths)";
        _previous.Enabled = result.PageNumber > 1;
        _next.Enabled = result.PageNumber < result.TotalPages;
        if (selectedId is not null)
        {
            foreach (DataGridViewRow row in _pathGrid.Rows)
            {
                if (row.DataBoundItem is LearningPathListItemDto item && item.Id == selectedId)
                {
                    row.Selected = true;
                    _pathGrid.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }
    }

    private async Task RefreshTreeAsync()
    {
        _tree.BeginUpdate();
        try
        {
            _tree.Nodes.Clear();
            _progressLabel.Text = "Kein Learning Path ausgewählt";
            _nodeInfo.Text = string.Empty;
            if (SelectedPath is not { } path)
            {
                return;
            }

            var nodes = await _pathService.ListNodesAsync(path.Id, _includeArchivedNodes.Checked).ConfigureAwait(true);
            AddChildren(_tree.Nodes, nodes, null);
            _tree.ExpandAll();
            var detail = await _pathService.GetDetailAsync(path.Id).ConfigureAwait(true);
            if (detail is not null)
            {
                _progressLabel.Text = $"Core: {(detail.CoreCompletionPercent is null ? "–" : $"{detail.CoreCompletionPercent:0.#} %")}   Pflicht {detail.RequiredCompleted}/{detail.RequiredTotal}   Optional {detail.OptionalCompleted}/{detail.OptionalTotal}";
            }
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Learning-Path-Baum laden");
        }
        finally
        {
            _tree.EndUpdate();
        }
    }

    private static void AddChildren(TreeNodeCollection collection, IReadOnlyList<LearningPathNodeListItemDto> nodes, Guid? parentId)
    {
        foreach (var item in nodes.Where(node => node.ParentNodeId == parentId)
                     .OrderBy(static node => node.SortOrder).ThenBy(static node => node.Title, StringComparer.OrdinalIgnoreCase))
        {
            var prefix = item.Status switch
            {
                LearningPathNodeStatus.Completed => "✓ ",
                LearningPathNodeStatus.Active => "▶ ",
                LearningPathNodeStatus.Skipped => "↷ ",
                LearningPathNodeStatus.Archived => "[A] ",
                _ => "○ "
            };
            var suffix = item.IsRequired ? string.Empty : "  (optional)";
            var treeNode = new TreeNode(prefix + item.Title + suffix) { Tag = item };
            collection.Add(treeNode);
            AddChildren(treeNode.Nodes, nodes, item.Id);
        }
    }

    private async Task CreatePathAsync()
    {
        using var form = new LearningPathEditForm(_pathService, _goalService, _logger);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task EditPathAsync()
    {
        if (SelectedPath is not { } path) return;
        if (path.Status == LearningPathStatus.Archived)
        {
            MessageBox.Show(FindForm(), "Archivierte Learning Paths müssen zuerst wiederhergestellt werden.", "Learning Path", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var form = new LearningPathEditForm(_pathService, _goalService, _logger, path.Id);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task ArchivePathAsync()
    {
        if (SelectedPath is not { } path || path.Status == LearningPathStatus.Archived) return;
        try
        {
            await _pathService.ArchiveAsync(path.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception exception) { UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Learning Path archivieren"); }
    }

    private async Task RestorePathAsync()
    {
        if (SelectedPath is not { Status: LearningPathStatus.Archived } path) return;
        try
        {
            await _pathService.RestoreAsync(path.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception exception) { UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Learning Path wiederherstellen"); }
    }

    private async Task CreateNodeAsync(Guid? parentId)
    {
        if (SelectedPath is not { } path) return;
        if (path.Status == LearningPathStatus.Archived)
        {
            MessageBox.Show(FindForm(), "Der Learning Path ist archiviert.", "Learning Path", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var form = new LearningPathNodeEditForm(_pathService, _skillService, _resourceService, _logger, path.Id, initialParentId: parentId);
        if (form.ShowDialog(FindForm()) == DialogResult.OK)
        {
            await RefreshAsync().ConfigureAwait(true);
        }
    }

    private async Task EditNodeAsync()
    {
        if (SelectedPath is not { } path || SelectedNode is not { } node) return;
        if (node.Status == LearningPathNodeStatus.Archived)
        {
            MessageBox.Show(FindForm(), "Archivierte Knoten müssen zuerst wiederhergestellt werden.", "Path-Knoten", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var form = new LearningPathNodeEditForm(_pathService, _skillService, _resourceService, _logger, path.Id, node.Id);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task MoveNodeAsync(bool upwards)
    {
        if (SelectedNode is not { } node) return;
        try
        {
            if (upwards) await _pathService.MoveNodeUpAsync(node.Id).ConfigureAwait(true);
            else await _pathService.MoveNodeDownAsync(node.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception exception) { UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Path-Knoten verschieben"); }
    }

    private async Task ArchiveNodeAsync()
    {
        if (SelectedNode is not { } node || node.Status == LearningPathNodeStatus.Archived) return;
        if (MessageBox.Show(FindForm(), "Den ausgewählten Knoten und alle Unterknoten archivieren?", "Teilbaum archivieren",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            await _pathService.ArchiveNodeSubtreeAsync(node.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception exception) { UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Path-Teilbaum archivieren"); }
    }

    private async Task RestoreNodeAsync()
    {
        if (SelectedNode is not { Status: LearningPathNodeStatus.Archived } node) return;
        try
        {
            await _pathService.RestoreNodeAsync(node.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception exception) { UiErrorHandler.Show(FindForm() ?? new Form(), exception, _logger, "Path-Knoten wiederherstellen"); }
    }

    private async Task ManageRelationsAsync()
    {
        if (SelectedPath is not { } path || SelectedNode is not { } node || node.Status == LearningPathNodeStatus.Archived) return;
        using var form = new LearningPathRelationsForm(_pathService, _logger, path.Id, node.Id);
        form.ShowDialog(FindForm());
        await RefreshTreeAsync().ConfigureAwait(true);
    }

    private void UpdateNodeInfo()
    {
        _nodeInfo.Text = SelectedNode is { } node
            ? $"{node.Type} | {(node.IsRequired ? "Pflicht" : "Optional")} | {node.Status}"
            : string.Empty;
    }

    private LearningPathListItemDto? SelectedPath => _pathGrid.CurrentRow?.DataBoundItem as LearningPathListItemDto;
    private LearningPathNodeListItemDto? SelectedNode => _tree.SelectedNode?.Tag as LearningPathNodeListItemDto;
    private sealed record StatusOption(LearningPathStatus? Status, string Text);
}
