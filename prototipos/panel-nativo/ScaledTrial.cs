using System.Runtime.InteropServices;

internal sealed class ScaledTrial : Form
{
    private readonly Run run;
    private readonly nint browser;
    private readonly int pid;
    private nint thumbnail;
    private Size original;
    private double zoom = 1;
    private Rectangle destination, source;
    private readonly System.Windows.Forms.Timer watch = new() { Interval = 200 };
    private string status = "Haz clic en el campo y escribe.";
    private static readonly double[] scales = [0.3, 1, 3];

    internal ScaledTrial(Run run, nint browser, int pid, bool checkInput)
    {
        this.run = run;
        this.browser = browser;
        this.pid = pid;
        Text = "Ensayo de interacción a escala";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = Screen.PrimaryScreen!.Bounds;
        BackColor = Color.FromArgb(22, 26, 34);
        ForeColor = Color.White;
        DoubleBuffered = true;
        SetStyle(ControlStyles.Selectable, false);
        Shown += (_, _) =>
        {
            try
            {
                if (!run.IsOurs(browser, pid) || !GetClientRect(browser, out var client))
                    throw new InvalidOperationException("No se identificó el cliente de Chrome.");
                original = client.ToRectangle().Size;
                Marshal.ThrowExceptionForHR(Native.DwmRegisterThumbnail(Handle, browser, out thumbnail));
                TopMost = true;
                Native.SetForegroundWindow(browser);
                Project();
                watch.Start();
                if (checkInput) _ = VerifyInput();
            }
            catch (Exception error) { run.Log(error.Message); Close(); Failed = true; }
        };
        watch.Tick += (_, _) =>
        {
            if (!IsWindow(browser) || Native.Pid(browser) != pid)
            {
                run.Log("La ventana de Chrome del ensayo dejó de existir.");
                Failed = true;
                Close();
                return;
            }
            var foreground = Native.GetForegroundWindow();
            if (!run.IsOurs(browser, pid) || (foreground != browser && foreground != Handle))
            {
                if (TopMost)
                {
                    TopMost = false;
                    status = "Ensayo en pausa. Vuelve y haz clic en el campo para continuar.";
                    run.Log("Entrada en pausa por cambio de foco o de documento.");
                    Invalidate();
                }
                return;
            }
            if (!TopMost) TopMost = true;
            if (!GetClientRect(browser, out var size) || size.ToRectangle().Size != original)
            {
                run.Log("Cambió el tamaño del cliente de Chrome.");
                Failed = true;
                Close();
            }
        };
    }

    internal bool Failed { get; private set; }

