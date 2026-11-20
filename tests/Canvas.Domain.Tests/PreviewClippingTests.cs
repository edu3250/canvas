using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public sealed class PreviewClippingTests
{
    private static readonly CanvasBounds Viewport = new(0, 0, 1000, 700);
    private static readonly CanvasBounds[] Controls = [new(350, 12, 300, 48), new(620, 646, 210, 42)];

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
