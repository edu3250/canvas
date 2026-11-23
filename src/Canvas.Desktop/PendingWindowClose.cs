using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Canvas.Desktop;

internal sealed class PendingWindowClose : IDisposable
{
    private static readonly ConcurrentDictionary<nuint, PendingWindowClose> Requests = new();
    private static readonly CloseCallback Callback = Complete;
    private static int nextId;
    private readonly nuint id = unchecked((uint)Interlocked.Increment(ref nextId));

    public bool Completed { get; private set; }
    public bool Accepted { get; private set; }

    public static PendingWindowClose Send(long handle)
    {
        const uint CloseMessage = 0x0010;
        var request = new PendingWindowClose();
        if (!Requests.TryAdd(request.id, request))
        {
            throw new InvalidOperationException("The close request ID is already in use.");
        }

        if (!SendMessageCallbackW((nint)handle, CloseMessage, 0, 0, Callback, request.id))
        {
            var error = new Win32Exception();
            request.Dispose();
            throw error;
        }

        return request;
    }

    private static void Complete(nint window, uint message, nuint id, nint result)
    {
        if (Requests.TryRemove(id, out var request))
        {
            // WM_CLOSE has no acceptance result. Snapshot visibility when its handler returns.
            request.Accepted = !WindowCatalog.IsVisible((long)window);
            request.Completed = true;
        }
    }

    public void Dispose() => Requests.TryRemove(id, out _);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void CloseCallback(nint window, uint message, nuint data, nint result);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SendMessageCallbackW(
        nint window, uint message, nint wParam, nint lParam, CloseCallback callback, nuint data);
}
