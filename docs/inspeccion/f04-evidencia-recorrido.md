# Evidencia de dos acomodos con VS Code

**Estado: t01 y t02 verificadas por Ejecuci?n; revisi?n independiente pendiente.** Plan: 957c66f6e192d29f08d409590591815dcf3d8242. Ensayo autorizado mediante ?adelante? en esta sesi?n y ejecutado sobre dfbea836dd6c9ed80e84953f7645ae4dc4541e1e; c?digo de salida 0.

## Observado en ejecuci?n no interactiva

18 de noviembre de 2026. Windows 11 25H2, compilaci?n 26200.9974 (registro de Windows); SDK 10.0.401 (dotnet --version), runtime instalado Windows Desktop 10.0.12 (dotnet --list-runtimes). Ensayo: VS Code 1.147.0, PanelNativo 1.0.0.0, runtime 10.0.12, sesi?n 1 interactiva; ambas ventanas a 96 DPI y ?rea primaria (0,0,1920,1032) px. Fuente: registro del ensayo.

Dos compilaciones correctas, 0 errores y 0 advertencias; segunda tras ajuste de sangr?a. layout-code --auto y layout --auto devolvieron 2. Directorios temporales PanelNativo-* antes y despu?s: 0. Una primera comprobaci?n prematura no encontr? el ejecutable mientras compilaba y no cuenta como validaci?n; se repiti? tras terminar. git diff --check sin incidencias.

## Confirmado en c?digo

[Program.cs](../../prototipos/panel-nativo/Program.cs): entrada exacta layout-code, recursos separados A/B, registro inmediato de procesos y asociaci?n PID-documento. IsOurs exige PID registrado vivo y t?tulo esperado. Cierre de ventana condicionado a identidad; timeout conserva terminaci?n y comprobaci?n de salida. Se conservan las ramas control, code, layout y --fixture. No se modificaron LayoutTrial ni WindowPlacementApi. Estas comprobaciones no prueban limpieza o geometr?a real.

## Observado en el ensayo interactivo

Registro local excluido de Git: evidencia-local/layout-code-a79249bd4d05/sesion.txt. A: PID 23752, HWND 0x150F4E, documento panel-a79249bd4d05.txt. B: PID 10124, HWND 0x140F76, documento neighbor-a79249bd4d05.txt. Los perfiles perfil-A y perfil-B y carpetas de extensiones ext-A y ext-B son distintos bajo el temporal propio; asignaci?n confirmada en Program.cs del SHA ejecutado.

[Or?culo](../desarrollo/04-acomodos-vscode/tareas/02-evidencia.md). Rect?ngulos (x,y,ancho,alto) en px: normal en coordenadas de ?rea de trabajo, pantalla mediante GetWindowRect. Estado 1 normal, 3 maximizado. Cada fila pas? la exigencia de dos lecturas consecutivas estables. ?Sin referencia? establece una medida, no una diferencia cero. D0 B conserv? el rect?ngulo normal previo a maximizar.

