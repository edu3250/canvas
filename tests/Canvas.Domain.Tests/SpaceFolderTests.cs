using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public class SpaceFolderTests
{
    [Fact]
    public void ResolvesEachSpaceUnderTheCanvasFolder()
    {
        var profile = Path.GetTempPath();
        Assert.Equal(Path.Combine(profile, "Escritorio Canvas", "Programación"),
            SpaceFolder.GetPath(profile, new SpaceId("Programación")));
        Assert.Equal(Path.Combine(profile, "Escritorio Canvas", "Juegos"),
            SpaceFolder.GetPath(profile, new SpaceId("Juegos")));
    }

    [Fact]
    public void ListsVisibleFilesAndFoldersAtTheTopLevel()
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var folder = directory.CreateSubdirectory("A");
            File.WriteAllText(Path.Combine(folder.FullName, "nested.txt"), "");
            var file = Path.Combine(directory.FullName, "b.txt");
            File.WriteAllText(file, "");
            if (OperatingSystem.IsWindows())
            {
                var hidden = Path.Combine(directory.FullName, "hidden.txt");
                var system = Path.Combine(directory.FullName, "system.txt");
                File.WriteAllText(hidden, "");
                File.WriteAllText(system, "");
                File.SetAttributes(hidden, FileAttributes.Hidden);
                File.SetAttributes(system, FileAttributes.System);
            }

            Assert.Equal([folder.FullName, file], SpaceFolder.ReadItems(directory.FullName));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
