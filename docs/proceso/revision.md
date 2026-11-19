# Revisión

Contrasta el trabajo con el contrato publicado, cierra la fase y fusiona. Trabaja en un contexto distinto del de quien planificó o implementó. Lee también la [guía de estilo](estilo.md).

## Revisar

- Compara el diff de cada tarea con su archivo: archivos permitidos, tests y firmas intactos, sin dependencias nuevas.
- Ejecuta build, tests y las comprobaciones no interactivas de cada tarea. No repite ensayos interactivos salvo que el encargo lo pida.
- Comprueba que cada criterio tenga evidencia y estado.
- Si el problema está en la implementación, pide a Ejecución cambios concretos. Si está en el contrato, devuelve el caso a Planificación.
- Registra en el PR un dictamen con el SHA revisado.

## Cerrar

En un último commit, que solo modifica documentación:

- crea `docs/inspeccion/fNN-cierre.md` con la [plantilla de cierre](../plantillas/cierre-fase.md);
- pon el estado del encargo en «cerrado»;
- actualiza el [mapa de fases](../plan-por-fases.md) y el [README](../../README.md).

## Fusionar sin pedir autorización

Revisión fusiona por su cuenta cuando se cumplen todas estas condiciones:

1. El PR no está en borrador y no queda ninguna decisión reservada al usuario sin responder.
2. Todos los criterios están verificados. Un criterio pendiente solo es admisible si el encargo lo declara fuera de alcance.
3. Build, tests y CI, cuando exista, pasan en el SHA final.
4. El SHA final es el dictaminado más el commit de cierre, y nada más.

Usa `gh pr merge <n> --merge --match-head-commit <sha>`. Si GitHub exige una aprobación de otra cuenta, deja el dictamen en un comentario identificado e informa a Coordinación. No desactives protecciones.

Si falla cualquier condición, no fusiones. Explica en el PR qué falta y quién debe resolverlo. Un cambio posterior al dictamen exige revisar de nuevo.
