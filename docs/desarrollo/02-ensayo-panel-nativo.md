# Fase 2 · Ensayo de un panel nativo con dos ventanas

## Decisión

Determinar si merece la pena construir el canvas con una vista general y activar una ventana nativa en la posición del panel. El [informe estático](../inspeccion/informe-cwc.md) verificó que Canvas Window Composer centra la ventana y oculta su mapa. El ensayo evalúa la alternativa recomendada en ese informe mediante un programa de prueba propio. La comparación con CWC se apoya por ahora en su código.

## Encargo para el ejecutor

1. Registrar versión de Windows, entorno de compilación y disponibilidad de una sesión interactiva. Usar dos ventanas creadas para la prueba. Para VS Code, abrir una instancia con perfil temporal y un archivo de prueba propio que tenga una línea larga y el ajuste de línea activado. Asegurar la identidad de la ventana antes de cambiar su posición.
2. Preparar un host mínimo de un solo monitor con dos paneles. Usar una vista previa del candidato en el primer panel, activar la ventana real en su rectángulo y conservar la representación del vecino. El caso de control puede usar una ventana de texto de prueba en otro proceso. Documentar las diferencias entre este control y la prueba con VS Code.
3. Ensayar dos anchos con la misma altura. Tomar 1475 y 492 píxeles como referencia cuando quepan. Registrar unidades, DPI, rectángulo solicitado y obtenido, ventana en primer plano y visibilidad efectiva del vecino. Detectar si la aplicación ajusta su tamaño mínimo.
4. En una sesión interactiva, activar el panel con un clic, escribir una marca en el documento temporal, abrir y cancelar un diálogo de archivo, y escribir una segunda marca. Comprobar ambas marcas en el editor y que el vecino sigue visible. Distinguir las observaciones de un usuario o una herramienta de UI de llamadas programáticas que solo ejercitan la geometría.
5. Al salir, comprobar que las ventanas temporales se cierran o recuperan su acomodo y que los intentos fallidos dejan limpia la sesión. Entregar instrucciones reproducibles, código mínimo y un informe con evidencia, limitaciones y recomendación. Conservar en Git solo archivos apropiados para el repositorio y datos de prueba.

## Criterios de salida

| Afirmación | Evidencia necesaria |
| --- | --- |
| Un clic lleva de la vista previa al editor | Identidad de la ventana activa antes y después, escritura comprobada dentro del documento temporal. |
| Se puede editar a dos tamaños | Geometría obtenida y observación del ajuste de línea conservando el tamaño de letra. |
| Un diálogo devuelve el control al editor | Resultado de abrir y cancelar el diálogo y de escribir la segunda marca. |
| El vecino permanece visible durante la edición | Observación del segundo panel o comprobación de que su contenido permanece visible en el escritorio. |
| La prueba se puede repetir | Comandos, entorno, resultado de compilación y alcance de la limpieza. |

Cada caso termina como verificado, fallido o pendiente con su motivo. Si una limitación de la sesión impide verificar entrada real, ejecutar las comprobaciones independientes disponibles y dejar listo el recorrido interactivo. Elegir la siguiente fase después de revisar esta evidencia. La restauración tras reinicio y la prueba de 15 ventanas permanecen en el mapa posterior.
