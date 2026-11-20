# ADR 003 · Posiciones de los iconos de Inicio

Estado acordado en la entrevista del [hito 08](../hitos/08-iconos-reales.md) (P5 y P16), el 19-11-2026.

## Decisión

Las posiciones de los iconos de Inicio se guardan en un único archivo JSON por usuario:

```
%LOCALAPPDATA%\Escritorio Canvas\posiciones.json
```

```json
{
  "version": 1,
  "spaces": {
    "Programación": {
      "VS Code.lnk": { "x": 120, "y": 176 }
    }
  }
}
```

- Cada icono se identifica por el **nombre de su archivo** dentro de la carpeta del espacio, sin la ruta, y sin distinguir mayúsculas. Así el archivo sobrevive si cambia la ruta del perfil.
- Las coordenadas son casillas de la cuadrícula de Inicio (`HomeGrid`): `x` ∈ {24, 120, 216, 312} e `y` = 80 + 96·k.
- El archivo se escribe en un temporal (`posiciones.json.tmp`) que luego sustituye al anterior. Un guardado interrumpido deja intacto el archivo previo.
- Se guarda después de mover iconos y después de cada cambio en la carpeta de un espacio. Solo se reemplaza la entrada de un espacio cuya carpeta ya se leyó en esa ejecución; los demás espacios conservan lo guardado.

## Archivo ausente o dañado

- Si el archivo no existe, los iconos se acomodan en las primeras casillas libres.
- Solo la envoltura puede invalidar el archivo entero. Si no se puede leer, no es JSON válido, `version` no es el número 1 o `spaces` no es un objeto, se trata como vacío y se registra un aviso. El siguiente guardado lo reemplaza.
- Cada espacio y cada icono se validan por separado. Se descartan las entradas inválidas: con un nombre ilegible (por ejemplo un escape Unicode incompleto), con un valor de tipo incorrecto, sin `x` o `y`, fuera de la cuadrícula, desalineadas, repetidas en la misma casilla o con un nombre que contiene una ruta. El resto se conserva.
- Al colocar los iconos, una posición guardada solo se usa si la casilla sigue libre. Si no, el icono va a la primera casilla libre.

Este archivo solo contiene posiciones. Perderlo o dañarlo **nunca** modifica las carpetas de los espacios ni sus archivos.

## Consecuencias

- Un archivo renombrado con la aplicación cerrada se trata como uno nuevo y pierde su posición. Con la aplicación abierta, la conserva (hito 08, P9).
- Un cambio incompatible del formato exige subir `version` y otro ADR.
- La lógica vive en `Canvas.Domain/HomeLayout.cs`, con pruebas en `HomeLayoutTests`.
