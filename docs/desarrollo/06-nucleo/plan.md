# Fase 6 · Primer incremento de navegación

**Estado: cerrado.** [Cierre de la fase 6](../../inspeccion/f06-cierre.md). Al terminar, una consola en español ejecutará un recorrido entre Programación, Inicio y Juegos. La biblioteca mantendrá una cámara y una vista de retorno por espacio. La CI de Windows comprobará el contrato de navegación y medirá la cobertura de la biblioteca.

## Decisiones acordadas

| Respuesta del usuario | Aplicación |
| --- | --- |
| «Sí, planificar el primer incremento de producto» | Entregar biblioteca, consola, pruebas y CI. |
| «Sí, fijar el SDK y los cuatro paquetes de pruebas» | SDK 10.0.401, Microsoft.NET.Test.Sdk 18.12.0, xunit 2.9.3, xunit.runner.visualstudio 4.0.1 y coverlet.collector 10.1.1. |
| «Recorrido automático y selección de espacio por argumento» | Ejecutar el recorrido sin argumentos o seleccionar un espacio con `--espacio NOMBRE`. |
| «se podrian adjuntar capturas de como se en los pasos en donde hay interfaz grafica para ver como esta quedando?» | Adjuntar en los PR con cambios gráficos capturas reales de la ventana de prueba, con paso y estado visible. En esta fase se aporta la salida real de consola. |
| «Sí, redactar y publicar el plan» | Publicar este encargo para su lectura y aprobación. |

