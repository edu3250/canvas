# Evidencia de salida y retorno al canvas

**Resultado verificado el 18 de noviembre de 2026.** El modo `layout` terminó con código 0. Las 12 lecturas del recorrido pasaron. Al salir del canvas por segunda vez, ambas ventanas recuperaron los mismos rectángulos que en la primera salida. El programa confirmó una restauración final de cada ventana, la salida de ambos procesos y la eliminación del directorio temporal.

## Montaje y reproducción

Windows 11 25H2, compilación 26200.9974, sesión interactiva 1. Monitor primario con área de trabajo de 1920 × 1032 px, origen (0, 0), medida con `GetMonitorInfoW`. Las dos ventanas registraron 96 DPI con `GetDpiForWindow`. Se usaron .NET SDK 10.0.401, Windows Desktop 10.0.12 y PanelNativo 1.0.0.0 en el editor WinForms y la ventana vecina. El recorrido realiza transiciones programáticas sobre dos ventanas descartables propias. [Comandos de reproducción](../../prototipos/panel-nativo/README.md#ensayo-de-salida-y-retorno)

Se compiló con `dotnet build prototipos/panel-nativo/PanelNativo.csproj` al terminar el adaptador y al integrar el recorrido. Ambas compilaciones terminaron con 0 errores y 0 advertencias. `layout --auto` y `control --invalido` devolvieron 2. Se comprobó en código la continuidad de las ramas de `control`, `code` y `--fixture`. [Adaptador Win32](https://github.com/edu3250/escritorio-canvas/blob/cdc0390704ef85b4f2c92f959d5f0576105fb598/prototipos/panel-nativo/WindowPlacementApi.cs) · [Entrada del prototipo](https://github.com/edu3250/escritorio-canvas/blob/cdc0390704ef85b4f2c92f959d5f0576105fb598/prototipos/panel-nativo/Program.cs)

## Lecturas observadas

Registro local `evidencia-local/control-4f10e4318973/sesion.txt`. A corresponde al editor (PID 8436, HWND `0x40102`) y B a la vecina (PID 2628, HWND `0xA80B1E`). Los valores se expresan como `(x, y, ancho, alto)` en píxeles. El rectángulo normal proviene de `GetWindowPlacement` y usa coordenadas del área de trabajo. El rectángulo de pantalla proviene de `GetWindowRect` e incluye el marco nativo. Estado 1 significa normal y 3 maximizado. Cada fila obtuvo dos lecturas consecutivas iguales. En todos los bordes con referencia previa la diferencia medida fue de 0 px. D0 B establece la referencia de pantalla para las dos salidas. [Oráculo de medición](../desarrollo/03-salida-y-retorno/tareas/02-recorrido.md#puntos-de-medición-dentro-de-layouttrialexecute) · [Recorrido ejecutado](https://github.com/edu3250/escritorio-canvas/blob/cdc0390704ef85b4f2c92f959d5f0576105fb598/prototipos/panel-nativo/LayoutTrial.cs)

| Etapa | Ventana | Estado | Rectángulo normal | Rectángulo de pantalla | Resultado |
| --- | --- | ---: | --- | --- | --- |
| D0 | A | 1 | (20, 40, 540, 400) | (20, 40, 540, 400) | Verificado |
| D0 | B | 3 | (580, 70, 540, 440) | (−8, −8, 1936, 1048) | Verificado |
| C1 | A | 1 | (30, 80, 480, 500) | (30, 80, 480, 500) | Verificado |
| C1 | B | 1 | (550, 120, 500, 450) | (550, 120, 500, 450) | Verificado |
| C2 | A | 1 | (65, 110, 510, 460) | (65, 110, 510, 460) | Verificado |
| C2 | B | 1 | (550, 120, 500, 450) | (550, 120, 500, 450) | Verificado |
| D1 | A | 1 | (20, 40, 540, 400) | (20, 40, 540, 400) | Verificado |
| D1 | B | 3 | (580, 70, 540, 440) | (−8, −8, 1936, 1048) | Verificado |
| C3 | A | 1 | (65, 110, 510, 460) | (65, 110, 510, 460) | Verificado |
| C3 | B | 1 | (550, 120, 500, 450) | (550, 120, 500, 450) | Verificado |
| D2 | A | 1 | (20, 40, 540, 400) | (20, 40, 540, 400) | Verificado |
| D2 | B | 3 | (580, 70, 540, 440) | (−8, −8, 1936, 1048) | Verificado |

## Criterios del encargo

| Criterio | Estado | Evidencia del registro |
| --- | --- | --- |
| Entrar y cambiar el acomodo | **Verificado** | C1 y C2 muestran ambas ventanas normales y a 0 px por borde de los destinos solicitados. |
| Salir al escritorio | **Verificado** | D1 recuperó el estado y ambos rectángulos de D0 para A y B. |
| Volver y repetir | **Verificado** | C3 recuperó los rectángulos de pantalla de C2. D2 volvió a igualar D0 en las dos ventanas. |
| Terminar | **Verificado** | Las dos filas de limpieza verificaron la restauración. Los dos PID registraron `terminado=True`, sin cierre forzado, y el temporal registró `eliminado=True`. |

La revisión independiente contrastó estas mediciones con el registro local y comprobó la implementación. [Cierre de fase y dictamen](f03-cierre.md).
