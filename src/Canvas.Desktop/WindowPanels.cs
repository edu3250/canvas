using Canvas.Domain;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using DomainPoint = Canvas.Domain.Point;
using WpfPoint = System.Windows.Point;

namespace Canvas.Desktop;

internal sealed class WindowPanels(Window owner, Func<CanvasBounds[]> iconBounds) : System.Windows.Controls.Canvas, IDisposable
{
    private const double TitleHeight = 28;
    private static readonly Brush BackgroundFill = new SolidColorBrush(Color.FromRgb(24, 51, 65));
    private readonly WindowCatalog catalog = new();
    private readonly List<Panel> panels = [];
    private SpaceId space;
    private Camera camera;
    private Drag? drag;

    public event Action? MapChanged;
    public event Action<Panel>? ActivationRequested;

    public Panel? ActivePanel { get; set; }

    public Panel? FindPanel(DomainPoint point) => panels.LastOrDefault(panel =>
        panel.Space == space && point.X >= panel.Bounds.X && point.X <= panel.Bounds.X + panel.Bounds.Width &&
        point.Y >= panel.Bounds.Y && point.Y <= panel.Bounds.Y + panel.Bounds.Height);

    public WpfPoint GetWindowOrigin(Panel panel, Camera targetCamera)
    {
        var point = Canvas.Domain.Geometry.WorldToView(new DomainPoint(panel.Bounds.X, panel.Bounds.Y + TitleHeight), targetCamera);
        return PointToScreen(new WpfPoint(point.X, point.Y));
    }

    public IReadOnlyList<FrameworkElement> OccludingControls { get; set; } = [];

    public MinimapItem[] GetMapItems() => panels.Where(panel => panel.Space == space)
        .Select(panel => new MinimapItem(panel.Bounds, null, IsPanel: true)).ToArray();

    public void ShowPicker(Button button)
    {
        RefreshWindows();
        var menu = new ContextMenu { PlacementTarget = button, Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom };
        foreach (var window in catalog.List().Where(window => panels.All(panel => panel.Window.Identity != window.Identity)))
        {
            var item = new MenuItem { Header = window.Title, ToolTip = window.Title, MaxWidth = 420 };
            item.Click += (_, _) => Add(window);
            menu.Items.Add(item);
        }

        if (menu.Items.Count == 0)
        {
            menu.Items.Add(new MenuItem { Header = "No hay ventanas disponibles", IsEnabled = false });
        }

        button.ContextMenu = menu;
        menu.IsOpen = true;
    }

