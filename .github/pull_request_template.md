<!-- Un PR por rama (hNN-slug). Para un bloqueo, pasar a borrador y anotarlo en «Bloqueos» del hito. -->

**Hito y pasos:** `docs/hitos/NN-slug.md` · pasos …

## Qué cambia

Una o dos frases.

## Cómo probarlo

Pasos para que el usuario lo pruebe. Para Win32: qué se probó, con qué ventanas y qué se observó.

- [ ] `dotnet build EscritorioCanvas.sln -c Release`
- [ ] `dotnet test EscritorioCanvas.sln -c Release`

## Zona sensible

- [ ] No toca la zona sensible: fusiona Construir tras la prueba y la autorización del usuario.
- [ ] Toca la zona sensible: fusiona Revisar tras su dictamen.

## Comprobaciones

- [ ] Registro del hito y `docs/estado.md` actualizados
- [ ] Sin dependencias nuevas, o autorizadas por el usuario
- [ ] Sin `bin/`, `obj/`, `evidencia-local/`, rutas del perfil ni secretos
- [ ] Ensayos interactivos solo tras el «adelante» del usuario

Herramienta y modelo:
