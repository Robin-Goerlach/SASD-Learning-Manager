using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Competencies;
using SASD.LearningManager.Domain.Competencies;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Management workspace for competency areas and topics used by skill classification.</summary>
public sealed class CompetencyCatalogForm : Form
{
    private readonly CompetencyCatalogService _service;
    private readonly ILogger _logger;
    private readonly DataGridView _areasGrid = CreateGrid();
    private readonly DataGridView _topicsGrid = CreateGrid();
    private readonly CheckBox _includeArchived = new() { Text = "Archivierte anzeigen", AutoSize = true };

    public CompetencyCatalogForm(CompetencyCatalogService service, ILogger logger)
    {
        _service = service;
        _logger = logger;
        Text = "Kompetenzkatalog – Bereiche und Topics";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(850, 600);
        Size = new Size(1000, 700);
        AutoScaleMode = AutoScaleMode.Dpi;
        ConfigureColumns();
        Controls.Add(BuildLayout());
        Load += async (_, _) => await RefreshAsync().ConfigureAwait(true);
        _includeArchived.CheckedChanged += async (_, _) => await RefreshAsync().ConfigureAwait(true);
        _areasGrid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditAreaAsync().ConfigureAwait(true); };
        _topicsGrid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditTopicAsync().ConfigureAwait(true); };
    }

    private Control BuildLayout()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildAreaTab());
        tabs.TabPages.Add(BuildTopicTab());
        var outer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, FlowDirection = FlowDirection.RightToLeft };
        top.Controls.Add(_includeArchived);
        outer.Controls.Add(tabs);
        outer.Controls.Add(top);
        return outer;
    }

    private TabPage BuildAreaTab()
    {
        var tab = new TabPage("Kompetenzbereiche");
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42 };
        var add = new Button { Text = "+ Bereich", AutoSize = true };
        var edit = new Button { Text = "Bearbeiten", AutoSize = true };
        var archive = new Button { Text = "Archivieren", AutoSize = true };
        var restore = new Button { Text = "Wiederherstellen", AutoSize = true };
        add.Click += async (_, _) => await CreateAreaAsync().ConfigureAwait(true);
        edit.Click += async (_, _) => await EditAreaAsync().ConfigureAwait(true);
        archive.Click += async (_, _) => await ChangeAreaArchiveAsync(true).ConfigureAwait(true);
        restore.Click += async (_, _) => await ChangeAreaArchiveAsync(false).ConfigureAwait(true);
        bar.Controls.AddRange([add, edit, archive, restore]);
        tab.Controls.Add(_areasGrid);
        tab.Controls.Add(bar);
        return tab;
    }

    private TabPage BuildTopicTab()
    {
        var tab = new TabPage("Topics");
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42 };
        var add = new Button { Text = "+ Topic", AutoSize = true };
        var edit = new Button { Text = "Bearbeiten", AutoSize = true };
        var archive = new Button { Text = "Archivieren", AutoSize = true };
        var restore = new Button { Text = "Wiederherstellen", AutoSize = true };
        add.Click += async (_, _) => await CreateTopicAsync().ConfigureAwait(true);
        edit.Click += async (_, _) => await EditTopicAsync().ConfigureAwait(true);
        archive.Click += async (_, _) => await ChangeTopicArchiveAsync(true).ConfigureAwait(true);
        restore.Click += async (_, _) => await ChangeTopicArchiveAsync(false).ConfigureAwait(true);
        bar.Controls.AddRange([add, edit, archive, restore]);
        tab.Controls.Add(_topicsGrid);
        tab.Controls.Add(bar);
        return tab;
    }

    private static DataGridView CreateGrid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        RowHeadersVisible = false,
        BackgroundColor = SystemColors.Window
    };

    private void ConfigureColumns()
    {
        _areasGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", DataPropertyName = nameof(CompetencyAreaListItemDto.Name), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _areasGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = nameof(CompetencyAreaListItemDto.Status), Width = 110 });
        _topicsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Topic", DataPropertyName = nameof(TopicListItemDto.Name), Width = 220 });
        _topicsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kompetenzbereiche", DataPropertyName = nameof(TopicListItemDto.CompetencyAreas), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _topicsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = nameof(TopicListItemDto.Status), Width = 110 });
    }

    private async Task RefreshAsync()
    {
        try
        {
            _areasGrid.DataSource = (await _service.ListAreasAsync(_includeArchived.Checked).ConfigureAwait(true)).ToList();
            _topicsGrid.DataSource = (await _service.ListTopicsAsync(_includeArchived.Checked).ConfigureAwait(true)).ToList();
        }
        catch (Exception ex) { UiErrorHandler.Show(this, ex, _logger, "Kompetenzkatalog laden"); }
    }

    private async Task CreateAreaAsync()
    {
        using var form = new CompetencyAreaEditForm(_service, _logger);
        if (form.ShowDialog(this) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task EditAreaAsync()
    {
        if (_areasGrid.CurrentRow?.DataBoundItem is not CompetencyAreaListItemDto item) return;
        using var form = new CompetencyAreaEditForm(_service, _logger, item.Id);
        if (form.ShowDialog(this) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task ChangeAreaArchiveAsync(bool archive)
    {
        if (_areasGrid.CurrentRow?.DataBoundItem is not CompetencyAreaListItemDto item) return;
        try
        {
            if (archive) await _service.ArchiveAreaAsync(item.Id).ConfigureAwait(true);
            else await _service.RestoreAreaAsync(item.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex) { UiErrorHandler.Show(this, ex, _logger, archive ? "Kompetenzbereich archivieren" : "Kompetenzbereich wiederherstellen"); }
    }

    private async Task CreateTopicAsync()
    {
        using var form = new TopicEditForm(_service, _logger);
        if (form.ShowDialog(this) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task EditTopicAsync()
    {
        if (_topicsGrid.CurrentRow?.DataBoundItem is not TopicListItemDto item) return;
        using var form = new TopicEditForm(_service, _logger, item.Id);
        if (form.ShowDialog(this) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task ChangeTopicArchiveAsync(bool archive)
    {
        if (_topicsGrid.CurrentRow?.DataBoundItem is not TopicListItemDto item) return;
        try
        {
            if (archive) await _service.ArchiveTopicAsync(item.Id).ConfigureAwait(true);
            else await _service.RestoreTopicAsync(item.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex) { UiErrorHandler.Show(this, ex, _logger, archive ? "Topic archivieren" : "Topic wiederherstellen"); }
    }
}
