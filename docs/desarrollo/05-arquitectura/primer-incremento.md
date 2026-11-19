# Primer incremento: navegación y espacios

**Propuesta; no autorizado para ejecución.** Plan [6597a51](https://github.com/edu3250/escritorio-canvas/commit/6597a51bb1ee6ad157d88345dd8006d7c6352160). Depende de aprobar [arquitectura](../../arquitectura/propuesta-producto.md), plataforma y paquetes. No se inicia otra fase con este documento.

## Corte y observación

Biblioteca pura de navegación con dos espacios en memoria y consola que imprime cámara, espacio y estado de retorno después de cada comando. Es una superficie diagnóstica de producto propuesta, no el canvas final. No abre ventanas ajenas, archivos de contenido, procesos ni atajos globales. No demuestra foco, miniaturas, persistencia, reapertura o rendimiento.

Recorrido concreto: iniciar Programación con cámara (120, 80), zoom 0.75; ir a Inicio (0, 0), zoom 1; volver y observar (120, 80), 0.75; cambiar a Juegos, desplazar su cámara y volver a Programación. Su cámara debe seguir intacta. Se propone consola por su pequeño alcance observable; la futura UI permanece pendiente de aprobación.

## Archivos previstos, sin crearlos ahora

| Archivo | Responsabilidad |
| --- | --- |
| EscritorioCanvas.sln, global.json | Solución y SDK fijado tras comprobar disponibilidad. |
| src/Canvas.Domain/Canvas.Domain.csproj | Biblioteca sin dependencias Windows. |
| src/Canvas.Domain/Geometry.cs | Point, Camera, conversión mundial/viewport y validación. |
| src/Canvas.Domain/Space.cs | SpaceId, Home, SpaceNavigation y SessionState inmutables. |
| src/Canvas.Domain/Navigation.cs | Comandos y resultado tipado, sin efectos. |
| src/Canvas.Console/Canvas.Console.csproj, Program.cs | Adaptador de consola con datos en memoria y salida en español. |
| tests/Canvas.Domain.Tests/Canvas.Domain.Tests.csproj | xUnit y cobertura. |
| tests/Canvas.Domain.Tests/NavigationTests.cs, GeometryTests.cs | Escenarios de la tabla siguiente. |
| .github/workflows/build-test.yml | windows-latest, build/test y control de cobertura. |
| eng/Check-Coverage.ps1 | Leer Cobertura y fallar si falta reporte o no alcanza umbral. |
| docs/desarrollo/05-arquitectura/recorrido-incremento.md | Futuro recorrido reproducible y resultados; requiere nuevo encargo. |

Los nombres son contratos propuestos del futuro encargo, no permiso para crear estos archivos en fase 5.

## Firmas públicas propuestas

`NavigationResult Apply(SessionState state, NavigationCommand command)` devuelve estado nuevo o `NavigationError`, sin mutar el argumento. Comandos: `SwitchSpace(SpaceId)`, `SetCamera(Camera)`, `ToggleHome()`. `SessionState` contiene espacios y ActiveSpaceId; cada `SpaceNavigation` contiene Camera, HomeCamera y ReturnView opcional.

`Point WorldToView(Point world, Camera camera)` y `Point ViewToWorld(Point view, Camera camera)` trabajan en unidades lógicas, sin DPI ni origen físico. `Camera` contiene Position y Zoom. `NavigationError` distingue UnknownSpace, InvalidGeometry y InvalidZoom. IDs duplicados fallan al construir sesión. NaN, infinito y zoom no positivo se rechazan, sin recorte silencioso. Una operación inválida conserva el estado anterior completo.

ToggleHome sin retorno guarda cámara actual y aplica HomeCamera; con retorno restaura y lo consume. SetCamera durante una visita cambia cámara visible pero conserva ReturnView. SwitchSpace no consume el retorno del origen y recupera la navegación del destino. El mismo espacio es una operación sin cambio. Propuesta de semántica a revisar, no nuevo acuerdo de interacción.

## Criterios por escenario

| Escenario | Resultado exigible / método | Estado actual |
| --- | --- | --- |
| Ida y vuelta | Igualdad de posición y zoom guardados, retorno consumido; xUnit | Pendiente de implementación |
| Paneo en Inicio | ReturnView no se sobreescribe; segunda acción vuelve a vista original | Pendiente |
| Cambio de espacio | Cámara y retorno independientes; ida y vuelta con ambos espacios modificados | Pendiente |
| Error de identificación | UnknownSpace y estado sin cambios | Pendiente |
| Geometría inválida | Rechazar NaN, infinito y zoom cero/negativo sin mutación | Pendiente |
| Transformaciones | Inversa recupera punto con tolerancia 1e-9 unidades lógicas en muestras acotadas, zoom distinto de 1 | Pendiente |
| Inmutabilidad | Estado previo conserva todos sus valores tras transición válida | Pendiente |
| Observabilidad | Consola imprime valores del recorrido y errores en español; salida 0 para recorrido válido y distinta de 0 para entrada inválida | Pendiente |
| Arquitectura | Domain no referencia Windows/UI/Storage ni usa disco; inspección de proyecto y código | Pendiente |
| CI y cobertura | Build y test pasan; cobertura de líneas al menos 60 % de toda lógica Domain creada/modificada | Pendiente |

La medición inicial incluye toda la biblioteca Domain, sin excluir clases de navegación o geometría para elevar el porcentaje. Agregar casos de error y aislamiento entre espacios; no limitarse a pruebas que reproduzcan la implementación.

## Validación futura y dependencias

Primera creación de src/ incluye CI windows-latest con `dotnet build` y `dotnet test --collect:"XPlat Code Coverage"`. El flujo restaura, compila, prueba y publica Cobertura; Check-Coverage verifica líneas cubiertas/válidas y falla por reporte ausente, denominador cero o porcentaje inferior a 60 %. No depender solo del éxito de dotnet test. Fijar versiones de acciones por revisión y SDK antes de implementar.

Conjunto propuesto pendiente de aprobación y comprobación de compatibilidad: Microsoft.NET.Test.Sdk, xUnit, xunit.runner.visualstudio y coverlet.collector. Las versiones concretas se fijarán en el siguiente encargo tras consultar sus fuentes primarias; no usar comodines ni instalar para resolverlas en esta fase. La [guía primaria de Microsoft](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage), consultada el 18-11-2026, documenta la recogida de cobertura; no constituye aprobación de dependencias.

Registrar Windows, SDK y versión de consola en ejecución futura; DPI no aplica a esta consola y se anotará expresamente. No requiere ensayo que tome primer plano. Revisión independiente del siguiente corte debe comprobar criterios, reporte y SHA; el 60 % no reemplaza escenarios funcionales.

## Límites y esfuerzo de esta entrega

Solo documentación, sin ejecución de aplicaciones ni pruebas Windows nuevas. Herramienta Codex, familia GPT-6; variante exacta no verificada. Una pasada de redacción y una validación mecánica previstas; tiempos retrospectivos e intentos finales constan en PR. Implementación de producto, integración por Revisión y revisión humana no realizadas. No hay estimación precisa del futuro incremento sin resolver plataforma y versiones.