    private void Add(WindowCandidate selected)
    {
        RefreshWindows();
        if (catalog.Find(selected.Handle) is not { } window || window.Identity != selected.Identity ||
            panels.Any(panel => panel.Window.Identity == window.Identity))
        {
            return;
        }

        if (catalog.Track(window.Identity) is not { } instance)
        {
            MessageBox.Show(owner, "La ventana ya no está disponible. Vuelve a abrir el selector.", "Añadir ventana",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dpi = VisualTreeHelper.GetDpi(this);
        var width = (window.NormalBounds.Right - (double)window.NormalBounds.Left) / dpi.DpiScaleX;
        var height = (window.NormalBounds.Bottom - (double)window.NormalBounds.Top) / dpi.DpiScaleY + TitleHeight;
        var center = CanvasBounds.Viewport(camera, new DomainPoint(ActualWidth, ActualHeight)).Center;
        var desired = new CanvasBounds(center.X - width / 2, center.Y - height / 2, width, height);
        var panel = new Panel(window, space, PanelPlacement.FindFree(desired, Occupied()), CreateCard(window.Title), instance);
        panels.Add(panel);
        Children.Add(panel.Card);
        panel.Card.PreviewMouseLeftButtonDown += (_, e) => StartDrag(panel, e);
        panel.Card.MouseMove += (_, e) => MoveDrag(e);
        panel.Card.PreviewMouseLeftButtonUp += (_, e) => FinishDrag(e);
        panel.Card.LostMouseCapture += (_, _) => CancelDrag();
        Render();
        MapChanged?.Invoke();
    }

    public void ShowSpace(SpaceId activeSpace, Camera activeCamera)
    {
        if (space != activeSpace)
        {
            CancelDrag();
        }

        space = activeSpace;
        camera = activeCamera;
        Render();
    }

    public void RefreshWindows()
    {
        var removed = panels.Where(panel => !WindowCatalog.IsLive(panel.Window.Identity, panel.Instance)).ToArray();
        if (removed.Length != 0)
        {
            foreach (var panel in removed)
            {
                if (drag?.Panel == panel)
                {
                    CancelDrag();
                }

                panel.Dispose();
                Children.Remove(panel.Card);
                panels.Remove(panel);
            }

            MapChanged?.Invoke();
        }

        Render();
    }

    public void SuspendPreview(Panel panel)
    {
        panel.Thumbnail?.Dispose();
        panel.Thumbnail = null;
    }

    public void Render()
    {
        if (PresentationSource.FromVisual(this) is not HwndSource source || source.RootVisual is not Visual root)
        {
            return;
        }

        var matrix = source.CompositionTarget.TransformToDevice;
        var transform = TransformToAncestor(root);
        WpfPoint ToDevice(double x, double y) => matrix.Transform(transform.Transform(new WpfPoint(x, y)));
        var clip = new Rect(ToDevice(0, 0), ToDevice(ActualWidth, ActualHeight));
        var controls = OccludingControls.Where(control => control.IsVisible).Select(control =>
        {
            var bounds = control.TransformToAncestor(root).TransformBounds(new Rect(control.RenderSize));
            var pixels = new Rect(matrix.Transform(bounds.TopLeft), matrix.Transform(bounds.BottomRight));
            return new CanvasBounds(pixels.X, pixels.Y, pixels.Width, pixels.Height);
        }).ToArray();

        foreach (var panel in panels)
        {
            var shown = panel.Space == space && owner.WindowState != WindowState.Minimized;
            panel.Card.Visibility = shown ? Visibility.Visible : Visibility.Collapsed;
            if (!shown)
            {
                panel.Thumbnail?.Hide();
                continue;
            }

            var bounds = panel.Bounds;
            var point = Canvas.Domain.Geometry.WorldToView(new DomainPoint(bounds.X, bounds.Y), camera);
            SetLeft(panel.Card, point.X);
            SetTop(panel.Card, point.Y);
            panel.Card.Width = bounds.Width;
            panel.Card.Height = bounds.Height;
            panel.Card.RenderTransform = new ScaleTransform(camera.Zoom, camera.Zoom);

            if (panel == ActivePanel)
            {
                panel.Thumbnail?.Hide();
                panel.Card.Text.Text = "";
                continue;
            }

            if (WindowCatalog.IsMinimized(panel.Window.Handle))
            {
                panel.Thumbnail?.Hide();
                panel.Card.Text.Text = "Ventana minimizada";
                continue;
            }

            panel.Thumbnail ??= WindowThumbnail.Register(source.Handle, panel.Window.Handle);
            var area = new Rect(ToDevice(point.X + camera.Zoom, point.Y + (TitleHeight + 1) * camera.Zoom),
                ToDevice(point.X + (bounds.Width - 1) * camera.Zoom, point.Y + (bounds.Height - 1) * camera.Zoom));
            panel.Card.Text.Text = panel.Thumbnail?.Display(area, clip, controls) == true ? "" : "Vista previa no disponible";
        }
    }

    private CanvasBounds[] Occupied(Panel? except = null) =>
        [.. iconBounds(), .. panels.Where(panel => panel.Space == space && panel != except).Select(panel => panel.Bounds)];

    private void StartDrag(Panel panel, MouseButtonEventArgs e)
    {
        if (drag is null && panel.Card.CaptureMouse())
        {
            drag = new Drag(panel, e.GetPosition(this), panel.Bounds);
            e.Handled = true;
        }
    }

    private void MoveDrag(MouseEventArgs e)
    {
        if (drag is not { } active || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        UpdateDrag(active, e.GetPosition(this));
        e.Handled = true;
    }

    private void UpdateDrag(Drag active, WpfPoint pointer)
    {
        var delta = pointer - active.Pointer;
        if (!active.HasMoved && Math.Abs(delta.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(delta.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        active.HasMoved = true;
        var desired = active.Original with
        {
            X = active.Original.X + delta.X / camera.Zoom,
            Y = active.Original.Y + delta.Y / camera.Zoom
        };
        active.Panel.Bounds = PanelPlacement.FindFree(desired, Occupied(active.Panel));
        Render();
        MapChanged?.Invoke();
    }

    private void FinishDrag(MouseButtonEventArgs e)
    {
        if (drag is { } active)
        {
            UpdateDrag(active, e.GetPosition(this));
            drag = null;
            active.Panel.Card.ReleaseMouseCapture();
            e.Handled = true;
            if (!active.HasMoved)
            {
                ActivationRequested?.Invoke(active.Panel);
            }
        }
    }

    public void CancelDrag()
    {
        if (drag is { } active)
        {
            drag = null;
            active.Panel.Bounds = active.Original;
            active.Panel.Card.ReleaseMouseCapture();
            Render();
            MapChanged?.Invoke();
        }
    }

    private static PanelCard CreateCard(string title)
    {
        var text = new TextBlock
        {
            Foreground = Brushes.White,
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 10, 0),
            Text = title,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        var head = new Border { Background = BackgroundFill, Child = text, Cursor = Cursors.SizeAll };
        var card = new PanelCard();
        var content = new Grid();
        content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(TitleHeight) });
        content.RowDefinitions.Add(new RowDefinition());
        content.Children.Add(head);
        Grid.SetRow(card.Text, 1);
        content.Children.Add(card.Text);
        card.Child = content;
        AutomationProperties.SetName(card, title);
        return card;
    }

    public void Dispose()
    {
        foreach (var panel in panels)
        {
            panel.Dispose();
        }

        panels.Clear();
    }

    internal sealed class PanelCard : Border
    {
        public PanelCard()
        {
            BorderBrush = BackgroundFill;
            BorderThickness = new Thickness(1);
            Background = new SolidColorBrush(Color.FromRgb(35, 67, 84));
        }

        public TextBlock Text { get; } = new()
        {
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center
        };
    }

    private sealed record Drag(Panel Panel, WpfPoint Pointer, CanvasBounds Original)
    {
        public bool HasMoved { get; set; }
    }

    internal sealed class Panel(WindowCandidate window, SpaceId space, CanvasBounds bounds,
        PanelCard card, WindowInstance instance) : IDisposable
    {
        public WindowCandidate Window { get; } = window;
        public SpaceId Space { get; } = space;
        public CanvasBounds Bounds { get; set; } = bounds;
        public PanelCard Card { get; } = card;
        public WindowThumbnail? Thumbnail { get; set; }
        public WindowInstance Instance { get; } = instance;

        public void Dispose()
        {
            Thumbnail?.Dispose();
            Instance.Dispose();
        }
    }
}
