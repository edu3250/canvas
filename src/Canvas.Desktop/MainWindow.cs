using Canvas.Domain;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
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
    private readonly WindowPanels windowPanels;
    private readonly WindowRecovery recovery;
    private readonly NativeWindowPlacement placement;
    private readonly DispatcherTimer edgeTimer = new() { Interval = TimeSpan.FromMilliseconds(75) };
    private Camera mapCamera;
    private nint windowHandle;
    private bool hotkeyRegistered;
    private readonly DispatcherTimer previewTimer = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private readonly Button homeButton;
    private readonly MinimapWindow minimap = new();
    private readonly TextBlock zoomValue = new() { MinWidth = 52, TextAlignment = TextAlignment.Center,
        Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center };
    private readonly Button zoomOut = CreateZoomButton("−", "Alejar");
    private readonly Button zoomIn = CreateZoomButton("+", "Acercar");
    private readonly Button fitContent = CreateZoomButton("⛶", "Encuadrar el contenido");
    private readonly ShellItems shell = new();
    private readonly Dictionary<SpaceId, SpaceFolderMonitor> folders;
    private readonly string layoutPath;
    private readonly Dictionary<SpaceId, Dictionary<string, DomainPoint>> savedLayouts;
    private readonly DispatcherTimer folderTimer = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private bool readingFolders;
    private bool closed;
    private string? previousClick;
    private SessionState session;
    private Gesture? gesture;

    internal MainWindow(WindowRecovery recovery, NativeWindowPlacement placement)
    {
        this.recovery = recovery;
        this.placement = placement;
        Title = "Escritorio Canvas";
        Width = InitialWidth;
        Height = InitialHeight;
        MinWidth = 640;
        MinHeight = 480;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowState = WindowState.Maximized;

        var home = new Camera(new Canvas.Domain.Point(0, 0), 1);
        session = new SessionState(Programming, new Dictionary<SpaceId, SpaceNavigation>
        {
            [Programming] = new(home, home, null),
            [Games] = new(home, home, null)
        });
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        folders = session.Spaces.Keys.ToDictionary(space => space,
            space => new SpaceFolderMonitor(SpaceFolder.GetPath(profile, space)));
        layoutPath = HomeLayout.GetPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        var loaded = HomeLayout.Load(layoutPath);
        savedLayouts = loaded.Layouts;
        if (loaded.Damaged)
        {
            System.Diagnostics.Trace.TraceWarning("El archivo de posiciones no se pudo leer; los iconos se acomodan de nuevo.");
        }

        spaceView = new SpaceView(shell, folders.Keys);
        windowPanels = new WindowPanels(this, () => spaceView.GetMapItems().Select(item => item.Bounds).ToArray());
        windowPanels.MapChanged += RefreshMap;
        windowPanels.PreviewMouseWheel += ZoomSurface;
        windowPanels.ActivationRequested += ActivatePanel;
        previewTimer.Tick += (_, _) =>
        {
            if (windowPanels.ActivePanel is { } panel &&
                (!WindowCatalog.IsLive(panel.Window.Identity, panel.Instance) ||
                    WindowCatalog.IsMinimized(panel.Window.Handle)))
            {
                ReturnToMap(restoreMinimized: false);
            }

            windowPanels.RefreshWindows();
        };
        edgeTimer.Tick += (_, _) => SyncMinimapWindow();
        PreviewMouseDown += (_, e) =>
        {
            if (windowPanels.ActivePanel is not null)
            {
                e.Handled = true;
                ReturnToMap();
            }
        };
        spaceView.MapChanged += RefreshMap;
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
        Deactivated += (_, _) =>
        {
            windowPanels.CancelDrag();
            CancelGesture();
        };
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape && windowPanels.IsMouseCaptureWithin)
            {
                windowPanels.CancelDrag();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && gesture is not null)
            {
                CancelGesture();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && spaceView.IsKeyboardFocusWithin && gesture is null)
            {
                foreach (var path in spaceView.GetSelection())
                {
                    ShellItems.Open(path);
                }

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
        viewport.Children.Add(windowPanels);
        var toolbar = CreateToolbar();
        var zoomControls = CreateZoomControls();
        viewport.Children.Add(toolbar);
        viewport.Children.Add(zoomControls);
        windowPanels.OccludingControls = [toolbar, zoomControls];
        Content = viewport;
        RefreshSpace();

        minimap.CenterRequested += SelectMapPoint;
        SourceInitialized += (_, _) =>
        {
            windowHandle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(windowHandle).AddHook(HandleHotkey);
            hotkeyRegistered = NativePanelInput.RegisterReturn(windowHandle);
        };
        ContentRendered += (_, _) =>
        {
            if (minimap.Owner is null)
            {
                minimap.Owner = this;
                SyncMinimapWindow();
                if (!hotkeyRegistered)
                {
                    MessageBox.Show(this, "Ctrl+Alt+Espacio no está disponible. Puedes volver al mapa con el minimapa o con un clic en el lienzo.",
                        Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        };
        StateChanged += (_, _) =>
        {
            SyncMinimapWindow();
            windowPanels.Render();
        };
        LocationChanged += (_, _) => SyncMinimapWindow();
        spaceView.SizeChanged += (_, _) =>
        {
            SyncMinimapWindow();
            windowPanels.Render();
        };
        DpiChanged += (_, _) => Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)SyncMinimapWindow);

        Loaded += (_, _) =>
        {
            folderTimer.Start();
            previewTimer.Start();
            _ = RefreshFoldersAsync();
        };
        folderTimer.Tick += (_, _) => _ = RefreshFoldersAsync();
        Closed += (_, _) =>
        {
            closed = true;
            folderTimer.Stop();
            previewTimer.Stop();
            edgeTimer.Stop();
            if (hotkeyRegistered)
            {
                NativePanelInput.UnregisterReturn(windowHandle);
            }
            windowPanels.Dispose();
            shell.Dispose();
            if (!readingFolders)
            {
                DisposeFolders();
            }
        };
    }

    private DomainPoint ViewportSize => new(spaceView.ActualWidth, spaceView.ActualHeight);

    private void SyncMinimapWindow()
    {
        if (minimap.Owner is null || closed)
        {
            return;
        }

        if (WindowState == WindowState.Minimized ||
            windowPanels.ActivePanel is not null && !PointerRevealsMinimap())
        {
            minimap.Hide();
            return;
        }

        minimap.AnchorTo(spaceView);
        if (!minimap.IsVisible)
        {
            minimap.Show();
        }
    }

    private void RefreshMap()
    {
        var size = ViewportSize;
        var camera = ActiveCamera;
        MinimapItem[] items = [.. spaceView.GetMapItems(includePreview: true), .. windowPanels.GetMapItems()];
        zoomValue.Text = camera.Zoom < 0.01 ? "<1 %" : $"{camera.Zoom * 100:0} %";
        zoomOut.IsEnabled = camera.Zoom > CameraGestures.MinZoom;
        zoomIn.IsEnabled = camera.Zoom < CameraGestures.MaxZoom;
        fitContent.IsEnabled = items.Length != 0;
        if (size.X > 0 && size.Y > 0)
        {
            minimap.UpdateMap(CanvasBounds.Viewport(camera, size), items);
        }
    }

    private bool PointerRevealsMinimap()
    {
        if (!NativePanelInput.TryGetPointer(out var pointer))
        {
            return false;
        }

        var relative = spaceView.PointFromScreen(pointer);
        var bottom = spaceView.ActualHeight;
        var right = spaceView.ActualWidth;
        return pointer.Y >= NativePanelInput.ScreenBottom - 2 ||
            relative.Y >= bottom - 2 && relative.X >= 0 && relative.X <= right ||
            minimap.IsVisible && relative.Y >= bottom - MinimapWindow.MapHeight - MinimapWindow.Inset &&
                relative.X >= right - MinimapWindow.MapWidth - MinimapWindow.Inset && relative.X <= right;
    }

    private nint HandleHotkey(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (message == NativePanelInput.HotkeyMessage && wParam == NativePanelInput.ReturnHotkey)
        {
            handled = true;
            ReturnToMap();
        }

        return 0;
    }

    private void ActivatePanel(WindowPanels.Panel panel)
    {
        if (panel == windowPanels.ActivePanel || !ReturnToMap())
        {
            return;
        }

        CancelGesture();
        var previous = ActiveCamera;
        var identity = panel.Window.Identity;
        var saved = false;
        try
        {
            NativeWindowPlacement.EnsureCurrent(panel.Instance);
            recovery.SaveOriginal(identity.Handle, panel.Instance);
            saved = true;

            ApplyNavigation(new SetCamera(CameraGestures.CenterOn(panel.Bounds.Center, ViewportSize, 1)));
            var origin = windowPanels.GetWindowOrigin(panel);
            placement.MoveTo(identity, panel.Instance, (int)Math.Round(origin.X), (int)Math.Round(origin.Y));
            NativeWindowPlacement.EnsureCurrent(panel.Instance);
            if (!NativePanelInput.SetForegroundWindow((nint)identity.Handle))
            {
                throw new InvalidOperationException("Windows denied foreground activation.");
            }

            mapCamera = previous;
            windowPanels.ActivePanel = panel;
            windowPanels.Render();
            minimap.Hide();
            edgeTimer.Start();
        }
        catch (Exception error) when (IsPanelError(error))
        {
            Trace.TraceWarning(error.ToString());
            var restored = !saved || RestorePanel(panel);
            ApplyNavigation(new SetCamera(previous));
            MessageBox.Show(this, restored ? "No se pudo activar la ventana en su panel." :
                "No se pudo devolver la ventana. Cierra Escritorio Canvas y ejecuta «Devolver ventanas».",
                Title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private bool ReturnToMap(bool restoreMinimized = true)
    {
        if (windowPanels.ActivePanel is not { } panel)
        {
            return true;
        }

        windowPanels.ActivePanel = null;
        edgeTimer.Stop();
        var restored = !restoreMinimized && WindowCatalog.IsLive(panel.Window.Identity, panel.Instance) &&
            WindowCatalog.IsMinimized(panel.Window.Handle) || RestorePanel(panel);
        if (!restored)
        {
            MessageBox.Show(this, "No se pudo devolver la ventana. Cierra Escritorio Canvas y ejecuta «Devolver ventanas».",
                Title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        ApplyNavigation(new SetCamera(mapCamera));
        windowPanels.Render();
        Activate();
        SyncMinimapWindow();
        return restored;
    }

    private bool RestorePanel(WindowPanels.Panel panel)
    {
        try
        {
            recovery.RestoreWindow(panel.Window.Handle);
            return true;
        }
        catch (Exception error) when (IsPanelError(error))
        {
            Trace.TraceWarning(error.ToString());
            return false;
        }
    }

    private static bool IsPanelError(Exception error) =>
        error is IOException or UnauthorizedAccessException or Win32Exception or InvalidOperationException;

    private void SelectMapPoint(DomainPoint world)
    {
        if (windowPanels.FindPanel(world) is { } panel)
        {
            ActivatePanel(panel);
        }
        else if (ReturnToMap())
        {
            CenterMap(world);
        }
    }

    private void CenterMap(DomainPoint world)
    {
        CancelGesture();
        ApplyNavigation(new SetCamera(CameraGestures.CenterOn(world, ViewportSize, ActiveCamera.Zoom)));
    }

    private Border CreateZoomControls()
    {
        zoomOut.Click += (_, _) => StepZoom(-1);
        zoomIn.Click += (_, _) => StepZoom(1);
        fitContent.Click += (_, _) => FitContent();

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(zoomOut);
        row.Children.Add(zoomValue);
        row.Children.Add(zoomIn);
        row.Children.Add(fitContent);
        return new Border
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, MinimapWindow.MapWidth + MinimapWindow.Inset + 8, MinimapWindow.Inset),
            Padding = new Thickness(5),
            CornerRadius = new CornerRadius(8),
            Background = BrushFromRgb(24, 51, 65),
            Child = row
        };
    }

    private static Button CreateZoomButton(string content, string name)
    {
        var button = new Button { Content = content, Width = 36, Height = 32, FontSize = 16, ToolTip = name };
        AutomationProperties.SetName(button, name);
        return button;
    }

    private void StepZoom(double steps)
    {
        CancelGesture();
        var size = ViewportSize;
        var center = new DomainPoint(size.X / 2, size.Y / 2);
        ApplyNavigation(new SetCamera(CameraGestures.ZoomAt(ActiveCamera, center, steps)));
    }

    private void FitContent()
    {
        CancelGesture();
        MinimapItem[] items = [.. spaceView.GetMapItems(), .. windowPanels.GetMapItems()];
        if (items.Length != 0)
        {
            var bounds = items.Select(item => item.Bounds).Aggregate((left, right) => left.Union(right));
            ApplyNavigation(new SetCamera(bounds.Fit(ViewportSize, 64, 1)));
        }
    }

    private async Task RefreshFoldersAsync()
    {
        if (readingFolders || closed)
        {
            return;
        }

        readingFolders = true;
        try
        {
            await Task.WhenAll(folders.Select(async entry =>
            {
                var (space, monitor) = entry;
                (string[] Paths, (string OldPath, string NewPath)[] Renames) snapshot;
                try
                {
                    snapshot = await Task.Run(monitor.ReadSnapshot);
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                {
                    System.Diagnostics.Trace.TraceWarning("No se pudo leer la carpeta del espacio: {0}", error.Message);
                    return;
                }

                if (closed || snapshot.Renames.Length == 0 && !spaceView.HasChanges(space, snapshot.Paths))
                {
                    return;
                }

                if (space == session.ActiveSpaceId)
                {
                    CancelGesture();
                    previousClick = null;
                }

                spaceView.UpdateItems(space, snapshot.Paths, snapshot.Renames,
                    savedLayouts.GetValueOrDefault(space) ?? []);
                SaveLayout(space);
            }));
        }
        finally
        {
            readingFolders = false;
            if (closed)
            {
                DisposeFolders();
            }
        }
    }

    private void SaveLayout(SpaceId space)
    {
        // Only spaces whose folder has been read replace their saved entry; the others keep it intact.
        savedLayouts[space] = spaceView.GetLayout(space);
        try
        {
            HomeLayout.Save(layoutPath, savedLayouts);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            System.Diagnostics.Trace.TraceWarning("No se pudieron guardar las posiciones: {0}", error.Message);
        }
    }

    private void DisposeFolders()
    {
        foreach (var folder in folders.Values)
        {
            folder.Dispose();
        }
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

        spaceView.Focus();
        var openOnRelease = kind == GestureKind.Icons && e.ClickCount == 2 && previousClick == icon;
        previousClick = null;
        gesture = new Gesture(e.ChangedButton, spaceView.GetPointer(e), ActiveCamera,
            kind.Value, icon, spaceView.GetSelection(), openOnRelease);
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
            SaveLayout(session.ActiveSpaceId);
        }

        FinishGesture();
        if (active.Kind == GestureKind.Icons && !active.HasMoved && spaceView.FindIcon(spaceView.InputHitTest(e.GetPosition(spaceView))!) == active.Icon)
        {
            if (active.OpenOnRelease)
            {
                ShellItems.Open(active.Icon!);
            }
            else
            {
                previousClick = active.Icon;
            }
        }

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
        var folderButton = new Button
        {
            Content = "Abrir carpeta del espacio",
            Margin = new Thickness(8, 0, 0, 0),
            Padding = new Thickness(12, 4, 12, 4)
        };
        folderButton.Click += (_, _) => ShellItems.Open(folders[session.ActiveSpaceId].Path);
        controls.Children.Add(folderButton);
        var addWindow = new Button
        {
            Content = "Añadir ventana",
            Margin = new Thickness(8, 0, 0, 0),
            Padding = new Thickness(12, 4, 12, 4)
        };
        addWindow.Click += (_, _) => windowPanels.ShowPicker(addWindow);
        controls.Children.Add(addWindow);
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
            previousClick = null;
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
        GestureKind Kind, string? Icon, string[] Selection, bool OpenOnRelease)
    {
        public bool HasMoved { get; set; }
    }

    private void RefreshSpace()
    {
        var navigation = session.Spaces[session.ActiveSpaceId];
        windowPanels.ShowSpace(session.ActiveSpaceId, navigation.Camera);
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
