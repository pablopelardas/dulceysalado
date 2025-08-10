-- Script para crear estructura de múltiples listas de precios
-- Fase 1: Crear nuevas tablas sin eliminar campos existentes

USE districatalogo;

-- 1. Tabla de listas de precios
CREATE TABLE IF NOT EXISTS listas_precios (
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

-- 2. Tabla de precios de productos base por lista
CREATE TABLE IF NOT EXISTS productos_base_precios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    producto_base_id INT NOT NULL,
    lista_precio_id INT NOT NULL,
    precio DECIMAL(10,3) NOT NULL DEFAULT 0.000,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    actualizado_gecom TIMESTAMP NULL,
    
    UNIQUE KEY uk_producto_lista (producto_base_id, lista_precio_id),
    INDEX idx_lista_precio (lista_precio_id),
    INDEX idx_actualizado_gecom (actualizado_gecom),
    
    FOREIGN KEY (producto_base_id) REFERENCES productos_base(id) ON DELETE CASCADE,
    FOREIGN KEY (lista_precio_id) REFERENCES listas_precios(id) ON DELETE CASCADE
);

-- 3. Tabla de precios personalizados por empresa
CREATE TABLE IF NOT EXISTS productos_empresa_precios (
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
    INDEX idx_tipo_producto (tipo_producto, producto_id),
    
    FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE,
    FOREIGN KEY (lista_precio_id) REFERENCES listas_precios(id) ON DELETE CASCADE
);

-- 4. Vista unificada de productos con precios por lista para cada empresa
CREATE OR REPLACE VIEW vista_productos_precios_empresa AS
-- Productos base con sus precios por lista
SELECT 
    pb.id as producto_id,
    'base' as tipo_producto,
    pb.codigo,
    pb.descripcion,
    pb.codigo_rubro,
    pb.visible,
    pb.destacado,
    pb.imagen_url,
    pb.marca,
    pb.unidad_medida,
    e.id as empresa_id,
    e.nombre as empresa_nombre,
    lp.id as lista_precio_id,
    lp.codigo as lista_codigo,
    lp.nombre as lista_nombre,
    COALESCE(pep.precio_override, pbp.precio, 0) as precio_final,
    CASE 
        WHEN pep.precio_override IS NOT NULL THEN TRUE 
        ELSE FALSE 
    END as precio_personalizado,
    pbp.actualizado_gecom,
    pb.updated_at
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
    pe.codigo_rubro,
    pe.visible,
    pe.destacado,
    pe.imagen_url,
    pe.marca,
    pe.unidad_medida,
    pe.empresa_id,
    e.nombre as empresa_nombre,
    lp.id as lista_precio_id,
    lp.codigo as lista_codigo,
    lp.nombre as lista_nombre,
    COALESCE(pep.precio_override, 0) as precio_final,
    TRUE as precio_personalizado,
    NULL as actualizado_gecom,
    pe.updated_at
FROM productos_empresa pe
JOIN empresas e ON pe.empresa_id = e.id
CROSS JOIN listas_precios lp
LEFT JOIN productos_empresa_precios pep ON pe.empresa_id = pep.empresa_id 
    AND pep.tipo_producto = 'empresa' 
    AND pe.id = pep.producto_id 
    AND lp.id = pep.lista_precio_id
WHERE pe.visible = TRUE 
    AND lp.activa = TRUE;

-- 5. Vista simplificada del catálogo con precio predeterminado
CREATE OR REPLACE VIEW vista_catalogo_empresa_precio_default AS
SELECT 
    vppe.producto_id,
    vppe.tipo_producto,
    vppe.codigo,
    vppe.descripcion,
    vppe.codigo_rubro,
    vppe.visible,
    vppe.destacado,
    vppe.imagen_url,
    vppe.marca,
    vppe.unidad_medida,
    vppe.empresa_id,
    vppe.empresa_nombre,
    vppe.precio_final as precio,
    vppe.precio_personalizado,
    vppe.actualizado_gecom,
    vppe.updated_at
FROM vista_productos_precios_empresa vppe
JOIN listas_precios lp ON vppe.lista_precio_id = lp.id
WHERE lp.es_predeterminada = TRUE;

-- 6. Insertar lista de precios predeterminada
INSERT INTO listas_precios (codigo, nombre, descripcion, es_predeterminada, orden)
VALUES ('LISTA1', 'Lista General', 'Lista de precios general', TRUE, 1);

-- Verificar que solo hay una lista predeterminada
DELIMITER $$
CREATE TRIGGER trg_lista_precio_default_unique
BEFORE INSERT ON listas_precios
FOR EACH ROW
BEGIN
    IF NEW.es_predeterminada = TRUE THEN
        UPDATE listas_precios SET es_predeterminada = FALSE WHERE es_predeterminada = TRUE;
    END IF;
END$$

CREATE TRIGGER trg_lista_precio_default_unique_update
BEFORE UPDATE ON listas_precios
FOR EACH ROW
BEGIN
    IF NEW.es_predeterminada = TRUE AND OLD.es_predeterminada = FALSE THEN
        UPDATE listas_precios SET es_predeterminada = FALSE WHERE id != NEW.id AND es_predeterminada = TRUE;
    END IF;
END$$
DELIMITER ;

-- 7. Agregar campo lista_precio_id a la tabla sync_sessions
ALTER TABLE sync_sessions 
ADD COLUMN lista_precio_id INT NULL AFTER empresa_principal_id,
ADD INDEX idx_lista_precio_id (lista_precio_id),
ADD FOREIGN KEY (lista_precio_id) REFERENCES listas_precios(id) ON DELETE SET NULL;

-- Mensaje de confirmación
SELECT 'Tablas de listas de precios creadas exitosamente' as mensaje;