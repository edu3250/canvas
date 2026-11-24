using System.Runtime.InteropServices;
using System.Windows;

namespace Canvas.Desktop;

// Temporarily open a small hole in Canvas for a click on the verified source window.
internal static class ProjectedPointer
{
    public static async Task DispatchAsync(nint canvas, nint source, Point target, nint? wheel, Func<bool> current)
    {
        if (!current() || !GetWindowRect(canvas, out var bounds) || !GetCursorPos(out var before)) return;
        var x = (int)Math.Round(target.X);
        var y = (int)Math.Round(target.Y);
        var saved = CreateRectRgn(0, 0, 0, 0);
        var clipped = CreateRectRgn(0, 0, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top);
        var hole = CreateRectRgn(x - bounds.Left - 2, y - bounds.Top - 2, x - bounds.Left + 3, y - bounds.Top + 3);
        bool installed = false, moved = false;
        try
        {
            if (saved == 0 || clipped == 0 || hole == 0) return;
            if (GetWindowRgn(canvas, saved) == 0)
            {
                DeleteObject(saved);
                saved = 0;
            }
            if (CombineRgn(clipped, saved == 0 ? clipped : saved, hole, 4) == 0 ||
                SetWindowRgn(canvas, clipped, true) == 0) return;
            clipped = 0; // Windows owns an installed region.
            installed = true;
            var point = new Pixel { X = x, Y = y };
            bool Eligible() => current() && GetAncestor(WindowFromPoint(point), 2) == source;
            if (!Eligible())
            {
                // The foreground source can still be covered by another window behind Canvas.
                // Resolve the recipient within that source, independently of desktop z-order.
                DispatchToChild(source, point, wheel, current);
                return;
            }
            if (wheel is { } keysAndDelta)
            {
                var recipient = WindowFromPoint(point);
                if (GetAncestor(recipient, 2) == source && current())
                    SendMessageTimeout(recipient, 0x20A, keysAndDelta, (nint)((y << 16) | (x & 0xffff)), 2, 50, out _);
                return;
            }
            if (!SetCursorPos(x, y)) return;
            moved = true;
            if (!GetCursorPos(out var at) || at.X != x || at.Y != y || !Eligible()) return;
            mouse_event(2, 0, 0, 0, 0);
            mouse_event(4, 0, 0, 0, 0);
            await Task.Delay(100);
        }
        finally
        {
            if (moved && GetCursorPos(out var at) && at.X == x && at.Y == y) SetCursorPos(before.X, before.Y);
            if (installed && IsWindow(canvas) && SetWindowRgn(canvas, saved, true) != 0) saved = 0;
            if (saved != 0) DeleteObject(saved);
            if (clipped != 0) DeleteObject(clipped);
            if (hole != 0) DeleteObject(hole);
        }
    }

    private static void DispatchToChild(nint source, Pixel screen, nint? wheel, Func<bool> current)
    {
        var child = source;
        Pixel local;
        while (true)
        {
            local = screen;
            if (!ScreenToClient(child, ref local)) return;
            var next = ChildWindowFromPointEx(child, local, 7); // Skip invisible, disabled and transparent children.
            if (next == 0) return;
            if (next == child) break;
            if (GetAncestor(next, 2) != source) return;
            child = next;
        }

        if (!current() || GetAncestor(child, 2) != source) return;
        if (wheel is { } delta)
        {
            SendMessageTimeout(child, 0x20A, delta, Pack(screen), 2, 50, out _);
        }
        else
        {
            var position = Pack(local);
            if (PostMessage(child, 0x0201, 1, position))
                PostMessage(child, 0x0202, 0, position);
        }
    }

    private static nint Pack(Pixel point) => (nint)((point.Y << 16) | (point.X & 0xffff));

    [DllImport("user32")] private static extern bool ScreenToClient(nint window, ref Pixel point);
    [DllImport("user32")] private static extern nint ChildWindowFromPointEx(nint parent, Pixel point, uint flags);
    [DllImport("user32", EntryPoint = "PostMessageW")] private static extern bool PostMessage(nint window, uint message, nint wp, nint lp);
    [StructLayout(LayoutKind.Sequential)] private struct Pixel { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)] private struct Bounds { public int Left, Top, Right, Bottom; }
    [DllImport("user32", EntryPoint = "SendMessageTimeoutW")] private static extern nint SendMessageTimeout(nint hwnd, uint msg, nint wp, nint lp, uint flags, uint timeout, out nuint result);
    [DllImport("user32")] private static extern bool GetWindowRect(nint window, out Bounds bounds);
    [DllImport("user32")] private static extern bool IsWindow(nint window);
    [DllImport("user32")] private static extern int GetWindowRgn(nint window, nint region);
    [DllImport("user32")] private static extern int SetWindowRgn(nint window, nint region, bool redraw);
    [DllImport("gdi32")] private static extern nint CreateRectRgn(int l, int t, int r, int b);
    [DllImport("gdi32")] private static extern int CombineRgn(nint output, nint first, nint second, int mode);
    [DllImport("gdi32")] private static extern bool DeleteObject(nint value);
    [DllImport("user32")] private static extern nint WindowFromPoint(Pixel point);
    [DllImport("user32")] private static extern nint GetAncestor(nint window, uint flags);
    [DllImport("user32")] private static extern bool GetCursorPos(out Pixel point);
    [DllImport("user32")] private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32")] private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, nuint extra);
}
