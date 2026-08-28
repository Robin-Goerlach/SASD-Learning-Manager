namespace SASD.Bewerbungsmanager.WinForms.Controls;

internal static class ControlFactory
{
    public static Button ToolbarButton(string text, EventHandler onClick)
    {
        var button = new Button
        {
            AutoSize = true,
            Text = text,
            Padding = new Padding(10, 3, 10, 3),
            Margin = new Padding(0, 0, 8, 0),
        };
        button.Click += onClick;
        return button;
    }

    public static DataGridView DataGrid() => new()
    {
        Dock = DockStyle.Fill,
        AutoGenerateColumns = true,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        MultiSelect = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        RowHeadersVisible = false,
    };

    public static TableLayoutPanel EditorTable() => new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 2,
        Padding = new Padding(12),
    };

    public static void AddEditorRow(TableLayoutPanel table, string labelText, Control control)
    {
        var row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var label = new Label
        {
            AutoSize = true,
            Text = labelText,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 7, 12, 7),
        };
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 3, 0, 3);
        table.Controls.Add(label, 0, row);
        table.Controls.Add(control, 1, row);
    }
}
