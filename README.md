# Escritorio Canvas

Diseño de un entorno de trabajo para Windows en un monitor. Cada espacio tiene un lienzo con una zona de Inicio para accesos y archivos. Las aplicaciones conservan sus ventanas nativas.

## Dónde empezar

- [Cómo se trabaja en el repositorio: roles, flujo y reglas](AGENTS.md)
- [Diseño y glosario](docs/diseno-inicial.md)
- [Mapa del proyecto](docs/plan-por-fases.md)
- [Fase 1 · Plan de inspección de Canvas Window Composer](docs/desarrollo/01-inspeccion-cwc.md)
- [Informe de inspección del código y pruebas pendientes en Windows](docs/inspeccion/informe-cwc.md)
- [Fase 2 · Ensayo de edición nativa con dos ventanas](docs/desarrollo/02-ensayo-panel-nativo.md)
- [Resultados con VS Code y un editor de control](docs/inspeccion/ensayo-panel-nativo.md)
- [Decisión sobre archivos y accesos](docs/adr/001-archivos-y-accesos.md)
- [Decisión sobre reapertura selectiva](docs/adr/002-sesion-y-reapertura.md)

## Cómo se trabaja

El usuario encarga un objetivo. Coordinación lanza Planificación, Ejecución y Revisión en contextos separados, y solo consulta al usuario las decisiones que le corresponden. Revisión fusiona cuando todo cumple. [Reglas](AGENTS.md) · [Guías por rol](docs/proceso/)

## Estado

La fase 4 verificó los dos acomodos con dos instancias aisladas de VS Code, incluidas las restauraciones finales y la limpieza. Una instancia necesitó cierre forzado. [Resultado, revisión y límites](docs/inspeccion/f04-cierre.md).

En una sesión de Windows 11, el ensayo de dos ventanas permitió editar en VS Code a dos anchos y recuperar el foco al cancelar un diálogo, con una miniatura vecina visible. La fase 3 verificó dos viajes entre escritorio y canvas con dos ventanas WinForms propias, conservando sus acomodos. [Cierre y límites de la fase 3](docs/inspeccion/f03-cierre.md). La restauración de aplicaciones reales, la recuperación tras reinicio y el rendimiento con 15 ventanas siguen pendientes.
