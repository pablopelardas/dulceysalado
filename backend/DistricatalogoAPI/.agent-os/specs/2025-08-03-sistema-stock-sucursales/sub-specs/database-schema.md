# Esquema de Base de Datos - Sistema de Stock por Sucursal

## Nuevas Tablas

### 1. productos_base_stock

```sql
CREATE TABLE productos_base_stock (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_id INT NOT NULL COMMENT 'FK a empresas - cada empresa es una sucursal',
    producto_base_id INT NOT NULL COMMENT 'FK a productos_base',
    existencia DECIMAL(10,3) NOT NULL DEFAULT 0.000 COMMENT 'Cantidad en stock para esta empresa/sucursal',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Índices para performance
    UNIQUE KEY uk_empresa_producto (empresa_id, producto_base_id) COMMENT 'Un registro por empresa-producto',
    INDEX idx_empresa (empresa_id) COMMENT 'Consultas por empresa',
    INDEX idx_producto (producto_base_id) COMMENT 'Consultas por producto',
    INDEX idx_existencia (existencia) COMMENT 'Filtros por stock > 0',
    INDEX idx_empresa_existencia (empresa_id, existencia) COMMENT 'Productos con stock por empresa',
    
    -- Claves foráneas
    FOREIGN KEY fk_stock_empresa (empresa_id) 
        REFERENCES empresas(id) 
        ON DELETE CASCADE 
        ON UPDATE CASCADE,
        
    FOREIGN KEY fk_stock_producto (producto_base_id) 
        REFERENCES productos_base(id) 
        ON DELETE CASCADE 
        ON UPDATE CASCADE
        
) ENGINE=InnoDB 
  DEFAULT CHARSET=utf8mb4 
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Stock de productos base por empresa/sucursal';
```

## Modificaciones a Tablas Existentes

### 1. productos_base - Eliminar campo existencia

```sql
-- Eliminar el campo existencia de productos_base
ALTER TABLE productos_base 
DROP COLUMN existencia;
```

### 2. Mantener productos_empresa sin cambios

```sql
-- productos_empresa.existencia se mantiene pero NO se usa
-- En especificación futura se eliminará toda la funcionalidad de productos_empresa
```

## Scripts de Migración

### Migration: 20250803_create_productos_base_stock.sql

```sql
-- ============================================================================
-- Migration: Crear sistema de stock por sucursal
-- Fecha: 2025-08-03
-- Descripción: Separa el stock de productos_base a tabla independiente por empresa
-- ============================================================================

-- Crear tabla de stock por empresa
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
    INDEX idx_empresa_existencia (empresa_id, existencia),
    
    FOREIGN KEY fk_stock_empresa (empresa_id) 
        REFERENCES empresas(id) 
        ON DELETE CASCADE 
        ON UPDATE CASCADE,
        
    FOREIGN KEY fk_stock_producto (producto_base_id) 
        REFERENCES productos_base(id) 
        ON DELETE CASCADE 
        ON UPDATE CASCADE
        
) ENGINE=InnoDB 
  DEFAULT CHARSET=utf8mb4 
  COLLATE=utf8mb4_unicode_ci
  COMMENT='Stock de productos base por empresa/sucursal';

-- NO MIGRAR DATOS - La sincronización diaria recreará todo el stock

-- Eliminar campo existencia de productos_base
ALTER TABLE productos_base 
DROP COLUMN existencia;

-- Verificar que la tabla se creó correctamente
SELECT 'Tabla productos_base_stock creada exitosamente' as status;

-- Mostrar estructura de la nueva tabla
DESCRIBE productos_base_stock;
```

## Consultas de Ejemplo

### 1. Obtener stock de un producto para una empresa específica

```sql
SELECT existencia 
FROM productos_base_stock 
WHERE empresa_id = 5 AND producto_base_id = 123;
```

### 2. Listar productos con stock para una empresa

```sql
SELECT 
    pb.codigo,
    pb.descripcion,
    pbs.existencia
FROM productos_base pb
INNER JOIN productos_base_stock pbs ON pb.id = pbs.producto_base_id
WHERE pbs.empresa_id = 5 
  AND pbs.existencia > 0
  AND pb.visible = true
ORDER BY pb.descripcion;
```

### 3. Actualizar stock de un producto para todas las empresas (sincronización)

```sql
-- Actualizar stock para todas las empresas
UPDATE productos_base_stock 
SET existencia = 50.000, updated_at = NOW()
WHERE producto_base_id = 123;

-- Insertar si no existe para alguna empresa
INSERT INTO productos_base_stock (empresa_id, producto_base_id, existencia)
SELECT e.id, 123, 50.000
FROM empresas e
WHERE NOT EXISTS (
    SELECT 1 FROM productos_base_stock pbs 
    WHERE pbs.empresa_id = e.id AND pbs.producto_base_id = 123
)
ON DUPLICATE KEY UPDATE 
    existencia = VALUES(existencia),
    updated_at = NOW();
```

### 4. Consulta optimizada para catálogo (reemplaza vista SQL)

