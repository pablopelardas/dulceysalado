-- ============================================================================
-- Migration: Migrar stock existente a productos_base_stock
-- Fecha: 2025-08-03  
-- Descripción: Carga stock inicial para todas las empresas basado en datos existentes
-- IMPORTANTE: Ejecutar ANTES de hacer DROP COLUMN existencia en productos_base
-- ============================================================================

-- Paso 1: Verificar que existe el campo existencia en productos_base
SELECT 'Verificando estructura de productos_base...' as status;
DESCRIBE productos_base;

-- Paso 2: Contar productos con stock actual
SELECT 
    COUNT(*) as total_productos,
    COUNT(CASE WHEN existencia > 0 THEN 1 END) as productos_con_stock,
    AVG(existencia) as stock_promedio
FROM productos_base 
WHERE existencia IS NOT NULL;

-- Paso 3: Listar todas las empresas activas
SELECT 'Empresas que recibirán stock inicial:' as info;
SELECT 
    id,
    nombre,
    activa,
    empresa_principal_id
FROM empresas 
WHERE activa = 1
ORDER BY id;

-- Paso 4: Migrar stock existente para TODAS las empresas activas
-- Cada empresa recibe una copia del stock de productos_base.existencia
INSERT INTO productos_base_stock (empresa_id, producto_base_id, existencia, created_at, updated_at)
SELECT 
    e.id as empresa_id,
    pb.id as producto_base_id,
    COALESCE(pb.existencia, 0.000) as existencia,
    NOW() as created_at,
    NOW() as updated_at
FROM empresas e
CROSS JOIN productos_base pb
WHERE e.activa = 1
  AND pb.existencia IS NOT NULL
ON DUPLICATE KEY UPDATE
    existencia = VALUES(existencia),
    updated_at = NOW();

-- Paso 5: Verificar resultados de la migración  
SELECT 'Resultados de la migración:' as info;

SELECT 
    COUNT(*) as total_registros_creados,
    COUNT(DISTINCT empresa_id) as empresas_afectadas,
    COUNT(DISTINCT producto_base_id) as productos_migrados,
    SUM(CASE WHEN existencia > 0 THEN 1 ELSE 0 END) as registros_con_stock,
    AVG(existencia) as stock_promedio
FROM productos_base_stock;

-- Paso 6: Resumen por empresa
SELECT 'Stock por empresa después de migración:' as info;
SELECT 
    pbs.empresa_id,
    e.nombre as empresa_nombre,
    COUNT(*) as total_productos,
    COUNT(CASE WHEN pbs.existencia > 0 THEN 1 END) as productos_con_stock,
    SUM(pbs.existencia) as stock_total,
    AVG(pbs.existencia) as stock_promedio
FROM productos_base_stock pbs
JOIN empresas e ON pbs.empresa_id = e.id
GROUP BY pbs.empresa_id, e.nombre
ORDER BY pbs.empresa_id;

-- Paso 7: Validación - comparar totales
SELECT 'Validación de integridad:' as info;
SELECT 
    'Productos en productos_base' as tabla,
    COUNT(*) as total_productos,
    SUM(COALESCE(existencia, 0)) as stock_total
FROM productos_base
WHERE existencia IS NOT NULL

UNION ALL

SELECT 
    'Productos únicos en productos_base_stock' as tabla, 
    COUNT(DISTINCT producto_base_id) as total_productos,
    (SELECT SUM(existencia) FROM productos_base_stock WHERE empresa_id = 1) as stock_total_empresa_1
FROM productos_base_stock;

SELECT 'Migración de stock completada exitosamente!' as status;