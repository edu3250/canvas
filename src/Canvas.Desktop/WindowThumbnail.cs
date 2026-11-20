using System.Runtime.InteropServices;
using System.Windows;

namespace Canvas.Desktop;

internal sealed class WindowThumbnail : IDisposable
{
    private readonly nint handle;
    private bool visible;

    private WindowThumbnail(nint handle) => this.handle = handle;

    public static WindowThumbnail? Register(nint destination, long source) =>
        DwmRegisterThumbnail(destination, (nint)source, out var thumbnail) >= 0 ? new WindowThumbnail(thumbnail) : null;

    public bool Display(Rect area, Rect viewport)
    {
        if (area.Width < 1 || area.Height < 1 || DwmQueryThumbnailSourceSize(handle, out var size) < 0 ||
            size.Width <= 0 || size.Height <= 0)
        {
            Hide();
            return false;
        }

        var scale = Math.Min(area.Width / size.Width, area.Height / size.Height);
        var width = size.Width * scale;
        var height = size.Height * scale;
        var content = new Rect(area.X + (area.Width - width) / 2, area.Y + (area.Height - height) / 2, width, height);
        var clipped = Rect.Intersect(content, viewport);
        if (clipped.IsEmpty || clipped.Width < 1 || clipped.Height < 1)
        {
            Hide();
            return true;
        }

        var properties = new ThumbnailProperties
        {
            Flags = 0x01 | 0x02 | 0x08,
            Visible = true,
            Destination = ToNative(clipped),
            Source = ToNative(new Rect((clipped.X - content.X) / scale, (clipped.Y - content.Y) / scale,
                clipped.Width / scale, clipped.Height / scale))
        };
        if (DwmUpdateThumbnailProperties(handle, in properties) < 0)
        {
            Hide();
            return false;
        }

        visible = true;
        return true;
    }

    public void Hide()
    {
        if (visible)
        {
            var properties = new ThumbnailProperties { Flags = 0x08 };
            DwmUpdateThumbnailProperties(handle, in properties);
            visible = false;
        }
    }

    public void Dispose() => DwmUnregisterThumbnail(handle);

    private static NativeRect ToNative(Rect rect) => new()
    {
        Left = (int)Math.Round(rect.Left),
        Top = (int)Math.Round(rect.Top),
        Right = (int)Math.Round(rect.Right),
        Bottom = (int)Math.Round(rect.Bottom)
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize
    {
        public int Width, Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ThumbnailProperties
    {
        public uint Flags;
        public NativeRect Destination;
        public NativeRect Source;
        public byte Opacity;
        [MarshalAs(UnmanagedType.Bool)] public bool Visible;
        [MarshalAs(UnmanagedType.Bool)] public bool ClientOnly;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmRegisterThumbnail(nint destination, nint source, out nint thumbnail);

    [DllImport("dwmapi.dll")]
    private static extern int DwmUnregisterThumbnail(nint thumbnail);

    [DllImport("dwmapi.dll")]
    private static extern int DwmQueryThumbnailSourceSize(nint thumbnail, out NativeSize size);

    [DllImport("dwmapi.dll")]
    private static extern int DwmUpdateThumbnailProperties(nint thumbnail, in ThumbnailProperties properties);
}
