using Canvas.Domain;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using SurfaceCanvas = System.Windows.Controls.Canvas;
using ShapePath = System.Windows.Shapes.Path;
using DomainPoint = Canvas.Domain.Point;

namespace Canvas.Desktop;

public sealed class MainWindow : Window
{
    private const double InitialWidth = 1100;
    private const double InitialHeight = 720;
    private static readonly SpaceId Programming = new("Programación");
    private static readonly SpaceId Games = new("Juegos");
    private readonly SpaceView spaceView;
    private readonly Button homeButton;
    private SessionState session;
    private Gesture? gesture;

    public MainWindow()
    {
        Title = "Escritorio Canvas";
        Width = InitialWidth;
        Height = InitialHeight;
        MinWidth = 640;
        MinHeight = 480;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var home = new Camera(new Canvas.Domain.Point(0, 0), 1);
        session = new SessionState(Programming, new Dictionary<SpaceId, SpaceNavigation>
        {
            [Programming] = new(home, home, null),
            [Games] = new(home, home, null)
        });
        spaceView = new SpaceView(Programming, Games);
        spaceView.PreviewMouseDown += BeginGesture;
        spaceView.MouseMove += ContinueGesture;
        spaceView.PreviewMouseUp += EndGesture;
        spaceView.LostMouseCapture += (_, _) =>
        {
            if (!spaceView.IsMouseCaptured)
            {
                CancelGesture();
            }
        };
        spaceView.PreviewMouseWheel += ZoomSurface;
        Deactivated += (_, _) => CancelGesture();
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape && gesture is not null)
            {
                CancelGesture();
                e.Handled = true;
            }
        };
        homeButton = new Button { MinWidth = 130, Padding = new Thickness(12, 4, 12, 4) };
        homeButton.Click += (_, _) => ApplyNavigation(new ToggleHome());

        var viewport = new Grid { ClipToBounds = true };
        viewport.Children.Add(new Viewbox
        {
            Stretch = Stretch.UniformToFill,
            IsHitTestVisible = false,
            Child = CreateLandscape()
        });
        viewport.Children.Add(spaceView);
        viewport.Children.Add(CreateToolbar());
        Content = viewport;
        RefreshSpace();
    }

    private Camera ActiveCamera => session.Spaces[session.ActiveSpaceId].Camera;

    private void BeginGesture(object sender, MouseButtonEventArgs e)
    {
        if (gesture is not null)
        {
            e.Handled = true;
            return;
        }

        var background = spaceView.IsBackground(e.OriginalSource);
        var icon = spaceView.FindIcon(e.OriginalSource);
        var kind = e.ChangedButton switch
        {
            MouseButton.Right when background => GestureKind.Pan,
            MouseButton.Left when background => GestureKind.Marquee,
            MouseButton.Left when icon is not null => GestureKind.Icons,
            _ => (GestureKind?)null
        };
        if (kind is null || !spaceView.CaptureMouse())
        {
            return;
        }

        gesture = new Gesture(e.ChangedButton, spaceView.GetPointer(e), ActiveCamera,
            kind.Value, icon, spaceView.GetSelection());
        if (kind == GestureKind.Marquee)
        {
            spaceView.SetSelection([]);
        }
        else if (kind == GestureKind.Icons && !gesture.Selection.Contains(icon!))
        {
            spaceView.SetSelection([icon!]);
        }

        e.Handled = true;
    }

    private void ContinueGesture(object sender, MouseEventArgs e)
    {
        if (gesture is not { } active)
        {
            return;
        }

        var pressed = active.Button == MouseButton.Right ? e.RightButton : e.LeftButton;
        if (pressed != MouseButtonState.Pressed || !spaceView.IsMouseCaptured)
        {
            CancelGesture();
            return;
        }

        UpdateGesture(active, spaceView.GetPointer(e));
        e.Handled = true;
    }

    private void UpdateGesture(Gesture active, DomainPoint pointer)
    {
        var delta = new DomainPoint(pointer.X - active.Pointer.X, pointer.Y - active.Pointer.Y);
        switch (active.Kind)
        {
            case GestureKind.Pan:
                ApplyNavigation(new SetCamera(CameraGestures.Pan(active.Camera, delta)));
                break;
            case GestureKind.Marquee:
                if (delta.X != 0 || delta.Y != 0 || active.HasMoved)
                {
                    active.HasMoved = true;
                    spaceView.ShowMarquee(active.Pointer, pointer);
                }
                break;
            case GestureKind.Icons:
                if (active.HasMoved || Math.Abs(delta.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(delta.Y) >= SystemParameters.MinimumVerticalDragDistance)
                {
                    active.HasMoved = true;
                    spaceView.PreviewMove(new DomainPoint(delta.X / active.Camera.Zoom, delta.Y / active.Camera.Zoom));
                }
                break;
        }
    }

    private void EndGesture(object sender, MouseButtonEventArgs e)
    {
        if (gesture is not { } active || active.Button != e.ChangedButton)
        {
            return;
        }

        UpdateGesture(active, spaceView.GetPointer(e));
        if (active.Kind == GestureKind.Icons && active.HasMoved)
        {
            spaceView.CommitMove(active.Icon!);
        }

        FinishGesture();
        e.Handled = true;
    }

    private void CancelGesture()
    {
        if (gesture is not { } active)
        {
            return;
        }

        FinishGesture();
        spaceView.SetSelection(active.Selection);
        if (active.Kind == GestureKind.Pan)
        {
            ApplyNavigation(new SetCamera(active.Camera));
        }
    }

    private void FinishGesture()
    {
        gesture = null;
        spaceView.ClearPreview();
        if (spaceView.IsMouseCaptured)
        {
            spaceView.ReleaseMouseCapture();
        }
    }

    private void ZoomSurface(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;
        if (gesture is null)
        {
            var cursor = spaceView.GetPointer(e);
            ApplyNavigation(new SetCamera(CameraGestures.ZoomAt(ActiveCamera, cursor, e.Delta / 120.0)));
        }
    }

    private Border CreateToolbar()
    {
        var selector = new ComboBox
        {
            ItemsSource = new[] { Programming, Games },
            DisplayMemberPath = nameof(SpaceId.Value),
            SelectedItem = session.ActiveSpaceId,
            MinWidth = 142,
            Margin = new Thickness(0, 0, 8, 0),
            VerticalContentAlignment = VerticalAlignment.Center
        };
        AutomationProperties.SetName(selector, "Espacio");
        selector.SelectionChanged += (_, _) =>
        {
            if (selector.SelectedItem is SpaceId space)
            {
                ApplyNavigation(new SwitchSpace(space));
            }
        };

        var controls = new StackPanel { Orientation = Orientation.Horizontal };
        controls.Children.Add(selector);
        controls.Children.Add(homeButton);
        return new Border
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(12),
            Padding = new Thickness(6),
            CornerRadius = new CornerRadius(8),
            Background = BrushFromRgb(24, 51, 65),
            Child = controls
        };
    }

    private void ApplyNavigation(NavigationCommand command)
    {
        if (command is ToggleHome || command is SwitchSpace change && change.Target != session.ActiveSpaceId)
        {
            CancelGesture();
        }

        var result = Navigation.Apply(session, command);
        if (ReferenceEquals(result.State, session))
        {
            return;
        }

        session = result.State;
        RefreshSpace();
    }

    private enum GestureKind { Pan, Marquee, Icons }

    private sealed record Gesture(MouseButton Button, DomainPoint Pointer, Camera Camera,
        GestureKind Kind, string? Icon, string[] Selection)
    {
        public bool HasMoved { get; set; }
    }

    private void RefreshSpace()
    {
        var navigation = session.Spaces[session.ActiveSpaceId];
        spaceView.ShowSpace(session.ActiveSpaceId, navigation.Camera);
        var homeLabel = navigation.ReturnView.HasValue ? "Volver a mi vista" : "Inicio";
        homeButton.Content = homeLabel;
        AutomationProperties.SetName(homeButton, homeLabel);
    }

    private static SurfaceCanvas CreateLandscape()
    {
        var landscape = new SurfaceCanvas
        {
            Width = InitialWidth,
            Height = InitialHeight,
            Background = new LinearGradientBrush(
                Color.FromRgb(79, 116, 144), Color.FromRgb(236, 201, 163), 90)
        };

        var sun = new Ellipse { Width = 100, Height = 100, Fill = BrushFromRgb(255, 222, 174) };
        SurfaceCanvas.SetLeft(sun, 752);
        SurfaceCanvas.SetTop(sun, 125);
        landscape.Children.Add(sun);

        AddLayer(landscape, "M0,434 L0,312 158,232 241,277 386,168 498,257 563,221 735,352 893,244 1100,326 1100,434 Z",
            BrushFromRgb(104, 131, 146));
        AddLayer(landscape, "M0,449 L0,337 128,291 270,369 396,307 517,356 668,282 787,342 942,302 1100,365 1100,449 Z",
            BrushFromRgb(66, 102, 118));
        AddLayer(landscape, "M0,459 L0,387 145,362 246,398 411,382 550,426 722,388 887,414 1014,369 1100,382 1100,459 Z",
            BrushFromRgb(41, 76, 88));

        var lake = new Rectangle
        {
            Width = InitialWidth,
            Height = 282,
            Fill = new LinearGradientBrush(Color.FromRgb(125, 155, 157), Color.FromRgb(27, 65, 77), 90)
        };
        SurfaceCanvas.SetTop(lake, 438);
        landscape.Children.Add(lake);

        AddLayer(landscape, "M628,461 H903 L850,466 H661 Z M715,486 H981 L913,491 H753 Z M566,525 H766 L729,529 H587 Z",
            new SolidColorBrush(Color.FromArgb(66, 255, 222, 174)));
        AddLayer(landscape, "M0,612 Q160,623 306,679 Q448,708 585,720 H0 Z M1100,620 Q1000,643 934,688 L870,720 H1100 Z",
            BrushFromRgb(18, 52, 63));
        return landscape;
    }

    private static SolidColorBrush BrushFromRgb(byte red, byte green, byte blue) =>
        new(Color.FromRgb(red, green, blue));

    private static void AddLayer(SurfaceCanvas landscape, string outline, Brush fill)
    {
        landscape.Children.Add(new ShapePath { Data = System.Windows.Media.Geometry.Parse(outline), Fill = fill });
    }
}
