# Cierre de fase 6 · Primer incremento de navegación

La consola ejecuta el recorrido entre Programación, Inicio y Juegos. El dominio conserva la cámara de cada espacio y su vista de retorno. [Plan aprobado en 19a9590](https://github.com/edu3250/escritorio-canvas/pull/15) · [Entrega dictaminada en 42cf6b7](https://github.com/edu3250/escritorio-canvas/pull/24).

## Criterios

Los quince criterios quedaron **verificados**. La [matriz por criterio](../desarrollo/05-arquitectura/recorrido-incremento.md#evidencia-de-la-matriz-de-aceptación) enlaza cada resultado con su prueba, código o registro en el SHA ensayado.

| Criterios | Estado | Evidencia |
| --- | --- | --- |
| Navegación, identidad, errores, conservación del estado y proyección inversa | Verificado | Las 17 pruebas xUnit pasaron en Release. [Pruebas en el SHA ensayado](https://github.com/edu3250/escritorio-canvas/tree/14844d65d9aeb570e7b995364a862abf48e72823/tests/Canvas.Domain.Tests). |
| Consola en éxito y error | Verificado | Cinco procesos reprodujeron el recorrido, las dos selecciones válidas y los dos diagnósticos. Códigos observados en ese orden: 0, 0, 0, 1 y 2. [Salida capturada](../desarrollo/05-arquitectura/recorrido-incremento.md#salida-capturada). |
| Dependencias del dominio | Verificado | La inspección de código y referencias confirmó operaciones en memoria y dependencia exclusiva de Microsoft.NETCore.App. [Dominio ensayado](https://github.com/edu3250/escritorio-canvas/tree/14844d65d9aeb570e7b995364a862abf48e72823/src/Canvas.Domain). |
| CI, cobertura y rechazo de informes inválidos | Verificado | La compilación y las pruebas pasaron en Windows. El control midió 70 de 71 líneas cubiertas (98,59 %) y pasó los nueve casos de comprobación. [CI del SHA ensayado](https://github.com/edu3250/escritorio-canvas/actions/runs/44629503532/job/135932065221). |

## Entorno y límites

Windows 11 Pro, compilación 26200, SDK .NET 10.0.401. El DPI no interviene en las coordenadas lógicas de la consola. La interfaz gráfica, las ventanas nativas y la persistencia quedan para fases posteriores. [Comandos de reproducción](../desarrollo/05-arquitectura/recorrido-incremento.md#reproducir-el-recorrido).

## Decisiones para el usuario

Los criterios de esta fase están resueltos. Abrir otra fase requiere autorización del usuario.

## Esfuerzo

| Rol | Herramienta y modelo | Intentos y duración registrada |
| --- | --- | --- |
| Planificación | Codex GPT-6 | Una redacción y un ajuste de prueba. Duración sin medición. [PR del plan](https://github.com/edu3250/escritorio-canvas/pull/15). |
| Ejecución | Codex GPT-6 | Nueve entregas y una corrección en T08. Las ventanas registradas de T01 a T08 suman 655 s antes de la corrección. T09 registró de 16:33 a 16:35, hora local. Duración total sin medición. [Corrección de T08](https://github.com/edu3250/escritorio-canvas/pull/23) · [Registro de T09](https://github.com/edu3250/escritorio-canvas/pull/24). |
| Revisión | Codex GPT-6 | Diez dictámenes para nueve tareas, incluidos el hallazgo de comparación Unicode y la verificación de la corrección. Tiempo activo total sin medición. [Dictamen de la corrección](https://github.com/edu3250/escritorio-canvas/pull/23#issuecomment-7024056497). |
