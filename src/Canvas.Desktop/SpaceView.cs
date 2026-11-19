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
    private readonly Dictionary<SpaceId, Dictionary<string, IconPlacement>> layouts;
    private SpaceId? displayedSpace;

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

    public DomainPoint GetPointer(MouseEventArgs input)
    {
        var point = input.GetPosition(this);
        return new DomainPoint(point.X, point.Y);
    }

    public void ShowSpace(SpaceId space, Camera camera)
    {
        var icons = layouts[space].Values;
        if (displayedSpace != space)
        {
            Children.Clear();
            foreach (var placement in icons)
            {
                Children.Add(placement.Tile);
            }

            displayedSpace = space;
        }

        var scale = new ScaleTransform(camera.Zoom, camera.Zoom);
        foreach (var placement in icons)
        {
            var projected = DomainGeometry.WorldToView(placement.Origin, camera);
            SetLeft(placement.Tile, projected.X);
            SetTop(placement.Tile, projected.Y);
            placement.Tile.RenderTransform = scale;
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
            Child = content
        };
        AutomationProperties.SetName(tile, name);
        return new IconPlacement(new DomainPoint(24, y), tile);
    }

    private sealed record IconPlacement(DomainPoint Origin, Border Tile);

    private sealed class IconTile : Border
    {
        protected override AutomationPeer OnCreateAutomationPeer() => new IconTilePeer(this);
    }

    private sealed class IconTilePeer(IconTile tile) : FrameworkElementAutomationPeer(tile)
    {
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Text;
        protected override List<AutomationPeer>? GetChildrenCore() => null;
    }
}
