-- ============================================================================
-- Migration: Crear sistema de stock por sucursal
-- Fecha: 2025-08-03
-- Descripción: Separa el stock de productos_base a tabla independiente por empresa
-- ============================================================================

-- Crear tabla de stock por empresa
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

-- NO MIGRAR DATOS - La sincronización diaria recreará todo el stock

-- Eliminar campo existencia de productos_base
ALTER TABLE productos_base 
DROP COLUMN existencia;

-- Verificar que la tabla se creó correctamente
SELECT 'Tabla productos_base_stock creada exitosamente' as status;

-- Mostrar estructura de la nueva tabla
DESCRIBE productos_base_stock;

-- Verificar índices creados
SHOW INDEX FROM productos_base_stock;