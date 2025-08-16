-- Script SQL para agregar campos de autenticación con Google a la tabla Clientes
-- Fecha: 2025-08-16
-- Descripción: Agrega campos necesarios para autenticación social con Google

USE dulceysaladomax;

-- Agregar campos para autenticación con Google
ALTER TABLE Clientes 
ADD COLUMN google_id VARCHAR(255) NULL COMMENT 'ID único del usuario en Google',
ADD COLUMN foto_url TEXT NULL COMMENT 'URL de la foto de perfil de Google',
ADD COLUMN email_verificado BOOLEAN DEFAULT FALSE COMMENT 'Indica si el email está verificado',
ADD COLUMN proveedor_auth VARCHAR(50) NULL COMMENT 'Proveedor de autenticación (google, local, etc.)';

-- Crear índice para búsquedas por Google ID
CREATE INDEX idx_clientes_google_id ON Clientes(google_id);

-- Crear índice para búsquedas por email y empresa
CREATE INDEX idx_clientes_email_empresa ON Clientes(email, empresa_id);

-- Crear índice para búsquedas por proveedor de autenticación
CREATE INDEX idx_clientes_proveedor_auth ON Clientes(proveedor_auth);

-- Actualizar clientes existentes para marcar como autenticación local
UPDATE Clientes 
SET proveedor_auth = 'local' 
WHERE proveedor_auth IS NULL AND password_hash IS NOT NULL;

-- Verificar que los campos se agregaron correctamente
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    COLUMN_COMMENT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'dulceysaladomax' 
  AND TABLE_NAME = 'Clientes' 
  AND COLUMN_NAME IN ('google_id', 'foto_url', 'email_verificado', 'proveedor_auth')
ORDER BY ORDINAL_POSITION;

-- Script completado
-- Los nuevos campos están listos para usar con la autenticación de Google