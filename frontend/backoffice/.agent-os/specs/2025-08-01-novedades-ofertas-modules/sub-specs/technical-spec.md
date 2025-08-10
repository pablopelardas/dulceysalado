# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-08-01-novedades-ofertas-modules/spec.md

> Created: 2025-08-01
> Version: 1.0.0

## Technical Requirements

### Frontend Components
- **Reutilización de AgrupacionesDragDrop.vue**: Modificar componente existente para aceptar props que configuren endpoints y títulos específicos
- **Nuevas páginas**: /admin/marketing/novedades e /admin/marketing/ofertas siguiendo el patrón de páginas existentes
- **Composables similares**: Crear useNovedades.ts y useOfertas.ts basados en useAgrupacionesCrud.ts
- **Dashboard Cards**: Agregar cards de "Gestionar Novedades" y "Gestionar Ofertas" al dashboard principal (pages/index.vue)

### API Integration
- **Integración API**: Consumir endpoints GET/PUT existentes usando mismo patrón de autenticación JWT
- **Middleware de permisos**: Reutilizar sistema existente, empresa principal gestiona todas las empresas
- **Tipos TypeScript**: Definir interfaces para NovedadesResponse y OfertasResponse siguiendo patrón existente

### UI/UX Requirements
- **Estados de loading**: Implementar mismos patrones de loading y error handling que módulo de agrupaciones
- **Responsive design**: Mantener consistencia con Nuxt UI y TailwindCSS del resto de la aplicación
- **Drag and Drop**: Mantener la misma experiencia de usuario del módulo de agrupaciones

## Approach

1. **Fase 1**: Crear estructura base de páginas y navegación
2. **Fase 2**: Adaptar componente AgrupacionesDragDrop.vue para ser reutilizable
3. **Fase 3**: Implementar composables específicos para novedades y ofertas
4. **Fase 4**: Integrar con endpoints de API y testing completo

## External Dependencies

- Nuxt 3 framework existente
- Nuxt UI component library
- TailwindCSS para estilos
- API REST endpoints (backend existente)
- JWT authentication system