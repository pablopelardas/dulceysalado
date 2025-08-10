-- Script para eliminar campos precio antiguos
-- ADVERTENCIA: Ejecutar SOLO después de confirmar que toda la aplicación
-- está usando la nueva estructura de listas de precios

USE districatalogo;

-- Verificar que hay datos migrados antes de eliminar columnas
SET @productos_base_migrados = (SELECT COUNT(*) FROM productos_base_precios);
SET @productos_empresa_migrados = (SELECT COUNT(*) FROM productos_empresa_precios);

IF @productos_base_migrados = 0 THEN
    SIGNAL SQLSTATE '45000' 
    SET MESSAGE_TEXT = 'No hay productos base migrados. Ejecute primero el script de migración.';
END IF;

-- Crear backup de las tablas antes de modificar (opcional pero recomendado)
/*
CREATE TABLE productos_base_backup_precio AS 
SELECT id, codigo, precio, actualizado_gecom 
FROM productos_base;

CREATE TABLE productos_empresa_backup_precio AS 
SELECT id, empresa_id, codigo, precio 
FROM productos_empresa;
*/

-- 1. Actualizar vista del catálogo para no depender del campo precio antiguo
DROP VIEW IF EXISTS vista_catalogo_empresa;

CREATE VIEW vista_catalogo_empresa AS
SELECT 
    vcp.producto_id as id,
    vcp.codigo,
    vcp.descripcion,
    vcp.codigo_rubro,
    vcp.precio_final as precio, -- Ahora viene de la vista con precio default
    pe.existencia as existencia_empresa, -- Para productos empresa
    COALESCE(pb.existencia, pe.existencia, 0) as existencia,
    vcp.visible,
    vcp.destacado,
    COALESCE(pb.orden_categoria, pe.orden_categoria, 0) as orden_categoria,
    vcp.imagen_url,
    COALESCE(pb.imagen_alt, pe.imagen_alt) as imagen_alt,
    COALESCE(pb.descripcion_corta, pe.descripcion_corta) as descripcion_corta,
    COALESCE(pb.descripcion_larga, pe.descripcion_larga) as descripcion_larga,
    COALESCE(pb.tags, pe.tags) as tags,
    COALESCE(pb.codigo_barras, pe.codigo_barras) as codigo_barras,
    vcp.marca,
    vcp.unidad_medida,
    vcp.tipo_producto,
    vcp.empresa_id,
    vcp.empresa_nombre,
    vcp.updated_at
FROM vista_catalogo_empresa_precio_default vcp
LEFT JOIN productos_base pb ON vcp.tipo_producto = 'base' AND vcp.producto_id = pb.id
LEFT JOIN productos_empresa pe ON vcp.tipo_producto = 'empresa' AND vcp.producto_id = pe.id;

-- 2. Eliminar columna precio de productos_base
ALTER TABLE productos_base DROP COLUMN precio;

-- 3. Eliminar columna precio de productos_empresa  
ALTER TABLE productos_empresa DROP COLUMN precio;

-- 4. Verificar que las vistas siguen funcionando
SELECT COUNT(*) as total_productos_vista FROM vista_catalogo_empresa LIMIT 1;
SELECT COUNT(*) as total_precios_vista FROM vista_productos_precios_empresa LIMIT 1;

-- 5. Mensaje de confirmación
SELECT 'Campos de precio antiguos eliminados exitosamente' as mensaje,
       'Las tablas de backup (si se crearon) se pueden eliminar después de verificar que todo funciona correctamente' as nota;

-- Scripts de limpieza (ejecutar manualmente después de verificar)
/*
DROP TABLE IF EXISTS productos_base_backup_precio;
DROP TABLE IF EXISTS productos_empresa_backup_precio;
*/