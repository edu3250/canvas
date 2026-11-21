# Hito 09 · Ventanas reales en el lienzo

**Estado:** en curso

## Objetivo

Las ventanas de las aplicaciones del usuario viven como paneles con miniatura en vivo en el lienzo de su espacio. Con un clic se trabaja en la ventana real en ese mismo lugar, y se vuelve al mapa con el minimapa o con un atajo.

**Fuera:** recordar los paneles tras reiniciar Escritorio Canvas; juegos a pantalla completa; ventanas que se ejecutan como administrador; notificaciones (Discord); botón propio de minimizar con panel plegado; entrada automática de la ventana al abrirla desde un icono de Inicio.

## Decisiones de la entrevista

Entrevista del 20-11-2026. El diario de uso estaba vacío y el candidato salió de `estado.md`. El usuario mostró capturas de Deska (minimapa, control de zoom y barra de panel) como referencia.

| Pregunta | Respuesta del usuario | Consecuencia |
| --- | --- | --- |
| P1 · Objetivo del hito | «si, la opcion a» | Ventanas reales en el lienzo, frente a un fondo propio por espacio u otra mejora. |
| P2 · Cómo entra una ventana | «ok» | Un botón «Añadir ventana» en la barra superior lista las ventanas abiertas, y el usuario elige cuál pasa al espacio actual. La entrada automática al abrir desde un icono queda para otro hito. |
| P3 · Tamaño de Escritorio Canvas | «si maximizada» | La ventana del lienzo se abre maximizada, con la barra de tareas visible. |
| P4 · Clic en un panel con el zoom alejado | «a me parece bien» | El lienzo se acerca al 100 % centrado en el panel y la ventana real ocupa su rectángulo con su tamaño normal. Al volver al mapa se recupera el zoom anterior. La ventana no se redimensiona al tamaño del panel en pantalla. |
| P5 · Cómo volver al mapa | «para eso esta este pequeño panel que aparte de deska tambien estaba en el windows canvas composer, busca como implementarlo» | Un minimapa en la esquina inferior derecha, encima de la barra de tareas. Será una ventana propia, siempre encima, que no roba el foco ni aparece en la barra de tareas, como el `MinimapOverlay` de CWC, pero clicable. Se usa como referencia, sin copiar código. |
| P5a · Cuándo se ve el minimapa | «b)» | Visible en el mapa. Mientras se trabaja en una ventana se oculta, reaparece al llevar el ratón al borde inferior de la pantalla y se oculta de nuevo al apartarlo. |
| P5b · Qué hace el minimapa | «si un click en un rectangulo te lleva a ese panel y activa su ventana» | Muestra un rectángulo por panel y el de la zona visible. Un clic en un panel lleva a él y activa su ventana; un clic en una zona vacía devuelve al mapa centrado ahí. Sin contador de ventanas. |
| P5c · Atajo de teclado | «si como complemento» | Ctrl+Alt+Espacio vuelve al mapa. |
| P5d · Control de zoom | «sirve para ampliar o alejar el canvas» y «si adelante» | Junto al minimapa: −, porcentaje, + y ⛶ para encuadrar todos los paneles del espacio. |
| P6 · Red de seguridad | «si las tres» | Antes de mover una ventana se guardan su posición y su tamaño originales. Vuelve a ese sitio al quitar su panel, al cerrar Escritorio Canvas y al reabrirlo tras un cierre brusco. Un acceso «Devolver ventanas» funciona aunque Escritorio Canvas no esté abierto. |
| P7 · Qué queda fuera | «si eso queda fuera» | Lo listado en «Fuera». Las ventanas maximizadas sí entran, y se restauran a tamaño normal al entrar al lienzo. |
| P8 · Botones del panel | «un boton x en el panel cierra esa ventana esa es la gracia de ser un escritorio» | Barra propia sobre cada panel en el mapa, porque la barra de la aplicación se ve en la miniatura pero no se puede pulsar. Mientras se trabaja en la ventana, valen sus botones nativos. |
| P8a · Cerrar y quitar | «si adelante» | ✕ envía la misma orden que la ✕ de la aplicación (`WM_CLOSE`) y nunca termina el proceso. Si aparece un diálogo de guardar, se activa la ventana. Si el usuario cancela, el panel se queda. Si la ventana se esconde en la bandeja o se cierra, el panel desaparece. El menú … contiene «Quitar del lienzo», que devuelve la ventana a Windows sin cerrarla. |
| P8b · Maximizar | «si adelante» | ⤢ lleva al panel y la ventana real ocupa la pantalla mientras se trabaja. Al volver al mapa recupera su tamaño de panel. |
| P8c · Minimizar | «si adelante» | Sin botón propio en este hito. Si la ventana se minimiza con su botón nativo, el panel muestra el icono de la aplicación y el aviso «minimizada», y un clic la restaura. |
| Resumen y dos detalles propuestos | «adelante» | Un panel nuevo toma el tamaño actual de la ventana y se coloca en el centro de la zona visible, o en el hueco libre más cercano. Condición de replanteo: ver «Riesgos». |
| Barra del lienzo y tamaño del panel | «si crea la barra propia» y «al hacer la ventana mas ancha como en la segunda captura la ventana real sale de la ventana de la apliacion» | Se permite la barra propia para la composición por capas. El panel sigue el tamaño normal de la ventana nativa cuando se redimensiona. |
| Alineación y salto de cámara | «las ventanas se ven muy desalineadas con el contenido que vive dentro» y «al dar click a una ventana para activarla se hace un salto de camara» | La tarjeta sigue las proporciones de la miniatura. Un clic directo al 100 % conserva la cámara. Si cambia la vista para activar el panel, se anima con el ajuste de animación de Windows. |
| Volver al mapa desde una ventana que estaba minimizada | «adelante» tras la propuesta de dejar la ventana abierta detrás del lienzo. | El regreso al mapa conserva la ventana fuente para DWM. La minimización nativa sigue siendo deliberada. El acomodo original queda en el registro hasta quitar el panel, cerrar Canvas o recuperar tras un fallo. |
| Continuidad visual tras el paso 4 | «adelante» | En el siguiente PR, al activar directamente desde una vista alejada, acercar al 100 % alrededor del panel y desplazar solo lo necesario para trabajar. Al 100 % conservar la cámara. Mantener el borde visible durante el relevo. Maximizar mediante una acción explícita y recuperar el encuadre anterior al salir. Esta decisión ajusta P4 para la activación directa. |
| Traspaso del paso 5 | «sí a todas», en la entrevista del [hito 10](10-mesa-de-luz.md) (Q2) | La barra del panel y el aviso de minimizada pasan al hito 10 como lengüeta de Mesa de luz. Este hito se cierra con la continuidad visual (paso 6). Contradice el alcance acordado aquí; manda la decisión del usuario en el chat. |

