using Canvas.Domain;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using System.IO;
using DomainPoint = Canvas.Domain.Point;

namespace Canvas.Desktop;

public sealed class MainWindow : Window
{
    private Action<string>? sampleProjection;
    private const double ActivationDurationMs = 200;
    private const double InitialWidth = 1100;
    private const double InitialHeight = 720;
    private readonly ResourceDictionary spaceTheme = Theme.CreateResources(VisualTokens.DefaultLight);
    private SpaceId displayedSpace;
    private static readonly Brush InactiveSpaceBrush = DeskControls.Brush(VisualTokens.Colors.Raised);
    private static readonly SpaceId Programming = new("Programación");
    private static readonly SpaceId Games = new("Juegos");
    private readonly SpaceView spaceView;
    private readonly WindowPanels windowPanels;
    private readonly WindowRecovery recovery;
    private readonly NativeWindowPlacement placement;
    private readonly DispatcherTimer edgeTimer = new() { Interval = TimeSpan.FromMilliseconds(75) };
    private Camera mapCamera;
    private PanelWheel? panelWheel;
    private nint windowHandle;
    private bool hotkeyRegistered;
    private readonly TextBlock returnShortcutStatus = new()
    {
        Foreground = DeskControls.Brush(VisualTokens.Colors.Warning),
        Background = DeskControls.Brush(VisualTokens.Colors.Slate),
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Bottom,
        Margin = new Thickness(12),
        Padding = new Thickness(8, 4, 8, 4),
        IsHitTestVisible = false,
        Text = "Ctrl+Alt+Espacio no disponible. Usa el minimapa para volver.",
        TextTrimming = TextTrimming.CharacterEllipsis,
        Visibility = Visibility.Collapsed
    };
    private bool activationPending;
    private bool cancelActivation;
    private readonly DispatcherTimer previewTimer = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private readonly Button homeButton;
    private readonly Dictionary<SpaceId, Button> spaceButtons = [];
    private readonly MinimapWindow minimap;
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
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        FontFamily = Theme.Font;
        FontSize = VisualTokens.Type.Interface;
        Foreground = DeskControls.Brush(VisualTokens.Colors.Bone);
        Background = DeskControls.NightGrid();
        UseLayoutRounding = true;
        var restoredChrome = new WindowChrome
        {
            CaptionHeight = 0,
            ResizeBorderThickness = SystemParameters.WindowResizeBorderThickness,
            GlassFrameThickness = new Thickness(0),
            UseAeroCaptionButtons = false
        };
        StateChanged += (_, _) => WindowChrome.SetWindowChrome(this,
            WindowState == WindowState.Normal ? restoredChrome : null);
        WindowState = WindowState.Maximized;

        Resources.MergedDictionaries.Add(spaceTheme);
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

