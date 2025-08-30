-- Migration: Crear tabla para tokens de corrección de pedidos
-- Fecha: 2025-08-29
-- Descripción: Sistema de correcciones de pedidos con aprobación por link

CREATE TABLE correccion_tokens (
    id INT AUTO_INCREMENT PRIMARY KEY,
    token VARCHAR(50) NOT NULL,
    pedido_id INT NOT NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_expiracion DATETIME NOT NULL,
    usado BOOLEAN NOT NULL DEFAULT FALSE,
    fecha_uso DATETIME NULL,
    respuesta_cliente VARCHAR(20) NULL,
    comentario_cliente VARCHAR(500) NULL,
    
    -- Claves foráneas
    CONSTRAINT FK_correccion_tokens_pedidos 
        FOREIGN KEY (pedido_id) REFERENCES pedidos(id) ON DELETE CASCADE,
    
    -- Índices
    UNIQUE KEY IX_correccion_tokens_token (token),
    KEY IX_correccion_tokens_pedido_id (pedido_id),
    KEY IX_correccion_tokens_fecha_expiracion (fecha_expiracion)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Comentarios para documentación
ALTER TABLE correccion_tokens 
    COMMENT = 'Tokens para aprobación de correcciones de pedidos';

ALTER TABLE correccion_tokens MODIFY COLUMN token VARCHAR(50) 
    COMMENT 'Token único para acceso público a la corrección';
    
ALTER TABLE correccion_tokens MODIFY COLUMN respuesta_cliente VARCHAR(20) 
    COMMENT 'Aprobado o Rechazado';
    
ALTER TABLE correccion_tokens MODIFY COLUMN comentario_cliente VARCHAR(500) 
    COMMENT 'Comentario opcional del cliente sobre la corrección';

-- Agregar nuevas columnas para snapshot del pedido original
ALTER TABLE correccion_tokens 
ADD COLUMN motivo_correccion LONGTEXT NULL COMMENT 'Motivo de la corrección explicado al cliente',
ADD COLUMN pedido_original_json LONGTEXT NOT NULL COMMENT 'Snapshot JSON del pedido antes de la corrección';