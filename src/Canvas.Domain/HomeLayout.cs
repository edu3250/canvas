using System.Text.Encodings.Web;
using System.Text.Json;

namespace Canvas.Domain;

/// <summary>
/// Persists where each Home icon sits, per space, keyed by the item's file name.
/// The file only stores positions: losing or damaging it never touches the space folders.
/// </summary>
public static class HomeLayout
{
    public const int Version = 1;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        // Keep space and file names such as "Programación" readable in the file.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string GetPath(string localAppDataPath) =>
        Path.Combine(localAppDataPath, "Escritorio Canvas", "posiciones.json");

    /// <summary>
    /// Reads the saved layout. A missing file yields an empty layout; an unreadable or
    /// damaged file yields an empty layout with <c>Damaged</c> set. Invalid entries are skipped.
    /// </summary>
    public static (Dictionary<SpaceId, Dictionary<string, Point>> Layouts, bool Damaged) Load(string path)
    {
        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (Exception error) when (error is FileNotFoundException or DirectoryNotFoundException)
        {
            return ([], false);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return ([], true);
        }

        LayoutFile? file;
        try
        {
            file = JsonSerializer.Deserialize<LayoutFile>(json, Options);
        }
        catch (JsonException)
        {
            return ([], true);
        }

        if (file is not { Version: Version } || file.Spaces is null)
        {
            return ([], true);
        }

        var layouts = new Dictionary<SpaceId, Dictionary<string, Point>>();
        foreach (var (space, items) in file.Spaces)
        {
            if (string.IsNullOrEmpty(space) || items is null)
            {
                continue;
            }

            var layout = new Dictionary<string, Point>(StringComparer.OrdinalIgnoreCase);
            var cells = new HashSet<Point>();
            foreach (var (name, saved) in items)
            {
                if (!IsValidName(name) || saved is null)
                {
                    continue;
                }

                var cell = new Point(saved.X, saved.Y);
                if (IsGridCell(cell) && cells.Add(cell))
                {
                    layout.TryAdd(name, cell);
                }
            }

            layouts.TryAdd(new SpaceId(space), layout);
        }

        return (layouts, false);
    }

    /// <summary>
    /// Writes the layout through a temporary file so an interrupted save leaves the previous file intact.
    /// </summary>
    public static void Save(string path, IReadOnlyDictionary<SpaceId, Dictionary<string, Point>> layouts)
    {
        ArgumentNullException.ThrowIfNull(layouts);
        var file = new LayoutFile(Version, layouts
            .OrderBy(entry => entry.Key.Value, StringComparer.Ordinal)
            .ToDictionary(
                entry => entry.Key.Value,
                entry => (Dictionary<string, SavedPoint?>?)entry.Value
                    .OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(item => item.Key, item => (SavedPoint?)new SavedPoint(item.Value.X, item.Value.Y))));

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(file, Options));
        File.Move(temporary, path, overwrite: true);
    }

    /// <summary>
    /// Chooses a cell for each new item: its saved cell when that cell is still valid and free,
    /// otherwise the first free cell, in the order the names are given.
    /// </summary>
    public static Dictionary<string, Point> Place(
        IReadOnlySet<Point> occupied, IReadOnlyList<string> names, IReadOnlyDictionary<string, Point> saved)
    {
        ArgumentNullException.ThrowIfNull(occupied);
        ArgumentNullException.ThrowIfNull(names);
        ArgumentNullException.ThrowIfNull(saved);

        var taken = new HashSet<Point>(occupied);
        var placed = new Dictionary<string, Point>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in names)
        {
            if (saved.TryGetValue(name, out var cell) && IsGridCell(cell) && taken.Add(cell))
            {
                placed.TryAdd(name, cell);
            }
        }

        using var freeCells = HomeGrid.FreeCells(taken).GetEnumerator();
        foreach (var name in names.Where(name => !placed.ContainsKey(name)))
        {
            freeCells.MoveNext();
            placed.Add(name, freeCells.Current);
        }

        return placed;
    }

    private static bool IsValidName(string name) =>
        !string.IsNullOrWhiteSpace(name) && name.IndexOfAny(['\\', '/']) < 0;

    private static bool IsGridCell(Point cell) =>
        double.IsFinite(cell.X) && double.IsFinite(cell.Y) &&
        HomeGrid.Clamp(cell) == cell &&
        (cell.X - HomeGrid.FirstX) % HomeGrid.Spacing == 0 &&
        (cell.Y - HomeGrid.FirstY) % HomeGrid.Spacing == 0;

    private sealed record LayoutFile(int Version, Dictionary<string, Dictionary<string, SavedPoint?>?>? Spaces);

    private sealed record SavedPoint(double X, double Y);
}
