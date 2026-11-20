# ADR 004 · Registro de devolución de ventanas

**Estado:** propuesto. Pendiente de autorización.

## Decisión propuesta

Guardar los acomodos originales en un registro JSON versionado por usuario en `%LOCALAPPDATA%\Escritorio Canvas\devolucion-ventanas.json`. El lienzo y el comando «Devolver ventanas» usan el mismo registro y un archivo de bloqueo exclusivo. Este registro sirve para devolver ventanas que siguen abiertas tras un cierre del lienzo. El ADR 002 rige la futura reapertura de aplicaciones.

Ejemplo de una entrada:

```json
{
  "version": 1,
  "windows": [
    {
      "handle": 65822,
      "processId": 1234,
      "processStartedUtcTicks": 639307296000000000,
      "sessionId": 1,
      "marker": "c433c126-2377-49b7-8dd8-ff1708fb5609",
      "placement": {
        "flags": 0,
        "showCommand": 1,
        "minPosition": { "x": -1, "y": -1 },
        "maxPosition": { "x": -1, "y": -1 },
        "normalPosition": { "left": 120, "top": 100, "right": 820, "bottom": 660 }
      }
    }
  ]
}
```

- Antes del primer movimiento, se captura el acomodo con [GetWindowPlacement](https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getwindowplacement). Se comprueba la identidad de la ventana y se coloca en ella una propiedad Win32 con un identificador aleatorio. Se escribe el registro en un temporal de la misma carpeta, se vacía a disco y se sustituye el archivo anterior. Solo después de confirmar identidad, marca y escritura se habilita el movimiento. Los movimientos posteriores conservan la primera captura.
- Al devolver, se verifican el identificador de sesión, el PID, el inicio del proceso y la marca en el identificador de ventana justo antes de llamar a [SetWindowPlacement](https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-setwindowplacement). La marca distingue ventanas sucesivas incluso cuando Windows reutiliza un identificador dentro del mismo proceso.
- Tras la devolución confirmada se elimina la entrada con la misma escritura atómica y se retira la marca propia. Una ventana que ya desapareció o cuya identidad cambió se omite. Un fallo de devolución conserva la entrada para reintentar. Un registro ilegible o de versión desconocida se conserva y bloquea nuevos movimientos.
- El lienzo mantiene el bloqueo durante su ejecución. Recupera el registro al arrancar y al cerrar. La misma operación permitirá devolver un solo panel al quitarlo. «Devolver ventanas» puede ejecutarse con el lienzo cerrado. Si otra instancia tiene el bloqueo, el comando informa y sale.
- Las coordenadas se pasan entre las dos API de acomodo de Windows. Windows puede ajustarlas si cambió el área disponible de la pantalla.

## Alternativa

Un archivo por ventana, con la misma identidad y garantía de escritura. Simplifica guardar entradas individuales, pero dispersa el registro y complica comprobar su versión y examinar todas las devoluciones pendientes.
