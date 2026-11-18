internal sealed class Fixture : Form
{
    internal static readonly Color Witness = Color.FromArgb(31, 164, 111);
    readonly TextBox editor = new() { Dock = DockStyle.Fill, Multiline = true, WordWrap = true,
        Font = new Font("Consolas", 12), ScrollBars = ScrollBars.Vertical };

    internal Fixture(string kind, string file)
    {
        Text = kind + " " + Path.GetFileName(file);
        StartPosition = FormStartPosition.Manual;
        Bounds = new Rectangle(Screen.PrimaryScreen!.WorkingArea.Location, new Size(640, 440));
        KeyPreview = true;
        if (kind == "vecino")
        {
            BackColor = Witness;
            Controls.Add(new Label { Text = "VECINO", Location = new(20, 20), ForeColor = Color.White, AutoSize = true });
            return;
        }
        editor.Text = File.ReadAllText(file);
        Controls.Add(editor);
        Shown += (_, _) => editor.Focus();
        KeyDown += (_, e) =>
        {
            if (!e.Control || e.KeyCode is not (Keys.S or Keys.O)) return;
            e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.S) File.WriteAllText(file, editor.Text);
            else
            {
                using var dialog = new OpenFileDialog { InitialDirectory = Path.GetDirectoryName(file) };
                dialog.ShowDialog(this);
                editor.Focus();
            }
        };
    }
}
