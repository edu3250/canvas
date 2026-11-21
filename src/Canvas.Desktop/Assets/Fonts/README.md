# Fuentes

| Archivos | Origen | Licencia |
| --- | --- | --- |
| `Geist-Regular.ttf`, `Geist-Medium.ttf`, `Geist-SemiBold.ttf` | [vercel/geist-font, `fonts/Geist/ttf/`](https://github.com/vercel/geist-font/tree/main/fonts/Geist/ttf), descargados el 20-11-2026. Autores: The Geist Project Authors. | SIL Open Font License 1.1, en [OFL.txt](OFL.txt) |

Son los tres pesos que usa el sistema visual: 400, 500 y 600. Se usan archivos fijos y no la fuente variable porque WPF solo lee el peso por defecto de una fuente variable y simularía la negrita.

La fuente va incrustada en `Canvas.Desktop` como recurso WPF y la licencia se copia junto al ejecutable en `Licencias/Geist-OFL.txt`. El usuario autorizó incluirla al elegir la tipografía del sistema visual [Mesa de luz](../../../../docs/sistema-visual/mesa-de-luz.html).
