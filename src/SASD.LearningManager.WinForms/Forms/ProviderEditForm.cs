using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Providers;
using SASD.LearningManager.Domain.Providers;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Creates and edits provider metadata.</summary>
public sealed class ProviderEditForm : Form
{
    private readonly ProviderService _service;
    private readonly ILogger _logger;
    private readonly Guid? _providerId;
    private readonly TextBox _name = new();
    private readonly TextBox _website = new();
    private readonly TextBox _description = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button _save = new() { Text = "Speichern", AutoSize = true };

    public ProviderEditForm(ProviderService service, ILogger logger, Guid? providerId)
    {
        _service = service;
        _logger = logger;
        _providerId = providerId;
        Text = providerId is null ? "Provider anlegen" : "Provider bearbeiten";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(620, 400);
        Font = new Font("Segoe UI", 9F);
        AutoScaleMode = AutoScaleMode.Dpi;
        _type.DataSource = Enum.GetValues<ProviderType>();
        _type.SelectedItem = ProviderType.LearningPlatform;
        Controls.Add(BuildLayout());
        Load += async (_, _) => await LoadAsync().ConfigureAwait(true);
        _save.Click += async (_, _) => await SaveAsync().ConfigureAwait(true);
        AcceptButton = _save;
    }

    private Control BuildLayout()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 5 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Add(panel, 0, "Name *", _name, 38);
        Add(panel, 1, "Website", _website, 38);
        Add(panel, 2, "Typ", _type, 38);
        _description.Height = 150;
        Add(panel, 3, "Beschreibung", _description, 170);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_save);
        panel.Controls.Add(buttons, 1, 4);
        CancelButton = cancel;
        return panel;
    }

    private static void Add(TableLayoutPanel panel, int row, string caption, Control control, int height)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        panel.Controls.Add(new Label { Text = caption, AutoSize = true, Margin = new Padding(0, 8, 8, 0) }, 0, row);
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 3, 0, 3);
        panel.Controls.Add(control, 1, row);
    }

    private async Task LoadAsync()
    {
        if (_providerId is null) return;
        try
        {
            var provider = await _service.GetAsync(_providerId.Value).ConfigureAwait(true)
                ?? throw new KeyNotFoundException("Der Provider wurde nicht gefunden.");
            _name.Text = provider.Name;
            _website.Text = provider.WebsiteUrl ?? string.Empty;
            _description.Text = provider.Description ?? string.Empty;
            _type.SelectedItem = provider.Type;
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Provider laden");
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            _save.Enabled = false;
            var model = new ProviderEditModel(_name.Text, NullIfWhiteSpace(_website.Text), NullIfWhiteSpace(_description.Text), (ProviderType)_type.SelectedItem!);
            if (_providerId is null)
            {
                await _service.CreateAsync(model).ConfigureAwait(true);
            }
            else
            {
                await _service.UpdateAsync(_providerId.Value, model).ConfigureAwait(true);
            }
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Provider speichern");
        }
        finally
        {
            _save.Enabled = true;
        }
    }

    private static string? NullIfWhiteSpace(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
