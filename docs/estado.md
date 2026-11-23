# Estado del proyecto

Lo lee toda sesión al empezar y lo actualiza al terminar si algo cambió. Como mucho una pantalla: el detalle va en el archivo del hito.

**Actualizado:** 22-11-2026.

## Ahora

- **Cerrado:** [hito 09 · Ventanas reales en el lienzo](hitos/09-ventanas-reales.md). Continuidad visual y regreso desde maximizada validados en el [PR #46](https://github.com/edu3250/canvas/pull/46).
- **En curso:** [hito 10 · Mesa de luz](hitos/10-mesa-de-luz.md). Los pasos 1 y 2 están fusionados. El asa, la lengüeta visual y la iluminación de los paneles están en el [PR #49](https://github.com/edu3250/canvas/pull/49).
- **Siguiente paso:** validar en uso el paso 3 y autorizar su fusión con la CI en verde. El recorrido del puntero hasta la lengüeta y la luz del minimapa y la selección ya se corrigieron en el PR. Después, construir las acciones de la lengüeta en el paso 4.

## Qué funciona hoy

| Pieza | Dónde | Estado |
| --- | --- | --- |
| Dominio: cámara, navegación, espacios, cuadrícula de Inicio | `src/Canvas.Domain`, con pruebas en `tests/` | Producto, con CI |
| Consola de navegación entre Programación, Inicio y Juegos | `src/Canvas.Console` | Producto ([fase 6](inspeccion/f06-cierre.md)) |
| Ventana WPF: recorrer el lienzo con paneo y zoom, iconos de las carpetas reales de cada espacio (seleccionar y mover en grupo), retorno a la vista anterior | `src/Canvas.Desktop` | Producto ([PR #35](https://github.com/edu3250/escritorio-canvas/pull/35)) |
| Activar una ventana real en el rectángulo de su panel con el vecino visible | `prototipos/panel-nativo` | Prototipo verificado con VS Code ([ensayo](inspeccion/ensayo-panel-nativo.md)) |
| Salir al escritorio y volver al lienzo conservando ambos acomodos | prototipos | Verificado con ventanas propias y con VS Code ([fase 3](inspeccion/f03-cierre.md), [fase 4](inspeccion/f04-cierre.md)) |

El lienzo abre maximizado e incluye control de zoom y minimapa de Inicio ([PR #40](https://github.com/edu3250/escritorio-canvas/pull/40)). El selector muestra ventanas abiertas como miniaturas DWM en paneles arrastrables ([PR #42](https://github.com/edu3250/escritorio-canvas/pull/42)). Se puede trabajar en la ventana real dentro de su panel y volver al mapa ([PR #43](https://github.com/edu3250/canvas/pull/43)).

## Riesgos abiertos

- Recuperar las ventanas si el gestor se cierra a mitad de una transición.
- Reglas de foco de Windows y notificaciones que activan otra ventana (Discord).
- Juegos a pantalla completa: representarlos con un indicador, sin mover su ventana.
- Reabrir ventanas tras reiniciar: no hay API para restaurar el estado de una aplicación.
- Rendimiento con 15 ventanas y seis paneles visibles: sin medir.

## Decisiones pendientes del usuario

- **Canvas Window Composer** (archivado, MIT). La recomendación es usarlo como referencia y extraer piezas sueltas con atribución: matemática de la cámara, selección de miniaturas visibles y filtros de ventanas gestionables. No conviene hacer un fork. Incorporar código suyo requiere la autorización del usuario. [Informe](inspeccion/informe-cwc.md)

## Hitos terminados

| Hito | Resultado | Cierre |
| --- | --- | --- |
| Fases 1–6 | Inspección de CWC, ensayos nativos, arquitectura y consola de navegación | [Mapa de fases](plan-por-fases.md) |
| Fase 7 | Primer lienzo gráfico en WPF con iconos de muestra que se seleccionan y mueven en grupo | [Plan](desarrollo/07-lienzo/plan.md) · [PR #32](https://github.com/edu3250/escritorio-canvas/pull/32) |
| Hito 08 | Iconos reales en Inicio con posiciones que se conservan al reiniciar | [Cierre del hito](hitos/08-iconos-reales.md) · [PR #37](https://github.com/edu3250/escritorio-canvas/pull/37) |
