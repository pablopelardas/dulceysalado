-- Migración: Crear tabla solicitudes_reventa
-- Fecha: 2025-01-06
-- Descripción: Tabla para gestionar solicitudes de cuentas de reventa

CREATE TABLE IF NOT EXISTS solicitudes_reventa (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cliente_id INT NOT NULL,
    empresa_id INT NOT NULL,
    
    -- Datos de la empresa solicitante
    cuit VARCHAR(20),
    razon_social VARCHAR(200),
    direccion_comercial VARCHAR(300),
    localidad VARCHAR(100),
    provincia VARCHAR(100),
    codigo_postal VARCHAR(20),
    telefono_comercial VARCHAR(50),
    categoria_iva VARCHAR(50),
    email_comercial VARCHAR(200),
    
    -- Estado de la solicitud
    estado INT NOT NULL DEFAULT 0 COMMENT '0=Pendiente, 1=Aprobada, 2=Rechazada',
    comentario_respuesta VARCHAR(500),
    fecha_respuesta DATETIME NULL,
    respondido_por VARCHAR(100),
    
    -- Auditoría
    fecha_solicitud DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Índices
    INDEX idx_cliente_id (cliente_id),
    INDEX idx_empresa_id (empresa_id),
    INDEX idx_cliente_estado (cliente_id, estado),
    
    -- Foreign Keys
    CONSTRAINT fk_solicitud_cliente FOREIGN KEY (cliente_id) 
        REFERENCES clientes(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Agregar columna de notificación en preferencias de usuario
ALTER TABLE user_notification_preferences 
ADD COLUMN IF NOT EXISTS notificacion_solicitudes_reventa BOOLEAN DEFAULT TRUE 
COMMENT 'Notificar nuevas solicitudes de cuenta de reventa';

-- Insertar comentario descriptivo
ALTER TABLE solicitudes_reventa 
COMMENT = 'Gestión de solicitudes de cuentas de reventa para clientes';