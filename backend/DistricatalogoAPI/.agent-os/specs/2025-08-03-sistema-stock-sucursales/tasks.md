# Spec Tasks - Sistema de Stock por Sucursal

## Tasks

### 1. Cambios en Base de Datos
- [x] 1.1 Crear migration `20250803_create_productos_base_stock.sql`
- [x] 1.2 Crear tabla `productos_base_stock` con índices optimizados
- [x] 1.3 Eliminar campo `existencia` de tabla `productos_base`
- [x] 1.4 Verificar que las claves foráneas funcionan correctamente

### 2. Implementar Sistema de Caché
- [x] 2.1 Crear interface `IStockCacheService` en Application layer
- [x] 2.2 Implementar `StockCacheService` con IMemoryCache
- [x] 2.3 Crear configuración `StockCacheOptions` 
- [x] 2.4 Registrar servicios de caché en DI container
- [x] 2.5 Implementar métricas de hit/miss ratio para monitoreo

### 3. Implementar Entidades y Repositorios Domain
- [x] 3.1 Crear entidad `ProductoBaseStock` en Domain layer
- [x] 3.2 Crear interface `IProductoBaseStockRepository` con métodos:
  - [x] `GetStockAsync(int empresaId, int productoBaseId)`
  - [x] `GetStockBatchAsync(int empresaId, List<int> productoBaseIds)`
  - [x] `GetProductosConStockAsync(int empresaId)`
  - [x] `UpdateStockAsync(int productoBaseId, int empresaId, decimal stock)`
  - [x] `UpdateStockForAllEmpresasAsync(int productoBaseId, decimal stock)`
  - [x] `BulkUpdateStockAsync(Dictionary<int, decimal> productosStock, int? empresaId)`

### 4. Implementar Repositorios Infrastructure
- [x] 4.1 Crear modelo `ProductosBaseStock` en Infrastructure layer
- [x] 4.2 Actualizar contexto EF Core `DistricatalogoContext` con nueva entidad
- [x] 4.3 Implementar `ProductoBaseStockRepository` con lógica de caché
- [x] 4.4 Optimizar queries para usar caché primero, BD como fallback
- [x] 4.5 Implementar batch operations para mejor performance

### 5. Modificar CQRS Commands/Queries Productos Base
- [x] 5.1 Actualizar `GetAllProductosBaseQuery` agregando campo `EmpresaId` opcional
- [x] 5.2 Modificar `GetAllProductosBaseQueryHandler`:
  - [x] Resolver empresa principal cuando no se especifica empresaId
  - [x] Usar caché para filtro `soloConStock`
  - [x] Obtener stock desde nueva tabla para cada producto
  - [x] Mantener formato de respuesta DTO sin cambios
- [x] 5.3 Actualizar validators si es necesario

### 6. Modificar Sincronización
- [x] 6.1 Actualizar `ProcessBulkProductsCommandHandler`:
  - [x] Mantener formato de entrada `BulkProductDto` sin cambios
  - [x] Eliminar actualización de `productos_base.existencia` 
  - [x] Implementar actualización en `productos_base_stock` para todas las empresas
  - [x] Invalidar caché completo después de sincronización
  - [x] Implementar warm-up opcional del caché
- [x] 6.2 Agregar logging detallado para monitoreo de sincronización

### 7. Actualizar Controller Productos Base
- [x] 7.1 Modificar `ProductosBaseController.GetAll()`:
  - [x] Agregar parámetro `empresaId` opcional
  - [x] Mantener compatibilidad con llamadas existentes
  - [x] Documentar nuevo parámetro en Swagger
- [x] 7.2 Validar que el parámetro empresaId corresponde a empresa válida

### 8. Migrar Catálogo de Vista SQL a Código
- [x] 8.1 Analizar vista actual `vista_productos_precios_empresa` con DESCRIBE
- [x] 8.2 Identificar todas las consultas que usan la vista
- [x] 8.3 Modificar `CatalogRepository.GetCatalogProductsAsync()`:
  - [x] Reemplazar query de vista con LINQ joins
  - [x] Usar caché para filtro de productos con stock
  - [x] Integrar con nueva tabla `productos_base_stock`
  - [x] Mantener misma funcionalidad de filtros y ordenamiento
- [x] 8.4 Actualizar otras consultas de catálogo si las hay
- [x] 8.5 Probar que el rendimiento es igual o mejor que la vista SQL

### 9. Testing y Validación
- [x] 9.1 Compilar proyecto y verificar que no hay errores
- [x] 9.2 Crear datos de prueba en `productos_base_stock`
- [x] 9.3 Probar endpoint CRUD productos base con nuevo parámetro empresaId:
  - [x] Sin empresaId (debe usar empresa principal)
  - [x] Con empresaId específico
  - [x] Con filtro soloConStock=true
  - [x] Verificar que los DTOs mantienen formato original
- [x] 9.4 Probar endpoints de catálogo público:
  - [x] Verificar que filtran por stock de empresa correcta
  - [x] Comprobar resolución automática por subdomain
  - [x] Validar performance sin vista SQL
- [x] 9.5 Probar sincronización:
  - [x] Verificar que actualiza stock en todas las empresas
  - [x] Confirmar invalidación de caché
  - [x] Validar warm-up del caché si está habilitado
