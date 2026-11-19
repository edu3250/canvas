# Fase 7 · Primer lienzo gráfico

**Estado: en aprobación.** Entregar una ventana WPF que permita recorrer Programación y Juegos, reorganizar sus iconos y recuperar una vista anterior. El criterio visible es que, después de visitar Inicio, Codex recupere la misma posición y tamaño en pantalla. El paisaje conserva su encuadre durante todo el recorrido.

## Decisiones acordadas

| Respuesta del usuario | Consecuencia |
| --- | --- |
| «Ver y recorrer el primer lienzo gráfico» | Primer incremento gráfico del producto. |
| «espero encontrarme un espacio casi vacio como en deska en donde mis iconos vivan en alguna parte del lienzo pensada solo para iconos» | Inicio agrupa los accesos a la izquierda; el centro queda despejado. |
| «2. la opcion recomendada» | Arrastrar el fondo desplaza la vista; la rueda acerca y aleja alrededor del puntero. |
| «En una barra pequeña siempre visible» | Selector de espacio e Inicio/retorno permanecen accesibles. |
| «Por los propios iconos sobre el fondo» | Sin marco permanente alrededor de Inicio. |
| «puedes poner un icono de vs code y otro de codex por ejemplo para probar» | Dos accesos de muestra en Programación. |
| «un icono lo resalta como en windows, lo puedes mover manteniendo el click izquierdo tal cual funciona en windows» | Selección individual y arrastre desde el icono o su nombre. |
| «el fondo se mantiene igual de momento» y «al cambiar de escritorio quiero que salgan otros iconos» | Un mismo paisaje fijo. Juegos muestra Steam y Discord. Cada espacio conserva su propio acomodo durante la sesión. |
| «En la última posición válida dentro de Inicio» y «Alineado en una cuadrícula invisible» | Limitar el arrastre y encajar al soltar, sin superposición. |
| «Sí, usar WPF y publicar el plan para revisión» | WPF del SDK de Windows; presentar este PR antes de ejecutar. |

## Base y límites

