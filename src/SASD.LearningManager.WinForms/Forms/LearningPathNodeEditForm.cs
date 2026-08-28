using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.LearningPaths;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Application.Skills;
using SASD.LearningManager.Domain.LearningPaths;
using SASD.LearningManager.Domain.Resources;
using SASD.LearningManager.Domain.Skills;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>
/// Edits one node including parent, Required/Optional state and Skill/Resource assignments. Tree
/// cycles are still validated in the application service so the UI is never the only protection.
/// </summary>
public sealed class LearningPathNodeEditForm : Form
{
    private readonly LearningPathService _pathService;
    private readonly SkillService _skillService;
    private readonly ResourceService _resourceService;
    private readonly ILogger _logger;
    private readonly Guid _pathId;
    private readonly Guid? _nodeId;
    private readonly Guid? _initialParentId;
    private readonly TextBox _title = new() { MaxLength = 500 };
    private readonly TextBox _description = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, MaxLength = 20000 };
    private readonly ComboBox _parent = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _required = new() { Text = "Pflichtknoten", AutoSize = true, Checked = true };
    private readonly CheckedListBox _skills = new() { CheckOnClick = true, IntegralHeight = false };
    private readonly CheckedListBox _resources = new() { CheckOnClick = true, IntegralHeight = false };
    private readonly Button _save = new() { Text = "Speichern", AutoSize = true };

    public LearningPathNodeEditForm(LearningPathService pathService, SkillService skillService, ResourceService resourceService,
        ILogger logger, Guid pathId, Guid? nodeId = null, Guid? initialParentId = null)
    {
        _pathService = pathService;
        _skillService = skillService;
        _resourceService = resourceService;
        _logger = logger;
        _pathId = pathId;
        _nodeId = nodeId;
        _initialParentId = initialParentId;
        Text = nodeId is null ? "Path-Knoten anlegen" : "Path-Knoten bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(900, 760);
        Size = new Size(1020, 860);
        AutoScaleMode = AutoScaleMode.Dpi;
        _type.DataSource = Enum.GetValues<LearningPathNodeType>();
        _status.DataSource = Enum.GetValues<LearningPathNodeStatus>().Where(static value => value != LearningPathNodeStatus.Archived).ToArray();
        _status.SelectedItem = LearningPathNodeStatus.Planned;
        Controls.Add(BuildLayout());
        AcceptButton = _save;
        Load += async (_, _) => await LoadDataAsync().ConfigureAwait(true);
        _save.Click += async (_, _) => await SaveAsync().ConfigureAwait(true);
    }

    private Control BuildLayout()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 9, AutoScroll = true };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        Add(panel, 0, "Titel *", _title);
        Add(panel, 1, "Übergeordnet", _parent);
        Add(panel, 2, "Typ", _type);
        var statePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        _status.Width = 180;
        statePanel.Controls.Add(_status);
        statePanel.Controls.Add(_required);
        Add(panel, 3, "Status / Relevanz", statePanel);
        Add(panel, 4, "Beschreibung", _description);
        Add(panel, 5, "Skills", _skills);
        Add(panel, 6, "Ressourcen", _resources);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_save);
        panel.Controls.Add(buttons, 1, 8);
        CancelButton = cancel;
        return panel;
    }

    private static void Add(TableLayoutPanel panel, int row, string label, Control control)
    {
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 3, 0, 3);
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 8, 8, 0) }, 0, row);
        panel.Controls.Add(control, 1, row);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var nodeDetail = _nodeId is null ? null : await _pathService.GetNodeDetailAsync(_nodeId.Value).ConfigureAwait(true);
            if (_nodeId is not null && nodeDetail is null)
            {
                throw new KeyNotFoundException("Der Path-Knoten wurde nicht gefunden.");
            }
            if (nodeDetail?.Status == LearningPathNodeStatus.Archived)
            {
                throw new InvalidOperationException("Archivierte Knoten müssen zuerst wiederhergestellt werden.");
            }

            var nodes = await _pathService.ListNodesAsync(_pathId, includeArchived: false).ConfigureAwait(true);
            var parentOptions = new List<ParentOption> { new(null, "(Wurzel / kein Parent)") };
            parentOptions.AddRange(nodes.Where(node => node.Id != _nodeId)
                .OrderBy(static node => node.Title, StringComparer.OrdinalIgnoreCase)
                .Select(static node => new ParentOption(node.Id, node.Title)));
            _parent.DataSource = parentOptions;
            _parent.DisplayMember = nameof(ParentOption.Text);

            var skills = await _skillService.ListLookupAsync(includeArchived: _nodeId is not null).ConfigureAwait(true);
            var skillOptions = skills.Select(static skill => new SkillOption(skill.Id,
                skill.Status == SkillStatus.Archived ? $"{skill.Name} [Archiviert]" : $"{skill.Name}  (Ist {skill.CurrentLevel?.ToString() ?? "–"} / Ziel {skill.TargetLevel?.ToString() ?? "–"})")).ToArray();
            _skills.DataSource = skillOptions;
            _skills.DisplayMember = nameof(SkillOption.Text);

            var resources = await _resourceService.ListLookupAsync(includeArchived: _nodeId is not null).ConfigureAwait(true);
            var resourceOptions = resources.Select(static resource => new ResourceOption(resource.Id, FormatResource(resource))).ToArray();
            _resources.DataSource = resourceOptions;
            _resources.DisplayMember = nameof(ResourceOption.Text);

            var selectedParentId = nodeDetail?.ParentNodeId ?? _initialParentId;
            _parent.SelectedItem = parentOptions.FirstOrDefault(option => option.Id == selectedParentId) ?? parentOptions[0];
            if (nodeDetail is null)
            {
                return;
            }

            _title.Text = nodeDetail.Title;
            _description.Text = nodeDetail.Description ?? string.Empty;
            _type.SelectedItem = nodeDetail.Type;
            _status.SelectedItem = nodeDetail.Status;
            _required.Checked = nodeDetail.IsRequired;
            for (var index = 0; index < skillOptions.Length; index++)
            {
                if (nodeDetail.SkillIds.Contains(skillOptions[index].Id)) _skills.SetItemChecked(index, true);
            }
            for (var index = 0; index < resourceOptions.Length; index++)
            {
                if (nodeDetail.ResourceIds.Contains(resourceOptions[index].Id)) _resources.SetItemChecked(index, true);
            }
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Path-Knoten laden");
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            _save.Enabled = false;
            var parentId = (_parent.SelectedItem as ParentOption)?.Id;
            var skillIds = _skills.CheckedItems.Cast<SkillOption>().Select(static item => item.Id).ToArray();
            var resourceIds = _resources.CheckedItems.Cast<ResourceOption>().Select(static item => item.Id).ToArray();
            var model = new LearningPathNodeEditModel(parentId, _title.Text, NullIfWhiteSpace(_description.Text),
                (LearningPathNodeType)_type.SelectedItem!, _required.Checked, (LearningPathNodeStatus)_status.SelectedItem!, skillIds, resourceIds);
            if (_nodeId is null)
            {
                await _pathService.CreateNodeAsync(_pathId, model).ConfigureAwait(true);
            }
            else
            {
                await _pathService.UpdateNodeAsync(_nodeId.Value, model).ConfigureAwait(true);
            }
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Path-Knoten speichern");
        }
        finally
        {
            _save.Enabled = true;
        }
    }


    private static string FormatResource(ResourceLookupDto resource)
    {
        var provider = string.IsNullOrWhiteSpace(resource.ProviderName) ? string.Empty : $"; {resource.ProviderName}";
        var archived = resource.Status == ResourceStatus.Archived ? "; Archiviert" : string.Empty;
        return $"{resource.Title} [{resource.Type}{provider}{archived}]";
    }

    private static string? NullIfWhiteSpace(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private sealed record ParentOption(Guid? Id, string Text);
    private sealed record SkillOption(Guid Id, string Text);
    private sealed record ResourceOption(Guid Id, string Text);
}
