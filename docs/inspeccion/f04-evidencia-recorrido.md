# Evidencia de dos acomodos con VS Code

**Estado: t01 verificada sin interacci?n; t02 preparada y pendiente de permiso.** Plan: 957c66f6e192d29f08d409590591815dcf3d8242. No se ejecut? layout-code ni se observaron ventanas de VS Code.

## Observado en ejecuci?n no interactiva

18 de noviembre de 2026. Windows 11 25H2, compilaci?n 26200.9974 (registro de Windows); SDK 10.0.401 (dotnet --version), runtime instalado Windows Desktop 10.0.12 (dotnet --list-runtimes). DPI, ?rea y versi?n de VS Code usada: pendientes del ensayo.

Dos compilaciones correctas, 0 errores y 0 advertencias; segunda tras ajuste de sangr?a. layout-code --auto y layout --auto devolvieron 2. Directorios temporales PanelNativo-* antes y despu?s: 0. Una primera comprobaci?n prematura no encontr? el ejecutable mientras compilaba y no cuenta como validaci?n; se repiti? tras terminar. git diff --check sin incidencias.

## Confirmado en c?digo

[Program.cs](../../prototipos/panel-nativo/Program.cs): entrada exacta layout-code, recursos separados A/B, registro inmediato de procesos y asociaci?n PID-documento. IsOurs exige PID registrado vivo y t?tulo esperado. Cierre de ventana condicionado a identidad; timeout conserva terminaci?n y comprobaci?n de salida. Se conservan las ramas control, code, layout y --fixture. No se modificaron LayoutTrial ni WindowPlacementApi. Estas comprobaciones no prueban limpieza o geometr?a real.

## Mediciones pendientes

[Or?culo](../desarrollo/04-acomodos-vscode/tareas/02-evidencia.md). Rect?ngulos (x,y,ancho,alto) en px relativos al ?rea primaria; estado 1 normal, 3 maximizado. No hay registro interactivo ni diferencias medidas.

| Etapa | Sujeto | Esperado | Obtenido | Diferencias | Estado |
| --- | --- | --- | --- | --- | --- |
| D0 | A | 1; (20,40,540,400) | Sin medir | Sin medir | Pendiente |
| D0 | B | 3; normal previo (580,70,540,440) | Sin medir | Sin medir | Pendiente |
| C1 | A | 1; (30,80,480,500) | Sin medir | Sin medir | Pendiente |
| C1 | B | 1; (550,120,500,450) | Sin medir | Sin medir | Pendiente |
| C2 | A | 1; (65,110,510,460) | Sin medir | Sin medir | Pendiente |
| C2 | B | 1; igual C1 | Sin medir | Sin medir | Pendiente |
| D1 | A | Estado y rect?ngulos D0 | Sin medir | Sin medir | Pendiente |
| D1 | B | Estado y rect?ngulos D0 | Sin medir | Sin medir | Pendiente |
| C3 | A | 1; visible C2 | Sin medir | Sin medir | Pendiente |
| C3 | B | 1; visible C2 | Sin medir | Sin medir | Pendiente |
| D2 | A | Estado y rect?ngulos D0 | Sin medir | Sin medir | Pendiente |
| D2 | B | Estado y rect?ngulos D0 | Sin medir | Sin medir | Pendiente |

Identidad real A/B, restauraciones finales, salida de procesos, eliminaci?n del temporal y c?digo de salida interactivo: **pendientes**, falta permiso. Compatibilidad de argumentos y compilaci?n: **verificadas** con las comprobaciones anteriores. Ning?n resultado geom?trico inferido.

[Comando reproducible](../../prototipos/panel-nativo/README.md#dos-acomodos-con-vs-code). Revisi?n independiente y cierre de fase pendientes.

## Esfuerzo

Codex, GPT-6 (familia declarada por la sesi?n; variante no verificada). Una pasada de implementaci?n, dos compilaciones y dos intentos del par de argumentos, cero ensayos interactivos. Estimaci?n retrospectiva sin cronometraje: implementaci?n 4 min, preparaci?n documental e integraci?n 3 min, comprobaciones Windows 2 min. Revisi?n humana y ensayo pendientes, sin tiempo medido.
