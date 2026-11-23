using System.Diagnostics;
using System.Runtime.InteropServices;

internal sealed class EmbeddingHost : Form
{
    private readonly Run run;
    private readonly string kind;
    private readonly bool checkInput;
    private readonly bool overlay;
    private bool inputConfirmed;
    private readonly Panel slot = new() { BackColor = Color.FromArgb(43, 47, 57) };
    private readonly Label status = new() { AutoSize = true, ForeColor = Color.White, Padding = new Padding(8) };
    private readonly System.Windows.Forms.Timer poll = new() { Interval = 25 };
    private readonly Stopwatch elapsed = new();
    private Process? process;
    private readonly Panel secondSlot = new() { BackColor = Color.FromArgb(43, 47, 57), Visible = false };
    private readonly Button another = new() { Text = "Abrir segunda ventana", AutoSize = true, Enabled = false };
    private readonly List<EmbeddedPanel> embedded = [];
    internal bool Failed { get; private set; }

    internal EmbeddingHost(Run run, string kind, bool checkInput, bool overlay)
    {
        this.run = run;
        this.kind = kind;
        this.checkInput = checkInput;
        this.overlay = overlay;
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
        another.Visible = kind == "chrome" && !overlay;
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
            if (checkInput && !inputConfirmed) Failed = true;
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
        var panel = new EmbeddedPanel(found, overlay);
        embedded.Add(panel);
        panel.Attach(second ? secondSlot : slot);
        if (overlay && panel.Current && Native.GetForegroundWindow() == Handle)
            Native.SetForegroundWindow(found);
        another.Enabled = !second && kind == "chrome";
        run.Log($"Ventana {(overlay ? "superpuesta" : "alojada")} a {elapsed.ElapsedMilliseconds} ms del lanzamiento.");
        status.Text = "Prueba un clic, escritura y un diálogo. Terminar devuelve la ventana y cierra la prueba.";
        if (checkInput) _ = VerifyInput(found, panel);
    }

    private async Task VerifyInput(nint window, EmbeddedPanel panel)
    {
        another.Enabled = false;
        try
        {
            await Task.Delay(1200);
            if (IsDisposed) return;
            var point = slot.PointToScreen(new Point(slot.Width / 2, slot.Height / 2));
            bool OwnTarget(nint target) => target == window || IsChild(window, target);
            bool Eligible() => panel.Current && run.IsOurs(window, process!.Id) &&
                (Native.GetForegroundWindow() == Handle || Native.GetForegroundWindow() == window);
            var hit = Native.WindowFromPoint(point);
            run.Log($"Centro del panel sobre Chrome={OwnTarget(hit)}; panel en primer plano={Eligible()}.");
            if (!Eligible() || !OwnTarget(hit)) throw new InvalidOperationException("El campo de Chrome no está disponible para el clic.");
            var previous = Cursor.Position;
            Cursor.Position = point;
            try
            {
                if (!Eligible() || Native.WindowFromPoint(point) != hit || Cursor.Position != point)
                    throw new InvalidOperationException("Cambió el destino del clic.");
                Native.mouse_event(2, 0, 0, 0, 0);
                Native.mouse_event(4, 0, 0, 0, 0);
            }
            finally { if (Cursor.Position == point) Cursor.Position = previous; }
            await Task.Delay(200);
            foreach (char key in "canvas42")
            {
                if (IsDisposed || !Eligible() || !OwnTarget(ForegroundFocus()))
                    throw new InvalidOperationException("Tras el clic, el foco de teclado no pertenece al Chrome de prueba.");
                SendKeys.SendWait(key.ToString());
            }
            for (var attempt = 0; attempt < 30 && !IsDisposed; attempt++)
            {
                if (panel.Current && Native.Title(window).Contains(" INPUT-OK", StringComparison.Ordinal))
                {
                    inputConfirmed = true;
                    run.Log("La página confirmó el texto exacto canvas42 después de un clic.");
                    return;
                }
                await Task.Delay(100);
            }
            throw new InvalidOperationException("La página no confirmó el texto de prueba.");
        }
        catch (Exception error) { Failed = true; run.Log("Comprobación de entrada: " + error.Message); }
        finally { if (!IsDisposed) Close(); }
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
            if (checkInput) Close();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) poll.Dispose();
        base.Dispose(disposing);
    }

    private delegate bool EnumWindow(nint handle, nint data);
    [DllImport("user32")] private static extern bool IsChild(nint parent, nint child);
    [StructLayout(LayoutKind.Sequential)]
    private struct GuiThreadInfo
    {
        public uint Size, Flags;
        public nint Active, Focus, Capture, MenuOwner, MoveSize, Caret;
        public Native.Rect CaretRect;
    }
    private static nint ForegroundFocus()
    {
        var info = new GuiThreadInfo { Size = (uint)Marshal.SizeOf<GuiThreadInfo>() };
        return GetGUIThreadInfo(0, ref info) ? info.Focus : 0;
    }
    [DllImport("user32")] private static extern bool GetGUIThreadInfo(uint thread, ref GuiThreadInfo info);
    [DllImport("user32")] private static extern bool EnumWindows(EnumWindow callback, nint data);
}
