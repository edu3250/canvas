namespace Canvas.Domain;

public static class HomeGrid
{
    public const int Spacing = 96;
    private const int Columns = 4;
    public const int FirstX = 24;
    public const int FirstY = 80;
    public const int LastX = FirstX + (Columns - 1) * Spacing;
    public const double LastY = FirstY + (double)int.MaxValue * Spacing;

    public static Point Clamp(Point origin)
    {
        if (!double.IsFinite(origin.X) || !double.IsFinite(origin.Y))
        {
            throw new ArgumentOutOfRangeException(nameof(origin));
        }

        return new Point(Math.Clamp(origin.X, FirstX, LastX), Math.Clamp(origin.Y, FirstY, LastY));
    }

    public static IEnumerable<Point> FreeCells(IReadOnlySet<Point> occupied)
    {
        for (long index = 0; ; index++)
        {
            var cell = new Point(FirstX + index % Columns * Spacing, FirstY + (double)(index / Columns) * Spacing);
            if (!occupied.Contains(cell))
            {
                yield return cell;
            }
        }
    }

    public static Point Snap(Point origin, IReadOnlyCollection<Point> occupied) =>
        SnapGroup(origin, [default], occupied.ToHashSet());

    public static Point SnapGroup(Point origin, IReadOnlyList<Point> offsets, IReadOnlySet<Point> occupied)
    {
        var target = Clamp(origin);
        var minX = FirstX - offsets.Min(point => point.X);
        var maxX = LastX - offsets.Max(point => point.X);
        var minY = FirstY - offsets.Min(point => point.Y);
        var maxY = LastY - offsets.Max(point => point.Y);
        var below = Math.Clamp(minY + Math.Floor((target.Y - minY) / Spacing) * Spacing, minY, maxY);
        var above = below + Spacing;
        var bestDistance = double.PositiveInfinity;
        var best = default(Point);

        while (below >= minY || above <= maxY)
        {
            var lowerDistance = below >= minY ? Math.Abs(target.Y - below) : double.PositiveInfinity;
            var upperDistance = above <= maxY ? Math.Abs(target.Y - above) : double.PositiveInfinity;
            var rowDistance = Math.Min(lowerDistance, upperDistance);
            if (rowDistance * rowDistance > bestDistance)
            {
                break;
            }

            var y = lowerDistance <= upperDistance ? below : above;
            if (lowerDistance <= upperDistance)
            {
                below -= Spacing;
            }
            else
            {
                above += Spacing;
            }

            for (var x = minX; x <= maxX; x += Spacing)
            {
                var dx = target.X - x;
                var distance = dx * dx + rowDistance * rowDistance;
                var earlier = y < best.Y || y == best.Y && x < best.X;
                if ((distance < bestDistance || distance == bestDistance && earlier) &&
                    offsets.All(offset => !occupied.Contains(new Point(x + offset.X, y + offset.Y))))
                {
                    best = new Point(x, y);
                    bestDistance = distance;
                }
            }
        }

        return double.IsFinite(bestDistance) ? best : throw new InvalidOperationException("No free position for the group.");
    }
}
