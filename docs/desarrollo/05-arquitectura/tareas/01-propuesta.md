# f05-t01 · Documentar arquitectura y primer incremento

**Rol:** Ejecución documental. **Rama:** f05-plan. **Interacción:** ninguna. **Entrada:** [encargo](../encargo.md) y sus enlaces.

## Archivos permitidos

Crear `docs/arquitectura/propuesta-producto.md` y `docs/desarrollo/05-arquitectura/primer-incremento.md`. No modificar planes, ADR existentes ni código. La revisión completará enlaces del mapa y README.

## Contrato de la propuesta

1. Encabezar con estado propuesto y evidencia por SHA. Distinguir observado, confirmado, inferido y pendiente. No generalizar los resultados de 96 DPI y dos ventanas a otras aplicaciones, escalas o carga.
2. Proponer módulos mínimos: dominio puro, coordinación de sesión, adaptador Windows, almacenamiento e interfaz. Mostrar dirección de dependencias y propiedad de estado. Evitar infraestructura distribuida o dependencias innecesarias.
3. Definir tipos conceptuales en inglés: Space, Canvas, Home, Panel, Shortcut, identificadores, geometría del lienzo, vista de retorno, ventana viva y política de reapertura. Separar HWND/PID efímeros de identidad persistida; detallar reconciliación sin reapertura duplicada y ambigüedad sin selección automática.
4. Definir estados y transiciones: vista general, edición, ampliación, Inicio/retorno, cambio de espacio, escritorio y salida. Separar zoom del mapa de tamaño nativo. Indicar unidades, transformaciones, DPI y límites de tamaño. No convertir una alternativa de interacción nueva en requisito acordado.
5. Aislar eventos Win32 y UI de dominio; prever eventos tardíos, pérdida de identidad y reversión parcial. Una llamada aceptada no demuestra geometría ni foco alcanzados. Definir qué ocurre cuando no se logra restaurar sin prometer recuperación tras caída no ensayada.
6. Separar las capacidades del producto para asignar ventanas existentes de las restricciones de automatización: ensayos solo sobre procesos propios. El cierre forzado de una instancia descartable de fase 4 no autoriza terminar aplicaciones reales al salir del gestor.
7. Comparar CWC completo, extracción selectiva y código propio. Usar la inspección fijada como evidencia histórica atribuida. No copiar código, ni consultar código externo sin fijar revisión. Enumerar dependencias estructurales propuestas (incluyendo plataforma UI) y paquetes de pruebas/medición que requerirán aprobación conforme AGENTS.md. Verificar en documentación primaria las afirmaciones técnicas nuevas o temporales y enlazar las fuentes.
8. Proponer almacenamiento versionado con política ante datos corruptos, escritura interrumpida, carpetas ausentes y cambios de ruta. Distinguir datos del gestor de contenido del usuario y estado interno de aplicaciones. La carpeta raíz del contenido sigue siendo decisión explícita, sin inventar una ruta personal.
9. Tabla requisito → módulo/contrato → prueba → estado. Lista priorizada de riesgos pendientes: foco externo, juegos, otras escalas, recuperación y rendimiento. Recomendar camino sin presentar estimaciones precisas sin datos.

## Primer incremento

Documento expresamente «propuesta; no autorizado para ejecución». Definir un corte pequeño que pruebe cámara/Inicio/retorno y modelo de espacios con lógica independiente de Windows, y una forma concreta de observarlo. Especificar archivos previstos, firmas públicas propuestas, comportamiento de errores y criterios por escenario. Mantener fuera de ese corte operaciones de ventanas y archivos reales si no son necesarias para el objetivo. Explicar qué parte visible se entrega y qué todavía no demuestra.

La primera creación de producto incluirá CI windows-latest con dotnet build y dotnet test, xUnit y medición del mínimo de cobertura 60 % de lógica modificada, con el conjunto de paquetes y versiones pendientes de acuerdo o verificación antes de ejecutar. No iniciar una fase posterior, instalar paquetes ni crear src/ en esta entrega.

## Validación y salida

Revisar enlaces locales, coherencia de nombres y requisitos, diff --check y ausencia de código/dependencias añadidos. Publicar commits en el mismo PR, conservar SHA del plan y añadir herramienta, modelo, intentos y esfuerzo. Si falta una decisión, presentar recomendación y alternativa concretas; completar primero toda la propuesta revisable. Ante contradicción con ADR o cambio de alcance, devolver el caso a Planificación.
