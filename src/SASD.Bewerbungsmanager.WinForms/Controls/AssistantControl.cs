using SASD.Bewerbungsmanager.Application.Services;
using SASD.Bewerbungsmanager.Domain.Entities;
using SASD.Bewerbungsmanager.Domain.Enums;
using SASD.Bewerbungsmanager.WinForms.Forms;
using SASD.Bewerbungsmanager.WinForms.Presentation;

namespace SASD.Bewerbungsmanager.WinForms.Controls;

/// <summary>
/// Reviewable assistant workspace. It prepares guarded prompts, copies them through the Windows
/// clipboard and stores explicitly pasted responses. No network or model call is performed here.
/// </summary>
public sealed class AssistantControl : UserControl
{
    private readonly AssistantWorkspaceService _service;
    private readonly UiExceptionPresenter _errors;
    private readonly DataGridView _grid = ControlFactory.DataGrid();
    private readonly TextBox _prompt = DetailBox();
    private readonly TextBox _response = DetailBox();
    private IReadOnlyList<AssistantSession> _items = [];

    /// <summary>Initializes the optional assistant workspace.</summary>
    public AssistantControl(AssistantWorkspaceService service, UiExceptionPresenter errors)
    {
        _service = service;
        _errors = errors;
        BuildLayout();
        Load += async (_, _) => await RefreshAsync();
        _grid.SelectionChanged += (_, _) => ShowSelectedDetails();
    }

    private void BuildLayout()
    {
        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8),
        };
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Neue Sitzung...", async (_, _) => await PrepareAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Prompt kopieren", (_, _) => CopyPrompt()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Antwort einfügen...", async (_, _) => await PasteResponseAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Antwort kopieren", (_, _) => CopyResponse()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Verwerfen", async (_, _) => await DiscardAsync()));
        toolbar.Controls.Add(ControlFactory.ToolbarButton("Aktualisieren", async (_, _) => await RefreshAsync()));

        var detailTabs = new TabControl { Dock = DockStyle.Fill };
        var promptTab = new TabPage("Prompt");
        var responseTab = new TabPage("Antwort");
        promptTab.Controls.Add(_prompt);
        responseTab.Controls.Add(_response);
        detailTabs.TabPages.Add(promptTab);
        detailTabs.TabPages.Add(responseTab);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 300,
            Panel1MinSize = 180,
            Panel2MinSize = 180,
        };
        split.Panel1.Controls.Add(_grid);
        split.Panel2.Controls.Add(detailTabs);

        var notice = new Label
        {
            Dock = DockStyle.Bottom,
            AutoSize = false,
            Height = 44,
            Padding = new Padding(4, 6, 4, 4),
            Text = "Optionaler Assistenz-Handoff: Es gibt keine automatische Cloud-Verbindung. " +
                   "Prompts und Antworten werden nur auf deine ausdrückliche Aktion über die Zwischenablage übertragen.",
        };

        Controls.Add(split);
        Controls.Add(notice);
        Controls.Add(toolbar);
    }

    private async Task RefreshAsync()
    {
        try
        {
            _items = await _service.ListAsync();
            _grid.DataSource = _items.Select(item => new
            {
                item.Id,
                Erstellt = item.CreatedAtUtc.LocalDateTime.ToString("g"),
                Aufgabe = DisplayText.AssistantTaskKind(item.TaskKind),
                Status = DisplayText.AssistantSessionStatus(item.Status),
                item.Title,
                Provider = item.ProviderLabel ?? string.Empty,
            }).ToList();
            if (_grid.Columns["Id"] is { } idColumn)
            {
                idColumn.Visible = false;
            }

            ShowSelectedDetails();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task PrepareAsync()
    {
        try
        {
            var targets = await _service.ListTargetsAsync();
            if (targets.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "Es gibt noch keine Stelle oder Bewerbung, die als Assistenz-Kontext verwendet werden kann.",
                    "Assistenz",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var dialog = new AssistantPrepareForm(targets);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var session = await _service.PrepareAsync(dialog.Input);
            await RefreshAsync();
            SelectSession(session.Id);

            if (MessageBox.Show(
                    this,
                    "Der Prompt wurde lokal erzeugt. Jetzt in die Zwischenablage kopieren?",
                    "Assistenz",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Clipboard.SetText(session.PromptText, TextDataFormat.UnicodeText);
            }
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private void CopyPrompt()
    {
        var selected = SelectedItem();
        if (selected is null || string.IsNullOrWhiteSpace(selected.PromptText))
        {
            return;
        }

        try
        {
            Clipboard.SetText(selected.PromptText, TextDataFormat.UnicodeText);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task PasteResponseAsync()
    {
        var selected = SelectedItem();
        if (selected is null)
        {
            return;
        }

        if (selected.Status != AssistantSessionStatus.Prepared)
        {
            MessageBox.Show(
                this,
                "Nur eine vorbereitete Sitzung kann eine Antwort erhalten. Für eine neue Auswertung bitte eine neue Sitzung anlegen.",
                "Assistenz",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        try
        {
            var initial = Clipboard.ContainsText(TextDataFormat.UnicodeText)
                ? Clipboard.GetText(TextDataFormat.UnicodeText)
                : string.Empty;
            using var dialog = new AssistantResponseForm(initial);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            await _service.CompleteAsync(selected.Id, dialog.Input);
            await RefreshAsync();
            SelectSession(selected.Id);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private void CopyResponse()
    {
        var selected = SelectedItem();
        if (selected?.ResponseText is null)
        {
            return;
        }

        try
        {
            Clipboard.SetText(selected.ResponseText, TextDataFormat.UnicodeText);
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private async Task DiscardAsync()
    {
        var selected = SelectedItem();
        if (selected is null || selected.Status != AssistantSessionStatus.Prepared)
        {
            return;
        }

        if (MessageBox.Show(
                this,
                $"Assistenz-Sitzung '{selected.Title}' verwerfen?",
                "Assistenz",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _service.DiscardAsync(selected.Id);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _errors.Show(ex, this);
        }
    }

    private void ShowSelectedDetails()
    {
        var selected = SelectedItem();
        _prompt.Text = selected?.PromptText ?? string.Empty;
        _response.Text = selected?.ResponseText ?? string.Empty;
    }

    private AssistantSession? SelectedItem()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is Guid id)
        {
            return _items.SingleOrDefault(item => item.Id == id);
        }

        return null;
    }

    private void SelectSession(Guid id)
    {
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.Cells["Id"].Value is Guid rowId && rowId == id)
            {
                row.Selected = true;
                _grid.CurrentCell = row.Cells.Cast<DataGridViewCell>().First(cell => cell.Visible);
                break;
            }
        }
    }

    private static TextBox DetailBox() => new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Both,
        WordWrap = true,
        Font = new Font(FontFamily.GenericMonospace, 9),
    };
}
