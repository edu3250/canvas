# f03-t01 · Leer y aplicar la colocación de una ventana

**Rama** `f03-t01-colocacion`. Depende del [plan](../encargo.md) fusionado. Ensayo interactivo: no.

**Único archivo permitido:** crear `prototipos/panel-nativo/WindowPlacementApi.cs`. Definir una clase estática interna WindowPlacementApi. Contrato: `Capture(nint hwnd)` devuelve la estructura Placement completa por valor; `Restore(nint hwnd, Placement saved)`, `Normalize(nint hwnd)` y `Place(nint hwnd, Rectangle screenBounds)` devuelven void. Exponer en Placement el comando de presentación y el rectángulo normal para su lectura por t02.

Usar la disposición de WINDOWPLACEMENT de Win32 (length, flags, showCmd, dos POINT y RECT). Al capturar e inmediatamente antes de restaurar, inicializar length con Marshal.SizeOf. GetWindowPlacement y SetWindowPlacement deben comprobar BOOL y propagar el error Win32 con operación y código. Normalize usa ShowWindow(SW_RESTORE); su retorno indica visibilidad previa. Place llama SetWindowPos con SWP_NOZORDER | SWP_NOACTIVATE. Usar coordenadas de pantalla para Place y reenviar rcNormalPosition intacto en Restore. La identidad se valida en el llamador.

**Verificación y aceptación:** ejecutar `dotnet build prototipos/panel-nativo/PanelNativo.csproj`. Adjuntar salida y comprobar en revisión los campos, banderas y tratamiento de retornos. La eficacia en escritorio se mide en t03. Ante ambigüedad, cambio de contrato o tres intentos fallidos por la misma causa, seguir el [procedimiento de bloqueo](../../../plantillas/informe-bloqueo.md).
