# Fase NN · Título

<!-- Copiar a docs/desarrollo/NN-slug/plan.md. Lo redacta Planificación después de /grill-me, en la rama fNN-plan. El usuario lo aprueba. Ejecución no lo modifica. -->

**Estado:** en aprobación | aprobado | en ejecución (tNN de NN fusionadas) | cerrado (enlace al cierre).

## Objetivo y decisión que debe permitir

Qué se construye o se ensaya, y qué podrá decidir el usuario al terminar la fase.

## Decisiones acordadas

| # | Decisión | Respuesta del usuario | Consecuencia en el plan |
| --- | --- | --- | --- |
| 1 | | «cita textual» | |

## Entrada

Documentos, ADR, informes y revisiones de código de partida. Fijar por SHA todo lo externo.

## Alcance

- Incluido:
- Fuera de alcance:
- Tipo: prototipo en `prototipos/<nombre>` | producto en `src/`.

## Diseño

Modelo de datos, interfaces, secuencia de llamadas Win32 y casos límite comunes a varias tareas. Lo específico de una tarea va en su sección.

## Secuencia de tareas

| Id | Objetivo | Depende de | Ensayo interactivo |
| --- | --- | --- | --- |
| t01 | | — | no |
| t02 | | t01 | no |
| t03 | | t02 | sí |

## Criterios de salida de la fase

| Afirmación | Evidencia necesaria |
| --- | --- |
| | |

## Qué obligaría a replanificar

Resultados de una tarea que invalidarían las siguientes.

---

## t01 · Título

- **Rama:** `fNN-t01-slug` · **Depende de:** ninguna · **Ensayo interactivo:** no | sí (solo tras el «adelante» del usuario)
- **Supone:** qué da por hecho de las tareas anteriores.

**Objetivo.** Una frase con el resultado verificable.

**Contexto mínimo.** Enlaces concretos (sección o línea) a lo que hay que leer.

**Archivos.**

| Acción | Ruta |
| --- | --- |
| crear | |
| modificar | |

Cualquier otro archivo queda fuera de alcance.

**Contrato.** Firmas exactas y tests que Ejecución crea tal cual y no modifica:

```csharp
```

Para Win32: llamadas, orden, banderas y qué hacer con cada valor de retorno. Para un ensayo: sujetos de prueba y cómo se comprueba su identidad, mediciones con API, unidades, referencia y tolerancia, destino de la evidencia y al menos un caso que debe fallar.

**Verificación.**

```powershell
dotnet build
dotnet test
```

**Criterios de aceptación.**

| Criterio | Evidencia esperada |
| --- | --- |
| | |

**Parada.** Además de las condiciones generales de Ejecución: (condiciones específicas de esta tarea).

---

## t02 · Título

(Misma estructura.)

---

## Cierre

Lo completa Revisión: estado «cerrado» y enlace a `docs/inspeccion/fNN-cierre.md`, redactado con la [plantilla de cierre](cierre-fase.md).
