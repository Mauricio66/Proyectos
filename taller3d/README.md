# Taller 3D

Editor 3D sencillo, al estilo Tinkercad, para diseñar piezas y exportarlas en STL para imprimir. Funciona en el navegador, sin instalar nada.

## Cómo abrirlo

1. Descarga `index.html` (o clona este repositorio).
2. Ábrelo con doble clic en Chrome, Edge o Firefox.
3. Necesita conexión a internet la primera vez, porque carga Three.js y Manifold desde jsDelivr.

## Cómo usarlo

- **Agregar figuras:** Caja, Cilindro, Esfera, Cono, Pirámide, Hexágono, Techo, Caja hueca y Cilindro hueco.
- **Medidas en mm:** selecciona una figura y escribe su ancho, largo, alto y posición en el panel.
- **Mover, rotar y escalar:** con las herramientas de la barra o las teclas W, E y R.
- **Sólido o Hueco:** marca una figura como *Hueco* para que reste material.
- **Agrupar:** selecciona varias figuras y pulsa *Agrupar*. Los huecos se restan de los sólidos y las figuras sólidas se unen. *Desagrupar* (Ctrl+Shift+G) las separa otra vez.
- **Apoyar en la mesa:** baja la figura hasta la cuadrícula.
- **Ajuste a la cuadrícula:** elige cuánto avanza la figura al moverla (libre, 0,5, 1, 5 o 10 mm). La base de la figura queda siempre alineada, y al escalar la figura no se hunde en la mesa.
- **Duplicar:** botón *Duplicar* o Ctrl+D. **Seleccionar todo:** Ctrl+A.
- **Deshacer / Rehacer:** Ctrl+Z y Ctrl+Y.
- **Guardar / Abrir:** guarda el proyecto como `.json` para seguir otro día.
- **Exportar STL:** descarga el diseño listo para el programa de laminado (Cura, PrusaSlicer, etc.). Todas las figuras sólidas se unen en una sola pieza cerrada, apoyada en Z = 0. Si hay figuras seleccionadas, se exporta solo la selección.
- **Tamaño de la pieza:** con nada seleccionado, el panel derecho muestra las medidas totales y avisa si la pieza no cabe en la cama de 200 × 200 mm.
