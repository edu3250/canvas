using System.Diagnostics;
using System.Runtime.InteropServices;

internal sealed class Host : Form
{
    readonly Run run;
    readonly bool automatic;
    readonly Button wide = new() { Text = "Ancho" }, narrow = new() { Text = "Estrecho" }, quit = new() { Text = "Salir" };
    nint preview, neighbor;
    Rectangle panel, next;
    bool activated;
    internal bool Failed { get; private set; }

    internal Host(Run run, bool automatic)
    {
        this.run = run;
        this.automatic = automatic;
        Text = "Ensayo de panel nativo";
        BackColor = Color.FromArgb(52, 56, 63);
        StartPosition = FormStartPosition.Manual;
        FormBorderStyle = FormBorderStyle.None;
        Bounds = Screen.PrimaryScreen!.WorkingArea;
        wide.SetBounds(8, 4, 90, 32);
        narrow.SetBounds(104, 4, 90, 32);
        quit.SetBounds(204, 4, 80, 32);
        Controls.AddRange([wide, narrow, quit]);
        wide.Click += (_, _) => Overview(1475);
        narrow.Click += (_, _) => Overview(492);
        quit.Click += (_, _) => Close();
        MouseDown += (_, e) => { if (e.Button == MouseButtons.Left && panel.Contains(e.Location)) ActivateCandidate(); };
        Shown += async (_, _) =>
        {
            try
            {
                Marshal.ThrowExceptionForHR(Native.DwmRegisterThumbnail(Handle, run.Candidate, out preview));
                Marshal.ThrowExceptionForHR(Native.DwmRegisterThumbnail(Handle, run.Neighbor, out neighbor));
                run.Log($"host=0x{Handle:X} DPI={Native.GetDpiForWindow(Handle)} área={Bounds} monitor={Screen.FromControl(this).Bounds}");
                if (ClientSize.Width < 500 || ClientSize.Height < 250) throw new InvalidOperationException("Área insuficiente");
                Overview(1475);
                Activate();
                if (automatic) await Replay();
            }
            catch (Exception ex) { Failed = true; run.Log("ERROR " + ex.Message); Close(); }
        };
        FormClosed += (_, _) =>
        {
            if (preview != 0) Native.DwmUnregisterThumbnail(preview);
            if (neighbor != 0) Native.DwmUnregisterThumbnail(neighbor);
        };
    }

    void Overview(int width)
    {
        activated = false;
        panel = new(8, 44, Math.Min(width, ClientSize.Width - 188), Math.Min(800, ClientSize.Height - 52));
        next = new(ClientSize.Width - 168, panel.Top, 160, 120);
        Native.Project(preview, panel, true);
        Native.Project(neighbor, next, true);
        run.Log($"vista ancho de referencia={width} solicitado={RectangleToScreen(panel)}");
    }

    void ActivateCandidate()
    {
        if (!run.IsOurs(run.Candidate, run.CandidatePid)) throw new InvalidOperationException("Identidad del candidato perdida");
        var target = RectangleToScreen(panel);
        var before = Native.GetForegroundWindow();
        Native.Project(preview, panel, false);
        if (!Native.SetWindowPos(run.Candidate, 0, target.X, target.Y, target.Width, target.Height, 0x10))
            throw new InvalidOperationException("SetWindowPos falló");
        var focus = Native.SetForegroundWindow(run.Candidate);
        activated = true;
        run.Log($"clic previo=0x{before:X} petición foco={focus} posterior=0x{Native.GetForegroundWindow():X} obtenido={Native.Bounds(run.Candidate)} DPI editor={Native.GetDpiForWindow(run.Candidate)}");
    }

