using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public sealed class HomeLayoutTests : IDisposable
{
    private static readonly SpaceId Programming = new("Programación");
    private static readonly SpaceId Games = new("Juegos");
    private readonly DirectoryInfo directory = Directory.CreateTempSubdirectory();

    private string FilePath => HomeLayout.GetPath(directory.FullName);

    public void Dispose() => directory.Delete(recursive: true);

    [Fact]
    public void StoresTheFileUnderTheLocalApplicationData()
    {
        Assert.Equal(Path.Combine(directory.FullName, "Escritorio Canvas", "posiciones.json"), FilePath);
    }

    [Fact]
    public void MissingFileYieldsAnEmptyLayoutThatIsNotDamaged()
    {
        var (layouts, damaged) = HomeLayout.Load(FilePath);

        Assert.Empty(layouts);
        Assert.False(damaged);
    }

    [Fact]
    public void SavedLayoutRoundTripsWithAccentsAndIgnoresCase()
    {
        HomeLayout.Save(FilePath, new Dictionary<SpaceId, Dictionary<string, Point>>
        {
            [Programming] = new() { ["VS Code.lnk"] = new Point(120, 176), ["Notas.txt"] = new Point(24, 80) },
            [Games] = new() { ["Steam.lnk"] = new Point(312, 272) }
        });

        var (layouts, damaged) = HomeLayout.Load(FilePath);

        Assert.False(damaged);
        Assert.Equal(new Point(120, 176), layouts[Programming]["vs code.LNK"]);
        Assert.Equal(new Point(24, 80), layouts[Programming]["Notas.txt"]);
        Assert.Equal(new Point(312, 272), layouts[Games]["Steam.lnk"]);
        Assert.Contains("Programación", File.ReadAllText(FilePath));
        Assert.False(File.Exists(FilePath + ".tmp"));
    }

    [Fact]
    public void SaveReplacesThePreviousFile()
    {
        HomeLayout.Save(FilePath, new Dictionary<SpaceId, Dictionary<string, Point>>
        {
            [Games] = new() { ["Steam.lnk"] = new Point(24, 80) }
        });
        HomeLayout.Save(FilePath, new Dictionary<SpaceId, Dictionary<string, Point>>
        {
            [Games] = new() { ["Discord.lnk"] = new Point(120, 80) }
        });

        var (layouts, _) = HomeLayout.Load(FilePath);

        Assert.Equal(["Discord.lnk"], layouts[Games].Keys);
    }

    [Theory]
    [InlineData("{ esto no es json")]
    [InlineData("""{ "version": 2, "spaces": {} }""")]
    [InlineData("""{ "version": 1 }""")]
    [InlineData("null")]
    public void DamagedFileYieldsAnEmptyLayoutMarkedDamaged(string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, content);

        var (layouts, damaged) = HomeLayout.Load(FilePath);

        Assert.Empty(layouts);
        Assert.True(damaged);
    }

    [Fact]
    public void InvalidEntriesAreSkippedAndTheRestKept()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, """
            {
              "version": 1,
              "spaces": {
                "Programación": {
                  "valido.txt": { "x": 24, "y": 80 },
                  "fuera.txt": { "x": 408, "y": 80 },
                  "desalineado.txt": { "x": 30, "y": 80 },
                  "encima.txt": { "x": 24, "y": 80 },
                  "ruta\\anidada.txt": { "x": 120, "y": 80 },
                  "nulo.txt": null
                },
                "Juegos": null
              }
            }
            """);

        var (layouts, damaged) = HomeLayout.Load(FilePath);

        Assert.False(damaged);
        Assert.Equal(["valido.txt"], layouts[Programming].Keys);
        Assert.False(layouts.ContainsKey(Games));
    }

    [Fact]
    public void PlaceUsesSavedCellsFirstAndFillsTheRestInOrder()
    {
        var saved = new Dictionary<string, Point>(StringComparer.OrdinalIgnoreCase)
        {
            ["b.txt"] = new Point(24, 80),
            ["c.txt"] = new Point(312, 176)
        };

        var placed = HomeLayout.Place(new HashSet<Point>(), ["a.txt", "b.txt", "c.txt"], saved);

        Assert.Equal(new Point(24, 80), placed["b.txt"]);
        Assert.Equal(new Point(312, 176), placed["c.txt"]);
        Assert.Equal(new Point(120, 80), placed["a.txt"]);
    }

    [Fact]
    public void PlaceFallsBackToAFreeCellWhenTheSavedOneIsTakenOrInvalid()
    {
        var saved = new Dictionary<string, Point>
        {
            ["ocupado.txt"] = new Point(24, 80),
            ["invalido.txt"] = new Point(25, 80),
            ["repetido.txt"] = new Point(120, 80),
            ["tambien.txt"] = new Point(120, 80)
        };

        var placed = HomeLayout.Place(new HashSet<Point> { new(24, 80) },
            ["ocupado.txt", "invalido.txt", "repetido.txt", "tambien.txt"], saved);

        Assert.Equal(new Point(120, 80), placed["repetido.txt"]);
        Assert.Equal(new Point(216, 80), placed["ocupado.txt"]);
        Assert.Equal(new Point(312, 80), placed["invalido.txt"]);
        Assert.Equal(new Point(24, 176), placed["tambien.txt"]);
        Assert.Equal(4, placed.Values.Distinct().Count());
    }
}
