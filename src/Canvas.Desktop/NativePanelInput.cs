using System.Runtime.InteropServices;
using System.Windows;

namespace Canvas.Desktop;

internal static class NativePanelInput
{
    public const int ReturnHotkey = 1;
    public const int HotkeyMessage = 0x0312;

    public static bool RegisterReturn(nint owner)
    {
        const uint Alt = 0x0001, Control = 0x0002, NoRepeat = 0x4000, Space = 0x20;
        return RegisterHotKey(owner, ReturnHotkey, Alt | Control | NoRepeat, Space);
    }

    public static void UnregisterReturn(nint owner) => UnregisterHotKey(owner, ReturnHotkey);

    public static bool TryGetPointer(out Point pointer)
    {
        var found = GetCursorPos(out var native);
        pointer = new Point(native.X, native.Y);
        return found;
    }

    public static int ScreenBottom => GetSystemMetrics(1);

    private const int ExtendedStyle = -20;
    private const int Topmost = 0x00000008;
    private const int NoActivateStyle = 0x08000000;

    public static string DescribeStack(nint canvas, nint source)
    {
        string Role(nint handle) => handle == 0 ? "none" : handle == canvas ? "canvas" :
            handle == source ? "source" : "other";
        var foreground = Role(GetForegroundWindow());
        if (source == 0 || !GetWindowRect(source, out var bounds)) return $"{foreground},none,,,,";
        var center = new NativePoint { X = bounds.Left + (bounds.Right - bounds.Left) / 2,
            Y = bounds.Top + (bounds.Bottom - bounds.Top) / 2 };
        var visible = Role(GetAncestor(WindowFromPoint(center), 2));
        return $"{foreground},{visible},{bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}";
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern nint WindowFromPoint(NativePoint point);
    [DllImport("user32.dll")] private static extern nint GetAncestor(nint window, uint flags);
    [DllImport("user32.dll")] private static extern bool GetWindowRect(nint window, out NativeRect rect);

    public static nint ForegroundWindow => GetForegroundWindow();

    public static bool IsForeground(nint window) => GetForegroundWindow() == window;

    public static bool TryReturnForeground(nint canvas, nint panel)
    {
        if (IsForeground(canvas)) return true;
        if (!IsForeground(panel)) return false;
        if (SetForegroundWindow(canvas) && IsForeground(canvas)) return true;
        if (!IsForeground(panel)) return false;

        var caller = GetCurrentThreadId();
        var source = GetWindowThreadProcessId(panel, out _);
        // Probe the panel's UI thread before temporarily sharing its input queue.
        const uint AbortIfHung = 0x0002;
        if (source == 0 || source == caller ||
            SendMessageTimeoutW(panel, 0, 0, 0, AbortIfHung, 100, out _) == 0 ||
            !AttachThreadInput(caller, source, true)) return false;
        try
        {
            return IsForeground(canvas) || (IsForeground(panel) &&
                SetForegroundWindow(canvas) && IsForeground(canvas));
        }
        finally
        {
            AttachThreadInput(caller, source, false);
        }
    }

    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window, out uint process);
    [DllImport("user32.dll")] private static extern bool AttachThreadInput(uint caller, uint source, bool attach);
    [DllImport("user32.dll")] private static extern nint SendMessageTimeoutW(nint window, uint message,
        nuint wParam, nint lParam, uint flags, uint timeout, out nuint result);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    public static bool IsTopmost(nint window) => (GetWindowLong(window, ExtendedStyle) & Topmost) != 0;

    public static bool KeepCanvasBehind(nint canvas, nint application)
    {
        const uint NoSize = 0x0001, NoMove = 0x0002, NoActivate = 0x0010, NoOwnerZOrder = 0x0200;
        var after = IsTopmost(application) ? 0 : application;
        return SetWindowPos(canvas, after, 0, 0, 0, 0, NoSize | NoMove | NoActivate | NoOwnerZOrder);
    }

    public static void StackAtCanvas(nint window, nint canvas, bool above)
    {
        // Preserve relative order among already-visible panels.
        for (var current = GetWindow(canvas, 3); current != 0; current = GetWindow(current, 3))
        {
            if (current != window) continue;
            if (!above) SetWindowPos(window, canvas, 0, 0, 0, 0, 0x213);
            return;
        }
        if (above) SetWindowPos(window, GetWindow(canvas, 3), 0, 0, 0, 0, 0x213);
    }

    [DllImport("user32.dll")] private static extern nint GetWindow(nint window, uint command);

    public static void RaiseWindow(nint window, bool topmost = false)
    {
        const uint NoSize = 0x0001, NoMove = 0x0002, NoActivate = 0x0010, NoOwnerZOrder = 0x0200;
        SetWindowPos(window, topmost ? -1 : 0, 0, 0, 0, 0, NoSize | NoMove | NoActivate | NoOwnerZOrder);
    }

    public static void PreventActivation(nint window) =>
        SetWindowLong(window, ExtendedStyle, GetWindowLong(window, ExtendedStyle) | NoActivateStyle);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(nint window);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong(nint window, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern int SetWindowLong(nint window, int index, int value);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(nint window, nint after, int x, int y, int width, int height, uint flags);

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(nint window, int id, uint modifiers, uint key);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(nint window, int id);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out NativePoint point);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X, Y;
    }
}
