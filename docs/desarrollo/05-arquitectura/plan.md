# Fase 5 · Arquitectura y contrato del primer incremento

**Estado: cerrado.** [Cierre de la fase 5](../../inspeccion/f05-cierre.md). La fase entrega una arquitectura documentada y un contrato de navegación que otra fase podrá implementar tras su autorización. La revisión del PR anterior dejó tres transiciones por precisar. Este plan fija el resultado esperado de cada una y conserva la evidencia original.

## Decisiones acordadas

| Tema | Respuesta del usuario | Consecuencia verificable |
| --- | --- | --- |
| Entrega | «Arquitectura y contrato del primer incremento» | Cada tarea entrega documentación. |
| Activación externa | «Permanecer en el espacio de la notificación» | La activación cancela el cambio aplazado. |
| Ampliación | «Volver al panel ampliado y poder desampliarlo» | Inicio y la ampliación guardan vistas independientes por espacio. |
| Escala | «Conservar su tamaño lógico» | El tamaño observado se normaliza por DPI antes de actualizar el panel. |
| Arquitectura | «Las necesarias para el primer incremento» | Núcleo de navegación propio, en memoria. Interfaz gráfica, persistencia, raíz de contenido y reutilización externa se aplazan. |
| Entrevista | «Sí, preparar y publicar el nuevo PR del plan» | El usuario revisa el documento publicado antes de autorizar su fusión. |

## Entradas y alcance

