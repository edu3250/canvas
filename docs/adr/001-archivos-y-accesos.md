# ADR 001 · Aplicaciones y contenido de cada espacio

Estado acordado en la segunda intervención del usuario.

## Decisión

Representar las aplicaciones instaladas mediante accesos directos. Un archivo o una carpeta que el usuario guarda en un espacio pertenece a ese espacio. Cuando el usuario traslada esa carpeta a otro espacio, sus documentos la acompañan.

## Consecuencias

El modelo debe distinguir el destino de un acceso de la ubicación de un documento. Cambiar de espacio es una operación de navegación. Mover una carpeta entre espacios es una operación sobre archivos y puede cambiar la ruta que usan otras aplicaciones.

## Guardado y proyectos externos

Cada espacio tendrá una carpeta con nombre y ruta accesible desde el Explorador. Cuando el usuario seleccione esa ruta en el diálogo habitual de guardado de una aplicación, el escritorio del espacio mostrará el archivo. «Crear acceso aquí» permitirá añadir un proyecto existente conservando su ruta, y «Mover a este espacio» trasladará el contenido. Queda pendiente elegir la carpeta raíz y la integración visual con el escritorio de Windows.
