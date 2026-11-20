using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public class CanvasBoundsTests
{
    [Fact]
    public void ViewportAndOffscreenItemBothFitInsideMinimap()
    {
        var viewport = CanvasBounds.Viewport(new Camera(new Point(400, -200), 2), new Point(800, 600));
        Assert.Equal(new CanvasBounds(400, -200, 400, 300), viewport);

        var union = viewport.Union(new CanvasBounds(24, 400, 88, 88));
        Assert.Equal(new CanvasBounds(24, -200, 776, 688), union);
        var projection = union.Fit(new Point(180, 108), 8, double.PositiveInfinity);

        var center = Geometry.WorldToView(union.Center, projection);
        Assert.Equal(90, center.X, 8);
        Assert.Equal(54, center.Y, 8);
        Assert.Equal(8, Geometry.WorldToView(new Point(union.X, union.Y), projection).Y, 8);
    }

    [Fact]
    public void RecenteringPreservesZoomAndPutsMapPointUnderViewportCenter()
    {
        var world = new Point(900, -300);
        var camera = CameraGestures.CenterOn(world, new Point(1000, 600), 2);
        Assert.Equal(new Camera(new Point(650, -450), 2), camera);
        Assert.Equal(world, Geometry.ViewToWorld(new Point(500, 300), camera));
    }

    [Fact]
    public void FitCanFrameDistantItemsAndZoomingOutDoesNotJumpIn()
    {
        var bounds = new CanvasBounds(24, 80, 88, 10_000);
        var size = new Point(800, 600);
        var fitted = bounds.Fit(size, 32, 1);
        Assert.InRange(fitted.Zoom, 0.05, 0.06);
        Assert.Equal(fitted, CameraGestures.ZoomAt(fitted, default, -1));
        Assert.True(CameraGestures.ZoomAt(fitted, default, 1).Zoom > fitted.Zoom);
        Assert.Equal(1, new CanvasBounds(24, 80, 88, 88).Fit(size, 32, 1).Zoom);
    }
}