El [PR #4](https://github.com/edu3250/escritorio-canvas/pull/4) conserva el trabajo histórico. Usar la [propuesta examinada](https://github.com/edu3250/escritorio-canvas/blob/bb687ea90d988b038937e0c86fb9da39424cb00c/docs/arquitectura/propuesta-producto.md), el [contrato anterior](https://github.com/edu3250/escritorio-canvas/blob/bb687ea90d988b038937e0c86fb9da39424cb00c/docs/desarrollo/05-arquitectura/primer-incremento.md) y el [dictamen H1–H3](https://github.com/edu3250/escritorio-canvas/blob/b6d1963d8a0b51dcedd221e6c82730856272d319/docs/inspeccion/f05-revision.md). Atribuir los resultados de Windows al ensayo histórico que los produjo.

Rigen el [diseño](../../diseno-inicial.md), los [ADR](../../adr/) y el [flujo](../../../AGENTS.md). Se describen el dominio, sus límites y los escenarios comprobables del futuro incremento. Quedan fuera código, paquetes, adopción de ADR, integración con aplicaciones y ensayos interactivos.

## Secuencia y controles comunes

Cada fila corresponde a una sesión y un PR desde main actualizado después de fusionar la dependencia. Cada tarea modifica únicamente los archivos de su sección. Rutas relativas a la raíz. En cada PR ejecutar `git diff --check origin/main...HEAD` y `git diff --name-only origin/main...HEAD`. Revisar el contenido indicado además del listado y del formato. Registrar evidencia y estado por criterio. Todas las verificaciones son documentales.

| Tarea | Rama | Dependencia |
| --- | --- | --- |
| t01 · Límites e identidades | f05-t01-dominio | — |
| t02 · Activación durante un diálogo | f05-t02-activacion | t01 |
| t03 · Ampliación y retorno | f05-t03-ampliacion | t02 |
| t04 · Geometría y DPI | f05-t04-geometria | t03 |
| t05 · Contrato de navegación | f05-t05-navegacion | t04 |
| t06 · Aceptación del incremento y cierre | f05-t06-aceptacion | t05 |

## t01 · Límites e identidades

Crear `docs/arquitectura/propuesta-producto.md`. Tomar de la propuesta histórica sus límites, modelo y alternativas. Una tabla asigna responsabilidad, estado y dirección de dependencia al dominio, coordinación, adaptador Windows, presentación y almacenamiento. El dominio puro conserva las cámaras por espacio. Distinguir identidad del espacio, ruta, acceso, panel y ventana nativa. Separar geometría del escritorio, geometría del lienzo y permiso de reapertura.

**Aceptación.** Seguir en la tabla dos casos: mover una carpeta entre espacios (ADR 001) y recordar la posición de un juego cuya reapertura está desactivada (ADR 002). Identificar el propietario de cada dato. Comparar WPF/WinForms, JSON/motor local y reutilización selectiva/propia como alternativas futuras. Detenerse ante una contradicción con un ADR.

## t02 · Activación durante un diálogo

Modificar solo `docs/arquitectura/propuesta-producto.md`. Usar las identidades de t01 y la tabla histórica de transiciones. Incorporar H1 con cuatro pasos: diálogo en A, petición de B aplazada, activación externa efectiva en C, cierre del diálogo. En la activación se consume la petición pendiente. El cierre conserva C y deja vacía la petición. Si el diálogo se cierra antes de una activación externa, se procesa el cambio pendiente una vez.

**Aceptación.** Una tabla muestra espacio activo y destino pendiente antes y después de cada evento en ambos órdenes. Distinguir la activación observada de una mera notificación que todavía nadie ha abierto. Parar si la regla requiere forzar el foco o atribuir a Windows un resultado sin evidencia.

## t03 · Ampliación y retorno

Modificar solo `docs/arquitectura/propuesta-producto.md`. Ampliar la tabla de t02. Al ampliar, guardar cámara y tamaño previos en el espacio propietario. Inicio conserva por separado la vista de retorno. Visitar otro espacio conserva ambos registros del origen. Desampliar restaura cámara y tamaño previos y consume el registro de ampliación.

**Aceptación.** Documentar dos recorridos completos, «ampliar → Inicio → retorno → desampliar» y «ampliar → cambiar espacio → volver → desampliar». Indicar espacio, cámara, tamaño y vigencia de ambos registros en cada paso. En ambos recorridos el panel sigue ampliado al volver. Parar si algún paso consume el retorno del otro mecanismo.

## t04 · Geometría y DPI

Modificar solo `docs/arquitectura/propuesta-producto.md`. Partir del tamaño que restaura t03. Definir NativeSize como tamaño exterior en unidades lógicas de 1/96 de pulgada. Una observación de tamaño exterior en píxeles se convierte por eje multiplicando por 96/DPI. Actualizar las dimensiones lógicas de CanvasBounds con ese valor en el acomodo correspondiente. En el mapa las dimensiones visibles derivan de CanvasBounds y del zoom. La proyección no modifica el dato guardado.

**Aceptación.** Ejemplo calculado: 1200 × 900 píxeles a 144 DPI dan 800 × 600 unidades. A zoom 0,5 ocupan 400 × 300 unidades de vista. Volver a zoom 1 conserva 800 × 600. Marcar el ejemplo como cálculo. Parar si la verificación requiere medir marcos o ejecutar un ensayo real.

## t05 · Contrato de navegación

Crear `docs/desarrollo/05-arquitectura/primer-incremento.md`. Tomar las firmas, tipos, archivos previstos y errores del contrato histórico enlazado. Especificar la biblioteca pura y su adaptador diagnóstico de consola. Referenciar t02–t04 como límites de integración futura. El incremento cubre cámara, Inicio y espacios en memoria.

**Contrato.** Documentar las firmas `NavigationResult Apply(SessionState state, NavigationCommand command)`, `Point WorldToView(Point world, Camera camera)` y `Point ViewToWorld(Point view, Camera camera)`. La posición de cámara es el origen visible. La proyección resta el origen y multiplica por zoom. La inversa divide por zoom y suma el origen. Describir SwitchSpace, SetCamera y ToggleHome. Cada espacio conserva cámara, HomeCamera y ReturnView opcional. Ir a Inicio guarda la cámara, moverse allí conserva el retorno y regresar lo consume. Cambiar al mismo espacio conserva todo el estado.

**Aceptación.** Mostrar entradas y salidas del recorrido Programación (120, 80; zoom 0,75) → Inicio (0, 0; zoom 1) → retorno → Juegos → mover su cámara → Programación. Documentar estado inmutable y errores por espacio desconocido, identificador duplicado al construir, coordenadas no finitas y zoom no positivo o no finito. Parar ante un caso que obligue a elegir interfaz o persistencia.

## t06 · Aceptación del incremento y cierre

Modificar `docs/desarrollo/05-arquitectura/primer-incremento.md`. En este mismo PR, Revisión crea `docs/inspeccion/f05-cierre.md` y actualiza `README.md` y `docs/plan-por-fases.md`. Usar el contrato de t05 y la plantilla de cierre. Registrar los SHA de las tareas y el esfuerzo medido por rol.

**Aceptación.** Añadir una matriz de pruebas futuras con entrada, acción, salida y método. Cubrir ida y vuelta, paneo en Inicio, independencia de espacios, errores sin mutación, estado anterior conservado, proyección inversa con tolerancia de 1e-9 unidades en las muestras acotadas y salida de consola en español con código cero en éxito y distinto de cero en error. Incluir inspección de dependencias del dominio y futura CI en windows-latest con build, test y cobertura de líneas del dominio de al menos 60 %. El control debe fallar ante informe ausente, denominador cero o cobertura insuficiente. Las versiones y permisos para paquetes corresponden al encargo de producto.

**Salida de fase.** Revisión contrasta H1–H3 con sus recorridos, enlaza la arquitectura y el contrato desde el cierre y registra cada criterio como verificado, fallido o pendiente con motivo. Mantener las pruebas de producto como pendientes de implementación. Si un supuesto depende de medir Windows o exige otra decisión de interacción, detener la tarea y devolver el caso a Planificación. La fusión del último PR cierra la entrega documental.
