using System.Collections.ObjectModel;

namespace Canvas.Domain;

public readonly record struct SpaceId(string Value);

public sealed record SpaceNavigation(Camera Camera, Camera HomeCamera, Camera? ReturnView);

public sealed class SessionState
{
    public SessionState(SpaceId activeSpaceId, IEnumerable<KeyValuePair<SpaceId, SpaceNavigation>> spaces)
    {
        ArgumentNullException.ThrowIfNull(spaces);

        var snapshot = new Dictionary<SpaceId, SpaceNavigation>();
        foreach (var (id, navigation) in spaces)
        {
            if (string.IsNullOrEmpty(id.Value))
            {
                throw new ArgumentException("Space identifiers must not be empty.", nameof(spaces));
            }

            ArgumentNullException.ThrowIfNull(navigation);
            ValidateCamera(navigation.Camera);
            ValidateCamera(navigation.HomeCamera);
            if (navigation.ReturnView is Camera returnView)
            {
                ValidateCamera(returnView);
            }

            if (!snapshot.TryAdd(id, navigation))
            {
                throw new ArgumentException("Space identifiers must be unique.", nameof(spaces));
            }
        }

        if (!snapshot.ContainsKey(activeSpaceId))
        {
            throw new ArgumentException("The active space must be present.", nameof(activeSpaceId));
        }

        ActiveSpaceId = activeSpaceId;
        Spaces = new ReadOnlyDictionary<SpaceId, SpaceNavigation>(snapshot);
    }

    public SpaceId ActiveSpaceId { get; }

    public IReadOnlyDictionary<SpaceId, SpaceNavigation> Spaces { get; }

    private static void ValidateCamera(Camera camera)
    {
        if (!double.IsFinite(camera.Position.X) || !double.IsFinite(camera.Position.Y) ||
            !double.IsFinite(camera.Zoom) || camera.Zoom <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(camera));
        }
    }
}
