using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public class HomeGridTests
{
    [Fact]
    public void PlacesAtBoundaryOrClosestFreeCell()
    {
        Assert.Equal(new Point(24, 900), HomeGrid.Clamp(new Point(-80, 900)));
        Assert.Equal(new Point(312, 944), HomeGrid.Snap(new Point(800, 900), []));
        Assert.Equal(new Point(120, 80), HomeGrid.Snap(new Point(24, 80), [new Point(24, 80)]));
        Assert.Equal(new Point(24, 80), HomeGrid.Snap(new Point(72, 80), []));
    }

    [Fact]
    public void FillsRowsInOrderAndReusesFreedCells()
    {
        var occupied = HomeGrid.FreeCells(new HashSet<Point>()).Take(17).ToHashSet();
        Assert.Contains(new Point(24, 464), occupied);
        Assert.Equal(new Point(120, 464), HomeGrid.FreeCells(occupied).First());

        occupied.Remove(new Point(216, 176));
        Assert.Equal(new Point(216, 176), HomeGrid.FreeCells(occupied).First());
    }

    [Fact]
    public void MovesGroupPastFourthRowWithoutOverlapOrChangingShape()
    {
        var occupied = new HashSet<Point> { new(312, 464) };
        Assert.Equal(new Point(312, 368),
            HomeGrid.SnapGroup(new Point(400, 424), [default, new Point(-96, 96)], occupied));
        Assert.Equal(new Point(312, 560),
            HomeGrid.SnapGroup(new Point(400, 475), [default, new Point(0, 96)], occupied));
    }

    [Fact]
    public void SnapsFarBelowTheCurrentRows()
    {
        Assert.Equal(new Point(24, 96_000_080), HomeGrid.Snap(new Point(24, 96_000_081), []));
        Assert.Throws<ArgumentOutOfRangeException>(() => HomeGrid.Clamp(new Point(24, double.NaN)));
    }
}