## Riesgos

- **Zona sensible:** casi todo el hito, porque mueve, enfoca, redimensiona y cierra ventanas reales. Los PR del paso 2 en adelante los revisa otra sesión.
- **Red de seguridad primero.** El paso 2 (acomodos originales y «Devolver ventanas») se fusiona antes que cualquier paso que mueva ventanas. Su formato persistido necesita un ADR.
- **Ensayos interactivos.** Las pruebas con ventanas reales toman el primer plano y solo se lanzan tras el «adelante» del usuario. Las automatizadas usan ventanas propias y desechables, identificadas por PID, nunca ventanas del usuario. Las pruebas de ✕ y de cierre brusco usan solo trabajo descartable.
- **Foco de Windows.** `SetForegroundWindow` puede negarse. Hay que comprobar el resultado y, si falla, no dejar la ventana a medio colocar.
- **Atajo ocupado.** Si Ctrl+Alt+Espacio ya está registrado por otra aplicación, el minimapa sigue funcionando y se avisa al usuario.
- **Replanteo.** Si una aplicación común (VS Code, Chrome, Discord) no puede activarse en su panel o devolverse a su sitio con APIs públicas, Construir se detiene y presenta alternativas. Las interfaces privadas, los permisos de administrador y el código de CWC requieren la autorización del usuario.