| Etapa | Sujeto | Estado esperado / obtenido | Normal esperado ? obtenido | Pantalla esperado ? obtenido | Diferencias (px) | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| D0 | A | 1 / 1 | (20,40,540,400) ? (20,40,540,400) | (20,40,540,400) ? (20,40,540,400) | 0 en bordes con referencia | Verificado |
| D0 | B | 3 / 3 | (580,70,540,440) ? (580,70,540,440) | sin referencia ? (-8,-8,1936,1048) | 0 en bordes con referencia | Verificado |
| C1 | A | 1 / 1 | sin referencia ? (30,80,480,500) | (30,80,480,500) ? (30,80,480,500) | 0 en bordes con referencia | Verificado |
| C1 | B | 1 / 1 | sin referencia ? (550,120,500,450) | (550,120,500,450) ? (550,120,500,450) | 0 en bordes con referencia | Verificado |
| C2 | A | 1 / 1 | sin referencia ? (65,110,510,460) | (65,110,510,460) ? (65,110,510,460) | 0 en bordes con referencia | Verificado |
| C2 | B | 1 / 1 | sin referencia ? (550,120,500,450) | (550,120,500,450) ? (550,120,500,450) | 0 en bordes con referencia | Verificado |
| D1 | A | 1 / 1 | (20,40,540,400) ? (20,40,540,400) | (20,40,540,400) ? (20,40,540,400) | 0 en bordes con referencia | Verificado |
| D1 | B | 3 / 3 | (580,70,540,440) ? (580,70,540,440) | (-8,-8,1936,1048) ? (-8,-8,1936,1048) | 0 en bordes con referencia | Verificado |
| C3 | A | 1 / 1 | sin referencia ? (65,110,510,460) | (65,110,510,460) ? (65,110,510,460) | 0 en bordes con referencia | Verificado |
| C3 | B | 1 / 1 | sin referencia ? (550,120,500,450) | (550,120,500,450) ? (550,120,500,450) | 0 en bordes con referencia | Verificado |
| D2 | A | 1 / 1 | (20,40,540,400) ? (20,40,540,400) | (20,40,540,400) ? (20,40,540,400) | 0 en bordes con referencia | Verificado |
| D2 | B | 3 / 3 | (580,70,540,440) ? (580,70,540,440) | (-8,-8,1936,1048) ? (-8,-8,1936,1048) | 0 en bordes con referencia | Verificado |

Las dos restauraciones finales pasaron: A estado 1 y B estado 3, rect?ngulos normal y de pantalla iguales a D0, diferencias de 0 px por borde. El registro indica cierre forzado del proceso propio A (PID 23752) tras el timeout de cierre; despu?s terminado=True. B (PID 10124) termin? sin cierre forzado registrado. Temporal eliminado=True. C?digo de salida observado: 0.

| Criterio | Estado | Evidencia |
| --- | --- | --- |
| Identidad A/B propia | Verificado | Asociaci?n PID, HWND y documentos del registro; perfiles separados en c?digo. |
| Entrar y cambiar | Verificado | C1 y C2, estado exacto y diferencias de 0 px. |
| Salir y volver sin deriva | Verificado | D1 y D2 iguales a D0; C3 igual a C2. |
| Terminar sin residuos | Verificado | Restauraciones finales, ambos procesos terminados y temporal eliminado; cierre forzado de A registrado. |
| Modos anteriores conservados | Verificado | Compilaci?n, argumentos y revisi?n del diff descritos arriba; sin repetir interacci?n hist?rica. |

**Inferido:** el resultado respalda estas transiciones program?ticas en este entorno. No demuestra navegaci?n por controles, edici?n, foco, reinicio, recuperaci?n tras ca?da ni otras escalas o aplicaciones.

[Comando reproducible](../../prototipos/panel-nativo/README.md#dos-acomodos-con-vs-code). Revisi?n independiente y cierre de fase pendientes.

## Esfuerzo

Codex, GPT-6 (familia declarada por la sesi?n; variante no verificada). Una pasada de implementaci?n, dos compilaciones y dos intentos del par de argumentos, cero ensayos interactivos. Estimaci?n retrospectiva sin cronometraje: implementaci?n 4 min, preparaci?n documental e integraci?n 3 min, comprobaciones Windows 2 min. Revisi?n humana y ensayo pendientes, sin tiempo medido.


Tras el permiso: una compilaci?n adicional (0 errores, 0 advertencias), un ensayo interactivo y una actualizaci?n documental. Esfuerzo adicional estimado retrospectivamente: comprobaciones Windows 1 min, evidencia e integraci?n 2 min. Totales de Ejecuci?n estimados: implementaci?n 4 min, documentaci?n e integraci?n 5 min, comprobaciones Windows 3 min; 12 min en total. Tres compilaciones y un ensayo interactivo acumulados. Revisi?n humana sin medici?n.
