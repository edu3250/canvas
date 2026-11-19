# Evidencia de dos acomodos con VS Code

**Estado: t01 y t02 verificadas por Ejecución; revisión independiente completada; véase el cierre de fase.** Plan: 957c66f6e192d29f08d409590591815dcf3d8242. Ensayo autorizado mediante «adelante» en esta sesión y ejecutado sobre dfbea836dd6c9ed80e84953f7645ae4dc4541e1e; código de salida 0.

## Observado en ejecución no interactiva

18 de noviembre de 2026. Windows 11 25H2, compilación 26200.9974 (registro de Windows); SDK 10.0.401 (dotnet --version), runtime instalado Windows Desktop 10.0.12 (dotnet --list-runtimes). Ensayo: VS Code 1.147.0, PanelNativo 1.0.0.0, runtime 10.0.12, sesión 1 interactiva; ambas ventanas a 96 DPI y área primaria (0,0,1920,1032) px. Fuente: registro del ensayo.

Dos compilaciones correctas, 0 errores y 0 advertencias; segunda tras ajuste de sangría. layout-code --auto y layout --auto devolvieron 2. Directorios temporales PanelNativo-* antes y después: 0. Una primera comprobación prematura no encontró el ejecutable mientras compilaba y no cuenta como validación; se repitió tras terminar. git diff --check sin incidencias.

## Confirmado en código

[Program.cs](../../prototipos/panel-nativo/Program.cs): entrada exacta layout-code, recursos separados A/B, registro inmediato de procesos y asociación PID-documento. IsOurs exige PID registrado vivo y título esperado. Cierre de ventana condicionado a identidad; timeout conserva terminación y comprobación de salida. Se conservan las ramas control, code, layout y --fixture. No se modificaron LayoutTrial ni WindowPlacementApi. Estas comprobaciones no prueban limpieza o geometría real.

## Observado en el ensayo interactivo

Registro local excluido de Git: evidencia-local/layout-code-a79249bd4d05/sesion.txt. A: PID 23752, HWND 0x150F4E, documento panel-a79249bd4d05.txt. B: PID 10124, HWND 0x140F76, documento neighbor-a79249bd4d05.txt. Los perfiles perfil-A y perfil-B y carpetas de extensiones ext-A y ext-B son distintos bajo el temporal propio; asignación confirmada en Program.cs del SHA ejecutado.

[Oráculo](../desarrollo/04-acomodos-vscode/tareas/02-evidencia.md). Rectángulos (x,y,ancho,alto) en px: normal en coordenadas de área de trabajo, pantalla mediante GetWindowRect. Estado 1 normal, 3 maximizado. Cada fila pasó la exigencia de dos lecturas consecutivas estables. «Sin referencia» establece una medida, no una diferencia cero. D0 B conservó el rectángulo normal previo a maximizar.

| Etapa | Sujeto | Estado esperado / obtenido | Normal esperado → obtenido | Pantalla esperado → obtenido | Diferencias (px) | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| D0 | A | 1 / 1 | (20,40,540,400) → (20,40,540,400) | (20,40,540,400) → (20,40,540,400) | 0 en bordes con referencia | Verificado |
| D0 | B | 3 / 3 | (580,70,540,440) → (580,70,540,440) | sin referencia → (-8,-8,1936,1048) | 0 en bordes con referencia | Verificado |
| C1 | A | 1 / 1 | sin referencia → (30,80,480,500) | (30,80,480,500) → (30,80,480,500) | 0 en bordes con referencia | Verificado |
| C1 | B | 1 / 1 | sin referencia → (550,120,500,450) | (550,120,500,450) → (550,120,500,450) | 0 en bordes con referencia | Verificado |
| C2 | A | 1 / 1 | sin referencia → (65,110,510,460) | (65,110,510,460) → (65,110,510,460) | 0 en bordes con referencia | Verificado |
| C2 | B | 1 / 1 | sin referencia → (550,120,500,450) | (550,120,500,450) → (550,120,500,450) | 0 en bordes con referencia | Verificado |
| D1 | A | 1 / 1 | (20,40,540,400) → (20,40,540,400) | (20,40,540,400) → (20,40,540,400) | 0 en bordes con referencia | Verificado |
| D1 | B | 3 / 3 | (580,70,540,440) → (580,70,540,440) | (-8,-8,1936,1048) → (-8,-8,1936,1048) | 0 en bordes con referencia | Verificado |
| C3 | A | 1 / 1 | sin referencia → (65,110,510,460) | (65,110,510,460) → (65,110,510,460) | 0 en bordes con referencia | Verificado |
| C3 | B | 1 / 1 | sin referencia → (550,120,500,450) | (550,120,500,450) → (550,120,500,450) | 0 en bordes con referencia | Verificado |
| D2 | A | 1 / 1 | (20,40,540,400) → (20,40,540,400) | (20,40,540,400) → (20,40,540,400) | 0 en bordes con referencia | Verificado |
| D2 | B | 3 / 3 | (580,70,540,440) → (580,70,540,440) | (-8,-8,1936,1048) → (-8,-8,1936,1048) | 0 en bordes con referencia | Verificado |

Las dos restauraciones finales pasaron: A estado 1 y B estado 3, rectángulos normal y de pantalla iguales a D0, diferencias de 0 px por borde. El registro indica cierre forzado del proceso propio A (PID 23752) tras el timeout de cierre; después terminado=True. B (PID 10124) terminó sin cierre forzado registrado. Temporal eliminado=True. Código de salida observado: 0.

| Criterio | Estado | Evidencia |
| --- | --- | --- |
| Identidad A/B propia | Verificado | Asociación PID, HWND y documentos del registro; perfiles separados en código. |
| Entrar y cambiar | Verificado | C1 y C2, estado exacto y diferencias de 0 px. |
| Salir y volver sin deriva | Verificado | D1 y D2 iguales a D0; C3 igual a C2. |
| Terminar sin residuos | Verificado | Restauraciones finales, ambos procesos terminados y temporal eliminado; cierre forzado de A registrado. |
| Modos anteriores conservados | Verificado | Compilación, argumentos y revisión del diff descritos arriba; sin repetir interacción histórica. |

**Inferido:** el resultado respalda estas transiciones programáticas en este entorno. No demuestra navegación por controles, edición, foco, reinicio, recuperación tras caída ni otras escalas o aplicaciones.

[Comando reproducible](../../prototipos/panel-nativo/README.md#dos-acomodos-con-vs-code). [Revisión independiente y cierre de fase](f04-cierre.md).

## Esfuerzo

**Registro histórico antes del permiso interactivo:** Codex, GPT-6 (familia declarada por la sesión; variante no verificada). Una pasada de implementación, dos compilaciones y dos intentos del par de argumentos, cero ensayos interactivos. Estimación retrospectiva sin cronometraje: implementación 4 min, preparación documental e integración 3 min, comprobaciones Windows 2 min. Revisión humana y ensayo pendientes, sin tiempo medido.


Tras el permiso: una compilación adicional (0 errores, 0 advertencias), un ensayo interactivo y una actualización documental. Esfuerzo adicional estimado retrospectivamente: comprobaciones Windows 1 min, evidencia e integración 2 min. Totales de Ejecución estimados: implementación 4 min, documentación e integración 5 min, comprobaciones Windows 3 min; 12 min en total. Tres compilaciones y un ensayo interactivo acumulados. Revisión humana sin medición.
