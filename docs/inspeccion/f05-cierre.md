# Cierre de fase 5 · Arquitectura y navegación

La fase entrega la [arquitectura del producto](../arquitectura/propuesta-producto.md) y el [contrato del primer incremento](../desarrollo/05-arquitectura/primer-incremento.md). La matriz define quince pruebas para la implementación futura. [Plan aprobado en 6cf2958](https://github.com/edu3250/escritorio-canvas/pull/7) · [Entrega dictaminada en 0f1a3e3](https://github.com/edu3250/escritorio-canvas/pull/14).

## Criterios

| Tarea y criterio | Estado | Evidencia documental |
| --- | --- | --- |
| T01 · Límites e identidades | Verificado | Los cinco módulos tienen propietario y dependencias. Los recorridos distinguen trasladar una carpeta y recordar un juego con reapertura desactivada. [7f81a34 · PR 9](https://github.com/edu3250/escritorio-canvas/pull/9). |
| T02 · Activación durante un diálogo (H1) | Verificado | Activar C consume la petición de B y cerrar conserva C. Al invertir los eventos, cerrar aplica B una vez antes de activar C. [32e5373 · PR 10](https://github.com/edu3250/escritorio-canvas/pull/10). |
| T03 · Ampliación y retorno (H2) | Verificado | Al regresar de Inicio o de otro espacio el panel sigue ampliado. Desampliar restaura la cámara y el tamaño originales. [aef6ee2 · PR 11](https://github.com/edu3250/escritorio-canvas/pull/11). |
| T04 · Geometría y DPI (H3) | Verificado | Cálculo de 1200 × 900 píxeles a 144 DPI. Se guardan 800 × 600 unidades lógicas y se proyectan 400 × 300 a zoom 0,5. Al volver a zoom 1 se recuperan 800 × 600. [22bc678 · PR 12](https://github.com/edu3250/escritorio-canvas/pull/12). |
| T05 · Contrato de navegación | Verificado | El recorrido conserva las cámaras por espacio. El contrato fija firmas, transformaciones, errores e inmutabilidad. [f3d176a · PR 13](https://github.com/edu3250/escritorio-canvas/pull/13). |
| T06 · Matriz de aceptación | Verificado | Los quince casos especifican entrada, acción, salida y método para navegación, errores, proyección, consola, dependencias, CI y cobertura. [0f1a3e3 · PR 14](https://github.com/edu3250/escritorio-canvas/pull/14). |
| Pruebas de producto | Pendiente | Requieren implementar la biblioteca, la consola y la CI en una fase posterior. [Matriz](../desarrollo/05-arquitectura/primer-incremento.md#matriz-de-pruebas-futuras). |

## Límites

La verificación fue documental y aritmética. Los 144 DPI son un dato del ejemplo calculado. La versión de Windows, el SDK y las aplicaciones corresponden a los ensayos históricos citados en la [arquitectura](../arquitectura/propuesta-producto.md#evidencia-histórica).

## Decisiones para el usuario

Los criterios de cierre están resueltos. Abrir la fase de producto y autorizar sus paquetes corresponde al usuario.

## Esfuerzo

| Rol | Herramienta y modelo | Intentos | Duración registrada |
| --- | --- | --- | --- |
| Planificación | Codex, GPT-6 | Una pasada de redacción del plan aprobado | Sin medición. [PR 7](https://github.com/edu3250/escritorio-canvas/pull/7). |
| Ejecución | Codex, GPT-6 | Una pasada por tarea | 328 s desde la comprobación de base hasta el commit, sumando 83, 38, 66, 33, 53 y 55 s. [Registro por tarea](https://github.com/edu3250/escritorio-canvas/pull/14). |
| Revisión | Codex, GPT-6 | Un dictamen por tarea | Sin medición del tiempo activo. Los seis dictámenes constan en los PR enlazados. |
