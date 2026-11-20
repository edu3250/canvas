# Hito 08 · Iconos reales en Inicio

**Estado:** en curso

## Objetivo

Cada icono de Inicio es un elemento real de la carpeta de su espacio: se ve igual que en el Explorador de Windows y, con doble clic, abre lo mismo que abriría el Explorador.

**Fuera:** renombrar, borrar o crear elementos desde el lienzo; el menú contextual de Windows; arrastrar entre el lienzo y el Explorador; mostrar ventanas como paneles (otro hito).

## Decisiones de la entrevista

Entrevista del 19-11-2026. Las preguntas completas y las respuestas recomendadas se resumen en la columna «Consecuencia».

| Pregunta | Respuesta del usuario | Consecuencia |
| --- | --- | --- |
| P1 · Origen de los elementos | «carpeta real por espacio» | Inicio muestra el contenido de una carpeta real por espacio (Programación, Juegos). Las aplicaciones entran como accesos directos. Aplica el [ADR 001](../adr/001-archivos-y-accesos.md). |
| P2 · Qué hace un icono | «si» | Doble clic abre con la acción predeterminada de Windows. Clic selecciona y arrastre mueve, como en la fase 7. |
| P3 · Qué cuenta como el mismo icono | «si» | Referencia: vista «Iconos grandes» del Explorador. Miniaturas en imágenes y vídeos, flecha en los accesos directos y sin indicadores de sincronización de OneDrive. |
| P4 · Qué queda fuera | «si» | Lo listado en «Fuera». |
| P5 · Posiciones al reiniciar | «si» | Las posiciones se guardan en `%LOCALAPPDATA%`. Es zona sensible y necesita un ADR del formato. |
| P6 · Comprobación | «si» | Juego de prueba fijo comparado lado a lado con el Explorador (ver «Lo doy por bueno cuando»). |
| P7 · Ubicación de las carpetas | «si a todos» | `%USERPROFILE%\Escritorio Canvas\Programación` y `%USERPROFILE%\Escritorio Canvas\Juegos`. Cierra la carpeta raíz pendiente del ADR 001. |
| P8 · Primer arranque | «si a todos» | Se crean las carpetas vacías y se retiran los cuatro iconos de muestra. La barra gana el botón «Abrir carpeta del espacio», que la abre en el Explorador. |
| P9 · Cambios con la aplicación abierta | «si a todos» | El lienzo refleja altas, bajas y renombrados en ≤ 1 s. Un elemento nuevo va a la primera casilla libre, uno borrado libera la suya y uno renombrado conserva su posición. |
| P10 · Capacidad de Inicio | «si a todos» | 4 columnas y filas sin límite hacia abajo. El centro del lienzo sigue despejado. |
| P11 · Ocultos y de sistema | «si a todos» | Nunca se muestran, aunque el Explorador tenga activado «mostrar ocultos». |
| P12 · Nombre bajo el icono | «si a todos» | El mismo nombre visible que el Explorador (sin `.lnk`, con las extensiones según su configuración), cortado con «…» y completo al pasar el ratón. |
| P13 · Accesos rotos | «si a todo lo demas» | Mismo icono que el Explorador. Al abrir, Windows muestra su propio aviso; no se añaden avisos propios. |
| P14 · Steam y Tienda | «si a todo lo demas» | El juego de prueba incluye un acceso `.url` de un juego de Steam y una aplicación de la Microsoft Store. |
| P15 · Abrir varios | «si a todo lo demas» | Doble clic abre solo el icono bajo el puntero. Intro abre todos los seleccionados. |
| P16 · Archivo de posiciones dañado o ausente | «si a todo lo demas» | Los iconos se reacomodan automáticamente y la aplicación sigue funcionando. Nunca se tocan los archivos reales. |
| P17 · Fusión en la zona sensible | «p17. no dejalo como esta» | Se mantiene `AGENTS.md`: Revisar fusiona la zona sensible tras su dictamen, sin esperar la prueba del usuario. |
| P18 · Condición de replanteo | «si a todo lo demas» | Ver «Riesgos». |
| Confirmación del resumen | «adelante» | Se escribe este hito. |

## Riesgos

