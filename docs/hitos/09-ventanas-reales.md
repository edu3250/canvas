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
- [ ] 5. **Zona sensible.** Barra del panel (✕, ⤢, «Quitar del lienzo») y aviso de ventana minimizada · `src/Canvas.Desktop`

Cada paso va en su propio PR. Los pasos 2–5 incluyen en el PR el ensayo con ventanas reales que hizo el usuario tras su «adelante».

## Lo doy por bueno cuando

Con VS Code, Chrome y Discord abiertos:

1. Añado VS Code y Chrome a Programación y Discord a Juegos.
2. Escribo en VS Code desde su panel, vuelvo al mapa con el minimapa y también con Ctrl+Alt+Espacio, y maximizo Chrome.
3. Cierro una ventana con ✕ y quito otra del lienzo.
4. Cierro Escritorio Canvas y todas las ventanas que quedan vuelven a donde estaban antes.

## Bloqueos

Ninguno.

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
