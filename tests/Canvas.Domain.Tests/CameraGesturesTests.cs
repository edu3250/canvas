using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public class CameraGesturesTests
{
    [Fact]
    public void PanUsesViewDistance()
    {
        var start = new Camera(new Point(10, 20), 2);
        Assert.Equal(new Camera(new Point(0, 25), 2), CameraGestures.Pan(start, new Point(20, -10)));
    }

    [Theory]
    [InlineData(1, 1.25)]
    [InlineData(100, 4)]
    [InlineData(-100, 0.25)]
    public void ZoomKeepsCursor(double steps, double expected)
    {
        var c = new Camera(new Point(12, 40), 1);
        var p = new Point(100, 80);
        var next = CameraGestures.ZoomAt(c, p, steps);
        Assert.Equal(expected, next.Zoom);
        Assert.Equal(Geometry.ViewToWorld(p, c), Geometry.ViewToWorld(p, next));
    }

    [Fact]
    public void PanelApproachAtActualSizeKeepsTheCameraEvenIfClipped()
    {
        var start = new Camera(new Point(25, 50), 1);
        var panel = new CanvasBounds(0, 0, 400, 300);

        Assert.Equal(start, CameraGestures.ApproachPanel(start, panel, new Point(800, 600)));
    }

    [Fact]
    public void PanelApproachKeepsTheVisibleCenterThroughoutZoom()
    {
        var start = new Camera(new Point(100, 0), 0.5);
        var panel = new CanvasBounds(500, 400, 200, 100);
        var target = CameraGestures.ApproachPanel(start, panel, new Point(800, 600));
        var midpoint = CameraGestures.InterpolateAround(start, target, panel.Center, 0.5);

        Assert.Equal(1, target.Zoom);
        Assert.Equal(0.75, midpoint.Zoom);
        Assert.Equal(new Point(250, 225), Geometry.WorldToView(panel.Center, target));
        Assert.Equal(new Point(250, 225), Geometry.WorldToView(panel.Center, midpoint));
    }

    [Theory]
    [InlineData(400, 1550, 600)]
    [InlineData(1200, 350, -125)]
    public void PanelApproachMovesOnlyTheAxisThatNeedsRoom(double width, double x, double expectedLeft)
    {
        var start = new Camera(new Point(0, 0), 0.5);
        var panel = new CanvasBounds(x, 450, width, 100);
        var target = CameraGestures.ApproachPanel(start, panel, new Point(1000, 600));

        Assert.Equal(new Point(expectedLeft, 200), Geometry.WorldToView(new Point(panel.X, panel.Y), target));
    }

    [Theory]
    [InlineData(350, 180, -125, -110)]
    [InlineData(800, 600, 0, 0)]
    [InlineData(0, 0, -200, -200)]
    public void OversizedPanelKeepsBothOffsetsWithinTheAreaItCanCover(
        double x, double y, double expectedLeft, double expectedTop)
    {
        var start = new Camera(new Point(0, 0), 0.5);
        var panel = new CanvasBounds(x, y, 1200, 800);
        var target = CameraGestures.ApproachPanel(start, panel, new Point(1000, 600));

        Assert.Equal(new Point(expectedLeft, expectedTop), Geometry.WorldToView(new Point(x, y), target));
    }
}
