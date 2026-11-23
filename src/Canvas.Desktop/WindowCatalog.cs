using Canvas.Domain;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Canvas.Desktop;

internal sealed record WindowCandidate(WindowIdentity Identity, string Title, WindowRectangle NormalBounds)
{
    public long Handle => Identity.Handle;
}

internal sealed class WindowCatalog
{
    private readonly NativeWindowPlacement placement = new();

    public IReadOnlyList<WindowCandidate> List()
    {
        var candidates = new List<WindowCandidate>();
        EnumWindows((handle, _) =>
        {
            if (Find(handle.ToInt64()) is { } candidate)
            {
                candidates.Add(candidate);
            }
            return true;
        }, 0);
        return candidates.OrderBy(candidate => candidate.Title, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }

    public WindowCandidate? Find(long handle)
    {
        var hwnd = (nint)handle;
        const uint Cloaked = 14;
        if (hwnd == GetShellWindow() || hwnd == GetDesktopWindow() || hwnd == GetShellTaskbar() ||
            !IsWindowVisible(hwnd) || GetWindow(hwnd, 4) != 0 ||
            DwmGetWindowAttribute(hwnd, Cloaked, out var cloaked, sizeof(int)) < 0 || cloaked != 0)
        {
            return null;
        }

        try
        {
            if (placement.Identify(handle) is not { } identity ||
                identity.ProcessId == Environment.ProcessId || !IsNonElevated(identity.ProcessId))
            {
                return null;
            }

            var title = new StringBuilder(512);
            if (GetWindowText(hwnd, title, title.Capacity) == 0)
            {
                return null;
            }

            var bounds = placement.Capture(handle).NormalPosition;
            return bounds.Right > bounds.Left && bounds.Bottom > bounds.Top && placement.Identify(handle) == identity
                ? new WindowCandidate(identity, title.ToString(), bounds)
                : null;
        }
        catch (Exception error) when (error is Win32Exception or InvalidOperationException)
        {
            return null;
        }
    }

    public WindowInstance? Track(WindowIdentity identity)
    {
        try
        {
            return WindowInstance.Track(placement, identity);
        }
        catch (Exception error) when (error is Win32Exception or InvalidOperationException)
        {
            return null;
        }
    }

    public static bool IsLive(WindowIdentity identity, WindowInstance instance)
    {
        try
        {
            return instance.IsCurrent() && IsWindowVisible((nint)identity.Handle);
        }
        catch (Exception error) when (error is Win32Exception or InvalidOperationException)
        {
            return false;
        }
    }

    public static bool IsMinimized(long handle) => IsIconic((nint)handle);

    public static bool IsVisible(long handle) => IsWindowVisible((nint)handle);

    private static bool IsNonElevated(int processId)
    {
        const uint QueryLimitedInformation = 0x1000;
        const uint TokenQuery = 0x0008;
        const int TokenElevation = 20;
        using var process = OpenProcess(QueryLimitedInformation, false, processId);
        if (process.IsInvalid || !OpenProcessToken(process, TokenQuery, out var token))
        {
            return false;
        }

        using (token)
        {
            return GetTokenInformation(token, TokenElevation, out var elevated, sizeof(int), out _) && elevated == 0;
        }
    }

    private static nint GetShellTaskbar() => FindWindow("Shell_TrayWnd", null);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint FindWindow(string className, string? title);

    [DllImport("user32.dll")]
    private static extern nint GetShellWindow();

    [DllImport("user32.dll")]
    private static extern nint GetDesktopWindow();

    private delegate bool WindowCallback(nint window, nint parameter);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(WindowCallback callback, nint parameter);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint window);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(nint window);

    [DllImport("user32.dll")]
    private static extern nint GetWindow(nint window, uint relation);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(nint window, StringBuilder text, int capacity);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(nint window, uint attribute, out int value, int size);

    [DllImport("kernel32.dll")]
    private static extern SafeProcessHandle OpenProcess(uint access, bool inherit, int processId);

    [DllImport("advapi32.dll")]
    private static extern bool OpenProcessToken(SafeProcessHandle process, uint access, out SafeAccessTokenHandle token);

    [DllImport("advapi32.dll")]
    private static extern bool GetTokenInformation(SafeAccessTokenHandle token, int informationClass,
        out int value, int size, out int returnedSize);
}
