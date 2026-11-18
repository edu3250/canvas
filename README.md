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

Cada sesión de un modelo declara un rol (Planificación, Ejecución o Revisión) y el archivo en el que trabaja. Una fase empieza con un plan que el usuario aprueba al fusionarlo. Después se divide en tareas pequeñas, cada una en su propia rama y su propio PR, siguiendo GitHub flow. Las reglas están en [AGENTS.md](AGENTS.md) y las plantillas en [docs/plantillas](docs/plantillas/).

## Estado

En una sesión de Windows 11, el ensayo de dos ventanas permitió editar en VS Code a dos anchos y recuperar el foco al cancelar un diálogo, con una miniatura vecina visible. Las pruebas de restauración y de rendimiento con 15 ventanas siguen pendientes.
