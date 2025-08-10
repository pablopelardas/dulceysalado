-- Migration: Crear tablas para sistema de agrupaciones
-- Fecha: 2025-01-07
-- Descripción: Crea tabla agrupaciones y empresas_agrupaciones_visibles para filtrado de productos por empresa

-- Crear tabla agrupaciones
CREATE TABLE IF NOT EXISTS agrupaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo INT NOT NULL COMMENT 'Código que viene de Grupo3 en productos',
    nombre VARCHAR(255) NOT NULL COMMENT 'Nombre descriptivo de la agrupación',
    descripcion VARCHAR(500) NULL COMMENT 'Descripción opcional',
    activa TINYINT(1) DEFAULT 1 COMMENT 'Si la agrupación está activa',
    empresa_principal_id INT NOT NULL COMMENT 'ID de la empresa principal que administra',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Índices
    UNIQUE KEY idx_agrupaciones_codigo_empresa (codigo, empresa_principal_id),
    KEY idx_agrupaciones_empresa_principal (empresa_principal_id),
    KEY idx_agrupaciones_activa (activa),
    
    -- Foreign keys
    CONSTRAINT fk_agrupaciones_empresa_principal 
        FOREIGN KEY (empresa_principal_id) REFERENCES empresas(id) 
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Agrupaciones de productos basadas en campo Grupo3';

-- Crear tabla empresas_agrupaciones_visibles
CREATE TABLE IF NOT EXISTS empresas_agrupaciones_visibles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_id INT NOT NULL COMMENT 'ID de la empresa (principal o cliente)',
    agrupacion_id INT NOT NULL COMMENT 'ID de la agrupación',
    visible TINYINT(1) DEFAULT 1 COMMENT 'Si la agrupación es visible para la empresa',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Índices
    UNIQUE KEY idx_empresa_agrupacion (empresa_id, agrupacion_id),
    KEY idx_agrupacion (agrupacion_id),
    KEY idx_empresa_visible (empresa_id, visible),
    
    -- Foreign keys
    CONSTRAINT fk_empresas_agrupaciones_empresa 
        FOREIGN KEY (empresa_id) REFERENCES empresas(id) 
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_empresas_agrupaciones_agrupacion 
        FOREIGN KEY (agrupacion_id) REFERENCES agrupaciones(id) 
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Configuración de visibilidad de agrupaciones por empresa';

-- Insertar datos de ejemplo (opcional - comentado por defecto)
-- INSERT INTO agrupaciones (codigo, nombre, descripcion, empresa_principal_id) VALUES
-- (1, 'Agrupación 1', 'Primera agrupación de ejemplo', 1),
-- (2, 'Agrupación 2', 'Segunda agrupación de ejemplo', 1);

-- Verificar que las tablas se crearon correctamente
SELECT 'Tabla agrupaciones creada correctamente' as status 
WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'agrupaciones' AND table_schema = DATABASE());

SELECT 'Tabla empresas_agrupaciones_visibles creada correctamente' as status 
WHERE EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'empresas_agrupaciones_visibles' AND table_schema = DATABASE());