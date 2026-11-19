# Propuesta de arquitectura del producto

El primer incremento se limita a un núcleo de navegación propio y en memoria. El resto del modelo describe los límites para una integración futura. La interfaz gráfica, la persistencia, la raíz de contenido y la reutilización externa se aplazan. [Plan de fase 5](../desarrollo/05-arquitectura/plan.md).

## Límites y propiedad del estado

La propuesta parte de un proceso local. Las flechas expresan dependencias de código. Coordinación define los puertos de efectos y el punto de composición conecta sus implementaciones.

| Módulo | Responsabilidad | Estado del que es dueño | Dependencia |
| --- | --- | --- | --- |
| Dominio | Validar identidades y geometría del lienzo y resolver comandos de navegación con datos puros. | Espacios, referencias de carpeta, accesos, paneles y permiso de reapertura por panel. Cada espacio conserva la cámara de su lienzo, la cámara de Inicio y su vista de retorno opcional. El estado de navegación incluye el espacio activo. | Ninguna hacia los demás módulos. |
| Coordinación | Ordenar comandos, pedir efectos por puertos y reconciliar sus resultados antes de actualizar el modelo. | Operaciones en curso, asociación entre panel y ventana viva, última ventana usada por aplicación y espacio, y acomodo del escritorio capturado por panel. | Coordinación → dominio. Los puertos de ventanas, operaciones de archivos y almacenamiento pertenecen a coordinación. |
| Adaptador Windows | Observar ventanas, validar su identidad, solicitar efectos mediante APIs públicas y comunicar el resultado observado. Las operaciones de archivos actúan sobre las rutas solicitadas y devuelven su resultado. | Referencias y recursos nativos, suscripciones y observaciones de geometría. El sistema de archivos es la fuente de verdad de las rutas y del contenido físico. | Adaptador Windows → puertos de coordinación. |
| Presentación | Mostrar el estado y los resultados de las operaciones, proyectar el lienzo y emitir comandos. | Selección visual y formularios transitorios. La cámara que usa para proyectar se lee del dominio. | Presentación → coordinación. |
| Almacenamiento | Cargar, validar y guardar instantáneas de metadatos cuando se incorpore persistencia. | Formato versionado y generaciones confirmadas. Conserva copias de los datos del dominio y del acomodo que corresponda guardar. | Almacenamiento → puerto de coordinación. |

## Identidades y acomodos

| Dato | Identidad y significado | Propietario |
| --- | --- | --- |
| Espacio | `SpaceId` es estable. Identifica el conjunto de carpeta, fondo, Inicio y lienzo aunque cambie la ruta de su carpeta. | Dominio, en `Space`. |
| Ruta | Ubica una carpeta de espacio o el destino de un acceso a contenido. Su validez depende del sistema de archivos. | Dominio para la referencia configurada. Sistema de archivos para la ubicación efectiva. |
| Acceso | `ShortcutId` identifica un icono de Inicio. Su destino distingue una aplicación instalada, contenido del espacio o contenido externo cuya ruta se conserva al crear el acceso. | Dominio, en `Home`. |
| Panel | `PanelId` identifica la representación de una ventana en un espacio aun cuando la ventana esté cerrada. El descriptor de aplicación y ventana permite distinguir paneles de una misma aplicación. | Dominio, en el espacio propietario. |
| Ventana nativa | `LiveWindowRef` identifica una ventana observada durante la sesión con token de sesión, HWND, PID y generación observada. Coordinación valida la asociación de una ventana a un único panel. | Adaptador Windows para la referencia nativa. Coordinación para la asociación con el panel. |
| Geometría del escritorio | `DesktopPlacement` registra coordenadas nativas con su contexto y el estado normal o maximizado para restaurar el acomodo de Windows. | Coordinación a partir de observaciones del adaptador Windows. |
| Geometría del lienzo | `CanvasBounds` sitúa y dimensiona el panel en el lienzo. `NativeSize` representa el tamaño de la ventana al editar. La presentación proyecta los límites con la cámara del espacio. | Dominio, en `Panel` para los límites y el tamaño, y en `Canvas` para la cámara. |
| Permiso de reapertura | `ReopenPolicy` guarda la elección explícita del usuario por panel. Coordinación consulta ese permiso antes de lanzar una aplicación para recuperar un panel. | Dominio, en `Panel`. Almacenamiento conserva esa elección como dato independiente de los acomodos. |

