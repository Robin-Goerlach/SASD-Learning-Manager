using Microsoft.Extensions.Logging;
using SASD.LearningManager.Application.Resources;
using SASD.LearningManager.WinForms.Presentation;

namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Describes how the Quick Capture dialog ended.</summary>
public enum QuickCaptureOutcome
{
    None,
    Created,
    OpenExisting
}

/// <summary>
/// Minimal URL capture dialog for Milestone 2. It deliberately avoids provider, tag and path fields;
/// those belong to the later Inbox-classification step rather than the interruption-sensitive capture step.
/// </summary>
public sealed class QuickCaptureForm : Form
{
    private readonly ResourceService _resourceService;
    private readonly ILogger _logger;
    private readonly TextBox _url = new() { PlaceholderText = "https://…" };
    private readonly TextBox _title = new() { PlaceholderText = "optional – kann später ergänzt werden" };
    private readonly TextBox _note = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly Button _save = new() { Text = "In Inbox speichern", AutoSize = true };

    /// <summary>Gets whether a resource was created or an existing duplicate should be opened.</summary>
    public QuickCaptureOutcome Outcome { get; private set; }

    /// <summary>Gets the created or selected existing resource identity.</summary>
    public Guid? ResourceId { get; private set; }

    public QuickCaptureForm(ResourceService resourceService, ILogger logger)
    {
        _resourceService = resourceService;
        _logger = logger;

        Text = "Ressource schnell erfassen";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(650, 360);
        Size = new Size(720, 420);
        Font = new Font("Segoe UI", 9F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Controls.Add(BuildLayout());
        AcceptButton = _save;
        _save.Click += async (_, _) => await SaveAsync().ConfigureAwait(true);

        Shown += (_, _) => _url.Focus();
    }

    private Control BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            ColumnCount = 2,
            RowCount = 6
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        AddRow(layout, 0, "URL *", _url);
        AddRow(layout, 1, "Titel", _title);
        AddRow(layout, 2, "Notiz", _note);

        var explanation = new Label
        {
            Text = "Nur die URL ist erforderlich. Provider, Typ, Tags und Lernstatus werden später in der Inbox klassifiziert.",
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Padding = new Padding(0, 8, 0, 0)
        };
        layout.Controls.Add(explanation, 1, 3);

        var shortcut = new Label
        {
            Text = "Globaler Shortcut: Ctrl+Shift+N",
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Padding = new Padding(0, 5, 0, 0)
        };
        layout.Controls.Add(shortcut, 1, 4);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };
        var cancel = new Button { Text = "Abbrechen", AutoSize = true, DialogResult = DialogResult.Cancel };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_save);
        layout.Controls.Add(buttons, 1, 5);
        CancelButton = cancel;
        return layout;
    }

    private static void AddRow(TableLayoutPanel layout, int row, string caption, Control control)
    {
        layout.Controls.Add(new Label
        {
            Text = caption,
            AutoSize = true,
            Margin = new Padding(0, 8, 8, 0)
        }, 0, row);
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 3, 0, 3);
        layout.Controls.Add(control, 1, row);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_url.Text))
        {
            MessageBox.Show(this, "Bitte eine HTTP- oder HTTPS-URL eingeben.", "Quick Capture", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _url.Focus();
            return;
        }

        var model = new QuickCaptureModel(
            _url.Text.Trim(),
            NullIfWhiteSpace(_title.Text),
            NullIfWhiteSpace(_note.Text));

        try
        {
            _save.Enabled = false;
            ResourceId = await _resourceService.QuickCaptureAsync(model).ConfigureAwait(true);
            Outcome = QuickCaptureOutcome.Created;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (DuplicateResourceException duplicate)
        {
            using var duplicateDialog = new DuplicateResourceForm(duplicate.ExistingResourceTitle);
            if (duplicateDialog.ShowDialog(this) != DialogResult.OK || duplicateDialog.Choice == DuplicateResourceChoice.Cancel)
            {
                return;
            }

            if (duplicateDialog.Choice == DuplicateResourceChoice.OpenExisting)
            {
                ResourceId = duplicate.ExistingResourceId;
                Outcome = QuickCaptureOutcome.OpenExisting;
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            ResourceId = await _resourceService.QuickCaptureAsync(model, allowDuplicateUrl: true).ConfigureAwait(true);
            Outcome = QuickCaptureOutcome.Created;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            UiErrorHandler.Show(this, exception, _logger, "Quick Capture");
        }
        finally
        {
            _save.Enabled = true;
        }
    }

    private static string? NullIfWhiteSpace(string text)
        => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
