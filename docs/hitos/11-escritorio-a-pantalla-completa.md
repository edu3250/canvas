# Hito 11 · Escritorio a pantalla completa

**Estado:** en definición. El primer ensayo decide cómo alojar las ventanas.

## Objetivo

Abrir una aplicación y empezar a escribir en el espacio actual de Canvas, incluso si Canvas estaba minimizado. Para el uso diario, el destino es «que la version final me permita no usar el escritorio nativo de windows para nada».

**Fuera:** barra de tareas propia («eso es a largo plazo»). El paso 5 de Mesa de luz sigue pendiente.

## Decisiones de la entrevista

| Tema | Palabras del usuario | Comportamiento |
| --- | --- | --- |
| Escritorio y maximización | «Ocupar ya toda la pantalla», «siempre dejan un margen superior» | Canvas cubre el monitor. Maximizar un panel reserva la franja superior para los controles. La pantalla completa de la aplicación es un estado aparte. |
| Incorporación | «Siempre que Canvas esté abierto», «Volver a Canvas con la aplicación activa» | Una ventana principal nueva iniciada por el usuario entra en el espacio actual. Si Canvas estaba minimizado, vuelve. Los diálogos quedan con su aplicación. |
| Foco | «Respetar dónde estoy escribiendo», «Activar Chrome y dejarlo listo para escribir» | Una aparición espontánea en segundo plano se incorpora sin cambiar el foco. Un clic directo prepara la ventana para escribir. |
| Controles | «Los mismos controles de Canvas en todo momento» | Un único juego de controles por ventana. Verificar las pestañas y menús propios de Chrome y VS Code antes de elegir arquitectura. |
| Apariencia | «demasiado oscuras», «me gusta el tamaño de la captura 3» | Atenuación discreta y tamaño inicial uniforme. Probar 90 % y aproximadamente 1060 × 810 a la escala de la captura, ajustando al espacio disponible. Son valores de prueba. |
| Navegación | «el minimapa inferior no debe de verse en la pantalla si la aplicacion esta minimizada», «control + desplazamiento de la rueda del mouse siempre te deja alejar el lienzor de la ventana (salvo pantalla completa)» | Al minimizar Canvas desaparece la isla. Ctrl+rueda sobre el contenido aleja el lienzo y conserva el zoom de la aplicación, salvo en pantalla completa. |
| Apertura y alojamiento | «podria funcionar», «okay vamos a ello» | Juzgar la primera aparición en un prototipo. Comparar ventana superpuesta y ventana hija. Observar el destello con arranque en frío y segunda ventana. |
| Cierres pendientes | «procedamos en el siguiente hito quiero cambiar muchas cosas entre esas esto que queda pendiente» | Retomar el cierre diferido a bandeja y la salida de Canvas ante una aplicación bloqueada del PR #50. |

## Riesgos

Zona sensible. Probar foco, diálogos, escalado, devolución y cierre con procesos propios, documentos o perfiles descartables. Guardar el acomodo antes de mover ventanas en producto. Replantear la arquitectura si se pierde escritura, se ocultan diálogos, quedan controles duplicados o no se puede devolver la ventana. Medir fluidez con las mismas aplicaciones y carga. El destello y el coste quedan pendientes de observación.

## Pasos

- [ ] 1. Comparar el alojamiento y la apertura con Chrome y VS Code; decidir con el usuario a partir del ensayo · `prototipos/`.
- [ ] 2. Abrir Canvas a pantalla completa, ajustar tamaño inicial y luz en reposo, retirar la isla al minimizar · `src/Canvas.Desktop`.
- [ ] 3. Incorporar las ventanas nuevas, distinguir apertura voluntaria de aparición en segundo plano y validar un clic y Ctrl+rueda sobre la aplicación real · `src/Canvas.Desktop`, `tests/`.
- [ ] 4. Unificar controles y diferenciar ventana normal, maximizada con margen y pantalla completa · `src/Canvas.Desktop`.
- [ ] 5. Verificar cierre a bandeja diferido, cancelación y salida con una aplicación bloqueada, conservando recuperación · `src/`, `tests/`.

## Lo doy por bueno cuando

Abro Chrome con Canvas minimizado y puedo escribir en su panel. Una segunda ventana recibe su propio panel. Si estoy escribiendo en VS Code y aparece una ventana en segundo plano, sigo escribiendo en VS Code. Compruebo maximización, F11, zoom y devolución en Windows real.

## Bloqueos

La elección de arquitectura depende del prototipo. El mecanismo de incorporación debe respetar el foco también cuando el origen de una ventana resulte ambiguo.

Chrome como ventana hija sigue bloqueado para escritura. El usuario no lo vio aparecer fuera del panel y no pudo escribir. La comprobación del clic y la lectura del foco reprodujeron el fallo, incluso al intentar dar foco al alojar la ventana. La superposición queda como candidata para la comparación visual.

## Registro de sesiones

| Fecha | Modo | Herramienta y modelo | Qué se hizo | PR | Queda |
| --- | --- | --- | --- | --- | --- |
| 23-11-2026 | Definir | Codex · GPT-6 | Se acordaron el recorrido, la política de foco y el ensayo de apertura. | [#51](https://github.com/edu3250/canvas/pull/51) | Validar el prototipo y elegir arquitectura. |
| 23-11-2026 | Construir | Codex · GPT-6 | Preparado el ensayo de alojamiento para Chrome y VS Code. Release compiló sin advertencias. La comprobación con ventanas ocultas verificó padre, estilo, tamaño y separación. | [#52](https://github.com/edu3250/canvas/pull/52) | Ensayo visible con perfiles descartables tras el «adelante». Evaluar destello, escritura y diálogos. |
| 23-11-2026 | Construir | Codex · GPT-6 | En Windows, Chrome superpuesto recibió `canvas42` tras un solo clic en dos ejecuciones. La página confirmó el texto exacto. Como hija, el clic alcanzó Chrome y el foco quedó fuera. Se cerraron los procesos de prueba y se eliminaron sus temporales. | [#52](https://github.com/edu3250/canvas/pull/52), `6bdbe43` | Observar la apertura superpuesta con el usuario y contrastar con VS Code y sus diálogos antes de decidir. |