```sql
SELECT 
    pb.id as producto_id,
    pb.codigo,
    pb.descripcion,
    pb.codigo_rubro,
    pb.visible,
    pb.destacado,
    pb.imagen_url,
    pb.marca,
    pb.unidad_medida,
    pbs.existencia,
    pbp.precio,
    lp.codigo as lista_codigo,
    lp.nombre as lista_nombre
FROM productos_base pb
INNER JOIN productos_base_stock pbs ON pb.id = pbs.producto_base_id
LEFT JOIN productos_base_precios pbp ON pb.id = pbp.producto_base_id
LEFT JOIN listas_precios lp ON pbp.lista_precio_id = lp.id
WHERE pb.visible = true
  AND pbs.empresa_id = ?
  AND pbs.existencia > 0
  AND (lp.es_predeterminada = true OR lp.id IS NULL)
ORDER BY pb.descripcion;
```

## Índices de Performance

### Análisis de Consultas Frecuentes

```sql
-- 1. Stock por empresa-producto (UNIQUE KEY)
-- Query: SELECT existencia FROM productos_base_stock WHERE empresa_id = ? AND producto_base_id = ?
-- Índice: uk_empresa_producto (empresa_id, producto_base_id)

-- 2. Productos con stock por empresa  
-- Query: SELECT producto_base_id FROM productos_base_stock WHERE empresa_id = ? AND existencia > 0
-- Índice: idx_empresa_existencia (empresa_id, existencia)

-- 3. Stock de un producto en todas las empresas (sincronización)
-- Query: UPDATE productos_base_stock SET existencia = ? WHERE producto_base_id = ?
-- Índice: idx_producto (producto_base_id)

-- 4. Catálogo con filtro de stock
-- Query: JOIN productos_base_stock pbs ON pb.id = pbs.producto_base_id WHERE pbs.empresa_id = ? AND pbs.existencia > 0
-- Índice: idx_empresa_existencia (empresa_id, existencia)
```

### Estadísticas de Uso Estimadas

```sql
-- Estimación de registros
-- Productos base: ~5,000
-- Empresas activas: ~50
-- Total registros en productos_base_stock: ~250,000

-- Distribución de consultas:
-- 80% - Consultas de catálogo por empresa (idx_empresa_existencia)
-- 15% - Consultas específicas producto-empresa (uk_empresa_producto)  
-- 5% - Actualizaciones masivas por producto (idx_producto)
```

## Configuración de Tabla

### Configuración de Storage Engine

```sql
-- InnoDB para transacciones ACID
-- utf8mb4_unicode_ci para soporte completo de caracteres
-- ROW_FORMAT=DYNAMIC para mejor compresión

ALTER TABLE productos_base_stock 
ENGINE=InnoDB 
ROW_FORMAT=DYNAMIC;
```

### Configuración de Particionado (Futuro)

```sql
-- Si la tabla crece mucho, considerar particionado por empresa_id
-- ALTER TABLE productos_base_stock 
-- PARTITION BY HASH(empresa_id) 
-- PARTITIONS 8;
```

## Monitoreo y Mantenimiento

### Consultas de Monitoreo

```sql
-- Verificar integridad referencial
SELECT 
    COUNT(*) as total_records,
    COUNT(DISTINCT empresa_id) as empresas_with_stock,
    COUNT(DISTINCT producto_base_id) as productos_with_stock,
    SUM(existencia) as total_stock
FROM productos_base_stock;

-- Empresas sin stock
SELECT e.id, e.nombre
FROM empresas e
LEFT JOIN productos_base_stock pbs ON e.id = pbs.empresa_id
WHERE pbs.empresa_id IS NULL;

-- Productos sin stock en ninguna empresa
SELECT pb.id, pb.codigo, pb.descripcion
FROM productos_base pb
LEFT JOIN productos_base_stock pbs ON pb.id = pbs.producto_base_id
WHERE pbs.producto_base_id IS NULL
  AND pb.visible = true;
```

### Mantenimiento Programado

```sql
-- Limpiar registros huérfanos (ejecutar post-sincronización)
DELETE pbs FROM productos_base_stock pbs
LEFT JOIN productos_base pb ON pbs.producto_base_id = pb.id
WHERE pb.id IS NULL;

DELETE pbs FROM productos_base_stock pbs  
LEFT JOIN empresas e ON pbs.empresa_id = e.id
WHERE e.id IS NULL;

-- Actualizar estadísticas de tabla
ANALYZE TABLE productos_base_stock;
```

## Rollback Plan

### Script de Rollback

```sql
-- ============================================================================
-- ROLLBACK: Restaurar campo existencia en productos_base
-- SOLO USAR EN EMERGENCIA
-- ============================================================================

-- 1. Agregar campo existencia de vuelta
ALTER TABLE productos_base 
ADD COLUMN existencia DECIMAL(10,3) DEFAULT 0.000;

-- 2. Migrar stock desde productos_base_stock (usar empresa principal)
UPDATE productos_base pb
SET existencia = (
    SELECT pbs.existencia 
    FROM productos_base_stock pbs
    INNER JOIN empresas e ON pbs.empresa_id = e.id
    WHERE pbs.producto_base_id = pb.id
      AND (e.tipo_empresa = 'principal' OR e.empresa_principal_id IS NULL)
    LIMIT 1
);

-- 3. Restaurar vista SQL vista_productos_precios_empresa
-- (ejecutar desde backup de vista anterior)

-- 4. Drop tabla productos_base_stock
DROP TABLE productos_base_stock;
```