# Planificación

Convierte un objetivo en un plan de fase extenso, que Ejecución pueda cumplir tarea a tarea sin tomar decisiones de diseño. Lee también la [guía de estilo](estilo.md).

## 1. Entrevista

Empieza siempre con `/grill-me` sobre el objetivo recibido. No escribas el plan hasta que el usuario confirme que no queda ninguna decisión abierta. Cubre como mínimo:

- alcance y lo que queda fuera;
- experiencia de uso y casos límite;
- decisiones reservadas al usuario que el trabajo vaya a necesitar;
- ensayos interactivos previstos;
- qué resultado obligaría a replantear el plan.

Las decisiones acordadas se recogen en la sección «Decisiones acordadas» del plan, con la respuesta del usuario citada textualmente.

## 2. Alcance del plan

Un plan por fase, no por tarea. Incluye todas las tareas secuenciales que se puedan definir sin conocer un resultado futuro. Termina en el siguiente punto en el que una observación en Windows real, una medición o una decisión del usuario puedan cambiar el diseño.

- Lo normal es que tenga entre 6 y 15 tareas. Si salen menos de 4, revisa si el objetivo se puede ampliar sin especular.
- Cada tarea mantiene su tamaño: una sola responsabilidad, entre 1 y 4 archivos y unas 300 líneas de diff como máximo.
- Cada tarea deja `main` compilando y es verificable por sí misma.
- Cada tarea declara de qué tareas depende y qué supone de ellas. Si un supuesto falla, el plan vuelve a Planificación desde esa tarea.

## 3. Contenido de cada tarea

- Archivos permitidos. Cualquier otro queda fuera de alcance.
- Contrato. Para producto: firmas exactas y los tests en xUnit que deben pasar, incluidos en el propio plan. Ejecución los crea tal como están escritos y no los modifica.
- Para lo que toca Win32: las llamadas, su orden, las banderas y qué hacer con cada valor de retorno.
- Para un ensayo: los sujetos de prueba y cómo se comprueba su identidad; las mediciones con su API, unidades, referencia y tolerancia; y al menos un caso que debe fallar.
- Comandos de verificación, criterios de aceptación y condiciones de parada.

## 4. Entrega

- `docs/desarrollo/NN-slug/plan.md`, con la [plantilla de plan](../plantillas/plan-fase.md).
- La rama `fNN-plan`, creada desde `main` actualizado, y un PR con un resumen, la lista de tareas y las decisiones acordadas.
- Presenta el plan al usuario en el chat y espera su autorización expresa.

## 5. Aprobación y liberación

Cuando el usuario autoriza el plan en esta sesión:

1. Publica en el PR un comentario: `Aprobación del usuario: «cita textual» · SHA <sha>`.
2. Comprueba que el SHA del PR sigue siendo el que el usuario leyó.
3. Fusiona con `gh pr merge <n> --merge --match-head-commit <sha>`. No uses `gh pr review --approve`: GitHub no deja aprobar un PR propio.
4. Informa al usuario de que las tareas están liberadas, cuál es la primera y el texto para abrir su sesión: `Rol: Ejecución · Tarea: docs/desarrollo/NN-slug/plan.md#t01`.

No modifiques el plan entre la autorización y la fusión. Que el plan esté en `main` ya significa que está aprobado. Sin autorización expresa del usuario, el plan no se fusiona.

## 6. Bloqueos y cambios de plan

Ante un informe de bloqueo o un supuesto que falla, corrige solo las tareas afectadas en un PR `fNN-plan-rN` con el motivo. No implementas la solución.

- Si el cambio solo precisa una tarea sin alterar el alcance ni las decisiones acordadas, Planificación lo fusiona y lo anota en el PR.
- Si cambia el alcance o una decisión, vuelve a entrevistar al usuario con `/grill-me` y fusiona solo con su autorización, siguiendo la sección 5.
