using System.ComponentModel;
using System.Runtime.InteropServices;

internal sealed class EmbeddedPanel
{
    private const int Style = -16, Child = 0x40000000, Popup = unchecked((int)0x80000000);
    private const uint FrameChanged = 0x20, NoActivate = 0x10, NoZOrder = 4;
    private readonly string marker = "Canvas.EmbeddingTrial." + Guid.NewGuid().ToString("N");
    private readonly nint window;
    private readonly int pid;
    private readonly int originalStyle;
    private readonly WindowPlacementApi.Placement placement;
    private readonly bool wasVisible;
    private readonly bool overlay;

    internal EmbeddedPanel(nint window, bool overlay = false)
    {
        this.window = window;
        this.overlay = overlay;
        pid = Native.Pid(window);
        originalStyle = GetWindowLongW(window, Style);
        placement = WindowPlacementApi.Capture(window);
        wasVisible = IsWindowVisible(window);
        if (GetParent(window) != 0 || !SetPropW(window, marker, 1))
            throw new InvalidOperationException("Se esperaba una ventana independiente y propia.");
    }

    internal bool Current => Native.Pid(window) == pid && GetPropW(window, marker) == 1;
    internal void Attach(Control slot)
    {
        if (!Current) throw new InvalidOperationException("La ventana de prueba cambió.");
        if (overlay) { Resize(slot); return; }
        SetStyle((originalStyle & ~Popup) | Child);
        SetParentChecked(slot.Handle);
        Resize(slot);
        if (GetParent(window) != slot.Handle)
            throw new InvalidOperationException("Windows no conservó el alojamiento solicitado.");
    }

    internal void Resize(Control slot)
    {
        var origin = overlay ? slot.PointToScreen(Point.Empty) : Point.Empty;
        if (Current && !Native.SetWindowPos(window, 0, origin.X, origin.Y, slot.ClientSize.Width, slot.ClientSize.Height,
                overlay ? NoActivate : NoZOrder | NoActivate | FrameChanged))
            throw new Win32Exception();
    }

    internal void Detach()
    {
        if (!Current) return;
        if (!overlay)
        {
            if (GetParent(window) != 0) SetParentChecked(0);
            SetStyle(originalStyle);
        }
        if (wasVisible) WindowPlacementApi.Restore(window, placement);
        RemovePropW(window, marker);
    }

    private void SetParentChecked(nint parent)
    {
        if (SetParent(window, parent) == 0 && Marshal.GetLastPInvokeError() != 0)
            throw new Win32Exception(Marshal.GetLastPInvokeError());
    }

    private void SetStyle(int value)
    {
        if (SetWindowLongW(window, Style, value) == 0 && Marshal.GetLastPInvokeError() != 0)
            throw new Win32Exception(Marshal.GetLastPInvokeError());
    }

    internal static int Check()
    {
        using var host = new Form();
        using var slot = new Panel { Size = new Size(480, 320) };
        host.Controls.Add(slot);
        using var candidate = new Form();
        var handle = candidate.Handle;
        var before = GetWindowLongW(handle, Style);
        var embedded = new EmbeddedPanel(handle);
        try
        {
            embedded.Attach(slot);
            if (GetParent(handle) != slot.Handle || (GetWindowLongW(handle, Style) & Child) == 0 ||
                Native.Bounds(handle).Size != slot.ClientSize || IsWindowVisible(handle)) return 1;
        }
        finally { embedded.Detach(); }
        return GetParent(handle) == 0 && GetWindowLongW(handle, Style) == before &&
            !embedded.Current && !IsWindowVisible(handle) ? 0 : 1;
    }

    [DllImport("user32", SetLastError = true)] static extern nint SetParent(nint child, nint parent);
    [DllImport("user32")] static extern nint GetParent(nint window);
    [DllImport("user32")] static extern int GetWindowLongW(nint window, int index);
    [DllImport("user32", SetLastError = true)] static extern int SetWindowLongW(nint window, int index, int value);
    [DllImport("user32", CharSet = CharSet.Unicode)] static extern bool SetPropW(nint window, string name, nint value);
    [DllImport("user32", CharSet = CharSet.Unicode)] static extern nint GetPropW(nint window, string name);
    [DllImport("user32", CharSet = CharSet.Unicode)] static extern nint RemovePropW(nint window, string name);
    [DllImport("user32")] internal static extern bool IsWindowVisible(nint window);
}
