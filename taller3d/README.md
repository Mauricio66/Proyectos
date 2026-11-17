# Taller 3D

Editor 3D sencillo, al estilo Tinkercad, para diseñar piezas y exportarlas en STL para imprimir. Funciona en el navegador, sin instalar nada.

## Cómo abrirlo

**Con un enlace (lo más fácil para los alumnos):** https://mauricio66.github.io/proyectos/taller3d/

**Sin internet en la sala o para tenerlo en el PC:**

1. Descarga `index.html` (o clona este repositorio).
2. Ábrelo con doble clic en Chrome, Edge o Firefox.
3. Necesita conexión a internet la primera vez, porque carga Three.js y Manifold desde jsDelivr.

## Cómo usarlo

- **Menús de figuras:** a la izquierda, *Figuras básicas*, *Figuras complejas* y *Huecos*; cada menú se abre y se cierra con un clic en su título.
- **Agregar figuras:** Caja, Cilindro, Esfera, Cono, Pirámide, Hexágono, Texto, Tubo, Estrella, Corazón, Polígono, Dona, Arco, Rosca, Tuerca, Perno, Arandela, Tubo en L, Tubo en T, Tubo en X, Escuadra, Engranaje, Corona dentada, Cremallera, Techo, Caja hueca, Cilindro hueco y Texto hueco.
- **Figuras con ajustes:** *Polígono* (número de lados), *Dona* (el grosor del aro es el Alto Z), *Arco* (grosor en mm) y *Rosca* (paso en mm).
- **Tuerca:** rosca hembra que calza con la *Rosca* si tiene el mismo Ø y paso; la *Holgura* (0,3 mm por defecto) deja espacio para que gire.
- **Engranajes:** *Engranaje* (dientes, módulo y agujero para el eje), *Corona dentada* (dientes por dentro) y *Cremallera* (barra dentada). Engranan si tienen el mismo módulo; distancia entre centros = módulo × (dientes1 + dientes2) ÷ 2.
- **Repetir en patrón:** con una o varias figuras elegidas, *Repetir en patrón…* hace copias **en fila** (cantidad, separación y dirección) o **en círculo** (cantidad, ángulo total y centro), girando cada copia hacia el centro.
- **Revisar antes de imprimir:** con nada seleccionado, el botón *✓ Revisar antes de imprimir* avisa si la pieza no cabe en tu impresora (puedes escribir su volumen), si alguna parte queda flotando en el aire y si hay paredes de menos de 0,8 mm.
- **Guardado automático:** cada cambio se guarda en el navegador. Si se cierra o se corta la luz, al volver a abrir aparece el último diseño, y se puede seguir deshaciendo con Ctrl+Z.
- **Redondear esquinas:** en una caja, mueve la barra *Redondear esquinas* (radio en mm). El radio se mantiene aunque cambies el tamaño.
- **Tubo:** cilindro hueco por dentro; escribe el grosor de la *Pared* en mm.
- **Girar 90°:** botones Z, X e Y en el panel derecho giran la figura (o la selección) un cuarto de vuelta con un clic, sin que se hunda en la mesa.
- **Cubo de vistas:** en la esquina de la mesa, haz clic en una cara (Frente, Arriba, Derecha…), un borde o una esquina para mirar desde ese lado (la zona se resalta al pasar el mouse). Arrastra sobre el cubo para girar la vista y haz doble clic para volver a la vista general. Debajo: ⟲ ⟳ giran la vista 90°, ⌂ vuelve a la vista general, ⤢ ajusta para ver todo (o lo seleccionado), y + / − acercan y alejan. La rueda del mouse acerca hacia donde apuntas.
- **Mouse como en los programas CAD (v11):** aprieta la rueda y arrastra para desplazar la vista; Shift + rueda apretada gira la vista; el botón izquierdo también gira y el derecho también desplaza. El navegador ya no activa su desplazamiento automático al apretar la rueda.
- **Barra de herramientas (v13):** debajo del menú superior hay una fila de botones con dibujo y nombre para todo lo que antes estaba solo en el teclado o en el panel: Mover, Rotar, Escalar y Ajuste; Todo, Elegir varias, Duplicar y Eliminar; Hueco, Agrupar y Desagrupar; Girar 90°, Reflejar y Alinear (con menú); A la mesa y Centrar; Medir, Plano, Patrón, Mis piezas y Revisar. Los botones que no sirven en ese momento se ven apagados. Atajos nuevos: **H** hueco o sólido, **D** apoyar en la mesa, **L** alinear.
- **Panel de la derecha (v14):** ordenado en secciones (Tamaño, Posición, Rotación, Forma y Color). Arriba queda siempre a la vista el nombre de la figura, que se cambia con un clic, y el selector Sólido / Hueco. Los botones que se repetían con la barra de herramientas se quitaron. Con nada elegido muestra el tamaño de la pieza, *Unir todo en una pieza*, tu impresora y los atajos (se abren con un clic).
- **Plano de trabajo (v12):** botón *Plano* en la barra de herramientas; haz clic en la cara de una pieza (arriba o de lado) y las figuras nuevas aparecen sobre esa cara. *× Quitar plano* lo devuelve a la mesa.
- **Medir (v12):** botón *Medir* en la barra de herramientas o tecla M; clic en dos puntos y muestra la distancia total y en X, Y, Z. El punto se pega a las esquinas.
- **Distancia exacta (v12):** con dos figuras elegidas, el panel muestra la distancia de borde a borde y de centro a centro; al escribir un valor se mueve la segunda figura.
- **Mis piezas (v12):** El botón *Mis piezas* de la barra guarda la figura o la selección con una miniatura; aparece en el menú de la izquierda para agregarla otra vez. *Exportar* e *Importar* permiten compartir la biblioteca con los alumnos.
- **Ejercicios (v12):** modo clase con actividades paso a paso (llavero con nombre, caja con tapa, engranajes que engranan). Cada paso muestra ✓ cuando está bien hecho y tiene un botón *Ayúdame*.
- **Importar imagen o SVG (v12):** *Importar* acepta PNG, JPG o SVG y lo convierte en una pieza con relieve (umbral, invertir, ancho y alto en mm). En un SVG, lo pintado de blanco encima se resta.
- **Medidas a la vista:** sobre lo seleccionado aparece una etiqueta con ancho × fondo × alto en mm.
- **Ayuda:** la primera vez aparece una guía corta; se puede volver a ver con el botón *? Ayuda*, que también muestra los atajos de teclado.
- **Texto 3D:** pulsa *Texto* y escribe en el cuadro *Texto* del panel derecho (acepta tildes y ñ). Elige la letra *Recta* o *Clásica*. Con *Texto hueco* puedes grabar letras en una placa.
- **Medidas en mm:** selecciona una figura y escribe su ancho, largo, alto y posición en el panel.
- **Candado de proporción:** junto a *Tamaño mm*. Cerrado (*Proporcional*): al cambiar una medida, o al escalar con el mouse, las otras cambian en la misma proporción. Abierto (*Libre*): cada medida cambia sola.
- **Mover, rotar y escalar:** con las herramientas de la barra o las teclas W, E y R. Al escalar con el mouse, el tamaño avanza en milímetros enteros (o según el *Ajuste*) y con poca sensibilidad; mantén **Shift** para un ajuste fino de 0,1 mm.
- **Sólido o Hueco:** marca una figura como *Hueco* para que reste material.
- **Agrupar:** pon el hueco dentro del sólido, haz clic en un espacio vacío y pulsa *Agrupar* (o *Unir todo en una pieza* en el panel derecho). Sin selección se agrupa todo; para agrupar solo algunas figuras, pulsa *Elegir varias* (abajo en la mesa) y haz clic en cada una, o usa Mayús + clic. Los huecos se restan de los sólidos y las figuras sólidas se unen. *Desagrupar* (Ctrl+Shift+G) las separa otra vez.
- **Apoyar en la mesa:** baja la figura hasta la cuadrícula. **Centrar en la mesa:** la lleva al centro.
- **Alinear:** con varias figuras elegidas, el panel muestra botones para alinearlas por la izquierda, el centro, la derecha, adelante, atrás, abajo o arriba. Sirve, por ejemplo, para centrar un agujero en una pieza.
- **Reflejar (espejo) (v15):** botón *Reflejar* de la barra, con ejes X, Y y Z. Arriba del menú se elige qué hacer: **Dar vuelta** (la figura queda en su lugar, en espejo) o **Copia en espejo** (la figura queda igual y aparece pegada a ella una copia reflejada: a la derecha en X, atrás en Y, arriba en Z; ideal para piezas simétricas). La opción elegida se recuerda. Para una copia igual, sin espejo, sigue estando *Duplicar* (Ctrl+D). Las copias en espejo se exportan bien en STL.
- **Vista isométrica explosiva (v15–v17):** botón *Explosión* de la barra o tecla **X**. Separa las piezas solo para mirar cómo está armado el diseño; no cambia nada. Se ve en **isométrico** (los tres ejes con el mismo ángulo y sin perspectiva, como un plano técnico), con **líneas guía** punteadas desde cada pieza hasta donde va. Las piezas que se tocan se levantan en línea recta una sobre otra (los huecos salen por arriba, en gris) y los conjuntos separados se alejan del centro. La barra *Separación* ajusta cuánto se abren; las casillas *Isométrica*, *Líneas guía* y *Abrir los grupos* se pueden apagar. *Armar de nuevo* o Esc vuelve a la vista normal (cualquier cambio en el diseño también la cierra).
- **Plano de armado (v17):** en la vista explosiva, cada pieza lleva un **número** y aparece la **lista de piezas** (nombre, color y medidas en mm) en el orden de armado: de abajo hacia arriba y los huecos al final. Al pasar el mouse por la lista se resalta el número. *▶ Ver cómo se arma* mueve las piezas a su lugar una por una, mostrando «Paso 2 de 4: …», y al final las vuelve a separar. *⬇ Imagen PNG* descarga la vista con los números y la lista, lista para pegar en una guía. La casilla *Números* los oculta.
- **Explosión en 360° (v18):** en la vista explosiva, *Dirección: Según cómo encaja* hace que cada pieza salga por donde realmente se saca: la pieza más grande queda quieta, una tapa sale hacia arriba, un perno o pasador puesto de lado sale de lado, a lo largo de su eje. *Siempre hacia arriba* deja la forma anterior. La opción elegida se recuerda.
- **Piezas nuevas (v18):** *Perno* (cabeza hexagonal y rosca; calza con la *Tuerca* del mismo Ø y paso; se ajusta Ø, paso y alto de cabeza), *Arandela*, *Tubo en L*, *Tubo en T* y *Tubo en X* (acostados en la mesa: el Alto Z es el diámetro del tubo, y se ajusta la pared) y *Escuadra* (perfil en L con grosor ajustable). Están en *Figuras complejas*.
- **Ajuste a la cuadrícula:** elige cuánto avanza la figura al moverla (libre, 0,5, 1, 5 o 10 mm). La base de la figura queda siempre alineada, y al escalar la figura no se hunde en la mesa.
- **Duplicar:** botón *Duplicar* o Ctrl+D. **Seleccionar todo:** Ctrl+A.
- **Deshacer / Rehacer:** Ctrl+Z y Ctrl+Y.
- **Guardar / Abrir:** *Guardar* (Ctrl+S) pide un nombre y guarda el proyecto como `.json` para seguir otro día; *Abrir* lo carga de nuevo. Además, la página recuerda el último diseño al recargarla.
- **Importar STL:** trae una pieza `.stl` a la mesa para moverla, cambiarle el tamaño, unirla con otras figuras o hacerle agujeros. Si la pieza tiene errores (no es cerrada) se puede ver y exportar, pero no unir. Las piezas importadas se guardan dentro del proyecto.
- **Exportar STL:** descarga el diseño listo para el programa de laminado (Cura, PrusaSlicer, etc.). Todas las figuras sólidas se unen en una sola pieza cerrada, apoyada en Z = 0, y se le restan los huecos, aunque no estén agrupados. Si hay figuras seleccionadas, se exporta solo la selección.
- **Tamaño de la pieza:** con nada seleccionado, el panel derecho muestra las medidas totales y avisa si la pieza no cabe en la cama de 200 × 200 mm.
