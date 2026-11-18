# Ensayo de dos paneles con una ventana nativa activa

**Resultado del 18 de noviembre de 2026.** VS Code aceptó los dos rectángulos, recibió la escritura después de un clic sobre la miniatura y recuperó el foco al cancelar el diálogo Abrir. La misma línea pasó de **3 a 11 renglones** con el mismo tamaño de letra. La miniatura del vecino siguió visible en su posición. El control WinForms completó el mismo recorrido. Recomiendo conservar la activación en el rectángulo del panel y dedicar el siguiente ensayo a salir y recuperar el acomodo original de las ventanas. La decisión de iniciar esa fase corresponde al usuario.

## Montaje

Windows 11 25H2, compilación 26200.9974, sesión interactiva 1. Monitor primario de 1920 × 1080, área de trabajo 1920 × 1032, origen (0, 0). Host y candidato registraron 96 DPI. Se usaron .NET SDK 10.0.401, runtime de Windows Desktop 10.0.12 y VS Code 1.146.0. La compilación final terminó con cero errores y cero advertencias.

El host WinForms proyectó con DWM dos ventanas creadas para el ensayo. El editor de control es un cuadro de texto multilínea en otro proceso, con ajuste de línea y Consolas de 12 puntos. VS Code abrió el documento con un perfil temporal, ajuste de línea activado, fuente de 16 px, interlínea de 24 px y chat desactivado. La primera línea contiene siete repeticiones de la misma frase. Las marcas se escriben en la segunda. Se comprobó la identidad del HWND mediante el PID recién iniciado y el nombre único del archivo antes de moverlo. [Código y comandos de reproducción](../../prototipos/panel-nativo/README.md)

Se usó entrada sintetizada de ratón y teclado en el escritorio interactivo. El mismo manejador de clic de la interfaz llama a `SetWindowPos` y `SetForegroundWindow`. Cada selección de ancho volvió primero a la vista previa. En los cuatro casos, 15 de 15 puntos muestreados dentro de la primera miniatura pertenecían al host y tenían un color distinto de su fondo. La selección del ancho y el clic de activación son dos acciones distintas. Un solo clic sobre la miniatura bastó para que el editor recibiera las teclas. La posición del cursor de texto se eligió con Ctrl+Fin.

## Resultados por aplicación y tamaño

Todos los rectángulos son coordenadas de pantalla en píxeles físicos, medidas con `GetWindowRect`. Se incluye el marco nativo. El punto superior izquierdo solicitado y obtenido fue **(8, 44)** en los cuatro casos.

| Editor | Solicitado | Obtenido | Renglones de la línea 1 | HWND en primer plano, de vista a editor | Diálogo Abrir |
| --- | --- | --- | --- | --- | --- |
| Control | 1475 × 800 | 1475 × 800 | 3 | `0xAE08AE` → `0x160A54` | `0x550940` |
| Control | 492 × 800 | 492 × 800 | 7 | `0xAE08AE` → `0x160A54` | `0x80ACC` |
| VS Code | 1475 × 800 | 1475 × 800 | 3 | `0xB0AE0` → `0x150A90` | `0x2C0A48` |
| VS Code | 492 × 800 | 492 × 800 | 11 | `0xB0AE0` → `0x150A90` | `0x290B20` |

| Criterio | Estado | Evidencia |
| --- | --- | --- |
| Un clic permite escribir | **Verificado** | En los cuatro casos el HWND activo cambió del host al candidato. Ctrl+Fin, escritura y Ctrl+S dejaron la primera marca en el archivo temporal. |
| Edición y ajuste de línea a dos tamaños | **Verificado** | Se contaron los renglones en capturas del HWND real. En VS Code, la palabra inicial «muestra» ocupó 10 px de tinta vertical a ambos anchos, medidos en las columnas 137–195 de cada imagen. La separación entre renglones fue de 24 px. Ninguna aplicación aumentó los anchos solicitados para imponer un mínimo en estos dos puntos. |
| Cancelación y segunda escritura | **Verificado** | Ctrl+O abrió cuatro diálogos Win32 de clase `#32770`. Cada uno tuvo al editor como propietario raíz. Un clic en Cancelar devolvió el primer plano al HWND previo. Ambos archivos terminaron con `ANTES1DESPUES1ANTES2DESPUES2` en su segunda línea. |
| Vecino visible al editar y al abrir el diálogo | **Verificado en el punto muestreado** | En (1832, 104), centro de la miniatura del vecino, el HWND bajo el punto seguía perteneciendo al host y el píxel del escritorio tenía el verde esperado, RGB (31, 164, 111). Se comprobó al entrar al editor, con el diálogo abierto y tras la segunda escritura, en los cuatro recorridos. |
| Salida y repetición | **Verificado** | Las ejecuciones finales `control-2ff7fba237b7` y `code-53abedba43ec` terminaron con código 0. Se confirmó la salida de los PID 23248 y 20308 en el control y 17076 y 11056 en VS Code, y la eliminación de ambos directorios temporales. El [prototipo](../../prototipos/panel-nativo/README.md) permite repetir la prueba automática o manual. |

**Incidencias observadas.** En los primeros intentos se envió Escape en cuanto apareció el diálogo y el editor no recuperó el foco dentro de 4,5 s. En otro intento el centro de Cancelar aún estaba cubierto. El host interrumpió la entrada y cerró los procesos descartables, en esos intentos con terminación forzada del candidato y eliminación del temporal. El recorrido final esperó la inicialización y verificó que el botón estuviera expuesto antes de hacer clic. Los cuatro diálogos de la tabla se cancelaron con clic y ambos recorridos finales cerraron los procesos normalmente.

**Alcance de la decisión.** Las capturas permiten inspeccionar el contenido del editor y el registro permite comprobar su posición. El muestreo del vecino demuestra que su contenido llegó al escritorio en los instantes medidos. Permanecen pendientes la observación humana, la cobertura completa de la miniatura durante transiciones y el comportamiento de otras aplicaciones, escalas y monitores. El primer clic activa el panel. La entrega de ese mismo clic a una posición de texto concreta queda pendiente. La restauración de ventanas previamente abiertas, la recuperación tras reinicio y la carga de 15 ventanas requieren sus propios ensayos. La comparación con CWC sigue sustentada en su [inspección estática](informe-cwc.md).
