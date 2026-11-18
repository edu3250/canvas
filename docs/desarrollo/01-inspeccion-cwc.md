# Fase 1 · Inspección de Canvas Window Composer

## Decisión que debe permitir

Elegir entre mantener una bifurcación del proyecto, integrar componentes concretos o usarlo como referencia para una implementación propia. El criterio principal es cuánto ayuda a trabajar dentro del canvas sin depender de detalles frágiles de Explorer. El resultado de esta fase es un informe trazable y una recomendación para el usuario.

**Entrada.** [Diseño acordado](../diseno-inicial.md), [decisiones sobre archivos](../adr/001-archivos-y-accesos.md), [sesión y reapertura](../adr/002-sesion-y-reapertura.md) y [revisión de origen](https://github.com/oreyg/CanvasWindowComposer/tree/c89634a43fe659482f9e6c7c97f5ed89b94fd701). Registrar el SHA en toda evidencia. El primer recorrido se puede hacer por lectura remota.

## Trabajo del inspector

1. **Identificar la base.** Verificar licencia, estado del repositorio, proyecto de compilación, dependencias y pruebas disponibles. Registrar qué archivos se leyeron y qué quedó sin inspeccionar.
2. **Seguir una interacción completa.** Partir de los manejadores de clic, arrastre, rueda y doble clic. Seguir cámara, coordenadas de pantalla, miniatura, posición de ventana y foco. Documentar qué recibe el clic en cada modo y en qué punto se oculta el mapa. Contrastar el código con el README cuando difieran.
3. **Trazar lo que cambia en Windows.** Localizar el mecanismo de descubrimiento y clasificación de ventanas, el tratamiento de ventanas maximizadas o a pantalla completa y la gestión de escritorios virtuales. Distinguir interfaces públicas de supuestos sobre clases o jerarquías de Explorer. Identificar qué se limpia al cerrar y qué dependería de un cierre normal.
4. **Reconstruir la propiedad de los datos.** Averiguar cómo se identifica cada ventana, qué se guarda en memoria o en disco y si sobrevive a otro proceso o al reinicio. Buscar equivalentes de espacio propio, carpeta de archivos, icono fijo en Inicio, última ventana usada por aplicación, vista de retorno y autorización de reapertura. Distinguir «ausente en el recorrido revisado» de «incompatible».
5. **Valorar las piezas por separado.** Para cámara, miniaturas, foco, movimiento, persistencia e integración con Windows indicar qué podría conservarse, qué requiere adaptación y qué evidencia falta. Comparar el costo de mantenimiento de las tres alternativas y explicar qué hallazgo cambiaría la recomendación.

## Escenarios para comprobar el proyecto original

Primero registrar el resultado esperado según el código. Si hay una compilación y una sesión de prueba disponible, observar el resultado real en el original sin modificar. Registrar versión de Windows, escala, aplicación y pasos. Las funciones ausentes se anotan como brechas de producto. Una prueba diseñada para el producto futuro no debe presentarse como una prueba superada por el proyecto original.

| Prioridad | Escenario | Qué registrar |
| --- | --- | --- |
| 1 | Mostrar dos ventanas, pulsar VS Code en el mapa, arrastrar, entrar a la ventana y abrir un diálogo. | Destino de cada gesto, visibilidad del mapa, foco, tamaño real y texto al estrechar la ventana. |
| 2 | Alternar ventanas normales y maximizadas, salir y volver a entrar. | Geometría de ambos acomodos y si las ventanas siguen accesibles. Evaluar un cierre inesperado únicamente en una sesión de prueba con trabajo descartable. |
| 3 | Comparar dos ventanas de VS Code, activar Discord desde una notificación y volver a un juego a pantalla completa. | Cómo encuentra la ventana, si la aplicación queda en su espacio, qué ventana recibe el foco y si el juego conserva su modo. |
| 4 | Repetir el mismo recorrido de cámara con 15 ventanas abiertas y seis visibles, una con contenido en movimiento. | Fluidez, tiempo de activación y costo de CPU/GPU/memoria respecto a la misma sesión sin el gestor. Registrar hardware y método de medición. |

## Entrega y cierre

El informe debe incluir una matriz requisito → evidencia → limitación → recomendación. Para cada hallazgo indicar si se observó en ejecución, se confirmó en código, se infirió o quedó pendiente. Enlazar a archivos y líneas de la revisión fijada. Una declaración del README se identifica como documentación del autor.

El responsable de diseño comprueba la evidencia y presenta al usuario la decisión de reutilización, las incertidumbres con impacto y el ensayo mínimo que podría resolverlas. Después de ese cierre se redactará el siguiente encargo a partir de los resultados. El inspector puede concluir que la evidencia justifica descartar una pieza sin construir un sustituto.
