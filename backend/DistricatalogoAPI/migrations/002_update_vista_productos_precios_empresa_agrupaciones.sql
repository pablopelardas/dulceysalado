-- Migration: Actualizar vista_productos_precios_empresa para incluir agrupaciones
-- Fecha: 2025-01-07
-- Descripción: Modifica la vista para incluir información de agrupaciones y filtros de visibilidad

-- Eliminar vista existente
DROP VIEW IF EXISTS vista_productos_precios_empresa;

-- Crear nueva vista con campos de agrupación
CREATE VIEW vista_productos_precios_empresa AS
-- Productos Base con agrupaciones y filtros de visibilidad
SELECT 
    pb.id AS producto_id,
    'base' AS tipo_producto,
    pb.codigo,
    pb.descripcion,
    pb.codigo_rubro,
    pb.visible,
    pb.destacado,
    pb.imagen_url,
    pb.marca,
    pb.unidad_medida,
    e.id AS empresa_id,
    e.nombre AS empresa_nombre,
    lp.id AS lista_precio_id,
    lp.codigo AS lista_codigo,
    lp.nombre AS lista_nombre,
    COALESCE(pep.precio_override, pbp.precio, 0) AS precio_final,
    (CASE WHEN pep.precio_override IS NOT NULL THEN true ELSE false END) AS precio_personalizado,
    pbp.actualizado_gecom,
    pb.updated_at,
    -- Campos de agrupación
    pb.grupo3 AS agrupacion_codigo,
    a.nombre AS agrupacion_nombre,
    a.activa AS agrupacion_activa
FROM productos_base pb
CROSS JOIN empresas e
CROSS JOIN listas_precios lp
LEFT JOIN productos_base_precios pbp ON (pb.id = pbp.producto_base_id AND lp.id = pbp.lista_precio_id)
LEFT JOIN productos_empresa_precios pep ON (e.id = pep.empresa_id AND pep.tipo_producto = 'base' AND pb.id = pep.producto_id AND lp.id = pep.lista_precio_id)
LEFT JOIN agrupaciones a ON (pb.grupo3 = a.codigo AND a.empresa_principal_id = COALESCE(e.empresa_principal_id, e.id))
LEFT JOIN empresas_agrupaciones_visibles eav ON (a.id = eav.agrupacion_id AND eav.empresa_id = e.id)
WHERE pb.visible = true 
    AND lp.activa = true 
    AND pb.existencia > 0
    AND (
        pb.grupo3 IS NULL OR  -- Sin agrupación asignada
        eav.visible = true OR  -- Agrupación visible para empresa
        NOT EXISTS (  -- Empresa no tiene configuración de visibilidad
            SELECT 1 FROM empresas_agrupaciones_visibles eav2 WHERE eav2.empresa_id = e.id
        )
    )

UNION ALL

-- Productos Empresa (sin agrupaciones, solo para mantener compatibilidad)
SELECT 
    pe.id AS producto_id,
    'empresa' AS tipo_producto,
    pe.codigo,
    pe.descripcion,
    pe.codigo_rubro,
    pe.visible,
    pe.destacado,
    pe.imagen_url,
    pe.marca,
    pe.unidad_medida,
    pe.empresa_id,
    e.nombre AS empresa_nombre,
    lp.id AS lista_precio_id,
    lp.codigo AS lista_codigo,
    lp.nombre AS lista_nombre,
    COALESCE(pep.precio_override, 0) AS precio_final,
    true AS precio_personalizado,
    NULL AS actualizado_gecom,
    pe.updated_at,
    -- Campos de agrupación (NULL para productos empresa)
    NULL AS agrupacion_codigo,
    NULL AS agrupacion_nombre,
    NULL AS agrupacion_activa
FROM productos_empresa pe
JOIN empresas e ON (pe.empresa_id = e.id)
CROSS JOIN listas_precios lp
LEFT JOIN productos_empresa_precios pep ON (pe.empresa_id = pep.empresa_id AND pep.tipo_producto = 'empresa' AND pe.id = pep.producto_id AND lp.id = pep.lista_precio_id)
WHERE pe.visible = true 
    AND lp.activa = true 
    AND pe.existencia > 0;

-- Verificar que la vista se creó correctamente
SELECT 'Vista vista_productos_precios_empresa actualizada correctamente' as status;

-- Mostrar estructura de la nueva vista
DESCRIBE vista_productos_precios_empresa;

-- Ejemplo de consulta para verificar funcionamiento
-- SELECT empresa_id, agrupacion_codigo, agrupacion_nombre, COUNT(*) as productos_count
-- FROM vista_productos_precios_empresa 
-- WHERE empresa_id = 1 AND lista_precio_id = 1
-- GROUP BY empresa_id, agrupacion_codigo, agrupacion_nombre
-- ORDER BY agrupacion_codigo;