using Canvas.Domain;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using DomainPoint = Canvas.Domain.Point;

namespace Canvas.Desktop;

internal sealed class MinimapWindow : Window
{
    public const double MapWidth = 184;
    public const double MapHeight = 112;
    public const double Inset = 12;

    private readonly MapSurface surface = new();

    public MinimapWindow()
    {
        Title = "Minimapa";
        Width = MapWidth;
        Height = MapHeight;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.Manual;
        ShowActivated = false;
        ShowInTaskbar = false;
        Topmost = true;
        Content = surface;
        AutomationProperties.SetName(surface, "Minimapa del espacio");
        surface.ToolTip = "Centrar el lienzo en este punto";
        surface.Cursor = Cursors.Hand;
        surface.MouseLeftButtonDown += (_, e) =>
        {
            var point = e.GetPosition(surface);
            if (surface.ToWorld(new DomainPoint(point.X, point.Y)) is { } world)
            {
                CenterRequested?.Invoke(world);
                e.Handled = true;
            }
        };
    }

    public event Action<DomainPoint>? CenterRequested;

    public void UpdateMap(CanvasBounds viewport, IReadOnlyList<MinimapItem> items) =>
        surface.UpdateMap(viewport, items);

    public void AnchorTo(FrameworkElement viewport)
    {
        if (PresentationSource.FromVisual(viewport)?.CompositionTarget is not { } target)
        {
            return;
        }

        var bottomRight = viewport.PointToScreen(new System.Windows.Point(viewport.ActualWidth, viewport.ActualHeight));
        var position = target.TransformFromDevice.Transform(bottomRight);
        Left = position.X - Width - Inset;
        Top = position.Y - Height - Inset;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ((HwndSource)PresentationSource.FromVisual(this)).AddHook(PreventMouseActivation);
    }

    private static IntPtr PreventMouseActivation(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int MouseActivate = 0x0021;
        const int NoActivate = 3;
        if (message == MouseActivate)
        {
            handled = true;
            return new IntPtr(NoActivate);
        }

        return IntPtr.Zero;
    }

    private sealed class MapSurface : FrameworkElement
    {
        private static readonly Brush Background = new SolidColorBrush(Color.FromRgb(24, 51, 65));
        private static readonly Brush Marker = new SolidColorBrush(Color.FromRgb(225, 239, 248));
        private static readonly Brush ViewFill = new SolidColorBrush(Color.FromArgb(45, 125, 202, 250));
        private static readonly Pen ViewOutline = new(new SolidColorBrush(Color.FromRgb(147, 210, 250)), 1);
        private static readonly Pen Frame = new(new SolidColorBrush(Color.FromRgb(94, 131, 152)), 1);
        private Camera? projection;
        private CanvasBounds viewport;
        private IReadOnlyList<MinimapItem> items = [];

        public DomainPoint? ToWorld(DomainPoint position) =>
            projection is { } map ? Canvas.Domain.Geometry.ViewToWorld(position, map) : null;

        public void UpdateMap(CanvasBounds visible, IReadOnlyList<MinimapItem> icons)
        {
            viewport = visible;
            items = icons;
            var extent = icons.Aggregate(visible, (bounds, icon) => bounds.Union(icon.Bounds));
            projection = extent.Fit(new DomainPoint(MapWidth, MapHeight), 8, double.PositiveInfinity);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawing)
        {
            drawing.DrawRectangle(Background, Frame, new Rect(0.5, 0.5, RenderSize.Width - 1, RenderSize.Height - 1));
            if (projection is not { } map)
            {
                return;
            }

            drawing.DrawRectangle(ViewFill, ViewOutline, Project(viewport, map));
            foreach (var item in items)
            {
                var bounds = Project(item.Bounds, map);
                if (item.IsPanel)
                {
                    drawing.DrawRectangle(ViewFill, ViewOutline, bounds);
                }
                else if (item.Image is { } image && bounds.Width >= 8)
                {
                    var scale = Math.Min(bounds.Width / image.Width, bounds.Height / image.Height);
                    var width = image.Width * scale;
                    var height = image.Height * scale;
                    drawing.DrawImage(image, new Rect(bounds.X + (bounds.Width - width) / 2,
                        bounds.Y + (bounds.Height - height) / 2, width, height));
                }
                else
                {
                    var size = Math.Clamp(bounds.Width, 2, 8);
                    drawing.DrawRoundedRectangle(Marker, null, new Rect(bounds.X + (bounds.Width - size) / 2,
                        bounds.Y + (bounds.Height - size) / 2, size, size), 1, 1);
                }
            }
        }

        private static Rect Project(CanvasBounds bounds, Camera map)
        {
            var origin = Canvas.Domain.Geometry.WorldToView(new DomainPoint(bounds.X, bounds.Y), map);
            return new Rect(origin.X, origin.Y, bounds.Width * map.Zoom, bounds.Height * map.Zoom);
        }
    }
}

internal readonly record struct MinimapItem(CanvasBounds Bounds, ImageSource? Image, bool IsPanel = false);
