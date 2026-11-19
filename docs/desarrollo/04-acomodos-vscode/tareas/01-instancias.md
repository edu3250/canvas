# f04-t01 · Crear dos ventanas aisladas de VS Code

**Rama:** f04-plan. **Depende de:** plan publicado. **Ensayo interactivo:** no.

## Archivos y contexto

Leer el [encargo](../encargo.md), `Program.cs`, `LayoutTrial.cs` y `WindowPlacementApi.cs` del [prototipo](../../../../prototipos/panel-nativo/README.md). Se permite modificar únicamente `prototipos/panel-nativo/Program.cs` en esta tarea. No cambiar el oráculo de geometría.

## Contrato

Aceptar únicamente `layout-code` como nuevo argumento sin opciones. Crear `Run("layout-code")`, llamar `Start()`, después `LayoutTrial.Execute(run)`, y conservar Dispose y comprobación de Clean. `layout` sigue usando `Run("control")`.

Para el nuevo modo, `Candidate`/`CandidatePid` identifican A y `Neighbor`/`NeighborPid` identifican B. Ambas son VS Code; no iniciar la ventana verde. Cada una usa documento con nombre único, `--user-data-dir` y `--extensions-dir` propios bajo el directorio temporal de Run. Conservar las opciones de aislamiento, configuración y resolución de ejecutable existentes del modo `code`. Los dos documentos pueden tener el mismo contenido, pero no el mismo nombre. Registrar versión de VS Code y asociación A/B, PID y documento sin publicar rutas personales.

Generalizar internamente `WaitWindow` y `IsOurs` para asociar cada PID iniciado al nombre de documento correspondiente. Conservar la firma `IsOurs(nint hwnd, int pid)` consumida por Host y LayoutTrial. Exigir PID registrado por esta ejecución y documento esperado en el título; un HWND de otro PID nunca es admisible aunque coincida el título. Espera de arranque máxima existente: 35 s. No buscar por ventana activa ni tomar instancias preexistentes. Preservar `Document` usado por Host para los modos anteriores.

Registrar cada proceso inmediatamente al crearlo para limpiar también arranques parciales. Cerrar únicamente procesos iniciados con su perfil/documento propio. Antes de solicitar cierre de una ventana, verificar su identidad; no actuar sobre una ventana sustituta. Si hay que terminar un proceso propio por timeout, registrar el hecho y verificar su salida. Conservar limpieza del temporal con reintentos y código 1 si falla. No lanzar instancias durante esta tarea.

## Verificación

`dotnet build prototipos/panel-nativo/PanelNativo.csproj` debe terminar sin errores. Comprobar que `layout-code --auto` y `layout --auto` devuelven 2 sin iniciar ventanas ni crear temporales. Revisar que los argumentos históricos conservan sus rutas y que las dos instancias nuevas tienen recursos distintos. No se exige CI ni cobertura al prototipo.

Ante ambigüedad, necesidad de cambiar otro archivo, otro contrato o tres fallos por la misma causa, devolver el caso a Planificación con la [plantilla de bloqueo](../../../plantillas/informe-bloqueo.md).
