using System.Collections.ObjectModel;
using System.Globalization;

namespace Canvas.Domain;

/// <summary>An opaque or translucent sRGB color stored as bytes, independent of any UI toolkit.</summary>
public readonly record struct TokenColor(byte Red, byte Green, byte Blue, byte Alpha = 255)
{
    public static TokenColor FromHex(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);
        var digits = hex.StartsWith('#') ? hex[1..] : hex;
        if (digits.Length is not (6 or 8) || !uint.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"'{hex}' is not a #RRGGBB or #RRGGBBAA color.");
        }

        return digits.Length == 6
            ? new TokenColor((byte)(value >> 16), (byte)(value >> 8), (byte)value)
            : new TokenColor((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }

    public TokenColor WithAlpha(double opacity)
    {
        if (!double.IsFinite(opacity) || opacity < 0 || opacity > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(opacity));
        }

        return this with { Alpha = (byte)Math.Round(opacity * 255) };
    }

    /// <summary>Composites this color over an opaque background, as the screen would show it.</summary>
    public TokenColor Over(TokenColor background)
    {
        var a = Alpha / 255.0;
        static byte Blend(byte top, byte bottom, double alpha) => (byte)Math.Round(top * alpha + bottom * (1 - alpha));
        return new TokenColor(Blend(Red, background.Red, a), Blend(Green, background.Green, a), Blend(Blue, background.Blue, a));
    }

    /// <summary>WCAG 2.x relative luminance of the opaque color.</summary>
    public double RelativeLuminance()
    {
        static double Channel(byte value)
        {
            var c = value / 255.0;
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(Red) + 0.7152 * Channel(Green) + 0.0722 * Channel(Blue);
    }

    /// <summary>WCAG 2.x contrast ratio between two opaque colors, from 1 to 21.</summary>
    public static double ContrastRatio(TokenColor first, TokenColor second)
    {
        var a = first.RelativeLuminance();
        var b = second.RelativeLuminance();
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    public override string ToString() => Alpha == 255
        ? $"#{Red:X2}{Green:X2}{Blue:X2}"
        : $"#{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2}";
}

/// <summary>A light that marks the active panel of a space. Users pick one per space from <see cref="VisualTokens.SpaceLights"/>.</summary>
public sealed record SpaceLight(string Id, string DisplayName, TokenColor Color);

/// <summary>Values of the "Mesa de luz" visual system (docs/sistema-visual/mesa-de-luz.html).</summary>
public static class VisualTokens
{
    public static class Colors
    {
        /// <summary>Canvas and app background.</summary>
        public static readonly TokenColor Night = TokenColor.FromHex("#141826");
        /// <summary>Panel handles, edge bar and floating islands.</summary>
        public static readonly TokenColor Slate = TokenColor.FromHex("#1E2233");
        /// <summary>Hover, menus and secondary buttons.</summary>
        public static readonly TokenColor Raised = TokenColor.FromHex("#2A2F44");
        public static readonly TokenColor Border = TokenColor.FromHex("#363C55");
        /// <summary>Primary text.</summary>
        public static readonly TokenColor Bone = TokenColor.FromHex("#E8E4D8");
        /// <summary>Secondary text.</summary>
        public static readonly TokenColor Ash = TokenColor.FromHex("#9A9EB2");
        /// <summary>Hints and disabled detail.</summary>
        public static readonly TokenColor Faint = TokenColor.FromHex("#6A6F86");
        public static readonly TokenColor Danger = TokenColor.FromHex("#FF6B6B");
        public static readonly TokenColor Warning = TokenColor.FromHex("#F5C46B");
    }

    /// <summary>Opacity of the night veil laid over a user wallpaper.</summary>
    public const double WallpaperVeilOpacity = 0.42;
    /// <summary>Opacity of a light used as a soft wash (selection, pool under the active panel).</summary>
    public const double LightWashOpacity = 0.16;
    /// <summary>Opacity of a light used as a halo or outline around the active panel.</summary>
    public const double LightHaloOpacity = 0.30;
    /// <summary>Brightness multiplier of panels at rest, so the active one stands out.</summary>
    public const double RestingPanelBrightness = 0.96;

    public static readonly ReadOnlyCollection<SpaceLight> SpaceLights = new(
    [
        new("glacier", "Glaciar", TokenColor.FromHex("#7CD6E8")),
        new("ember", "Brasa", TokenColor.FromHex("#FF7A9C")),
        new("lime", "Lima", TokenColor.FromHex("#C4E27A")),
        new("lavender", "Lavanda", TokenColor.FromHex("#B7A6FF")),
        new("honey", "Miel", TokenColor.FromHex("#F5C46B")),
        new("mint", "Menta", TokenColor.FromHex("#6FE3B5"))
    ]);

    public static SpaceLight DefaultLight => SpaceLights[0];

    public static SpaceLight FindLight(string id) =>
        SpaceLights.FirstOrDefault(light => light.Id == id) ?? DefaultLight;

    public static class Type
    {
        public const string FamilyName = "Geist";
        public const double Display = 48;
        public const double Title = 30;
        public const double Heading = 19;
        public const double Body = 14;
        public const double Interface = 13;
        public const double Detail = 12;
        public const double Caption = 11;
        /// <summary>Letter spacing for display and title text, in thousandths of an em (WPF units).</summary>
        public const int DisplayTracking = -40;
        public const int TitleTracking = -30;
    }

    public static class Radius
    {
        public const double Island = 12;
        public const double Panel = 10;
        public const double Handle = 8;
        public const double Control = 7;
    }

    public static class Size
    {
        public const double CompactControl = 28;
        public const double Control = 32;
        public const double DialogControl = 40;
        public const double PanelHandle = 30;
    }

    public static readonly ReadOnlyCollection<double> Spacing = new([4, 8, 12, 16, 24, 32, 48]);

    public static class Motion
    {
        public static readonly TimeSpan SpaceChange = TimeSpan.FromMilliseconds(420);
        public static readonly TimeSpan Panel = TimeSpan.FromMilliseconds(220);
        public static readonly TimeSpan EdgeBar = TimeSpan.FromMilliseconds(180);
        public static readonly TimeSpan Menu = TimeSpan.FromMilliseconds(140);

        /// <summary>Control points of the single easing curve used everywhere: cubic-bezier(0.2, 0.7, 0.2, 1).</summary>
        public static readonly (double X1, double Y1, double X2, double Y2) Curve = (0.2, 0.7, 0.2, 1);
    }
}
