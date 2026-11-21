namespace Canvas.Domain;

public static class CameraGestures
{
    public const double MinZoom = 0.25;
    public const double MaxZoom = 4;

    public static Camera CenterOn(Point world, Point viewport, double zoom) =>
        Pan(new Camera(world, zoom), new Point(viewport.X / 2, viewport.Y / 2));

    public static Camera ApproachPanel(Camera start, CanvasBounds panel, Point viewport)
    {
        if (start.Zoom == 1)
        {
            return start;
        }

        var center = Geometry.WorldToView(panel.Center, start);
        var left = Math.Clamp(center.X - panel.Width / 2, 0, Math.Max(0, viewport.X - panel.Width));
        var top = Math.Clamp(center.Y - panel.Height / 2, 0, Math.Max(0, viewport.Y - panel.Height));
        return new Camera(new Point(panel.X - left, panel.Y - top), 1);
    }

    public static Camera InterpolateAround(Camera start, Camera target, Point anchor, double progress)
    {
        var from = Geometry.WorldToView(anchor, start);
        var to = Geometry.WorldToView(anchor, target);
        var screen = new Point(Lerp(from.X, to.X), Lerp(from.Y, to.Y));
        return Pan(new Camera(anchor, Lerp(start.Zoom, target.Zoom)), screen);

        double Lerp(double a, double b) => a + (b - a) * progress;
    }

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
