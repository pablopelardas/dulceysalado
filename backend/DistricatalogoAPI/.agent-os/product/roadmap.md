# Product Roadmap

> Last Updated: 2025-08-01
> Version: 1.0.0
> Status: Planning

## Phase 0: Ya Completado (100%)

**Goal:** Establecer la plataforma base funcional con todas las características core
**Success Criteria:** API completamente funcional, multi-tenant operativo, sincronización GECOM activa

### Must-Have Features

- ✅ **Autenticación y Autorización JWT**: Sistema completo de usuarios con roles
- ✅ **Multi-tenant Architecture**: Gestión de empresas con divisiones y configuración personalizada
- ✅ **Sincronización GECOM**: Import automático de productos, categorías y listas de precios
- ✅ **CRUD Productos/Categorías Base**: Gestión completa con patrón CQRS
- ✅ **CRUD Productos/Categorías por Empresa**: Gestión específica por división
- ✅ **Catálogo Público**: API pública con resolución por subdominio
- ✅ **Sistema de Filtros**: Búsqueda por categoría, marca, precios
- ✅ **Middleware de Resolución**: CompanyResolutionMiddleware operativo
- ✅ **Clean Architecture**: Implementación completa con .NET 9.0
- ✅ **Logging y Monitoreo**: Serilog con dashboard Seq

## Phase 1: Sistema de Novedades y Ofertas (EN PROGRESO)

**Goal:** Implementar sistema de destacados usando agrupaciones de nivel 1
**Success Criteria:** Compradores pueden ver novedades y ofertas destacadas en catálogo público

### Must-Have Features

- 🔄 **Agrupaciones de Nivel 1**: Configuración ID 1 = Novedades, ID 2 = Ofertas
- 🔄 **Endpoints de Novedades**: `/api/catalog/novedades` con productos destacados
- 🔄 **Endpoints de Ofertas**: `/api/catalog/ofertas` con productos en promoción
- 🔄 **Gestión Administrativa**: CRUD para asignación de productos a agrupaciones
- 🔄 **Filtros Avanzados**: Integración de novedades/ofertas en filtros existentes

### Nice-to-Have Features

- 📋 **Dashboard de Novedades**: Panel administrativo para gestión visual
- 📋 **Programación de Ofertas**: Sistema de fechas de inicio/fin para ofertas
- 📋 **Métricas de Engagement**: Tracking de productos más consultados

## Phase 2: Optimizaciones y Analytics (PLANIFICADO)

**Goal:** Mejorar performance y agregar capacidades de análisis
**Success Criteria:** Catálogo responde en <200ms, dashboard de métricas operativo

### Must-Have Features

- 📋 **ProductCount Real**: Conteo dinámico de productos por categoría
- 📋 **Categorías Base Mejoradas**: Jerarquía completa en catálogo público
- 📋 **Cache Strategy**: Redis para consultas frecuentes del catálogo
- 📋 **Search Optimization**: Elasticsearch para búsquedas avanzadas
- 📋 **Analytics Dashboard**: Métricas de uso por división y productos

### Nice-to-Have Features

- 📋 **CDN Integration**: Cloudflare para imágenes de productos
- 📋 **API Rate Limiting**: Protección contra abuso del catálogo público
- 📋 **Advanced Filtering**: Filtros por rango de precios, disponibilidad

## Phase 3: Integrations y Ecosystem (FUTURO)

**Goal:** Expandir integraciones y capacidades del ecosistema
**Success Criteria:** Integración con CRM/ERP, notificaciones automatizadas

### Must-Have Features

- 📋 **CRM Integration**: Sincronización bidireccional con sistemas de ventas
- 📋 **Email Notifications**: Alertas de nuevos productos y ofertas
- 📋 **API Webhooks**: Notificaciones en tiempo real de cambios
- 📋 **Mobile API**: Endpoints optimizados para aplicaciones móviles

### Nice-to-Have Features

- 📋 **WhatsApp Integration**: Catálogo compartible vía WhatsApp Business
- 📋 **PDF Catalog Export**: Generación automática de catálogos PDF
- 📋 **Multi-language Support**: Internacionalización del catálogo

## Leyenda

- ✅ **Completado**: Característica implementada y en producción
- 🔄 **En Progreso**: Desarrollo activo en curso
- 📋 **Planificado**: En roadmap, pendiente de desarrollo

## Notas Técnicas

### Arquitectura Actual
- Clean Architecture completamente implementada
- CQRS con MediatR para separación de responsabilidades
- Repository Pattern para acceso a datos
- JWT Authentication con multi-tenant support

### Deuda Técnica Identificada
- ✅ ~~Bug de paginación de agrupaciones: Resuelto - pageSize incrementado a 100~~
- Optimización de consultas EF Core para catálogo público  
- Implementación de cache para consultas frecuentes
- Testing automatizado: Sin tests unitarios actualmente