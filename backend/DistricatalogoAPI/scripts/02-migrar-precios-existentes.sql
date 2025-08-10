-- Script para migrar precios existentes a la nueva estructura
-- IMPORTANTE: Ejecutar DESPUÉS de crear las tablas con 01-crear-tablas-listas-precios.sql

USE districatalogo;

-- Verificar que existe la lista predeterminada
SET @lista_default_id = (SELECT id FROM listas_precios WHERE es_predeterminada = TRUE LIMIT 1);

IF @lista_default_id IS NULL THEN
    SIGNAL SQLSTATE '45000' 
    SET MESSAGE_TEXT = 'No se encontró lista de precios predeterminada. Ejecute primero el script 01-crear-tablas-listas-precios.sql';
END IF;

-- 1. Migrar precios de productos_base a productos_base_precios
INSERT INTO productos_base_precios (producto_base_id, lista_precio_id, precio, actualizado_gecom)
SELECT 
    pb.id,
    @lista_default_id,
    pb.precio,
    pb.actualizado_gecom
FROM productos_base pb
WHERE pb.precio IS NOT NULL AND pb.precio > 0
ON DUPLICATE KEY UPDATE 
    precio = VALUES(precio),
    actualizado_gecom = VALUES(actualizado_gecom);

-- Contar registros migrados
SELECT CONCAT('Productos base migrados: ', COUNT(*)) as resultado
FROM productos_base_precios 
WHERE lista_precio_id = @lista_default_id;

-- 2. Migrar precios personalizados de productos_empresa
-- Nota: Los productos_empresa mantienen su precio propio, no son overrides de productos_base
INSERT INTO productos_empresa_precios (empresa_id, producto_id, tipo_producto, lista_precio_id, precio_override)
SELECT 
    pe.empresa_id,
    pe.id,
    'empresa',
    @lista_default_id,
    pe.precio
FROM productos_empresa pe
WHERE pe.precio IS NOT NULL AND pe.precio > 0
ON DUPLICATE KEY UPDATE 
    precio_override = VALUES(precio_override);

-- Contar registros migrados
SELECT CONCAT('Productos empresa migrados: ', COUNT(*)) as resultado
FROM productos_empresa_precios 
WHERE lista_precio_id = @lista_default_id AND tipo_producto = 'empresa';

-- 3. Verificación de integridad
-- Verificar que todos los productos con precio fueron migrados
SELECT 
    'Productos base sin migrar' as tipo,
    COUNT(*) as cantidad
FROM productos_base pb
WHERE pb.precio IS NOT NULL 
    AND pb.precio > 0
    AND NOT EXISTS (
        SELECT 1 
        FROM productos_base_precios pbp 
        WHERE pbp.producto_base_id = pb.id 
        AND pbp.lista_precio_id = @lista_default_id
    )
UNION ALL
SELECT 
    'Productos empresa sin migrar' as tipo,
    COUNT(*) as cantidad
FROM productos_empresa pe
WHERE pe.precio IS NOT NULL 
    AND pe.precio > 0
    AND NOT EXISTS (
        SELECT 1 
        FROM productos_empresa_precios pep 
        WHERE pep.empresa_id = pe.empresa_id 
        AND pep.producto_id = pe.id 
        AND pep.tipo_producto = 'empresa'
        AND pep.lista_precio_id = @lista_default_id
    );

-- 4. Crear listas adicionales de ejemplo (opcional)
-- Descomente si desea crear listas adicionales
/*
INSERT INTO listas_precios (codigo, nombre, descripcion, orden) VALUES
('LISTA2', 'Lista Mayorista', 'Precios especiales para mayoristas', 2),
('LISTA3', 'Lista Distribuidor', 'Precios para distribuidores', 3),
('LISTA4', 'Lista VIP', 'Precios preferenciales para clientes VIP', 4);
*/

-- 5. Resumen de migración
SELECT 
    'RESUMEN DE MIGRACIÓN' as titulo,
    (SELECT COUNT(*) FROM productos_base_precios WHERE lista_precio_id = @lista_default_id) as productos_base_migrados,
    (SELECT COUNT(*) FROM productos_empresa_precios WHERE lista_precio_id = @lista_default_id) as productos_empresa_migrados,
    (SELECT COUNT(*) FROM listas_precios) as total_listas_creadas,
    NOW() as fecha_migracion;

-- Mensaje de confirmación
SELECT 'Migración de precios completada exitosamente' as mensaje;

-- NOTA: Los campos precio originales se mantienen por compatibilidad
-- Se eliminarán en una fase posterior con el script 03-eliminar-campos-precio-antiguos.sql