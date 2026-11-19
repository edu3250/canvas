# Revisión

Contrasta cada PR de tarea con su sección del plan, lo fusiona cuando todo cumple y cierra la fase. No revisa ni fusiona los PR del plan (`fNN-plan*`), que son responsabilidad de Planificación. Trabaja en una sesión distinta de la que implementó. Lee también la [guía de estilo](estilo.md).

## Revisar

- Compara el diff con la tarea del plan: archivos permitidos, tests y firmas idénticos a los del plan, sin dependencias nuevas.
- Ejecuta build, tests y las comprobaciones no interactivas de la tarea. No repite ensayos interactivos salvo que el plan lo pida.
- Comprueba que cada criterio tenga evidencia y estado.
- Si el problema está en la implementación, pide a Ejecución cambios concretos. Si está en el contrato, devuelve el caso a Planificación.
- Comprueba que las tareas siguientes del plan siguen siendo válidas con lo entregado. Si alguna deja de serlo, avisa a Planificación.
- Registra el dictamen como comentario en el PR (`gh pr comment`), con el SHA revisado y el resultado.

## Cerrar

En el PR de la última tarea de la fase, después del dictamen, añade un commit que solo modifica documentación:

- crea `docs/inspeccion/fNN-cierre.md` con la [plantilla de cierre](../plantillas/cierre-fase.md);
- pon el estado del plan en «cerrado»;
- actualiza el [mapa de fases](../plan-por-fases.md) y el [README](../../README.md).

## Fusionar sin pedir autorización

Revisión fusiona por su cuenta cuando se cumplen todas estas condiciones:

1. El PR no está en borrador y no queda ninguna decisión reservada al usuario sin responder.
2. Todos los criterios están verificados. Un criterio pendiente solo es admisible si el plan lo declara fuera de alcance.
3. Build, tests y CI, cuando exista, pasan en el SHA final.
4. El SHA final es el dictaminado, o el dictaminado más el commit de cierre en la última tarea, y nada más.

Usa `gh pr merge <n> --merge --match-head-commit <sha>`. No uses `gh pr review --approve`: la sesión usa la misma cuenta que abrió el PR y GitHub no permite aprobarlo. El comentario de dictamen es la aprobación. Si GitHub rechazara la fusión, informa al usuario y no desactives protecciones.

Si falla cualquier condición, no fusiones. Explica en el PR qué falta y quién debe resolverlo. Un cambio posterior al dictamen exige revisar de nuevo.
