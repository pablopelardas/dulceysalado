# Sistema de Stock por Sucursal - Especificación Detallada

## Situación Actual

### Estructura de Stock
- `productos_base.existencia` - Campo único de stock en productos base
- `productos_empresa.existencia` - Campo único de stock en productos empresa (NO USADO)
- Vista `vista_productos_precios_empresa` - Filtra productos con `pb.existencia > 0` y `pe.existencia > 0`

### Problemas Identificados
1. **Una sola cantidad de stock** para múltiples sucursales/empresas
2. **Filtros inadecuados** para consultas por empresa específica
3. **Vista SQL compleja** difícil de mantener desde código
4. **Frontend sin control** sobre qué empresa consultar para stock

## Solución Propuesta

### Nueva Estructura de Stock

#### Tabla `productos_base_stock`
```sql
CREATE TABLE productos_base_stock (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_id INT NOT NULL,
    producto_base_id INT NOT NULL,
    existencia DECIMAL(10,3) NOT NULL DEFAULT 0.000,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    UNIQUE KEY uk_empresa_producto (empresa_id, producto_base_id),
    INDEX idx_empresa (empresa_id),
    INDEX idx_producto (producto_base_id),
    INDEX idx_existencia (existencia),
    
    FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE,
    FOREIGN KEY (producto_base_id) REFERENCES productos_base(id) ON DELETE CASCADE
);
```

## Sistema de Caché

### Estrategia de Caching
Dado que precios y stock **solo cambian durante sincronización diaria** (normalmente por la mañana):

#### Caché de Stock por Empresa
```csharp
// Key: "stock:empresa:{empresaId}:producto:{productoId}"
// Value: decimal (existencia)
// TTL: 6 horas (se renueva en sincronización)
```

#### Caché de Productos con Stock
```csharp
// Key: "productos_con_stock:empresa:{empresaId}"
// Value: List<int> (IDs de productos con stock > 0)
// TTL: 6 horas
```

#### Invalidación de Caché
1. **Durante sincronización**: Limpiar todo el caché de stock
2. **Tiempo**: TTL de 6 horas como backup
3. **Manual**: Endpoint administrativo para limpiar caché

### Implementación del Caché

#### IMemoryCache Service
```csharp
public interface IStockCacheService
{
    Task<decimal?> GetStockAsync(int empresaId, int productoId);
    Task SetStockAsync(int empresaId, int productoId, decimal stock);
    Task<List<int>> GetProductosConStockAsync(int empresaId);
    Task SetProductosConStockAsync(int empresaId, List<int> productoIds);
    Task InvalidateStockCacheAsync();
    Task InvalidateEmpresaCacheAsync(int empresaId);
}
```

## Cambios en la API

### 1. CRUD Productos Base

#### Endpoint Actual
```
GET /api/productos-base?page=1&pageSize=20&soloConStock=false
```

#### Endpoint Modificado
```
GET /api/productos-base?page=1&pageSize=20&soloConStock=false&empresaId=5
```

**Parámetros:**
- `empresaId` (opcional): ID de empresa para filtrar stock. Si no se envía, usa empresa principal
- `soloConStock` (opcional): Filtra solo productos con stock > 0 de la empresa especificada

**Flujo con Caché:**
1. Si `soloConStock=true`: Consultar caché de productos con stock
2. Para cada producto: Obtener stock desde caché
3. Si no existe en caché: Consultar BD y cachear resultado

### 2. Catálogo Público

#### Vista SQL Actual: `vista_productos_precios_empresa`
- Filtra: `pb.existencia > 0`
- Une productos base y empresa en una consulta compleja

#### Propuesta: Migrar a Código C# con Caché
- Eliminar vista SQL
- Implementar consultas LINQ en `CatalogRepository`
- **Usar caché** para filtrar productos con stock
- Join con `productos_base_stock` solo si no está en caché

### 3. Sincronización

#### Endpoint Actual
```csharp
public class BulkProductDto 
{
    public decimal? Existencia { get; set; }
    // ... otros campos
}
```

#### Endpoint Modificado con Caché
1. Procesar productos normalmente
2. Actualizar `productos_base_stock` para todas las empresas
3. **Invalidar todo el caché de stock**
4. **Pre-cargar caché** con nuevos valores (opcional, warm-up)

## Puntos de Implementación

### 1. Modificaciones en Base de Datos
- [x] Crear tabla `productos_base_stock`
- [x] Eliminar campo `existencia` de `productos_base`
- [x] Mantener campo `existencia` en `productos_empresa` (sin usar)

### 2. Sistema de Caché
- [x] Implementar `IStockCacheService`
- [x] Configurar IMemoryCache con políticas apropiadas
- [x] Métricas de hit/miss de caché para monitoreo

### 3. Modificaciones en Domain/Infrastructure
- [x] Crear entidad `ProductoBaseStock`
- [x] Crear interfaz `IProductoBaseStockRepository`
- [x] Implementar repositorio con métodos que usen caché

### 4. Modificaciones en Application
- [x] Actualizar handlers para usar caché primero
- [x] Invalidación de caché en handlers de sincronización

### 5. Modificaciones en API
- [x] Actualizar controladores para usar nuevo sistema
- [x] Endpoint administrativo para limpiar caché manualmente

## Consideraciones Técnicas

### Performance
- **Caché en memoria** para acceso ultra-rápido
- **TTL de 6 horas** como backup (sincronización es daily)
- **Warm-up** opcional después de sincronización

### Configuración de Caché
```json
{
  "Cache": {
    "Stock": {
      "DefaultTTLHours": 6,
      "MaxMemoryMB": 100,
      "WarmUpAfterSync": true
    }
  }
}
```

### Monitoreo
- **Métricas** de hit ratio del caché
- **Logs** de invalidación de caché
- **Alertas** si caché tiene demasiados misses

### Compatibilidad
- **DTOs sin cambios** - `existencia` field mantiene compatibilidad
- **Endpoints actuales** - Funcionan sin empresa_id (usa empresa principal)
- **Fallback a BD** - Si caché falla, consulta directa

## Beneficios del Caché

### Performance
- **Consultas ultra-rápidas** - Stock desde memoria
- **Reducción de carga en BD** - Especialmente para catálogo público
- **Filtros eficientes** - Lista pre-computada de productos con stock

### Escalabilidad
- **Múltiples sucursales** sin impacto en performance
- **Catálogo público optimizado** - Sin joins complejos en cada consulta

## Fases Futuras

### Fase 2: Caché Distribuido
- Redis para ambiente multi-instancia
- Sincronización de invalidación entre instancias

### Fase 3: Gestión Independiente por Sucursal
- API para actualización manual de stock (invalida caché)
- Dashboard de stock por sucursal
- Reportes en tiempo real