## Pasos

- [x] 1. Lienzo maximizado, control de zoom (−, %, +, ⛶) y minimapa con los iconos de Inicio, todavía sin ventanas. Fuera de la zona sensible · `src/Canvas.Desktop`, `src/Canvas.Domain`, `tests/`
- [x] 2. **Zona sensible.** Registro de acomodos originales en disco antes de mover nada, devolución al quitar, al cerrar y tras un cierre brusco, y acceso «Devolver ventanas» independiente. ADR del formato · `src/`, `tests/`, `docs/adr/`
- [x] 3. **Zona sensible.** «Añadir ventana» con la lista de ventanas abiertas, paneles con miniatura DWM en vivo que se arrastran por el lienzo y colocación del panel nuevo · `src/Canvas.Desktop`, `src/Canvas.Domain`, `tests/`
- [x] 4. **Zona sensible.** Activar la ventana en su panel con el acercamiento al 100 % y volver al mapa con el minimapa, Ctrl+Alt+Espacio o un clic fuera. El minimapa se oculta mientras se trabaja y reaparece en el borde inferior · `src/Canvas.Desktop`
- [ ] ~~5. **Zona sensible.** Barra del panel (✕, ⤢, «Quitar del lienzo») y aviso de ventana minimizada · `src/Canvas.Desktop`~~ Pasa al paso 4 del [hito 10](10-mesa-de-luz.md), que la construye como lengüeta de Mesa de luz.
- [ ] 6. **Zona sensible.** Continuidad visual entre el mapa y el panel acordada tras el paso 4 · `src/Canvas.Desktop`

Cada paso va en su propio PR. Los pasos 2–6 incluyen en el PR el ensayo con ventanas reales que hizo el usuario tras su «adelante».

## Lo doy por bueno cuando

Con VS Code, Chrome y Discord abiertos:

1. Añado VS Code y Chrome a Programación y Discord a Juegos.
2. Escribo en VS Code desde su panel y vuelvo al mapa con el minimapa y también con Ctrl+Alt+Espacio. Activo un panel desde una vista alejada y la cámara se acerca sin saltos.
3. Cierro Escritorio Canvas y todas las ventanas que quedan vuelven a donde estaban antes.

Cerrar con ✕, quitar del lienzo y maximizar pasaron a la prueba final del [hito 10](10-mesa-de-luz.md).

## Bloqueos

Falta confirmar la vista en vivo con Claude y vídeo real en Chrome. Con una ventana Chrome descartable, la miniatura DWM de Escritorio Canvas captó los dos colores de una animación local mientras el lienzo por capas cubría el navegador.

## Registro de sesiones