    // WindowFromPoint comprueba la superficie expuesta; el color procede del contenido de la miniatura.
    bool Witness(string stage)
    {
        var point = PointToScreen(new(next.Left + next.Width / 2, next.Top + next.Height / 2));
        bool above = Native.GetAncestor(Native.WindowFromPoint(point), 2) == Handle;
        using var pixel = new Bitmap(1, 1);
        using var g = Graphics.FromImage(pixel);
        g.CopyFromScreen(point, Point.Empty, new Size(1, 1));
        var color = pixel.GetPixel(0, 0);
        var expected = Fixture.Witness;
        bool visible = above && Math.Abs(color.R - expected.R) < 10 && Math.Abs(color.G - expected.G) < 10 && Math.Abs(color.B - expected.B) < 10;
        run.Log($"vecino {stage} centro={point} host expuesto={above} RGB={color.R},{color.G},{color.B} visible={visible}");
        return visible;
    }

    // PrintWindow recibe únicamente el HWND identificado. Las imágenes se quedan en evidencia-local.
    void CaptureEditor(string name)
    {
        if (!run.IsOurs(run.Candidate, run.CandidatePid)) throw new InvalidOperationException("Captura sin identidad");
        var size = Native.Bounds(run.Candidate).Size;
        using var image = new Bitmap(size.Width, size.Height);
        using (var g = Graphics.FromImage(image))
        {
            var dc = g.GetHdc();
            try { if (!Native.PrintWindow(run.Candidate, dc, 2)) throw new InvalidOperationException("PrintWindow falló"); }
            finally { g.ReleaseHdc(dc); }
        }
        image.Save(run.Evidence(name + ".png"), System.Drawing.Imaging.ImageFormat.Png);
    }

    static async Task WaitFor(Func<bool> condition, string label, int ms = 4500)
    {
        var timer = Stopwatch.StartNew();
        while (!condition())
        {
            if (timer.ElapsedMilliseconds >= ms) throw new TimeoutException(label);
            await Task.Delay(80);
        }
    }

    void ClickTestWindow(Point point, nint expected)
    {
        bool Eligible()
        {
            var foreground = Native.GetForegroundWindow();
            var targetRoot = Native.GetAncestor(expected, 2);
            if (targetRoot == Handle && foreground == Handle) return true;
            if (!run.IsOurs(run.Candidate, run.CandidatePid)) return false;
            return (targetRoot == Handle && foreground == run.Candidate) ||
                (targetRoot == foreground && IsDialog(foreground));
        }
        if (!Eligible()) throw new InvalidOperationException("Otra ventana tiene el foco; se omite el clic");
        if (Native.WindowFromPoint(point) != expected) throw new InvalidOperationException("Punto cubierto; se omite el clic");
        var previous = Cursor.Position;
        Cursor.Position = point;
        try
        {
            if (!Eligible() || Cursor.Position != point || Native.WindowFromPoint(point) != expected) throw new InvalidOperationException("Destino del clic cambió");
            Native.mouse_event(2, 0, 0, 0, 0);
            Native.mouse_event(4, 0, 0, 0, 0);
        }
        finally { if (Cursor.Position == point) Cursor.Position = previous; }
    }

    void KeysTo(nint window, string keys)
    {
        if (Native.GetForegroundWindow() != window || !run.IsOurs(run.Candidate, run.CandidatePid) ||
            (window != run.Candidate && !IsDialog(window))) throw new InvalidOperationException("Cambió el foco; se omiten las teclas");
        SendKeys.SendWait(keys);
    }

    bool IsDialog(nint hwnd) => hwnd != 0 && Native.Class(hwnd) == "#32770" &&
        Native.GetAncestor(hwnd, 3) == run.Candidate;

