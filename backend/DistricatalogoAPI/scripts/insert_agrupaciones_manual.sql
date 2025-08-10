-- Script para crear agrupaciones de novedades y ofertas manualmente
-- Ejecutar en el servidor productivo para testing antes de la sincronización

-- ======================================
-- AGRUPACIONES TIPO 1 (Novedades/Ofertas)
-- ======================================

-- Obtener empresa principal (la primera empresa activa)
SET @empresa_principal_id = (SELECT id FROM empresas WHERE activa = 1 ORDER BY id ASC LIMIT 1);

-- Insertar agrupación "Novedades" (código 1)
INSERT INTO agrupaciones (codigo, nombre, descripcion, tipo, empresa_principal_id, activa, created_at, updated_at)
VALUES (
    1, 
    'Novedades', 
    'Productos novedades para mostrar en el catálogo público',
    1,
    @empresa_principal_id,
    1,
    NOW(),
    NOW()
) ON DUPLICATE KEY UPDATE
    nombre = VALUES(nombre),
    descripcion = VALUES(descripcion),
    activa = 1,
    updated_at = NOW();

-- Insertar agrupación "Ofertas" (código 2)  
INSERT INTO agrupaciones (codigo, nombre, descripcion, tipo, empresa_principal_id, activa, created_at, updated_at)
VALUES (
    2,
    'Ofertas',
    'Productos en oferta para mostrar en el catálogo público', 
    1,
    @empresa_principal_id,
    1,
    NOW(),
    NOW()
) ON DUPLICATE KEY UPDATE
    nombre = VALUES(nombre),
    descripcion = VALUES(descripcion),
    activa = 1,
    updated_at = NOW();

-- ======================================
-- AGRUPACIONES TIPO 2 (Futuro uso)
-- ======================================

-- Insertar algunas agrupaciones tipo 2 de ejemplo
INSERT INTO agrupaciones (codigo, nombre, descripcion, tipo, empresa_principal_id, activa, created_at, updated_at)
VALUES 
    (10, 'Grupo Futuro 10', 'Agrupación tipo 2 para uso futuro', 2, @empresa_principal_id, 1, NOW(), NOW()),
    (20, 'Grupo Futuro 20', 'Agrupación tipo 2 para uso futuro', 2, @empresa_principal_id, 1, NOW(), NOW()),
    (30, 'Grupo Futuro 30', 'Agrupación tipo 2 para uso futuro', 2, @empresa_principal_id, 1, NOW(), NOW())
ON DUPLICATE KEY UPDATE
    nombre = VALUES(nombre),
    descripcion = VALUES(descripcion),
    activa = 1,
    updated_at = NOW();

-- ======================================
-- MARCAR LISTA CÓDIGO "1" COMO PREDETERMINADA
-- ======================================

-- Desmarcar todas las listas como predeterminadas
UPDATE listas_precios SET es_predeterminada = 0 WHERE es_predeterminada = 1;

-- Marcar lista código "1" como predeterminada
UPDATE listas_precios 
SET es_predeterminada = 1, updated_at = NOW()
WHERE codigo = '1' AND activa = 1;

-- ======================================
-- VERIFICACIONES
-- ======================================

-- Mostrar las agrupaciones creadas
SELECT 
    id,
    codigo,
    nombre,
    descripcion,
    tipo,
    empresa_principal_id,
    activa,
    created_at
FROM agrupaciones 
WHERE tipo IN (1, 2)
ORDER BY tipo, codigo;

-- Mostrar la lista predeterminada
SELECT 
    id,
    codigo,
    nombre,
    es_predeterminada,
    activa
FROM listas_precios 
WHERE es_predeterminada = 1;

-- Verificar algunos productos que tienen grupo1 = 1 o 2 (deberían aparecer en novedades/ofertas)
SELECT 
    codigo,
    descripcion,
    grupo1,
    grupo2,
    grupo3,
    administrado_por_empresa_id
FROM productos_bases 
WHERE grupo1 IN (1, 2) 
LIMIT 10;

-- Contar productos por grupo1
SELECT 
    grupo1,
    COUNT(*) as cantidad_productos
FROM productos_bases 
WHERE grupo1 IS NOT NULL AND grupo1 > 0
GROUP BY grupo1
ORDER BY grupo1;