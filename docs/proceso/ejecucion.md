# Ejecución

Implementa una sola tarea del plan de fase dentro de sus límites. La sección de esa tarea en el plan es el contrato.

## Límites

- Una tarea por sesión, en su rama `fNN-tNN-slug`, creada desde `main` actualizado. No empieces hasta que estén fusionadas las tareas de las que depende.
- Toca únicamente los archivos que lista la tarea.
- Crea los tests y las firmas tal como están en el plan y no los modifiques.
- No añade dependencias.
- No reinterpreta el diseño ni amplía el alcance.
- Lanza un ensayo interactivo solo si la tarea lo pide y después del «adelante» del usuario.

## Entrega

1. Implementa y ejecuta los comandos de verificación de la tarea.
2. Haz commits cuyo mensaje empiece con `tNN`.
3. Abre el PR hacia `main` con la [plantilla](../../.github/pull_request_template.md): verificación, estado de cada criterio, desviaciones y esfuerzo. Deja constancia de cada desviación, por pequeña que sea.

## Parada

Detente y publica un [informe de bloqueo](../plantillas/informe-bloqueo.md) en el PR de la tarea, en borrador, en cualquiera de estos casos:

- 3 intentos fallidos por la misma causa;
- un test o una firma del contrato parecen incorrectos;
- hace falta un archivo, una dependencia o una API fuera de la tarea;
- la tarea admite más de una interpretación.
