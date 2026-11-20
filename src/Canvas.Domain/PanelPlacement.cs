namespace Canvas.Domain;

public static class PanelPlacement
{
    public static CanvasBounds FindFree(CanvasBounds preferred, IReadOnlyList<CanvasBounds> occupied)
    {
        if (IsFree(preferred, occupied))
        {
            return preferred;
        }

        var right = occupied.Max(bounds => bounds.X + bounds.Width);
        var best = preferred with { X = right };
        var distance = Math.Abs(right - preferred.X);
        var columns = new HashSet<double> { preferred.X };
        var rows = new HashSet<double> { preferred.Y };
        foreach (var bounds in occupied)
        {
            columns.Add(bounds.X + bounds.Width);
            columns.Add(bounds.X - preferred.Width);
            rows.Add(bounds.Y + bounds.Height);
            rows.Add(bounds.Y - preferred.Height);
        }

        // The closest free origin lies on the edge of a blocked region, or at the preferred origin.
        foreach (var x in columns)
        {
            if (Math.Abs(x - preferred.X) > distance)
            {
                continue;
            }

            foreach (var y in rows)
            {
                var dx = x - preferred.X;
                var dy = y - preferred.Y;
                var candidateDistance = Math.Sqrt(dx * dx + dy * dy);
                var candidate = preferred with { X = x, Y = y };
                if (candidateDistance < distance && IsFree(candidate, occupied))
                {
                    distance = candidateDistance;
                    best = candidate;
                }
            }
        }

        return best;
    }

    private static bool IsFree(CanvasBounds candidate, IReadOnlyList<CanvasBounds> occupied) =>
        occupied.All(bounds => candidate.X >= bounds.X + bounds.Width || candidate.X + candidate.Width <= bounds.X ||
            candidate.Y >= bounds.Y + bounds.Height || candidate.Y + candidate.Height <= bounds.Y);
}
