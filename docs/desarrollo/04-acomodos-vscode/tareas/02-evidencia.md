# f04-t02 · Medir los dos acomodos de VS Code

**Rama:** f04-plan. **Depende de:** t01 verificada. **Ensayo interactivo:** sí, con aviso y «adelante» en la sesión de Ejecución, conforme a AGENTS.md §7.

## Archivos y entrada

Modificar `prototipos/panel-nativo/README.md` y crear `docs/inspeccion/f04-evidencia-recorrido.md`. Leer el [encargo](../encargo.md) y la [tabla de mediciones de fase 3](../../03-salida-y-retorno/tareas/02-recorrido.md). No cambiar código durante esta tarea sin devolver el caso a Planificación.

Compilar desde la raíz con `dotnet build prototipos/panel-nativo/PanelNativo.csproj`. Tras el permiso, desde `prototipos/panel-nativo` ejecutar `.\bin\Debug\net10.0-windows\PanelNativo.exe layout-code`. Registrar Windows, SDK, runtime, versión de VS Code, DPI y área de trabajo en píxeles. Usar solo las dos instancias temporales creadas por Run.

## Oráculo

Las dimensiones y posiciones se conservan de fase 3, relativas al origen del área de trabajo primaria (mínimo 1200 × 800 px). Para cada sujeto leer estado y rectángulo normal con GetWindowPlacement, límites visibles con GetWindowRect. No mezclar sistemas de coordenadas.

| Etapa | Medición y referencia |
| --- | --- |
| D0 | A normal en (20,40,540,400); B normal en (580,70,540,440) antes de maximizar. Capturar originales inmutables tras maximizar B; cotejar su rectángulo normal con el previo. |
| C1 | Ambas normales; A (30,80,480,500), B (550,120,500,450). |
| C2 | A pasa a (65,110,510,460), B permanece igual. Guardar ambos rectángulos efectivos. |
| D1 | Estado, rectángulo normal y visible de cada ventana coinciden con D0. |
| C3 | Ambas normales; límites visibles coinciden con C2. |
| D2 | Repetir comparación completa con D0. |

Exigir dos lecturas consecutivas estables, sondeo cada 100 ms, máximo 3 s, estado exacto y tolerancia de 1 px por borde. Si B tiene los límites visibles originales pero estado normal en D1, falla. Un cambio de DPI, monitor o área invalida el ensayo. Registrar restauraciones finales y limpieza además de las 12 lecturas.

## Evidencia y aceptación

Contrastar `evidencia-local/layout-code-<id>/sesion.txt` con el oráculo. Publicar tabla de 12 filas con esperado, obtenido, diferencias y estado; los registros crudos permanecen fuera de Git. Registrar código de salida y limpieza. Ante interrupción, señalar primera lectura fallida y posteriores pendientes. No inventar filas ni confundir compilación con ejecución.

Sin permiso interactivo, completar documentación y comprobaciones no interactivas, marcar el ensayo pendiente y entregar el comando preparado. Registrar esfuerzo e intentos en el PR. Las dimensiones rechazadas por VS Code son un resultado fallido, no autorización para alterar el contrato.