**Entrada.** Los valores de los ejemplos y los quince criterios proceden del [contrato y matriz de fase 5](https://github.com/edu3250/escritorio-canvas/blob/332b263f3bb65edf0ddddfea37129bdc5c2239cd/docs/desarrollo/05-arquitectura/primer-incremento.md). Planificación verificó en un proyecto temporal que el SDK y las cuatro versiones restauran, ejecutan una prueba y generan XML Cobertura con líneas de la biblioteca. Los resultados del producto siguen pendientes.

**Alcance.** Dos espacios en memoria, proyección lógica, comandos y diagnóstico. La ventana gráfica, el disco como almacén de sesión, la interacción Win32, los atajos globales y la reapertura quedan para fases posteriores. Ninguna tarea toma el primer plano.

## Entrega y verificación común

Una sesión, rama y PR por fila. Crear cada rama desde main actualizado tras integrar su dependencia. Los archivos indicados son los únicos autorizados para la tarea. Revisar `git diff --check origin/main...HEAD` y el listado de archivos del PR. Desde t01 ejecutar `dotnet build EscritorioCanvas.sln -c Release` y `dotnet test EscritorioCanvas.sln -c Release --no-build`. Desde t02 exigir ambos en windows-latest. En cada PR registrar revisión, comando, resultado, esfuerzo medido y criterios pendientes con motivo. Si falla un supuesto, volver a Planificación. Requieren nueva decisión cambios de contrato, dependencias, alcance o ensayos con ventanas.

| Tarea y rama | Depende de | Resultado |
| --- | --- | --- |
| t01 · f06-t01-pruebas | — | Herramientas y proyecto de pruebas reproducibles |
| t02 · f06-t02-ci | t01 | Biblioteca en la solución y CI de Windows |
| t03 · f06-t03-cobertura | t02 | Control verificable de cobertura |
| t04 · f06-t04-geometria | t03 | Transformaciones y control activo sobre el dominio |
| t05 · f06-t05-espacios | t04 | Estado inmutable y validación de construcción |
| t06 · f06-t06-navegacion | t05 | Comandos de navegación y errores tipados |
| t07 · f06-t07-consola | t06 | Recorrido y selección por argumento |
| t08 · f06-t08-aceptacion | t07 | Prueba de consola en CI y evidencia de aceptación |
| t09 · f06-t09-cierre | t08 | Reproducción y cierre |

## t01 · Preparar las pruebas

Crear `global.json` (SDK exacto, rollForward disable), `EscritorioCanvas.sln` (formato sln), `tests/Canvas.Domain.Tests/Canvas.Domain.Tests.csproj` y `tests/Canvas.Domain.Tests/EnvironmentTests.cs`. Todos los proyectos de la fase usan net10.0, referencias anulables e importaciones implícitas. Incorporar el proyecto de pruebas a la solución y fijar las cuatro versiones acordadas. Tests en espacio de nombres Canvas.Domain.Tests, con `using Xunit;`. Cuerpo completo de la prueba:

```csharp
[Fact]
public void RunsOnNetTen() => Assert.Equal(10, Environment.Version.Major);
```

Verificar SDK con `dotnet --version`, restaurar y ejecutar los dos comandos comunes. Debe descubrirse una prueba. Parar si otra versión o paquete resulta imprescindible.

## t02 · Crear la biblioteca y su CI

Crear `src/Canvas.Domain/Canvas.Domain.csproj` y `.github/workflows/build-test.yml`. Modificar la solución y el proyecto de pruebas para agregar biblioteca y ProjectReference respectivamente. Biblioteca inicialmente vacía. En push a main y en pull_request a main, compilar y probar en windows-latest con permisos contents: read. Checkout fijado a 11bd71901bbe5b1630ceea73d27597364c9af683 y setup-dotnet a 67a3573c9a986a3f9c594539f4ab511d57bb3ce9 con global-json-file global.json. Evidencia: ejecución verde enlazada y prueba t01 descubierta. Inspeccionar que Domain solo depende del marco .NET; repetir esa inspección en t08.

## t03 · Verificar el control de cobertura

Crear `eng/Check-Coverage.ps1` y `eng/Test-CoverageGate.ps1`; modificar el flujo. Interfaz: `pwsh -File eng/Check-Coverage.ps1 -Path <archivo>`. Seleccionar el paquete XML cuyo nombre exacto sea Canvas.Domain. Contar las líneas únicas por (archivo de clase, número de línea); una línea está cubierta si algún registro tiene hits positivos. Aceptar si hay líneas y cubiertas × 100 ≥ válidas × 60. Salida 0 en éxito, 1 ante ausencia, XML inválido, paquete ausente o ambiguo, denominador cero o porcentaje insuficiente. Imprimir cantidades y porcentaje. El segundo script crea informes propios temporales, invoca el primero en un proceso por caso, compara códigos y limpia al finalizar. Probar archivo ausente, cero líneas, 2/5, 3/5, 4/5 y líneas duplicadas; incluir otro paquete cubierto para comprobar el filtro. Ejecutar este segundo script en CI y propagar cualquier fallo. El informe real será exigible desde t04, primera tarea con líneas de dominio ejecutables.

## t04 · Implementar geometría y activar la medición

Crear `src/Canvas.Domain/Geometry.cs` y `tests/Canvas.Domain.Tests/GeometryTests.cs`; modificar el flujo. En namespace Canvas.Domain definir tipos públicos readonly record struct Point(double X, double Y) y Camera(Point Position, double Zoom). Clase estática Geometry con métodos públicos estáticos Point WorldToView(Point world, Camera camera) y Point ViewToWorld(Point view, Camera camera). Fórmulas y unidades del contrato. Rechazar entrada no finita o zoom no positivo mediante ArgumentOutOfRangeException antes de devolver un punto. Importar Canvas.Domain y Xunit en tests. Añadir a GeometryTests:

```csharp
[Theory]
[InlineData(120, 80, 0, 0)]
[InlineData(160, 40, 30, -30)]
public void ProjectsBothWays(double x, double y, double u, double v)
{
    var camera = new Camera(new Point(120, 80), 0.75);
    var world = new Point(x, y);
    var view = new Point(u, v);
    Assert.Equal(view, Geometry.WorldToView(world, camera));
    Assert.Equal(world, Geometry.ViewToWorld(view, camera));
    var roundWorld = Geometry.ViewToWorld(Geometry.WorldToView(world, camera), camera);
    var roundView = Geometry.WorldToView(Geometry.ViewToWorld(view, camera), camera);
    foreach (var delta in new[] { roundWorld.X-x, roundWorld.Y-y, roundView.X-u, roundView.Y-v })
        Assert.InRange(Math.Abs(delta), 0, 1e-9);
}

[Fact]
public void RejectsInvalidOperands()
{
    var zero = new Point(0, 0);
    var valid = new Camera(zero, 1);
    foreach (var n in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
    foreach (var p in new[] { new Point(n, 0), new Point(0, n) })
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Geometry.WorldToView(p, valid));
        Assert.Throws<ArgumentOutOfRangeException>(() => Geometry.ViewToWorld(p, valid));
        Assert.Throws<ArgumentOutOfRangeException>(() => Geometry.WorldToView(zero, new Camera(p, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Geometry.ViewToWorld(zero, new Camera(p, 1)));
    }
    foreach (var z in new[] { 0d, -1, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Geometry.WorldToView(zero, new Camera(zero, z)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Geometry.ViewToWorld(zero, new Camera(zero, z)));
    }
}
```

En CI ejecutar las pruebas Release con `--collect:"XPlat Code Coverage" --results-directory TestResults` en directorio limpio; exigir exactamente un coverage.cobertura.xml y pasarlo al control t03. Fallar el job si cualquier paso falla. Evidencia: registro de fracción real y casos de proyección/rechazo.

## t05 · Construir la sesión

Crear `src/Canvas.Domain/Space.cs` y `tests/Canvas.Domain.Tests/SpaceTests.cs`. Tipos públicos: readonly record struct SpaceId(string Value), sealed record SpaceNavigation(Camera Camera, Camera HomeCamera, Camera? ReturnView), sealed class SessionState con constructor público (SpaceId activeSpaceId, IEnumerable<KeyValuePair<SpaceId, SpaceNavigation>> spaces), propiedades get-only SpaceId ActiveSpaceId e IReadOnlyDictionary<SpaceId, SpaceNavigation> Spaces. Copia independiente y de solo lectura. Identidad por igualdad ordinal. Validar cámaras iniciales y retornos con las reglas t04. Rechazar ID vacío, repetido, destino activo ausente o cámara inválida con ArgumentException (incluidas sus subclases). Los datos originales sobreviven al rechazo. Prueba:

```csharp
[Fact]
public void OwnsItsSpacesAndRejectsDuplicates()
{
    var id = new SpaceId("A");
    var c = new Camera(new Point(0, 0), 1);
    var nav = new SpaceNavigation(c, c, null);
    var input = new List<KeyValuePair<SpaceId, SpaceNavigation>> { new(id, nav) };
    var session = new SessionState(id, input);
    input.Add(new(id, nav));
    Assert.ThrowsAny<ArgumentException>(() => new SessionState(id, input));
    Assert.Equal(2, input.Count);
    input.Clear();
    Assert.Equal(id, session.ActiveSpaceId);
    Assert.Equal(nav, Assert.Single(session.Spaces).Value);
}
```

Verificar los comandos comunes, la cobertura y las validaciones de construcción con casos adicionales. Parar si el diseño precisa exponer una colección mutable.

## t06 · Aplicar comandos

Crear `src/Canvas.Domain/Navigation.cs` y `tests/Canvas.Domain.Tests/NavigationTests.cs`. Clase estática pública Navigation, método estático público NavigationResult Apply(SessionState state, NavigationCommand command). NavigationCommand es un record abstracto con tres records públicos derivados: SwitchSpace(SpaceId Target), SetCamera(Camera Camera), ToggleHome(). NavigationResult expone SessionState State y NavigationError? Error. En fallo devuelve el estado original y un error; en éxito Error es null. Enum con UnknownSpace, InvalidGeometry e InvalidZoom. Validar posición antes del zoom. Aplicar literalmente la tabla de comandos del contrato. Usar estos miembros en NavigationTests:

```csharp
private static readonly SpaceId A = new("Programación"), B = new("Juegos");
private static readonly Camera Home = new(new Point(0,0),1), First = new(new Point(120,80),0.75),
    Second = new(new Point(60,40),1), Pan = new(new Point(25,10),1);
private static SessionState Start() => new(A, new Dictionary<SpaceId, SpaceNavigation>
{
    [A] = new(First, Home, null), [B] = new(Second, Home, null)
});
private static SessionState Step(SessionState s, NavigationCommand cmd)
{
    var result = Navigation.Apply(s, cmd);
    Assert.Null(result.Error);
    return result.State;
}
private static void Check(SessionState s, SpaceId active, Camera a, Camera? ar, Camera b, Camera? br)
{
    Assert.Equal(active, s.ActiveSpaceId);
    Assert.Equal(2, s.Spaces.Count);
    Assert.Equal(new SpaceNavigation(a, Home, ar), s.Spaces[A]);
    Assert.Equal(new SpaceNavigation(b, Home, br), s.Spaces[B]);
}
[Fact]
public void KeepsSeparateViewsAndRestoresEachReturn()
{
    var original = Start();
    var s = Step(original, new ToggleHome());
    Check(s,A,Home,First,Second,null);
    Check(Step(s,new SwitchSpace(A)),A,Home,First,Second,null);
    s = Step(s,new SetCamera(Pan));
    Check(s,A,Pan,First,Second,null);
    Check(Step(s,new ToggleHome()),A,First,null,Second,null);
    s = Step(original,new ToggleHome());
    s = Step(s,new SwitchSpace(B));
    Check(s,B,Home,First,Second,null);
    s = Step(Step(s,new ToggleHome()),new SetCamera(Pan));
    Check(s,B,Home,First,Pan,Second);
    s = Step(s,new SwitchSpace(A));
    Check(s,A,Home,First,Pan,Second);
    Check(Step(s,new ToggleHome()),A,First,null,Pan,Second);
    Check(original,A,First,null,Second,null);
}
[Fact]
public void InvalidCommandsPreserveTheEntireState()
{
    var s = Step(Start(),new ToggleHome());
    void Reject(NavigationCommand cmd, NavigationError error)
    {
        var r = Navigation.Apply(s,cmd);
        Assert.Equal<NavigationError?>(error,r.Error);
        Check(s,A,Home,First,Second,null);
        Check(r.State,A,Home,First,Second,null);
    }
    Reject(new SwitchSpace(new("Otro")),NavigationError.UnknownSpace);
    foreach (var n in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
    foreach (var p in new[] { new Point(n,0), new Point(0,n) })
        Reject(new SetCamera(new(p,1)),NavigationError.InvalidGeometry);
    foreach (var z in new[] { 0d,-1,double.NaN,double.PositiveInfinity,double.NegativeInfinity })
        Reject(new SetCamera(new(new Point(0,0),z)),NavigationError.InvalidZoom);
}
```

Evidencia: pruebas verdes y cotejo de la matriz por nombre. La sesión original conserva todas sus vistas al aplicar una transición válida.

## t07 · Ejecutar el recorrido

Crear `src/Canvas.Console/Canvas.Console.csproj` y `src/Canvas.Console/Program.cs`, modificar la solución. Referenciar únicamente Domain y el marco. UTF-8, números con coma decimal y sin separadores de millares. Sin argumentos, inicializar la sesión exacta del recorrido histórico (Juegos comienza en 0,0 con zoom 1) y ejecutar sus cinco comandos. Emitir una línea inicial y una línea por comando con esta gramática: `Espacio: NOMBRE | Cámara: (X; Y; Z) | Retorno: vacío`. Para un retorno guardado sustituir «vacío» por la terna guardada. Con `--espacio NOMBRE`, construir la misma sesión, ejecutar una selección y mostrar el resultado final; nombre ordinal y sensible a mayúsculas. Destino desconocido: stderr «Espacio desconocido: NOMBRE», salida 1. Sintaxis inválida: stderr «Uso: Canvas.Console [--espacio NOMBRE]», salida 2. Salida 0 en éxito. La consola delega las decisiones en Apply.

Adjuntar al PR salida real completa del recorrido y de `--espacio Desconocido` con códigos. Verificar también Juegos, Programación y sintaxis incorrecta ejecutando `dotnet run -c Release --no-build --project src/Canvas.Console --` seguido de los argumentos. Si un cambio requiere tocar ventanas, detener y volver a Planificación.

## t08 · Verificar la entrega

Ejecución crea `eng/Test-NavigationConsole.ps1` y modifica el flujo para invocarlo tras el build. El script ejecuta la DLL Release en procesos separados, captura stdout, stderr y código. Comparar literalmente las seis líneas del recorrido con la gramática t07 y la tabla histórica; comprobar también selección válida, espacio desconocido y sintaxis inválida. Cualquier diferencia falla el job. Presentar salida real con cada código y enlace al job. Comprobar las referencias y el código de Domain en el SHA revisado. Ninguna llamada a disco, ventanas, presentación o almacenamiento.

Para respetar el máximo por tarea, el cierre documental queda en t09. Evidencia de t08: los quince casos de la matriz con comando, resultado y referencia a log, prueba o línea. Parar si falla alguno.

## t09 · Publicar reproducción y cierre

Rama `f06-t09-cierre`, desde t08 fusionada. Ejecución crea `docs/desarrollo/05-arquitectura/recorrido-incremento.md` con comandos reproducibles, transcript real, SHA, versión de Windows y SDK; indicar que DPI no interviene en la consola. Revisión añade `docs/inspeccion/f06-cierre.md` y actualiza `README.md` y `docs/plan-por-fases.md` en el mismo PR. Comprobar que cada criterio de la matriz enlaza su evidencia en el SHA ensayado y que el workflow también pasa en este PR. Registrar esfuerzo por rol y estado verificado, fallido o pendiente por criterio. La fusión cierra la fase cuando los quince criterios estén verificados. Una decisión que requiera una interfaz gráfica abre una discusión de alcance posterior.
