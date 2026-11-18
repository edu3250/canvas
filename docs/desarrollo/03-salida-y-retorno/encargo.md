# Fase 03 · Salida y retorno al canvas

**Estado:** plan disponible para ejecución.

## Decisión y alcance

Decidir si podemos conservar dos acomodos por ventana durante una sesión. Usaremos dos ventanas de control en procesos propios del [prototipo desechable](../../../prototipos/panel-nativo/README.md). La [fase 2](../../inspeccion/ensayo-panel-nativo.md) comprobó foco y tamaño. El [ADR de sesión](../../adr/002-sesion-y-reapertura.md) distingue recordar posiciones y lanzar aplicaciones. Este ensayo termina al cerrar el gestor normalmente. Reinicio, caída abrupta, ventanas minimizadas y multimonitor quedan para otros encargos.

## Diseño y criterio de salida

El modo de ensayo usa el editor y el vecino que ya crea el modo control. El recorrido programático reproduce las transiciones de geometría sin exigir interacción con la cámara ni simular clics. Una ventana parte normal y otra maximizada. Se conserva para cada una un WINDOWPLACEMENT original inmutable en coordenadas de área de trabajo y un rectángulo de panel medido en coordenadas de pantalla. Usar cada representación con la misma API que la produjo evita desplazar la ventana al alternar entre vistas.

| Transición | Evidencia para aprobar |
| --- | --- |
| Entrar y cambiar el acomodo | Las dos ventanas pasan a estado normal. Sus rectángulos obtenidos coinciden con los elegidos para el canvas. |
| Salir al escritorio | Cada ventana recupera su estado y rectángulo normal original. La maximizada también recupera sus límites visibles. |
| Volver y repetir | Las dos recuperan el último acomodo del canvas. La segunda salida recupera los originales sin deriva. |
| Terminar | Se verifica la restauración antes del cierre de los procesos de prueba y la limpieza del directorio temporal. |

## Tareas

| Orden | Entrega | Dependencia | Interactiva |
| --- | --- | --- | --- |
| 1 | [Adaptador de colocación Win32](tareas/01-colocacion.md) | Plan publicado | No |
| 2 | [Recorrido de dos ventanas](tareas/02-recorrido.md) | t01 verificada | No durante esta tarea |
| 3 | [Reproducción y evidencia](tareas/03-evidencia.md) | t02 verificada | Sí, con el «adelante» de esa sesión |

Toda la lógica ensayada depende del estado real de HWND. La prueba de integración constituye el oráculo. Se usan las dependencias existentes. La ejecución continúa en el PR #2. Revisión independiente autoriza y fusiona la implementación final. Revisión cerrará con informe en `docs/inspeccion/`, esfuerzo acumulado y actualización del mapa y del README.

## Riesgos

La posición normal de una ventana maximizada difiere de sus límites visibles. Un éxito de la llamada Win32 tampoco demuestra que la ventana ya cambió. Registrar ambas mediciones tras estabilizarse. Un cambio de DPI o del área de trabajo invalida la comparación y se consigna como pendiente.
