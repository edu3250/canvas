using Canvas.Domain;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Canvas.Desktop;

internal sealed class PanelCard : Border
{
    private static readonly Brush Slate = DeskControls.Brush(VisualTokens.Colors.Slate);
    private static readonly Brush Raised = DeskControls.Brush(VisualTokens.Colors.Raised);
    private static readonly Brush Bone = DeskControls.Brush(VisualTokens.Colors.Bone);
    private static readonly CornerRadius TabRadius = new(VisualTokens.Radius.Handle, VisualTokens.Radius.Handle, 0, 0);
    private readonly Image icon = new() { Width = 16, Height = 16 };
    private readonly Border iconFrame;
    private readonly Border tongue;
    private readonly Border halo;
    private readonly Border lightEdge;
    private readonly Grid body;
    private readonly TextBlock minimizedLabel = new()
    {
        Text = "minimizada", Foreground = DeskControls.Brush(VisualTokens.Colors.Warning),
        FontFamily = Theme.Font, FontSize = VisualTokens.Type.Caption, VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(6, 0, 0, 0), Visibility = Visibility.Collapsed
    };
    private bool active;
    private bool compact;

    public event Action? FoldRequested;
    public event Action? MaximizeRequested;
    public event Action? CloseRequested;
    public event Action? RemoveRequested;

    public PanelCard(string title)
    {
        BorderThickness = new Thickness(1);
        var content = new Grid();
        content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(VisualTokens.Size.PanelHandle) });
        content.RowDefinitions.Add(new RowDefinition());
        Child = content;

        iconFrame = new Border
        {
            Width = 20, Height = 20, CornerRadius = new CornerRadius(5), Background = Raised,
            Child = new TextBlock
            {
                Text = "▣", Foreground = Bone, HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center, FontSize = VisualTokens.Type.Detail
            }
        };
        var label = new TextBlock
        {
            Text = title, TextTrimming = TextTrimming.CharacterEllipsis, Foreground = Bone,
            FontFamily = Theme.Font, FontSize = VisualTokens.Type.Detail,
            Margin = new Thickness(7, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center
        };
        var handleContent = new DockPanel();
        handleContent.Children.Add(iconFrame);
        DockPanel.SetDock(minimizedLabel, Dock.Right);
        handleContent.Children.Add(minimizedLabel);
        handleContent.Children.Add(label);
        Handle = new Border
        {
            Background = Slate, CornerRadius = TabRadius, Child = handleContent,
            Cursor = Cursors.SizeAll, ToolTip = title, HorizontalAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(7, 0, 10, 0), ClipToBounds = true
        };
        AutomationProperties.SetName(Handle, title);

        var controls = new UniformGrid { Rows = 1, Columns = 4 };
        AddControl("…", "Más opciones", OpenMenu);
        AddControl("−", "Plegar o desplegar panel", _ => FoldRequested?.Invoke());
        AddControl("⤢", "Maximizar ventana", _ => MaximizeRequested?.Invoke());
        AddControl("×", "Cerrar ventana", _ => CloseRequested?.Invoke());
        tongue = new Border { Background = Slate, CornerRadius = TabRadius, Child = controls };
        var tabs = new Grid { Background = Brushes.Transparent };
        tabs.ColumnDefinitions.Add(new ColumnDefinition());
        tabs.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MaxWidth = 126 });
        tabs.Children.Add(Handle);
        Grid.SetColumn(tongue, 1);
        tabs.Children.Add(tongue);
        content.Children.Add(tabs);

        body = new Grid { Background = Raised };
        Grid.SetRow(body, 1);
        content.Children.Add(body);

        PreviewButton = new Button
        {
            Style = DeskControls.ButtonStyle, MinHeight = 0, Padding = new Thickness(0),
            Background = Brushes.Transparent, BorderBrush = Brushes.Transparent,
            Cursor = Cursors.Hand, Content = Text
        };
        AutomationProperties.SetName(PreviewButton, $"Activar {title}");
        body.Children.Add(PreviewButton);

        var glow = new DropShadowEffect { BlurRadius = 24, ShadowDepth = 0, Opacity = 1 };
        halo = new Border
        {
            BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(VisualTokens.Radius.Panel),
            IsHitTestVisible = false, Visibility = Visibility.Collapsed, Margin = new Thickness(-1),
            Effect = glow
        };
        halo.SetResourceReference(Border.BorderBrushProperty, ThemeKeys.LightHalo);
        BindingOperations.SetBinding(glow, DropShadowEffect.ColorProperty, new Binding("BorderBrush.Color") { Source = halo });
        body.Children.Add(halo);

        lightEdge = new Border
        {
            Height = 2, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, -2, 0, 0),
            IsHitTestVisible = false, Visibility = Visibility.Collapsed
        };
        lightEdge.SetResourceReference(BackgroundProperty, ThemeKeys.Light);
        body.Children.Add(lightEdge);

        MouseEnter += (_, _) => UpdateTongue();
        MouseLeave += (_, _) => UpdateTongue();
        IsKeyboardFocusWithinChanged += (_, _) => UpdateTongue();
        UpdateTongue();

        void AddControl(string glyph, string name, Action<Button> action)
        {
            var button = new Button
            {
                Style = DeskControls.ButtonStyle, MinHeight = 0, ToolTip = name,
                Background = Brushes.Transparent, BorderBrush = Brushes.Transparent,
                Padding = new Thickness(2), Margin = new Thickness(0, 3, 0, 3),
                Content = new Viewbox
                {
                    MaxWidth = 13, MaxHeight = 14,
                    Child = new TextBlock { Text = glyph, Foreground = Bone }
                }
            };
            AutomationProperties.SetName(button, name);
            button.Click += (_, _) => action(button);
            controls.Children.Add(button);
        }

        void OpenMenu(Button more)
        {
            var remove = new MenuItem { Header = "Quitar del lienzo" };
            remove.Click += (_, _) => RemoveRequested?.Invoke();
            var menu = new ContextMenu { PlacementTarget = more, Placement = PlacementMode.Bottom };
            menu.Items.Add(remove);
            menu.Closed += (_, _) => UpdateTongue();
            more.ContextMenu = menu;
            menu.IsOpen = true;
        }
    }

    public Border Handle { get; }
    public Button PreviewButton { get; }
    public TextBlock Text { get; } = new()
    {
        Foreground = Bone, FontFamily = Theme.Font,
        TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center
    };

    public void SetIcon(ImageSource image)
    {
        icon.Source = image;
        iconFrame.Child = icon;
    }

    public void SetCompact(bool value)
    {
        if (compact == value)
        {
            return;
        }

        compact = value;
        minimizedLabel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        body.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
        UpdateTongue();
    }

    public void SetActive(bool value)
    {
        if (value == active)
        {
            return;
        }

        active = value;
        halo.Visibility = lightEdge.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
        PreviewButton.Visibility = active ? Visibility.Hidden : Visibility.Visible;
        if (active)
        {
            iconFrame.SetResourceReference(BackgroundProperty, ThemeKeys.Light);
        }
        else
        {
            iconFrame.Background = Raised;
        }

        UpdateTongue();
    }

    private void UpdateTongue() =>
        tongue.Visibility = active || compact || IsMouseOver || IsKeyboardFocusWithin
            ? Visibility.Visible : Visibility.Hidden;
}
