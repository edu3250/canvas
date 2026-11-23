using Canvas.Domain;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using DomainPoint = Canvas.Domain.Point;
using DomainGeometry = Canvas.Domain.Geometry;
using SurfaceCanvas = System.Windows.Controls.Canvas;

namespace Canvas.Desktop;

internal sealed class SpaceView : SurfaceCanvas
{
    private const double TileSize = 88;
    private const double IconSize = 64;
    private readonly Brush selectionFill;
    private readonly Brush selectionOutline;
    private readonly Dictionary<SpaceId, Dictionary<string, IconPlacement>> layouts;
    private readonly ShellItems shell;
    private readonly HashSet<string> selected = [];
    private readonly Border marquee = new()
    {
        BorderThickness = new Thickness(1),
        IsHitTestVisible = false,
        Visibility = Visibility.Collapsed
    };
    private SpaceId? displayedSpace;
    private Camera camera;
    private DomainPoint previewOffset;

    public SpaceView(ShellItems shell, IEnumerable<SpaceId> spaces, Brush selectionFill, Brush selectionOutline)
    {
        this.shell = shell;
        this.selectionFill = marquee.Background = selectionFill;
        this.selectionOutline = marquee.BorderBrush = selectionOutline;
        Background = Brushes.Transparent;
        ClipToBounds = true;
        Focusable = true;
        layouts = spaces.ToDictionary(space => space, _ => new Dictionary<string, IconPlacement>());
        SizeChanged += (_, _) => RenderIcons();
    }

    public event Action? MapChanged;

    public MinimapItem[] GetMapItems(bool includePreview = false) =>
        ActiveIcons.Select(entry =>
        {
            var placement = entry.Value;
            var origin = includePreview && selected.Contains(entry.Key)
                ? Translate(placement.Origin, previewOffset)
                : placement.Origin;
            return new MinimapItem(new CanvasBounds(origin.X, origin.Y, TileSize, TileSize), placement.Tile.Icon.Source);
        }).ToArray();

    public bool HasChanges(SpaceId space, string[] paths) =>
        paths.Length != layouts[space].Count || paths.Any(path => !layouts[space].ContainsKey(path));

    public Dictionary<string, DomainPoint> GetLayout(SpaceId space) =>
        layouts[space].ToDictionary(entry => Path.GetFileName(entry.Key), entry => entry.Value.Origin,
            StringComparer.OrdinalIgnoreCase);

    public void UpdateItems(SpaceId space, string[] paths, (string OldPath, string NewPath)[] renames,
        IReadOnlyDictionary<string, DomainPoint> saved)
    {
        var icons = layouts[space];
        var visible = displayedSpace == space;
        var snapshot = paths.ToHashSet();
        foreach (var (oldPath, newPath) in renames)
        {
            if (!icons.Remove(oldPath, out var placement))
            {
                continue;
            }

            RemoveTile(placement, visible);
            if (icons.Remove(newPath, out var replaced))
            {
                RemoveTile(replaced, visible);
                if (visible)
                {
                    selected.Remove(newPath);
                }
            }

            var replacement = CreatePlacement(newPath, placement.Origin);
            icons[newPath] = replacement;
            if (visible)
            {
                if (selected.Remove(oldPath))
                {
                    selected.Add(newPath);
                }

                Children.Insert(Children.Count - 1, replacement.Tile);
            }
        }

        foreach (var path in icons.Keys.Where(path => !snapshot.Contains(path)).ToArray())
        {
            RemoveTile(icons[path], visible);
            icons.Remove(path);
            if (visible)
            {
                selected.Remove(path);
            }
        }

        var occupied = icons.Values.Select(placement => placement.Origin).ToHashSet();
        var added = paths.Where(path => !icons.ContainsKey(path)).ToArray();
        var cells = HomeLayout.Place(occupied, added.Select(Path.GetFileName).ToArray()!, saved);
        foreach (var path in added)
        {
            var placement = CreatePlacement(path, cells[Path.GetFileName(path)]);
            icons.Add(path, placement);
            if (visible)
            {
                Children.Insert(Children.Count - 1, placement.Tile);
            }
        }

        if (visible)
        {
            RenderIcons();
        }
    }

    private void RemoveTile(IconPlacement placement, bool visible)
    {
        placement.CancelImage();
        if (visible)
        {
            Children.Remove(placement.Tile);
        }
    }

