# Ensayo de interacción a escala

En Windows, con Canvas abierto, ejecuta desde la raíz del repositorio:

~~~powershell
.\tests\manual\zoom-panel\Test-PanelZoom.ps1
~~~

El script abre Chrome con un perfil temporal y una página local. Añade «Ensayo de zoom de Canvas» desde el selector de ventanas. Sigue las instrucciones de la página. Al terminar, devuelve la ventana desde Canvas y ciérrala para que el script elimine el perfil.

La prueba registra los cambios en el área de contenido de Chrome (píxeles CSS y escala), la reproducción de un flujo de vídeo local, la escritura y la selección. El cambio de zoom en Canvas y cualquier destello de la ventana nativa se comprueban visualmente. Una segunda pulsación en Reproducir o un segundo clic en el buscador requieren repetir la prueba desde Iniciar.
Para diagnosticar el parpadeo, inicia Canvas con CANVAS_PROJECTION_TRACE apuntando a un archivo CSV nuevo. Cierra la instancia después del ensayo para completar la traza de cámara, foco, rectángulo nativo y ventana visible en su centro.

Para probar el acoplamiento, coloca al lado un editor con un documento de prueba. Anota el tamaño de Chrome, reproduce el vídeo y escribe en el editor. Cambia el zoom con Ctrl + rueda primero sobre el editor y luego sobre Chrome. Comprueba que ambas ventanas conservan sus dimensiones, que el vídeo sigue avanzando y que el primer clic en Pausa funciona sin destello. Repite después de desplazar el lienzo.
