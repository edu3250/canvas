# Recorrido del primer incremento de navegación

Se ejecutó el 19 de noviembre de 2026 en Windows 11 Pro (10.0.26200, compilación 26200) con el SDK de .NET 10.0.401. El DPI no interviene en la consola. Las coordenadas pertenecen al modelo lógico de navegación.

## Reproducir el recorrido

Desde la raíz del repositorio, usa el [commit ensayado](https://github.com/edu3250/escritorio-canvas/blob/14844d65d9aeb570e7b995364a862abf48e72823/EscritorioCanvas.sln) `14844d65d9aeb570e7b995364a862abf48e72823` y ejecuta estos comandos.

```powershell
git rev-parse HEAD
dotnet --version
dotnet build EscritorioCanvas.sln -c Release
powershell.exe -NoProfile -File eng/Test-NavigationConsole.ps1
dotnet test EscritorioCanvas.sln -c Release --no-build
```

La compilación terminó con 0 advertencias y 0 errores. Las 17 pruebas xUnit pasaron. El probador ejecuta la DLL Release en cinco procesos, muestra el código de cada proceso y compara ambos canales con igualdad ordinal. Para ejecutar solo el recorrido automático:

```powershell
dotnet src/Canvas.Console/bin/Release/net10.0/Canvas.Console.dll
```

## Salida capturada

```text
Caso Recorrido automático · código 0
stdout:
Espacio: Programación | Cámara: (120; 80; 0,75) | Retorno: vacío
Espacio: Programación | Cámara: (0; 0; 1) | Retorno: (120; 80; 0,75)
Espacio: Programación | Cámara: (120; 80; 0,75) | Retorno: vacío
Espacio: Juegos | Cámara: (0; 0; 1) | Retorno: vacío
Espacio: Juegos | Cámara: (60; 40; 1) | Retorno: vacío
Espacio: Programación | Cámara: (120; 80; 0,75) | Retorno: vacío

stderr:

Caso Selección de Juegos · código 0
stdout:
Espacio: Juegos | Cámara: (0; 0; 1) | Retorno: vacío

stderr:

Caso Selección de Programación · código 0
stdout:
Espacio: Programación | Cámara: (120; 80; 0,75) | Retorno: vacío

stderr:

Caso Espacio desconocido · código 1
stdout:

stderr:
Espacio desconocido: Desconocido

Caso Sintaxis inválida · código 2
stdout:

stderr:
Uso: Canvas.Console [--espacio NOMBRE]
```

## Evidencia de la matriz de aceptación

Los enlaces a código corresponden al commit ensayado. La [ejecución de CI en windows-latest](https://github.com/edu3250/escritorio-canvas/actions/runs/44629503532/job/135932065221) corresponde al mismo SHA. Para comprobar las referencias de Domain:

```powershell
dotnet msbuild src/Canvas.Domain/Canvas.Domain.csproj -getItem:FrameworkReference,PackageReference,ProjectReference
```

| Criterio | Estado | Resultado y evidencia |
| --- | --- | --- |
| Ida y vuelta a Inicio | Verificado | Se restaura la cámara anterior y se consume el retorno. [Prueba de navegación][navigation]. |
| Paneo en Inicio | Verificado | Se actualiza la cámara vigente y se conservan la cámara de Inicio y el retorno. [Prueba de navegación][navigation]. |
| Independencia entre espacios | Verificado | Cada espacio conserva su propia cámara y retorno. [Prueba de navegación][navigation]. |
| Selección del mismo espacio | Verificado | Se conserva el estado completo durante la visita a Inicio. [Prueba de navegación][navigation]. |
| Espacio desconocido | Verificado | Se devuelve `UnknownSpace` con el estado intacto. [Prueba de comandos inválidos][invalid]. |
| Identificador duplicado | Verificado | El constructor rechaza el duplicado y preserva la entrada. [Prueba de espacios][spaces]. |
| Coordenadas no finitas | Verificado | Ambas transformaciones rechazan la entrada y `SetCamera` devuelve `InvalidGeometry` con el estado intacto. [Geometría inválida][invalid-geometry] y [comandos inválidos][invalid]. |
| Zoom inválido | Verificado | Ambas transformaciones rechazan la cámara y `SetCamera` devuelve `InvalidZoom` con el estado intacto. [Geometría inválida][invalid-geometry] y [comandos inválidos][invalid]. |
| Estado anterior conservado | Verificado | Cambiar la cámara activa conserva los valores de la sesión anterior y del otro espacio. [Prueba de navegación][navigation]. |
| Proyección inversa | Verificado | En ambos sentidos, el error absoluto por eje queda dentro de `1e-9` en las dos muestras. [Prueba de proyección][projection]. |
| Consola en éxito | Verificado | El recorrido emite seis líneas y las dos selecciones válidas emiten una línea cada una. Los tres casos terminan con código 0. [Salida capturada](#salida-capturada) y [probador][console]. |
| Consola en error | Verificado | El espacio desconocido y la sintaxis inválida escriben en stderr y devuelven 1 y 2 respectivamente. [Salida capturada](#salida-capturada) y [probador][console]. |
| Dependencias del dominio | Verificado | MSBuild devuelve solo `Microsoft.NETCore.App`, con las listas de paquetes y proyectos vacías. La inspección del dominio confirma operaciones en memoria. [Proyecto][domain] y [código][domain-code]. |
| CI y cobertura de líneas | Verificado | La CI compila y ejecuta 17 pruebas. El dominio cubre 70 de 71 líneas (98,59 %) y la prueba del umbral acepta 3 de 5 (60 %). [CI][ci] y [caso del umbral][gate]. |
| Rechazo de cobertura inválida | Verificado | El control devuelve 1 para informe ausente, cero líneas y 2 de 5 líneas. La CI ejecuta el control sobre el informe real y falla si lo rechaza. [Casos del control][gate] y [flujo][workflow]. |

[navigation]: https://github.com/edu3250/escritorio-canvas/blob/14844d65d9aeb570e7b995364a862abf48e72823/tests/Canvas.Domain.Tests/NavigationTests.cs#L29
[invalid]: https://github.com/edu3250/escritorio-canvas/blob/14844d65d9aeb570e7b995364a862abf48e72823/tests/Canvas.Domain.Tests/NavigationTests.cs#L49
[spaces]: https://github.com/edu3250/escritorio-canvas/blob/14844d65d9aeb570e7b995364a862abf48e72823/tests/Canvas.Domain.Tests/SpaceTests.cs#L9
[invalid-geometry]: https://github.com/edu3250/escritorio-canvas/blob/14844d65d9aeb570e7b995364a862abf48e72823/tests/Canvas.Domain.Tests/GeometryTests.cs#L25
[projection]: https://github.com/edu3250/escritorio-canvas/blob/14844d65d9aeb570e7b995364a862abf48e72823/tests/Canvas.Domain.Tests/GeometryTests.cs#L11
[console]: https://github.com/edu3250/escritorio-canvas/blob/14844d65d9aeb570e7b995364a862abf48e72823/eng/Test-NavigationConsole.ps1
[domain]: https://github.com/edu3250/escritorio-canvas/blob/14844d65d9aeb570e7b995364a862abf48e72823/src/Canvas.Domain/Canvas.Domain.csproj
[domain-code]: https://github.com/edu3250/escritorio-canvas/tree/14844d65d9aeb570e7b995364a862abf48e72823/src/Canvas.Domain
[ci]: https://github.com/edu3250/escritorio-canvas/actions/runs/44629503532/job/135932065221
[gate]: https://github.com/edu3250/escritorio-canvas/blob/14844d65d9aeb570e7b995364a862abf48e72823/eng/Test-CoverageGate.ps1#L44
[workflow]: https://github.com/edu3250/escritorio-canvas/blob/14844d65d9aeb570e7b995364a862abf48e72823/.github/workflows/build-test.yml#L33
