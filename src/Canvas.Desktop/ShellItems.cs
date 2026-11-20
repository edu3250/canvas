using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Canvas.Desktop;

internal sealed class ShellItems : IDisposable
{
    public const int LargeIconPixels = 96;
    private readonly Dispatcher dispatcher;

    public ShellItems()
    {
        var ready = new TaskCompletionSource<Dispatcher>();
        var thread = new Thread(() =>
        {
            ready.SetResult(Dispatcher.CurrentDispatcher);
            Dispatcher.Run();
        }) { IsBackground = true, Name = "Shell item images" };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        dispatcher = ready.Task.GetAwaiter().GetResult();
    }

    public Task<string> ReadNameAsync(string path) =>
        dispatcher.InvokeAsync(() =>
        {
            if (SHParseDisplayName(path, IntPtr.Zero, out var item, 0, out _) >= 0)
            {
                try
                {
                    const int NormalDisplay = 0;
                    if (SHGetNameFromIDList(item, NormalDisplay, out var name) >= 0)
                    {
                        try { return Marshal.PtrToStringUni(name) ?? Path.GetFileName(path); }
                        finally { Marshal.FreeCoTaskMem(name); }
                    }
                }
                finally { Marshal.FreeCoTaskMem(item); }
            }

            return Path.GetFileName(path);
        }, DispatcherPriority.Normal).Task;

    public Task<(BitmapSource? Image, BitmapSource? Overlay)> ReadImageAsync(
        string path, int pixels, CancellationToken cancellation) =>
        dispatcher.InvokeAsync(() => ReadImage(path, pixels), DispatcherPriority.Background, cancellation).Task;

    private static (BitmapSource?, BitmapSource?) ReadImage(string path, int pixels)
    {
        var iid = typeof(IShellItemImageFactory).GUID;
        if (SHCreateItemFromParsingName(path, IntPtr.Zero, in iid, out var factory) < 0)
        {
            return default;
        }

        try
        {
            var extension = Path.GetExtension(path);
            var isShortcut = extension.Equals(".lnk", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".url", StringComparison.OrdinalIgnoreCase);
            const int BiggerSizeOk = 1;
            const int IconOnly = 4;
            var flags = BiggerSizeOk | (isShortcut ? IconOnly : 0);
            if (factory.GetImage(new NativeSize(pixels, pixels), flags, out var bitmap) < 0)
            {
                return default;
            }

            try
            {
                var image = Imaging.CreateBitmapSourceFromHBitmap(bitmap, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                image.Freeze();
                return (image, isShortcut ? ReadShortcutOverlay() : null);
            }
            finally { DeleteObject(bitmap); }
        }
        finally { Marshal.ReleaseComObject(factory); }
    }

    private static BitmapSource? ReadShortcutOverlay()
    {
        const int LinkOverlay = 29;
        const uint GetIcon = 0x100;
        var info = new StockIconInfo { Size = (uint)Marshal.SizeOf<StockIconInfo>() };
        if (SHGetStockIconInfo(LinkOverlay, GetIcon, ref info) < 0 || info.Icon == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            var overlay = Imaging.CreateBitmapSourceFromHIcon(info.Icon, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            overlay.Freeze();
            return overlay;
        }
        finally { DestroyIcon(info.Icon); }
    }

    public static void Open(string path)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true,
                ErrorDialog = true
            });
        }
        catch (Win32Exception)
        {
            // ShellExecute displays its own errors, including broken shortcuts.
        }
    }

    public void Dispose() => dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);

    [ComImport, Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItemImageFactory
    {
        [PreserveSig]
        int GetImage(NativeSize size, int flags, out IntPtr bitmap);
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct NativeSize(int width, int height)
    {
        public readonly int Width = width;
        public readonly int Height = height;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StockIconInfo
    {
        public uint Size;
        public IntPtr Icon;
        public int ImageIndex;
        public int IconIndex;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string Path;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int SHCreateItemFromParsingName(string path, IntPtr bindContext,
        in Guid iid, [MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory factory);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int SHParseDisplayName(string path, IntPtr bindContext, out IntPtr item,
        uint requestedAttributes, out uint attributes);

    [DllImport("shell32.dll", ExactSpelling = true)]
    private static extern int SHGetNameFromIDList(IntPtr item, int displayNameType, out IntPtr name);

    [DllImport("shell32.dll", ExactSpelling = true)]
    private static extern int SHGetStockIconInfo(int id, uint flags, ref StockIconInfo info);

    [DllImport("gdi32.dll", ExactSpelling = true)]
    private static extern bool DeleteObject(IntPtr handle);

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern bool DestroyIcon(IntPtr icon);
}