    public bool IsBackground(object source) => ReferenceEquals(source, this);

    public string? FindIcon(object source)
    {
        while (source is Visual visual && !ReferenceEquals(visual, this))
        {
            if (visual is IconTile tile)
            {
                return tile.Id;
            }

            source = VisualTreeHelper.GetParent(visual)!;
        }

        return null;
    }

    public DomainPoint GetPointer(MouseEventArgs input)
    {
        var point = input.GetPosition(this);
        return new DomainPoint(point.X, point.Y);
    }

    public void ShowSpace(SpaceId space, Camera camera)
    {
        var icons = layouts[space];
        if (displayedSpace != space)
        {
            if (displayedSpace is not null)
            {
                foreach (var placement in ActiveIcons.Values)
                {
                    placement.CancelImage();
                }
            }

            selected.Clear();
            Children.Clear();
            foreach (var placement in icons.Values)
            {
                Children.Add(placement.Tile);
            }

            Children.Add(marquee);
            displayedSpace = space;
        }

        this.camera = camera;
        RenderIcons();
    }

    private Dictionary<string, IconPlacement> ActiveIcons => layouts[displayedSpace!.Value];

    public string[] GetSelection() => [.. selected];

    public void SetSelection(IEnumerable<string> ids)
    {
        selected.Clear();
        selected.UnionWith(ids);
        RenderIcons();
    }

    public void ShowMarquee(DomainPoint start, DomainPoint end)
    {
        var rect = new Rect(start.X, start.Y, 0, 0);
        rect.Union(new System.Windows.Point(end.X, end.Y));
        marquee.Width = rect.Width;
        marquee.Height = rect.Height;
        SetLeft(marquee, rect.Left);
        SetTop(marquee, rect.Top);
        marquee.Visibility = Visibility.Visible;
        var size = TileSize * camera.Zoom;
        SetSelection(ActiveIcons.Where(entry =>
        {
            var topLeft = DomainGeometry.WorldToView(entry.Value.Origin, camera);
            return rect.IntersectsWith(new Rect(topLeft.X, topLeft.Y, size, size));
        }).Select(entry => entry.Key));
    }

    public void PreviewMove(DomainPoint requested)
    {
        var group = selected.Select(id => ActiveIcons[id].Origin).ToArray();
        previewOffset = new DomainPoint(
            Math.Clamp(requested.X, HomeGrid.FirstX - group.Min(point => point.X), HomeGrid.LastX - group.Max(point => point.X)),
            Math.Clamp(requested.Y, HomeGrid.FirstY - group.Min(point => point.Y), HomeGrid.LastY - group.Max(point => point.Y)));
        RenderIcons();
    }

    public void CommitMove(string grabbed)
    {
        var icons = ActiveIcons;
        var occupied = icons.Where(entry => !selected.Contains(entry.Key))
            .Select(entry => entry.Value.Origin).ToHashSet();
        var origin = icons[grabbed].Origin;
        var group = selected.Select(id => icons[id]).ToArray();
        var offsets = group.Select(icon => new DomainPoint(icon.Origin.X - origin.X, icon.Origin.Y - origin.Y)).ToArray();
        var destination = HomeGrid.SnapGroup(Translate(origin, previewOffset), offsets, occupied);
        for (var index = 0; index < group.Length; index++)
        {
            group[index].Origin = Translate(destination, offsets[index]);
        }
    }

    public void ClearPreview()
    {
        marquee.Visibility = Visibility.Collapsed;
        previewOffset = default;
        RenderIcons();
    }

    private static DomainPoint Translate(DomainPoint origin, DomainPoint offset) =>
        new(origin.X + offset.X, origin.Y + offset.Y);

