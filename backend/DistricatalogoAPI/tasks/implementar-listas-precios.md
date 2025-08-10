# Implementación de Múltiples Listas de Precios

## Análisis de la Situación Actual

### Estructura Actual
1. **productos_base**: Tiene un campo `precio` único (administrado por empresa principal)
2. **productos_empresa**: Tiene un campo `precio` único (administrado por empresa cliente)
3. Los catálogos de empresa muestran TODOS los productos base + sus productos propios
4. La sincronización desde GECOM actualiza solo el precio único

### Problema a Resolver
- Necesitamos soportar múltiples listas de precios (Lista 1, Lista 2, ..., Lista N)
- Cada producto puede tener diferentes precios en cada lista
- Las empresas cliente deben poder personalizar los precios de cada lista

## Diseño de la Nueva Arquitectura

### Nuevas Tablas

#### 1. `listas_precios`
```sql
CREATE TABLE listas_precios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(50) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion TEXT,
    activa BOOLEAN DEFAULT TRUE,
    es_predeterminada BOOLEAN DEFAULT FALSE,
    orden INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    UNIQUE KEY uk_codigo (codigo),
    INDEX idx_activa_orden (activa, orden)
);
```

#### 2. `productos_base_precios`
```sql
CREATE TABLE productos_base_precios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    producto_base_id INT NOT NULL,
    lista_precio_id INT NOT NULL,
    precio DECIMAL(10,3) NOT NULL DEFAULT 0.000,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    actualizado_gecom TIMESTAMP NULL,
    
    UNIQUE KEY uk_producto_lista (producto_base_id, lista_precio_id),
    INDEX idx_lista_precio (lista_precio_id),
    
    FOREIGN KEY (producto_base_id) REFERENCES productos_base(id) ON DELETE CASCADE,
    FOREIGN KEY (lista_precio_id) REFERENCES listas_precios(id) ON DELETE CASCADE
);
```

#### 3. `productos_empresa_precios`
```sql
CREATE TABLE productos_empresa_precios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_id INT NOT NULL,
    producto_id INT NOT NULL,
    tipo_producto ENUM('base', 'empresa') NOT NULL,
    lista_precio_id INT NOT NULL,
    precio_override DECIMAL(10,3) NULL, -- NULL = usa precio base
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    UNIQUE KEY uk_empresa_producto_lista (empresa_id, tipo_producto, producto_id, lista_precio_id),
    INDEX idx_empresa_lista (empresa_id, lista_precio_id),
    
    FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE,
    FOREIGN KEY (lista_precio_id) REFERENCES listas_precios(id) ON DELETE CASCADE
);
```

#### 4. Vista Unificada de Precios
```sql
CREATE VIEW vista_productos_precios_empresa AS
-- Productos base con sus precios por lista
SELECT 
    pb.id as producto_id,
    'base' as tipo_producto,
    pb.codigo,
    pb.descripcion,
    e.id as empresa_id,
    lp.id as lista_precio_id,
    lp.codigo as lista_codigo,
    lp.nombre as lista_nombre,
    COALESCE(pep.precio_override, pbp.precio, 0) as precio_final,
    CASE 
        WHEN pep.precio_override IS NOT NULL THEN TRUE 
        ELSE FALSE 
    END as precio_personalizado,
    pbp.actualizado_gecom
FROM productos_base pb
CROSS JOIN empresas e
CROSS JOIN listas_precios lp
LEFT JOIN productos_base_precios pbp ON pb.id = pbp.producto_base_id AND lp.id = pbp.lista_precio_id
LEFT JOIN productos_empresa_precios pep ON e.id = pep.empresa_id 
    AND pep.tipo_producto = 'base' 
    AND pb.id = pep.producto_id 
    AND lp.id = pep.lista_precio_id
WHERE e.tipo_empresa = 'cliente' 
    AND pb.visible = TRUE 
    AND lp.activa = TRUE

UNION ALL

-- Productos propios de empresa con sus precios por lista
SELECT 
    pe.id as producto_id,
    'empresa' as tipo_producto,
    pe.codigo,
    pe.descripcion,
    pe.empresa_id,
    lp.id as lista_precio_id,
    lp.codigo as lista_codigo,
    lp.nombre as lista_nombre,
    COALESCE(pep.precio_override, pe.precio, 0) as precio_final,
    TRUE as precio_personalizado,
    NULL as actualizado_gecom
FROM productos_empresa pe
CROSS JOIN listas_precios lp
LEFT JOIN productos_empresa_precios pep ON pe.empresa_id = pep.empresa_id 
    AND pep.tipo_producto = 'empresa' 
    AND pe.id = pep.producto_id 
    AND lp.id = pep.lista_precio_id
WHERE pe.visible = TRUE 
    AND lp.activa = TRUE;
```

## Plan de Migración

