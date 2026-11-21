using Canvas.Domain;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Canvas.Desktop;

internal sealed class EdgeBar : Grid
{
    private readonly Border bar;
    private readonly Button grip;
    private readonly DispatcherTimer retreat = new() { Interval = TimeSpan.FromMilliseconds(260) };
    private bool keepKeyboardFocus;

    public EdgeBar(Window owner, Border bar)
    {
        this.bar = bar;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Top;

        var light = new Border
        {
            Width = 32, Height = 4, CornerRadius = new CornerRadius(2),
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
        };
        light.SetResourceReference(Border.BackgroundProperty, ThemeKeys.Light);
        var marker = new Border
        {
            Width = 116, Height = 4, CornerRadius = new CornerRadius(2),
            Background = DeskControls.Brush(VisualTokens.Colors.Border),
            Child = light
        };
        grip = new Button
        {
            Style = DeskControls.ButtonStyle, Width = 120, Height = 20, MinHeight = 0,
            VerticalAlignment = VerticalAlignment.Top,
            Padding = new Thickness(0), Content = marker,
            Background = System.Windows.Media.Brushes.Transparent,
            BorderThickness = new Thickness(0)
        };
        AutomationProperties.SetName(grip, "Mostrar orilla");
        grip.ToolTip = "Mostrar orilla";
        grip.Click += (_, _) =>
        {
            keepKeyboardFocus = true;
            Reveal();
            bar.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        };

        var trigger = new Border { Height = 22, Background = System.Windows.Media.Brushes.Transparent,
            VerticalAlignment = VerticalAlignment.Top };
        trigger.MouseLeftButtonDown += (_, e) =>
        {
            if (owner.WindowState == WindowState.Normal && e.ButtonState == MouseButtonState.Pressed)
            {
                owner.DragMove();
                e.Handled = true;
            }
        };
        Children.Add(trigger);
        Children.Add(grip);
        Children.Add(bar);
        bar.Visibility = Visibility.Collapsed;
        MouseEnter += (_, _) => Reveal();
        MouseLeave += (_, _) => ScheduleRetreat();
        PreviewMouseDown += (_, _) => keepKeyboardFocus = false;
        PreviewKeyDown += (_, _) => keepKeyboardFocus = true;
        IsKeyboardFocusWithinChanged += (_, _) => ScheduleRetreat();
        retreat.Tick += (_, _) =>
        {
            retreat.Stop();
            if (!IsMouseOver && !(keepKeyboardFocus && IsKeyboardFocusWithin))
            {
                bar.Visibility = Visibility.Collapsed;
                grip.Visibility = Visibility.Visible;
            }
        };
        Unloaded += (_, _) => retreat.Stop();
    }

    public FrameworkElement Grip => grip;

    private void Reveal()
    {
        retreat.Stop();
        if (bar.IsVisible)
        {
            return;
        }

        bar.Visibility = Visibility.Visible;
        if (!grip.IsKeyboardFocused)
        {
            grip.Visibility = Visibility.Hidden;
        }

        if (Theme.AnimationsEnabled)
        {
            var motion = new DoubleAnimationUsingKeyFrames();
            motion.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            motion.KeyFrames.Add(new SplineDoubleKeyFrame(1, KeyTime.FromTimeSpan(VisualTokens.Motion.EdgeBar), Theme.MotionSpline()));
            bar.BeginAnimation(OpacityProperty, motion);
        }
    }

    private void ScheduleRetreat()
    {
        retreat.Stop();
        retreat.Start();
    }
}