    private void RenderIcons()
    {
        if (displayedSpace is null)
        {
            return;
        }

        var scale = new ScaleTransform(camera.Zoom, camera.Zoom);
        var tileSize = TileSize * camera.Zoom;
        var pixels = ShellItems.LargeIconPixels;
        var requiredPixels = IconSize * camera.Zoom * VisualTreeHelper.GetDpi(this).DpiScaleX;
        while (pixels < requiredPixels && pixels < 768)
        {
            pixels *= 2;
        }
        foreach (var (id, placement) in ActiveIcons)
        {
            var isSelected = selected.Contains(id);
            var origin = isSelected ? Translate(placement.Origin, previewOffset) : placement.Origin;
            var projected = DomainGeometry.WorldToView(origin, camera);
            SetLeft(placement.Tile, projected.X);
            SetTop(placement.Tile, projected.Y);
            placement.Tile.RenderTransform = scale;
            placement.Tile.Background = isSelected ? selectionFill : Brushes.Transparent;
            placement.Tile.BorderBrush = isSelected ? selectionOutline : Brushes.Transparent;

            if (projected.X < ActualWidth && projected.Y < ActualHeight &&
                projected.X + tileSize > 0 && projected.Y + tileSize > 0)
            {
                if (!placement.NameRequested)
                {
                    placement.NameRequested = true;
                    _ = LoadNameAsync(placement.Tile);
                }

                if (placement.RequestedPixels < pixels)
                {
                    placement.CancelImage();
                    placement.RequestedPixels = pixels;
                    placement.ImageRequest = new CancellationTokenSource();
                    _ = LoadImageAsync(placement, pixels, placement.ImageRequest.Token);
                }
            }
            else
            {
                placement.CancelImage();
            }
        }

        MapChanged?.Invoke();
    }

    private async Task LoadNameAsync(IconTile tile)
    {
        try
        {
            tile.SetName(await shell.ReadNameAsync(tile.Id));
        }
        catch (OperationCanceledException)
        {
            // The Shell dispatcher is shutting down.
        }
    }

    private async Task LoadImageAsync(IconPlacement placement, int pixels, CancellationToken cancellation)
    {
        try
        {
            var (image, overlay) = await shell.ReadImageAsync(placement.Tile.Id, pixels, cancellation);
            if (!cancellation.IsCancellationRequested)
            {
                placement.Tile.Icon.Source = image;
                placement.Tile.Overlay.Source = overlay;
                placement.LoadedPixels = pixels;
                MapChanged?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            // The icon left the view or its requested size changed.
        }
    }

    private static IconPlacement CreatePlacement(string path, DomainPoint origin)
    {
        var tile = new IconTile(path)
        {
            Width = TileSize,
            Height = TileSize,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.Transparent,
            CornerRadius = new CornerRadius(6)
        };
        return new IconPlacement(origin, tile);
    }

    private sealed class IconPlacement(DomainPoint origin, IconTile tile)
    {
        public DomainPoint Origin { get; set; } = origin;
        public IconTile Tile { get; } = tile;
        public bool NameRequested { get; set; }
        public int RequestedPixels { get; set; }
        public int LoadedPixels { get; set; }
        public CancellationTokenSource? ImageRequest { get; set; }

        public void CancelImage()
        {
            ImageRequest?.Cancel();
            ImageRequest?.Dispose();
            ImageRequest = null;
            RequestedPixels = LoadedPixels;
        }
    }

    private sealed class IconTile : Border
    {
        private readonly TextBlock label = new()
        {
            FontSize = VisualTokens.Type.Detail,
            FontFamily = Theme.Font,
            Foreground = DeskControls.Brush(VisualTokens.Colors.Bone),
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextAlignment = TextAlignment.Center
        };

        public IconTile(string id)
        {
            Id = id;
            var artwork = new Grid { Width = IconSize, Height = IconSize };
            artwork.Children.Add(Icon);
            artwork.Children.Add(Overlay);
            var content = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            content.Children.Add(artwork);
            content.Children.Add(new Border
            {
                Margin = new Thickness(2, 2, 2, 0),
                Padding = new Thickness(3, 1, 3, 1),
                CornerRadius = new CornerRadius(4),
                Background = DeskControls.Brush(VisualTokens.Colors.Slate),
                Child = label
            });
            Child = content;
            SetName(Path.GetFileName(id));
        }

        public string Id { get; }
        public Image Icon { get; } = new();
        public Image Overlay { get; } = new() { IsHitTestVisible = false };

        public void SetName(string name)
        {
            label.Text = name;
            ToolTip = name;
            AutomationProperties.SetName(this, name);
        }

        protected override AutomationPeer OnCreateAutomationPeer() => new IconTilePeer(this);
    }

    private sealed class IconTilePeer(IconTile tile) : FrameworkElementAutomationPeer(tile)
    {
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Text;
        protected override List<AutomationPeer>? GetChildrenCore() => null;
    }
}