| Fecha | Modo | Herramienta y modelo | Qué se hizo | PR | Queda |
| --- | --- | --- | --- | --- | --- |
| 20-11-2026 | Definir | Claude Code · claude-opus-5-5 | Entrevista en cuatro rondas (P1–P8c) con capturas de Deska como referencia y lectura del minimapa de CWC. Redacción del hito. | [#39](https://github.com/edu3250/escritorio-canvas/pull/39) | Construir el paso 1 |
| 20-11-2026 | Construir | Codex · GPT-6 | Paso 1. Inicio maximizado, control de zoom y minimapa interactivo. Compilación, 44 pruebas y dibujo WPF fuera de pantalla. | [#40](https://github.com/edu3250/escritorio-canvas/pull/40) | Prueba de uso, autorización de fusión y paso 2. |
| 20-11-2026 | Construir | Codex · GPT-6 | El usuario probó el paso 1 y confirmó «funciona bien». Autorizó la fusión en el chat. | [#40](https://github.com/edu3250/escritorio-canvas/pull/40) | Construir el paso 2. |
| 20-11-2026 | Construir | Codex · GPT-6 | Propuesta de ADR 004 para el registro de devolución de ventanas. | [#41](https://github.com/edu3250/escritorio-canvas/pull/41) | Autorizar el formato e implementar el paso 2. |
| 20-11-2026 | Construir | Codex · GPT-6 | El usuario aprobó el ADR 004 («adelante aprobado»). Se implementó el paso 2, con 54 pruebas y un ensayo en Windows con una ventana y un proceso descartables. | [#41](https://github.com/edu3250/escritorio-canvas/pull/41) | Revisar el PR de zona sensible y fusionarlo antes del paso 3. |
| 20-11-2026 | Revisar | Codex · GPT-6 | Compilación Release y 54 pruebas correctas. La revisión detectó que recuperar desde otra sesión de Windows puede borrar el acomodo original. | [#41](https://github.com/edu3250/escritorio-canvas/pull/41) | Construir corrige el aislamiento por sesión y añade una prueba de regresión. |
| 20-11-2026 | Construir | Codex · GPT-6 | Se corrigió el aislamiento del registro de devolución por sesión de Windows. Pasaron 56 pruebas, incluidas dos regresiones con dos sesiones y el mismo identificador de ventana. | [#41](https://github.com/edu3250/escritorio-canvas/pull/41) | Revisar la corrección y fusionar antes del paso 3. |
| 20-11-2026 | Revisar | Codex · GPT-6 | Verificado el aislamiento de acomodos al guardar y devolver con el mismo identificador en dos sesiones simuladas. Compilación Release limpia y 56 pruebas superadas. | [#41](https://github.com/edu3250/escritorio-canvas/pull/41) | Construir el paso 3. |
| 20-11-2026 | Construir | Codex · GPT-6 | Se añadió el selector, la colocación y los paneles con miniatura DWM. Pasaron 60 pruebas y un ensayo sin activar las ventanas descartables. | [#42](https://github.com/edu3250/escritorio-canvas/pull/42) | Prueba de uso y revisión en otra sesión antes del paso 4. |
| 20-11-2026 | Revisar | Codex · GPT-6 | Compilación Release limpia y 60 pruebas superadas. Se pidieron cambios en el recorte de la miniatura y la retirada del panel cuando el proceso reutiliza el identificador de ventana. | [#42](https://github.com/edu3250/escritorio-canvas/pull/42) | Construir corrige ambos casos. Quedan la prueba de uso y la nueva revisión. |
| 20-11-2026 | Construir | Codex · GPT-6 | Se corrigieron el recorte de las miniaturas y la detección de ventanas reemplazadas. Pasaron 64 pruebas y un ensayo Win32 con ventanas descartables. | [#42](https://github.com/edu3250/escritorio-canvas/pull/42) | Prueba de uso y nueva revisión antes del paso 4. |
| 20-11-2026 | Revisar | Codex · GPT-6 | Verificados el recorte por control y la retirada de la instancia reemplazada en d97d126. Compilación Release limpia y 64 pruebas superadas. | [#42](https://github.com/edu3250/escritorio-canvas/pull/42) | Prueba de uso del paso 3 antes de fusionar. |
| 20-11-2026 | Revisar | Codex · GPT-6 | El usuario confirmó «funcionan adelante» tras probar la miniatura en movimiento, el arrastre y zoom, la pertenencia al espacio y la retirada del panel al cerrar la ventana. | [#42](https://github.com/edu3250/escritorio-canvas/pull/42) | Construir el paso 4. |
| 20-11-2026 | Construir | Codex · GPT-6 | Paso 4. Activación del panel al 100 %, regreso al mapa y minimapa al borde inferior. Pasaron 65 pruebas y el ensayo Win32 verificó foco, tamaño normal y devolución con una ventana descartable en tres estados. Windows rechazó el registro del atajo en el ensayo local. | [#43](https://github.com/edu3250/escritorio-canvas/pull/43) | Prueba de uso y revisión en otra sesión antes del paso 5. |
| 20-11-2026 | Construir | Codex · GPT-6 | En la prueba con Claude el usuario informó que la activación mostraba el escritorio y la miniatura quedaba en blanco al volver. Se ajustaron el orden de ventanas y el foco del minimapa, se renovó la miniatura al salir y se excluyó el escritorio de Windows del selector. Pasaron 65 pruebas y un ensayo con la ventana WPF de otro proceso descartable. | [#43](https://github.com/edu3250/escritorio-canvas/pull/43) | Repetir la prueba con Claude y revisar en otra sesión. |
| 20-11-2026 | Construir | Codex · GPT-6 | Chrome funcionó dentro del panel en la prueba del usuario. Se corrigió el arrastre derecho que cerraba la edición. Pasaron 65 pruebas y un ensayo Win32 con una ventana descartable confirmó paneo al 100 %, tamaño y devolución. En el ensayo con Chrome descartable faltó validar el foco inicial. | [#43](https://github.com/edu3250/escritorio-canvas/pull/43) | Probar el gesto completo en Chrome. Decidir si se usa composición por capas y verificar Claude. |
| 20-11-2026 | Construir | Codex · GPT-6 | Se integraron la composición por capas y la barra de título propia. El panel sigue el tamaño nativo y el lienzo maximizado respeta la barra de tareas. Pasaron 65 pruebas. Con Chrome descartable se verificaron cuadros distintos en la miniatura cubierta, crecimiento del panel de 600 a 720 píxeles y devolución del acomodo original. | [#43](https://github.com/edu3250/escritorio-canvas/pull/43) | Prueba de uso con vídeo, Claude y paneo. Revisar en otra sesión. |
| 20-11-2026 | Construir | Codex · GPT-6 | Se ajustó el marco a la miniatura sin estirarla, se alineó la ventana real por el borde visible y se suavizó la activación conservando la cámara al 100 %. Pasaron 68 pruebas. Un ensayo con una ventana descartable comprobó proporción, alineación y devolución. Otro ensayo comprobó cinco estados intermedios renderizados durante la transición. | [#43](https://github.com/edu3250/escritorio-canvas/pull/43) | Repetir el caso de Chrome y Claude y revisar en otra sesión. |
| 20-11-2026 | Revisar | Codex · GPT-6 | Se pidieron cambios en b98e0b8 por la pérdida del tamaño al reactivar, la navegación al panel activo desde el minimapa y la reserva permanente del atajo. Compilación correcta y 68 pruebas aprobadas. | [Dictamen #43](https://github.com/edu3250/escritorio-canvas/pull/43#issuecomment-7047743055) | Corregir los tres fallos, completar la prueba de uso y volver a revisar. |
| 20-11-2026 | Construir | Codex · GPT-6 | Se corrigieron los tres hallazgos. El panel conserva el tamaño de trabajo entre activaciones y el minimapa recentra el panel activo. El atajo se reserva durante la edición. Pasaron 68 pruebas. Con una ventana descartable se comprobaron dos ciclos, la devolución y el recentrado con foco intacto. Ctrl+Alt+Espacio estaba ocupado en la sesión de prueba. Se verificó la liberación con una combinación alternativa en dos ventanas propias. | [#43](https://github.com/edu3250/escritorio-canvas/pull/43) | Repetir la prueba de uso con Chrome y Claude y revisar en otra sesión. |
| 20-11-2026 | Revisar | Codex · GPT-6 | En 4e83111 se verificaron las tres correcciones. Al cambiar la proporción, la tarjeta del mapa aún pierde la huella de trabajo. Compilación limpia y 68 pruebas aprobadas. | [Dictamen #43](https://github.com/edu3250/escritorio-canvas/pull/43#issuecomment-7048021638) | Corregir la geometría intermedia y completar la prueba de uso. |
| 20-11-2026 | Construir | Codex · GPT-6 | La tarjeta, el minimapa y los clics conservan la huella de trabajo tras volver al mapa. La miniatura DWM se encaja sin estirarla. Compilación limpia y 68 pruebas superadas. Un ensayo descartable verificó por separado el ancho (660×350) y el alto (500×480), dos activaciones y la devolución original. | [#43](https://github.com/edu3250/escritorio-canvas/pull/43) | Prueba de uso con Chrome y Claude y revisión en otra sesión. |
| 20-11-2026 | Revisar | Codex · GPT-6 | Dictamen favorable en 14be15e. Se verificó la geometría tras volver al mapa. Compilación limpia y 68 pruebas aprobadas. | [Dictamen #43](https://github.com/edu3250/escritorio-canvas/pull/43#issuecomment-7050052709) | Completar la prueba de uso con Chrome y Claude antes de cerrar el borrador. |
| 20-11-2026 | Construir | Codex · GPT-6 | El vídeo reprodujo la pérdida de miniatura con Claude y Edge inicialmente minimizados. Se separó el regreso al mapa de la devolución a Windows. Pasaron 69 pruebas y un ensayo con una ventana descartable verificó dos regresos con DWM disponible, minimización deliberada y devolución final. | [#43](https://github.com/edu3250/escritorio-canvas/pull/43) | Repetir el vídeo con Claude y Edge y revisar en otra sesión. |
| 20-11-2026 | Revisar | Codex · GPT-6 | El usuario confirmó «ya realice la prueba y funciona». En 2334cb7 pasaron 69 pruebas. Se detectó por inspección que una ventana siempre encima puede tapar el mapa al volver. | [Dictamen #43](https://github.com/edu3250/canvas/pull/43#issuecomment-7056389846) | Resolver ese regreso. Planear en otro PR la continuidad visual entre el mapa y el panel tomando Deska como referencia. |
| 20-11-2026 | Revisar | Codex · GPT-6 | Se registró la interacción aprobada para el PR posterior. Ensayo previsto con dos paneles vecinos y un tercero debajo: activar, panear al tercero, maximizar y volver al encuadre anterior. | [#43](https://github.com/edu3250/canvas/pull/43) | Construir corrige el caso siempre encima en #43. Tras revisarlo y fusionarlo, implementar la continuidad visual desde main. |
| 20-11-2026 | Construir | Codex · GPT-6 | Se corrigió el regreso al mapa desde una ventana siempre encima. Se eleva el lienzo durante el regreso y la vista en primer plano, sin cambiar el estilo de la ventana externa. Pasaron 69 pruebas. Un ensayo Win32 con ventanas descartables comprobó el cambio del destino del clic en la zona cubierta. | [#43](https://github.com/edu3250/escritorio-canvas/pull/43) | Validar el ciclo completo de foco y devolución y revisar en otra sesión. |
| 20-11-2026 | Revisar | Codex · GPT-6 | El usuario confirmó «ya realize la prueba interactiva pendiente y funciona». Revisión favorable del regreso desde ventanas siempre encima y de la integración con main. Compilación limpia y 89 pruebas aprobadas. | [#43](https://github.com/edu3250/canvas/pull/43) | Construir la continuidad visual acordada en un PR separado. |
| 20-11-2026 | Definir | Claude Code · claude-opus-5-5 | En la entrevista del hito 10 el usuario aceptó traspasar el paso 5 a ese hito («sí a todas»). Se tachó el paso 5, se añadió el paso 6 de continuidad visual y se ajustó la prueba final. | [#45](https://github.com/edu3250/canvas/pull/45) | Construir el paso 6 y cerrar el hito. |
