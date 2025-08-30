-- Migración: Crear tabla de preferencias de notificaciones de usuarios
-- Fecha: 2025-08-30
-- Descripción: Sistema granular de control de notificaciones por usuario

-- Crear tabla de preferencias de notificaciones
CREATE TABLE IF NOT EXISTS user_notification_preferences (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    
    -- Tipos de notificaciones de pedidos
    notificacion_nuevos_pedidos BOOLEAN DEFAULT TRUE COMMENT 'Recibir emails cuando llegan nuevos pedidos',
    notificacion_correcciones_aprobadas BOOLEAN DEFAULT TRUE COMMENT 'Recibir emails cuando clientes aprueban correcciones',  
    notificacion_correcciones_rechazadas BOOLEAN DEFAULT TRUE COMMENT 'Recibir emails cuando clientes rechazan correcciones',
    notificacion_pedidos_cancelados BOOLEAN DEFAULT TRUE COMMENT 'Recibir emails cuando clientes cancelan pedidos',
    
    -- Campos de auditoría
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by VARCHAR(255) DEFAULT 'SYSTEM',
    updated_by VARCHAR(255) DEFAULT 'SYSTEM',
    
    -- Constraints
    FOREIGN KEY (user_id) REFERENCES usuarios(id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_preferences (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Tabla de preferencias de notificaciones por usuario - permite control granular de emails de notificación';

-- Crear preferencias predeterminadas para usuarios existentes
-- Solo para usuarios activos
INSERT INTO user_notification_preferences (user_id, created_by, updated_by)
SELECT 
    id as user_id,
    'MIGRATION_20250830' as created_by,
    'MIGRATION_20250830' as updated_by
FROM usuarios
WHERE activo = TRUE
ON DUPLICATE KEY UPDATE updated_at = CURRENT_TIMESTAMP;

-- Índices para optimizar consultas
CREATE INDEX idx_user_notification_prefs_user_id ON user_notification_preferences(user_id);
CREATE INDEX idx_user_notification_prefs_active_users ON user_notification_preferences(user_id) 
    WHERE notificacion_nuevos_pedidos = TRUE 
       OR notificacion_correcciones_aprobadas = TRUE 
       OR notificacion_correcciones_rechazadas = TRUE 
       OR notificacion_pedidos_cancelados = TRUE;