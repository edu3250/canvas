using Canvas.Domain;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;

namespace Canvas.Desktop;

internal sealed class WindowPicker : Window
{
    private readonly ListBox options;
    private readonly Button add;

    public WindowPicker(Window owner, WindowCandidate[] windows)
    {
        Owner = owner;
        Title = "Añadir ventana";
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        WindowStyle = WindowStyle.None;
        ShowInTaskbar = false;
        ResizeMode = ResizeMode.NoResize;
        Width = Math.Clamp(owner.ActualWidth - 24, 320, 480);
        SizeToContent = SizeToContent.Height;
        FontFamily = Theme.Font;
        FontSize = VisualTokens.Type.Body;
        Background = DeskControls.Brush(VisualTokens.Colors.Slate);
        Foreground = DeskControls.Brush(VisualTokens.Colors.Bone);

        var body = new StackPanel { Margin = new Thickness(24) };
        body.Children.Add(new TextBlock
        {
            Text = "Añadir ventana al espacio", FontSize = VisualTokens.Type.Title,
            TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 16)
        });
        options = new ListBox
        {
            ItemTemplate = CreateTitleTemplate(), ItemsSource = windows,
            MaxHeight = Math.Clamp(owner.ActualHeight / 2, 120, 320),
            Background = DeskControls.Brush(VisualTokens.Colors.Night),
            Foreground = Foreground,
            BorderBrush = DeskControls.Brush(VisualTokens.Colors.Border),
            Padding = new Thickness(6)
        };
        AutomationProperties.SetName(options, "Ventanas disponibles");
        options.Resources[SystemColors.HighlightBrushKey] = DeskControls.Brush(VisualTokens.Colors.Raised);
        options.Resources[SystemColors.HighlightTextBrushKey] = Foreground;
        options.Resources[SystemColors.InactiveSelectionHighlightBrushKey] = options.Resources[SystemColors.HighlightBrushKey];
        options.Resources[SystemColors.InactiveSelectionHighlightTextBrushKey] = Foreground;
        ScrollViewer.SetHorizontalScrollBarVisibility(options, ScrollBarVisibility.Disabled);
        if (windows.Length == 0)
        {
            body.Children.Add(new TextBlock
            {
                Text = "No hay ventanas disponibles.", Foreground = DeskControls.Brush(VisualTokens.Colors.Ash)
            });
        }
        else
        {
            body.Children.Add(options);
        }

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 16, 0, 0)
        };
        var cancel = new Button
        {
            Style = DeskControls.ButtonStyle, IsCancel = true, Content = "Cancelar",
            MinHeight = VisualTokens.Size.DialogControl, Margin = new Thickness(0, 0, 8, 0)
        };
        add = new Button
        {
            Style = DeskControls.ButtonStyle, IsDefault = true, Content = "Añadir",
            MinHeight = VisualTokens.Size.DialogControl, IsEnabled = false,
            Background = DeskControls.Brush(VisualTokens.DefaultLight.Color),
            Foreground = DeskControls.Brush(VisualTokens.Colors.Night)
        };
        add.SetResourceReference(Control.BackgroundProperty, ThemeKeys.Light);
        add.Click += (_, _) => Confirm();
        options.SelectionChanged += (_, _) => add.IsEnabled = options.SelectedItem is WindowCandidate;
        options.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                Confirm();
                e.Handled = true;
            }
        };
        options.MouseDoubleClick += (_, e) =>
        {
            if (e.OriginalSource is DependencyObject source &&
                ItemsControl.ContainerFromElement(options, source) is not null)
            {
                Confirm();
            }
        };
        Loaded += (_, _) =>
        {
            if (windows.Length == 0)
            {
                cancel.Focus();
            }
            else
            {
                options.SelectedIndex = 0;
                if (options.ItemContainerGenerator.ContainerFromIndex(0) is ListBoxItem item)
                {
                    item.Focus();
                }
                else
                {
                    options.Focus();
                }
            }
        };
        actions.Children.Add(cancel);
        actions.Children.Add(add);
        body.Children.Add(actions);
        Content = new Border
        {
            Background = Background,
            BorderBrush = DeskControls.Brush(VisualTokens.Colors.Border),
            BorderThickness = new Thickness(1),
            Child = body
        };
    }

    public WindowCandidate? SelectedWindow { get; private set; }

    private static DataTemplate CreateTitleTemplate()
    {
        var title = new FrameworkElementFactory(typeof(TextBlock));
        var binding = new System.Windows.Data.Binding(nameof(WindowCandidate.Title));
        title.SetBinding(TextBlock.TextProperty, binding);
        title.SetBinding(ToolTipProperty, new System.Windows.Data.Binding(nameof(WindowCandidate.Title)));
        title.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
        title.SetValue(TextBlock.PaddingProperty, new Thickness(8, 5, 8, 5));
        return new DataTemplate { VisualTree = title };
    }

    private void Confirm()
    {
        if (options.SelectedItem is WindowCandidate selected)
        {
            SelectedWindow = selected;
            DialogResult = true;
        }
    }
}
