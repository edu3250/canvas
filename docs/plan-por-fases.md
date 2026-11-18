# Propuesta de arquitectura y esfuerzo

Borrador para una primera versión de uso personal en Windows 11, con un monitor. La estimación contempla redimensionamiento nativo para editar y zoom para recorrer el mapa. Se respetan los tamaños que admite cada aplicación.

## Estructura

Una aplicación de escritorio en C# con un adaptador Win32 y almacenamiento local. La sesión empieza en el canvas. Cada espacio tiene una identidad, una carpeta real, un fondo y posiciones de iconos y paneles. Lanzar una aplicación desde un icono vincula la ventana resultante al espacio de origen. También se permite asignar ventanas que ya estaban abiertas al iniciar el gestor. Se guardan por separado la posición y el tamaño en Windows, la geometría del panel y la autorización de reapertura de cada ventana.

Probar una superficie de iconos propia con operaciones de archivos del shell y atajos configurables disponibles. La reasignación temporal de combinaciones de Windows queda para evaluación posterior. La interfaz pública de escritorios virtuales lista consultas de pertenencia y movimiento de ventanas. La creación y activación de escritorios por una aplicación quedan fuera de los métodos que documenta esta interfaz. [Microsoft Learn](https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-ivirtualdesktopmanager)

Probar las miniaturas DWM para el mapa. Al activar un panel, colocar la ventana real en sus coordenadas visibles y devolverle el foco. Validar tamaño mínimo, menús, ventanas modales, orden de ventanas y vuelta al mapa. Escuchar activaciones mediante eventos externos al proceso y verificar con una notificación real de Discord. Un juego a pantalla completa conserva el control de su presentación. [Miniaturas DWM](https://learn.microsoft.com/en-us/windows/win32/dwm/thumbnail-ovw) · [Eventos Win32](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook)

Evaluar el código MIT de Canvas Window Composer para la cámara y el mapa. Antes de adoptarlo revisar sus dependencias del shell y ensayar recuperación si el gestor se cierra en mitad de una transición. La decisión sobre reutilización sigue pendiente. [Repositorio](https://github.com/oreyg/CanvasWindowComposer)

## Fases y criterios de salida

La [inspección estática de Canvas Window Composer](inspeccion/informe-cwc.md) identificó una diferencia central entre el mapa de CWC y los paneles editables del producto. El [segundo encargo](desarrollo/02-ensayo-panel-nativo.md) prueba la activación de una ventana real dentro de su panel. Los hitos posteriores son provisionales. El usuario decide los cambios de alcance, experiencia o arquitectura.

Desde la fase 3, cada fase sigue el [ciclo de AGENTS.md](../AGENTS.md#5-ciclo-de-una-fase). Planificación entrega el plan en un PR `fNN-plan`, con el encargo en `docs/desarrollo/NN-slug/encargo.md` y una tarea por archivo en `tareas/`. El usuario lo fusiona para autorizar la fase. Ejecución entrega cada tarea en su propio PR, que pasa por Revisión antes de que el usuario lo fusione. Revisión cierra la fase con un informe en `docs/inspeccion/`. El número y el alcance de la fase 3 se fijan cuando el usuario la abre. El [informe del ensayo](inspeccion/ensayo-panel-nativo.md) propone dedicarla a salir del canvas y recuperar el acomodo original de las ventanas.

Prueba de navegación del prototipo: con dos ventanas de VS Code en Programación, activar la segunda y alejar la vista. Ir a Inicio con el atajo y pulsarlo de nuevo. Deben recuperarse el punto y el zoom anteriores. Volver a Inicio y pulsar el icono de VS Code debe activar la segunda ventana. Desde el menú se debe poder seleccionar la primera o pedir una nueva.

| Fase | Resultado verificable | Jornadas equivalentes |
| --- | --- | ---: |
| 1. Inspección del candidato | Informe estático entregado y ensayos priorizados. | Sin registro de jornadas reales |
| 2. Ensayo de panel nativo | Activar y editar en dos tamaños con el vecino visible, abrir y cancelar un diálogo, recuperar el foco. | La referencia de 3–5 jornadas correspondía al prototipo de viabilidad más amplio |
| Espacios y archivos | Cambiar entre Juegos y Programación, ver sus iconos y fondo, guardar desde otra aplicación por ruta y mover una carpeta entre espacios. | 5–9 |
| Canvas | Desplazar la cámara, escribir dentro de un panel, ampliar, volver y conservar ambos acomodos. Verificar foco de Discord y retorno a un juego a pantalla completa. | 8–15 |
| Reapertura | Tras reiniciar, recuperar las ventanas marcadas en sus posiciones. Verificar dos ventanas de VS Code, una de Codex y un juego que sigue cerrado. | 4–7 |
| Estabilidad y rendimiento | Ensayar con 15 ventanas y seis paneles. Medir tiempos de cuadro y de activación, CPU, GPU y memoria del gestor. Comparar con la misma sesión con el gestor cerrado. Probar recuperación tras cierre inesperado y repetir las pruebas críticas en otra compilación de Windows 11. | 5–9 |

La estimación histórica de 25–45 jornadas contemplaba inspección y prototipo dentro del mismo bloque. Se revisará al elegir qué componentes reutilizar para evitar contabilizar ese trabajo dos veces. Las jornadas reales de cada fase se tomarán del esfuerzo que registran sus PR y se resumirán en su informe de cierre.
