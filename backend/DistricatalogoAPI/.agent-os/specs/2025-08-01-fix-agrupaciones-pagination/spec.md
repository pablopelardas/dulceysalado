# Spec Requirements Document

> Spec: Fix Agrupaciones Pagination
> Created: 2025-08-01
> Status: Planning

## Overview

Corregir el bug del endpoint de agrupaciones que limita los resultados a 20 elementos por defecto, causando que el frontend no muestre todas las agrupaciones disponibles. La solución es incrementar el tamaño de página predeterminado a 100 elementos para garantizar que todas las agrupaciones se devuelvan en una sola respuesta.

## User Stories

### Administradores visualizan todas las agrupaciones

Como administrador del sistema, quiero ver todas las agrupaciones disponibles en la interfaz de usuario sin tener que implementar paginación en el frontend, para poder gestionar y asignar productos a cualquier agrupación existente.

Actualmente el sistema devuelve solo 20 agrupaciones de un total de 28, lo que hace que 8 agrupaciones no sean visibles ni seleccionables en el frontend. El administrador necesita acceso completo a todas las agrupaciones para la correcta gestión de productos por división empresarial.

## Spec Scope

1. **Modificar endpoint de agrupaciones** - Cambiar el tamaño de página predeterminado de 20 a 100 elementos
2. **Mantener estructura de respuesta** - Conservar el formato actual con paginación para compatibilidad futura
3. **Validar respuesta completa** - Verificar que las 28 agrupaciones se devuelvan en una sola página

## Out of Scope

- Eliminar completamente la paginación del endpoint
- Modificar la estructura de respuesta JSON existente
- Implementar paginación en el frontend
- Cambiar otros endpoints que usan paginación

## Expected Deliverable

1. El endpoint GET /api/agrupaciones devuelve las 28 agrupaciones en una sola respuesta con page_size=100
2. La respuesta mantiene la estructura actual con información de paginación (total: 28, page: 1, page_size: 100, total_pages: 1)
3. El frontend muestra todas las agrupaciones disponibles sin necesidad de cambios