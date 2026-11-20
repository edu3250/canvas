# Estado del proyecto

Lo lee toda sesión al empezar y lo actualiza al terminar si algo cambió. Como mucho una pantalla: el detalle va en el archivo del hito.

**Actualizado:** 19-11-2026.

## Ahora

- **En curso:** fase 7, la última con el proceso anterior. Queda el [PR #32](https://github.com/edu3250/escritorio-canvas/pull/32) (t06: seleccionar y mover varios iconos). Se termina con su [plan vigente](desarrollo/07-lienzo/plan.md). No toca la zona sensible: el usuario lo prueba y Construir lo fusiona.
- **Siguiente paso:** al fusionar el PR #32, abrir una sesión en modo Definir para el hito 08. Propuesta: *ventanas reales en el lienzo*, con dos o tres ventanas del usuario como miniaturas DWM, clic para trabajar en su panel y una tecla para volver al mapa. El usuario decide el objetivo en la entrevista.

## Qué funciona hoy

| Pieza | Dónde | Estado |
| --- | --- | --- |
| Dominio: cámara, navegación, espacios, cuadrícula de Inicio | `src/Canvas.Domain`, con pruebas en `tests/` | Producto, con CI |
| Consola de navegación entre Programación, Inicio y Juegos | `src/Canvas.Console` | Producto ([fase 6](inspeccion/f06-cierre.md)) |
| Ventana WPF: recorrer el lienzo con paneo y zoom, iconos de muestra por espacio, retorno a la vista anterior | `src/Canvas.Desktop` | Producto ([fase 7](desarrollo/07-lienzo/plan.md)) |
| Activar una ventana real en el rectángulo de su panel con el vecino visible | `prototipos/panel-nativo` | Prototipo verificado con VS Code ([ensayo](inspeccion/ensayo-panel-nativo.md)) |
| Salir al escritorio y volver al lienzo conservando ambos acomodos | prototipos | Verificado con ventanas propias y con VS Code ([fase 3](inspeccion/f03-cierre.md), [fase 4](inspeccion/f04-cierre.md)) |

El producto todavía no muestra ni mueve ventanas reales.

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
