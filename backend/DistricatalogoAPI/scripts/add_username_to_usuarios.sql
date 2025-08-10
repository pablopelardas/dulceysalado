-- Script para agregar campo username a la tabla usuarios
-- Ejecutar en la base de datos

-- Agregar la columna username (opcional, unico si no es null)
ALTER TABLE usuarios 
ADD COLUMN username VARCHAR(100) NULL 
AFTER email;

-- Crear indice unico para username (permite nulls pero no duplicados)
CREATE UNIQUE INDEX idx_usuarios_username_unique 
ON usuarios (username);

-- Verificar la estructura actualizada
DESCRIBE usuarios;

-- Mostrar algunos usuarios para verificar
SELECT id, email, username, nombre, apellido, activo 
FROM usuarios 
WHERE activo = 1 
LIMIT 5;