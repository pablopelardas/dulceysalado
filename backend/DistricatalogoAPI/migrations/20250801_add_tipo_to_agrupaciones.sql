-- Migration: Add 'tipo' field to agrupaciones table
-- Date: 2025-08-01
-- Purpose: Differentiate between Grupo 1 (Novedades/Ofertas), Grupo 2 (Future), Grupo 3 (Current)

-- Step 1: Add tipo column to agrupaciones table
ALTER TABLE agrupaciones 
ADD COLUMN tipo TINYINT NOT NULL DEFAULT 3 
COMMENT '1=Grupo1(Novedades/Ofertas), 2=Grupo2(Futuro), 3=Grupo3(Actual)';

-- Step 2: Create indexes for performance
CREATE INDEX idx_agrupaciones_tipo ON agrupaciones(tipo);
CREATE INDEX idx_agrupaciones_tipo_empresa ON agrupaciones(tipo, empresa_principal_id);

-- Step 3: Verify the change
SELECT COUNT(*) as total_agrupaciones, 
       SUM(CASE WHEN tipo = 1 THEN 1 ELSE 0 END) as tipo_1,
       SUM(CASE WHEN tipo = 2 THEN 1 ELSE 0 END) as tipo_2,
       SUM(CASE WHEN tipo = 3 THEN 1 ELSE 0 END) as tipo_3
FROM agrupaciones;

-- Step 4: Modificar  index for codigo and empresa_principal_id
ALTER TABLE agrupaciones DROP INDEX idx_agrupaciones_codigo_empresa;
ALTER TABLE agrupaciones ADD UNIQUE INDEX idx_agrupaciones_codigo_empresa_tipo (codigo, empresa_principal_id, tipo);

