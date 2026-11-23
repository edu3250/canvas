# Ensayo de panel nativo en Windows

El host muestra dos miniaturas DWM. Un clic en la primera coloca el editor real sobre su panel y le solicita el foco. El vecino verde proviene de otra ventana de prueba y conserva su sitio. **Ancho** y **Estrecho** vuelven a la vista previa. **Salir** cierra la sesión.

## Compilar y reproducir

Usar Windows con escritorio interactivo, .NET SDK 10 con Windows Desktop y, para el segundo caso, VS Code. En PowerShell, desde la raíz del repositorio, ejecutar por separado las dos pruebas. Cada una necesita el primer plano mientras dura la interacción.

```powershell
Set-Location prototipos/panel-nativo
dotnet build PanelNativo.csproj -p:NuGetAudit=false
.\bin\Debug\net10.0-windows\PanelNativo.exe control --auto
.\bin\Debug\net10.0-windows\PanelNativo.exe code --auto
```

El modo `code` busca la instalación por usuario bajo `%LOCALAPPDATA%\Programs\Microsoft VS Code`. Si la instalación está en otra carpeta, definir `$env:PANEL_CODE_EXE` con la ruta completa de `Code.exe` antes de ejecutar. Cada intento crea su propio archivo de texto, perfil de usuario y carpeta de extensiones. El control abre un editor WinForms en otro proceso. El host identifica por PID y nombre de archivo las ventanas que crea.

La automatización recorre los dos tamaños, pulsa la miniatura y escribe al final del documento. Guarda con Ctrl+S, abre con Ctrl+O, cancela, escribe la segunda marca y guarda de nuevo. Comprueba el foco antes de cada envío y verifica el contenido guardado. El botón Cancelar recibe un clic si su centro está expuesto. Si el botón está cubierto, intenta Escape únicamente si el diálogo sigue en primer plano. Un destino ajeno interrumpe el recorrido. El código de salida es 0 al completarse, 1 para fallo y 2 para argumentos incorrectos. Las discrepancias de tamaño se pueden ver comparando los rectángulos del registro.

La carpeta local `evidencia-local/<caso>-<id>/` contiene el registro, la copia final del documento automático y las dos imágenes del editor cuando el recorrido llega a capturarlas. Las imágenes muestran la superficie del HWND del editor. La visibilidad en el escritorio se comprueba aparte tomando el color de un punto del vecino. Las capturas pueden contener la ruta del perfil de Windows. La carpeta de evidencia y los resultados de compilación se excluyen de Git.

## Ensayo de salida y retorno

Usar Windows con escritorio interactivo y un área de trabajo primaria de al menos 1200 × 800 px. El ensayo puede tomar el primer plano. Ejecutar desde la raíz del repositorio.

```powershell
dotnet build prototipos/panel-nativo/PanelNativo.csproj
Set-Location prototipos/panel-nativo
.\bin\Debug\net10.0-windows\PanelNativo.exe layout
$LASTEXITCODE
```

El modo `layout` mueve dos ventanas descartables del prototipo y alterna dos veces entre sus acomodos de escritorio y canvas. El registro local `evidencia-local/control-<id>/sesion.txt` contiene 12 lecturas en seis etapas y las comprobaciones de limpieza. El código 0 indica que el recorrido y la limpieza se verificaron. El código 1 indica un fallo y el 2 indica argumentos inválidos. [Mediciones del ensayo](../../docs/inspeccion/f03-evidencia-recorrido.md)

## Recorrido manual

1. Ejecutar uno de los casos sin `--auto`. Hacer clic en la vista previa del editor. La franja superior queda disponible para volver al host.
2. Pulsar Ctrl+Fin, escribir `ANTES1` y guardar. Abrir el diálogo con Ctrl+O, cancelarlo, escribir `DESPUES1` y guardar. Observar el vecino verde en cada paso.
3. Pulsar **Estrecho** y hacer clic otra vez en la vista previa. Comprobar cuántos renglones ocupa la primera línea y repetir con `ANTES2` y `DESPUES2`. Las marcas deben quedar en la segunda línea del editor.
4. Pulsar **Salir**. El registro consigna el cierre de cada proceso iniciado y la eliminación del directorio temporal. En modo manual conviene leer el documento antes de salir. El registro y las mediciones manuales se pueden contrastar con el [informe del ensayo](../../docs/inspeccion/ensayo-panel-nativo.md).

## Dos acomodos con VS Code