## Recorridos de los ADR

| Caso y paso | Responsabilidad y cambio | Propietario del dato |
| --- | --- | --- |
| [ADR 001](../adr/001-archivos-y-accesos.md) · Guardar una carpeta de proyecto en Programación | La carpeta y sus documentos quedan bajo la ruta del espacio. Inicio muestra el contenido de esa ubicación. | Dominio posee `SpaceId` y la referencia de carpeta del espacio. El sistema de archivos posee la carpeta de proyecto y su contenido. |
| ADR 001 · Cambiar a Juegos | La navegación activa el espacio destino con su cámara y conserva la de Programación. La carpeta de proyecto sigue en Programación. | Dominio posee el espacio activo y ambas cámaras. El sistema de archivos conserva la ubicación. |
| ADR 001 · Solicitar «Mover a este espacio» | Coordinación pide al adaptador una operación de archivos desde la carpeta de Programación hasta la de Juegos. La carpeta se traslada con sus documentos. Coordinación concilia la ubicación y las referencias de contenido afectadas a partir del resultado confirmado. Si el traslado falla o queda parcial, presenta el resultado observado antes de registrar cambios de referencias. | El sistema de archivos posee las ubicaciones efectivas. Coordinación posee el estado de la operación. Dominio posee las referencias de contenido y conserva los identificadores y las rutas raíz de ambos espacios. |
| ADR 001 · Crear un acceso a un proyecto externo | «Crear acceso aquí» agrega un icono que apunta a la ruta externa. La ubicación de la carpeta permanece en el sistema de archivos. | Dominio posee el nuevo `ShortcutId` y su destino. El sistema de archivos posee la carpeta y los documentos. |
| [ADR 002](../adr/002-sesion-y-reapertura.md) · Recordar la posición de un juego | El panel conserva sus límites en el lienzo y su permiso de reapertura desactivado. El acomodo del escritorio se captura por separado cuando hay una ventana asociada. | Dominio posee `PanelId`, `CanvasBounds` y `ReopenPolicy`. Coordinación posee `DesktopPlacement`. Almacenamiento conserva los metadatos que se guarden. |
| ADR 002 · Reiniciar el gestor con el juego cerrado | Coordinación carga los metadatos y reconcilia primero las ventanas existentes. Al faltar una ventana del juego y estar desactivada su reapertura, el panel conserva su posición y queda sin asociación. El juego permanece cerrado. | Dominio posee la posición y el permiso. Coordinación posee el estado de la asociación. El adaptador Windows informa las ventanas observadas. |
| ADR 002 · El usuario inicia el juego | Si se verifica una coincidencia única, coordinación asocia la nueva ventana al panel recordado. Si hay varias coincidencias, se requiere una elección. La aplicación recupera sus propios datos. | Adaptador Windows posee la nueva referencia viva. Coordinación posee la asociación. Dominio conserva la identidad del panel y sus límites. |

## Activación durante un diálogo

El diálogo modal pertenece al panel de la ventana que lo abrió. El adaptador Windows comunica la activación efectiva que observa. Coordinación valida la `LiveWindowRef`, resuelve su asociación con el `PanelId` y obtiene el `SpaceId` propietario. El dominio conserva el espacio activo y las cámaras. Coordinación conserva el destino aplazado como un `SpaceId` opcional mientras el diálogo está abierto.

