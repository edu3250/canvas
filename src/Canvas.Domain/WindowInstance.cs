namespace Canvas.Domain;

public sealed class WindowInstance : IDisposable
{
    private readonly IWindowPlacementAccess windows;
    private readonly WindowIdentity identity;
    private readonly Guid marker;
    private bool disposed;

    private WindowInstance(IWindowPlacementAccess windows, WindowIdentity identity, Guid marker)
    {
        this.windows = windows;
        this.identity = identity;
        this.marker = marker;
    }

    public static WindowInstance? Track(IWindowPlacementAccess windows, WindowIdentity identity)
    {
        if (windows.Identify(identity.Handle) != identity)
        {
            return null;
        }

        var marker = Guid.NewGuid();
        windows.Mark(identity.Handle, marker);
        var instance = new WindowInstance(windows, identity, marker);
        if (instance.IsCurrent())
        {
            return instance;
        }

        instance.Dispose();
        return null;
    }

    internal bool Matches(WindowIdentity candidate) => identity == candidate && IsCurrent();

    public bool IsCurrent() => !disposed && windows.Identify(identity.Handle) == identity &&
        windows.HasMarker(identity.Handle, marker);

    public void Dispose()
    {
        if (!disposed && windows.HasMarker(identity.Handle, marker))
        {
            windows.RemoveMarker(identity.Handle, marker);
        }

        disposed = true;
    }
}
