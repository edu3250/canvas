using Canvas.Domain;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DomainPoint = Canvas.Domain.Point;
using DomainGeometry = Canvas.Domain.Geometry;
using SurfaceCanvas = System.Windows.Controls.Canvas;

namespace Canvas.Desktop;

internal sealed class SpaceView : SurfaceCanvas
{
    private const double TileSize = 88;
    private const int GridSpacing = 96;
    private static readonly DomainPoint FirstCell = HomeGrid.Clamp(new DomainPoint(double.MinValue, double.MinValue));
    private static readonly DomainPoint LastCell = HomeGrid.Clamp(new DomainPoint(double.MaxValue, double.MaxValue));
    private static readonly Brush SelectionFill = new SolidColorBrush(Color.FromArgb(85, 74, 156, 245));
    private static readonly Brush SelectionBorder = new SolidColorBrush(Color.FromRgb(160, 212, 255));
    private readonly Dictionary<SpaceId, Dictionary<string, IconPlacement>> layouts;
    private readonly HashSet<string> selected = [];
    private readonly Border marquee = new()
    {
        Background = SelectionFill,
        BorderBrush = SelectionBorder,
        BorderThickness = new Thickness(1),
        IsHitTestVisible = false,
        Visibility = Visibility.Collapsed
    };
    private SpaceId? displayedSpace;
    private Camera camera;
    private DomainPoint previewOffset;

    public SpaceView(SpaceId programming, SpaceId games)
    {
        Background = Brushes.Transparent;
        ClipToBounds = true;
        layouts = new()
        {
            [programming] = new()
            {
                ["vscode"] = CreatePlacement("VS Code", "‹›", Color.FromRgb(45, 93, 128), 80),
                ["codex"] = CreatePlacement("Codex", "✧", Color.FromRgb(72, 75, 130), 176)
            },
            [games] = new()
            {
                ["steam"] = CreatePlacement("Steam", "◉", Color.FromRgb(45, 80, 107), 80),
                ["discord"] = CreatePlacement("Discord", "◇", Color.FromRgb(94, 73, 133), 176)
            }
        };
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
            selected.Clear();
            Children.Clear();
            foreach (var (id, placement) in icons)
            {
                placement.Tile.Id = id;
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
            Math.Clamp(requested.X, FirstCell.X - group.Min(point => point.X), LastCell.X - group.Max(point => point.X)),
            Math.Clamp(requested.Y, FirstCell.Y - group.Min(point => point.Y), LastCell.Y - group.Max(point => point.Y)));
        RenderIcons();
    }

    public void CommitMove(string grabbed)
    {
        var icons = ActiveIcons;
        var occupied = icons.Where(entry => !selected.Contains(entry.Key))
            .Select(entry => entry.Value.Origin).ToHashSet();
        var origin = icons[grabbed].Origin;
        if (selected.Count == 1)
        {
            icons[grabbed].Origin = HomeGrid.Snap(Translate(origin, previewOffset), occupied);
            return;
        }

        var group = selected.Select(id => icons[id]).ToArray();
        var best = new DomainPoint(0, 0);
        var bestDistance = double.PositiveInfinity;
        for (var y = FirstCell.Y; y <= LastCell.Y; y += GridSpacing)
        {
            for (var x = FirstCell.X; x <= LastCell.X; x += GridSpacing)
            {
                var offset = new DomainPoint(x - origin.X, y - origin.Y);
                var dx = offset.X - previewOffset.X;
                var dy = offset.Y - previewOffset.Y;
                var distance = dx * dx + dy * dy;
                if (distance < bestDistance && group.All(icon => CanPlace(Translate(icon.Origin, offset), occupied)))
                {
                    best = offset;
                    bestDistance = distance;
                }
            }
        }

        foreach (var icon in group)
        {
            icon.Origin = Translate(icon.Origin, best);
        }
    }

    public void ClearPreview()
    {
        marquee.Visibility = Visibility.Collapsed;
        previewOffset = default;
        RenderIcons();
    }

    private static bool CanPlace(DomainPoint point, HashSet<DomainPoint> occupied) =>
        HomeGrid.Clamp(point) == point && !occupied.Contains(point);

    private static DomainPoint Translate(DomainPoint origin, DomainPoint offset) =>
        new(origin.X + offset.X, origin.Y + offset.Y);

    private void RenderIcons()
    {
        var scale = new ScaleTransform(camera.Zoom, camera.Zoom);
        foreach (var (id, placement) in ActiveIcons)
        {
            var isSelected = selected.Contains(id);
            var origin = isSelected ? Translate(placement.Origin, previewOffset) : placement.Origin;
            var projected = DomainGeometry.WorldToView(origin, camera);
            SetLeft(placement.Tile, projected.X);
            SetTop(placement.Tile, projected.Y);
            placement.Tile.RenderTransform = scale;
            placement.Tile.Background = isSelected ? SelectionFill : Brushes.Transparent;
            placement.Tile.BorderBrush = isSelected ? SelectionBorder : Brushes.Transparent;
        }
    }

    private static IconPlacement CreatePlacement(string name, string symbol, Color color, double y)
    {
        var badge = new Border
        {
            Width = 44,
            Height = 44,
            CornerRadius = new CornerRadius(11),
            Background = new SolidColorBrush(color),
            Child = new TextBlock
            {
                Text = symbol,
                FontFamily = new FontFamily("Segoe UI Symbol"),
                FontSize = 24,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        var label = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
            Padding = new Thickness(6, 2, 6, 2),
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(Color.FromRgb(24, 51, 65)),
            Child = new TextBlock { Text = name, FontSize = 12, Foreground = Brushes.White }
        };
        var content = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        content.Children.Add(badge);
        content.Children.Add(label);
        var tile = new IconTile
        {
            Width = TileSize,
            Height = TileSize,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.Transparent,
            CornerRadius = new CornerRadius(6),
            Child = content
        };
        AutomationProperties.SetName(tile, name);
        return new IconPlacement(new DomainPoint(24, y), tile);
    }

    private sealed class IconPlacement(DomainPoint origin, IconTile tile)
    {
        public DomainPoint Origin { get; set; } = origin;
        public IconTile Tile { get; } = tile;
    }

    private sealed class IconTile : Border
    {
        public string Id { get; set; } = "";
        protected override AutomationPeer OnCreateAutomationPeer() => new IconTilePeer(this);
    }

    private sealed class IconTilePeer(IconTile tile) : FrameworkElementAutomationPeer(tile)
    {
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Text;
        protected override List<AutomationPeer>? GetChildrenCore() => null;
    }
}