    async Task Replay()
    {
        int index = 0;
        foreach (var button in new[] { wide, narrow })
        {
            index++;
            try
            {
                await Task.Delay(600);
                ClickTestWindow(button.PointToScreen(new(10, 10)), button.Handle);
                await WaitFor(() => !activated && Native.GetForegroundWindow() == Handle, "Vista sin foco");
                await Task.Delay(350);
                ObservePreview();
                var candidatePoint = PointToScreen(new(panel.Left + 24, panel.Top + 24));
                ClickTestWindow(candidatePoint, Handle);
                await WaitFor(() => activated && Native.GetForegroundWindow() == run.Candidate, "El clic no dejó el editor en primer plano");
                await Task.Delay(750);
                run.Log($"tamaño {index} final={Native.Bounds(run.Candidate)} foco=0x{Native.GetForegroundWindow():X}");
                CaptureEditor("editor-" + index);
                if (!Witness("editor " + index)) Failed = true;
                string first = $"ANTES{index}", second = $"DESPUES{index}";
                KeysTo(run.Candidate, "^{END}");
                KeysTo(run.Candidate, first);
                KeysTo(run.Candidate, "^s");
                await WaitFor(() => File.ReadAllText(run.Document).Contains(first), $"Falta {first} al guardar");
                run.Log($"documento contiene {first}");
                KeysTo(run.Candidate, "^o");
                await WaitFor(() => IsDialog(Native.GetForegroundWindow()), "Falta diálogo nativo con propietario esperado");
                var dialog = Native.GetForegroundWindow();
                run.Log($"diálogo=0x{dialog:X} clase={Native.Class(dialog)} propietario raíz=0x{Native.GetAncestor(dialog, 3):X}");
                Witness("diálogo " + index);
                await Task.Delay(550);
                var cancel = Native.GetDlgItem(dialog, 2); // IDCANCEL
                await WaitFor(() => cancel != 0 && Native.IsWindowEnabled(cancel), "Cancelar no está disponible");
                var cancelBounds = Native.Bounds(cancel);
                var cancelPoint = new Point(cancelBounds.Left + cancelBounds.Width / 2, cancelBounds.Top + cancelBounds.Height / 2);
                for (int attempt = 0; attempt < 10 && Native.WindowFromPoint(cancelPoint) != cancel; attempt++)
                {
                    await Task.Delay(100);
                    cancelBounds = Native.Bounds(cancel);
                    cancelPoint = new(cancelBounds.Left + cancelBounds.Width / 2, cancelBounds.Top + cancelBounds.Height / 2);
                }
                if (Native.WindowFromPoint(cancelPoint) == cancel)
                {
                    run.Log($"cancelación por clic en IDCANCEL, rect={cancelBounds}");
                    ClickTestWindow(cancelPoint, cancel);
                }
                else
                {
                    run.Log($"cancelación por Escape, IDCANCEL sin superficie expuesta, rect={cancelBounds}");
                    KeysTo(dialog, "{ESC}");
                }
                await WaitFor(() => Native.GetForegroundWindow() == run.Candidate, "El diálogo no devolvió el foco");
                KeysTo(run.Candidate, second);
                KeysTo(run.Candidate, "^s");
                await WaitFor(() => File.ReadAllText(run.Document).Contains(first + second), $"Falta {first + second} al guardar");
                run.Log($"documento contiene {first + second} tras Cancelar; foco=0x{Native.GetForegroundWindow():X}");
                if (!Witness("tras diálogo " + index)) Failed = true;
            }
            catch (Exception ex)
            {
                Failed = true;
                run.Log($"ERROR tamaño {index}: {ex.Message}");
                var foreground = Native.GetForegroundWindow();
                run.Log($"foco al fallo=0x{foreground:X} diálogo propio={IsDialog(foreground)}");
                if (foreground != Handle && foreground != run.Candidate) break;
            }
        }
        File.Copy(run.Document, run.Evidence("documento-final.txt"));
        Close();
    }

    void ObservePreview()
    {
        // Muestrea solo puntos del host dentro del rectángulo asignado a DWM.
        using var sample = new Bitmap(1, 1);
        using var g = Graphics.FromImage(sample);
        int painted = 0, exposed = 0;
        for (int row = 1; row <= 3; row++)
        for (int col = 1; col <= 5; col++)
        {
            var point = PointToScreen(new(panel.Left + col * panel.Width / 6, panel.Top + row * panel.Height / 4));
            if (Native.WindowFromPoint(point) != Handle) continue;
            exposed++;
            g.CopyFromScreen(point, Point.Empty, sample.Size);
            if (sample.GetPixel(0, 0).ToArgb() != BackColor.ToArgb()) painted++;
        }
        run.Log($"vista previa editor: puntos expuestos={exposed}/15, distintos del fondo={painted}");
        if (painted == 0) Failed = true;
    }
}