        var lightOutline = (Brush)spaceTheme[ThemeKeys.Light];
        var lightFill = (Brush)spaceTheme[ThemeKeys.LightWash];
        minimap = new MinimapWindow(lightFill, lightOutline);
        spaceView = new SpaceView(shell, folders.Keys, lightFill, lightOutline);
        windowPanels = new WindowPanels(this, shell, () => spaceView.GetMapItems().Select(item => item.Bounds).ToArray(),
            TryReleaseUnavailablePanel);
        windowPanels.MapChanged += RefreshMap;
        windowPanels.MapChanged += UpdateMapPriority;
        windowPanels.PreviewMouseWheel += ZoomSurface;
        windowPanels.ActivationRequested += ActivatePanel;
        windowPanels.Added += ActivatePanel;
        windowPanels.PanelMoved += AlignDraggedWindow;
        windowPanels.FoldRequested += TogglePanelFold;
        windowPanels.MaximizeRequested += MaximizePanel;
        windowPanels.CloseRequested += ClosePanel;
        windowPanels.RemoveRequested += RemovePanel;
        previewTimer.Tick += (_, _) =>
        {
            if (windowPanels.ActivePanel is { } panel &&
                panel.CloseRequest is not { Completed: false } &&
                WindowCatalog.IsMinimized(panel.Window.Handle))
            {
                ReturnToMap();
            }

            var focused = windowPanels.Panels.FirstOrDefault(p => p.Presented && p.Space == session.ActiveSpaceId &&
                NativePanelInput.IsForeground((nint)p.Window.Handle) && CanUsePanel(p) && WindowCatalog.IsLive(p.Window.Identity, p.Instance));
            if (focused is not null && focused != windowPanels.ActivePanel)
            {
                windowPanels.ActivePanel = focused;
                edgeTimer.Start();
            }
            windowPanels.RefreshWindows();
            UpdateMapPriority();
        };
        edgeTimer.Tick += (_, _) =>
        {
            UpdateActivePanelBounds();
            UpdateMinimapVisibility();
        };
        homeButton = new Button { Style = DeskControls.ButtonStyle, MinWidth = 110 };
        homeButton.Click += (_, _) => ApplyNavigation(new ToggleHome());
        var toolbar = CreateToolbar();
        var edgeBar = new EdgeBar(this, toolbar);
        var viewport = new Grid { ClipToBounds = true };
        viewport.PreviewMouseDown += (_, e) =>
        {
            if (activationPending)
            {
                e.Handled = true;
                return;
            }

            if (windowPanels.ActivePanel is not null && e.ChangedButton == MouseButton.Left &&
                !windowPanels.IsPointerOverPanel)
            {
                e.Handled = true;
                if (edgeBar.IsMouseOver)
                {
                    e.Handled = false;
                    ReturnToMap(restoreCamera: false);
                }
                else if (gesture is null)
                {
                    ReturnToMap(restoreCamera: false);
                }
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
        Activated += (_, _) => UpdateMapPriority();
        Deactivated += (_, _) =>
        {
            // Read the new foreground after Windows completes the focus handoff.
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                if (!closed) UpdateMapPriority();
            }));
            cancelActivation = true;
            windowPanels.CancelDrag();
            CancelGesture();
        };
        PreviewKeyDown += (_, e) =>
        {
            if (activationPending && e.Key != Key.System)
            {
                cancelActivation |= e.Key == Key.Escape;
                e.Handled = true;
                return;
            }

            if (HandleZoomKey(e.Key))
            {
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && windowPanels.IsMouseCaptureWithin)
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
        viewport.Children.Add(spaceView);
        viewport.Children.Add(windowPanels);
        viewport.Children.Add(returnShortcutStatus);
        viewport.Children.Add(edgeBar);
        toolbar.IsVisibleChanged += (_, _) => Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)windowPanels.Render);
        Content = viewport;
        RefreshSpace();

        minimap.CenterRequested += SelectMapPoint;
        minimap.ZoomRequested += StepZoom;
        minimap.FitRequested += FitContent;
        SourceInitialized += (_, _) =>
        {
            windowHandle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(windowHandle).AddHook(HandleWindowMessage);
            UpdateShellFullscreen();
            var tracePath = Environment.GetEnvironmentVariable("CANVAS_PROJECTION_TRACE");
            if (!string.IsNullOrWhiteSpace(tracePath)) StartProjectionTrace(tracePath);
        };
        ContentRendered += (_, _) =>
        {
            if (minimap.Owner is null)
            {
                minimap.Owner = this;
                SyncMinimapWindow();
            }
        };
        StateChanged += (_, _) =>
        {
            UpdateShellFullscreen();
            cancelActivation = true;
            ReturnToMap(activateCanvas: false);
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
        Closing += (_, e) =>
        {
            var released = true;
            foreach (var panel in windowPanels.GetUnavailablePanels(includePendingClose: true))
            {
                released &= TryReleaseUnavailablePanel(panel);
            }

            if (!released)
            {
                e.Cancel = true;
                MessageBox.Show(this, "Queda un cierre de ventana por confirmar o registrar. Espera un momento y vuelve a cerrar Canvas.",
                    Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        };
        Closed += (_, _) =>
        {
            closed = true;
            ReleasePanelWheel();
            folderTimer.Stop();
            previewTimer.Stop();
            edgeTimer.Stop();
            ReleaseReturnShortcut();
            windowPanels.Dispose();
            shell.Dispose();
            if (!readingFolders)
            {
                DisposeFolders();
            }
        };
    }

    private StackPanel CreateAppButtons()
    {
        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        AddButton("−", "Minimizar", () => SystemCommands.MinimizeWindow(this));
        var maximize = AddButton("", "", () =>
        {
            if (WindowState == WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(this);
            }
            else
            {
                SystemCommands.MaximizeWindow(this);
            }
        });
        AddButton("×", "Cerrar", () => SystemCommands.CloseWindow(this));
        UpdateMaximize();
        StateChanged += (_, _) => UpdateMaximize();
        return buttons;

        Button AddButton(string glyph, string label, Action action)
        {
            var button = new Button { Content = glyph, Style = DeskControls.ButtonStyle, Width = 32, Height = 28, FontSize = 16, Margin = new Thickness(4, 0, 0, 0), Padding = new Thickness(0), ToolTip = label };
            AutomationProperties.SetName(button, label);
            button.Click += (_, _) => action();
            buttons.Children.Add(button);
            return button;
        }

        void UpdateMaximize()
        {
            var maximized = WindowState == WindowState.Maximized;
            var label = maximized ? "Restaurar tamaño" : "Maximizar";
            maximize.Content = maximized ? "❐" : "□";
            maximize.ToolTip = label;
            AutomationProperties.SetName(maximize, label);
        }
    }

    private void UpdateActivePanelBounds(bool syncPosition = true)
    {
        if (windowPanels.ActivePanel is { } panel && panel.Space == session.ActiveSpaceId &&
            WindowCatalog.IsLive(panel.Window.Identity, panel.Instance) &&
            placement.ReadNormalScreenBounds(panel.Window.Handle) is { } bounds)
        {
            windowPanels.SyncActiveBounds(panel, bounds, syncPosition);
        }
    }

    private DomainPoint ViewportSize => new(spaceView.ActualWidth, spaceView.ActualHeight);
    private Camera MapZoomCamera => ActiveCamera;

    private void SyncMinimapWindow()
    {
        if (minimap.Owner is null || closed)
        {
            return;
        }

        if (WindowState == WindowState.Minimized)
        {
            minimap.Hide();
            return;
        }

        minimap.AnchorTo(spaceView);
        UpdateMinimapVisibility();
    }

    private void UpdateMinimapVisibility()
    {
        if (closed || minimap.Owner is null)
        {
            return;
        }

        var panel = windowPanels.ActivePanel;
        var canvasInFront = NativePanelInput.IsForeground(windowHandle);
        var panelInFront = panel is not null && NativePanelInput.IsForeground((nint)panel.Window.Handle)
            && WindowCatalog.IsLive(panel.Window.Identity, panel.Instance);
        if (WindowState == WindowState.Minimized || !(canvasInFront || panelInFront) ||
            panel is not null && !PointerRevealsMinimap())
        {
            minimap.Hide();
            return;
        }

        if (!minimap.IsVisible)
        {
            minimap.Show();
        }
        minimap.EnsureTopmost();
    }

    private void RefreshMap()
    {
        var size = ViewportSize;
        var camera = ActiveCamera;
        MinimapItem[] items = [.. spaceView.GetMapItems(includePreview: true), .. windowPanels.GetMapItems()];
        if (size.X > 0 && size.Y > 0)
        {
            minimap.UpdateMap(CanvasBounds.Viewport(camera, size), MapZoomCamera.Zoom, items);
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
            minimap.IsVisible && relative.Y >= bottom - MinimapWindow.IslandHeight - MinimapWindow.Inset &&
                relative.X >= right - MinimapWindow.IslandWidth - MinimapWindow.Inset && relative.X <= right;
    }

    private nint HandleWindowMessage(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (message is 0x21 or 0x201 or 0x202 or 0x06) sampleProjection?.Invoke($"wm-{message:x}");
        if (message == NativeCanvasFrame.GetMinMaxInfo)
        {
            handled = NativeCanvasFrame.MaximizeToMonitor(hwnd, lParam);
        }

        const int EnterSizeMove = 0x0231;
        if (message == EnterSizeMove)
        {
            ReturnToMap(activateCanvas: false);
        }

        if (message == NativePanelInput.HotkeyMessage && wParam == NativePanelInput.ReturnHotkey &&
            hotkeyRegistered && windowPanels.ActivePanel is not null)
        {
            handled = true;
            cancelActivation = true;
            ReturnToMap();
        }

        return 0;
    }

    private async void ActivatePanel(WindowPanels.Panel panel) => await ActivatePanelAsync(panel, fromMinimap: false);

    private bool CanUsePanel(WindowPanels.Panel panel) =>
        !closed && !activationPending && !panel.WaitingForRemoval && panel.CloseRequest?.Accepted != true &&
        panel.Space == session.ActiveSpaceId && windowPanels.Contains(panel);

    private void TogglePanelFold(WindowPanels.Panel panel)
    {
        if (!CanUsePanel(panel))
        {
            return;
        }

        if (WindowCatalog.IsMinimized(panel.Window.Handle))
        {
            ActivatePanel(panel);
            return;
        }

        if (windowPanels.ActivePanel == panel)
        {
            ReturnToMap();
        }

        panel.IsFolded = !panel.IsFolded;
        windowPanels.Render();
        StackNativeWindows();
    }

    private async void MaximizePanel(WindowPanels.Panel panel) =>
        await UseActivePanelAsync(panel, () =>
        {
            placement.Maximize(panel.Window.Identity, panel.Instance);
            SetPanelPresentation((nint)panel.Window.Handle, projected: false);
            UpdateMapPriority();
            windowPanels.Render();
            if (!NativePanelInput.KeepCanvasBehind(windowHandle, (nint)panel.Window.Handle))
                throw new Win32Exception();
        }, "No se pudo maximizar la ventana.");

    private async void ClosePanel(WindowPanels.Panel panel) =>
        await UseActivePanelAsync(panel, () =>
        {
            if (panel.CloseRequest is { Completed: false } or { Accepted: true })
            {
                return;
            }

            NativeWindowPlacement.EnsureCurrent(panel.Instance);
            var source = (nint)panel.Window.Handle;
            if (!NativePanelInput.IsForeground(source) && !NativePanelInput.SetForegroundWindow(source))
            {
                throw new InvalidOperationException("Windows denied foreground activation.");
            }

            panel.CloseRequest = placement.RequestClose(panel.Window.Identity, panel.Instance);
        }, "No se pudo solicitar el cierre de la ventana.");

    private async Task UseActivePanelAsync(WindowPanels.Panel panel, Action action, string errorMessage)
    {
        if (!CanUsePanel(panel))
        {
            return;
        }

        await ActivatePanelAsync(panel, fromMinimap: false);
        if (!CanUsePanel(panel) || windowPanels.ActivePanel != panel)
        {
            return;
        }

        try
        {
            action();
        }
        catch (Exception error) when (IsPanelError(error))
        {
            Trace.TraceWarning(error.ToString());
            MessageBox.Show(this, errorMessage, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void RemovePanel(WindowPanels.Panel panel)
    {
        if (!CanUsePanel(panel))
        {
            return;
        }

        if (windowPanels.ActivePanel == panel)
        {
            ReturnToMap();
        }

        if (RestorePanel(panel))
        {
            windowPanels.Remove(panel);
        }
        else
        {
            MessageBox.Show(this, "No se pudo devolver la ventana a su acomodo original.", Title,
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private bool TryReleaseUnavailablePanel(WindowPanels.Panel panel)
    {
        try
        {
            if (panel.CloseRequest is { Completed: false } && panel.Instance.IsCurrent())
            {
                return false;
            }

            if (windowPanels.ActivePanel == panel)
            {
                ReturnToMap();
            }

            if (panel.CloseRequest is { Accepted: true })
            {
                recovery.ForgetWindow(panel.Window.Identity);
            }
            return true;
        }
        catch (Exception error) when (IsPanelError(error))
        {
            Trace.TraceWarning(error.ToString());
            return false;
        }
    }

    private async Task ActivatePanelAsync(WindowPanels.Panel panel, bool fromMinimap)
    {
        if (!CanUsePanel(panel))
        {
            return;
        }

        if (panel == windowPanels.ActivePanel)
        {
            if (fromMinimap)
            {
                PanTo(CameraGestures.CenterOn(panel.Bounds.Center, ViewportSize, ActiveCamera.Zoom));
            }

            return;
        }

        var previous = ActiveCamera;
        ReturnToMap(restoreCamera: false);

        CancelGesture();
        activationPending = true;
        cancelActivation = false;
        var identity = panel.Window.Identity;
        var saved = false;
        try
        {
            var start = ActiveCamera;
            NativeWindowPlacement.EnsureCurrent(panel.Instance);
            var bounds = panel.Bounds;
            if (fromMinimap && (!await AnimateActivationAsync(start,
                CameraGestures.CenterOn(bounds.Center, ViewportSize, 1), bounds.Center) ||
                panel.Space != session.ActiveSpaceId))
            {
                if (!closed && panel.Space == session.ActiveSpaceId)
                {
                    ApplyNavigation(new SetCamera(previous));
                }

                return;
            }

            NativeWindowPlacement.EnsureCurrent(panel.Instance);
            recovery.SaveOriginal(identity.Handle, panel.Instance);
            saved = true;
            mapCamera = previous;
            panel.LastNativeSize = null;
            windowPanels.ActivePanel = panel;
            UpdateMapPriority();
            placement.EnsureNormal(identity, panel.Instance);
            var origin = windowPanels.GetWindowOrigin(panel, ActiveCamera);
            placement.Reposition(identity, panel.Instance, (int)Math.Round(origin.X), (int)Math.Round(origin.Y));
            panel.Presented = true;
            NativeWindowPlacement.EnsureCurrent(panel.Instance);
            var application = (nint)identity.Handle;
            SetPanelPresentation(application, false);
            if (!NativePanelInput.SetForegroundWindow(application))
            {
                throw new InvalidOperationException("Windows denied foreground activation.");
            }

            NativeWindowPlacement.EnsureCurrent(panel.Instance);
            if (!NativePanelInput.KeepCanvasBehind(windowHandle, application))
            {
                throw new Win32Exception();
            }

            panel.IsFolded = false;
            
            UpdateMapPriority();
            UpdateActivePanelBounds(syncPosition: false);
            windowPanels.Render();
            RefreshMap();
            minimap.Hide();
            hotkeyRegistered = NativePanelInput.RegisterReturn(windowHandle);
            returnShortcutStatus.Visibility = hotkeyRegistered ? Visibility.Collapsed : Visibility.Visible;
            ListenForPanelZoom();
            edgeTimer.Start();
        }
        catch (Exception error) when (IsPanelError(error))
        {
            Trace.TraceWarning(error.ToString());
            ReleaseReturnShortcut();
            windowPanels.ActivePanel = null;
            panel.Presented = false;
            var restored = !saved || RestorePanel(panel);
            UpdateMapPriority();
            ApplyNavigation(new SetCamera(previous));
            MessageBox.Show(this, restored ? "No se pudo activar la ventana en su panel." :
                "No se pudo devolver la ventana. Cierra Escritorio Canvas y ejecuta «Devolver ventanas».",
                Title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            activationPending = false;
        }
    }

    private async Task<bool> AnimateActivationAsync(Camera start, Camera target, DomainPoint anchor)
    {
        if (start != target && SystemParameters.ClientAreaAnimation)
        {
            var elapsed = Stopwatch.StartNew();
            while (true)
            {
                await Task.Delay(16);
                if (closed || cancelActivation)
                {
                    return false;
                }

                var time = elapsed.Elapsed.TotalMilliseconds / ActivationDurationMs;
                if (time >= 1)
                {
                    break;
                }

                var progress = time * time * (3 - 2 * time);
                ApplyNavigation(new SetCamera(CameraGestures.InterpolateAround(start, target, anchor, progress)));
            }
        }

        ApplyNavigation(new SetCamera(target));
        return true;
    }

    private void ReturnToMap(bool activateCanvas = true, bool restoreCamera = true)
    {
        if (windowPanels.ActivePanel is not { } panel)
        {
            return;
        }

        ReleaseReturnShortcut();
        UpdateActivePanelBounds(syncPosition: activateCanvas);
        FinishGesture();
        windowPanels.ActivePanel = null;
        edgeTimer.Stop();

        var restoreFailed = false;
        try
        {
            if (WindowCatalog.IsLive(panel.Window.Identity, panel.Instance))
            {
                NativeWindowPlacement.ClearPanelFullscreen((nint)panel.Window.Handle);
                if (panel.CloseRequest?.Accepted != true)
                {
                    placement.RestoreMaximizedPanel(panel.Window.Identity, panel.Instance);
                }
            }
        }
        catch (Exception error) when (IsPanelError(error))
        {
            Trace.TraceWarning(error.ToString());
            restoreFailed = true;
        }

        // Original placements stay in the recovery journal until the windows are released.
        if (activateCanvas)
        {
            NativePanelInput.RaiseWindow(windowHandle);
            Activate();
            if (WindowState == WindowState.Maximized && NativePanelInput.IsForeground(windowHandle))
            {
                UpdateShellFullscreen();
            }
        }
        UpdateMapPriority();
        if (activateCanvas && Topmost)
        {
            NativePanelInput.RaiseWindow(windowHandle, topmost: true);
            minimap.EnsureTopmost();
        }

        if (restoreCamera)
        {
            ApplyNavigation(new SetCamera(mapCamera));
        }

        windowPanels.Render();
        RefreshMap();
        SyncMinimapWindow();
        if (restoreFailed)
        {
            MessageBox.Show(this, "Windows dejó la ventana maximizada. Puedes restaurar su tamaño desde la propia ventana.",
                Title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void UpdateMapPriority()
    {
        var hasNativeWindows = windowPanels.Panels.Any(p => p.Presented && p.Space == session.ActiveSpaceId && !p.IsFolded);
        Topmost = !hasNativeWindows && WindowState == WindowState.Maximized && NativePanelInput.IsForeground(windowHandle);
        StackNativeWindows();
        UpdateMinimapVisibility();
    }

    private void ReleaseReturnShortcut()
    {
        if (hotkeyRegistered)
        {
            NativePanelInput.UnregisterReturn(windowHandle);
            hotkeyRegistered = false;
        }

        returnShortcutStatus.Visibility = Visibility.Collapsed;
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

    private async void SelectMapPoint(DomainPoint world)
    {
        if (activationPending)
        {
            return;
        }

        if (windowPanels.FindPanel(world) is { } panel)
        {
            await ActivatePanelAsync(panel, fromMinimap: true);
        }
        else
        {
            ReturnToMap();
            CenterMap(world);
        }
    }

    private void CenterMap(DomainPoint world)
    {
        CancelGesture();
        ApplyNavigation(new SetCamera(CameraGestures.CenterOn(world, ViewportSize, ActiveCamera.Zoom)));
    }

    private bool HandleZoomKey(Key key)
    {
        var modifiers = Keyboard.Modifiers;
        if (modifiers != ModifierKeys.Control &&
            !(modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && key == Key.OemPlus))
        {
            return false;
        }

        switch (key)
        {
            case Key.OemMinus or Key.Subtract:
                StepZoom(-1);
                return true;
            case Key.OemPlus or Key.Add:
                StepZoom(1);
                return true;
            case Key.D0 or Key.NumPad0:
                FitContent();
                return true;
            default:
                return false;
        }
    }

    private void StepZoom(double steps)
    {
        var camera = MapZoomCamera;
        if (activationPending ||
            (steps < 0 && camera.Zoom <= CameraGestures.MinZoom) ||
            (steps > 0 && camera.Zoom >= CameraGestures.MaxZoom))
        {
            return;
        }

        ReturnToMap();
        CancelGesture();
        var size = ViewportSize;
        var center = new DomainPoint(size.X / 2, size.Y / 2);
        ApplyNavigation(new SetCamera(CameraGestures.ZoomAt(camera, center, steps)));
    }

    private void FitContent()
    {
        if (activationPending)
        {
            return;
        }

        ReturnToMap();
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
        if (kind == GestureKind.Pan && windowPanels.ActivePanel is { } panel &&
            WindowCatalog.IsLive(panel.Window.Identity, panel.Instance))
        {
            NativePanelInput.KeepCanvasBehind(windowHandle, (nint)panel.Window.Handle);
        }

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
                PanTo(CameraGestures.Pan(active.Camera, delta));
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
        if (active.Kind == GestureKind.Pan && windowPanels.ActivePanel is { } panel &&
            WindowCatalog.IsLive(panel.Window.Identity, panel.Instance) && !WindowCatalog.IsMinimized(panel.Window.Handle))
        {
            NativePanelInput.SetForegroundWindow((nint)panel.Window.Handle);
            UpdateMapPriority();
        }

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
            PanTo(active.Camera);
        }
    }

    private void AlignDraggedWindow(WindowPanels.Panel panel)
    {
        if (!panel.Presented || WindowCatalog.IsMinimized(panel.Window.Handle) || placement.IsMaximized(panel.Window.Handle) ||
            !WindowCatalog.IsLive(panel.Window.Identity, panel.Instance)) return;
        try
        {
            var origin = windowPanels.GetWindowOrigin(panel, ActiveCamera);
            placement.Reposition(panel.Window.Identity, panel.Instance, (int)Math.Round(origin.X), (int)Math.Round(origin.Y));
        }
        catch (Exception error) when (IsPanelError(error))
        {
            Trace.TraceWarning(error.ToString());
            ReturnToMap(restoreCamera: false);
        }
    }

    private void PanTo(Camera camera)
    {
        if (camera == ActiveCamera)
        {
            return;
        }

        ApplyNavigation(new SetCamera(camera));
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
        if (windowPanels.ActivePanel is { } panel && !activationPending &&
            Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
            !NativeCanvasFrame.HasFullscreenClient((nint)panel.Window.Handle))
        {
            ZoomFromPanel(spaceView.GetPointer(e), e.Delta);
            return;
        }
        if (gesture is null && windowPanels.ActivePanel is null && !activationPending)
        {
            var cursor = spaceView.GetPointer(e);
            ApplyNavigation(new SetCamera(CameraGestures.ZoomAt(ActiveCamera, cursor, e.Delta / 120.0)));
        }
    }

    private void ReleasePanelWheel()
    {
        panelWheel?.Dispose();
        panelWheel = null;
    }

    private void ListenForPanelZoom()
    {
        panelWheel ??= new PanelWheel(
            window => windowPanels.Panels.Any(p => p.Presented && !p.IsFolded && !p.WaitingForRemoval &&
                p.Space == session.ActiveSpaceId && p.Window.Handle == (long)window && p.Instance.IsCurrent()),
            (window, point, delta) => Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                if (closed || activationPending || !NativePanelInput.IsForeground(window)) return;
                var cursor = spaceView.PointFromScreen(point);
                ZoomFromPanel(new DomainPoint(cursor.X, cursor.Y), delta);
            })));
    }

    private void ZoomFromPanel(DomainPoint cursor, int delta)
    {
        ApplyNavigation(new SetCamera(CameraGestures.ZoomAt(ActiveCamera, cursor, delta / 120.0)));
    }

    private void StackNativeWindows()
    {
        var foreground = NativePanelInput.ForegroundWindow;
        if (windowHandle == 0 || WindowState == WindowState.Minimized ||
            (foreground != windowHandle && !windowPanels.Panels.Any(p => p.Presented && p.Window.Handle == (long)foreground && p.Space == session.ActiveSpaceId))) return;
        foreach (var panel in windowPanels.Panels)
        {
            if (!panel.Presented || !WindowCatalog.IsLive(panel.Window.Identity, panel.Instance) ||
                WindowCatalog.IsMinimized(panel.Window.Handle)) continue;
            NativePanelInput.StackAtCanvas((nint)panel.Window.Handle, windowHandle,
                panel.Space == session.ActiveSpaceId && !panel.IsFolded);
        }
    }

    private void StartProjectionTrace(string path)
    {
        StreamWriter log;
        try { log = new StreamWriter(new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read)); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Trace.TraceWarning(ex.Message);
            return;
        }

        var clock = Stopwatch.StartNew();
        string? last = null;
        void Sample(string reason)
        {
            var source = (nint)(windowPanels.ActivePanel?.Window.Handle ?? 0);
            var camera = ActiveCamera;
            var state = FormattableString.Invariant($"{camera.Position.X:R},{camera.Position.Y:R},{camera.Zoom:R},False,{NativePanelInput.DescribeStack(windowHandle, source)}");
            if (reason == "frame" && state == last) return;
            last = state;
            try { log.WriteLine($"{clock.ElapsedMilliseconds},{reason},{state}"); }
            catch (IOException) { CompositionTarget.Rendering -= OnFrame; sampleProjection = null; }
        }
        void OnFrame(object? sender, EventArgs args) => Sample("frame");
        log.WriteLine("ms,event,cameraX,cameraY,zoom,projected,foreground,visibleAtSourceCenter,left,top,right,bottom");
        sampleProjection = Sample;
        CompositionTarget.Rendering += OnFrame;
        Closed += (_, _) =>
        {
            CompositionTarget.Rendering -= OnFrame;
            sampleProjection = null;
            log.Dispose();
        };
    }

    private void SetPanelPresentation(nint source, bool projected)
    {
        // Mark only the directly displayed native window as fullscreen for the shell.
        // The projected source can receive keyboard focus while Canvas covers it.
        if (!projected && WindowState == WindowState.Maximized)
            NativeWindowPlacement.MarkPanelFullscreen(source);
        else
            NativeWindowPlacement.ClearPanelFullscreen(source);
    }

    private void UpdateShellFullscreen()
    {
        if (windowHandle == 0) return;
        try
        {
            ShellFullscreen.Set(windowHandle, WindowState == WindowState.Maximized);
        }
        catch (InvalidOperationException error)
        {
            Trace.TraceWarning(error.ToString());
        }
    }

    private Border CreateToolbar()
    {
        var controls = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var space in session.Spaces.Keys)
        {
            var button = AddAction(space.Value, () => ApplyNavigation(new SwitchSpace(space)));
            spaceButtons.Add(space, button);
        }

        homeButton.Margin = new Thickness(8, 0, 4, 0);
        controls.Children.Add(homeButton);
        AddAction("Carpeta", () => ShellItems.Open(folders[session.ActiveSpaceId].Path), "Abrir carpeta del espacio");
        AddAction("Añadir ventana", windowPanels.ShowPicker);
        controls.Children.Add(CreateAppButtons());

        Button AddAction(string label, Action action, string? tooltip = null)
        {
            var button = new Button
            {
                Style = DeskControls.ButtonStyle, Content = label,
                Margin = new Thickness(0, 0, 4, 0), ToolTip = tooltip
            };
            if (tooltip is not null)
            {
                AutomationProperties.SetName(button, tooltip);
            }

            button.Click += (_, _) => action();
            controls.Children.Add(button);
            return button;
        }

        return new Border
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(8, 10, 8, 0),
            Padding = new Thickness(5),
            CornerRadius = new CornerRadius(VisualTokens.Radius.Island),
            Background = DeskControls.Brush(VisualTokens.Colors.Slate),
            BorderBrush = DeskControls.Brush(VisualTokens.Colors.Border),
            BorderThickness = new Thickness(1),
            Child = controls
        };
    }

    private void ApplyNavigation(NavigationCommand command)
    {
        var switchingSpace = command is SwitchSpace change && change.Target != session.ActiveSpaceId;
        if (switchingSpace)
        {
            ReturnToMap(restoreCamera: false);
            cancelActivation = true;
        }

        if (command is ToggleHome || switchingSpace)
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
        foreach (var panel in windowPanels.Panels.Where(p => p.Space == session.ActiveSpaceId && !p.IsFolded).ToArray())
            AlignDraggedWindow(panel);
        StackNativeWindows();
    }

    private enum GestureKind { Pan, Marquee, Icons }

    private sealed record Gesture(MouseButton Button, DomainPoint Pointer, Camera Camera,
        GestureKind Kind, string? Icon, string[] Selection, bool OpenOnRelease)
    {
        public bool HasMoved { get; set; }
    }

    private void RefreshSpace()
    {
        var activeSpace = session.ActiveSpaceId;
        var navigation = session.Spaces[activeSpace];
        if (displayedSpace != activeSpace)
        {
            displayedSpace = activeSpace;
            var light = activeSpace == Games ? VisualTokens.FindLight("ember") :
                activeSpace == Programming ? VisualTokens.DefaultLight : VisualTokens.FindLight("lavender");
            Theme.SetLight(spaceTheme, light);
        }

        foreach (var (space, button) in spaceButtons)
        {
            var selected = space == activeSpace;
            if (selected)
            {
                button.SetResourceReference(BackgroundProperty, ThemeKeys.LightWash);
            }
            else
            {
                button.Background = InactiveSpaceBrush;
            }

            AutomationProperties.SetItemStatus(button, selected ? "Espacio actual" : "");
        }
        windowPanels.ShowSpace(activeSpace, navigation.Camera);
        spaceView.ShowSpace(activeSpace, navigation.Camera);
        var homeLabel = navigation.ReturnView.HasValue ? "Volver a mi vista" : "Inicio";
        homeButton.Content = homeLabel;
        AutomationProperties.SetName(homeButton, homeLabel);
    }

}
