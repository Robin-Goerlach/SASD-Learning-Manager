using SASD.Bewerbungsmanager.Infrastructure.Operations;

namespace SASD.Bewerbungsmanager.WinForms.Forms;

/// <summary>
/// Small password dialog used only at the backup boundary. Passwords are never persisted by the
/// application; the entered value is passed directly to the backup operation and then discarded.
/// </summary>
public sealed class BackupPasswordDialog : Form
{
    private readonly TextBox _password = new()
    {
        Dock = DockStyle.Top,
        UseSystemPasswordChar = true,
        MaxLength = 256,
    };

    private readonly TextBox? _confirmation;

    /// <summary>Gets the password entered by the user after the dialog returned <see cref="DialogResult.OK"/>.</summary>
    public string Password => _password.Text;

    /// <summary>Initializes the password dialog for either creating or opening an encrypted backup.</summary>
    public BackupPasswordDialog(bool confirmPassword)
    {
        Text = confirmPassword ? "Backup-Passwort festlegen" : "Backup-Passwort eingeben";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(480, confirmPassword ? 225 : 175);

        _confirmation = confirmPassword
            ? new TextBox
            {
                Dock = DockStyle.Top,
                UseSystemPasswordChar = true,
                MaxLength = 256,
            }
            : null;

        Controls.Add(BuildLayout(confirmPassword));
        AcceptButton = Controls.Find("okButton", true).OfType<Button>().Single();
        CancelButton = Controls.Find("cancelButton", true).OfType<Button>().Single();
        Shown += (_, _) => _password.Focus();
    }

    private Control BuildLayout(bool confirmPassword)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = confirmPassword ? 6 : 4,
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        if (confirmPassword)
        {
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(new Label
        {
            AutoSize = true,
            Text = confirmPassword
                ? $"Passwort (mindestens {PasswordProtectedBackupService.RequiredPasswordLength} Zeichen)"
                : "Passwort",
            Margin = new Padding(0, 0, 0, 4),
        });
        root.Controls.Add(_password);

        if (confirmPassword && _confirmation is not null)
        {
            root.Controls.Add(new Label
            {
                AutoSize = true,
                Text = "Passwort wiederholen",
                Margin = new Padding(0, 10, 0, 4),
            });
            root.Controls.Add(_confirmation);
        }

        var hint = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            Padding = new Padding(0, 10, 0, 0),
            Text = confirmPassword
                ? "Das Passwort wird nicht gespeichert. Ohne dieses Passwort kann die verschlüsselte Sicherung nicht wiederhergestellt werden."
                : "Das Passwort wird ausschließlich für diesen Lesevorgang verwendet und nicht gespeichert.",
        };
        root.Controls.Add(hint);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
        };
        var ok = new Button { Name = "okButton", Text = "OK", AutoSize = true };
        var cancel = new Button { Name = "cancelButton", Text = "Abbrechen", AutoSize = true, DialogResult = DialogResult.Cancel };
        ok.Click += (_, _) => ValidateAndClose(confirmPassword);
        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);
        root.Controls.Add(buttons);

        return root;
    }

    private void ValidateAndClose(bool confirmPassword)
    {
        if (string.IsNullOrEmpty(_password.Text))
        {
            MessageBox.Show(this, "Bitte ein Passwort eingeben.", "Backup-Passwort", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (confirmPassword)
        {
            try
            {
                PasswordProtectedBackupService.ValidateNewPassword(_password.Text);
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show(this, exception.Message, "Backup-Passwort", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_confirmation is null || !string.Equals(_password.Text, _confirmation.Text, StringComparison.Ordinal))
            {
                MessageBox.Show(this, "Die beiden Passwörter stimmen nicht überein.", "Backup-Passwort", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