- **Fidelidad del icono.** Usar la API pública del shell (`IShellItemImageFactory` y el nombre visible del elemento). Si algún tipo del juego de prueba no puede verse igual que en el Explorador con APIs públicas, Construir se detiene y presenta alternativas. No se usan interfaces privadas sin autorización.
- **Archivos del usuario.** El lienzo solo lee la carpeta del espacio y abre elementos. Nunca mueve, renombra ni borra archivos.
- **Zona sensible:** solo el paso 5 (guardar posiciones). Va en un PR propio que revisa otra sesión. No hace falta ningún ensayo interactivo: la aplicación no toca ventanas de otras aplicaciones.
- **Rendimiento.** Las miniaturas de muchas imágenes pueden tardar. Cargarlas sin bloquear la interfaz.

## Pasos

- [x] 1. Carpetas por espacio y lectura de su contenido: crear las carpetas si faltan, listar elementos sin ocultos ni de sistema, retirar las muestras y añadir el botón «Abrir carpeta del espacio». Lógica en el dominio con pruebas xUnit · `src/Canvas.Domain`, `src/Canvas.Desktop`, `tests/`
- [x] 2. Iconos, miniaturas y nombres tomados del shell, nítidos al ampliar el lienzo, con el nombre completo al pasar el ratón · `src/Canvas.Desktop`
- [x] 3. Abrir con doble clic (un elemento) e Intro (los seleccionados) · `src/Canvas.Desktop`
- [x] 4. Actualización en vivo (≤ 1 s, el renombrado conserva la posición) y cuadrícula de 4 columnas que crece hacia abajo · `src/Canvas.Domain/HomeGrid.cs`, `src/Canvas.Desktop`, `tests/`
- [x] 5. **Zona sensible.** Guardar las posiciones por espacio en `%LOCALAPPDATA%`, con reacomodo automático si el archivo falta o está dañado, y un ADR con el formato · `src/`, `tests/`, `docs/adr/003-*.md`

Los pasos 1–4 pueden ir en uno o varios PR que el usuario prueba antes de fusionar. El paso 5 va en un PR aparte.

## Lo doy por bueno cuando

Abro la carpeta del espacio en el Explorador junto al lienzo y estos elementos se ven igual en ambos lados y abren lo mismo con doble clic:

- un acceso a VS Code;
- un acceso con icono personalizado;
- una carpeta normal y una carpeta con icono personalizado;
- una imagen (con miniatura), un PDF y un `.txt`;
- un acceso `.url` de un juego de Steam;
- una aplicación de la Microsoft Store.

Además: si añado, borro o renombro algo desde el Explorador, el lienzo lo refleja solo; y al cerrar y volver a abrir la aplicación, los iconos siguen donde los dejé.

## Bloqueos

Ninguno.

## Registro de sesiones

| Fecha | Modo | Herramienta y modelo | Qué se hizo | PR | Queda |
| --- | --- | --- | --- | --- | --- |
| 19-11-2026 | Definir | Claude Code · claude-opus-5-5 | Entrevista en tres rondas (P1–P18) y redacción del hito. Raíz de carpetas anotada en el ADR 001. | [#34](https://github.com/edu3250/escritorio-canvas/pull/34) | Construir los pasos 1–4 |
| 19-11-2026 | Construir | Codex · GPT-6 | Pasos 1–4. Compilación, 27 pruebas y comprobación del shell y el seguimiento de renombrados con archivos temporales en Windows. | [#35](https://github.com/edu3250/escritorio-canvas/pull/35) | Comparación visual y apertura con el juego del hito, autorización de fusión y paso 5 en su propio PR. |
| 20-11-2026 | Construir | Codex · GPT-6 | El usuario verificó los iconos y aprobó el PR. Se añadió el script de arranque y un acceso directo local excluido de Git. | [#35](https://github.com/edu3250/escritorio-canvas/pull/35) | Paso 5 en su propio PR. |
| 20-11-2026 | Construir | Claude Code · claude-opus-5-5 | Paso 5: `HomeLayout` guarda las posiciones por espacio en `%LOCALAPPDATA%\Escritorio Canvas\posiciones.json` con escritura atómica; reacomodo automático si falta o está dañado. [ADR 003](../adr/003-posiciones-de-inicio.md). 38 pruebas, cobertura del dominio 97,4 %. | ver PR de `h08-iconos-reales-posiciones` | Revisar en otra sesión. Después, el usuario cierra el hito con su prueba de reinicio. |
