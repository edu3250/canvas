namespace Canvas.Domain;

public readonly record struct CanvasBounds(double X, double Y, double Width, double Height)
{
    public Point Center => new(X + Width / 2, Y + Height / 2);

    public static CanvasBounds Viewport(Camera camera, Point size) =>
        new(camera.Position.X, camera.Position.Y, size.X / camera.Zoom, size.Y / camera.Zoom);

    public CanvasBounds Union(CanvasBounds other)
    {
        var left = Math.Min(X, other.X);
        var top = Math.Min(Y, other.Y);
        return new CanvasBounds(left, top,
            Math.Max(X + Width, other.X + other.Width) - left,
            Math.Max(Y + Height, other.Y + other.Height) - top);
    }

    public Camera Fit(Point viewport, double padding, double maxZoom)
    {
        var zoom = Math.Min(maxZoom, Math.Min(
            Math.Max(1, viewport.X - 2 * padding) / Width,
            Math.Max(1, viewport.Y - 2 * padding) / Height));
        return CameraGestures.CenterOn(Center, viewport, zoom);
    }
}
