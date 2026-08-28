using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.LearningPaths;
using SASD.LearningManager.Domain.LearningPaths;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Small editor for non-hierarchical node relations such as Requires or AlternativeTo.</summary>
public sealed class LearningPathRelationsForm : Form
{
    private readonly LearningPathService _service;
    private readonly ILogger _logger;
    private readonly Guid _pathId;
    private readonly Guid _sourceNodeId;
    private readonly ComboBox _target = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly TextBox _note = new() { Width = 260, MaxLength = 2000 };
    private readonly DataGridView _grid = new();

    public LearningPathRelationsForm(LearningPathService service, ILogger logger, Guid pathId, Guid sourceNodeId)
    {
        _service = service;
        _logger = logger;
        _pathId = pathId;
        _sourceNodeId = sourceNodeId;
        Text = "Knoten-Beziehungen";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(800, 480);
        Size = new Size(920, 600);
        AutoScaleMode = AutoScaleMode.Dpi;
        _type.DataSource = Enum.GetValues<LearningPathNodeRelationType>();
        ConfigureGrid();
        Controls.Add(_grid);
        Controls.Add(BuildToolbar());
        Load += async (_, _) => await LoadAsync().ConfigureAwait(true);
    }

    private Control BuildToolbar()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 78, Padding = new Padding(8), WrapContents = true };
        var add = new Button { Text = "Beziehung hinzufügen", AutoSize = true };
        var remove = new Button { Text = "Ausgewählte entfernen", AutoSize = true };
        add.Click += async (_, _) => await AddAsync().ConfigureAwait(true);
        remove.Click += async (_, _) => await DeleteAsync().ConfigureAwait(true);
        panel.Controls.AddRange([
            new Label { Text = "Ziel:", AutoSize = true, Margin = new Padding(0, 8, 3, 0) }, _target,
            new Label { Text = "Typ:", AutoSize = true, Margin = new Padding(10, 8, 3, 0) }, _type,
            new Label { Text = "Notiz:", AutoSize = true, Margin = new Padding(10, 8, 3, 0) }, _note,
            add, remove]);
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
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Quelle", DataPropertyName = nameof(LearningPathNodeRelationDto.SourceTitle), Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Typ", DataPropertyName = nameof(LearningPathNodeRelationDto.Type), Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ziel", DataPropertyName = nameof(LearningPathNodeRelationDto.TargetTitle), Width = 220 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Notiz", DataPropertyName = nameof(LearningPathNodeRelationDto.Note), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
    }

    private async Task LoadAsync()
    {
        try
        {
            var nodes = await _service.ListNodesAsync(_pathId).ConfigureAwait(true);
            var options = nodes.Where(node => node.Id != _sourceNodeId && node.Status != LearningPathNodeStatus.Archived)
                .OrderBy(static node => node.Title, StringComparer.OrdinalIgnoreCase)
                .Select(static node => new NodeOption(node.Id, node.Title)).ToArray();
            _target.DataSource = options;
            _target.DisplayMember = nameof(NodeOption.Text);
            _grid.DataSource = (await _service.ListRelationsAsync(_pathId).ConfigureAwait(true)).ToList();
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Knoten-Beziehungen laden");
        }
    }

    private async Task AddAsync()
    {
        if (_target.SelectedItem is not NodeOption target)
        {
            return;
        }
        try
        {
            await _service.AddRelationAsync(new LearningPathNodeRelationModel(_sourceNodeId, target.Id,
                (LearningPathNodeRelationType)_type.SelectedItem!, string.IsNullOrWhiteSpace(_note.Text) ? null : _note.Text.Trim())).ConfigureAwait(true);
            _note.Clear();
            await LoadAsync().ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Knoten-Beziehung anlegen");
        }
    }

    private async Task DeleteAsync()
    {
        if (_grid.CurrentRow?.DataBoundItem is not LearningPathNodeRelationDto relation)
        {
            return;
        }
        try
        {
            await _service.DeleteRelationAsync(relation.Id).ConfigureAwait(true);
            await LoadAsync().ConfigureAwait(true);
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Knoten-Beziehung entfernen");
        }
    }

    private sealed record NodeOption(Guid Id, string Text);
}
