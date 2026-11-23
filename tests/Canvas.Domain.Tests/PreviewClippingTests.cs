using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public sealed class PreviewClippingTests
{
    private static readonly CanvasBounds Viewport = new(0, 0, 1000, 700);
    private static readonly CanvasBounds[] Controls = [new(350, 12, 300, 48), new(620, 646, 210, 42)];

    [Theory]
    [InlineData(1920, 1080, 960, 540)]
    [InlineData(600, 1200, 360, 720)]
    [InlineData(960, 720, 960, 720)]
    public void FitsPreviewProportionallyAtThePanelOrigin(
        double sourceWidth, double sourceHeight, double width, double height)
    {
        var area = new CanvasBounds(12, 38, 960, 720);

        Assert.Equal(new CanvasBounds(12, 38, width, height),
            PreviewClipping.Fit(area, new Point(sourceWidth, sourceHeight)));
    }

    [Theory]
    [InlineData(0.3)]
    [InlineData(1)]
    [InlineData(3)]
    public void LocatesClickWithinTheVisibleSourceCrop(double zoom)
    {
        var crop = new CanvasBounds(120, 60, 800, 400);
        var target = new CanvasBounds(-50, 100, 800 * zoom, 400 * zoom);
        Assert.Equal(new Point(320, 260), PreviewClipping.MapToSource(target, crop,
            new Point(target.X + 200 * zoom, target.Y + 200 * zoom)));
        Assert.Null(PreviewClipping.MapToSource(target, crop, new Point(target.X + target.Width, target.Y)));
        Assert.Null(PreviewClipping.MapToSource(target, crop, new Point(target.X, target.Y - 1)));
    }

    [Fact]
    public void LeavesBottomLeftPreviewIntactAtSameHeightAsZoomControls()
    {
        var preview = new CanvasBounds(40, 480, 420, 220);
        Assert.Equal([preview], PreviewClipping.VisibleParts(preview, Viewport, Controls));
    }

    [Fact]
    public void ClipsOnlyTheToolbarIntersectionWhenPreviewCrossesTheTopEdge()
    {
        var preview = new CanvasBounds(200, -10, 600, 170);
        CanvasBounds[] expected =
        [
            new(200, 0, 600, 12),
            new(200, 60, 600, 100),
            new(200, 12, 150, 48),
            new(650, 12, 150, 48)
        ];

        Assert.Equal(expected, PreviewClipping.VisibleParts(preview, Viewport, Controls));
    }

    [Fact]
    public void ClipsRightSideZoomControlsButKeepsTheBottomLeft()
    {
        var preview = new CanvasBounds(580, 620, 320, 80);
        CanvasBounds[] expected =
        [
            new(580, 620, 320, 26),
            new(580, 688, 320, 12),
            new(580, 646, 40, 42),
            new(830, 646, 70, 42)
        ];

        Assert.Equal(expected, PreviewClipping.VisibleParts(preview, Viewport, Controls));
        Assert.Empty(PreviewClipping.VisibleParts(new(1000, 0, 100, 100), Viewport, Controls));
    }
}
