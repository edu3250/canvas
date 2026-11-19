# Fase 4 · Dos acomodos con VS Code

**Estado:** ejecución y revisión técnica completadas. [Cierre de fase](../../inspeccion/f04-cierre.md). El PR #3 registra la aprobación del SHA final y su integración.

## Decisión y entrada

Comprobar si el recorrido verificado con ventanas de control conserva también los dos acomodos de dos ventanas reales de VS Code. Base: `3fab799e8bfc9669ef492919994c3b775023a6bf`, fusión del PR #2. Leer el [cierre de fase 3](../../inspeccion/f03-cierre.md), su [evidencia](../../inspeccion/f03-evidencia-recorrido.md), el [ADR 002](../../adr/002-sesion-y-reapertura.md) y el [prototipo](../../../prototipos/panel-nativo/README.md).

## Alcance y diseño

Prototipo desechable existente, sin producto, dependencias nuevas ni código ajeno. Añadir el argumento exacto `layout-code`. Crear dos instancias de VS Code, cada una con documento y perfil temporales propios. A parte normal y B maximizada. Reutilizar `LayoutTrial.Execute(Run run)` y `WindowPlacementApi` para el recorrido de seis etapas y dos restauraciones finales. Mantener `layout`, `control` y `code`.

La prueba mide geometría y estado mediante transiciones programáticas. No demuestra navegación por controles de interfaz, edición, foco, reinicio, caída abrupta ni otras aplicaciones o escalas. No reutilizar ninguna ventana del usuario.

## Tareas

| Orden | Entrega | Depende de | Interactiva |
| --- | --- | --- | --- |
| 1 | [Dos instancias aisladas y entrada](tareas/01-instancias.md) | Plan publicado | No |
| 2 | [Ensayo y evidencia](tareas/02-evidencia.md) | t01 verificada | Sí, previo «adelante» en la sesión de Ejecución |

## Criterios de salida

| Afirmación | Evidencia necesaria |
| --- | --- |
| A y B pertenecen a esta ejecución | PID, HWND y documento únicos por sujeto; perfiles distintos; versión de VS Code. |
| Entrar y cambiar acomodo | C1 y C2 verificadas para ambas ventanas. |
| Salir y volver sin deriva | D1 y D2 coinciden con D0; C3 coincide con C2. Estado exacto y tolerancia de 1 px por borde. |
| Terminar sin residuos | Restauraciones finales verificadas, salida de procesos propios y eliminación del temporal. |
| Modos anteriores conservados | Compilación, revisión del diff y comprobaciones de argumentos sin abrir ventanas. |

## Riesgos y cierre

VS Code puede imponer dimensiones mínimas o tardar en estabilizarse. No ajustar silenciosamente medidas, tolerancia ni tiempos para obtener éxito: documentar el caso y devolverlo a Planificación. Si no se obtiene identidad inequívoca, abortar sin buscar sustitutos ajenos.

Revisión independiente compara el SHA final con el SHA del plan registrado en el PR, completa `docs/inspeccion/f04-cierre.md`, el mapa y el README, y aprueba y fusiona solo cuando pasan los criterios. El mismo PR contiene plan e implementación en commits separados. La evidencia interactiva pendiente impide declarar cerrada la fase. Registrar herramienta, modelo, intentos y esfuerzo por categoría, distinguiendo estimación de medición.
