using System.Runtime.InteropServices;
using System.Text;

internal static class Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        public int Left, Top, Right, Bottom;
        public Rect(Rectangle r) => (Left, Top, Right, Bottom) = (r.Left, r.Top, r.Right, r.Bottom);
        public readonly Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Thumbnail
    {
        public uint Flags;
        public Rect Destination, Source;
        public byte Opacity;
        public int Visible, ClientOnly;
    }

    [DllImport("dwmapi")] internal static extern int DwmRegisterThumbnail(nint host, nint source, out nint thumbnail);
    [DllImport("dwmapi")] internal static extern int DwmUpdateThumbnailProperties(nint thumbnail, ref Thumbnail properties);
    [DllImport("dwmapi")] internal static extern int DwmUnregisterThumbnail(nint thumbnail);
    [DllImport("user32")] internal static extern bool SetForegroundWindow(nint hwnd);
    [DllImport("user32")] internal static extern nint GetForegroundWindow();
    [DllImport("user32")] internal static extern bool GetWindowRect(nint hwnd, out Rect rect);
    [DllImport("user32")] internal static extern bool SetWindowPos(nint hwnd, nint after, int x, int y, int w, int h, uint flags);
    [DllImport("user32")] internal static extern uint GetDpiForWindow(nint hwnd);
    [DllImport("user32")] internal static extern uint GetWindowThreadProcessId(nint hwnd, out int pid);
    [DllImport("user32", CharSet = CharSet.Unicode)] static extern int GetWindowText(nint hwnd, StringBuilder text, int count);
    [DllImport("user32", CharSet = CharSet.Unicode)] static extern int GetClassName(nint hwnd, StringBuilder text, int count);
    [DllImport("user32")] internal static extern nint GetAncestor(nint hwnd, uint flags);
    [DllImport("user32")] internal static extern nint GetDlgItem(nint dialog, int id);
    [DllImport("user32")] internal static extern bool IsWindowEnabled(nint hwnd);
    [DllImport("user32")] internal static extern nint WindowFromPoint(Point point);
    [DllImport("user32")] internal static extern bool PrintWindow(nint hwnd, nint dc, uint flags);
    [DllImport("user32")] internal static extern void mouse_event(uint flags, uint dx, uint dy, uint data, nuint extra);

    internal static int Pid(nint hwnd) { GetWindowThreadProcessId(hwnd, out var pid); return pid; }
    internal static string Title(nint hwnd) { var text = new StringBuilder(512); GetWindowText(hwnd, text, text.Capacity); return text.ToString(); }
    internal static string Class(nint hwnd) { var text = new StringBuilder(128); GetClassName(hwnd, text, text.Capacity); return text.ToString(); }
    internal static Rectangle Bounds(nint hwnd) => GetWindowRect(hwnd, out var rect) ? rect.ToRectangle() : throw new InvalidOperationException("GetWindowRect");

    internal static void Project(nint thumbnail, Rectangle bounds, bool visible)
    {
        var props = new Thumbnail { Flags = 1 | 8, Destination = new(bounds), Visible = visible ? 1 : 0 };
        Marshal.ThrowExceptionForHR(DwmUpdateThumbnailProperties(thumbnail, ref props));
    }
}
