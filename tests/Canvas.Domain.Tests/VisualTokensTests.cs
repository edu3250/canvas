using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public sealed class VisualTokensTests
{
    private static readonly TokenColor Night = VisualTokens.Colors.Night;

    [Theory]
    [InlineData("#141826", 0x14, 0x18, 0x26, 255)]
    [InlineData("E8E4D8", 0xE8, 0xE4, 0xD8, 255)]
    [InlineData("#7CD6E880", 0x7C, 0xD6, 0xE8, 0x80)]
    public void FromHexReadsSixAndEightDigitColors(string hex, int r, int g, int b, int a)
    {
        Assert.Equal(new TokenColor((byte)r, (byte)g, (byte)b, (byte)a), TokenColor.FromHex(hex));
    }

    [Theory]
    [InlineData("")]
    [InlineData("#12345")]
    [InlineData("#GGGGGG")]
    public void FromHexRejectsMalformedColors(string hex)
    {
        Assert.Throws<FormatException>(() => TokenColor.FromHex(hex));
    }

    [Fact]
    public void ToStringRoundTrips()
    {
        Assert.Equal("#141826", Night.ToString());
        Assert.Equal("#14182680", Night.WithAlpha(128 / 255.0).ToString());
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    public void WithAlphaRejectsOpacityOutsideRange(double opacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Night.WithAlpha(opacity));
    }

    [Fact]
    public void OverCompositesOnBackground()
    {
        var white = new TokenColor(255, 255, 255);
        var black = new TokenColor(0, 0, 0);
        Assert.Equal(new TokenColor(128, 128, 128), white.WithAlpha(128 / 255.0).Over(black));
        Assert.Equal(white, white.Over(black));
    }

    [Fact]
    public void ContrastRatioMatchesWcagExtremes()
    {
        Assert.Equal(21, TokenColor.ContrastRatio(new(255, 255, 255), new(0, 0, 0)), 3);
        Assert.Equal(1, TokenColor.ContrastRatio(Night, Night), 3);
    }

    [Fact]
    public void PrimaryTextIsReadableOnEverySurface()
    {
        foreach (var surface in new[] { Night, VisualTokens.Colors.Slate, VisualTokens.Colors.Raised })
        {
            Assert.True(TokenColor.ContrastRatio(VisualTokens.Colors.Bone, surface) >= 7, $"Hueso sobre {surface}");
        }
    }

    [Fact]
    public void SecondaryTextIsReadableOnNightAndSlate()
    {
        Assert.True(TokenColor.ContrastRatio(VisualTokens.Colors.Ash, Night) >= 4.5);
        Assert.True(TokenColor.ContrastRatio(VisualTokens.Colors.Ash, VisualTokens.Colors.Slate) >= 4.5);
    }

    [Fact]
    public void EveryLightReadsOnNightAndCarriesNightText()
    {
        foreach (var light in VisualTokens.SpaceLights)
        {
            // The light draws outlines on the canvas and fills primary buttons with night-coloured text.
            Assert.True(TokenColor.ContrastRatio(light.Color, Night) >= 4.5, light.DisplayName);
        }
    }

    [Fact]
    public void LightsHaveUniqueIdsAndNames()
    {
        Assert.Equal(6, VisualTokens.SpaceLights.Select(l => l.Id).Distinct().Count());
        Assert.Equal(6, VisualTokens.SpaceLights.Select(l => l.DisplayName).Distinct().Count());
    }

    [Fact]
    public void FindLightFallsBackToDefault()
    {
        Assert.Equal("Brasa", VisualTokens.FindLight("ember").DisplayName);
        Assert.Same(VisualTokens.DefaultLight, VisualTokens.FindLight("missing"));
    }

    [Fact]
    public void WallpaperVeilKeepsLightVisibleOverBrightImage()
    {
        var snow = new TokenColor(235, 238, 245);
        var veiled = Night.WithAlpha(VisualTokens.WallpaperVeilOpacity).Over(snow);
        Assert.True(TokenColor.ContrastRatio(VisualTokens.Colors.Bone, veiled) < TokenColor.ContrastRatio(VisualTokens.Colors.Bone, Night));
        Assert.True(veiled.RelativeLuminance() < snow.RelativeLuminance());
    }

    [Fact]
    public void MotionDurationsStayShortAndOrdered()
    {
        Assert.True(VisualTokens.Motion.Menu < VisualTokens.Motion.EdgeBar);
        Assert.True(VisualTokens.Motion.EdgeBar < VisualTokens.Motion.Panel);
        Assert.True(VisualTokens.Motion.Panel < VisualTokens.Motion.SpaceChange);
        Assert.True(VisualTokens.Motion.SpaceChange <= TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public void RadiusGrowsWithObjectSize()
    {
        Assert.True(VisualTokens.Radius.Island > VisualTokens.Radius.Panel);
        Assert.True(VisualTokens.Radius.Panel > VisualTokens.Radius.Handle);
        Assert.True(VisualTokens.Radius.Handle > VisualTokens.Radius.Control);
    }
}
