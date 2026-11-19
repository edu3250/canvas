using System.ComponentModel;
using System.Runtime.InteropServices;

internal static class WindowPlacementApi
{
    const int SwRestore = 9;
    const uint SwpNoZOrder = 0x0004;
    const uint SwpNoActivate = 0x0010;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Placement
    {
        internal uint Length;
        internal uint Flags;
        internal uint ShowCommand;
        internal Point MinPosition;
        internal Point MaxPosition;
        internal Native.Rect NormalPosition;

        internal readonly Rectangle NormalBounds => NormalPosition.ToRectangle();
    }

    internal static Placement Capture(nint hwnd)
    {
        var placement = new Placement { Length = (uint)Marshal.SizeOf<Placement>() };
        if (!GetWindowPlacement(hwnd, ref placement))
            throw LastError(nameof(GetWindowPlacement));
        return placement;
    }

    internal static void Restore(nint hwnd, Placement saved)
    {
        saved.Length = (uint)Marshal.SizeOf<Placement>();
        if (!SetWindowPlacement(hwnd, ref saved))
            throw LastError(nameof(SetWindowPlacement));
    }

    internal static void Normalize(nint hwnd) => ShowWindow(hwnd, SwRestore);

    internal static void Place(nint hwnd, Rectangle screenBounds)
    {
        if (!SetWindowPos(hwnd, 0, screenBounds.X, screenBounds.Y, screenBounds.Width,
            screenBounds.Height, SwpNoZOrder | SwpNoActivate))
            throw LastError(nameof(SetWindowPos));
    }

    static Win32Exception LastError(string operation)
    {
        int code = Marshal.GetLastWin32Error();
        return new Win32Exception(code, $"{operation}: Win32 {code}");
    }

    [DllImport("user32", SetLastError = true)]
    static extern bool GetWindowPlacement(nint hwnd, ref Placement placement);

    [DllImport("user32", SetLastError = true)]
    static extern bool SetWindowPlacement(nint hwnd, ref Placement placement);

    [DllImport("user32")]
    static extern bool ShowWindow(nint hwnd, int command);

    [DllImport("user32", SetLastError = true)]
    static extern bool SetWindowPos(nint hwnd, nint after, int x, int y, int width, int height, uint flags);
}
