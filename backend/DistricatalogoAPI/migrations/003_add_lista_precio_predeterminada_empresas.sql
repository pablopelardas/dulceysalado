-- Migration: Agregar lista de precios predeterminada por empresa
-- Fecha: 2025-01-08
-- Descripción: Permite que cada empresa tenga su propia lista de precios predeterminada

-- Agregar columna a la tabla empresas
ALTER TABLE empresas 
ADD COLUMN lista_precio_predeterminada_id INT NULL 
COMMENT 'Lista de precios por defecto para esta empresa. Si es NULL usa la lista global predeterminada.';

-- Agregar foreign key
ALTER TABLE empresas 
ADD CONSTRAINT fk_empresas_lista_precio_predeterminada 
FOREIGN KEY (lista_precio_predeterminada_id) 
REFERENCES listas_precios(id) 
ON DELETE SET NULL ON UPDATE CASCADE;

-- Crear índice para mejorar performance en las consultas
CREATE INDEX idx_empresas_lista_precio_predeterminada 
ON empresas(lista_precio_predeterminada_id);

-- Verificar que la migration se ejecutó correctamente
SELECT 
    'Columna lista_precio_predeterminada_id agregada correctamente' as status,
    COUNT(*) as empresas_count,
    COUNT(lista_precio_predeterminada_id) as configuradas_count
FROM empresas;

-- Ejemplo de cómo configurar una empresa con lista específica (comentado por defecto)
-- UPDATE empresas SET lista_precio_predeterminada_id = 2 WHERE id = 3;