**Estado: ensayo verificado por Ejecución; revisión independiente completada; véase el cierre de fase.** Requiere aviso y «adelante» del usuario (ver «Seguridad en Windows» en AGENTS.md). Abre dos ventanas descartables de VS Code con documentos, perfiles y extensiones separados; puede tomar el primer plano. No usar ventanas existentes.

Desde la raíz, después del permiso:

```powershell
dotnet build prototipos/panel-nativo/PanelNativo.csproj
Set-Location prototipos/panel-nativo
.\bin\Debug\net10.0-windows\PanelNativo.exe layout-code
$LASTEXITCODE
```

Conserva la resolución de Code.exe y PANEL_CODE_EXE descritas arriba. Requiere área primaria mínima de 1200 × 800 px. Ejecuta el mismo oráculo de seis etapas de layout: A normal y B maximizada en D0; dos retornos y restauraciones finales. No añadir --auto. Registro: evidencia-local/layout-code-<id>/sesion.txt, excluido de Git. Código 0: recorrido y limpieza verificados; 1: fallo; 2: argumentos inválidos.

Contrastar las 12 lecturas, estado exacto y tolerancia de 1 px por borde, sin alterar tiempos ni dimensiones si VS Code los rechaza. Registrar versiones, DPI, área, salida y limpieza. [Evidencia y pendientes](../../docs/inspeccion/f04-evidencia-recorrido.md).

## Ensayo de ventana hija del hito 11

Compilar con `dotnet build prototipos/panel-nativo/PanelNativo.csproj -c Release`. Desde la carpeta de salida, `.\PanelNativo.exe embed-check` comprueba el cambio de padre, el estilo de ventana hija, el tamaño del panel y la devolución del estilo y del padre. Usa ventanas ocultas del mismo proceso y devuelve 0 al pasar.

El ensayo visible requiere aviso y «adelante». Ejecutar por separado `.\PanelNativo.exe embed control`, `.\PanelNativo.exe embed chrome` y `.\PanelNativo.exe embed code`. El host abre a pantalla completa y lanza el candidato automáticamente. Observar dónde aparece por primera vez la ventana y hacer un clic en el contenido para escribir. En Chrome, **Abrir segunda ventana** lanza otra ventana en el mismo perfil descartable. En el editor de control o en VS Code, abrir y cancelar un diálogo. **Terminar ensayo** separa las ventanas y cierra los procesos del ensayo. Se admite `PANEL_CHROME_EXE` o `PANEL_CODE_EXE` para instalaciones personalizadas.

Se comprueban el PID del proceso lanzado y el título del documento único antes del alojamiento. El registro local recoge los tiempos desde el lanzamiento hasta detectar y alojar la ventana. Para valorar el destello hay que observar la pantalla: esos tiempos por sí solos no prueban qué fotograma vio el usuario. Comparar con el modo `code` de superposición anterior. La captación de ventanas ajenas al ensayo, los controles unificados, el foco en segundo plano y el zoom sobre aplicaciones reales quedan para etapas posteriores.

Para comparar la escritura con el mismo Chrome descartable, ejecutar `PanelNativo.exe embed chrome --check-input` y después `PanelNativo.exe overlay chrome --check-input`. Ambos ensayos toman el primer plano. Hacen un clic en el campo central y comprueban el foco antes de cada tecla. El éxito exige que la página confirme el texto exacto `canvas42` y que se complete la limpieza. Para observar la primera aparición y escribir a mano, omitir `--check-input`. La comparación superpuesta prueba una sola ventana.

### Ensayo de interacción a escala

Desde la carpeta del prototipo, compilar en Release y ejecutar el binario con `scale-check` para comprobar la proyección del puntero sin abrir ventanas. Con autorización para el ensayo en primer plano, usar `scale chrome`. Abre un perfil y una página descartables. La miniatura proyecta el área cliente, conserva el tamaño original de Chrome y recorta el centro si la vista supera el monitor. La prueba envía únicamente un clic izquierdo traducido a esa ventana y deja que Chrome reciba el teclado directamente.

En el campo central, escribir «uno tres», colocar el cursor antes de «tres», pulsar el control 30 % y escribir «dos » sin otro clic. Debe quedar «uno dos tres». A 300 %, hacer clic entre dos letras visibles y escribir ahí. Verificar el cursor y los saltos de línea. Terminar con el botón superior. Cambiar a una aplicación externa termina el ensayo. Los controles de porcentaje permiten probar el foco y la escala por separado de la rueda. Este ensayo no valida arrastre, menús, diálogos ni otras aplicaciones.