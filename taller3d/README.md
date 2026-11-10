# Taller 3D

Editor 3D sencillo, al estilo Tinkercad, para diseñar piezas y exportarlas en STL para imprimir. Funciona en el navegador, sin instalar nada.

## Cómo abrirlo

1. Descarga `index.html` (o clona este repositorio).
2. Ábrelo con doble clic en Chrome, Edge o Firefox.
3. Necesita conexión a internet la primera vez, porque carga Three.js desde jsDelivr.

## Cómo usarlo

- **Agregar figuras:** Caja, Cilindro, Esfera, Cono, Pirámide, Hexágono, Techo, Caja hueca y Cilindro hueco.
- **Medidas en mm:** selecciona una figura y escribe su ancho, largo, alto y posición en el panel.
- **Mover, rotar y escalar:** con las herramientas de la barra o las teclas W, E y R.
- **Sólido o Hueco:** marca una figura como *Hueco* para que reste material.
- **Agrupar:** selecciona varias figuras y pulsa *Agrupar*. Los huecos se restan de los sólidos y las figuras sólidas se unen. *Desagrupar* (Ctrl+Shift+G) las separa otra vez.
- **Apoyar en la mesa:** baja la figura hasta la cuadrícula.
- **Deshacer / Rehacer:** Ctrl+Z y Ctrl+Y.
- **Guardar / Abrir:** guarda el proyecto como `.json` para seguir otro día.
- **Exportar STL:** descarga el diseño listo para el programa de laminado (Cura, PrusaSlicer, etc.).
