# Escritorio Canvas

Diseño de un entorno de trabajo para Windows en un monitor. Cada espacio tiene un lienzo con una zona de Inicio para accesos y archivos. Las aplicaciones conservan sus ventanas nativas.

## Dónde empezar

- [Cómo se trabaja en el repositorio: hitos, modos y reglas](AGENTS.md)
- [Estado actual y siguiente paso](docs/estado.md)
- [Diseño y glosario](docs/diseno-inicial.md)
- [Mapa del proyecto](docs/plan-por-fases.md)
- [Fase 1 · Plan de inspección de Canvas Window Composer](docs/desarrollo/01-inspeccion-cwc.md)
- [Informe de inspección del código y pruebas pendientes en Windows](docs/inspeccion/informe-cwc.md)
- [Fase 2 · Ensayo de edición nativa con dos ventanas](docs/desarrollo/02-ensayo-panel-nativo.md)
- [Resultados con VS Code y un editor de control](docs/inspeccion/ensayo-panel-nativo.md)
- [Decisión sobre archivos y accesos](docs/adr/001-archivos-y-accesos.md)
- [Decisión sobre reapertura selectiva](docs/adr/002-sesion-y-reapertura.md)

## Cómo se trabaja

El trabajo avanza por hitos que el usuario puede usar. Cada hito empieza con una entrevista en la que el usuario decide qué se construye. Después se implementa en uno o varios PR. Los cambios que mueven ventanas reales o guardan acomodos pasan además por una revisión en otra sesión. Varios modelos comparten el trabajo a través de [docs/estado.md](docs/estado.md) y del registro de cada hito. [Reglas](AGENTS.md) · [Estado actual](docs/estado.md) · [Diario de uso](docs/diario.md)

## Estado

La fase 6 entrega una consola en español para navegar entre Programación, Inicio y Juegos. La cámara y el retorno se conservan por espacio durante la sesión. [Ejecutar el recorrido](docs/desarrollo/05-arquitectura/recorrido-incremento.md) · [Cierre de la fase 6](docs/inspeccion/f06-cierre.md).

La fase 5 cerró la arquitectura y el contrato de navegación en memoria. El contrato incluye quince casos de prueba para la futura implementación. [Cierre de la fase 5](docs/inspeccion/f05-cierre.md).

La fase 4 verificó los dos acomodos con dos instancias aisladas de VS Code, incluidas las restauraciones finales y la limpieza. Una instancia necesitó cierre forzado. [Resultado, revisión y límites](docs/inspeccion/f04-cierre.md).

En una sesión de Windows 11, el ensayo de dos ventanas permitió editar en VS Code a dos anchos y recuperar el foco al cancelar un diálogo, con una miniatura vecina visible. La fase 3 verificó dos viajes entre escritorio y canvas con dos ventanas WinForms propias, conservando sus acomodos. [Cierre y límites de la fase 3](docs/inspeccion/f03-cierre.md). La restauración de aplicaciones reales, la recuperación tras reinicio y el rendimiento con 15 ventanas siguen pendientes.
