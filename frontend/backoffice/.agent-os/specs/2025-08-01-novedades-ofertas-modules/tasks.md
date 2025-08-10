# Spec Tasks

Estas son las tareas a completar para la especificación detallada en @.agent-os/specs/2025-08-01-novedades-ofertas-modules/spec.md

> Creado: 2025-08-01
> Estado: Listo para Implementación

## Tasks

- [x] 1. Crear Tipos TypeScript y Composables Base
  - [x] 1.1 Crear types/marketing.ts con interfaces NovedadesResponse, OfertasResponse
  - [x] 1.2 Crear composables/useNovedades.ts basado en useAgrupacionesCrud.ts
  - [x] 1.3 Crear composables/useOfertas.ts basado en useAgrupacionesCrud.ts
  - [x] 1.4 Implementar manejo de estados loading/error en composables
  - [x] 1.5 Verificar integración con endpoints API existentes

- [x] 2. Adaptar Componente DragDrop para Reutilización
  - [x] 2.1 Analizar AgrupacionesDragDrop.vue para identificar props necesarias
  - [x] 2.2 Modificar componente para aceptar props de configuración (endpoint, título)
  - [x] 2.3 Crear variantes NovedadesDragDrop y OfertasDragDrop como wrappers
  - [x] 2.4 Mantener funcionalidad existente sin romper módulo de agrupaciones
  - [x] 2.5 Verificar comportamiento drag-and-drop funciona correctamente

- [x] 3. Integrar Cards en Dashboard Principal
  - [x] 3.1 Analizar estructura actual de pages/index.vue
  - [x] 3.2 Crear cards "Gestionar Novedades" y "Gestionar Ofertas" 
  - [x] 3.3 Implementar lógica de permisos para mostrar/ocultar cards
  - [x] 3.4 Configurar navegación a páginas de marketing correspondientes
  - [x] 3.5 Verificar responsive design y consistencia visual

- [x] 4. Crear Páginas de Marketing
  - [x] 4.1 Crear pages/admin/marketing/novedades/index.vue
  - [x] 4.2 Crear pages/admin/marketing/ofertas/index.vue
  - [x] 4.3 Implementar selector de empresa (solo empresa principal)
  - [x] 4.4 Integrar componentes drag-drop configurados
  - [x] 4.5 Configurar middleware de autenticación y permisos
  - [x] 4.6 Implementar breadcrumbs y navegación coherente
  - [x] 4.7 Verificar funcionalidad completa en ambas páginas

- [ ] 5. Testing y Refinamiento Final
  - [ ] 5.1 Probar flujo completo empresa principal → todas las empresas
  - [ ] 5.2 Probar flujo empresa cliente → solo su empresa
  - [ ] 5.3 Verificar reutilización sin afectar módulo agrupaciones existente
  - [ ] 5.4 Validar responsive design en dispositivos móviles
  - [ ] 5.5 Confirmar integración API funciona correctamente
  - [ ] 5.6 Realizar pruebas de regresión en funcionalidad existente
  - [ ] 5.7 Documentar cualquier cambio o consideración especial