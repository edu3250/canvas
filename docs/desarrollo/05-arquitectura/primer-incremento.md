# Primer incremento de navegación

Contrato para una futura implementación de la navegación en memoria. La biblioteca pura resuelve cámara, Inicio y cambio de espacio. Un adaptador diagnóstico de consola prepara una sesión de ejemplo, aplica los comandos mediante la biblioteca e imprime en español el espacio activo, la cámara y la vista de retorno después de cada paso. Los errores se muestran en español. [Plan de la fase 5](plan.md).

La gestión de ventanas queda para la integración futura. Sus límites están en la propuesta de arquitectura. [T02 · Activación durante un diálogo](../../arquitectura/propuesta-producto.md#activación-durante-un-diálogo), [T03 · Ampliación y retorno](../../arquitectura/propuesta-producto.md#ampliación-y-retorno), [T04 · Geometría y DPI](../../arquitectura/propuesta-producto.md#geometría-y-dpi).

## Estado y firmas

`SessionState` contiene los espacios indexados por `SpaceId` y el identificador activo `ActiveSpaceId`. Cada espacio conserva su `SpaceNavigation` con la cámara vigente `Camera`, la cámara fija de Inicio `HomeCamera` y la vista de retorno opcional `ReturnView`. Una cámara reúne `Position` de tipo `Point` y `Zoom`. La vista de retorno guarda posición y zoom.

```csharp
NavigationResult Apply(SessionState state, NavigationCommand command)
Point WorldToView(Point world, Camera camera)
Point ViewToWorld(Point view, Camera camera)
```

`Apply` devuelve el estado resultante o un `NavigationError`. El estado es inmutable. Una transición válida con cambios produce un estado nuevo y conserva todos los valores del anterior. Una operación inválida deja intacto el estado de entrada.

| Comando | Resultado |
| --- | --- |
| `SwitchSpace(SpaceId)` | Activa el destino con su propia cámara y conserva las cámaras y vistas de retorno de ambos espacios. Seleccionar el espacio activo conserva íntegro el estado. |
| `SetCamera(Camera)` | Actualiza la cámara del espacio activo. Durante una visita a Inicio, permite mover la vista y conserva `ReturnView` y `HomeCamera`. |
| `ToggleHome()` con retorno vacío | Guarda la cámara vigente en `ReturnView` y aplica `HomeCamera`. |
| `ToggleHome()` con retorno vigente | Restaura la posición y el zoom guardados y consume `ReturnView`. |

La posición de cámara es el origen visible. En unidades lógicas, `WorldToView` calcula `(world - camera.Position) × camera.Zoom`. La inversa, `ViewToWorld`, calcula `view / camera.Zoom + camera.Position`.

## Recorrido de consola

La sesión de ejemplo comienza en Programación con cámara (120, 80) y zoom 0,75, y en Juegos con cámara guardada (0, 0) y zoom 1. La cámara de Inicio de Programación es (0, 0) con zoom 1. Ambos retornos empiezan vacíos. En la tabla, cada cámara se expresa como `(x, y; zoom)`.

| Entrada | Espacio activo resultante | Cámara de Programación | Retorno de Programación | Cámara de Juegos | Retorno de Juegos |
| --- | --- | --- | --- | --- | --- |
| Construir la sesión | Programación | (120, 80; 0,75) | Vacío | (0, 0; 1) | Vacío |
| `ToggleHome()` para ir a Inicio | Programación | (0, 0; 1) | (120, 80; 0,75) | (0, 0; 1) | Vacío |
| `ToggleHome()` para volver | Programación | (120, 80; 0,75) | Vacío | (0, 0; 1) | Vacío |
| `SwitchSpace(Juegos)` | Juegos | (120, 80; 0,75) | Vacío | (0, 0; 1) | Vacío |
| `SetCamera((60, 40; 1))` | Juegos | (120, 80; 0,75) | Vacío | (60, 40; 1) | Vacío |
| `SwitchSpace(Programación)` | Programación | (120, 80; 0,75) | Vacío | (60, 40; 1) | Vacío |

## Entradas inválidas

| Entrada | Resultado |
| --- | --- |
| `SwitchSpace` con un identificador desconocido | `NavigationError.UnknownSpace`. |
| Dos espacios con el mismo `SpaceId` al construir la sesión | La construcción falla y no se crea una sesión parcial. |
| Coordenada `NaN` o infinita en un punto o en la posición de cámara | Se rechaza la geometría. En `Apply`, `NavigationError.InvalidGeometry`. |
| Zoom cero, negativo, `NaN` o infinito | Se rechaza el zoom. En `Apply`, `NavigationError.InvalidZoom`. |

Las transformaciones requieren coordenadas finitas y zoom finito y positivo. Una entrada inválida se rechaza antes de producir un punto. La validación conserva los valores suministrados y evita correcciones silenciosas.

## Archivos previstos para la implementación

| Archivo | Responsabilidad |
| --- | --- |
| `EscritorioCanvas.sln`, `global.json` | Solución y versión del SDK. |
| `src/Canvas.Domain/Canvas.Domain.csproj` | Biblioteca pura, sin dependencias de Windows ni acceso al disco. |
| `src/Canvas.Domain/Geometry.cs` | `Point`, `Camera`, transformaciones y validación geométrica. |
| `src/Canvas.Domain/Space.cs` | `SpaceId`, `Home`, `SpaceNavigation` y `SessionState`. |
| `src/Canvas.Domain/Navigation.cs` | Comandos, resultado tipado y transiciones. |
| `src/Canvas.Console/Canvas.Console.csproj`, `src/Canvas.Console/Program.cs` | Adaptador diagnóstico y salida en español. |
| `tests/Canvas.Domain.Tests/Canvas.Domain.Tests.csproj`, `tests/Canvas.Domain.Tests/NavigationTests.cs`, `tests/Canvas.Domain.Tests/GeometryTests.cs` | Proyecto y casos de prueba futuros. |
| `.github/workflows/build-test.yml`, `eng/Check-Coverage.ps1` | Compilación, pruebas y comprobación de cobertura. |
| `docs/desarrollo/05-arquitectura/recorrido-incremento.md` | Reproducción y resultados de la futura implementación. |
