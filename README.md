# Escritorio Canvas

Diseño de un entorno de trabajo para Windows en un monitor. Cada espacio tiene un lienzo con una zona de Inicio para accesos y archivos. Las aplicaciones conservan sus ventanas nativas.

## Dónde empezar

- [Diseño y glosario](docs/diseno-inicial.md)
- [Mapa del proyecto](docs/plan-por-fases.md)
- [Fase 1 · Plan de inspección de Canvas Window Composer](docs/desarrollo/01-inspeccion-cwc.md)
- [Informe de inspección del código y pruebas pendientes en Windows](docs/inspeccion/informe-cwc.md)
- [Fase 2 · Ensayo de edición nativa con dos ventanas](docs/desarrollo/02-ensayo-panel-nativo.md)
- [Decisión sobre archivos y accesos](docs/adr/001-archivos-y-accesos.md)
- [Decisión sobre reapertura selectiva](docs/adr/002-sesion-y-reapertura.md)

La inspección identificó la cámara y el filtrado de miniaturas como candidatos para reutilización. El ensayo actual comprueba qué ocurre cuando el usuario pasa del mapa a editar con otra ventana visible. Cada fase posterior se planifica con los resultados de la anterior.
