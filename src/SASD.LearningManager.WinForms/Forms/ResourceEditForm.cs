using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.Domain.Resources;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Creates or edits a canonical resource without exposing persistence details to WinForms.</summary>
public sealed class ResourceEditForm : Form
{
    private readonly ResourceService _resourceService;
    private readonly ProviderService _providerService;
    private readonly ILogger _logger;
    private readonly Guid? _resourceId;
    private readonly bool _classificationMode;

    private readonly TextBox _title = new();
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _provider = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _url = new();
    private readonly TextBox _localPath = new();
    private readonly TextBox _creator = new();
    private readonly TextBox _language = new();
    private readonly TextBox _version = new();
    private readonly NumericUpDown _estimatedMinutes = new() { Minimum = 0, Maximum = 1_000_000, ThousandsSeparator = true };
    private readonly CheckBox _useEstimate = new() { Text = "Aufwand erfassen", AutoSize = true };
    private readonly ComboBox _difficulty = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _priority = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _progress = new() { Minimum = 0, Maximum = 100 };
    private readonly CheckBox _useProgress = new() { Text = "Fortschritt erfassen", AutoSize = true };
    private readonly TextBox _tags = new();
    private readonly TextBox _description = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _whySaved = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly Button _save = new() { Text = "Speichern", AutoSize = true };

    public ResourceEditForm(ResourceService resourceService, ProviderService providerService, ILogger logger, Guid? resourceId, bool classificationMode = false)
    {
        _resourceService = resourceService;
        _providerService = providerService;
        _logger = logger;
        _resourceId = resourceId;
        _classificationMode = classificationMode;

        Text = classificationMode ? "Inbox-Ressource klassifizieren" : resourceId is null ? "Ressource anlegen" : "Ressource bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(780, 720);
        Size = new Size(850, 780);
        Font = new Font("Segoe UI", 9F);
        AutoScaleMode = AutoScaleMode.Dpi;

        _type.DataSource = Enum.GetValues<ResourceType>();
        _difficulty.DataSource = Enum.GetValues<ResourceDifficulty>();
        _priority.DataSource = Enum.GetValues<ResourcePriority>();
        _status.DataSource = Enum.GetValues<ResourceStatus>().Where(static x => x != ResourceStatus.Archived).ToArray();
        _priority.SelectedItem = ResourcePriority.Normal;
        _difficulty.SelectedItem = ResourceDifficulty.Unknown;
        _status.SelectedItem = ResourceStatus.Planned;

        Controls.Add(BuildLayout());
        AcceptButton = _save;
        Load += async (_, _) => await LoadAsync().ConfigureAwait(true);
        _save.Click += async (_, _) => await SaveAsync().ConfigureAwait(true);
        _useEstimate.CheckedChanged += (_, _) => _estimatedMinutes.Enabled = _useEstimate.Checked;
        _useProgress.CheckedChanged += (_, _) => _progress.Enabled = _useProgress.Checked;
        _estimatedMinutes.Enabled = false;
        _progress.Enabled = false;
    }

    private Control BuildLayout()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2,
            RowCount = 18,
            AutoScroll = true
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(panel, 0, "Titel *", _title);
        AddRow(panel, 1, "Typ", _type);
        AddRow(panel, 2, "Provider", _provider);
        AddRow(panel, 3, "URL", _url);

