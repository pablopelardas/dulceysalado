# Spec Requirements Document

> Spec: Stock Diferencial por Empresa
> Created: 2025-08-04
> Status: Planning

## Overview

Implementar la funcionalidad frontend para permitir que la empresa principal (Hub) pueda gestionar el stock diferencial de productos base por empresa cliente. Esta funcionalidad permitirá personalizar la disponibilidad de productos para cada empresa del ecosistema Hub-and-Spoke sin modificar el producto base.

El backend ya cuenta con la API completa para esta funcionalidad, solo requiere pasar el parámetro `empresaId` en las operaciones GET y PUT de stock.

## User Stories

**Como administrador de la empresa principal:**
- Quiero poder seleccionar una empresa específica al gestionar productos base
- Quiero visualizar el stock diferencial de un producto para una empresa específica
- Quiero poder editar el stock diferencial de productos por empresa
- Quiero que los cambios de stock diferencial no afecten el producto base ni otras empresas

**Como sistema:**
- Debe mantener la integridad de los productos base
- Debe permitir stock específico por empresa sin modificar la estructura base
- Debe utilizar la API existente con el parámetro empresaId

## Spec Scope

**Frontend (Solo):**
- Modificar CRUD de productos base para incluir selector de empresas
- Crear/modificar composables para manejo de stock diferencial
- Adaptar componentes existentes para mostrar stock por empresa
- Actualizar páginas de gestión de productos para incluir funcionalidad diferencial
- Integrar con API existente usando parámetro empresaId

**Funcionalidades Específicas:**
- Selector de empresa en interfaz de productos
- Visualización de stock diferencial por empresa seleccionada
- Edición de stock específico por empresa
- Indicadores visuales para diferenciar stock base vs diferencial

## Out of Scope

- Modificaciones de backend o API (ya implementado)
- Cambios en estructura de base de datos
- Nuevos endpoints de API
- Modificaciones en lógica de negocio del backend
- Gestión de permisos/roles (se asume configuración existente)

## Expected Deliverable

**Composables:**
- Composable para gestión de stock diferencial por empresa
- Extensión de composables existentes para incluir empresaId

**Componentes:**
- Selector de empresa para productos base
- Componente de stock diferencial editable
- Indicadores visuales de stock diferencial

**Páginas/Vistas:**
- Modificación de páginas CRUD productos para incluir selector empresa
- Vista de stock diferencial por empresa

**Integración API:**
- Integración con endpoints existentes usando parámetro empresaId
- Manejo de estados loading/error específicos

## Spec Documentation

- Tasks: @.agent-os/specs/2025-08-04-stock-diferencial-empresa/tasks.md
- Technical Specification: @.agent-os/specs/2025-08-04-stock-diferencial-empresa/sub-specs/technical-spec.md