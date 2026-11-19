using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public class SpaceTests
{
    [Fact]
    public void OwnsItsSpacesAndRejectsDuplicates()
    {
        var id = new SpaceId("A");
        var c = new Camera(new Point(0, 0), 1);
        var nav = new SpaceNavigation(c, c, null);
        var input = new List<KeyValuePair<SpaceId, SpaceNavigation>> { new(id, nav) };
        var session = new SessionState(id, input);
        input.Add(new(id, nav));
        Assert.ThrowsAny<ArgumentException>(() => new SessionState(id, input));
        Assert.Equal(2, input.Count);
        input.Clear();
        Assert.Equal(id, session.ActiveSpaceId);
        Assert.Equal(nav, Assert.Single(session.Spaces).Value);
    }

    [Fact]
    public void UsesOrdinalIdentifiersAndExposesReadOnlySpaces()
    {
        var upper = new SpaceId("A");
        var lower = new SpaceId("a");
        var home = new Camera(new Point(0, 0), 1);
        var view = new Camera(new Point(120, 80), 0.75);
        var input = new Dictionary<SpaceId, SpaceNavigation>
        {
            [upper] = new(home, home, view),
            [lower] = new(view, home, null)
        };

        var session = new SessionState(lower, input);
        Assert.Equal(lower, session.ActiveSpaceId);
        Assert.Equal(input[upper], session.Spaces[upper]);
        Assert.Equal(input[lower], session.Spaces[lower]);
        var spaces = Assert.IsAssignableFrom<IDictionary<SpaceId, SpaceNavigation>>(session.Spaces);
        Assert.Throws<NotSupportedException>(() => spaces.Remove(upper));
    }

    [Fact]
    public void RejectsEmptyIdentifiersAndMissingActiveSpace()
    {
        var id = new SpaceId("A");
        var camera = new Camera(new Point(0, 0), 1);
        var navigation = new SpaceNavigation(camera, camera, null);
        var input = new Dictionary<SpaceId, SpaceNavigation> { [id] = navigation };

        Assert.ThrowsAny<ArgumentException>(() => new SessionState(new SpaceId("a"), input));
        Assert.Equal(navigation, Assert.Single(input).Value);

        foreach (var empty in new[] { default(SpaceId), new SpaceId(string.Empty) })
        {
            input.Add(empty, navigation);
            Assert.ThrowsAny<ArgumentException>(() => new SessionState(id, input));
            Assert.Equal(navigation, input[empty]);
            input.Remove(empty);
        }
    }

    [Theory]
    [InlineData(double.NaN, 0, 1)]
    [InlineData(0, double.PositiveInfinity, 1)]
    [InlineData(double.NegativeInfinity, 0, 1)]
    [InlineData(0, 0, 0)]
    [InlineData(0, 0, -1)]
    [InlineData(0, 0, double.NaN)]
    [InlineData(0, 0, double.PositiveInfinity)]
    [InlineData(0, 0, double.NegativeInfinity)]
    public void RejectsInvalidCamerasInEveryView(double x, double y, double zoom)
    {
        var id = new SpaceId("A");
        var valid = new Camera(new Point(0, 0), 1);
        var invalid = new Camera(new Point(x, y), zoom);
        SpaceNavigation[] invalidViews =
        [
            new(invalid, valid, null),
            new(valid, invalid, null),
            new(valid, valid, invalid)
        ];

        foreach (var navigation in invalidViews)
        {
            var input = new Dictionary<SpaceId, SpaceNavigation> { [id] = navigation };
            Assert.ThrowsAny<ArgumentException>(() => new SessionState(id, input));
            Assert.Equal(navigation, Assert.Single(input).Value);
        }
    }
}
