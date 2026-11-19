namespace Canvas.Domain;

public abstract record NavigationCommand;

public sealed record SwitchSpace(SpaceId Target) : NavigationCommand;

public sealed record SetCamera(Camera Camera) : NavigationCommand;

public sealed record ToggleHome() : NavigationCommand;

public sealed record NavigationResult(SessionState State, NavigationError? Error);

public enum NavigationError
{
    UnknownSpace,
    InvalidGeometry,
    InvalidZoom
}

public static class Navigation
{
    public static NavigationResult Apply(SessionState state, NavigationCommand command)
    {
        switch (command)
        {
            case SwitchSpace change:
                if (!state.Spaces.ContainsKey(change.Target))
                {
                    return new NavigationResult(state, NavigationError.UnknownSpace);
                }

                if (change.Target == state.ActiveSpaceId)
                {
                    return new NavigationResult(state, null);
                }

                return new NavigationResult(new SessionState(change.Target, state.Spaces), null);

            case SetCamera move:
                var camera = move.Camera;
                if (!double.IsFinite(camera.Position.X) || !double.IsFinite(camera.Position.Y))
                {
                    return new NavigationResult(state, NavigationError.InvalidGeometry);
                }

                if (!double.IsFinite(camera.Zoom) || camera.Zoom <= 0)
                {
                    return new NavigationResult(state, NavigationError.InvalidZoom);
                }

                return UpdateActiveSpace(state, state.Spaces[state.ActiveSpaceId] with { Camera = camera });

            case ToggleHome:
                var space = state.Spaces[state.ActiveSpaceId];
                var next = space.ReturnView is Camera returnView
                    ? space with { Camera = returnView, ReturnView = null }
                    : space with { Camera = space.HomeCamera, ReturnView = space.Camera };
                return UpdateActiveSpace(state, next);

            default:
                throw new ArgumentException("Unsupported navigation command.", nameof(command));
        }
    }

    private static NavigationResult UpdateActiveSpace(SessionState state, SpaceNavigation navigation)
    {
        var spaces = new Dictionary<SpaceId, SpaceNavigation>(state.Spaces)
        {
            [state.ActiveSpaceId] = navigation
        };
        return new NavigationResult(new SessionState(state.ActiveSpaceId, spaces), null);
    }
}
