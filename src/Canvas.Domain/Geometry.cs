namespace Canvas.Domain;

public readonly record struct Point(double X, double Y);

public readonly record struct Camera(Point Position, double Zoom);

public static class Geometry
{
    public static Point WorldToView(Point world, Camera camera)
    {
        ValidatePoint(world, nameof(world));
        ValidateCamera(camera);
        return new Point(
            (world.X - camera.Position.X) * camera.Zoom,
            (world.Y - camera.Position.Y) * camera.Zoom);
    }

    public static CanvasBounds NativeWindowFootprint(CanvasBounds window, Camera camera)
    {
        ValidateCamera(camera);
        return window with { Width = window.Width / camera.Zoom, Height = window.Height / camera.Zoom };
    }

    public static Point ViewToWorld(Point view, Camera camera)
    {
        ValidatePoint(view, nameof(view));
        ValidateCamera(camera);
        return new Point(
            view.X / camera.Zoom + camera.Position.X,
            view.Y / camera.Zoom + camera.Position.Y);
    }

    private static void ValidateCamera(Camera camera)
    {
        ValidatePoint(camera.Position, nameof(camera));
        if (!double.IsFinite(camera.Zoom) || camera.Zoom <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(camera));
        }
    }

    private static void ValidatePoint(Point point, string parameterName)
    {
        if (!double.IsFinite(point.X) || !double.IsFinite(point.Y))
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
