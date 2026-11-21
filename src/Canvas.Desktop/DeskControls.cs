using Canvas.Domain;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Canvas.Desktop;

internal static class DeskControls
{
    public static SolidColorBrush Brush(TokenColor color)
    {
        var brush = new SolidColorBrush(Theme.ToColor(color));
        brush.Freeze();
        return brush;
    }

    public static Style ButtonStyle { get; } = CreateButtonStyle();

    private static Style CreateButtonStyle()
    {
        var style = new Style(typeof(Button));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Brush(VisualTokens.Colors.Raised)));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Brush(VisualTokens.Colors.Bone)));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, Brush(VisualTokens.Colors.Border)));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(9, 0, 9, 0)));
        style.Setters.Add(new Setter(Control.MinHeightProperty, VisualTokens.Size.CompactControl));
        style.Setters.Add(new Setter(Control.FontFamilyProperty, Theme.Font));
        style.Setters.Add(new Setter(Control.FontSizeProperty, VisualTokens.Type.Interface));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
        style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));

        var border = new FrameworkElementFactory(typeof(Border)) { Name = "Chrome" };
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(VisualTokens.Radius.Control));
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding(nameof(Control.Background))
            { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
        border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));
        var content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetValue(FrameworkElement.MarginProperty, new TemplateBindingExtension(Control.PaddingProperty));
        content.SetValue(FrameworkElement.HorizontalAlignmentProperty, new TemplateBindingExtension(Control.HorizontalContentAlignmentProperty));
        content.SetValue(FrameworkElement.VerticalAlignmentProperty, new TemplateBindingExtension(Control.VerticalContentAlignmentProperty));
        content.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);
        border.AppendChild(content);

        var template = new ControlTemplate(typeof(Button)) { VisualTree = border };
        AddTrigger(UIElement.IsMouseOverProperty, true, VisualTokens.Colors.Ash);
        AddTrigger(UIElement.IsKeyboardFocusedProperty, true, VisualTokens.DefaultLight.Color);
        AddTrigger(ButtonBase.IsPressedProperty, true, VisualTokens.Colors.Bone);
        var disabled = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
        disabled.Setters.Add(new Setter(UIElement.OpacityProperty, 0.47, "Chrome"));
        template.Triggers.Add(disabled);
        style.Setters.Add(new Setter(Control.TemplateProperty, template));
        return style;

        void AddTrigger(DependencyProperty property, bool value, TokenColor color)
        {
            var trigger = new Trigger { Property = property, Value = value };
            trigger.Setters.Add(new Setter(Border.BorderBrushProperty, Brush(color), "Chrome"));
            template.Triggers.Add(trigger);
        }
    }

    public static DrawingBrush NightGrid()
    {
        var dot = new GeometryDrawing(Brush(VisualTokens.Colors.Bone.WithAlpha(0.09)), null,
            new EllipseGeometry(new System.Windows.Point(12, 12), 1, 1));
        var tile = new DrawingGroup();
        tile.Children.Add(new GeometryDrawing(Brush(VisualTokens.Colors.Night), null,
            new RectangleGeometry(new Rect(0, 0, 24, 24))));
        tile.Children.Add(dot);
        var brush = new DrawingBrush(tile)
        {
            ViewportUnits = BrushMappingMode.Absolute,
            Viewport = new Rect(0, 0, 24, 24),
            TileMode = TileMode.Tile
        };
        brush.Freeze();
        return brush;
    }
}
