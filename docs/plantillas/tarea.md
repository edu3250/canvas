# fNN-tNN · Título

<!-- Copiar a docs/desarrollo/NN-slug/tareas/NN-slug.md. Lo redacta Planificación. Ejecución no lo modifica. -->

- **Rama:** rama del PR del encargo
- **Depende de:** tNN | ninguna
- **Ensayo interactivo:** no | sí (AGENTS.md §7: avisar y esperar el «adelante» del usuario)

## Objetivo

Una frase con el resultado verificable.

## Contexto mínimo

Enlaces concretos (sección o línea) a lo que hay que leer. Nada más.

## Archivos

| Acción | Ruta |
| --- | --- |
| crear | |
| modificar | |

Cualquier otro archivo queda fuera de alcance. Tampoco se pueden modificar los tests ni las firmas que entrega Planificación.

## Contrato

Firmas o interfaces exactas, ya presentes en la rama del plan o copiadas aquí:

```csharp
```

Tests que deben pasar: `tests/...` (nombres de los tests).

Para Win32: llamadas, orden, banderas y qué hacer con cada valor de retorno.

## Pasos sugeridos

1.

## Verificación

```powershell
dotnet build
dotnet test
```

Guion manual o ensayo, si aplica, con el resultado esperado en cada paso.

## Criterios de aceptación

| Criterio | Evidencia en el PR |
| --- | --- |
| | |

## Condiciones de parada

Detente y abre un PR en borrador con el informe de `docs/plantillas/informe-bloqueo.md` si:

- tras 3 intentos el mismo problema persiste;
- un test o una firma del contrato parece incorrecto;
- hace falta un archivo, una dependencia o una API fuera de esta tarea;
- la tarea admite más de una interpretación;
- (condiciones específicas de esta tarea)
