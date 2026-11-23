using System.Runtime.InteropServices;

namespace Canvas.Desktop;

internal static class NativeCanvasFrame
{
    public const int GetMinMaxInfo = 0x0024;

    public static bool MaximizeToMonitor(nint window, nint info)
    {
        const uint NearestMonitor = 2;
        var monitor = MonitorFromWindow(window, NearestMonitor);
        var bounds = new MonitorInfo { Size = (uint)Marshal.SizeOf<MonitorInfo>() };
        if (monitor == 0 || !GetMonitorInfo(monitor, ref bounds))
        {
            return false;
        }

        var limits = Marshal.PtrToStructure<MinMaxInfo>(info);
        limits.MaxPosition = default;
        limits.MaxSize = new NativePoint
        {
            X = bounds.MonitorArea.Right - bounds.MonitorArea.Left,
            Y = bounds.MonitorArea.Bottom - bounds.MonitorArea.Top
        };
        Marshal.StructureToPtr(limits, info, false);
        return true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X, Y;
    }

    public static bool TryGetMonitorArea(nint window, out (int Left, int Top, int Right, int Bottom) area)
    {
        var bounds = new MonitorInfo { Size = (uint)Marshal.SizeOf<MonitorInfo>() };
        var found = GetMonitorInfo(MonitorFromWindow(window, 2), ref bounds);
        var rect = bounds.MonitorArea;
        area = (rect.Left, rect.Top, rect.Right, rect.Bottom);
        return found;
    }

    public static bool HasFullscreenClient(nint window)
    {
        var bounds = new MonitorInfo { Size = (uint)Marshal.SizeOf<MonitorInfo>() };
        var origin = new NativePoint();
        if (!GetMonitorInfo(MonitorFromWindow(window, 2), ref bounds) ||
            !GetClientRect(window, out var client) || !ClientToScreen(window, ref origin))
        {
            return true;
        }

        var monitor = bounds.MonitorArea;
        return origin.X <= monitor.Left && origin.Y <= monitor.Top &&
            origin.X + client.Right >= monitor.Right && origin.Y + client.Bottom >= monitor.Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(nint window, ref NativePoint point);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(nint window, out NativeRect bounds);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public uint Size;
        public NativeRect MonitorArea, WorkArea;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo
    {
        public NativePoint Reserved, MaxSize, MaxPosition, MinTrackSize, MaxTrackSize;
    }

    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint hwnd, uint flags);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    private static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo info);
}
