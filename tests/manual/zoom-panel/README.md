# Ensayo de interacción a escala

En Windows, con Canvas abierto, ejecuta desde la raíz del repositorio:

~~~powershell
.\tests\manual\zoom-panel\Test-PanelZoom.ps1
~~~

El script abre Chrome con un perfil temporal y una página local. Añade «Ensayo de zoom de Canvas» desde el selector de ventanas. Sigue las instrucciones de la página. Al terminar, devuelve la ventana desde Canvas y ciérrala para que el script elimine el perfil.

La prueba registra los cambios en el área de contenido de Chrome (píxeles CSS y escala), la reproducción de un flujo de vídeo local, la escritura y la selección. El cambio de zoom en Canvas y cualquier destello de la ventana nativa se comprueban visualmente. Una segunda pulsación en Reproducir o un segundo clic en el buscador requieren repetir la prueba desde Iniciar.
