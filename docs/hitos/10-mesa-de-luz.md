# Hito 10 · Mesa de luz

**Estado:** en curso

## Objetivo

Escritorio Canvas se ve y se siente como el sistema visual [Mesa de luz](../sistema-visual/mesa-de-luz.html). La orilla superior y la isla del mapa se retiran mientras se trabaja. Cada panel tiene un asa y una lengüeta de controles. El panel activo se enciende con la luz de su espacio y los demás quedan a media luz. Cada espacio conserva su luz y su imagen de fondo con velo al reiniciar.

**Fuera:** opción de orilla fija, fondos animados o de vídeo, temas claros, cambios de comportamiento que no sean visuales o de los controles del panel. La continuidad visual de la cámara sigue en el [hito 09](09-ventanas-reales.md).

## Decisiones de la entrevista

Entrevista del 20-11-2026 en dos rondas. El diario de uso estaba vacío. El usuario eligió este hito frente a cerrar antes el 09 u otra prioridad («Hito 10 · Mesa de luz (Recomendado)»). Los valores del sistema visual ya están en `main` ([PR #44](https://github.com/edu3250/canvas/pull/44)): `VisualTokens`, `Theme` y la fuente Geist.

Respuesta del usuario a la primera ronda: «sí a todas». A la segunda: «sí a todas».

| Pregunta | Respuesta del usuario | Consecuencia |
| --- | --- | --- |
| Q1 · Alcance | «sí» | Entran la orilla, los paneles con asa y lengüeta, la isla del mapa, la luz por espacio con el relevo de paneles, el fondo con velo, los iconos de Inicio y el diálogo «Añadir ventana». Primero lo visual, al final lo que toca ventanas o datos guardados. |
| Q2 · Hito 09 | «sí» | El paso 5 del hito 09 (✕, ⤢, «Quitar del lienzo» y aviso de minimizada) pasa a este hito y se construye directamente como lengüeta. El 09 se cierra con la continuidad visual. |
| Q3 · Minimizar | «sí» | Botón propio que pliega el panel a su asa con el aviso «minimizada»; un clic en el asa lo restaura. Plegar **no** minimiza la ventana en Windows, para no perder la miniatura DWM. |
| Q4 · Fondo y luz | «sí» | Cada espacio tiene su luz y su imagen. Se eligen desde un menú del espacio en la orilla y se guardan en `%LOCALAPPDATA%\Escritorio Canvas\`, con un ADR del formato. Luces por defecto: Glaciar en Programación, Lavanda en Inicio y Brasa en Juegos. Sin imagen, la mesa es noche con retícula de puntos. |
| Q5 · Media luz | «sí» | Los paneles en reposo se atenúan con la opacidad de la miniatura DWM, porque no se puede tapar con WPF. Si no se ve bien, se atenúan solo el asa y la lengüeta. |
| Q6 · Orilla | «sí» | La orilla se retira siempre y aparece al acercar el cursor al borde superior. Queda una franja fina como pista. Sin opción fija en este hito. La barra de tareas de Windows sigue visible. |
| Q7 · Isla del mapa | «sí» | Minimapa y zoom en una isla con el comportamiento actual (se oculta mientras se trabaja, reaparece en el borde inferior). Vidrio con `DWMWA_SYSTEMBACKDROP_TYPE` de Windows 11; si no está disponible, pizarra opaca. |
| Q8 · Menú del espacio | «sí» | Junto al nombre del espacio en la orilla: elegir luz entre las seis con vista previa al pasar el cursor, «Elegir imagen…» con el selector de archivos de Windows y «Quitar imagen». Se aplica al momento, sin botón de guardar. |
| Q9 · Imagen elegida | «sí» | Se copia a `%LOCALAPPDATA%\Escritorio Canvas\fondos\`, para que no se pierda si el original se mueve o se borra. Cubre la pantalla recortada desde el centro, sin deformarse. |
| Q10 · Asa | «sí» | Icono de la aplicación y título de su ventana, recortado con «…». El panel se arrastra solo por el asa; un clic en la miniatura activa la ventana. |
| Q11 · Lengüeta | «sí» | A la derecha: … (más), minimizar, maximizar y ✕. Visible al pasar el cursor y con el panel encendido. El menú … contiene «Quitar del lienzo». ✕ y ⤢ hacen lo acordado en P8a y P8b del hito 09. |
| Q12 · Botones de la app | «sí» | Minimizar y cerrar Escritorio Canvas van al extremo derecho de la orilla. Cerrar sigue devolviendo todas las ventanas. |
| Q13 · Replanteo | «sí» | Construir se detiene y consulta si el asa o la lengüeta no pueden ir pegadas a la miniatura sin taparla ni descolocarse al hacer zoom, o si el relevo entre espacios baja la fluidez de forma visible con seis paneles en pantalla. |
| Q14 · Prueba final | «sí» | La de «Lo doy por bueno cuando». |

## Riesgos

- **Miniaturas DWM.** Windows las dibuja encima del contenido WPF: el asa, la lengüeta y el halo van fuera de su rectángulo y la media luz depende de su opacidad (Q5).
- **Vidrio.** El fondo translúcido depende de Windows 11 y del ajuste de transparencia del usuario. Siempre hay alternativa opaca.
- **Zona sensible:** los pasos 4 y 5. El 4 cierra y restaura ventanas reales; el 5 crea un formato persistido nuevo y necesita el ADR 005 aprobado por el usuario antes de fusionarse. Los revisa otra sesión.
- **Ensayos interactivos.** Los pasos 4 y 5 se ensayan con ventanas y procesos descartables identificados por PID, y la prueba con ventanas reales solo tras el «adelante» del usuario. Las pruebas de ✕ usan solo trabajo descartable.
- **Movimiento reducido.** Con «Mostrar animaciones» desactivado en Windows, la orilla, el relevo y el cambio de luz se aplican sin animar.
- **Replanteo:** ver Q13.

## Pasos

- [x] 1. Base visual: Geist y la paleta en toda la app, noche con retícula, orilla retráctil con el selector de espacio y los botones de la app, iconos de Inicio y diálogo «Añadir ventana». Fuera de la zona sensible · `src/Canvas.Desktop`
- [ ] 2. Isla del mapa: minimapa y zoom juntos, con vidrio o pizarra opaca. Fuera de la zona sensible · `src/Canvas.Desktop/MinimapWindow.cs`, `MainWindow.cs`
- [ ] 3. Paneles: asa con icono y título y arrastre solo por ella, lengüeta visual, luz y halo del panel activo, media luz y relevo de paneles con el cambio de luz entre espacios. Fuera de la zona sensible · `src/Canvas.Desktop/WindowPanels.cs`, `SpaceView.cs`, `src/Canvas.Domain`, `tests/`
- [ ] 4. **Zona sensible.** Acciones de la lengüeta: ✕ con `WM_CLOSE`, ⤢, plegar sin minimizar en Windows, «Quitar del lienzo» y aviso «minimizada» cuando la ventana se minimiza con su botón nativo (antes paso 5 del hito 09) · `src/Canvas.Desktop`, `tests/`
- [ ] 5. **Zona sensible.** Menú del espacio con luz e imagen copiada, conservadas al reiniciar. ADR 005 del formato · `src/`, `tests/`, `docs/adr/`

Cada paso va en su propio PR. Los pasos 4 y 5 incluyen en el PR el ensayo con ventanas reales que hizo el usuario tras su «adelante».

## Lo doy por bueno cuando

Con VS Code, Chrome y Discord abiertos:

1. Pongo una imagen en Programación y otra en Juegos y cambio la luz de Juegos. Al reiniciar Escritorio Canvas siguen igual.
2. Cambio de espacio y veo el relevo con la nueva luz.
3. Arrastro un panel por su asa y lo enciendo con un clic. Los demás quedan a media luz.
4. Pliego Chrome y lo restauro, maximizo VS Code y vuelvo al mapa, cierro Discord con ✕ y quito otro panel con «Quitar del lienzo».
5. La orilla y la isla se retiran mientras trabajo y vuelven al acercar el cursor.
6. Cierro Escritorio Canvas desde la orilla y todas las ventanas vuelven a donde estaban.

## Bloqueos

Ninguno.

## Registro de sesiones

| Fecha | Modo | Herramienta y modelo | Qué se hizo | PR | Queda |
| --- | --- | --- | --- | --- | --- |
| 20-11-2026 | Definir | Claude Code · claude-opus-5-5 | Entrevista en dos rondas (Q1–Q14) a partir de la maqueta de Mesa de luz y los tokens de `main`. Redacción del hito y traspaso del paso 5 del hito 09. | [#45](https://github.com/edu3250/canvas/pull/45) | Fusionar el hito, cerrar el 09 y construir el paso 1. |
| 21-11-2026 | Construir | Codex · GPT-6 | Se aplicaron Geist, la paleta y la retícula, con orilla retráctil y un diálogo para añadir ventanas. Compilación limpia y 96 pruebas aprobadas. Fuera de pantalla se observó el encuadre de la orilla a 640 × 480 y el recorte de un título largo en el diálogo. | [#47](https://github.com/edu3250/canvas/pull/47) | Prueba de uso y autorización de fusión. Después, paso 2. |
| 21-11-2026 | Revisar | Codex · GPT-6 | Revisado 3889e26. La orilla retirada solo detecta el centro del borde y el selector confirma dobles clics secundarios. Compilación Release limpia y 96 pruebas aprobadas. | [#47](https://github.com/edu3250/canvas/pull/47) | Corregir ambos casos y completar la prueba de uso. |
| 21-11-2026 | Construir | Codex · GPT-6 | Se extendió la detección de la orilla a todo el borde superior y se limitó la confirmación por doble clic a la fila pulsada con el botón izquierdo. Compilación limpia y 96 pruebas aprobadas. Un ensayo WPF fuera de pantalla verificó la geometría en tres puntos y los tres botones del ratón con dos filas. La correspondencia Orilla y EdgeBar ya figura en el glosario. | [#47](https://github.com/edu3250/canvas/pull/47) | Prueba de uso y autorización de fusión. |
| 21-11-2026 | Revisar | Codex · GPT-6 | Dictamen favorable en eaa3d4d. Resueltos la detección del borde y el doble clic del selector. Compilación Release limpia y 96 pruebas aprobadas. | [#47](https://github.com/edu3250/canvas/pull/47) | Prueba de uso y autorización de fusión para Construir. |
