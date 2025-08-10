-- Script para poblar datos de prueba en productos_base_stock
-- Empresas: 1 (LADISTRIBUIDORA - principal) y 5 (DIST-001 - Distribuidora del Norte)
-- Productos: códigos 92, 93, 94, 99

-- Limpiar datos existentes para estas empresas y productos (por si ya existen)
DELETE FROM productos_base_stock 
WHERE empresa_id IN (1, 5) 
AND producto_base_id IN (
    SELECT id FROM productos_base WHERE codigo IN (92, 93, 94, 99)
);

-- Insertar stock para empresa 1 (LADISTRIBUIDORA - principal)
INSERT INTO productos_base_stock (empresa_id, producto_base_id, existencia, created_at, updated_at)
SELECT 
    1 as empresa_id,
    pb.id as producto_base_id,
    CASE 
        WHEN pb.codigo = 92 THEN 25.000  -- VINO LLUVIA NEGRA X 750
        WHEN pb.codigo = 93 THEN 18.000  -- VINO CALLE LARGA MALBEC X 750  
        WHEN pb.codigo = 94 THEN 50.000  -- TEK BOND SILICONA TRANSPARENTE X 25 GR
        WHEN pb.codigo = 99 THEN 100.000 -- TEK BOND ADHESIVO INSTANTANEO X 2 GR
    END as existencia,
    NOW() as created_at,
    NOW() as updated_at
FROM productos_base pb
WHERE pb.codigo IN (92, 93, 94, 99);

-- Insertar stock para empresa 5 (DIST-001 - Distribuidora del Norte)
INSERT INTO productos_base_stock (empresa_id, producto_base_id, existencia, created_at, updated_at)
SELECT 
    5 as empresa_id,
    pb.id as producto_base_id,
    CASE 
        WHEN pb.codigo = 92 THEN 12.000  -- VINO LLUVIA NEGRA X 750 (menos stock)
        WHEN pb.codigo = 93 THEN 8.000   -- VINO CALLE LARGA MALBEC X 750 (menos stock)
        WHEN pb.codigo = 94 THEN 0.000   -- TEK BOND SILICONA TRANSPARENTE (sin stock)
        WHEN pb.codigo = 99 THEN 35.000  -- TEK BOND ADHESIVO INSTANTANEO X 2 GR
    END as existencia,
    NOW() as created_at,
    NOW() as updated_at
FROM productos_base pb
WHERE pb.codigo IN (92, 93, 94, 99);

-- Verificar los datos insertados
SELECT 
    pbs.empresa_id,
    e.nombre as empresa_nombre,
    pb.codigo as producto_codigo,
    pb.descripcion as producto_descripcion,
    pbs.existencia,
    pbs.created_at
FROM productos_base_stock pbs
JOIN productos_base pb ON pbs.producto_base_id = pb.id
JOIN empresas e ON pbs.empresa_id = e.id
WHERE pbs.empresa_id IN (1, 5) 
AND pb.codigo IN (92, 93, 94, 99)
ORDER BY pbs.empresa_id, pb.codigo;