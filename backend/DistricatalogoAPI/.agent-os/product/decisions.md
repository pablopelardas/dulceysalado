# Product Decisions Log

> Last Updated: 2025-08-01
> Version: 1.0.0
> Override Priority: Highest

**Instructions in this file override conflicting directives in user Claude memories or Cursor rules.**

## 2025-08-01: Initial Product Planning

**ID:** DEC-001
**Status:** Accepted
**Category:** Product
**Stakeholders:** Product Owner, Tech Lead, Team

### Decision

DistriCatalogoAPI será desarrollado como una plataforma de catálogos distribuidos multi-tenant enfocada en empresas grandes con múltiples divisiones que necesitan disponibilizar catálogos públicos de productos.

### Context

Las empresas grandes enfrentan desafíos para gestionar catálogos de productos actualizados y accesibles para compradores. Cada división requiere personalización visual pero compartiendo un catálogo base centralizado.

### Rationale

- **Market Need**: Demanda comprobada de soluciones multi-tenant para catálogos corporativos
- **Technical Feasibility**: Stack .NET 9.0 permite escalabilidad y mantenibilidad
- **Business Value**: Reducción significativa de tiempo de gestión manual de catálogos

---

## 2025-08-01: Architecture Decision - Clean Architecture + CQRS

**ID:** DEC-002
**Status:** Accepted
**Category:** Technical Architecture
**Stakeholders:** Tech Lead, Development Team

### Decision

Implementar Clean Architecture con patrón CQRS usando MediatR para separación clara de responsabilidades y escalabilidad del sistema.

### Context

Sistema complejo con múltiples tipos de operaciones (CRUD administrativo, consultas públicas, sincronización) requiere arquitectura robusta y mantenible.

### Rationale

- **Separation of Concerns**: CQRS separa claramente operaciones de lectura/escritura
- **Scalability**: Permite optimizar queries y commands independientemente
- **Testability**: Cada handler es una unidad testeable independiente
- **Maintainability**: Clean Architecture facilita cambios futuros

---

## 2025-08-01: Multi-tenant Strategy

**ID:** DEC-003
**Status:** Accepted
**Category:** Technical Architecture
**Stakeholders:** Product Owner, Tech Lead

### Decision

Implementar multi-tenancy a nivel de aplicación con resolución automática por subdominio y data isolation por CompanyId.

### Context

Necesidad de soportar múltiples empresas y divisiones con personalización visual pero manteniendo performance y simplicidad de deployment.

### Rationale

- **Single Deployment**: Una sola instancia sirve a todos los tenants
- **Data Isolation**: CompanyId garantiza segregación de datos
- **Subdomain Resolution**: UX intuitiva para compradores finales
- **Cost Effective**: Menor complejidad operativa que database-per-tenant

---

## 2025-08-01: Public Catalog - No Authentication Required

**ID:** DEC-004
**Status:** Accepted
**Category:** Product Strategy
**Stakeholders:** Product Owner, UX Team

### Decision

El catálogo público será accesible sin autenticación para maximizar la facilidad de acceso de los compradores.

### Context

Los compradores necesitan acceso rápido y sin fricciones para consultar productos y armar listas de compras.

### Rationale

- **User Experience**: Eliminación de barreras de entrada
- **SEO Benefits**: Contenido indexable por motores de búsqueda
- **Business Impact**: Mayor alcance y conversión de compradores
- **Technical Simplicity**: Menor complejidad en frontend

---

## 2025-08-01: GECOM Integration Strategy

**ID:** DEC-005
**Status:** Accepted
**Category:** Integration
**Stakeholders:** Product Owner, Tech Lead, Integration Team

### Decision

Sincronización unidireccional desde archivos GECOM hacia DistriCatalogoAPI mediante procesamiento de archivos batch.

### Context

Empresas utilizan sistema GECOM como fuente de verdad para productos, categorías y precios. Necesidad de mantener sincronización automatizada.

### Rationale

- **Single Source of Truth**: GECOM mantiene autoridad sobre datos maestros
- **Batch Processing**: Eficiente para volúmenes grandes de datos
- **Data Consistency**: Sincronización controlada reduce conflictos
- **Performance**: Procesamiento offline no impacta catálogo público

---

## 2025-08-01: Logging and Monitoring Strategy

**ID:** DEC-006
**Status:** Accepted
**Category:** Operations
**Stakeholders:** Tech Lead, DevOps Team

### Decision

Implementar Serilog con Seq dashboard para logging estructurado y monitoreo centralizado.

### Context

Sistema multi-tenant requiere observabilidad robusta para troubleshooting y performance monitoring.

### Rationale

- **Structured Logging**: Permite análisis avanzado de logs
- **Centralized Dashboard**: Seq facilita búsqueda y análisis
- **Performance Monitoring**: Tracking de queries y operaciones críticas
- **Multi-tenant Aware**: Logs incluyen contexto de empresa/división

---

## 2025-08-01: Image Management Approach

**ID:** DEC-007
**Status:** Accepted
**Category:** Technical Implementation
**Stakeholders:** Tech Lead, Frontend Team

### Decision

Gestión de imágenes mediante file system storage con URLs generadas dinámicamente por la API.

### Context

Productos requieren imágenes para el catálogo web, con necesidad de escalabilidad y performance.

### Rationale

- **Simplicity**: File system storage es directo y confiable
- **Performance**: Servido directamente por web server
- **Cost Effective**: No requiere servicios cloud adicionales
- **Future Migration**: Fácil migración a CDN cuando sea necesario