Reutilizar [Geometry](https://github.com/edu3250/escritorio-canvas/blob/34fccf4dc019937f0bcc73a6e00e781856f791aa/src/Canvas.Domain/Geometry.cs) y [Navigation.Apply](https://github.com/edu3250/escritorio-canvas/blob/34fccf4dc019937f0bcc73a6e00e781856f791aa/src/Canvas.Domain/Navigation.cs). La [fase 6](../06-nucleo/plan.md) conserva sus pruebas y consola. Los accesos son muestras locales. Abrir aplicaciones, manipular ventanas ajenas, persistir al salir, múltiples lienzos por espacio y atajos globales quedan fuera. Ningún paquete nuevo.

Los tamaños, límites de zoom, iconografía y emplazamiento siguientes son propuestas de implementación para aprobar con este plan. Usar unidades lógicas de WPF. Ventana normal redimensionable, inicialmente 1100 × 720, mínimo 640 × 480. Fondo propio de paisaje sencillo con cielo, lago y montañas en capas, dibujado con recursos WPF locales. La captura del usuario orienta la composición. Los iconos de muestra llevan símbolos originales, etiquetas legibles y nombres accesibles. Evitar incorporar marcas gráficas externas. Mantener el mismo encuadre a tamaño de ventana constante.

## Secuencia

Un PR por tarea. Cada fila depende de la anterior ya fusionada; t01 parte de main actualizado. Los archivos enumerados en cada sección delimitan la tarea. Directorios abreviados: D = src/Canvas.Domain, U = src/Canvas.Desktop, T = tests/Canvas.Domain.Tests. Conservar el SDK y los controles existentes de Windows. Ejecutar en cada PR:

```powershell
dotnet build EscritorioCanvas.sln -c Release
dotnet test EscritorioCanvas.sln -c Release --no-build
git diff --check origin/main...HEAD
git diff --name-only origin/main...HEAD
```

CI verde, cobertura de al menos 60 % en lógica independiente modificada y evidencia por criterio. Los bloques xUnit son miembros de clases de pruebas del proyecto T con imports Xunit y Canvas.Domain. Para los cambios WPF comprobar además el recorrido manual indicado. Si hace falta otra dependencia, una decisión de interacción o una API privada, volver a Planificación. No usar llamadas Win32 en esta fase.

| Tarea | Rama | Resultado |
| --- | --- | --- |
| t01 | f07-t01-camara | Movimiento y zoom anclado |
| t02 | f07-t02-inicio | Límites y ocupación de Inicio |
| t03 | f07-t03-ventana | Ventana con paisaje fijo |
| t04 | f07-t04-espacios | Barra, Inicio y accesos por espacio |
| t05 | f07-t05-recorrido | Arrastre de fondo y rueda |
| t06 | f07-t06-iconos | Selección y recolocación |
| t07 | f07-t07-cierre | Evidencia comparada y cierre |

## t01 · Calcular la cámara

Crear D/CameraGestures.cs y T/CameraGesturesTests.cs. Clase estática pública CameraGestures con métodos Camera Pan(Camera start, Point delta) y Camera ZoomAt(Camera start, Point cursor, double steps). Pan resta delta/zoom a la posición inicial. ZoomAt calcula el punto de mundo bajo cursor con Geometry.ViewToWorld; el nuevo zoom multiplica por 1,25 elevado a steps y se limita a [0,25; 4]. Recalcular el origen para conservar ese punto bajo el cursor. Delta y cursor son relativos a la superficie; validar operandos finitos y cámara válida antes de operar. Resultado inválido lanza ArgumentOutOfRangeException.

```csharp
[Fact]
public void PanUsesViewDistance()
{
    var start = new Camera(new Point(10, 20), 2);
    Assert.Equal(new Camera(new Point(0, 25), 2), CameraGestures.Pan(start, new Point(20, -10)));
}
[Theory]
[InlineData(1, 1.25)]
[InlineData(100, 4)]
[InlineData(-100, 0.25)]
public void ZoomKeepsCursor(double steps, double expected)
{
    var c = new Camera(new Point(12, 40), 1);
    var p = new Point(100, 80);
    var next = CameraGestures.ZoomAt(c, p, steps);
    Assert.Equal(expected, next.Zoom);
    Assert.Equal(Geometry.ViewToWorld(p, c), Geometry.ViewToWorld(p, next));
}
```

## t02 · Resolver posiciones de iconos

Crear D/HomeGrid.cs y T/HomeGridTests.cs. Clase estática pública HomeGrid con Point Clamp(Point origin) y Point Snap(Point origin, IReadOnlyCollection<Point> occupied). El origen es la esquina superior izquierda de una ficha de 88 × 88. Los 16 orígenes válidos forman cuatro columnas y cuatro filas, desde (24,80) a intervalos de 96 unidades en ambos ejes. Clamp limita cada eje al rectángulo de orígenes. Snap elige la casilla libre más cercana por distancia euclídea al punto limitado. Empates por fila y luego columna. Excluir de occupied el icono que se mueve. Entrada no finita lanza ArgumentOutOfRangeException; cuadrícula llena lanza InvalidOperationException.

```csharp
[Fact]
public void PlacesAtBoundaryOrClosestFreeCell()
{
    Assert.Equal(new Point(24,368), HomeGrid.Clamp(new Point(-80,900)));
    Assert.Equal(new Point(312,368), HomeGrid.Snap(new Point(800,900), Array.Empty<Point>()));
    Assert.Equal(new Point(120,80), HomeGrid.Snap(new Point(24,80), new[]{new Point(24,80)}));
    Assert.Equal(new Point(24,80), HomeGrid.Snap(new Point(72,80), Array.Empty<Point>()));
}
```

## t03 · Abrir la ventana

Crear U/Canvas.Desktop.csproj, U/Program.cs y U/MainWindow.cs. Añadir el proyecto a EscritorioCanvas.sln. WinExe net10.0-windows con UseWPF, referencia a Domain, entrada STA de Program.Main() y MainWindow : Window. Componer con WPF desde C#. Superponer al paisaje un lienzo transparente que recibe eventos incluso en zona vacía; reservar capa superior para la barra. Separar las coordenadas de la vista de las del borde de ventana. Iniciar con `dotnet run -c Release --project src/Canvas.Desktop`. Verificar redimensionamiento y legibilidad. Adjuntar captura inicial de la ventana propia.

## t04 · Mostrar espacios e Inicio

Modificar U/MainWindow.cs y crear U/SpaceView.cs. Crear las dos entradas de SessionState con cámara e Inicio (0,0; 1) y ReturnView vacío. Al abrir, Programación está activo. Los iconos ocupan la primera y segunda fila de la primera columna, con sus nombres respectivos. SpaceView proyecta posición y escala mediante la cámara; mantener el paisaje y barra fuera de esa transformación. Mantener por espacio la relación entre identificador estable del icono y origen lógico. Botones/selector con nombre accesible y foco visible; barra compacta arriba, ajena al arrastre. Usar SwitchSpace y ToggleHome mediante Navigation.Apply. El botón dice «Volver a mi vista» mientras existe ReturnView, también si se mueve la cámara durante la visita. Conmutar al espacio actual conserva estado. Capturas de Programación y Juegos al mismo tamaño: mismo fondo, dos juegos distintos de etiquetas. Probar con teclado el selector y el botón.

## t05 · Recorrer la superficie

Modificar U/MainWindow.cs y U/SpaceView.cs. Al pulsar fondo libre, capturar el ratón y guardar posición de puntero y cámara; actualizar mediante SetCamera y Pan desde esos valores iniciales. Al soltar, liberar captura. La rueda llama ZoomAt con Delta/120.0 y las coordenadas del cursor en la superficie. Consumir la rueda sobre el lienzo, también encima de un icono. El clic en barra se procesa únicamente por su control. Cancelar gesto activo al perder captura, desactivar ventana o cambiar espacio; jamás aplicar un movimiento tardío al nuevo espacio. Verificar pan a zoom distinto de 1, zoom mínimo y máximo, Inicio/retorno y cámaras independientes. Adjuntar antes/después con igual tamaño de ventana y cursor sobre una referencia conocida.

## t06 · Seleccionar y arrastrar

Modificar U/MainWindow.cs y U/SpaceView.cs. Pulsar icono o etiqueta selecciona solo esa ficha y conserva el desplazamiento inicial entre puntero y ficha. Resaltar con borde y fondo translúcido. Al superar el umbral horizontal o vertical de arrastre del sistema, calcular el delta en mundo con Geometry.ViewToWorld; aplicar Clamp durante el gesto. Congelar la cámara durante este arrastre. Soltar confirma Snap en el espacio propietario. Escape, desactivación o pérdida de captura antes de confirmar restaura la casilla inicial. Clic en fondo limpia selección; cambio de espacio limpia selección y conserva los acomodos. Doble clic conserva la selección. Verificar al 100 % y tras zoom: mover solo Codex, intentar ocupar VS Code, atravesar cada borde de Inicio, cancelar, cambiar de espacio y volver. Capturas de selección, arrastre al borde y acomodo recuperado. Una ficha nunca tapa el rótulo contiguo al soltar.

## t07 · Reproducir y cerrar

Ejecución crea docs/desarrollo/07-lienzo/recorrido.md. Revisión añade docs/inspeccion/f07-cierre.md y actualiza README.md y docs/plan-por-fases.md en ese PR. Registrar SHA, Windows, DPI, SDK, tamaño de ventana, resultados xUnit y CI. Enlazar evidencia de cada tarea y anotar esfuerzo medido por rol. Repetir el recorrido: mover Codex a una casilla libre, acercar sobre él, capturar A, visitar Inicio, mover la vista allí, volver y capturar B; visitar Juegos, mover su vista y volver a Programación para C. Comparar el borde y centro de la ficha en A/B/C a tamaño y DPI constantes (tolerancia de rasterización: 1 píxel físico); comprobar encuadre de paisaje y barra con dos puntos de referencia fijos. Caso negativo: soltar sobre una ficha ocupada deja ambos iconos en casillas distintas. La verificación del cambio de espacio incluye los nombres visibles en ambos.

## Evidencia y condiciones de parada

Los PR t03–t07 requieren capturas reales con acción, estado y SHA en el pie. Adjuntar imágenes al PR o enlazar evidencia accesible al revisor. Recortar a la ventana propia y comprobar ausencia de datos personales. Ensayo interactivo en Windows solo tras «adelante» en la sesión de Ejecución. Usar una instancia creada por el ensayo, con PID y ejecutable del checkout de ensayo identificados; cerrar solo esa instancia y limpiar archivos temporales. Sin confirmación, registrar el criterio visual como pendiente. Mantener pruebas automáticas independientes de ventanas. Revisión comprueba en cada PR la lista de archivos y la CI; cada criterio termina verificado, fallido o pendiente con evidencia. Cerrar la fase al completar los recorridos y su evidencia visual.
