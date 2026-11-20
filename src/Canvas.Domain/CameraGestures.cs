namespace Canvas.Domain;

public static class CameraGestures
{
    public const double MinZoom = 0.25;
    public const double MaxZoom = 4;

    public static Camera CenterOn(Point world, Point viewport, double zoom) =>
        Pan(new Camera(world, zoom), new Point(viewport.X / 2, viewport.Y / 2));

    public static Camera Pan(Camera start, Point delta)
    {
        var position = Geometry.ViewToWorld(new Point(-delta.X, -delta.Y), start);
        if (!double.IsFinite(position.X) || !double.IsFinite(position.Y))
        {
            throw new ArgumentOutOfRangeException(nameof(delta));
        }

        return new Camera(position, start.Zoom);
    }

    public static Camera ZoomAt(Camera start, Point cursor, double steps)
    {
        if (!double.IsFinite(steps))
        {
            throw new ArgumentOutOfRangeException(nameof(steps));
        }

        var anchor = Geometry.ViewToWorld(cursor, start);
        var factor = Math.Pow(1.25, steps);
        // Combine in log space when the factor alone would lose range or precision.
        var scaledZoom = double.IsNormal(factor)
            ? start.Zoom * factor
            : Math.Exp(Math.Log(start.Zoom) + steps * Math.Log(1.25));
        // Fit can go below the manual limit when items are far apart.
        var zoom = Math.Clamp(scaledZoom, Math.Min(start.Zoom, MinZoom), MaxZoom);

        return Pan(new Camera(anchor, zoom), cursor);
    }
}
