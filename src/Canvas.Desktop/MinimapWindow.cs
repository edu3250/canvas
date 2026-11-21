using Canvas.Domain;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using DomainPoint = Canvas.Domain.Point;

namespace Canvas.Desktop;

internal sealed class MinimapWindow : Window
{
    private const double MapWidth = 192;
    private const double MapHeight = 114;
    private const double IslandPadding = 8;
    private const double Gap = 8;
    public const double IslandWidth = MapWidth + 2 * IslandPadding;
    public const double IslandHeight = MapHeight + Gap + VisualTokens.Size.Control + 2 * IslandPadding;
    public const double Inset = 16;

    private readonly MapSurface surface = new();
    private readonly Border island;
    private readonly TextBlock zoomValue = new()
    {
        Text = "100 %", TextAlignment = TextAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
    };
    private readonly Button zoomOut = CreateZoomButton("−", "Alejar", "Ctrl+-");
    private readonly Button zoomIn = CreateZoomButton("+", "Acercar", "Ctrl++");
    private readonly Button fitContent = CreateZoomButton("⛶", "Encuadrar el contenido", "Ctrl+0");

    public MinimapWindow()
    {
        Title = "Isla del mapa";
        Width = IslandWidth;
        Height = IslandHeight;
        FontFamily = Theme.Font;
        FontSize = VisualTokens.Type.Interface;
        Foreground = DeskControls.Brush(VisualTokens.Colors.Bone);
        Background = DeskControls.Brush(VisualTokens.Colors.Slate);
        UseLayoutRounding = true;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.Manual;
        ShowActivated = false;
        ShowInTaskbar = false;
        Topmost = true;
        surface.Height = MapHeight;
        var zoom = new Grid { Margin = new Thickness(0, Gap, 0, 0) };
        AddZoomControl(zoomOut, new GridLength(36));
        AddZoomControl(zoomValue, new GridLength(1, GridUnitType.Star));
        AddZoomControl(zoomIn, new GridLength(36));
        AddZoomControl(fitContent, new GridLength(36));
        zoomOut.Click += (_, _) => ZoomRequested?.Invoke(-1);
        zoomIn.Click += (_, _) => ZoomRequested?.Invoke(1);
        fitContent.Click += (_, _) => FitRequested?.Invoke();
        var contents = new StackPanel();
        contents.Children.Add(surface);
        contents.Children.Add(zoom);
        island = new Border
        {
            CornerRadius = new CornerRadius(VisualTokens.Radius.Island),
            Background = Background,
            Padding = new Thickness(IslandPadding),
            Child = contents
        };
        Content = island;

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

        void AddZoomControl(UIElement control, GridLength width)
        {
            Grid.SetColumn(control, zoom.ColumnDefinitions.Count);
            zoom.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
            zoom.Children.Add(control);
        }
    }

    public event Action<DomainPoint>? CenterRequested;
    public event Action<double>? ZoomRequested;
    public event Action? FitRequested;

    public void UpdateMap(CanvasBounds viewport, double zoom, IReadOnlyList<MinimapItem> items)
    {
        zoomValue.Text = zoom < 0.01 ? "<1 %" : $"{zoom * 100:0} %";
        zoomOut.IsEnabled = zoom > CameraGestures.MinZoom;
        zoomIn.IsEnabled = zoom < CameraGestures.MaxZoom;
        fitContent.IsEnabled = items.Count != 0;
        surface.UpdateMap(viewport, items);
    }

    private static Button CreateZoomButton(string content, string name, string shortcut)
    {
        var button = new Button
        {
            Style = DeskControls.ButtonStyle, Content = content, Height = VisualTokens.Size.Control,
            FontSize = 16, ToolTip = $"{name} ({shortcut})", Focusable = false
        };
        AutomationProperties.SetName(button, name);
        AutomationProperties.SetAcceleratorKey(button, shortcut);
        return button;
    }

    public void EnsureTopmost()
    {
        if (IsVisible && PresentationSource.FromVisual(this) is HwndSource source &&
            !NativePanelInput.IsTopmost(source.Handle))
        {
            NativePanelInput.RaiseWindow(source.Handle, topmost: true);
        }
    }

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
        var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        NativePanelInput.PreventActivation(source.Handle);
        source.AddHook(PreventMouseActivation);
        if (!SystemParameters.HighContrast && MapBackdrop.TryEnable(source.Handle))
        {
            source.CompositionTarget.BackgroundColor = Colors.Transparent;
            Background = Brushes.Transparent;
            island.Background = DeskControls.Brush(VisualTokens.Colors.Slate.WithAlpha(0.88));
        }
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
        private static readonly Brush Background = DeskControls.Brush(VisualTokens.Colors.Night);
        private static readonly Brush Marker = DeskControls.Brush(VisualTokens.Colors.Bone);
        private static readonly Brush ViewFill = DeskControls.Brush(VisualTokens.DefaultLight.Color.WithAlpha(VisualTokens.LightWashOpacity));
        private static readonly Pen ViewOutline = new(DeskControls.Brush(VisualTokens.DefaultLight.Color), 1);
        private static readonly Pen Frame = new(DeskControls.Brush(VisualTokens.Colors.Border), 1);
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
            drawing.DrawRoundedRectangle(Background, Frame,
                new Rect(0.5, 0.5, RenderSize.Width - 1, RenderSize.Height - 1),
                VisualTokens.Radius.Control, VisualTokens.Radius.Control);
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
