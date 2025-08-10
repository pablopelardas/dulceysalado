# Spec Tasks

## Tasks

- [ ] 1. Implementar servicios de API para Novedades y Ofertas
  - [ ] 1.1 Escribir tests para getNovedades() y getOfertas() en catalogService
  - [ ] 1.2 Implementar métodos getNovedades() y getOfertas() en src/services/catalog.ts
  - [ ] 1.3 Añadir sistema de caché básico con TTL de 5 minutos
  - [ ] 1.4 Implementar manejo de errores y fallbacks graceful
  - [ ] 1.5 Verificar que todos los tests pasan

- [ ] 2. Crear componentes base de carrusel
  - [ ] 2.1 Escribir tests para ProductCarousel.vue y CarouselCard.vue
  - [ ] 2.2 Crear componente CarouselCard.vue con diseño destacado
  - [ ] 2.3 Crear componente ProductCarousel.vue con lógica de rotación
  - [ ] 2.4 Implementar controles manuales (flechas izquierda/derecha)
  - [ ] 2.5 Añadir funcionalidad de pausa en hover y interacciones
  - [ ] 2.6 Verificar que todos los tests pasan

- [ ] 3. Implementar funcionalidad responsive y touch
  - [ ] 3.1 Escribir tests para comportamiento responsive (4 desktop, 1 mobile)
  - [ ] 3.2 Implementar breakpoints responsive con Tailwind CSS
  - [ ] 3.3 Añadir soporte touch/swipe para dispositivos móviles
  - [ ] 3.4 Optimizar performance con Intersection Observer
  - [ ] 3.5 Verificar que todos los tests pasan

- [ ] 4. Crear header sticky con transición suave
  - [ ] 4.1 Escribir tests para composable useScrollHeader
  - [ ] 4.2 Crear composable useScrollHeader.ts para detectar scroll
  - [ ] 4.3 Implementar transición CSS suave a header compacto (80px)
  - [ ] 4.4 Modificar AppHeader.vue para usar funcionalidad sticky
  - [ ] 4.5 Asegurar z-index correcto y no interferencia con contenido
  - [ ] 4.6 Verificar que todos los tests pasan

- [ ] 5. Integrar carruseles en vista principal del catálogo
  - [ ] 5.1 Escribir tests para integración de carruseles en vista Catalog
  - [ ] 5.2 Modificar vista Catalog.vue para incluir secciones de carrusel
  - [ ] 5.3 Posicionar carruseles entre SearchBar y lista de categorías
  - [ ] 5.4 Implementar loading states con skeleton loaders
  - [ ] 5.5 Añadir error boundaries para manejo graceful de fallos
  - [ ] 5.6 Verificar que todos los tests pasan y la integración es fluida

- [ ] 6. Implementar navegación inteligente por secciones
  - [ ] 6.1 Escribir tests para funcionalidad de scroll inteligente en limpieza de filtros
  - [ ] 6.2 Crear referencias a elementos de categorías usando template refs
  - [ ] 6.3 Implementar lógica de scroll automático solo al limpiar filtros de categorías
  - [ ] 6.4 Preservar posición de scroll durante paginación normal de productos
  - [ ] 6.5 Configurar estado inicial del header como compacto solo en limpieza de filtros
  - [ ] 6.6 Verificar que todos los tests pasan y el comportamiento es intuitivo