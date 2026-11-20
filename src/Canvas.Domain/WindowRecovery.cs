using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel;

namespace Canvas.Domain;

public sealed record WindowIdentity(long Handle, int ProcessId, long ProcessStartedUtcTicks, int SessionId);
public readonly record struct WindowPoint([property: JsonRequired] int X, [property: JsonRequired] int Y);
public readonly record struct WindowRectangle([property: JsonRequired] int Left, [property: JsonRequired] int Top,
    [property: JsonRequired] int Right, [property: JsonRequired] int Bottom);
public sealed record WindowPlacement(uint Flags, uint ShowCommand, WindowPoint MinPosition,
    WindowPoint MaxPosition, WindowRectangle NormalPosition);
public sealed record WindowRecoveryEntry(long Handle, int ProcessId, long ProcessStartedUtcTicks,
    int SessionId, Guid Marker, WindowPlacement Placement)
{
    internal WindowIdentity Identity => new(Handle, ProcessId, ProcessStartedUtcTicks, SessionId);
}

public interface IWindowPlacementAccess
{
    int SessionId { get; }
    WindowIdentity? Identify(long handle);
    WindowPlacement Capture(long handle);
    void Mark(long handle, Guid marker);
    bool HasMarker(long handle, Guid marker);
    void RemoveMarker(long handle, Guid marker);
    void Restore(long handle, WindowPlacement placement);
}

public sealed class WindowRecovery : IDisposable
{
    private const int Version = 1;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true
    };

    private readonly string path;
    private readonly FileStream gate;
    private readonly IWindowPlacementAccess windows;
    private List<WindowRecoveryEntry> entries;

    public WindowRecovery(string path, IWindowPlacementAccess windows)
    {
        this.path = path;
        this.windows = windows;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        gate = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        try
        {
            entries = Load(path);
        }
        catch
        {
            gate.Dispose();
            throw;
        }
    }

    public static string GetPath(string localData) =>
        Path.Combine(localData, "Escritorio Canvas", "devolucion-ventanas.json");

    public void SaveOriginal(long handle)
    {
        ObjectDisposedException.ThrowIf(gate.SafeFileHandle.IsClosed, this);
        var identity = windows.Identify(handle) ?? throw new InvalidOperationException("The window no longer exists.");
        if (identity.SessionId != windows.SessionId)
        {
            throw new InvalidOperationException("The window belongs to another session.");
        }

        var existing = FindInCurrentSession(handle);
        if (existing is not null && Matches(existing))
        {
            return;
        }

        var marker = Guid.NewGuid();
        windows.Mark(handle, marker);
        var committed = false;
        try
        {
            // Mark first so a recycled handle cannot pair a new window with an older capture.
            var placement = windows.Capture(handle);
            var entry = new WindowRecoveryEntry(handle, identity.ProcessId, identity.ProcessStartedUtcTicks,
                identity.SessionId, marker, placement);
            if (!Valid(entry) || !Matches(entry))
            {
                throw new InvalidOperationException("The original window placement could not be verified.");
            }

            Write([.. entries.Where(current => current != existing), entry]);
            committed = true;
            if (!Matches(entry))
            {
                throw new InvalidOperationException("The window changed before it could be moved.");
            }
        }
        finally
        {
            if (!committed && windows.HasMarker(handle, marker))
            {
                windows.RemoveMarker(handle, marker);
            }
        }
    }

    public void RestoreWindow(long handle)
    {
        ObjectDisposedException.ThrowIf(gate.SafeFileHandle.IsClosed, this);
        var entry = FindInCurrentSession(handle);
        if (entry is null)
        {
            return;
        }

        var matched = Matches(entry);
        if (matched)
        {
            windows.Restore(handle, entry.Placement);
        }

        Write(entries.Where(item => item != entry).ToList());
        if (matched && Matches(entry))
        {
            windows.RemoveMarker(handle, entry.Marker);
        }
    }

    public IReadOnlyList<Exception> RestoreAll()
    {
        ObjectDisposedException.ThrowIf(gate.SafeFileHandle.IsClosed, this);
        var failures = new List<Exception>();
        foreach (var entry in entries.Where(item => item.SessionId == windows.SessionId).ToArray())
        {
            try
            {
                RestoreWindow(entry.Handle);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or Win32Exception or InvalidOperationException)
            {
                failures.Add(error);
            }
        }

        return failures;
    }

    public void Dispose() => gate.Dispose();

    private WindowRecoveryEntry? FindInCurrentSession(long handle) =>
        entries.Find(entry => entry.SessionId == windows.SessionId && entry.Handle == handle);

    private bool Matches(WindowRecoveryEntry entry) =>
        windows.Identify(entry.Handle) == entry.Identity && windows.HasMarker(entry.Handle, entry.Marker);

    private static List<WindowRecoveryEntry> Load(string path)
    {
        FileStream stream;
        try
        {
            stream = File.OpenRead(path);
        }
        catch (FileNotFoundException)
        {
            return [];
        }

        using (stream)
        {
            try
            {
                var document = JsonSerializer.Deserialize<Journal>(stream, JsonOptions);
                if (document is null || document.Version != Version || document.Windows is null ||
                    document.Windows.Any(entry => !Valid(entry)) ||
                    document.Windows.Select(entry => (entry.SessionId, entry.Handle)).Distinct().Count() != document.Windows.Count)
                {
                    throw new InvalidDataException("Invalid or unsupported window recovery journal.");
                }

                return document.Windows;
            }
            catch (JsonException error)
            {
                throw new InvalidDataException("The window recovery journal is unreadable.", error);
            }
        }
    }

    private static bool Valid(WindowRecoveryEntry? entry) =>
        entry is { Handle: > 0 and <= uint.MaxValue, ProcessId: > 0,
            ProcessStartedUtcTicks: > 0 and <= 3155378975999999999L, SessionId: >= 0,
            Placement: { ShowCommand: 1 or 2 or 3 } placement } &&
        entry.Marker != Guid.Empty && (placement.Flags & ~3u) == 0 &&
        placement.NormalPosition is var rect && rect.Right > rect.Left && rect.Bottom > rect.Top;

    private void Write(List<WindowRecoveryEntry> replacement)
    {
        var temporary = path + ".tmp";
        using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            JsonSerializer.Serialize(stream, new Journal(Version, replacement), JsonOptions);
            stream.Flush(flushToDisk: true);
        }

        File.Move(temporary, path, overwrite: true);
        entries = replacement;
    }

    private sealed record Journal(int Version, List<WindowRecoveryEntry> Windows);
}
