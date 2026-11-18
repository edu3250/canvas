# f03-t02 · Ejecutar dos viajes entre escritorio y canvas

**Rama:** f03-plan, PR #2. Depende de t01 verificada. Sin ensayo interactivo en esta tarea.

**Archivos permitidos:** modificar `prototipos/panel-nativo/Program.cs` y crear `prototipos/panel-nativo/LayoutTrial.cs`. Añadir el argumento exacto `layout`; reutilizar Run("control"), Start, Log, IsOurs y Dispose. Mantener los modos anteriores. Entrada de la clase: `internal static int Execute(Run run)`. Usar el adaptador de t01. Máximo aproximado de 300 líneas de diff.

1. Antes de cada cambio comprobar identidad por PID y documento único. Tomar el área de trabajo primaria en píxeles y exigir al menos 1200 × 800. Todos los puntos siguientes son desplazamientos desde su origen. Fijar las ventanas normales en (20,40,540,400) y (580,70,540,440). Maximizar la segunda con ShowWindow(SW_MAXIMIZE). Esperar a que se estabilicen. Capturar Placement y GetWindowRect originales de ambas. Exigir estado normal y maximizado respectivamente.
2. Normalizar y colocar los paneles en (30,80,480,500) y (550,120,500,450). Modificar el primero a (65,110,510,460). Verificar y conservar los dos rectángulos reales finales. Al salir, restaurar ambos Placement originales. Verificar showCmd, rcNormalPosition y GetWindowRect contra los originales.
3. Reentrar normalizando y aplicando los rectángulos del canvas guardados. Verificar ambos y volver a salir con las mismas comprobaciones. En cada transición sondear cada 100 ms con tope de 3 s y exigir dos lecturas consecutivas coincidentes; tolerancia de 1 píxel por borde, estado exacto. Registrar esperado, obtenido, identidad, DPI y transición. Medir el rectángulo normal con GetWindowPlacement y el visible con GetWindowRect.
4. En finally intentar la restauración de cada ventana propia que cuente con instantánea, incluso si falla la otra. Registrar fallo y devolver código 1 ante discrepancia, timeout o pérdida de identidad; 0 solo si todos los criterios pasan. Conservar el cierre de procesos y la limpieza propios de Run. Si cambia el monitor o el DPI durante el recorrido, invalidar el ensayo.

**Aceptación:** `dotnet build prototipos/panel-nativo/PanelNativo.csproj` y revisión del recorrido, ramas de error y compatibilidad de argumentos. En el PR distinguir compilación verificada de ejecución pendiente. Ante ambigüedad, tres intentos fallidos o necesidad de otro archivo o dependencia, usar el [informe de bloqueo](../../../plantillas/informe-bloqueo.md).
## Punto de integración y sujetos de la medición

En `Program.Main`, reconocer el argumento único `layout` antes de la validación que admite `control|code`. Para ese argumento construir `Run("control")`, llamar a `run.Start()` y pasar el resultado a `LayoutTrial.Execute(run)` en lugar de construir `Host`. Conservar el bloque de limpieza y la comprobación final de `run.Clean`. La rama `--fixture` debe seguir iniciando los procesos hijos. [Punto de entrada y creación de ventanas](../../../../prototipos/panel-nativo/Program.cs)

| Sujeto fijo para toda la ejecución | Handle y PID que recibe Execute | Estado inicial que se construye |
| --- | --- | --- |
| A, editor de texto de control | `run.Candidate`, `run.CandidatePid` | Normal |
| B, ventana verde vecina | `run.Neighbor`, `run.NeighborPid` | Maximizada después de fijar su rectángulo normal |

Ambas son ventanas principales de procesos distintos que inició este mismo Run con el archivo temporal único de la ejecución. Comprobar `run.IsOurs(hwnd, pid)` antes de medir y antes de mover o restaurar, incluso en la limpieza. Una identidad perdida invalida ese sujeto. Nunca buscar un reemplazo por orden de aparición, posición o ventana activa.

## Puntos de medición dentro de LayoutTrial.Execute

Para cada fila tomar una lectura por HWND una vez estabilizada la transición. Leer `Placement.showCmd` y `rcNormalPosition` mediante Capture y los bordes visibles con GetWindowRect. Comparar cada rectángulo con una referencia en su propio sistema de coordenadas. Escribir en `run.Log` etapa, A/B, HWND, PID, DPI, estado, esperado, obtenido, diferencias por borde y resultado.

| Etapa | Momento exacto | Referencia de aceptación |
| --- | --- | --- |
| D0 | Tras preparar A normal y B maximizada, antes de entrar | Guardar dos instantáneas inmutables, una por ventana. Para B guardar también su rectángulo normal antes de maximizar y cotejarlo con rcNormalPosition después. Sus límites visibles maximizados se miden en el monitor real. |
| C1 | Tras colocar las dos ventanas en el canvas simulado | Ambas normales y cada GetWindowRect igual al rectángulo solicitado en el paso 2. |
| C2 | Tras modificar A, antes de salir | Ambas normales. Validar los nuevos bordes de A y los anteriores de B; guardar los rectángulos efectivos de ambas como destino de retorno. |
| D1 | Tras la primera salida y antes de reentrar | Para cada HWND comparar estado, rectángulo normal y bordes visibles con D0. |
| C3 | Tras la reentrada y antes de volver a salir | Ambas normales y bordes visibles de cada HWND iguales a sus valores guardados en C2. |
| D2 | Tras la segunda salida, antes de Dispose | Repetir la comparación con D0, incluso si D1 pasó. |

Ejemplo de fallo: si en D1 B tiene los mismos bordes visibles que en D0 pero showCmd indica normal, D1 falla. Registrar la discrepancia, intentar la restauración final de las ventanas propias y devolver 1. La evidencia de este modo valida transiciones programáticas; una prueba posterior de los controles de salida de la UI tendría otro punto de entrada.