### Fase 1: Crear nuevas estructuras
1. Crear las nuevas tablas sin eliminar las columnas `precio` existentes
2. Crear lista de precios predeterminada (Lista 1)
3. Migrar precios actuales a la nueva estructura:
   ```sql
   -- Migrar precios de productos_base
   INSERT INTO productos_base_precios (producto_base_id, lista_precio_id, precio, actualizado_gecom)
   SELECT id, 1, precio, actualizado_gecom FROM productos_base WHERE precio IS NOT NULL;
   
   -- Migrar precios personalizados de productos_empresa
   INSERT INTO productos_empresa_precios (empresa_id, producto_id, tipo_producto, lista_precio_id, precio_override)
   SELECT empresa_id, id, 'empresa', 1, precio FROM productos_empresa WHERE precio IS NOT NULL;
   ```

### Fase 2: Actualizar código
1. Mantener compatibilidad hacia atrás temporalmente
2. Actualizar sincronización para soportar múltiples listas
3. Actualizar APIs para gestionar listas de precios
4. Actualizar catálogo público para usar la nueva estructura

### Fase 3: Deprecar estructura antigua
1. Eliminar columnas `precio` de las tablas originales
2. Actualizar vistas y consultas que dependan de ellas

## Cambios en la API

### 1. Sincronización (SyncController)

#### Modificar `BulkProductDto`:
```csharp
public class BulkProductDto
{
    public string Codigo { get; set; }
    public string Descripcion { get; set; }
    // ... otros campos existentes ...
    
    // Reemplazar precio único por lista de precios
    public List<PrecioListaDto> Precios { get; set; } = new();
}

public class PrecioListaDto
{
    public string ListaCodigo { get; set; } // "LISTA1", "LISTA2", etc.
    public decimal Precio { get; set; }
}
```

### 2. Nuevos Endpoints de Gestión

#### ListasPreciosController
- `GET /api/listas-precios` - Listar todas las listas
- `POST /api/listas-precios` - Crear nueva lista
- `PUT /api/listas-precios/{id}` - Actualizar lista
- `DELETE /api/listas-precios/{id}` - Eliminar lista (si no tiene precios)

#### ProductosBaseController (modificaciones)
- `GET /api/productos-base/{id}/precios` - Obtener todos los precios del producto
- `PUT /api/productos-base/{id}/precios/{listaId}` - Actualizar precio en lista específica
- `POST /api/productos-base/{id}/precios/bulk` - Actualizar múltiples precios

#### ProductosEmpresaController (modificaciones)
- `GET /api/productos-empresa/precios` - Obtener precios personalizados de la empresa
- `PUT /api/productos-empresa/precios` - Actualizar precios personalizados (bulk)

### 3. Catálogo Público (CatalogController)

Modificar respuestas para incluir precios por lista:
```json
{
  "id": 123,
  "codigo": "PROD001",
  "descripcion": "Producto ejemplo",
  "precios": [
    {
      "lista_id": 1,
      "lista_codigo": "LISTA1",
      "lista_nombre": "Lista General",
      "precio": 100.50,
      "precio_personalizado": false
    },
    {
      "lista_id": 2,
      "lista_codigo": "LISTA2", 
      "lista_nombre": "Lista Mayorista",
      "precio": 85.00,
      "precio_personalizado": true
    }
  ]
}
```

## Consideraciones Técnicas

### Performance
1. Índices optimizados para consultas frecuentes
2. Cacheo de precios en Redis para catálogo público
3. Bulk updates para sincronización eficiente

### Seguridad
1. Solo empresas principales pueden crear/modificar listas
2. Empresas cliente solo pueden personalizar precios, no crear listas
3. Validación de permisos en cada operación

### Compatibilidad
1. Mantener endpoints actuales durante transición
2. Agregar header `X-API-Version` para nueva versión
3. Documentar deprecación en Swagger

## Tareas de Implementación

### Backend
1. [ ] Crear scripts SQL para nuevas tablas
2. [ ] Actualizar modelos Entity Framework
3. [ ] Crear repositorios para listas de precios
4. [ ] Modificar handlers de sincronización
5. [ ] Crear nuevos endpoints de gestión
6. [ ] Actualizar catálogo público
7. [ ] Implementar migración de datos
8. [ ] Agregar tests unitarios e integración

### Frontend (si aplica)
1. [ ] UI para gestión de listas de precios
2. [ ] Actualizar vistas de productos para mostrar múltiples precios
3. [ ] Selector de lista en catálogo público

### DevOps
1. [ ] Scripts de migración para producción
2. [ ] Backup antes de migración
3. [ ] Plan de rollback

## Timeline Estimado
- Fase 1 (Estructura): 2-3 días
- Fase 2 (Backend): 5-7 días
- Fase 3 (Testing y migración): 2-3 días
- Total: ~2 semanas

## Riesgos y Mitigación
1. **Pérdida de datos**: Backup completo antes de migración
2. **Incompatibilidad**: Mantener APIs actuales durante transición
3. **Performance**: Pruebas de carga con nueva estructura
4. **Complejidad**: Documentación detallada y capacitación