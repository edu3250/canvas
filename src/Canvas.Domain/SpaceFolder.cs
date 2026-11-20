using System.IO;

namespace Canvas.Domain;

public static class SpaceFolder
{
    public static string GetPath(string profilePath, SpaceId space) =>
        Path.Combine(profilePath, "Escritorio Canvas", space.Value);

    public static string[] ReadItems(string path) =>
        Directory.EnumerateFileSystemEntries(path, "*", new EnumerationOptions
        {
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,
            IgnoreInaccessible = false
        }).Order(StringComparer.OrdinalIgnoreCase).ThenBy(item => item, StringComparer.Ordinal).ToArray();
}
