using Canvas.Domain;
using System.Runtime.InteropServices;
using System.Windows;

namespace Canvas.Desktop;

internal sealed class WindowThumbnail : IDisposable
{
    private const uint OpacityFlag = 0x04;
    private readonly nint destination;
    private readonly nint source;
    private readonly List<nint> handles;
    private int visibleCount;
    private byte? opacity;

    private WindowThumbnail(nint destination, nint source, nint initial)
    {
        this.destination = destination;
        this.source = source;
        handles = [initial];
    }

    public static WindowThumbnail? Register(nint destination, long source) =>
        DwmRegisterThumbnail(destination, (nint)source, out var thumbnail) >= 0
            ? new WindowThumbnail(destination, (nint)source, thumbnail)
            : null;

    public Canvas.Domain.Point? ReadSourceSize() =>
        DwmQueryThumbnailSourceSize(handles[0], out var size) >= 0 && size.Width > 0 && size.Height > 0
            ? new Canvas.Domain.Point(size.Width, size.Height)
            : null;

    public bool Display(Rect area, Rect viewport, IReadOnlyList<CanvasBounds> controls, Canvas.Domain.Point size, byte opacity)
    {
        if (area.Width < 1 || area.Height < 1)
        {
            Hide();
            return false;
        }

        var content = PreviewClipping.Fit(new CanvasBounds(area.X, area.Y, area.Width, area.Height), size);
        var scale = content.Width / size.X;
        var visible = PreviewClipping.VisibleParts(content,
            new CanvasBounds(viewport.X, viewport.Y, viewport.Width, viewport.Height), controls);
        var used = 0;
        foreach (var part in visible)
        {
            var target = ToNative(new Rect(part.X, part.Y, part.Width, part.Height));
            var crop = ToNative(new Rect((part.X - content.X) / scale, (part.Y - content.Y) / scale,
                part.Width / scale, part.Height / scale));
            if (target.Left >= target.Right || target.Top >= target.Bottom ||
                crop.Left >= crop.Right || crop.Top >= crop.Bottom)
            {
                continue;
            }

            if (used == handles.Count)
            {
                if (DwmRegisterThumbnail(destination, source, out var thumbnail) < 0)
                {
                    Hide();
                    return false;
                }
                handles.Add(thumbnail);
            }

            var properties = new ThumbnailProperties
            {
                Flags = 0x01 | 0x02 | OpacityFlag | 0x08,
                Opacity = opacity,
                Visible = true,
                Destination = target,
                Source = crop
            };
            if (DwmUpdateThumbnailProperties(handles[used], in properties) < 0)
            {
                Hide();
                return false;
            }
            used++;
        }

        HideFrom(used);
        visibleCount = used;
        this.opacity = opacity;
        return true;
    }

    public void SetOpacity(byte value)
    {
        if (value == opacity)
        {
            return;
        }

        var properties = new ThumbnailProperties { Flags = OpacityFlag, Opacity = value };
        var updated = true;
        for (var index = 0; index < visibleCount; index++)
        {
            updated &= DwmUpdateThumbnailProperties(handles[index], in properties) >= 0;
        }

        opacity = updated ? value : null;
    }

    public void Hide()
    {
        HideFrom(0);
        visibleCount = 0;
    }

    private void HideFrom(int first)
    {
        var properties = new ThumbnailProperties { Flags = 0x08 };
        for (var index = first; index < handles.Count; index++)
        {
            DwmUpdateThumbnailProperties(handles[index], in properties);
        }
    }

    public void Dispose()
    {
        foreach (var handle in handles)
        {
            DwmUnregisterThumbnail(handle);
        }
        handles.Clear();
    }

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
