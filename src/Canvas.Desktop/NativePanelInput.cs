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

    public static bool KeepCanvasBehind(nint canvas, nint application)
    {
        const uint NoSize = 0x0001, NoMove = 0x0002, NoActivate = 0x0010, NoOwnerZOrder = 0x0200;
        var after = (GetWindowLong(application, ExtendedStyle) & Topmost) == 0 ? application : 0;
        return SetWindowPos(canvas, after, 0, 0, 0, 0, NoSize | NoMove | NoActivate | NoOwnerZOrder);
    }

    public static void RaiseCanvas(nint canvas)
    {
        const uint NoSize = 0x0001, NoMove = 0x0002, NoActivate = 0x0010, NoOwnerZOrder = 0x0200;
        SetWindowPos(canvas, 0, 0, 0, 0, 0, NoSize | NoMove | NoActivate | NoOwnerZOrder);
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
