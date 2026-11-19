using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SurfaceCanvas = System.Windows.Controls.Canvas;
using ShapePath = System.Windows.Shapes.Path;

namespace Canvas.Desktop;

public sealed class MainWindow : Window
{
    private const double InitialWidth = 1100;
    private const double InitialHeight = 720;

    public MainWindow()
    {
        Title = "Escritorio Canvas";
        Width = InitialWidth;
        Height = InitialHeight;
        MinWidth = 640;
        MinHeight = 480;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var viewport = new Grid { ClipToBounds = true };
        viewport.Children.Add(new Viewbox
        {
            Stretch = Stretch.UniformToFill,
            IsHitTestVisible = false,
            Child = CreateLandscape()
        });
        viewport.Children.Add(new SurfaceCanvas { Background = Brushes.Transparent, ClipToBounds = true });
        viewport.Children.Add(new Grid { VerticalAlignment = VerticalAlignment.Top });
        Content = viewport;
    }

    private static SurfaceCanvas CreateLandscape()
    {
        var landscape = new SurfaceCanvas
        {
            Width = InitialWidth,
            Height = InitialHeight,
            Background = new LinearGradientBrush(
                Color.FromRgb(79, 116, 144), Color.FromRgb(236, 201, 163), 90)
        };

        var sun = new Ellipse { Width = 100, Height = 100, Fill = BrushFromRgb(255, 222, 174) };
        SurfaceCanvas.SetLeft(sun, 752);
        SurfaceCanvas.SetTop(sun, 125);
        landscape.Children.Add(sun);

        AddLayer(landscape, "M0,434 L0,312 158,232 241,277 386,168 498,257 563,221 735,352 893,244 1100,326 1100,434 Z",
            BrushFromRgb(104, 131, 146));
        AddLayer(landscape, "M0,449 L0,337 128,291 270,369 396,307 517,356 668,282 787,342 942,302 1100,365 1100,449 Z",
            BrushFromRgb(66, 102, 118));
        AddLayer(landscape, "M0,459 L0,387 145,362 246,398 411,382 550,426 722,388 887,414 1014,369 1100,382 1100,459 Z",
            BrushFromRgb(41, 76, 88));

        var lake = new Rectangle
        {
            Width = InitialWidth,
            Height = 282,
            Fill = new LinearGradientBrush(Color.FromRgb(125, 155, 157), Color.FromRgb(27, 65, 77), 90)
        };
        SurfaceCanvas.SetTop(lake, 438);
        landscape.Children.Add(lake);

        AddLayer(landscape, "M628,461 H903 L850,466 H661 Z M715,486 H981 L913,491 H753 Z M566,525 H766 L729,529 H587 Z",
            new SolidColorBrush(Color.FromArgb(66, 255, 222, 174)));
        AddLayer(landscape, "M0,612 Q160,623 306,679 Q448,708 585,720 H0 Z M1100,620 Q1000,643 934,688 L870,720 H1100 Z",
            BrushFromRgb(18, 52, 63));
        return landscape;
    }

    private static SolidColorBrush BrushFromRgb(byte red, byte green, byte blue) =>
        new(Color.FromRgb(red, green, blue));

    private static void AddLayer(SurfaceCanvas landscape, string outline, Brush fill)
    {
        landscape.Children.Add(new ShapePath { Data = Geometry.Parse(outline), Fill = fill });
    }
}
