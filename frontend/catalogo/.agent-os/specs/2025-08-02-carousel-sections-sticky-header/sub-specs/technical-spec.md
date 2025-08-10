# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-08-02-carousel-sections-sticky-header/spec.md

## Technical Requirements

### Carousel Components
- **Crear componente base `ProductCarousel.vue`** con slots para títulos y configuración de productos por vista
- **Crear componente `CarouselCard.vue`** con diseño destacado (sombras más pronunciadas, bordes, colores de accent)
- **Implementar auto-rotation timer** con `setInterval` que se pausa en hover y se reinicia en mouseleave
- **Controles de navegación** con flechas izquierda/derecha usando Hero Icons
- **Touch/Swipe support** para mobile usando eventos touch nativos o biblioteca existente
- **Responsive breakpoints** que cambie de 4 productos (desktop) a 1 producto (mobile) automáticamente

### Sticky Header
- **Implementar composable `useScrollHeader.ts`** que detecte scroll y aplique clases CSS
- **Transición suave** entre header normal y compacto usando CSS transitions
- **Altura fija de 80px** para header compacto manteniendo proporciones de logo y búsqueda
- **Z-index apropiado** para mantener header por encima de contenido
- **Estado de scroll persistente** que no cause reflow excesivo en la página

### API Integration  
- **Crear servicios** `getNovidades()` y `getOfertas()` en el archivo de servicios existente
- **Implementar caché básico** usando Map o localStorage para evitar llamadas repetidas
- **Manejo de errores** con fallback a array vacío si el endpoint falla
- **Tipos TypeScript** para la respuesta de productos que ya existe en el proyecto

### Layout Integration
- **Modificar vista principal** para incluir carruseles entre SearchBar y categorías
- **Grid layout responsive** que mantenga coherencia con el diseño existente  
- **Loading states** con skeleton loaders similares a los del catálogo principal
- **Error boundaries** para manejar fallas de los carruseles sin romper la página

### Smart Navigation
- **Implementar scroll inteligente** que detecta solo limpieza de filtros de categorías y posiciona en sección de categorías
- **Referencias de elementos** usando `ref` para obtener posición de sección de categorías
- **Scroll programático** con `scrollTo()` o `scrollIntoView()` solo al limpiar filtros de categorías
- **Preservar posición de scroll** durante paginación normal de productos
- **Estado inicial del header** configurado como compacto cuando se navega directamente a categorías por limpieza de filtros
- **Preservar comportamiento normal** en primera carga y cambios de página

### Performance Optimizations
- **Intersection Observer** para pausar carruseles que no están en viewport
- **Cleanup de timers** en unmount de componentes para evitar memory leaks
- **Debounce en resize events** para optimizar recálculos responsive
- **Cache con TTL** de 5 minutos para datos de novedades y ofertas

## External Dependencies

No se requieren nuevas dependencias externas. El proyecto utilizará:
- **Vue 3 Composition API** existente para lógica de componentes
- **Tailwind CSS** existente para estilos responsive y animaciones
- **Hero Icons** existente para controles de navegación
- **Pinia store** existente para manejo de estado si es necesario
- **API service layer** existente para llamadas HTTP