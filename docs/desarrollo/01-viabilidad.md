# Boceto histórico · Posibles pruebas de un prototipo

**Estado:** referencia de diseño. El encargo vigente es la [inspección del candidato](01-inspeccion-cwc.md). El informe determinará qué pruebas de este boceto merecen pasar al siguiente encargo.

**Pregunta de salida:** en un monitor, ¿se puede abrir una aplicación desde Inicio, trabajar en su ventana nativa dentro del espacio que ocupa su panel y volver al mismo punto del canvas con el foco correcto?

**Presupuesto orientativo:** 3–5 jornadas equivalentes, revisión incluida. Si al acabar la primera jornada no se consigue pasar de una vista del mapa a escribir en una ventana real, priorizar ese fallo y comunicar la causa antes de invertir tiempo en menús o apariencia.

## Contexto para quien implementa

Lee el [diseño del producto](../diseno-inicial.md) y el [mapa de fases](../plan-por-fases.md). La demostración usa un solo espacio temporal, Programación, un monitor y atajos configurables disponibles. Puede comenzar con accesos de prueba en Inicio. Las carpetas reales por espacio, la restauración tras reiniciar y la carga de 15 ventanas corresponden a fases posteriores.

El responsable de diseño establece el comportamiento y revisa los resultados. El modelo ejecutor puede resolver detalles locales, corregir fallos y repetir pruebas. Si la solución exige cambiar la interacción acordada, adoptar una interfaz privada de Windows o introducir una dependencia estructural, entrega el caso observado y las alternativas para que el usuario decida.

## Encargos en orden

Se puede dar un solo encargo a un modelo por sesión. Antes de continuar, el siguiente ejecutor debe poder reproducir el resultado anterior a partir del código y de las instrucciones entregadas.

### 1. Preparar una comparación reproducible

