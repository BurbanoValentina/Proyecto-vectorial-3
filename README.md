# Proyecto de Simulación Vectorial 3D

Un proyecto interactivo desarrollado en Unity que combina visualización de campos vectoriales con simulación física en tiempo real. Diseñado para explorar conceptos matemáticos de manera visual mediante barcos navegables, entornos procedurales y controles dinámicos.

# Características Principales

El proyecto incluye generación dinámica de campos vectoriales basados en fórmulas matemáticas personalizables. Los barcos navegan automáticamente siguiendo las corrientes del campo vectorial mientras mantienen un sistema realista de flotación sobre agua con efectos de ondas.

La exploración se realiza en primera persona sobre una isla completamente modelada, permitiendo observar los vectores desde cualquier ángulo. El sistema incluye tooltips interactivos que muestran información detallada de cada vector al pasar el cursor sobre las flechas de visualización.

Un panel de control en tiempo real permite modificar todos los parámetros del campo vectorial mientras el juego está en ejecución, facilitando la experimentación con diferentes configuraciones matemáticas.

# Controles

Las teclas WASD controlan el movimiento del jugador mientras el mouse permite mirar alrededor del entorno. La tecla V alterna la visibilidad del panel de control. Al posicionar el cursor sobre las flechas vectoriales se despliega información sobre posición, dirección y magnitud del vector.

# Panel de Control

El panel permite ajustar las coordenadas del centro del campo en los tres ejes, definir el tamaño del área de generación tanto en ancho como en largo, e ingresar fórmulas matemáticas para los componentes X e Y del vector. El botón Generar crea el campo con los parámetros especificados, mientras que Limpiar elimina el campo actual.

# Ejemplos de Fórmulas

Para crear un campo rotacional tipo remolino utilice formulaX = -y y formulaY = x. Un campo radial de expansión desde el centro se logra con formulaX = x y formulaY = y. Los campos ondulatorios pueden generarse con formulaX = sin(y) y formulaY = cos(x).

# Estructura del Proyecto

El proyecto está organizado en Assets/_Project/ con cinco módulos principales.

El módulo Island contiene materiales de terrenos y agua, modelos 3D de edificios y elementos naturales, prefabs organizados por categorías como puentes, decoración de suelo, masas de tierra, montañas, ríos, rocas y vegetación, además de texturas y shaders especializados.

El módulo Boats incluye scripts de control y flotación del barco junto con prefabs completos y modelos 3D con accesorios.

El módulo VectorField es el núcleo del sistema con scripts de gestión, visualización, seguimiento vectorial y tooltips, además de herramientas de editor personalizado, prefabs de flechas, materiales, shaders de computación y documentación completa.

El módulo Water implementa efectos de ondas mediante scripts de input, colisiones y simulación de lluvia, junto con materiales, shaders, texturas y prefabs de planos de agua.

El módulo Scenes contiene BigIsland.unity como escena principal, Demo Scene.unity para demostraciones, y configuraciones de iluminación y calidad.

# Instalación

Clone el repositorio o descargue el ZIP y descomprima en su carpeta de proyectos de Unity. Abra Unity Hub, agregue el proyecto y ábralo con Unity 2022.3 LTS o superior. Cargue la escena BigIsland.unity ubicada en Assets/_Project/Scenes/ y presione Play para comenzar.

# Requisitos Técnicos

El proyecto requiere Unity 2022.3 LTS o superior con TextMeshPro incluido. Es compatible con Windows, macOS y Linux. Se recomienda un mínimo de 8GB de RAM y una GPU compatible con DirectX 11 o superior.

# Conceptos Educativos

Este proyecto es útil para aprender sobre campos vectoriales, funciones de dos variables, simulación de flujos y corrientes, generación procedural en Unity, optimización de código y visualización gráfica de conceptos matemáticos abstractos.

# Licencia

Proyecto de código abierto disponible para propósitos educativos y de aprendizaje.
