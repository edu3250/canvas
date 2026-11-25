namespace Canvas.Domain;

public static class PreviewClipping
{
    public static Point? MapToSource(CanvasBounds destination, CanvasBounds crop, Point pointer)
    {
        var x = pointer.X - destination.X;
        var y = pointer.Y - destination.Y;
        if (x < 0 || y < 0 || x >= destination.Width || y >= destination.Height) return null;
        return new Point(crop.X + x * crop.Width / destination.Width, crop.Y + y * crop.Height / destination.Height);
    }

    public static CanvasBounds Fit(CanvasBounds area, Point source)
    {
        var scale = Math.Min(area.Width / source.X, area.Height / source.Y);
        return area with { Width = source.X * scale, Height = source.Y * scale };
    }

    public static IEnumerable<CanvasBounds> VisibleParts(CanvasBounds content, CanvasBounds viewport,
        IReadOnlyList<CanvasBounds> controls)
    {
        if (Intersect(content, viewport) is not { } visible)
        {
            return [];
        }

        return controls.Aggregate<CanvasBounds, IEnumerable<CanvasBounds>>([visible],
            (pieces, control) => pieces.SelectMany(piece => Subtract(piece, control)));
    }

    private static IEnumerable<CanvasBounds> Subtract(CanvasBounds piece, CanvasBounds control)
    {
        if (Intersect(piece, control) is not { } overlap)
        {
            yield return piece;
            yield break;
        }

        var right = piece.X + piece.Width;
        var bottom = piece.Y + piece.Height;
        var overlapRight = overlap.X + overlap.Width;
        var overlapBottom = overlap.Y + overlap.Height;
        CanvasBounds[] strips =
        [
            new(piece.X, piece.Y, piece.Width, overlap.Y - piece.Y),
            new(piece.X, overlapBottom, piece.Width, bottom - overlapBottom),
            new(piece.X, overlap.Y, overlap.X - piece.X, overlap.Height),
            new(overlapRight, overlap.Y, right - overlapRight, overlap.Height)
        ];

        foreach (var strip in strips.Where(strip => strip.Width > 0 && strip.Height > 0))
        {
            yield return strip;
        }
    }

    private static CanvasBounds? Intersect(CanvasBounds first, CanvasBounds second)
    {
        var x = Math.Max(first.X, second.X);
        var y = Math.Max(first.Y, second.Y);
        var right = Math.Min(first.X + first.Width, second.X + second.Width);
        var bottom = Math.Min(first.Y + first.Height, second.Y + second.Height);
        return right > x && bottom > y ? new CanvasBounds(x, y, right - x, bottom - y) : null;
    }
}