Al observar una activación externa validada en otro espacio, coordinación consume el destino aplazado y aplica el cambio al espacio de la ventana activada. Una notificación que nadie ha abierto deja intactos el espacio activo y el destino pendiente. El cambio sigue el foco que Windows ya otorgó. Si el diálogo se cierra primero, coordinación consume el destino al procesarlo para que el cambio se aplique una sola vez.

Ambos recorridos parten del espacio A activo y la petición vacía. Los eventos se procesan en el orden de la tabla. El diálogo pertenece a un panel de A y la activación externa corresponde a una ventana ya asociada a un panel de C.

| Orden | Evento | Espacio activo antes | Destino pendiente antes | Espacio activo después | Destino pendiente después |
| --- | --- | --- | --- | --- | --- |
| Activación antes del cierre | Se abre el diálogo en A. | A | Vacío | A | Vacío |
| Activación antes del cierre | Se solicita B durante el diálogo. | A | Vacío | A | B |
| Activación antes del cierre | Se observa la activación externa efectiva en C. | A | B | C | Vacío |
| Activación antes del cierre | Se cierra el diálogo de A. | C | Vacío | C | Vacío |
| Cierre antes de la activación | Se abre el diálogo en A. | A | Vacío | A | Vacío |
| Cierre antes de la activación | Se solicita B durante el diálogo. | A | Vacío | A | B |
| Cierre antes de la activación | Se cierra el diálogo de A y se procesa la petición de B. | A | B | B | Vacío |
| Cierre antes de la activación | Se observa la activación externa efectiva en C. | B | Vacío | C | Vacío |

La tabla especifica la respuesta del gestor a los eventos observados. La integración con activaciones reales de Windows queda pendiente de ensayo de producto.

## Alternativas para fases futuras

Los costes son comparaciones cualitativas de la propuesta histórica.

| Decisión | Alternativas y consecuencias |
| --- | --- |
| Interfaz gráfica | WPF aporta composición, controles y enlace de datos. WinForms aporta experiencia del prototipo y requeriría más composición manual del lienzo según la propuesta histórica. La integración de ventanas nativas requiere validación con cualquiera de las dos. |
| Persistencia | JSON versionado facilita inspeccionar una instantánea local con un escritor y requiere diseñar validación, migración y recuperación. Un motor local ofrece consultas y transacciones a cambio de incorporar y mantener una dependencia. |
| Reutilización | El núcleo propio permite acotar la navegación y mantenerla aislada de Windows, a cambio de implementar sus reglas y pruebas. Una extracción selectiva de Canvas Window Composer podría aprovechar cámara o descarte geométrico si se comprueba el acoplamiento de cada pieza y se autoriza su incorporación con licencia MIT y atribución a Devnova. La adopción completa arrastraría dependencias de shell y del ciclo de activación descritas en la inspección histórica. [Inspección de Canvas Window Composer](../inspeccion/informe-cwc.md). |

## Evidencia histórica

El ensayo de fase 2 registró edición a dos anchos y cancelación de un diálogo. Los ensayos controlados de las fases 3 y 4 documentaron recorridos entre dos acomodos. La fase 4 probó dos instancias aisladas de VS Code a 96 DPI en Windows 11. Los resultados pertenecen a esos ensayos y las pruebas de producto siguen pendientes. [Fase 2](../inspeccion/ensayo-panel-nativo.md), [fase 3](../inspeccion/f03-cierre.md), [fase 4](../inspeccion/f04-cierre.md).

Los límites, el modelo y las alternativas proceden de la propuesta histórica. El recorrido del diálogo precisa H1. Las transiciones H2 y H3 corresponden a T03 y T04. [Propuesta examinada](https://github.com/edu3250/escritorio-canvas/blob/bb687ea90d988b038937e0c86fb9da39424cb00c/docs/arquitectura/propuesta-producto.md), [dictamen histórico](https://github.com/edu3250/escritorio-canvas/blob/b6d1963d8a0b51dcedd221e6c82730856272d319/docs/inspeccion/f05-revision.md).
