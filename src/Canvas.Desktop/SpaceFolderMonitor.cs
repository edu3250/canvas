using Canvas.Domain;
using System.Collections.Concurrent;
using System.IO;

namespace Canvas.Desktop;

internal sealed class SpaceFolderMonitor(string path) : IDisposable
{
    private readonly ConcurrentQueue<(string OldPath, string NewPath)> renames = new();
    private FileSystemWatcher? watcher;

    public string Path { get; } = path;

    public (string[] Paths, (string OldPath, string NewPath)[] Renames) ReadSnapshot()
    {
        var paths = ReadItems();
        var changes = TakeRenames();
        if (changes.Length != 0)
        {
            // A rename may have occurred during enumeration.
            paths = ReadItems();
        }

        return (paths, changes);
    }

    private string[] ReadItems()
    {
        if (watcher is null)
        {
            Directory.CreateDirectory(Path);
            var next = new FileSystemWatcher(Path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            next.Renamed += (_, e) => renames.Enqueue((e.OldFullPath, e.FullPath));
            try
            {
                next.EnableRaisingEvents = true;
                watcher = next;
            }
            catch
            {
                next.Dispose();
                throw;
            }
        }

        try
        {
            return SpaceFolder.ReadItems(Path);
        }
        catch (DirectoryNotFoundException)
        {
            Dispose();
            return [];
        }
    }

    private (string OldPath, string NewPath)[] TakeRenames()
    {
        var changes = new List<(string, string)>();
        while (renames.TryDequeue(out var change))
        {
            changes.Add(change);
        }

        return changes.ToArray();
    }

    public void Dispose()
    {
        watcher?.Dispose();
        watcher = null;
    }
}
