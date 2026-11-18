# Propuesta de arquitectura y esfuerzo

Borrador para una primera versión de uso personal en Windows 11, con un monitor. La estimación contempla redimensionamiento nativo para editar y zoom para recorrer el mapa. Se respetan los tamaños que admite cada aplicación.

## Estructura

Una aplicación de escritorio en C# con un adaptador Win32 y almacenamiento local. La sesión empieza en el canvas. Cada espacio tiene una identidad, una carpeta real, un fondo y posiciones de iconos y paneles. Lanzar una aplicación desde un icono vincula la ventana resultante al espacio de origen. También se permite asignar ventanas que ya estaban abiertas al iniciar el gestor. Se guardan por separado la posición y el tamaño en Windows, la geometría del panel y la autorización de reapertura de cada ventana.

Probar una superficie de iconos propia con operaciones de archivos del shell y atajos configurables disponibles. La reasignación temporal de combinaciones de Windows queda para evaluación posterior. La interfaz pública de escritorios virtuales lista consultas de pertenencia y movimiento de ventanas. La creación y activación de escritorios por una aplicación quedan fuera de los métodos que documenta esta interfaz. [Microsoft Learn](https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-ivirtualdesktopmanager)

Probar las miniaturas DWM para el mapa. Al activar un panel, colocar la ventana real en sus coordenadas visibles y devolverle el foco. Validar tamaño mínimo, menús, ventanas modales, orden de ventanas y vuelta al mapa. Escuchar activaciones mediante eventos externos al proceso y verificar con una notificación real de Discord. Un juego a pantalla completa conserva el control de su presentación. [Miniaturas DWM](https://learn.microsoft.com/en-us/windows/win32/dwm/thumbnail-ovw) · [Eventos Win32](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook)

Evaluar el código MIT de Canvas Window Composer para la cámara y el mapa. Antes de adoptarlo revisar sus dependencias del shell y ensayar recuperación si el gestor se cierra en mitad de una transición. La decisión sobre reutilización sigue pendiente. [Repositorio](https://github.com/oreyg/CanvasWindowComposer)

## Fases y criterios de salida

El primer encargo es la [inspección de Canvas Window Composer](desarrollo/01-inspeccion-cwc.md). Los hitos posteriores son provisionales. Al cerrar la inspección, el responsable de diseño contrasta las evidencias con el objetivo del producto y propone qué construir y estimar a continuación. El usuario decide los cambios de alcance, experiencia o arquitectura.

Prueba de navegación del prototipo: con dos ventanas de VS Code en Programación, activar la segunda y alejar la vista. Ir a Inicio con el atajo y pulsarlo de nuevo. Deben recuperarse el punto y el zoom anteriores. Volver a Inicio y pulsar el icono de VS Code debe activar la segunda ventana. Desde el menú se debe poder seleccionar la primera o pedir una nueva.

| Fase | Resultado verificable | Jornadas equivalentes |
| --- | --- | ---: |
| 1. Inspección del candidato | Informe trazable de comportamiento, dependencias, límites y pruebas pendientes. Decisión de reutilización o ensayo necesario para tomarla. | Reestimar al conocer el entorno de prueba |
| Prototipo de viabilidad | Hito candidato. Probar la interacción nativa que resulte incierta tras la inspección. | Referencia anterior de 3–5, sujeta al informe |
| Espacios y archivos | Cambiar entre Juegos y Programación, ver sus iconos y fondo, guardar desde otra aplicación por ruta y mover una carpeta entre espacios. | 5–9 |
| Canvas | Desplazar la cámara, escribir dentro de un panel, ampliar, volver y conservar ambos acomodos. Verificar foco de Discord y retorno a un juego a pantalla completa. | 8–15 |
| Reapertura | Tras reiniciar, recuperar las ventanas marcadas en sus posiciones. Verificar dos ventanas de VS Code, una de Codex y un juego que sigue cerrado. | 4–7 |
| Estabilidad y rendimiento | Ensayar con 15 ventanas y seis paneles. Medir tiempos de cuadro y de activación, CPU, GPU y memoria del gestor. Comparar con la misma sesión con el gestor cerrado. Probar recuperación tras cierre inesperado y repetir las pruebas críticas en otra compilación de Windows 11. | 5–9 |

La estimación histórica de 25–45 jornadas contemplaba inspección y prototipo dentro del mismo bloque. Se revisará al elegir qué componentes reutilizar para evitar contabilizar ese trabajo dos veces.
