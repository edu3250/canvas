using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public class GeometryTests
{
    [Theory]
    [InlineData(120, 80, 0, 0)]
    [InlineData(160, 40, 30, -30)]
    public void ProjectsBothWays(double x, double y, double u, double v)
    {
        var camera = new Camera(new Point(120, 80), 0.75);
        var world = new Point(x, y);
        var view = new Point(u, v);
        Assert.Equal(view, Geometry.WorldToView(world, camera));
        Assert.Equal(world, Geometry.ViewToWorld(view, camera));
        var roundWorld = Geometry.ViewToWorld(Geometry.WorldToView(world, camera), camera);
        var roundView = Geometry.WorldToView(Geometry.ViewToWorld(view, camera), camera);
        foreach (var delta in new[] { roundWorld.X-x, roundWorld.Y-y, roundView.X-u, roundView.Y-v })
            Assert.InRange(Math.Abs(delta), 0, 1e-9);
    }

    [Fact]
    public void RejectsInvalidOperands()
    {
        var zero = new Point(0, 0);
        var valid = new Camera(zero, 1);
        foreach (var n in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        foreach (var p in new[] { new Point(n, 0), new Point(0, n) })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Geometry.WorldToView(p, valid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Geometry.ViewToWorld(p, valid));
            Assert.Throws<ArgumentOutOfRangeException>(() => Geometry.WorldToView(zero, new Camera(p, 1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => Geometry.ViewToWorld(zero, new Camera(p, 1)));
        }
        foreach (var z in new[] { 0d, -1, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Geometry.WorldToView(zero, new Camera(zero, z)));
            Assert.Throws<ArgumentOutOfRangeException>(() => Geometry.ViewToWorld(zero, new Camera(zero, z)));
        }
    }
}
