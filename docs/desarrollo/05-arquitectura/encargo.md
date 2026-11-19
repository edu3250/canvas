# Fase 5 · Arquitectura del producto

**Estado:** encargo autorizado por el usuario: «Definir la arquitectura del producto». Entrega documental; las decisiones estructurales se presentan para aprobación, sin instalar dependencias ni crear producto.

## Objetivo y entrada

Proponer una arquitectura trazable al [diseño](../../diseno-inicial.md), [ADR 001](../../adr/001-archivos-y-accesos.md) y [ADR 002](../../adr/002-sesion-y-reapertura.md), y delimitar un primer incremento implementable. Base: `7f87cf07da4ec3297a2245c7bf4e7c55f80e63ff`. Leer la [inspección CWC](../../inspeccion/informe-cwc.md), [fase 2](../../inspeccion/ensayo-panel-nativo.md), [fase 3](../../inspeccion/f03-cierre.md), [fase 4](../../inspeccion/f04-cierre.md) y [mapa](../../plan-por-fases.md).

## Alcance y tareas

| Tarea | Entrega | Dependencia | Interactiva |
| --- | --- | --- | --- |
| 1 | [Propuesta y primer incremento](tareas/01-propuesta.md) | Este plan publicado | No |
| 2 | Revisión documental independiente y síntesis | t01 | No |

No crear src/, tests/, proyectos ni paquetes; no copiar código del prototipo o de terceros. No modificar decisiones acordadas. Una propuesta de ADR nuevo debe decir explícitamente «propuesto» hasta decisión del usuario. Publicar el plan autoriza esta entrega documental, no el incremento de producto que se describa.

## Criterios verificables

| Criterio | Evidencia |
| --- | --- |
| Responsabilidades y dependencias explícitas | Diagrama y tabla de módulos; dominio independiente de Windows. |
| Modelo compatible con experiencia y ADR | Espacios, Inicio, paneles, accesos, cámara, retorno, última ventana, identidad temporal y persistente. |
| Transiciones y fallos definidos | Dos acomodos y coordenadas; foco/modal; cambio de espacio; salida normal y caída; separación de lo ensayado y propuesto. |
| Reutilización y plataforma comparadas | Alternativas CWC completo, selectivo y propio; alternativas de interfaz y almacenamiento, costes cualitativos y dependencias propuestas. |
| Primer incremento ejecutable después de aprobación | Archivos y contratos previstos, no objetivos genéricos; criterios, xUnit, cobertura mínima 60 % de áreas de lógica modificadas y CI windows-latest build/test. |
| Decisiones pendientes visibles | Lista corta de decisiones con recomendación, alternativa y consecuencia; ninguna presentada como acordada. |

## Revisión y cierre

Revisión independiente comprueba trazabilidad y coherencia, registra SHA del plan y entrega en el PR y crea `docs/inspeccion/f05-revision.md`. Puede corregir solo enlaces o errores editoriales; cambios de diseño vuelven a Planificación. Actualiza README y mapa para enlazar la propuesta sin declarar arquitectura adoptada. La adopción y el inicio de producto requieren la decisión del usuario; mantener el PR en borrador mientras queden decisiones estructurales por resolver. No fusionar propuestas como decisiones acordadas.

Registrar esfuerzo e intentos por rol, método de estimación y limitaciones. No ejecutar aplicaciones ni ensayos interactivos.
