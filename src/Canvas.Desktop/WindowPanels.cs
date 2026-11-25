using Canvas.Domain;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DomainPoint = Canvas.Domain.Point;
using WpfPoint = System.Windows.Point;

namespace Canvas.Desktop;

internal sealed class WindowPanels : System.Windows.Controls.Canvas, IDisposable
{
    private const double TitleHeight = VisualTokens.Size.PanelHandle;
    private static readonly DependencyProperty RevealProperty = DependencyProperty.Register(
        "Reveal", typeof(double), typeof(WindowPanels), new PropertyMetadata(1d, OnRevealChanged));
    private readonly Window owner;
    private readonly ShellItems shell;
    private readonly Func<CanvasBounds[]> iconBounds;
    private readonly Func<Panel, bool> releaseUnavailable;
    private readonly WindowCatalog catalog = new();
    private readonly List<Panel> panels = [];
    private SpaceId space;
    private Camera camera;
    private Drag? drag;
    private bool disposed;

    public WindowPanels(Window owner, ShellItems shell, Func<CanvasBounds[]> iconBounds, Func<Panel, bool> releaseUnavailable)
    {
        this.owner = owner;
        this.shell = shell;
        this.iconBounds = iconBounds;
        this.releaseUnavailable = releaseUnavailable;
    }

    public event Action? MapChanged;
    public event Action<Panel>? PanelMoved;
    public bool IsDragging => drag is not null;
    public event Action<Panel>? ActivationRequested;
    public event Action<Panel>? FoldRequested;
    public event Action<Panel>? MaximizeRequested;
    public event Action<Panel>? CloseRequested;
    public event Action<Panel>? RemoveRequested;

    public bool ProjectActive { get; set; }
    public Panel? ActivePanel { get; set; }
    public bool IsPointerOverPanel => panels.Any(panel => panel.Card.IsMouseOver);
    public bool Contains(Panel panel) => panels.Contains(panel);
    public Panel[] GetUnavailablePanels(bool includePendingClose = false) => panels
        .Where(panel => panel.CloseRequest?.Accepted == true ||
            includePendingClose && panel.CloseRequest is { Completed: false } ||
            !WindowCatalog.IsLive(panel.Window.Identity, panel.Instance)).ToArray();

    public bool HasTopmostSource() => panels.Any(panel =>
        NativePanelInput.IsTopmost((nint)panel.Window.Handle) &&
        WindowCatalog.IsLive(panel.Window.Identity, panel.Instance) &&
        !WindowCatalog.IsMinimized(panel.Window.Handle));

    public Panel? FindPanel(DomainPoint point) => panels.LastOrDefault(panel =>
        !panel.WaitingForRemoval && panel.Space == space &&
        point.X >= panel.DisplayBounds.X && point.X <= panel.DisplayBounds.X + panel.DisplayBounds.Width &&
        point.Y >= panel.DisplayBounds.Y && point.Y <= panel.DisplayBounds.Y + panel.DisplayBounds.Height);

    public WpfPoint GetWindowOrigin(Panel panel, Camera targetCamera)
    {
        var point = Canvas.Domain.Geometry.WorldToView(new DomainPoint(panel.Bounds.X + 1, panel.Bounds.Y + TitleHeight + 1), targetCamera);
        return PointToScreen(new WpfPoint(point.X, point.Y));
    }

    public (int Width, int Height) GetUnscaledPixelSize(Panel panel)
    {
        var dpi = VisualTreeHelper.GetDpi(this);
        return (Math.Max(1, (int)Math.Round((panel.Bounds.Width - 2) * dpi.DpiScaleX)),
            Math.Max(1, (int)Math.Round((panel.Bounds.Height - TitleHeight - 2) * dpi.DpiScaleY)));
    }

