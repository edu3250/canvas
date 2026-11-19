# Revisión documental de fase 5

**Resultado: solicitar precisiones a Planificación antes del dictamen favorable completo.** La separación de módulos, el alcance documental y la compatibilidad con los ADR resultan favorables. Las transiciones tienen ambigüedades materiales pendientes. Esto no adopta arquitectura ni autoriza producto. Mantener el PR en borrador, sin fusionar.

## Revisiones y evidencia

Plan: `6597a51bb1ee6ad157d88345dd8006d7c6352160`. Entrega examinada: `bb687ea90d988b038937e0c86fb9da39424cb00c`. Base: `7f87cf07da4ec3297a2245c7bf4e7c55f80e63ff`. El SHA del cierre editorial y su dictamen quedan registrados en el [PR #4](https://github.com/edu3250/escritorio-canvas/pull/4). Contrato: [encargo](../desarrollo/05-arquitectura/encargo.md) y [tarea](../desarrollo/05-arquitectura/tareas/01-propuesta.md).

**Observado en esta revisión:** lectura del plan fijado, propuesta, incremento, diseño, ADR y antecedentes; solo dos documentos añadidos por Ejecución respecto al plan. No se ejecutó producto, instaló dependencia ni repitió ensayo. Windows, DPI, SDK y aplicaciones no medidos: no aplican a esta comprobación documental.

**Confirmado por documentación histórica, no por nueva ejecución ni inspección del prototipo:** [fase 2](ensayo-panel-nativo.md), [fase 3](f03-cierre.md) y [fase 4](f04-cierre.md). Fase 4 informa dos instancias aisladas de VS Code 1.147.0, Windows 11 25H2 26200.9974, 96 DPI, SDK 10.0.401 y Desktop 10.0.12; una instancia requirió terminación forzada. No demuestra recuperación tras caída, otras escalas ni ventanas preexistentes. La [inspección CWC](informe-cwc.md) se atribuye a su revisión histórica c89634a43fe659482f9e6c7c97f5ed89b94fd701; no se inspeccionó código externo nuevo.

## Criterios del encargo

| Criterio | Estado | Motivo y evidencia |
| --- | --- | --- |
| Responsabilidades y dependencias | Verificado documentalmente | Propuesta, sección Límites y propiedad del estado: dominio puro y puertos de coordinación. |
| Modelo compatible con experiencia y ADR | Verificado documentalmente | Modelo e identidad; navegación separada del traslado y reapertura separada de posición, conforme [ADR 001](../adr/001-archivos-y-accesos.md) y [ADR 002](../adr/002-sesion-y-reapertura.md). Sin contradicción directa detectada. |
| Transiciones y fallos definidos | Fallido documentalmente | H1–H3 dejan resultados observables sin determinar; no se resuelven como edición de estilo. |
| Reutilización y plataforma comparadas | Verificado documentalmente | Alternativas y dependencias propuestas compara CWC completo, selectivo, propio, WPF/WinForms y JSON/motor local, con costes cualitativos. |
| Primer incremento ejecutable tras aprobación | Verificado como especificación propuesta | [Incremento](../desarrollo/05-arquitectura/primer-incremento.md): archivos, firmas, errores, escenarios, consola, xUnit, CI y umbral de líneas del 60 %. Versiones y ejecución pendientes del siguiente encargo autorizado. |
| Decisiones pendientes visibles | Verificado documentalmente | Tabla final de propuesta, sin adopción de paquetes, UI, almacenamiento ni incremento. |

## Hallazgos para Planificación

Todos son de prioridad media y requieren aclaración de diseño; no se altera la propuesta desde Revisión. Evidencia fijada: [propuesta revisada](https://github.com/edu3250/escritorio-canvas/blob/bb687ea90d988b038937e0c86fb9da39424cb00c/docs/arquitectura/propuesta-producto.md).

- **H1 · Modal, activación externa y cambio pendiente (líneas 63 y 69–71).** Se aplaza el cambio solicitado durante un modal y se sigue la activación externa, pero no se especifica qué ocurre con la petición aplazada cuando una notificación lleva a otro espacio. Podría ejecutarse luego una petición ya obsoleta. Planificación debe proponer cancelar esa petición al cambiar externamente, o conservarla con reconfirmación. Añadir escenario: modal en A, solicitud B, activación externa C, cierre del modal; fijar destino y ausencia de salto tardío.
- **H2 · Salida de Expanded y vista guardada (líneas 54 y 60–63).** El texto guarda tamaño y vista anteriores, pero la tabla solo restaura tamaño y no define salida desde Expanded ni su relación con ReturnView de Inicio. Proponer una vista de ampliación separada y restauración explícita, o conservación de cámara actual con descarte explícito. Añadir ampliar → Inicio → retorno → desampliar y ampliar → cambiar espacio → volver, con cámara, tamaño y retornos esperados. La elección corresponde al usuario cuando cambie la interacción.
- **H3 · Unidades y autoridad de NativeSize (líneas 44 y 52–54).** Se definen mundo lógico y pantalla física, pero NativeSize no declara unidad, marco exterior o área cliente, ni cómo se reconcilia con CanvasBounds después de un redimensionamiento observado. Proponer tamaño lógico convertido por DPI o tamaño físico etiquetado con contexto; declarar qué dato gobierna la miniatura. Añadir ejemplo numérico a 144 DPI y zoom distinto de 1, sin afirmar ensayo realizado. El resultado a 96 DPI no resuelve esta elección.

**Inferido:** estos vacíos permiten implementaciones distintas de la misma tabla. No prueban un fallo de producto inexistente. No bloquean estudiar el pequeño corte puro de navegación, pero impiden declarar íntegramente definido el contrato de transiciones de arquitectura.

## Fuentes técnicas y controles

Se contrastaron las afirmaciones con Microsoft Learn: [restricciones de foco](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow), [representación DWM](https://learn.microsoft.com/en-us/windows/win32/dwm/thumbnail-ovw), [capacidades WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/) y [cobertura con Coverlet](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage). Estas fuentes respaldan las afirmaciones acotadas de la propuesta, no su viabilidad integrada ni versiones futuras de paquetes.

Cierre editorial: README reconoce el ensayo limitado de restauración de fase 4; README y mapa enlazan propuesta, incremento y esta revisión. Comprobar enlaces locales y `git diff --check` antes de publicar; resultados definitivos y SHA en comentario del PR. Sin cambios en ADR, plan, producto, dependencias ni artifact de T-5.

## Decisiones pendientes y esfuerzo

Después de precisar H1–H3, presentar al usuario las recomendaciones ya documentadas: dominio propio frente a extracción CWC; futura WPF frente a WinForms; JSON versionado frente a motor local; raíz de contenido por separado; interacción de edición/modal; autorización separada del incremento y paquetes con versiones verificadas. Ninguna está adoptada. La revisión documental favorable futura tampoco sustituye esas decisiones.

Estimaciones retrospectivas sin cronometraje, en minutos. Fuente de los roles anteriores: descripción del PR #4 al revisar bb687ea. Planificación: Codex/gpt-6-astra, 4 min, una pasada. Ejecución documental: Codex/GPT-6, variante no verificada, 8 min de redacción y 2 min de validación/publicación, una pasada cada una. Revisión independiente: Codex/GPT-6, variante no verificada, 6 min de contraste y 2 min de cierre/publicación, una pasada y comprobación final. Total registrado: 22 min. Implementación de producto y pruebas Windows nuevas: 0 min. Revisión humana y fusión: no realizadas, sin tiempo registrado.
