namespace SASD.LearningManager.WinForms.Forms;

/// <summary>Possible user decisions when Quick Capture detects an existing canonical URL.</summary>
public enum DuplicateResourceChoice
{
    Cancel,
    OpenExisting,
    CreateDuplicate
}

/// <summary>
/// Small explicit conflict dialog used instead of ambiguous Yes/No wording. The dialog makes the
/// canonical-resource default obvious while still allowing the documented exceptional duplicate case.
/// </summary>
public sealed class DuplicateResourceForm : Form
{
    /// <summary>Gets the decision made by the user.</summary>
    public DuplicateResourceChoice Choice { get; private set; } = DuplicateResourceChoice.Cancel;

    public DuplicateResourceForm(string existingResourceTitle)
    {
        Text = "Mögliche Dublette";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 9F);
        ClientSize = new Size(600, 230);

        var message = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            Text =
                "Diese URL ist bereits gespeichert.\n\n" +
                $"Bestehende Ressource: {existingResourceTitle}\n\n" +
                "Normalerweise sollte der bestehende kanonische Eintrag verwendet werden. " +
                "Nur wenn dieselbe URL fachlich wirklich zwei getrennte Ressourcen beschreibt, " +
                "sollte bewusst eine zweite Ressource angelegt werden."
        };

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8),
            WrapContents = false
        };

        var cancel = new Button { Text = "Abbrechen", AutoSize = true };
        cancel.Click += (_, _) => Complete(DuplicateResourceChoice.Cancel);
        var duplicate = new Button { Text = "Trotzdem neu anlegen", AutoSize = true };
        duplicate.Click += (_, _) => Complete(DuplicateResourceChoice.CreateDuplicate);
        var open = new Button { Text = "Bestehende öffnen", AutoSize = true };
        open.Click += (_, _) => Complete(DuplicateResourceChoice.OpenExisting);

        buttons.Controls.Add(cancel);
        buttons.Controls.Add(duplicate);
        buttons.Controls.Add(open);
        Controls.Add(message);
        Controls.Add(buttons);
        CancelButton = cancel;
    }

    private void Complete(DuplicateResourceChoice choice)
    {
        Choice = choice;
        DialogResult = DialogResult.OK;
        Close();
    }
}
