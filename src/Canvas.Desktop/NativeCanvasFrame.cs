using System.Runtime.InteropServices;

namespace Canvas.Desktop;

internal static class NativeCanvasFrame
{
    public const int GetMinMaxInfo = 0x0024;

    public static bool MaximizeToWorkArea(nint window, nint info)
    {
        const uint NearestMonitor = 2;
        var monitor = MonitorFromWindow(window, NearestMonitor);
        var bounds = new MonitorInfo { Size = (uint)Marshal.SizeOf<MonitorInfo>() };
        if (monitor == 0 || !GetMonitorInfo(monitor, ref bounds))
        {
            return false;
        }

        var limits = Marshal.PtrToStructure<MinMaxInfo>(info);
        limits.MaxPosition = new NativePoint
        {
            X = bounds.WorkArea.Left - bounds.MonitorArea.Left,
            Y = bounds.WorkArea.Top - bounds.MonitorArea.Top
        };
        limits.MaxSize = new NativePoint
        {
            X = bounds.WorkArea.Right - bounds.WorkArea.Left,
            Y = bounds.WorkArea.Bottom - bounds.WorkArea.Top
        };
        Marshal.StructureToPtr(limits, info, false);
        return true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X, Y;
    }

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
