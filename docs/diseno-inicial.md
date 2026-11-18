# Escritorios de trabajo para Windows

## Objetivo

El canvas es el entorno de trabajo principal desde que se abre la aplicación. Cada espacio reúne su fondo, sus iconos, sus archivos y las ventanas que el usuario organiza como paneles. Se espera poder pasar de un icono a trabajar en la aplicación abierta dentro de ese mismo espacio. El escritorio de Windows queda disponible para uso ocasional. Las aplicaciones conservan su interfaz nativa.

Dos sesiones servirán de referencia para las pruebas. En Juegos se usan Discord, Steam, un juego, Spotify y varias ventanas de navegador. En Programación se usan varias ventanas de navegador junto con herramientas como VS Code, Obsidian, Claude, Codex, Unity y Blender. La primera prueba se hará en un monitor con 15 ventanas abiertas y seis paneles visibles, incluida una escena de Unity en movimiento.

El desarrollo se plantea con varios modelos de IA y las decisiones importantes a cargo del usuario. Al estimar las fases habrá que distinguir implementación, integración, pruebas en Windows y tiempo de revisión humana.

## Glosario de trabajo

| Término | Significado propuesto |
| --- | --- |
| Espacio | Contexto de trabajo con carpeta, fondo, iconos y lienzo propios. El usuario lo selecciona mediante un atajo configurable. |
| Acceso directo a una aplicación | Icono desde el que se inicia una aplicación instalada. Su posición pertenece al espacio. |
| Archivo o carpeta del espacio | Contenido guardado en ese espacio. Trasladarlo a otro espacio cambia dónde se encuentra el contenido. |
| Lienzo | Superficie donde las ventanas conservan una posición al desplazar la vista. |
| Inicio | Zona fija del lienzo de cada espacio donde se encuentran sus iconos. Un atajo permite visitarla y recuperar la vista anterior. |
| Panel | Representación de una ventana en el lienzo. Al activarlo debe permitir trabajar en el tamaño elegido. Una segunda acción explícita lo amplía. |
| Ventana activa | Ventana de una aplicación que recibe la entrada del usuario. Una aplicación puede tener varias ventanas. |

## Punto de partida técnico

[Canvas Window Composer](https://github.com/oreyg/CanvasWindowComposer) documenta un lienzo por escritorio virtual y una vista general compuesta con miniaturas de Windows. Su licencia es MIT y el repositorio está archivado. Es candidato para un ensayo de reutilización. Falta compilarlo, probar el foco y las ventanas de diálogo, medir su consumo y evaluar cuánto código conviene mantener.

[Deska](https://deska.dev/es/canvas) aporta una referencia para la organización espacial de herramientas. [Spatial Canvas](https://github.com/13auth/spatial-canvas) muestra otra transición entre la vista general y la aplicación real. Su licencia propietaria restringe la reutilización.

## Decisiones de experiencia

Los iconos permanecen en Inicio al recorrer el canvas. El atajo de ida y vuelta guarda y recupera la posición de la vista y su nivel de zoom dentro del espacio. Si hay varias ventanas de una aplicación en el espacio activo, pulsar su icono lleva a la última que se usó allí. El menú del icono permite elegir otra o solicitar una ventana nueva.

Un atajo configurable cambia el espacio activo procurando interferir lo mínimo con Windows. Una ventana puede pertenecer a un espacio y una aplicación puede tener ventanas en varios. Al pulsar una notificación que activa Discord, el usuario espera llegar al espacio de esa ventana. Hay que comprobar la integración con la activación real de Discord.

Se recuerda el acomodo durante la sesión. Tras reiniciar, el usuario escoge qué ventanas se pueden reabrir y recolocar. La reapertura de un juego requiere elección expresa. La política acordada se recoge en [ADR 002](adr/002-sesion-y-reapertura.md).

## Decisiones pendientes

- Validar en el prototipo la colocación del primer panel en una zona libre próxima a Inicio.
- Disponibilidad para revisión humana y pruebas de aceptación.

La relación entre carpetas y accesos está en [ADR 001](adr/001-archivos-y-accesos.md). La propuesta arquitectónica es usar el espacio como identidad compartida del escritorio y su lienzo. La implementación concreta del escritorio sigue abierta.

## Criterios acordados

El usuario puede asignar a sus espacios las ventanas que ya estaban abiertas en Windows antes de iniciar el gestor. El flujo principal es abrir las aplicaciones desde los iconos del propio espacio.

Al estrechar un panel interactivo, la aplicación recibe el nuevo ancho y distribuye su contenido según sus propias reglas. En las capturas de VS Code, pasar de 1475 × 800 a 492 × 800 conserva la altura de las letras y hace que una línea larga ocupe varios renglones. Alejar el mapa es un gesto independiente.

La primera versión usará atajos propios configurables. Si una combinación está ocupada, se propondrá elegir otra. El usuario deja para una versión posterior la evaluación de reasignar temporalmente un atajo del sistema.

El escritorio y el canvas conservan acomodos independientes. Los juegos que se ejecutan a pantalla completa siguen en ese modo. En el canvas basta con un indicador que permita volver al juego. Para la reapertura basta con recuperar la aplicación en cada posición seleccionada. Los documentos, pestañas y herramientas internas pertenecen a cada aplicación.

Reducir el mantenimiento tras actualizaciones de Windows es un criterio para elegir la arquitectura. La primera propuesta y sus condiciones se describen en el [plan por fases](plan-por-fases.md).
