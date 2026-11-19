# f03-t03 · Reproducir el recorrido en Windows

**Rama:** f03-plan, PR #2. Depende de t02 verificada. Ensayo interactivo con el aviso y el «adelante» de AGENTS.md §7.

**Archivos permitidos:** modificar `prototipos/panel-nativo/README.md` con el comando y crear `docs/inspeccion/f03-evidencia-recorrido.md` con los resultados. La síntesis y el cierre de fase corresponden a Revisión.

El oráculo es la [tabla de mediciones de t02](02-recorrido.md#puntos-de-medición-dentro-de-layouttrialexecute). Recoger las 12 lecturas esperadas (dos ventanas en seis etapas) desde el archivo local `evidencia-local/control-<id>/sesion.txt` creado por Run. Si el ensayo se interrumpe, identificar la primera lectura fallida y marcar las posteriores que no se ejecutaron como pendientes. La falta de filas impide declarar verificado el recorrido completo.

1. Desde la raíz, ejecutar `dotnet build prototipos/panel-nativo/PanelNativo.csproj`. Registrar Windows, DPI, área de trabajo y SDK. Antes de tomar el primer plano, avisar y esperar el «adelante» del usuario en esa sesión. Sin confirmación, registrar el ensayo como pendiente.
2. Desde `prototipos/panel-nativo`, ejecutar `.\bin\Debug\net10.0-windows\PanelNativo.exe layout` y registrar el código de salida. Contrastar el registro con las cuatro filas de aceptación del encargo. Incluir una tabla por transición con estado y rectángulos de las dos ventanas, más la comprobación de limpieza.
3. Documentar cualquier fallo reproducible con paso, esperado y obtenido. Mantener registros crudos y capturas fuera de Git; publicar solo mediciones sin rutas personales. Indicar que son dos ventanas descartables y transiciones programáticas.

**Aceptación:** comandos reproducibles y un estado verificado, fallido o pendiente por criterio respaldado por evidencia. Registrar herramienta, modelo, intentos y esfuerzo según la plantilla del PR. Si falta autorización o entorno, terminar las comprobaciones no interactivas; ante ambigüedad, necesidad de cambiar código o tres fallos repetidos, devolver el caso con la [plantilla de bloqueo](../../../plantillas/informe-bloqueo.md).
