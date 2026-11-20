using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public sealed class PanelPlacementTests
{
    [Fact]
    public void KeepsPreferredCenterWhenFree()
    {
        var preferred = new CanvasBounds(-120, 30, 180, 100);
        Assert.Equal(preferred, PanelPlacement.FindFree(preferred, []));
    }

    [Fact]
    public void ChoosesCloserVerticalGapInsteadOfMovingToTheOuterRight()
    {
        var preferred = new CanvasBounds(30, 20, 60, 40);
        var occupied = new CanvasBounds(0, 0, 100, 30);

        Assert.Equal(new CanvasBounds(30, 30, 60, 40), PanelPlacement.FindFree(preferred, [occupied]));
    }

    [Fact]
    public void UsesCornerWhenMovementAlongEitherAxisAloneCostsMore()
    {
        var preferred = new CanvasBounds(48, 48, 10, 10);
        CanvasBounds[] occupied = [new(0, 0, 50, 200), new(0, 0, 200, 50)];

        Assert.Equal(new CanvasBounds(50, 50, 10, 10), PanelPlacement.FindFree(preferred, occupied));
    }

    [Fact]
    public void PlacingIdenticalPanelsKeepsTheirEdgesFromOverlapping()
    {
        var bounds = new CanvasBounds(80, 96, 40, 160);
        Assert.Equal(new CanvasBounds(120, 96, 40, 160), PanelPlacement.FindFree(bounds, [bounds]));
    }
}
