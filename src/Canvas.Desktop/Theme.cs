using Canvas.Domain;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Palette = Canvas.Domain.VisualTokens.Colors;

namespace Canvas.Desktop;

/// <summary>Resource keys of the "Mesa de luz" visual system, shared by every WPF element.</summary>
public static class ThemeKeys
{
    public const string Font = "Theme.Font";
    public const string Night = "Theme.Night";
    public const string Slate = "Theme.Slate";
    public const string Raised = "Theme.Raised";
    public const string Border = "Theme.Border";
    public const string Bone = "Theme.Bone";
    public const string Ash = "Theme.Ash";
    public const string Faint = "Theme.Faint";
    public const string Danger = "Theme.Danger";
    public const string Warning = "Theme.Warning";
    public const string WallpaperVeil = "Theme.WallpaperVeil";
    /// <summary>Light of the active space. Animated when the space changes.</summary>
    public const string Light = "Theme.Light";
    public const string LightWash = "Theme.LightWash";
    public const string LightHalo = "Theme.LightHalo";
}

/// <summary>Turns <see cref="VisualTokens"/> into WPF brushes, fonts and animations.</summary>
public static class Theme
{
    private static readonly Uri FontBase = new("pack://application:,,,/");

    /// <summary>Geist from the app package, with Segoe UI Variable as the fallback if the file is missing.</summary>
    public static FontFamily Font { get; } = new(FontBase, $"./Assets/Fonts/#{VisualTokens.Type.FamilyName}, Segoe UI Variable Text, Segoe UI");

    public static Color ToColor(TokenColor color) => Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);

    public static KeySpline MotionSpline()
    {
        var (x1, y1, x2, y2) = VisualTokens.Motion.Curve;
        return new KeySpline(x1, y1, x2, y2);
    }

    /// <summary>Windows "Show animations" setting. When off, transitions apply instantly.</summary>
    public static bool AnimationsEnabled => SystemParameters.ClientAreaAnimation;

    public static ResourceDictionary CreateResources(SpaceLight light)
    {
        ArgumentNullException.ThrowIfNull(light);
        var resources = new ResourceDictionary
        {
            [ThemeKeys.Font] = Font,
            [ThemeKeys.Night] = Frozen(Palette.Night),
            [ThemeKeys.Slate] = Frozen(Palette.Slate),
            [ThemeKeys.Raised] = Frozen(Palette.Raised),
            [ThemeKeys.Border] = Frozen(Palette.Border),
            [ThemeKeys.Bone] = Frozen(Palette.Bone),
            [ThemeKeys.Ash] = Frozen(Palette.Ash),
            [ThemeKeys.Faint] = Frozen(Palette.Faint),
            [ThemeKeys.Danger] = Frozen(Palette.Danger),
            [ThemeKeys.Warning] = Frozen(Palette.Warning),
            [ThemeKeys.WallpaperVeil] = Frozen(Palette.Night.WithAlpha(VisualTokens.WallpaperVeilOpacity)),
            // Light brushes stay unfrozen so SetLight can animate them in place.
            [ThemeKeys.Light] = new SolidColorBrush(ToColor(light.Color)),
            [ThemeKeys.LightWash] = new SolidColorBrush(ToColor(light.Color.WithAlpha(VisualTokens.LightWashOpacity))),
            [ThemeKeys.LightHalo] = new SolidColorBrush(ToColor(light.Color.WithAlpha(VisualTokens.LightHaloOpacity)))
        };
        return resources;
    }

    /// <summary>Changes the light of the active space, fading it over <see cref="VisualTokens.Motion.SpaceChange"/>.</summary>
    public static void SetLight(ResourceDictionary resources, SpaceLight light)
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(light);
        Fade(resources, ThemeKeys.Light, light.Color);
        Fade(resources, ThemeKeys.LightWash, light.Color.WithAlpha(VisualTokens.LightWashOpacity));
        Fade(resources, ThemeKeys.LightHalo, light.Color.WithAlpha(VisualTokens.LightHaloOpacity));
    }

    private static void Fade(ResourceDictionary resources, string key, TokenColor target)
    {
        if (resources[key] is not SolidColorBrush brush || brush.IsFrozen)
        {
            throw new InvalidOperationException($"The theme resource '{key}' must be an unfrozen SolidColorBrush.");
        }

        var to = ToColor(target);
        if (!AnimationsEnabled)
        {
            brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            brush.Color = to;
            return;
        }

        var animation = new ColorAnimationUsingKeyFrames { FillBehavior = FillBehavior.HoldEnd };
        animation.KeyFrames.Add(new SplineColorKeyFrame(to, KeyTime.FromTimeSpan(VisualTokens.Motion.SpaceChange), MotionSpline()));
        brush.BeginAnimation(SolidColorBrush.ColorProperty, animation, HandoffBehavior.SnapshotAndReplace);
    }

    private static SolidColorBrush Frozen(TokenColor color)
    {
        var brush = new SolidColorBrush(ToColor(color));
        brush.Freeze();
        return brush;
    }
}
