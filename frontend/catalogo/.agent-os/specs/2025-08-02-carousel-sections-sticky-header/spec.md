# Spec Requirements Document

> Spec: Carousel Sections & Sticky Header
> Created: 2025-08-02
> Status: Planning

## Overview

Implementar secciones de carrusel para "Novedades" y "Ofertas" que consuman nuevos endpoints de API, junto con funcionalidad de header sticky para mejorar la experiencia de navegación. Estas mejoras proporcionarán mayor visibilidad a productos especiales y mantendrán las herramientas de búsqueda siempre accesibles durante el scroll.

## User Stories

### Historia 1: Navegación de Novedades y Ofertas

Como **consumidor del catálogo**, quiero ver carruseles destacados de productos nuevos y en oferta al inicio de la página, para que pueda descubrir rápidamente los mejores productos sin tener que buscar específicamente.

**Flujo detallado:**
1. Usuario entra al catálogo público de una empresa
2. Ve inmediatamente las secciones "Novedades" y "Ofertas" debajo de la búsqueda
3. Los carruseles rotan automáticamente cada 3 segundos mostrando diferentes productos
4. Puede usar controles manuales (flechas) para navegar si algo le interesa
5. Al hacer hover, el carrusel se pausa para revisar el producto con calma
6. Puede agregar productos al carrito sin perder el contexto del carrusel

### Historia 2: Búsqueda Accesible Durante el Scroll

Como **usuario navegando el catálogo**, quiero que el header con búsqueda se mantenga visible cuando hago scroll hacia abajo, para que pueda buscar productos específicos en cualquier momento sin tener que volver al inicio.

**Flujo detallado:**
1. Usuario comienza a hacer scroll hacia abajo en el catálogo
2. El header se reduce automáticamente a una versión compacta (80px) pero permanece fijo
3. Mantiene acceso al logo, búsqueda y carrito de compras
4. Puede usar la búsqueda desde cualquier posición de la página
5. El header compacto no interfiere con la visualización de productos

### Historia 4: Navegación Inteligente por Secciones

Como **usuario que limpia filtros de categorías**, quiero que el scroll me posicione directamente en la sección de categorías con el header ya reducido, para que pueda ver inmediatamente todas las opciones de categorías disponibles sin tener que pasar por los carruseles nuevamente.

**Flujo detallado:**
1. Usuario limpia filtros de categorías activos
2. La página se posiciona automáticamente en la sección de categorías (scroll hacia abajo)
3. El header aparece en su estado compacto (80px) desde el inicio
4. Los carruseles de novedades y ofertas quedan arriba pero no visibles
5. Usuario puede subir manualmente si desea ver los carruseles nuevamente

### Historia 5: Continuidad en Paginación

Como **usuario navegando por páginas del catálogo**, quiero mantener mi posición de scroll en la zona de productos cuando cambio de página, para que pueda continuar viendo productos sin interrupciones visuales.

**Flujo detallado:**
1. Usuario navega por páginas del catálogo (página 2, 3, etc.)
2. La página mantiene el scroll en la sección de productos (sin volver arriba)
3. El header se mantiene en su estado compacto si ya estaba así
4. Los carruseles permanecen arriba pero no interfieren con la experiencia de paginación
5. El flujo de navegación entre productos se mantiene fluido

### Historia 3: Experiencia Mobile Optimizada

Como **usuario mobile**, quiero que los carruseles muestren un producto a la vez ocupando todo el ancho disponible, para que pueda ver los detalles claramente en mi dispositivo pequeño.

**Flujo detallado:**
1. Usuario accede desde dispositivo móvil
2. Los carruseles muestran 1 producto por vista en ancho completo
3. Puede deslizar horizontalmente para ver más productos
4. El header sticky funciona igual pero optimizado para touch
5. Los productos se ven con suficiente detalle para tomar decisiones de compra

## Spec Scope

1. **Carrusel de Novedades** - Sección que consume `/api/catalog/novedades` con rotación automática y controles manuales
2. **Carrusel de Ofertas** - Sección que consume `/api/catalog/ofertas` con las mismas funcionalidades que Novedades
3. **Header Sticky Responsivo** - Header que se mantiene fijo y se reduce a 80px al hacer scroll
4. **Product Cards Destacadas** - Diseño especial para productos en carruseles que destaque más que las cards normales
5. **Sistema de Cache** - Implementación de caché para las llamadas iniciales de novedades y ofertas
6. **Navegación Inteligente** - Scroll automático a sección de categorías solo al limpiar filtros, manteniendo posición en productos durante paginación

## Out of Scope

- Paginación en los endpoints de novedades y ofertas (se asume datasets pequeños)
- Configuración administrativa de los productos en novedades/ofertas
- Personalización de velocidad de carrusel por empresa
- Lazy loading de imágenes en carruseles (usar sistema existente)
- Animaciones avanzadas de transición entre productos

## Expected Deliverable

1. **Carruseles Funcionales** - Secciones de Novedades y Ofertas visibles debajo de la búsqueda con rotación automática de 3 segundos
2. **Header Sticky Operativo** - Header que se mantiene visible y se reduce al hacer scroll manteniendo funcionalidad completa
3. **Responsive Design Completo** - Carruseles que muestran 4 productos en desktop y 1 en mobile con controles apropiados para cada dispositivo