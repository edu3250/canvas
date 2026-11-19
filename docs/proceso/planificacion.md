# Planificación

Convierte un objetivo autorizado en un encargo y en tareas que Ejecución pueda cumplir sin tomar decisiones de diseño. Lee también la [guía de estilo](estilo.md).

## Entrega

- `docs/desarrollo/NN-slug/encargo.md`, con la [plantilla de encargo](../plantillas/encargo-fase.md).
- Una tarea por archivo en `tareas/`, con la [plantilla de tarea](../plantillas/tarea.md).
- Para producto: los tests en xUnit y las firmas vacías que forman el contrato.
- Todo en el primer commit de la rama `fNN-slug`, creada desde `main` actualizado.
- El PR hacia `main` con la [plantilla](../../.github/pull_request_template.md), con el SHA del plan y la tabla de tareas. En borrador si quedan consultas para el usuario.

## Reglas

- Cada tarea tiene una sola responsabilidad: entre 1 y 4 archivos y unas 300 líneas de diff como máximo.
- Cada tarea lista los archivos permitidos, el contrato, los comandos de verificación, los criterios de aceptación y las condiciones de parada.
- Para lo que toca Win32, indica las llamadas, su orden, las banderas y qué hacer con cada valor de retorno.
- Para un ensayo, define los sujetos de prueba y cómo se comprueba su identidad, las mediciones con su API, unidades, referencia y tolerancia, y al menos un caso que debe fallar.
- Lo que no puedas decidir, márcalo como **consulta al usuario** en el encargo y deja el PR en borrador.
- Ante un informe de bloqueo, corrige la tarea, divídela o eleva la consulta. Cada cambio de contrato va en un commit propio, con el motivo. No implementas la solución.
