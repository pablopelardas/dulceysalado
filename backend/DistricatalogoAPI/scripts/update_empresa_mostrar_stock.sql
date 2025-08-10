-- Script para actualizar configuración de mostrar_stock para empresa 1
-- Esto permite que el stock se muestre en las respuestas del catálogo

UPDATE empresas 
SET mostrar_stock = 1 
WHERE id = 1;

-- Verificar el cambio
SELECT id, codigo, nombre, mostrar_precios, mostrar_stock, lista_precio_predeterminada_id 
FROM empresas 
WHERE id = 1;