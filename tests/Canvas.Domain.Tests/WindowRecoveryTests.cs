using Canvas.Domain;
using Xunit;

namespace Canvas.Domain.Tests;

public sealed class WindowRecoveryTests : IDisposable
{
    private readonly DirectoryInfo directory = Directory.CreateTempSubdirectory("canvas-recovery-");
    private readonly FakeWindows windows = new();

    private string Journal => WindowRecovery.GetPath(directory.FullName);

    [Fact]
    public void FirstCaptureSurvivesReopeningAndSubsequentMoves()
    {
        using (var recovery = Open())
        {
            recovery.SaveOriginal(100);
            Assert.True(File.Exists(Journal));
            windows.Placement = windows.Placement with { ShowCommand = 3 };
            recovery.SaveOriginal(100);
            Assert.Equal(1, windows.CaptureCount);
        }

        using var retry = Open();
        Assert.Empty(retry.RestoreAll());
        Assert.Equal(1u, windows.Placement.ShowCommand);
        Assert.Empty(windows.Markers);
        Assert.Empty(retry.RestoreAll());
        Assert.Equal(1, windows.RestoreCount);
    }

    [Fact]
    public void IndividualRestoreRetainsOtherWindows()
    {
        using var recovery = Open();
        recovery.SaveOriginal(100);
        recovery.SaveOriginal(200);
        recovery.RestoreWindow(100);
        Assert.Equal([200L], windows.Markers.Keys);
        Assert.Empty(recovery.RestoreAll());
        Assert.Equal(2, windows.RestoreCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MissingMarkerOrChangedProcessCannotRestoreReusedHandle(bool removeMarker)
    {
        using var recovery = Open();
        recovery.SaveOriginal(100);
        if (removeMarker)
        {
            windows.Markers.Clear();
        }
        else
        {
            windows.Started++;
        }

        Assert.Empty(recovery.RestoreAll());
        Assert.Equal(0, windows.RestoreCount);
        Assert.DoesNotContain("\"handle\"", File.ReadAllText(Journal));
    }

    [Fact]
    public void OneFailedReturnDoesNotPreventOthersAndRemainsPending()
    {
        using (var recovery = Open())
        {
            recovery.SaveOriginal(100);
            recovery.SaveOriginal(200);
            windows.FailRestore = 100;
            Assert.Single(recovery.RestoreAll());
            Assert.Equal([100L], windows.Markers.Keys);
        }

        windows.FailRestore = 0;
        using var retry = Open();
        Assert.Empty(retry.RestoreAll());
        Assert.Equal(2, windows.RestoreCount);
    }

    [Fact]
    public void FailedCommitKeepsJournalAndRemovesUncommittedMark()
    {
        using var recovery = Open();
        recovery.SaveOriginal(100);
        var original = File.ReadAllText(Journal);
        Directory.CreateDirectory(Journal + ".tmp");

        Assert.ThrowsAny<Exception>(() => recovery.SaveOriginal(200));
        Assert.Equal(original, File.ReadAllText(Journal));
        Assert.Equal([100L], windows.Markers.Keys);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("{\"version\":2,\"windows\":[]}")]
    [InlineData("{\"version\":1,\"windows\":[{}]}")]
    public void UnreadableJournalIsPreservedAndLockReleased(string data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Journal)!);
        File.WriteAllText(Journal, data);
        Assert.Throws<InvalidDataException>(() => Open());
        Assert.Equal(data, File.ReadAllText(Journal));

        File.Delete(Journal);
        using var reopened = Open();
        Assert.Empty(reopened.RestoreAll());
    }

    [Fact]
    public void OnlyOneOwnerMayOpenJournal()
    {
        using var owned = Open();
        Assert.Throws<IOException>(() => Open());
    }

    private WindowRecovery Open() => new(Journal, windows);

    public void Dispose() => directory.Delete(recursive: true);

    private sealed class FakeWindows : IWindowPlacementAccess
    {
        public WindowPlacement Placement { get; set; } =
            new(0, 1, default, default, new WindowRectangle(24, 80, 700, 540));

        public Dictionary<long, Guid> Markers { get; } = [];
        public long Started { get; set; } = 300;
        public long FailRestore { get; set; }
        public int CaptureCount { get; private set; }
        public int RestoreCount { get; private set; }

        public WindowIdentity Identify(long handle) => new(handle, 12, Started, 1);

        public WindowPlacement Capture(long handle)
        {
            CaptureCount++;
            return Placement;
        }

        public void Mark(long handle, Guid marker) => Markers.Add(handle, marker);

        public bool HasMarker(long handle, Guid marker) =>
            Markers.TryGetValue(handle, out var value) && value == marker;

        public void RemoveMarker(long handle, Guid marker) => Markers.Remove(handle);

        public void Restore(long handle, WindowPlacement placement)
        {
            if (handle == FailRestore)
            {
                throw new IOException("Window could not be restored.");
            }

            Placement = placement;
            RestoreCount++;
        }
    }
}