    public void SyncActiveBounds(Panel panel, WindowRectangle screenBounds, bool syncPosition)
    {
        var dpi = VisualTreeHelper.GetDpi(this);
        var width = screenBounds.Right - screenBounds.Left;
        var height = screenBounds.Bottom - screenBounds.Top;
        if (width <= 0 || height <= 0) return;

        var bounds = panel.Bounds;
        // The first measurement can reflect the application's enforced minimum.
        // Only subsequent native size changes should resize the panel on the canvas.
        var lastSize = panel.LastNativeSize;
        if (lastSize is { } previous)
        {
            if (width != previous.Width)
                bounds = bounds with { Width = width / dpi.DpiScaleX / camera.Zoom + 2 };
            if (height != previous.Height)
                bounds = bounds with { Height = height / dpi.DpiScaleY / camera.Zoom + TitleHeight + 2 };
        }
        panel.LastNativeSize = (width, height);
        if (syncPosition)
        {
            var topLeft = PointFromScreen(new WpfPoint(screenBounds.Left, screenBounds.Top));
            var origin = Canvas.Domain.Geometry.ViewToWorld(new DomainPoint(topLeft.X, topLeft.Y), camera);
            bounds = bounds with { X = origin.X - 1, Y = origin.Y - TitleHeight - 1 };
        }

        if (bounds != panel.Bounds || lastSize != (width, height))
        {
            panel.Bounds = bounds;
            Render();
            MapChanged?.Invoke();
        }
    }

    public IReadOnlyList<FrameworkElement> OccludingControls { get; set; } = [];

    public MinimapItem[] GetMapItems() => panels.Where(panel => panel.Space == space && !panel.WaitingForRemoval)
        .Select(panel => new MinimapItem(panel.DisplayBounds, null, IsPanel: true)).ToArray();

    public void ShowPicker()
    {
        RefreshWindows();
        var candidates = catalog.List().Where(window => panels.All(panel => panel.Window.Identity != window.Identity))
            .ToArray();
        var picker = new WindowPicker(owner, candidates);
        if (picker.ShowDialog() == true && picker.SelectedWindow is { } selected)
        {
            Add(selected);
        }
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

        var width = Math.Clamp(ActualWidth, 3, 1060);
        var height = Math.Clamp(ActualHeight, TitleHeight + 3, 810);
        var center = CanvasBounds.Viewport(camera, new DomainPoint(ActualWidth, ActualHeight)).Center;
        var desired = new CanvasBounds(center.X - width / 2, center.Y - height / 2, width, height);
        var panel = new Panel(window, space, PanelPlacement.FindFree(desired, Occupied()), new PanelCard(window.Title), instance);
        panels.Add(panel);
        Children.Add(panel.Card);
        panel.Card.Handle.PreviewMouseLeftButtonDown += (_, e) => StartDrag(panel, e);
        panel.Card.Handle.MouseMove += (_, e) => MoveDrag(e);
        panel.Card.Handle.PreviewMouseLeftButtonUp += (_, e) => FinishDrag(e);
        panel.Card.Handle.LostMouseCapture += (_, _) => CancelDrag();
        panel.Card.PreviewButton.Click += (_, _) => ActivationRequested?.Invoke(panel);
        panel.Card.FoldRequested += () => FoldRequested?.Invoke(panel);
        panel.Card.MaximizeRequested += () => MaximizeRequested?.Invoke(panel);
        panel.Card.CloseRequested += () => CloseRequested?.Invoke(panel);
        panel.Card.RemoveRequested += () => RemoveRequested?.Invoke(panel);
        _ = LoadIconAsync(panel);
        Render();
        MapChanged?.Invoke();
    }

    private async Task LoadIconAsync(Panel panel)
    {
        try
        {
            using var process = Process.GetProcessById(panel.Window.Identity.ProcessId);
            if (process.MainModule?.FileName is { } path)
            {
                var (image, _) = await shell.ReadImageAsync(path, 32, CancellationToken.None, iconOnly: true);
                if (image is not null && !disposed && panels.Contains(panel))
                {
                    panel.Card.SetIcon(image);
                }
            }
        }
        catch (Exception error) when (error is ArgumentException or Win32Exception or InvalidOperationException or OperationCanceledException)
        {
            Trace.TraceInformation(error.Message);
        }
    }

