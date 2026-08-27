using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Domain.Providers;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Provides the Milestone-1 provider CRUD surface.</summary>
public sealed class ProviderManagementForm : Form
{
    private readonly ProviderService _service;
    private readonly ILogger _logger;
    private readonly DataGridView _grid = new();
    private readonly CheckBox _includeArchived = new() { Text = "Archiv anzeigen", AutoSize = true };

    public ProviderManagementForm(ProviderService service, ILogger logger)
    {
        _service = service;
        _logger = logger;
        Text = "Provider verwalten";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(800, 520);
        MinimumSize = new Size(650, 400);
        Font = new Font("Segoe UI", 9F);
        AutoScaleMode = AutoScaleMode.Dpi;

        ConfigureGrid();
        Controls.Add(_grid);
        Controls.Add(BuildToolbar());
        Load += async (_, _) => await RefreshAsync().ConfigureAwait(true);
        _includeArchived.CheckedChanged += async (_, _) => await RefreshAsync().ConfigureAwait(true);
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditAsync().ConfigureAwait(true); };
    }

    private Control BuildToolbar()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(8, 5, 8, 5), WrapContents = false };
        var add = new Button { Text = "+ Provider", AutoSize = true };
        add.Click += async (_, _) => await CreateAsync().ConfigureAwait(true);
        var edit = new Button { Text = "Bearbeiten", AutoSize = true };
        edit.Click += async (_, _) => await EditAsync().ConfigureAwait(true);
        var archive = new Button { Text = "Archivieren", AutoSize = true };
        archive.Click += async (_, _) => await ArchiveAsync().ConfigureAwait(true);
        var restore = new Button { Text = "Wiederherstellen", AutoSize = true };
        restore.Click += async (_, _) => await RestoreAsync().ConfigureAwait(true);
        panel.Controls.AddRange([add, edit, archive, restore, _includeArchived]);
        return panel;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", DataPropertyName = nameof(ProviderListItemDto.Name), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Typ", DataPropertyName = nameof(ProviderListItemDto.Type), Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = nameof(ProviderListItemDto.Status), Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Website", DataPropertyName = nameof(ProviderListItemDto.WebsiteUrl), Width = 260 });
    }

    private ProviderListItemDto? Selected => _grid.CurrentRow?.DataBoundItem as ProviderListItemDto;

    private async Task RefreshAsync()
    {
        try
        {
            _grid.DataSource = (await _service.ListAsync(_includeArchived.Checked).ConfigureAwait(true)).ToList();
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Provider laden");
        }
    }

    private async Task CreateAsync()
    {
        using var form = new ProviderEditForm(_service, _logger, null);
        if (form.ShowDialog(this) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task EditAsync()
    {
        if (Selected is not { } selected) return;
        if (selected.Status == ProviderStatus.Archived)
        {
            MessageBox.Show(this, "Archivierte Provider müssen zunächst wiederhergestellt werden.", "Provider", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var form = new ProviderEditForm(_service, _logger, selected.Id);
        if (form.ShowDialog(this) == DialogResult.OK) await RefreshAsync().ConfigureAwait(true);
    }

    private async Task ArchiveAsync()
    {
        if (Selected is not { } selected || selected.Status == ProviderStatus.Archived) return;
        if (MessageBox.Show(this, $"Provider '{selected.Name}' archivieren? Bestehende Resources behalten ihren historischen Bezug.", "Provider archivieren", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            await _service.ArchiveAsync(selected.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception exception) { UiErrorHandler.Show(this, exception, _logger, "Provider archivieren"); }
    }

    private async Task RestoreAsync()
    {
        if (Selected is not { } selected || selected.Status != ProviderStatus.Archived) return;
        try
        {
            await _service.RestoreAsync(selected.Id).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception exception) { UiErrorHandler.Show(this, exception, _logger, "Provider wiederherstellen"); }
    }
}
