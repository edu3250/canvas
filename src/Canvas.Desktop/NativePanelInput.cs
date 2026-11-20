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

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(nint window);

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
