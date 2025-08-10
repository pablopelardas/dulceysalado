# Spec Requirements Document

> Spec: Sistema de Novedades y Ofertas
> Created: 2025-08-01
> Status: Planning

## Overview

Implementar un sistema de gestión de novedades y ofertas que permita a las empresas principales configurar qué productos destacar para cada división, utilizando agrupaciones de Grupo 1 de GECOM. El sistema proporcionará endpoints públicos no paginados para facilitar el cacheo en frontend y endpoints administrativos para la gestión por empresa.

## User Stories

### Administradores gestionan novedades y ofertas por división

Como administrador de la empresa principal, quiero poder seleccionar qué agrupaciones de Grupo 1 se mostrarán como novedades y ofertas para cada división/local, para destacar productos específicos según la estrategia comercial de cada ubicación.

El sistema debe permitir que una misma agrupación pueda estar tanto en novedades como en ofertas simultáneamente, y que cada división tenga una configuración independiente de productos destacados.

### Compradores acceden a productos destacados

Como comprador visitando el catálogo público, quiero acceder rápidamente a las novedades y ofertas de la empresa sin paginación, para poder ver inmediatamente los productos promocionados y tomar decisiones de compra más eficientes.

Los productos destacados deben seguir apareciendo en sus categorías normales, manteniendo la navegación tradicional del catálogo intacta.

## Spec Scope

1. **Extensión del modelo de agrupaciones** - Agregar campo `tipo` para diferenciar Grupo 1, 2 y 3
2. **Nuevas tablas de relación** - Crear `empresas_novedades` y `empresas_ofertas` para configuración por empresa
3. **Endpoints administrativos** - CRUD completo para gestión de novedades y ofertas por empresa principal
4. **Endpoints públicos del catálogo** - `/api/catalog/novedades` y `/api/catalog/ofertas` sin paginación
5. **Integración con sistema existente** - Los productos mantienen su visibilidad en categorías normales

## Out of Scope

- Modificación del sistema de sincronización GECOM (se mantiene el Grupo 3 actual)
- Cambios en los endpoints de catálogo existentes
- Sistema de fechas de vigencia para ofertas (implementación futura)
- Análiticas o métricas de engagement de productos destacados

## Expected Deliverable

1. Empresas principales pueden asignar agrupaciones de Grupo 1 como novedades/ofertas para cada división
2. Endpoints `/api/catalog/novedades` y `/api/catalog/ofertas` devuelven productos completos sin paginación
3. Sistema administrativo funcional similar al módulo de agrupaciones existente
4. Productos destacados mantienen su visibilidad en categorías y rubros normales