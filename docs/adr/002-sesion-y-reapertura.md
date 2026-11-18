# ADR 002 · Posiciones y reapertura selectiva

## Decisión acordada

Guardar el acomodo de las ventanas y permitir que el usuario elija cuáles se reabrirán al iniciar de nuevo el gestor. Un juego puede tener una posición recordada y permanecer cerrado hasta que el usuario lo inicie.

## Consecuencias para el diseño

Guardar una posición y autorizar el lanzamiento de una aplicación son operaciones independientes. Las reglas deben poder distinguir ventanas de una misma aplicación. Al recuperar la sesión habrá que reconocer las ventanas que ya abrió Windows o la aplicación para evitar duplicados.

## Por validar

Dos ventanas seleccionadas de VS Code deben volver a ocupar sus dos posiciones y tamaños. Basta con abrir Unity para ocupar su posición. La aplicación correspondiente decide qué proyecto, pestañas o documentos recupera. Falta validar qué aplicaciones permiten solicitar otra ventana y cómo reconocer una instancia que ya volvió a abrirse.
