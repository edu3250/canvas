namespace Canvas.Domain;

public static class HomeGrid
{
    private const int CellsPerAxis = 4;
    private const int Spacing = 96;
    private const int FirstX = 24;
    private const int FirstY = 80;
    private const int LastOffset = (CellsPerAxis - 1) * Spacing;

    public static Point Clamp(Point origin)
    {
        ValidatePoint(origin, nameof(origin));
        return new Point(
            Math.Clamp(origin.X, FirstX, FirstX + LastOffset),
            Math.Clamp(origin.Y, FirstY, FirstY + LastOffset));
    }

    public static Point Snap(Point origin, IReadOnlyCollection<Point> occupied)
    {
        var clamped = Clamp(origin);
        ArgumentNullException.ThrowIfNull(occupied);
        var occupiedCells = new HashSet<Point>();
        foreach (var point in occupied)
        {
            ValidatePoint(point, nameof(occupied));
            occupiedCells.Add(point);
        }

        Point? nearest = null;
        var nearestDistance = double.PositiveInfinity;
        for (var row = 0; row < CellsPerAxis; row++)
        {
            for (var column = 0; column < CellsPerAxis; column++)
            {
                var cell = new Point(FirstX + column * Spacing, FirstY + row * Spacing);
                if (occupiedCells.Contains(cell))
                {
                    continue;
                }

                var dx = cell.X - clamped.X;
                var dy = cell.Y - clamped.Y;
                var distance = dx * dx + dy * dy;
                if (distance < nearestDistance)
                {
                    nearest = cell;
                    nearestDistance = distance;
                }
            }
        }

        return nearest ?? throw new InvalidOperationException("The home grid has no free cells.");
    }

    private static void ValidatePoint(Point point, string parameterName)
    {
        if (!double.IsFinite(point.X) || !double.IsFinite(point.Y))
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
