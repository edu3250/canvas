using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public class HomeGridTests
{
    [Fact]
    public void PlacesAtBoundaryOrClosestFreeCell()
    {
        Assert.Equal(new Point(24,368), HomeGrid.Clamp(new Point(-80,900)));
        Assert.Equal(new Point(312,368), HomeGrid.Snap(new Point(800,900), Array.Empty<Point>()));
        Assert.Equal(new Point(120,80), HomeGrid.Snap(new Point(24,80), new[]{new Point(24,80)}));
        Assert.Equal(new Point(24,80), HomeGrid.Snap(new Point(72,80), Array.Empty<Point>()));
    }
}
