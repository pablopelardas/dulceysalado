# Spec Requirements Document

> Spec: Módulos de Novedades y Ofertas
> Created: 2025-08-01
> Status: Planning

## Overview

Implementar dos módulos de marketing (Novedades y Ofertas) que permitan a las empresas configurar qué agrupaciones aparecen en los endpoints públicos de novedades y ofertas. Los módulos reutilizarán la funcionalidad existente del sistema de agrupaciones con interfaz drag-and-drop para configuración por empresa.

## User Stories

### Administrador de Empresa Principal - Configuración de Novedades

Como administrador de empresa principal, quiero configurar qué agrupaciones aparecen como novedades para cada empresa cliente, para que pueda controlar centralizadamente el contenido promocional de todas las sucursales.

El administrador accede a la nueva sección "Marketing" → "Novedades", selecciona una empresa cliente del dropdown, y usa la interfaz drag-and-drop para mover agrupaciones entre "Disponibles" y "Seleccionadas como Novedades". Los cambios se guardan automáticamente al hacer drop.

### Administrador de Empresa Cliente - Gestión de Ofertas

Como administrador de empresa cliente, quiero configurar qué agrupaciones de mi empresa aparecen como ofertas, para que pueda destacar productos específicos en mi catálogo sin depender de la empresa principal.

El administrador accede a "Marketing" → "Ofertas", ve automáticamente las agrupaciones de su empresa, y configura mediante drag-and-drop cuáles aparecen en el endpoint /api/ofertas para su empresa.

## Spec Scope

1. **Módulo de Novedades** - Sistema drag-and-drop para configurar agrupaciones que aparecen en endpoint de novedades por empresa
2. **Módulo de Ofertas** - Sistema drag-and-drop para configurar agrupaciones que aparecen en endpoint de ofertas por empresa  
3. **Reutilización de Componentes** - Adaptar AgrupacionesDragDrop.vue existente para trabajar con los nuevos endpoints
4. **Cards en Dashboard** - Nuevas cards en dashboard principal para acceder a módulos de marketing
5. **Integración con API** - Consumir endpoints GET/PUT para novedades y ofertas con misma lógica que agrupaciones

## Out of Scope

- Modificación de la lógica del backend o endpoints existente
- Creación de flags o campos adicionales en base de datos  
- Gestión de vigencia o duración de promociones
- Analytics o métricas de performance de novedades/ofertas
- Personalización visual específica para módulos de marketing

## Expected Deliverable

1. Página funcional /admin/marketing/novedades con drag-and-drop para configurar novedades por empresa
2. Página funcional /admin/marketing/ofertas con drag-and-drop para configurar ofertas por empresa
3. Cards de acceso rápido en dashboard principal para gestionar novedades y ofertas con permisos adecuados

## Spec Documentation

- Tasks: @.agent-os/specs/2025-08-01-novedades-ofertas-modules/tasks.md
- Technical Specification: @.agent-os/specs/2025-08-01-novedades-ofertas-modules/sub-specs/technical-spec.md
- API Specification: @.agent-os/specs/2025-08-01-novedades-ofertas-modules/sub-specs/api-spec.md