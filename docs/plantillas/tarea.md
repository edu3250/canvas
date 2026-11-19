# fNN-tNN · Título

<!-- Copiar a docs/desarrollo/NN-slug/tareas/NN-slug.md. Lo redacta Planificación. Ejecución no lo modifica. -->

- **Rama:** `fNN-slug` (la de la fase). Commits con prefijo `tNN`.
- **Depende de:** tNN | ninguna
- **Ensayo interactivo:** no | sí (solo tras el «adelante» del usuario; ver «Seguridad en Windows» en AGENTS.md)

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

Para un ensayo, identifica el punto de entrada y los símbolos existentes que debe usar el ejecutor. Nombra cada sujeto de prueba, cómo se crea y cómo se comprueba su identidad. Especifica en una tabla cuándo se recoge cada medición, con qué API y unidades, contra qué referencia se compara y qué tolerancia se acepta. Indica el destino de la evidencia y al menos un ejemplo que deba fallar aunque el resultado parezca correcto a simple vista.

## Pasos sugeridos

1.

## Verificación

```powershell
dotnet build
dotnet test
```

Guion manual o ensayo, si aplica, con el resultado esperado en cada paso.

## Criterios de aceptación

| Criterio | Evidencia esperada |
| --- | --- |
| | |

## Condiciones de parada

Detente y publica en el PR el informe de `docs/plantillas/informe-bloqueo.md` si:

- tras 3 intentos el mismo problema persiste;
- un test o una firma del contrato parece incorrecto;
- hace falta un archivo, una dependencia o una API fuera de esta tarea;
- la tarea admite más de una interpretación;
- (condiciones específicas de esta tarea)
