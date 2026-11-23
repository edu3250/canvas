using System.Diagnostics;
using System.Runtime.InteropServices;

internal sealed class EmbeddingHost : Form
{
    private readonly Run run;
    private readonly string kind;
    private readonly Panel slot = new() { BackColor = Color.FromArgb(43, 47, 57) };
    private readonly Label status = new() { AutoSize = true, ForeColor = Color.White, Padding = new Padding(8) };
    private readonly System.Windows.Forms.Timer poll = new() { Interval = 25 };
    private readonly Stopwatch elapsed = new();
    private Process? process;
    private readonly Panel secondSlot = new() { BackColor = Color.FromArgb(43, 47, 57), Visible = false };
    private readonly Button another = new() { Text = "Abrir segunda ventana", AutoSize = true, Enabled = false };
    private readonly List<EmbeddedPanel> embedded = [];
    internal bool Failed { get; private set; }

    internal EmbeddingHost(Run run, string kind)
    {
        this.run = run;
        this.kind = kind;
        Text = "Canvas · Ensayo de apertura";
        BackColor = Color.FromArgb(20, 23, 31);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = Screen.PrimaryScreen!.Bounds;
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48 };
        var exit = new Button { Text = "Terminar ensayo", AutoSize = true };
        Shown += (_, _) => Guard(() =>
        {
            LayoutSlot();
            elapsed.Start();
            process = run.LaunchEmbeddingCandidate(kind);
            status.Text = "Esperando la ventana del perfil de prueba…";
            poll.Start();
        });
        another.Visible = kind == "chrome";
        another.Click += (_, _) => Guard(() =>
        {
            another.Enabled = false;
            elapsed.Restart();
            // Chrome forwards this launch to the first process through the isolated profile.
            run.LaunchEmbeddingCandidate("chrome");
            status.Text = "Esperando otra ventana del mismo perfil…";
            poll.Start();
        });
        exit.Click += (_, _) => Close();
        another.BackColor = exit.BackColor = SystemColors.Control;
        another.ForeColor = exit.ForeColor = SystemColors.ControlText;
        bar.Controls.AddRange([another, exit, status]);
        Controls.AddRange([slot, secondSlot, bar]);
        ClientSizeChanged += (_, _) => Guard(LayoutSlot);
        poll.Tick += (_, _) => Guard(FindCandidate);
        FormClosing += (_, _) =>
        {
            poll.Stop();
            foreach (var panel in embedded.AsEnumerable().Reverse())
            {
                try { panel.Detach(); }
                catch (Exception error) { Failed = true; run.Log("Error al devolver: " + error.Message); }
            }
        };
    }

    private void LayoutSlot()
    {
        var split = secondSlot.Visible;
        var width = Math.Clamp(split ? (ClientSize.Width - 36) / 2 : ClientSize.Width - 24, 1, 1060);
        var height = Math.Clamp(ClientSize.Height - 72, 1, 810);
        slot.Bounds = new Rectangle(split ? (ClientSize.Width - 2 * width - 12) / 2 : (ClientSize.Width - width) / 2,
            Math.Max(48, (ClientSize.Height - height) / 2), width, height);
        secondSlot.Bounds = new Rectangle(slot.Right + 12, slot.Top, width, height);
        if (embedded.Count > 0) embedded[0].Resize(slot);
        if (embedded.Count > 1) embedded[1].Resize(secondSlot);
    }

    private void FindCandidate()
    {
        if (process == null) return;
        if (elapsed.Elapsed.TotalSeconds > 35 || process.HasExited)
            throw new InvalidOperationException("Falta la ventana del perfil descartable.");
        nint found = 0;
        EnumWindows((handle, _) =>
        {
            if (Native.Pid(handle) != process.Id || !EmbeddedPanel.IsWindowVisible(handle) ||
                !run.IsOurs(handle, process.Id)) return true;
            found = handle;
            return false;
        }, 0);
        if (found == 0) return;
        poll.Stop();
        run.Log($"Ventana visible identificada a {elapsed.ElapsedMilliseconds} ms del lanzamiento.");
        var second = embedded.Count == 1;
        secondSlot.Visible = second;
        LayoutSlot();
        var panel = new EmbeddedPanel(found);
        embedded.Add(panel);
        panel.Attach(second ? secondSlot : slot);
        another.Enabled = !second && kind == "chrome";
        run.Log($"Ventana alojada a {elapsed.ElapsedMilliseconds} ms del lanzamiento.");
        status.Text = "Prueba un clic, escritura y un diálogo. Terminar devuelve la ventana y cierra la prueba.";
    }

    private void Guard(Action action)
    {
        try { action(); }
        catch (Exception error)
        {
            Failed = true;
            poll.Stop();
            run.Log("Error de ensayo: " + error.Message);
            status.Text = error.Message;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) poll.Dispose();
        base.Dispose(disposing);
    }

    private delegate bool EnumWindow(nint handle, nint data);
    [DllImport("user32")] private static extern bool EnumWindows(EnumWindow callback, nint data);
}
