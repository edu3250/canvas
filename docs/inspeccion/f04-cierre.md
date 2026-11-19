# Cierre de fase 4 · Dos acomodos con VS Code

**Resultado: criterios técnicos verificados y cierre documental completado.** La aprobación del SHA final y la integración quedan registradas en el [PR #3](https://github.com/edu3250/escritorio-canvas/pull/3).

## Contrato y resultado

Plan: `957c66f6e192d29f08d409590591815dcf3d8242`. Ensayo de Ejecución: `dfbea836dd6c9ed80e84953f7645ae4dc4541e1e`. Entrega revisada: `e9a6dca9e503d8ca6684357c2490c3f3db010289`. El cierre posterior solo modifica documentación. [Encargo](../desarrollo/04-acomodos-vscode/encargo.md).

**Observado por Ejecución y contrastado por Revisión:** las 12 lecturas y las dos restauraciones finales del registro local `layout-code-a79249bd4d05/sesion.txt` coinciden con la [evidencia publicada](f04-evidencia-recorrido.md). A requirió cierre forzado tras el timeout; ambos procesos terminaron y el temporal se eliminó. El contrato permite terminar procesos propios tras timeout. No se deduce un cierre normal de ambas instancias.

| Criterio | Estado | Evidencia |
| --- | --- | --- |
| Identidad propia A/B | Verificado | PID, HWND y documentos distintos en el registro; perfiles y extensiones separados en Program.cs. |
| Entrar y cambiar | Verificado | C1 y C2: estado exacto y 0 px de diferencia por borde. |
| Salir y volver sin deriva | Verificado | D1 y D2 coinciden con D0; C3 con C2. |
| Terminar sin residuos | Verificado | Restauraciones finales y cierre de ambos PID; temporal eliminado. |
| Conservar modos anteriores | Verificado en el alcance contratado | Diff revisado, compilación y argumentos inválidos; sin repetir interacción histórica. |

Entorno del ensayo: Windows 11 25H2 26200.9974, SDK 10.0.401, Windows Desktop 10.0.12, VS Code 1.147.0, PanelNativo 1.0.0.0, 96 DPI y área primaria 1920 × 1032 px. Métodos y valores completos en la evidencia enlazada.

**Confirmado en código:** asociación PID-documento, procesos registrados inmediatamente, identidad comprobada antes de cerrar ventanas, recursos aislados y oráculo intacto. Sin dependencias nuevas ni cambios de ADR. [Dictamen técnico](https://github.com/edu3250/escritorio-canvas/pull/3#issuecomment-7000639245).

**Observado en Revisión:** compilación con 0 errores y 0 advertencias; `layout-code --auto` y `layout --auto` devolvieron 2, con 0 temporales antes y después. `git diff --check` pasó. No hay checks remotos reportados; CI y cobertura no son exigibles al prototipo. Se corrigieron los caracteres perdidos y se identificó el párrafo histórico de esfuerzo previo al permiso.

**Inferido:** el resultado respalda conservar dos acomodos en estas transiciones programáticas y este entorno.

**Pendiente y fuera del alcance:** navegación por UI, foco, reinicio, recuperación tras caída, otras escalas y aplicaciones. Revisión no lanzó ensayos interactivos nuevos. La aprobación del usuario no equivale a observación humana del ensayo.

## Esfuerzo

Estimaciones retrospectivas, sin cronometraje. Planificación registró Codex/gpt-6-astra; Ejecución, Codex/GPT-6 con variante no verificada; Revisión, Codex con variante no verificada. Fuentes: descripción del PR y dictamen enlazado.

| Actividad | Minutos estimados | Intentos |
| --- | ---: | --- |
| Planificación | 5 | Una pasada. |
| Implementación | 4 | Una pasada. |
| Documentación e integración de Ejecución | 5 | Preparación y actualización tras ensayo. |
| Comprobaciones Windows de Ejecución | 3 | Tres compilaciones, dos intentos del par de argumentos y un ensayo interactivo. |
| Revisión técnica | 4 | Una compilación y dos comprobaciones de argumentos. |
| Cierre documental e integración de Revisión | 3 | Una pasada documental. |
| Total registrado | 24 | Revisión humana sin medición disponible. |
