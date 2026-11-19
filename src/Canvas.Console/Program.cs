using System.Globalization;
using System.Text;
using Canvas.Domain;

Console.OutputEncoding = Encoding.UTF8;

if (args.Length != 0 && (args.Length != 2 || args[0] != "--espacio"))
{
    Console.Error.WriteLine("Uso: Canvas.Console [--espacio NOMBRE]");
    return 2;
}

var programming = new SpaceId("Programación");
var games = new SpaceId("Juegos");
var home = new Camera(new Point(0, 0), 1);
var state = new SessionState(programming, new Dictionary<SpaceId, SpaceNavigation>
{
    [programming] = new(new Camera(new Point(120, 80), 0.75), home, null),
    [games] = new(home, home, null)
});
var culture = CultureInfo.GetCultureInfo("es-ES");

if (args.Length == 2)
{
    var target = new SpaceId(args[1]);
    var result = Navigation.Apply(state, new SwitchSpace(target));
    if (result.Error == NavigationError.UnknownSpace)
    {
        Console.Error.WriteLine($"Espacio desconocido: {target.Value}");
        return 1;
    }

    PrintState(result.State, culture);
    return 0;
}

NavigationCommand[] commands =
[
    new ToggleHome(),
    new ToggleHome(),
    new SwitchSpace(games),
    new SetCamera(new Camera(new Point(60, 40), 1)),
    new SwitchSpace(programming)
];

PrintState(state, culture);
foreach (var command in commands)
{
    var result = Navigation.Apply(state, command);
    if (result.Error is not null)
    {
        Console.Error.WriteLine("No se pudo completar el recorrido.");
        return 1;
    }

    state = result.State;
    PrintState(state, culture);
}

return 0;

static void PrintState(SessionState state, CultureInfo culture)
{
    var space = state.Spaces[state.ActiveSpaceId];
    var returnView = space.ReturnView is Camera camera ? FormatCamera(camera, culture) : "vacío";
    Console.WriteLine($"Espacio: {state.ActiveSpaceId.Value} | Cámara: {FormatCamera(space.Camera, culture)} | Retorno: {returnView}");
}

static string FormatCamera(Camera camera, CultureInfo culture)
{
    return string.Create(culture, $"({camera.Position.X}; {camera.Position.Y}; {camera.Zoom})");
}
