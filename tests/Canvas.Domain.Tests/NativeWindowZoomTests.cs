using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public sealed class NativeWindowZoomTests
{
    [Theory]
    [InlineData(0.5)]
    [InlineData(1)]
    [InlineData(3)]
    public void CameraScalesWindowSeparationAndPreservesScreenDimensions(double zoom)
    {
        var camera = new Camera(new Point(100, 20), zoom);
        var first = new CanvasBounds(120, 40, 1000, 700);
        var second = first with { X = 1320 };
        var a = Geometry.WorldToView(new Point(first.X, first.Y), camera);
        var b = Geometry.WorldToView(new Point(second.X, second.Y), camera);
        var footprint = Geometry.NativeWindowFootprint(first, camera);
        Assert.Equal(1200 * zoom, b.X - a.X);
        var end = Geometry.WorldToView(new Point(footprint.X + footprint.Width, footprint.Y + footprint.Height), camera);
        Assert.Equal(1000, end.X - a.X, 8);
        Assert.Equal(700, end.Y - a.Y, 8);
    }
}