        var localPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Height = 30 };
        localPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        localPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _localPath.Dock = DockStyle.Fill;
        var browse = new Button { Text = "…", Width = 36 };
        browse.Click += (_, _) => BrowseLocalFile();
        localPanel.Controls.Add(_localPath, 0, 0);
        localPanel.Controls.Add(browse, 1, 0);
        AddRow(panel, 4, "Lokale Datei", localPanel);

        AddRow(panel, 5, "Autor/Trainer", _creator);
        AddRow(panel, 6, "Sprache", _language);
        AddRow(panel, 7, "Version", _version);

        var estimatePanel = HorizontalPair(_useEstimate, _estimatedMinutes);
        AddRow(panel, 8, "Aufwand (Min.)", estimatePanel);
        AddRow(panel, 9, "Schwierigkeit", _difficulty);
        AddRow(panel, 10, "Priorität", _priority);
        AddRow(panel, 11, "Status", _status);
        AddRow(panel, 12, "Fortschritt", HorizontalPair(_useProgress, _progress));
        AddRow(panel, 13, "Tags", _tags);

        _description.Height = 90;
        AddRow(panel, 14, "Beschreibung", _description, 100);
        _whySaved.Height = 90;
        AddRow(panel, 15, "Warum gespeichert?", _whySaved, 100);

        var hint = new Label
        {
            Text = "Tags mit Komma oder Semikolon trennen. Ein Kursabschluss verändert später keinen Skill-Level automatisch.",
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Padding = new Padding(0, 5, 0, 5)
        };
        panel.Controls.Add(hint, 1, 16);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_save);
        panel.Controls.Add(buttons, 1, 17);
        CancelButton = cancel;
        return panel;
    }

    private static Control HorizontalPair(Control left, Control right)
    {
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoSize = true };
        left.Margin = new Padding(0, 5, 12, 0);
        right.Width = 120;
        flow.Controls.Add(left);
        flow.Controls.Add(right);
        return flow;
    }

    private static void AddRow(TableLayoutPanel panel, int row, string label, Control control, int height = 36)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        var caption = new Label { Text = label, AutoSize = true, Margin = new Padding(0, 7, 8, 0) };
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 3, 0, 3);
        panel.Controls.Add(caption, 0, row);
        panel.Controls.Add(control, 1, row);
    }

    private async Task LoadAsync()
    {
        try
        {
            var providers = await _providerService.ListAsync(includeArchived: false).ConfigureAwait(true);
            var options = new List<ProviderOption> { new(null, "(kein Provider)") };
            options.AddRange(providers.Select(static p => new ProviderOption(p.Id, p.Name)));
            _provider.DataSource = options;
            _provider.DisplayMember = nameof(ProviderOption.Name);

            if (_resourceId is null)
            {
                return;
            }

            var detail = await _resourceService.GetDetailAsync(_resourceId.Value).ConfigureAwait(true)
                ?? throw new KeyNotFoundException("Die Ressource wurde nicht gefunden.");
            _title.Text = detail.Title;
            _type.SelectedItem = detail.Type;
            _provider.SelectedItem = options.FirstOrDefault(x => x.Id == detail.ProviderId) ?? options[0];
            _url.Text = detail.Url ?? string.Empty;
            _localPath.Text = detail.LocalPath ?? string.Empty;
            _description.Text = detail.Description ?? string.Empty;
            _whySaved.Text = detail.WhySaved ?? string.Empty;
            _creator.Text = detail.Creator ?? string.Empty;
            _language.Text = detail.LanguageCode ?? string.Empty;
            _version.Text = detail.VersionText ?? string.Empty;
            _difficulty.SelectedItem = detail.Difficulty;
            _priority.SelectedItem = detail.Priority;
            _status.SelectedItem = _classificationMode && detail.Status == ResourceStatus.Inbox
                ? ResourceStatus.Planned
                : detail.Status;
            _tags.Text = string.Join(", ", detail.Tags);

            _useEstimate.Checked = detail.EstimatedMinutes is not null;
            if (detail.EstimatedMinutes is not null) _estimatedMinutes.Value = detail.EstimatedMinutes.Value;
            _useProgress.Checked = detail.ProgressPercent is not null;
            if (detail.ProgressPercent is not null) _progress.Value = detail.ProgressPercent.Value;
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Ressource laden");
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            _save.Enabled = false;
            var providerId = (_provider.SelectedItem as ProviderOption)?.Id;
            var tags = _tags.Text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var model = new ResourceEditModel(
                _title.Text,
                (ResourceType)_type.SelectedItem!,
                providerId,
                NullIfWhiteSpace(_url.Text),
                NullIfWhiteSpace(_localPath.Text),
                NullIfWhiteSpace(_description.Text),
                NullIfWhiteSpace(_whySaved.Text),
                NullIfWhiteSpace(_creator.Text),
                NullIfWhiteSpace(_language.Text),
                NullIfWhiteSpace(_version.Text),
                _useEstimate.Checked ? Decimal.ToInt32(_estimatedMinutes.Value) : null,
                (ResourceDifficulty)_difficulty.SelectedItem!,
                (ResourcePriority)_priority.SelectedItem!,
                (ResourceStatus)_status.SelectedItem!,
                _useProgress.Checked ? Decimal.ToInt32(_progress.Value) : null,
                tags);

            if (_classificationMode && model.Status == ResourceStatus.Inbox)
            {
                MessageBox.Show(
                    this,
                    "Wähle zum Abschließen der Klassifikation einen Status außerhalb der Inbox.",
                    "Inbox klassifizieren",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                await SaveModelAsync(model, allowDuplicateUrl: false).ConfigureAwait(true);
            }
            catch (DuplicateResourceException duplicate)
            {
                var choice = MessageBox.Show(
                    this,
                    $"Diese URL ist bereits als '{duplicate.ExistingResourceTitle}' gespeichert.\n\n" +
                    "Soll die aktuelle Ressource trotzdem bewusst mit derselben URL gespeichert werden?",
                    "Mögliche Dublette",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (choice != DialogResult.Yes)
                {
                    return;
                }

                await SaveModelAsync(model, allowDuplicateUrl: true).ConfigureAwait(true);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Ressource speichern");
        }
        finally
        {
            _save.Enabled = true;
        }
    }

    /// <summary>Routes the edit model through the correct application use case.</summary>
    private Task SaveModelAsync(ResourceEditModel model, bool allowDuplicateUrl)
    {
        if (_resourceId is null)
        {
            return _resourceService.CreateAsync(model, allowDuplicateUrl);
        }

        return _classificationMode
            ? _resourceService.ClassifyInboxAsync(_resourceId.Value, model, allowDuplicateUrl)
            : _resourceService.UpdateAsync(_resourceId.Value, model, allowDuplicateUrl);
    }

    private void BrowseLocalFile()
    {
        using var dialog = new OpenFileDialog { CheckFileExists = true, Multiselect = false, Title = "Lokale Lernressource auswählen" };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _localPath.Text = dialog.FileName;
        }
    }

    private static string? NullIfWhiteSpace(string text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    private sealed record ProviderOption(Guid? Id, string Name);
}
