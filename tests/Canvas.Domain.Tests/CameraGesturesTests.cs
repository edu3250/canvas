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
}
