# Taller 3D

Editor 3D sencillo, al estilo Tinkercad, para diseñar piezas y exportarlas en STL para imprimir. Funciona en el navegador, sin instalar nada.

## Cómo abrirlo

**Con un enlace (lo más fácil para los alumnos):** https://mauricio66.github.io/proyectos/taller3d/

**Sin internet en la sala o para tenerlo en el PC:**

1. Descarga `index.html` (o clona este repositorio).
2. Ábrelo con doble clic en Chrome, Edge o Firefox.
3. Necesita conexión a internet la primera vez, porque carga Three.js y Manifold desde jsDelivr.

## Cómo usarlo

- **Agregar figuras:** Caja, Cilindro, Esfera, Cono, Pirámide, Hexágono, Texto, Techo, Caja hueca, Cilindro hueco y Texto hueco.
- **Texto 3D:** pulsa *Texto* y escribe en el cuadro *Texto* del panel derecho (acepta tildes y ñ). Elige la letra *Recta* o *Clásica*. Con *Texto hueco* puedes grabar letras en una placa.
- **Medidas en mm:** selecciona una figura y escribe su ancho, largo, alto y posición en el panel.
- **Mover, rotar y escalar:** con las herramientas de la barra o las teclas W, E y R.
- **Sólido o Hueco:** marca una figura como *Hueco* para que reste material.
- **Agrupar:** pon el hueco dentro del sólido, haz clic en un espacio vacío y pulsa *Agrupar* (o *Unir todo en una pieza* en el panel derecho). Sin selección se agrupa todo; para agrupar solo algunas figuras, pulsa *Elegir varias* (abajo en la mesa) y haz clic en cada una, o usa Mayús + clic. Los huecos se restan de los sólidos y las figuras sólidas se unen. *Desagrupar* (Ctrl+Shift+G) las separa otra vez.
- **Apoyar en la mesa:** baja la figura hasta la cuadrícula. **Centrar en la mesa:** la lleva al centro.
- **Alinear:** con varias figuras elegidas, el panel muestra botones para alinearlas por la izquierda, el centro, la derecha, adelante, atrás, abajo o arriba. Sirve, por ejemplo, para centrar un agujero en una pieza.
- **Reflejar (espejo):** botones X, Y y Z en el panel derecho dan vuelta la figura (o la selección) como en un espejo.
- **Ajuste a la cuadrícula:** elige cuánto avanza la figura al moverla (libre, 0,5, 1, 5 o 10 mm). La base de la figura queda siempre alineada, y al escalar la figura no se hunde en la mesa.
- **Duplicar:** botón *Duplicar* o Ctrl+D. **Seleccionar todo:** Ctrl+A.
- **Deshacer / Rehacer:** Ctrl+Z y Ctrl+Y.
- **Guardar / Abrir:** *Guardar* (Ctrl+S) pide un nombre y guarda el proyecto como `.json` para seguir otro día; *Abrir* lo carga de nuevo. Además, la página recuerda el último diseño al recargarla.
- **Importar STL:** trae una pieza `.stl` a la mesa para moverla, cambiarle el tamaño, unirla con otras figuras o hacerle agujeros. Si la pieza tiene errores (no es cerrada) se puede ver y exportar, pero no unir. Las piezas importadas se guardan dentro del proyecto.
- **Exportar STL:** descarga el diseño listo para el programa de laminado (Cura, PrusaSlicer, etc.). Todas las figuras sólidas se unen en una sola pieza cerrada, apoyada en Z = 0, y se le restan los huecos, aunque no estén agrupados. Si hay figuras seleccionadas, se exporta solo la selección.
- **Tamaño de la pieza:** con nada seleccionado, el panel derecho muestra las medidas totales y avisa si la pieza no cabe en la cama de 200 × 200 mm.
