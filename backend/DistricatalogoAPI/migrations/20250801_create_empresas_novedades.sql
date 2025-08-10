-- Migration: Create empresas_novedades table
-- Date: 2025-08-01
-- Purpose: Manage which agrupaciones of Grupo 1 are visible as "novedades" for each empresa

CREATE TABLE empresas_novedades (
    id INT PRIMARY KEY AUTO_INCREMENT,
    empresa_id INT NOT NULL,
    agrupacion_id INT NOT NULL,
    visible TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Foreign Keys
    CONSTRAINT fk_empresas_novedades_empresa 
        FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE,
    CONSTRAINT fk_empresas_novedades_agrupacion 
        FOREIGN KEY (agrupacion_id) REFERENCES agrupaciones(id) ON DELETE CASCADE,
    
    -- Unique constraint to prevent duplicates
    UNIQUE KEY uk_empresas_novedades_empresa_agrupacion (empresa_id, agrupacion_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create indexes for performance
CREATE INDEX idx_empresas_novedades_empresa ON empresas_novedades(empresa_id);
CREATE INDEX idx_empresas_novedades_agrupacion ON empresas_novedades(agrupacion_id);
CREATE INDEX idx_empresas_novedades_visible ON empresas_novedades(visible);
CREATE INDEX idx_empresas_novedades_empresa_visible ON empresas_novedades(empresa_id, visible);

-- Verify table creation
DESCRIBE empresas_novedades;