- Inspeccionar el repositorio y el entorno Windows. Si solo hay documentación, crear el proyecto mínimo del prototipo usando C# y el SDK disponible que sea compatible. Registrar cómo compilar, ejecutar y cerrar. Trabajar con ventanas y documentos de prueba.
- Evaluar Canvas Window Composer como candidato para cámara, mapa y conversión de coordenadas. Registrar la revisión examinada, las partes reutilizables, las dependencias del shell y las obligaciones de licencia de cualquier código incorporado. Si el candidato no compila, registrar el error y continuar el ensayo con una implementación mínima basada en interfaces públicas. [Repositorio del candidato](https://github.com/oreyg/CanvasWindowComposer)
- **Entrega:** prototipo compilable con una superficie de trabajo, versión de Windows y escala de pantalla registradas, instrucciones reproducibles y comparación inicial del candidato.

### 2. Conseguir que el primer panel sea realmente editable

- Crear en Inicio un acceso a VS Code. Al activarlo, identificar la ventana que abrió o activó la aplicación. Un proceso recién creado y una ventana visible pueden tener ciclos de vida distintos. Si hay varias candidatas ambiguas, permitir elegir una explícitamente en el prototipo.
- Antes de mover la ventana, conservar su posición, tamaño y estado original, incluido si estaba maximizada. En el mapa se puede probar una miniatura en vivo. Al activar el panel, colocar la ventana nativa en el rectángulo visible del panel, respetar el tamaño que acepte y darle el foco. Los demás paneles deben seguir visibles.
- Reducir el ancho de la ventana como en las capturas de referencia. Con el ajuste de línea activado en VS Code, la misma frase debe ocupar más renglones y conservar el tamaño de letra. Comprobar que el teclado llega a la ventana correcta, que el menú responde y que un diálogo se puede cerrar para seguir escribiendo.
- **Entrega:** demostración reproducible con VS Code y otro panel visible. Registrar por separado tamaño solicitado y obtenido si una aplicación aplica un mínimo.

### 3. Conectar Inicio, cámara y ventanas

- Mantener por separado la posición de cada icono y panel en el lienzo, la vista actual (posición y zoom) y la vista de retorno. Al ir a Inicio, guardar la vista actual una sola vez. Al volver, recuperar ambos valores. Mover la cámara conserva las posiciones de los objetos.
- Mantener la última ventana utilizada por aplicación dentro del espacio. Al pulsar el icono, activar esa ventana si sigue abierta. Si se cerró, elegir la última restante. Si ya no hay ninguna, iniciar la aplicación. El menú permite elegir una ventana concreta o solicitar una nueva donde la aplicación lo admita.
- Al salir del canvas, restituir el acomodo de escritorio de las ventanas de prueba que sigan abiertas. Al volver al canvas durante la misma sesión, recuperar su acomodo en el lienzo. Liberar el atajo al cerrar el gestor.
- **Entrega:** recorrido completo por Inicio y los paneles, más pruebas de la lógica de ida y vuelta y de selección cuando se cierra la última ventana usada.

### 4. Ejecutar las pruebas y comparar opciones

| Caso | Acción | Resultado observable |
| --- | --- | --- |
| Edición nativa | Abrir VS Code desde Inicio. Estrechar el panel, pulsar dentro y escribir. Abrir un menú y el diálogo Abrir archivo, cancelarlo y volver a escribir. | Se conserva el tamaño de letra. El texto, el teclado y el diálogo se comportan como en la ventana normal. Otro panel permanece visible. |
| Viaje a Inicio | Desplazar la vista, cambiar el zoom, pulsar dos veces el atajo de Inicio. | La primera pulsación muestra los iconos. La segunda recupera posición y zoom. Verificar los valores de cámara además de la observación visual. |
| Dos ventanas | Activar sucesivamente dos ventanas de VS Code, volver a Inicio y pulsar su icono. Usar después el menú para elegir la primera. Cerrar una y repetir. | Se activa la última utilizada que siga abierta. La elección explícita activa la otra y el cierre elimina su entrada. |
| Compatibilidad | En Codex escribir en el campo de entrada. En Unity usar una escena de prueba, activar un control y abrir un menú. Alternar foco entre ambas aplicaciones. | Cada acción llega al panel elegido. Registrar por aplicación cualquier salto de tamaño, pérdida de foco o menú oculto. |
| Regreso a Windows | Comenzar con una ventana maximizada y otra normal. Acomodarlas en el canvas, salir, volver a entrar y cerrar el prototipo. | Cada vista conserva su distribución durante la sesión. Tras cerrar se puede seguir trabajando en las ventanas desde Windows. |
| Interrupción | Con ventanas de prueba, cerrar una aplicación mientras su panel está activo y terminar el gestor de forma inesperada en una ejecución separada. | Registrar si quedan ventanas inaccesibles, fuera del monitor o sin posibilidad de recibir foco. Si sucede, recuperar la sesión de prueba y documentar la condición que lo produjo. |

Automatizar las comprobaciones de cámara y elección de ventana que puedan ejecutarse sin aplicaciones externas. Para interacción nativa, registrar los pasos y el resultado en un Windows real. Si falta una aplicación o un entorno adecuado, marcar ese caso como pendiente y especificar qué falta para reproducirlo.

## Entrega de cierre para el responsable de diseño

El ejecutor deja, junto al prototipo, un informe breve con estos datos:

1. Revisión del código, entorno, comandos exactos de compilación, ejecución y pruebas.
2. Resultado de cada caso: pasó, falló o quedó pendiente. Incluir observación, pasos y evidencia suficiente para que otro modelo pueda verificarlo.
3. Código propio y componentes reutilizados, con revisión y atribución aplicable. Comparar ambas opciones por foco, recuperación y dependencia de detalles de Windows.
4. Tiempo de implementación, integración y pruebas. Identificar cuánto exigió revisión humana y qué consumió más esfuerzo del previsto.
5. Recomendación: continuar con el enfoque probado, corregir un fallo acotado y repetir, o replantear el enfoque. Incluir la consecuencia para el siguiente hito.

**Cierre de fase:** el responsable de diseño reproduce el recorrido principal y contrasta las limitaciones con el uso real del usuario. Una compilación exitosa por sí sola deja abierta la pregunta de viabilidad. Si falla la edición nativa o quedan ventanas inaccesibles, la siguiente planificación parte de ese fallo. Si el recorrido funciona, el informe servirá para decidir cuánto reutilizar y redactar la fase de espacios y archivos.