    public void ShowSpace(SpaceId activeSpace, Camera activeCamera)
    {
        if (space != activeSpace)
        {
            CancelDrag();
            var animate = space != default && Theme.AnimationsEnabled;
            space = activeSpace;
            BeginAnimation(RevealProperty, animate ? CreateRevealAnimation() : null);
        }

        camera = activeCamera;
        Render();
    }

    private static DoubleAnimationUsingKeyFrames CreateRevealAnimation()
    {
        var animation = new DoubleAnimationUsingKeyFrames { FillBehavior = FillBehavior.Stop };
        animation.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        animation.KeyFrames.Add(new SplineDoubleKeyFrame(1, KeyTime.FromTimeSpan(VisualTokens.Motion.SpaceChange),
            Theme.MotionSpline()));
        return animation;
    }

    private static byte PreviewOpacity(double reveal) =>
        (byte)Math.Round(reveal * VisualTokens.RestingPanelBrightness * byte.MaxValue);

    private static void OnRevealChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var layer = (WindowPanels)sender;
        var reveal = (double)e.NewValue;
        layer.Opacity = reveal;
        var opacity = PreviewOpacity(reveal);
        foreach (var panel in layer.panels)
        {
            if (panel.Space == layer.space)
            {
                panel.Thumbnail?.SetOpacity(opacity);
            }
        }
    }

    public void RefreshWindows()
    {
        var unavailable = GetUnavailablePanels().ToHashSet();
        var mapChanged = false;
        foreach (var panel in panels.ToArray())
        {
            var hidden = unavailable.Contains(panel);
            var visibilityChanged = panel.WaitingForRemoval != hidden;
            panel.WaitingForRemoval = hidden;
            if (panel.CloseRequest is { Completed: true, Accepted: false })
            {
                panel.CloseRequest.Dispose();
                panel.CloseRequest = null;
            }

            if (hidden && releaseUnavailable(panel))
            {
                Remove(panel);
            }
            else
            {
                mapChanged |= visibilityChanged;
            }
        }

        Render();
        if (mapChanged)
        {
            MapChanged?.Invoke();
        }
    }

    public void Remove(Panel panel)
    {
        if (drag?.Panel == panel)
        {
            CancelDrag();
        }

        if (panels.Remove(panel))
        {
            panel.Dispose();
            Children.Remove(panel.Card);
            MapChanged?.Invoke();
        }
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

        var mapChanged = false;
        var opacity = PreviewOpacity((double)GetValue(RevealProperty));
        foreach (var panel in panels)
        {
            var shown = panel.Space == space && !panel.WaitingForRemoval && owner.WindowState != WindowState.Minimized;
            panel.Card.Visibility = shown ? Visibility.Visible : Visibility.Collapsed;
            if (!shown)
            {
                panel.Thumbnail?.Hide();
                continue;
            }

            var bounds = panel.Bounds;
            if (panel == ActivePanel && !ProjectActive && panel.LastNativeSize is { } nativeSize)
            {
                var dpi = VisualTreeHelper.GetDpi(this);
                bounds = bounds with
                {
                    Width = nativeSize.Width / dpi.DpiScaleX / camera.Zoom + 2,
                    Height = nativeSize.Height / dpi.DpiScaleY / camera.Zoom + TitleHeight + 2
                };
            }
            var minimized = WindowCatalog.IsMinimized(panel.Window.Handle);
            var compact = minimized || panel.IsFolded;
            DomainPoint? size = null;
            if (compact)
            {
                bounds = bounds with { Height = TitleHeight + 2 };
            }
            else if (panel != ActivePanel || ProjectActive)
            {
                panel.Thumbnail ??= WindowThumbnail.Register(source.Handle, panel.Window.Handle);
                size = panel.Thumbnail?.ReadSourceSize();
            }

            mapChanged |= bounds != panel.DisplayBounds;
            panel.DisplayBounds = bounds;
            var point = Canvas.Domain.Geometry.WorldToView(new DomainPoint(bounds.X, bounds.Y), camera);
            SetLeft(panel.Card, point.X);
            SetTop(panel.Card, point.Y);
            panel.Card.Width = bounds.Width;
            panel.Card.Height = bounds.Height;
            panel.Card.RenderTransform = new ScaleTransform(camera.Zoom, camera.Zoom);
            panel.Card.SetActive(panel == ActivePanel);
            panel.Card.SetCompact(compact);

            if (panel == ActivePanel && !ProjectActive)
            {
                panel.Thumbnail?.Hide();
                panel.Card.Text.Text = "";
                continue;
            }

            if (compact)
            {
                panel.Thumbnail?.Hide();
                panel.Card.Text.Text = "";
                continue;
            }

            var area = new Rect(ToDevice(point.X + camera.Zoom, point.Y + (TitleHeight + 1) * camera.Zoom),
                ToDevice(point.X + (bounds.Width - 1) * camera.Zoom, point.Y + (bounds.Height - 1) * camera.Zoom));
            panel.Card.Text.Text = size is { } dimensions &&
                panel.Thumbnail?.Display(area, clip, controls, dimensions, panel == ActivePanel ? byte.MaxValue : opacity) == true ? "" : "Vista previa no disponible";
            if (size is null)
            {
                panel.Thumbnail?.Hide();
            }
        }

        if (mapChanged)
        {
            MapChanged?.Invoke();
        }
    }

    private CanvasBounds[] Occupied(Panel? except = null) =>
        [.. iconBounds(), .. panels.Where(panel => panel.Space == space && panel != except).Select(panel => panel.Bounds)];

    private void StartDrag(Panel panel, MouseButtonEventArgs e)
    {
        if (drag is null && panel.Card.Handle.CaptureMouse())
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
        var bounds = PanelPlacement.FindFree(desired, Occupied(active.Panel));
        if (bounds == active.Panel.Bounds) return;
        active.Panel.Bounds = bounds;
        PanelMoved?.Invoke(active.Panel);
        Render();
        MapChanged?.Invoke();
    }

    private void FinishDrag(MouseButtonEventArgs e)
    {
        if (drag is { } active)
        {
            UpdateDrag(active, e.GetPosition(this));
            drag = null;
            active.Panel.Card.Handle.ReleaseMouseCapture();
            e.Handled = true;
            if (!active.HasMoved)
            {
                var panel = active.Panel;
                if (WindowCatalog.IsMinimized(panel.Window.Handle))
                {
                    ActivationRequested?.Invoke(panel);
                }
                else if (panel.IsFolded)
                {
                    FoldRequested?.Invoke(panel);
                }
            }
        }
    }

    public void CancelDrag()
    {
        if (drag is { } active)
        {
            drag = null;
            active.Panel.Bounds = active.Original;
            if (active.HasMoved) PanelMoved?.Invoke(active.Panel);
            active.Panel.Card.Handle.ReleaseMouseCapture();
            Render();
            MapChanged?.Invoke();
        }
    }

    public void Dispose()
    {
        disposed = true;
        BeginAnimation(RevealProperty, null);
        foreach (var panel in panels)
        {
            panel.Dispose();
        }

        panels.Clear();
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
        public CanvasBounds DisplayBounds { get; set; } = bounds;
        public (int Width, int Height)? LastNativeSize { get; set; }
        public PanelCard Card { get; } = card;
        public WindowThumbnail? Thumbnail { get; set; }
        public WindowInstance Instance { get; } = instance;
        public bool IsFolded { get; set; }
        public PendingWindowClose? CloseRequest { get; set; }
        public bool WaitingForRemoval { get; set; }

        public void Dispose()
        {
            CloseRequest?.Dispose();
            Thumbnail?.Dispose();
            Instance.Dispose();
        }
    }
}