    protected override CreateParams CreateParams
    {
        get
        {
            var parameters = base.CreateParams;
            parameters.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
            return parameters;
        }
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == 0x21 && Native.GetForegroundWindow() == browser)
        {
            message.Result = 3; // MA_NOACTIVATE: preserve the caret in Chrome.
            return;
        }
        // A normal WinForms mouse-down focuses the form even after MA_NOACTIVATE.
        if (message.Msg == 0x0201) return;
        if (message.Msg == 0x0202)
        {
            long packed = message.LParam.ToInt64();
            HandleClick(new Point((short)packed, (short)(packed >> 16)));
            return;
        }
        base.WndProc(ref message);
    }

    private static (Rectangle Source, Rectangle Destination) Projected(Size content, Rectangle area, double scale)
    {
        var w = Math.Max(1, Math.Min(content.Width, (int)Math.Floor(area.Width / scale)));
        var h = Math.Max(1, Math.Min(content.Height, (int)Math.Floor(area.Height / scale)));
        var dw = Math.Max(1, (int)Math.Round(w * scale));
        var dh = Math.Max(1, (int)Math.Round(h * scale));
        return (new((content.Width-w)/2, (content.Height-h)/2, w, h),
            new(area.X+(area.Width-dw)/2, area.Y+(area.Height-dh)/2, dw, dh));
    }

    private static Point ToSource(Point point, Rectangle dest, Rectangle src) =>
        new(src.X + (point.X-dest.X)*src.Width/dest.Width, src.Y + (point.Y-dest.Y)*src.Height/dest.Height);

    internal static int CheckProjection()
    {
        var size = new Size(1000, 800);
        var area = new Rectangle(0, 60, 1500, 900);
        foreach (var scale in scales)
        {
            var (src, dest) = Projected(size, area, scale);
            var center = ToSource(new(dest.Left+dest.Width/2,dest.Top+dest.Height/2), dest, src);
            if (!area.Contains(dest) || !new Rectangle(Point.Empty,size).Contains(src) ||
                Math.Abs(center.X-500)>1 || Math.Abs(center.Y-400)>1 || ToSource(dest.Location,dest,src)!=src.Location)
                return 1;
        }
        return 0;
    }

    private void Project()
    {
        (source, destination) = Projected(original, new Rectangle(0, 60, ClientSize.Width, ClientSize.Height-60), zoom);
        var props = new Native.Thumbnail { Flags = 31, Destination = new(destination), Source = new(source),
            Visible = 1, ClientOnly = 1, Opacity = 255 };
        Marshal.ThrowExceptionForHR(Native.DwmUpdateThumbnailProperties(thumbnail, ref props));
        run.Log($"Escala {zoom:P0}; cliente {original.Width} × {original.Height}.");
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        for (int i=0; i<4; i++)
        {
            var rect = new Rectangle(8+i*94,8,88,38);
            e.Graphics.FillRectangle(Brushes.DimGray, rect);
            TextRenderer.DrawText(e.Graphics, i==3 ? "Terminar" : scales[i].ToString("P0"), Font, rect, Color.White);
        }
        TextRenderer.DrawText(e.Graphics, status, Font, new Point(392,20), ForeColor);
    }

    private void HandleClick(Point point)
    {
        run.Log($"Clic del panel {point.X}, {point.Y}.");
        for (int i=0; i<4; i++)
        {
            if (!new Rectangle(8+i*94,8,88,38).Contains(point)) continue;
            if (i==3) { run.Log("Salida con el botón Terminar."); Close(); }
            else if (FocusBrowser()) { zoom=scales[i]; Project(); }
            return;
        }
        if (!destination.Contains(point) || !FocusBrowser()) return;
        var pos = ToSource(point, destination, source);
        BeginInvoke(new Action(() => ForwardClick(pos)));
    }

    private async void ForwardClick(Point pos)
    {
        if (!run.IsOurs(browser, pid) || Native.GetForegroundWindow() != browser) return;
        Capture = false;
        if (!ClientToScreen(browser, ref pos)) return;
        var local = PointToClient(pos);
        var previous = Cursor.Position;
        using var aperture = new Region(ClientRectangle);
        aperture.Exclude(new Rectangle(local.X - 2, local.Y - 2, 5, 5));
        Region = aperture;
        try
        {
            bool CanClick() => Native.GetForegroundWindow() == browser && run.IsOurs(browser, pid) &&
                Native.GetAncestor(Native.WindowFromPoint(pos), 2) == browser;
            if (!CanClick())
            {
                status = "El punto de Chrome está cubierto. Vuelve a hacer clic.";
                run.Log("Clic omitido: destino nativo no disponible.");
                return;
            }
            Cursor.Position = pos;
            if (Cursor.Position != pos || !CanClick()) { run.Log("El cursor o su destino cambió."); return; }
            run.Log("Destino nativo verificado.");
            Native.mouse_event(2, 0, 0, 0, 0);
            Native.mouse_event(4, 0, 0, 0, 0);
            await Task.Delay(100);
            status = "Escribe en el campo de Chrome.";
        }
        finally
        {
            if (Cursor.Position == pos) Cursor.Position = previous;
            if (!IsDisposed) { Region = null; Invalidate(); }
        }
    }

    private async Task VerifyInput()
    {
        try
        {
            await Task.Delay(900);
            async Task ClickPanel(Point point)
            {
                if (!run.IsOurs(browser, pid) || Native.GetForegroundWindow() != browser)
                    throw new InvalidOperationException("El ensayo perdió el primer plano.");
                var screen = PointToScreen(point);
                var previous = Cursor.Position;
                Cursor.Position = screen;
                try
                {
                    await Task.Delay(80);
                    if (Native.WindowFromPoint(screen) != Handle || Cursor.Position != screen ||
                        Native.GetForegroundWindow() != browser || !run.IsOurs(browser, pid))
                        throw new InvalidOperationException("El panel no está disponible para el clic de prueba.");
                    Native.mouse_event(2, 0, 0, 0, 0);
                    await Task.Delay(50);
                    Native.mouse_event(4, 0, 0, 0, 0);
                    await Task.Delay(500);
                }
                finally { if (Cursor.Position == screen) Cursor.Position = previous; }
            }
            void Type(string keys)
            {
                if (!run.IsOurs(browser,pid) || Native.GetForegroundWindow()!=browser ||
                    !Native.Title(browser).Contains(" FIELD", StringComparison.Ordinal))
                    throw new InvalidOperationException($"Campo sin confirmar: foco navegador={Native.GetForegroundWindow()==browser}; identidad={run.IsOurs(browser,pid)}; campo={Native.Title(browser).Contains(" FIELD")}.");
                SendKeys.SendWait(keys);
            }
            Point Center() => new(destination.Left+destination.Width/2, destination.Top+destination.Height/2);
            await ClickPanel(Center());
            Type("canvas2{LEFT}");
            await ClickPanel(new Point(42, 24));
            if (zoom != 0.3) throw new InvalidOperationException("No se seleccionó 30 %.");
            Type("4");
            await Task.Delay(500);
            if (!Native.Title(browser).Contains("INPUT-OK")) throw new InvalidOperationException("Fallo de inserción al 30 %.");
            await ClickPanel(new Point(235, 24));
            if (zoom != 3) throw new InvalidOperationException("No se seleccionó 300 %.");
            await ClickPanel(Center());
            Type("x");
            await Task.Delay(500);
            if (!Native.Title(browser).Contains("SCALE-OK")) throw new InvalidOperationException("Fallo de inserción al 300 %.");
            run.Log("El campo confirmó canvas42 al 30 % y canvas42x después del clic al 300 %.");
        }
        catch (Exception error) { Failed = true; run.Log(error.Message); }
        finally { if (!IsDisposed) Close(); }
    }
    private bool FocusBrowser()
    {
        run.Log($"Clic válido={run.IsOurs(browser,pid)}; foco navegador={Native.GetForegroundWindow()==browser}; foco panel={Native.GetForegroundWindow()==Handle}.");
        if (!run.IsOurs(browser, pid)) return false;
        if (Native.GetForegroundWindow() == browser) return true;
        if (Native.GetForegroundWindow() == Handle)
        {
            TopMost = true;
            Native.SetForegroundWindow(browser);
            if (Native.GetForegroundWindow() == browser) return true;
        }
        status = "Chrome aún no tiene el foco. Vuelve a hacer clic en el campo.";
        Invalidate();
        return false;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        watch.Stop();
        if (thumbnail!=0) Native.DwmUnregisterThumbnail(thumbnail);
        watch.Dispose();
        base.OnFormClosed(e);
    }

    [DllImport("user32")] private static extern bool GetClientRect(nint window, out Native.Rect rect);
    [DllImport("user32")] private static extern bool IsWindow(nint window);
    [DllImport("user32")] private static extern bool ClientToScreen(nint window, ref Point point);
}