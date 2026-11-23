using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Canvas.Desktop;

internal sealed class PanelWheel : IDisposable
{
    private const int MouseWheel = 0x020A, ControlKey = 0x11, LowLevelMouse = 14;
    private const uint RootWindow = 2;
    private readonly Hook callback;
    private readonly nint handle;

    public PanelWheel(nint window, nint canvas, Func<bool> isProjected, Action<System.Windows.Point, int> queueZoom)
    {
        callback = (code, message, data) =>
        {
            if (code >= 0 && message == MouseWheel && GetAsyncKeyState(ControlKey) < 0 &&
                NativePanelInput.IsForeground(window))
            {
                var wheel = Marshal.PtrToStructure<WheelData>(data);
                var root = GetAncestor(WindowFromPoint(wheel.Position), RootWindow);
                if ((root == window || (root == canvas && isProjected())) &&
                    !NativeCanvasFrame.HasFullscreenClient(window))
                {
                    queueZoom(new(wheel.Position.X, wheel.Position.Y), (short)(wheel.MouseData >> 16));
                    return 1;
                }
            }
            return CallNextHookEx(0, code, message, data);
        };
        handle = SetWindowsHookExW(LowLevelMouse, callback, GetModuleHandleW(null), 0);
        if (handle == 0) throw new Win32Exception();
    }

    public void Dispose() => UnhookWindowsHookEx(handle);

    [StructLayout(LayoutKind.Sequential)]
    private struct ScreenPoint { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)]
    private struct WheelData { public ScreenPoint Position; public uint MouseData; }
    private delegate nint Hook(int code, nint message, nint data);
    [DllImport("user32.dll", SetLastError = true)] private static extern nint SetWindowsHookExW(int type, Hook hook, nint module, uint thread);
    [DllImport("user32.dll")] private static extern bool UnhookWindowsHookEx(nint hook);
    [DllImport("user32.dll")] private static extern nint CallNextHookEx(nint hook, int code, nint message, nint data);
    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll")] private static extern nint WindowFromPoint(ScreenPoint point);
    [DllImport("user32.dll")] private static extern nint GetAncestor(nint window, uint flags);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern nint GetModuleHandleW(string? name);
}