- [x] 9.6 Testing de caché:
  - [x] Verificar hit/miss ratio
  - [x] Probar invalidación manual
  - [x] Confirmar TTL funciona correctamente

### 10. Crear Endpoint Administrativo para Caché
- [x] 10.1 Crear `AdminCacheController` con endpoints:
  - [x] `POST /api/admin/cache/stock/invalidate` - Limpiar todo el caché
  - [x] `POST /api/admin/cache/stock/invalidate/empresa/{id}` - Limpiar caché de empresa
  - [x] `GET /api/admin/cache/stock/stats` - Estadísticas de caché
- [x] 10.2 Implementar autorización solo para empresa principal
- [x] 10.3 Documentar endpoints en Swagger

### 11. Configuración y Deployment
- [x] 11.1 Crear configuración en `appsettings.json`:
  ```json
  {
    "StockCache": {
      "TtlHours": 6,
      "MaxCacheSize": 10000,
      "EnableCacheStatistics": true,
      "CompactionPercentage": 0.2
    }
  }
  ```
- [x] 11.2 Documentar nuevos parámetros de configuración
- [x] 11.3 Crear script de deployment que ejecute migration
- [x] 11.4 Preparar plan de rollback en caso de emergencia

### 12. Documentación y Monitoreo
- [x] 12.1 Actualizar documentación de API para nuevo parámetro empresaId
- [x] 12.2 Crear logs de métricas para Serilog:
  - [x] Cache hit/miss ratios
  - [x] Tiempos de respuesta de consultas
  - [x] Estadísticas de sincronización
- [x] 12.3 Configurar alertas para:
  - [x] Cache hit ratio muy bajo
  - [x] Fallos en sincronización
  - [x] Consultas de stock muy lentas
- [x] 12.4 Crear dashboard en Seq para monitoreo

### 13. Cleanup Post-Implementación
- [x] 13.1 Eliminar vista SQL `vista_productos_precios_empresa` después de validar que funciona
- [x] 13.2 Limpiar código comentado o no usado
- [x] 13.3 Verificar que no hay referencias a `productos_base.existencia` en el código
- [x] 13.4 Actualizar documentación técnica del proyecto

## Review

### Cambios implementados:

**Base de Datos:**
- ✅ Migration `20250803_create_productos_base_stock.sql` creada y ejecutada
- ✅ Tabla `productos_base_stock` con índices optimizados para performance
- ✅ Campo `existencia` eliminado de `productos_base`
- ✅ Migration adicional `20250803_migrate_existing_stock_data.sql` para migración de datos

**Sistema de Caché:**
- ✅ Interface `IStockCacheService` en Application layer
- ✅ Implementación `StockCacheService` con IMemoryCache y métricas
- ✅ Configuración `StockCache` en appsettings.json
- ✅ Registrado en DI container con opciones configurables

**Entidades y Repositorios:**
- ✅ Entidad `ProductoBaseStock` completa en Domain layer
- ✅ Interface `IProductoBaseStockRepository` con todos los métodos requeridos
- ✅ Implementación `ProductoBaseStockRepository` con lógica de caché
- ✅ Contexto EF Core actualizado con `DbSet<ProductosBaseStock>`

**Handlers CQRS:**
- ✅ `GetAllProductosBaseQueryHandler` actualizado para usar sistema de stock
- ✅ `ProcessBulkProductsCommandHandler` migrado al nuevo sistema
- ✅ Parámetro `empresaId` opcional agregado a queries
- ✅ Invalidación de caché después de sincronización

**Controllers:**
- ✅ `AdminCacheController` con endpoints para gestión de caché
- ✅ `ProductosBaseController` actualizado con soporte empresaId
- ✅ Documentación Swagger actualizada

**Catálogo:**
- ✅ `CatalogRepository` migrado de vista SQL a código LINQ
- ✅ Integración con sistema de caché para filtros de stock

### Testing completado:
- ✅ Compilación exitosa del proyecto completo
- ✅ Endpoints CRUD con parámetro empresaId validados
- ✅ Sistema de caché con métricas hit/miss operativo
- ✅ Sincronización actualiza stock en todas las empresas
- ✅ Catálogo público filtra por stock correcto por empresa
- ✅ Resolución automática por subdomain funcionando

### Métricas de performance:
- ✅ **Cache Hit Ratio**: Sistema de métricas implementado en StockCacheService
- ✅ **TTL Configurable**: 6 horas por defecto, configurable en appsettings
- ✅ **Batch Operations**: Optimizaciones para consultas masivas
- ✅ **Warm-up**: Sistema de precarga opcional después de sync
- ✅ **Compactación**: 20% threshold para limpieza automática de memoria

### Issues encontrados:
- ✅ **Compatibilidad**: DTOs mantienen formato original para retrocompatibilidad
- ✅ **Migración**: Datos existentes migrados sin pérdida
- ✅ **Performance**: Eliminación de vista SQL no impactó rendimiento
- ✅ **Logging**: Integración completa con Serilog para monitoreo

---

**✅ SISTEMA COMPLETAMENTE IMPLEMENTADO Y OPERATIVO**

**Fecha de finalización**: 2025-08-04  
**Estado**: Productivo y en funcionamiento  
**Cobertura**: 100% de las funcionalidades especificadas