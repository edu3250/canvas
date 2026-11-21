using Canvas.Domain;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Canvas.Desktop;

internal sealed class NativeWindowPlacement : IWindowPlacementAccess
{
    private const string MarkerPrefix = "EscritorioCanvas.WindowRecovery.";
    private static readonly nint MarkerValue = 1;
    public int SessionId { get; } = GetCurrentSessionId();

    public WindowIdentity? Identify(long handle)
    {
        if (GetWindowThreadProcessId((nint)handle, out var processId) == 0)
        {
            return null;
        }

        Process process;
        try
        {
            process = Process.GetProcessById(processId);
        }
        catch (ArgumentException)
        {
            return null;
        }

        using (process)
        {
            if (process.SessionId != SessionId)
            {
                return null;
            }

            var started = process.StartTime.ToUniversalTime().Ticks;
            return GetWindowThreadProcessId((nint)handle, out var currentProcessId) != 0 && processId == currentProcessId
                ? new WindowIdentity(handle, processId, started, SessionId)
                : null;
        }
    }

    public WindowPlacement Capture(long handle)
    {
        var placement = new NativePlacement { Length = (uint)Marshal.SizeOf<NativePlacement>() };
        if (!GetWindowPlacement((nint)handle, ref placement))
        {
            throw new Win32Exception();
        }

        return new WindowPlacement(placement.Flags, placement.ShowCommand,
            placement.MinPosition.ToPoint(), placement.MaxPosition.ToPoint(),
            new WindowRectangle(placement.Left, placement.Top, placement.Right, placement.Bottom));
    }

    public void Mark(long handle, Guid marker)
    {
        if (!SetProp((nint)handle, MarkerName(marker), MarkerValue))
        {
            throw new Win32Exception();
        }
    }

    public bool HasMarker(long handle, Guid marker) =>
        GetProp((nint)handle, MarkerName(marker)) == MarkerValue;

    public void RemoveMarker(long handle, Guid marker) =>
        RemoveProp((nint)handle, MarkerName(marker));

    public void Restore(long handle, WindowPlacement placement)
    {
        var native = new NativePlacement
        {
            Length = (uint)Marshal.SizeOf<NativePlacement>(),
            Flags = placement.Flags,
            ShowCommand = placement.ShowCommand,
            MinPosition = new NativePoint { X = placement.MinPosition.X, Y = placement.MinPosition.Y },
            MaxPosition = new NativePoint { X = placement.MaxPosition.X, Y = placement.MaxPosition.Y },
            Left = placement.NormalPosition.Left,
            Top = placement.NormalPosition.Top,
            Right = placement.NormalPosition.Right,
            Bottom = placement.NormalPosition.Bottom
        };

        if (!SetWindowPlacement((nint)handle, in native))
        {
            throw new Win32Exception();
        }
    }

    public (WindowRectangle Visible, (int Width, int Height) Size)? ReadNormalScreenBounds(long handle)
    {
        var window = (nint)handle;
        return !IsIconic(window) && !IsZoomed(window) && GetWindowRect(window, out var bounds)
            ? (VisibleFrame(window, bounds), (bounds.Right - bounds.Left, bounds.Bottom - bounds.Top))
            : null;
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(nint window, out WindowRectangle bounds);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(nint window);

    [DllImport("user32.dll")]
    private static extern bool IsZoomed(nint window);

    public void MoveTo(WindowIdentity identity, WindowInstance instance, int x, int y, (int Width, int Height)? size)
    {
        EnsureCurrent(instance);
        var current = Capture(identity.Handle);
        if (current.ShowCommand != 1)
        {
            EnsureCurrent(instance);
            Restore(identity.Handle, current with { ShowCommand = 1, Flags = 0 });
        }

        Reposition(identity, instance, x, y, size);
    }

    public void Reposition(WindowIdentity identity, WindowInstance instance, int x, int y, (int Width, int Height)? size = null)
    {
        EnsureCurrent(instance);
        var window = (nint)identity.Handle;
        if (!GetWindowRect(window, out var full))
        {
            throw new Win32Exception();
        }

        var visible = VisibleFrame(window, full);
        const uint NoSize = 0x0001, NoZOrder = 0x0004, NoActivate = 0x0010;
        var flags = NoZOrder | NoActivate;
        if (size is null)
        {
            flags |= NoSize;
        }

        if (!SetWindowPos(window, 0, x - (visible.Left - full.Left), y - (visible.Top - full.Top),
            size?.Width ?? 0, size?.Height ?? 0, flags))
        {
            throw new Win32Exception();
        }
    }

    private static WindowRectangle VisibleFrame(nint window, WindowRectangle fallback)
    {
        const uint ExtendedFrameBounds = 9;
        return DwmGetWindowAttribute(window, ExtendedFrameBounds, out var frame, Marshal.SizeOf<WindowRectangle>()) >= 0 &&
            frame.Right > frame.Left && frame.Bottom > frame.Top ? frame : fallback;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(nint window, uint attribute, out WindowRectangle bounds, int size);

    public static void EnsureCurrent(WindowInstance instance)
    {
        if (!instance.IsCurrent())
        {
            throw new InvalidOperationException("The window is no longer available.");
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(nint window, nint after, int x, int y, int width, int height, uint flags);

    private static string MarkerName(Guid marker) => MarkerPrefix + marker.ToString("N");

    private static int GetCurrentSessionId()
    {
        using var process = Process.GetCurrentProcess();
        return process.SessionId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;

        public readonly WindowPoint ToPoint() => new(X, Y);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePlacement
    {
        public uint Length;
        public uint Flags;
        public uint ShowCommand;
        public NativePoint MinPosition;
        public NativePoint MaxPosition;
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(nint window, out int processId);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowPlacement(nint window, ref NativePlacement placement);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPlacement(nint window, in NativePlacement placement);

    [DllImport("user32.dll", EntryPoint = "SetPropW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetProp(nint window, string name, nint data);

    [DllImport("user32.dll", EntryPoint = "GetPropW", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern nint GetProp(nint window, string name);

    [DllImport("user32.dll", EntryPoint = "RemovePropW", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern nint RemoveProp(nint window, string name);
}
