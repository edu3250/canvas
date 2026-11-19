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

## Matriz de pruebas futuras

Todos los casos están **pendientes de implementación**. Las cámaras de ejemplo se expresan como (x, y; zoom). Cada comprobación de estado compara el espacio activo y, para cada espacio, su cámara vigente, su cámara de Inicio y su retorno.

| Caso | Entrada | Acción | Resultado esperado | Método de comprobación |
| --- | --- | --- | --- | --- |
| Ida y vuelta a Inicio | Programación activa con (120, 80; 0,75), Inicio en (0, 0; 1) y retorno vacío | Aplicar `ToggleHome()` dos veces | La primera llamada muestra Inicio y guarda (120, 80; 0,75). La segunda restaura esos valores y vacía el retorno. | xUnit. Comprobar el estado tras cada llamada. |
| Paneo en Inicio | Programación con los valores del caso anterior | Ir a Inicio, aplicar `SetCamera((25, 10; 1))` y volver con `ToggleHome()` | El paneo cambia la cámara vigente a (25, 10; 1), conserva la cámara de Inicio y el retorno original. Volver restaura (120, 80; 0,75) y consume el retorno. | xUnit. Comparar las tres vistas después del paneo y del retorno. |
| Independencia entre espacios | Programación activa con (120, 80; 0,75), Juegos con (60, 40; 1) e Inicio en (0, 0; 1) en ambos. Retornos vacíos. | Entrar a Inicio en Programación, cambiar a Juegos, entrar a su Inicio, mover su cámara a (25, 10; 1), volver a Programación y salir de su Inicio | Al regresar, Programación muestra su Inicio y conserva su retorno a (120, 80; 0,75). Salir de Inicio restaura esa cámara y vacía solo su retorno. Juegos conserva (25, 10; 1) y su retorno a (60, 40; 1). | xUnit. Comparar ambos espacios después del cambio, del regreso y del retorno. |
| Selección del mismo espacio | Programación activa durante una visita a Inicio y con retorno vigente | Aplicar `SwitchSpace(Programación)` | Se conserva el estado completo, incluido el retorno vigente. | xUnit. Comparar todos los valores del estado antes y después. |
| Espacio desconocido | Sesión válida con Programación activa y retorno vigente. El identificador solicitado no existe. | Aplicar `SwitchSpace` con ese identificador | `UnknownSpace`. El estado de entrada conserva todos sus valores. | xUnit. Comprobar el error tipado y comparar el estado con una copia de sus valores previos. |
| Identificador duplicado | Dos espacios con el mismo `SpaceId` | Construir `SessionState` | La construcción se rechaza sin publicar una sesión parcial. | xUnit. Comprobar el rechazo y que los datos suministrados conservan sus valores. |
| Coordenadas no finitas | Sesión válida y cámaras con `NaN`, infinito positivo o infinito negativo en un eje. Puntos con esos valores para las transformaciones. | Aplicar `SetCamera` y, en casos separados, invocar ambas transformaciones con el punto o cámara inválidos | `SetCamera` devuelve `InvalidGeometry` y deja intacto el estado. Cada transformación rechaza la entrada antes de producir un punto. | xUnit parametrizado por eje y valor. Comprobar el error, el estado completo y el rechazo en ambas transformaciones. |
| Zoom inválido | Sesión válida, posición finita y zoom 0, -1, `NaN`, infinito positivo o infinito negativo | Aplicar `SetCamera` e invocar ambas transformaciones en casos separados | `SetCamera` devuelve `InvalidZoom` y deja intacto el estado. Las transformaciones rechazan la cámara antes de producir un punto. | xUnit parametrizado por zoom. Comprobar el error, el estado completo y el rechazo en ambas transformaciones. |
| Estado anterior conservado | Sesión válida con Programación activa y dos espacios. Copia por valor de la sesión previa. | Aplicar `SetCamera` con otra cámara válida | El resultado nuevo tiene la nueva cámara activa. El estado anterior conserva todos sus valores, incluido el otro espacio. | xUnit. Comparar por separado el resultado y el estado anterior con la copia previa. |
| Proyección inversa | Cámara (120, 80; 0,75), puntos de mundo (120, 80) y (160, 40), y puntos de vista (0, 0) y (30, -30) | Proyectar y aplicar la inversa. Repetir en el sentido vista → mundo → vista | Los puntos de mundo proyectan a (0, 0) y (30, -30). Cada ida y vuelta recupera el punto de entrada con error absoluto de hasta `1e-9` unidades lógicas por eje en estas muestras acotadas. | xUnit. Comparar las proyecciones y las diferencias absolutas por eje con la tolerancia. |
| Consola en éxito | Sesión y comandos válidos del recorrido anterior | Ejecutar el adaptador con el recorrido completo | Salida en español con espacio activo, cámara y retorno tras cada comando. Código de salida cero. | Prueba de integración. Capturar salida y código y contrastarlos con el recorrido. |
| Consola en error | Entrada diagnóstica con una solicitud de espacio desconocido | Ejecutar el adaptador con esa entrada | Mensaje de error en español y código de salida distinto de cero. | Prueba de integración. Capturar la salida diagnóstica y el código y comprobar ambos. |
| Dependencias del dominio | Proyecto y código de `Canvas.Domain` | Inspeccionar referencias de proyecto, dependencias y llamadas del dominio | El dominio trabaja con datos puros y prescinde de Windows, presentación, almacenamiento y acceso a disco. | Inspección de proyecto y código. Revisar referencias y uso de APIs de efectos. |
| CI y cobertura de líneas | Solución, pruebas y reporte Cobertura con líneas válidas del dominio | Ejecutar `dotnet build` y `dotnet test` con recolección de cobertura en `windows-latest`, y comprobar el cociente de líneas cubiertas entre líneas válidas del dominio | La compilación y los tests deben pasar. El control acepta cobertura de al menos 60 % del dominio, incluido el 60 % exacto. | Inspección del flujo y ejecución futura en CI. Comprobar el registro de ambos comandos y un informe con cobertura en el umbral. |
| Rechazo de cobertura inválida | Tres casos separados con informe ausente, cero líneas válidas o cobertura inferior al 60 % | Ejecutar el control de cobertura para cada caso | El control falla y el trabajo de CI termina en error en los tres casos. | Comprobar el control con informes de prueba y verificar su código de salida y su propagación al trabajo de CI. |

Las versiones y la autorización de paquetes se resuelven en el encargo de producto.

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
