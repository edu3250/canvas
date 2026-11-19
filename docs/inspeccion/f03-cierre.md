# Cierre de fase 3 · Salida y retorno

**Resultado:** criterios técnicos verificados; cierre documental completado para aprobación. La fusión queda pendiente por instrucción del usuario. El dictamen del SHA final se publica en el [PR #2](https://github.com/edu3250/escritorio-canvas/pull/2).

## Contrato y evidencia

Plan autorizado: `23e97d6933dde3dba4550c4d4f7b95bcea68fb63`. Precisiones del oráculo: `b96bdbf7325efddbbf7f2067522ec8c1da06825b`. Implementación: `cdc0390704ef85b4f2c92f959d5f0576105fb598`. Evidencia revisada: `a74de904e870c88925e1cc5c3aa05d37f02e4dd9`. [Encargo](../desarrollo/03-salida-y-retorno/encargo.md).

**Observado por Ejecución:** las 12 mediciones y las dos restauraciones finales pasaron; ambos procesos terminaron y el temporal se eliminó. Revisión contrastó el registro local con la [tabla publicada, entorno y versiones](f03-evidencia-recorrido.md). El entorno fue Windows 11 25H2, compilación 26200.9974, 96 DPI, área primaria 1920 × 1032 px, SDK 10.0.401 y PanelNativo 1.0.0.0.

| Criterio | Estado | Evidencia |
| --- | --- | --- |
| Entrar y cambiar el acomodo | Verificado | C1 y C2: ambas ventanas normales, diferencias de 0 px. |
| Salir al escritorio | Verificado | D1 coincide con estado y rectángulos de D0. |
| Volver y repetir | Verificado | C3 coincide con C2; D2 coincide con D0. |
| Terminar | Verificado | Restauraciones finales, procesos terminados y temporal eliminado. |

**Confirmado en código:** captura por valor de WINDOWPLACEMENT, separación de coordenadas, comprobación de identidad antes de medir o mover, estabilización y restauración individual en finally. No se añadieron dependencias ni se modificaron ADR. [Revisión técnica y comprobaciones](https://github.com/edu3250/escritorio-canvas/pull/2#issuecomment-6999779139).

**Observado en Revisión:** una compilación correcta, sin advertencias ni errores; dos invocaciones con argumentos inválidos devolvieron 2; comprobación de espacios de Git sin incidencias. GitHub no reportó checks. El prototipo no exige CI ni cobertura. El cierre posterior solo cambia documentación.

**Inferido:** el ensayo respalda conservar dos acomodos durante esta sesión controlada. No demuestra el comportamiento de aplicaciones arbitrarias.

**Pendiente:** pruebas con aplicaciones reales, otras escalas, reinicio, caída abrupta, multimonitor y controles de navegación. Revisión no repitió el ensayo interactivo porque no recibió un «adelante» para ello. La aprobación del usuario no se interpreta como observación humana de las ventanas.

## Esfuerzo

Estimaciones retrospectivas en minutos, no cronometraje. Fuente de Ejecución: descripción del PR; fuente de Revisión: comentario enlazado y cierre documental de esta sesión.

| Actividad | Minutos estimados | Herramienta e intentos |
| --- | ---: | --- |
| Implementación | 5 | Codex, GPT-6; una sesión de Ejecución. |
| Preparación e integración del PR | 3 | Misma sesión. |
| Comprobaciones de Ejecución en Windows | 2 | Dos compilaciones, dos comprobaciones de argumentos y un ensayo interactivo. |
| Revisión técnica independiente | 5 | Codex; una pasada, una compilación y dos comprobaciones de argumentos. |
| Cierre documental | 3 | Codex; una pasada documental, sin cambiar implementación. |
| Total registrado | 18 | No incluye planificación ni revisión humana, sin medición disponible. |

La etiqueta exacta del modelo revisor no está disponible de forma verificable en esta sesión; la atribución GPT-6.2 del comentario inicial no debe tomarse como dato confirmado. La revisión humana y la fusión no tienen tiempo registrado.
