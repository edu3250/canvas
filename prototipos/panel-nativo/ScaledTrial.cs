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

    internal ScaledTrial(Run run, nint browser, int pid)
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
        Shown += (_, _) =>
        {
            try
            {
                if (!run.IsOurs(browser, pid) || !GetClientRect(browser, out var client))
                    throw new InvalidOperationException("No se identificó el cliente de Chrome.");
                original = client.ToRectangle().Size;
                Marshal.ThrowExceptionForHR(Native.DwmRegisterThumbnail(Handle, browser, out thumbnail));
                TopMost = true;
                if (!Native.SetForegroundWindow(browser) && Native.GetForegroundWindow() != browser)
                    throw new InvalidOperationException("Chrome no recibió el foco.");
                Project();
                watch.Start();
            }
            catch (Exception error) { run.Log(error.Message); Close(); Failed = true; }
        };
        watch.Tick += (_, _) =>
        {
            if (!run.IsOurs(browser, pid)) { Close(); return; }
            var foreground = Native.GetForegroundWindow();
            if (foreground != browser && foreground != Handle) { Close(); return; }
            if (!GetClientRect(browser, out var size) || size.ToRectangle().Size != original)
            {
                run.Log("Cambió el tamaño del cliente de Chrome.");
                Failed = true;
                Close();
            }
        };
    }

    internal bool Failed { get; private set; }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == 0x21) { message.Result = 3; return; } // MA_NOACTIVATE
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

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        for (int i=0; i<4; i++)
        {
            if (!new Rectangle(8+i*94,8,88,38).Contains(e.Location)) continue;
            if (i==3) Close(); else { zoom=scales[i]; Project(); }
            return;
        }
        if (!destination.Contains(e.Location) || !run.IsOurs(browser,pid) || Native.GetForegroundWindow()!=browser) return;
        var pos = ToSource(e.Location, destination, source);
        var target = browser;
        while (true)
        {
            var child = ChildWindowFromPointEx(target, pos, 7);
            if (child==0 || child==target) break;
            if (Native.Pid(child)!=pid) return;
            MapWindowPoints(target, child, ref pos, 1);
            target = child;
        }
        var coords = (nint)((pos.X & 0xffff) | ((pos.Y & 0xffff)<<16));
        if (Native.GetAncestor(target,2)!=browser || !run.IsOurs(browser,pid)) return;
        bool sent = PostMessageW(target,0x201,1,coords) && PostMessageW(target,0x202,0,coords);
        status = sent ? "Clic enviado. Comprueba el cursor y escribe en Chrome." : "Windows rechazó el clic.";
        Invalidate();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        watch.Stop();
        if (thumbnail!=0) Native.DwmUnregisterThumbnail(thumbnail);
        watch.Dispose();
        base.OnFormClosed(e);
    }

    [DllImport("user32")] private static extern bool GetClientRect(nint window, out Native.Rect rect);
    [DllImport("user32")] private static extern nint ChildWindowFromPointEx(nint window, Point point, uint flags);
    [DllImport("user32")] private static extern int MapWindowPoints(nint from, nint to, ref Point point, uint count);
    [DllImport("user32")] private static extern bool PostMessageW(nint window, uint msg, nuint wParam, nint lParam);
}
