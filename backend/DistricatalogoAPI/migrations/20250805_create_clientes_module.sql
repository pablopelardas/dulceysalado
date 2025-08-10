-- Módulo de Clientes con Autenticación
-- Fecha: 2025-08-05
-- Descripción: Creación de tablas para gestión de clientes con autenticación independiente

-- Tabla principal de clientes
CREATE TABLE IF NOT EXISTS clientes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_id INT NOT NULL,
    codigo VARCHAR(20) NOT NULL,
    nombre VARCHAR(255),
    direccion VARCHAR(500),
    localidad VARCHAR(255),
    telefono VARCHAR(100),
    cuit VARCHAR(50), -- Flexible, permite duplicados y cualquier formato
    altura VARCHAR(50), -- String para manejar formatos como "4.301"
    provincia VARCHAR(100),
    email VARCHAR(255),
    tipo_iva VARCHAR(50),
    
    -- Campos de autenticación
    username VARCHAR(100),
    password_hash VARCHAR(255),
    is_active BOOLEAN DEFAULT false,
    last_login TIMESTAMP NULL,
    
    -- Lista de precios (asumiendo que ya existe la tabla listas_precios)
    lista_precio_id INT,
    
    -- Auditoría
    activo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by VARCHAR(100),
    updated_by VARCHAR(100),
    
    -- Índices
    INDEX idx_empresa_codigo (empresa_id, codigo),
    INDEX idx_empresa_username (empresa_id, username),
    INDEX idx_nombre (nombre),
    INDEX idx_cuit (cuit),
    INDEX idx_localidad (localidad),
    INDEX idx_activo (activo),
    INDEX idx_is_active (is_active),
    
    -- Constraints
    UNIQUE KEY uk_empresa_codigo (empresa_id, codigo),
    UNIQUE KEY uk_empresa_username (empresa_id, username),
    FOREIGN KEY (empresa_id) REFERENCES empresas(id),
    FOREIGN KEY (lista_precio_id) REFERENCES listas_precios(id)
    -- La FK de lista_precio_id se agregará después de verificar que existe la tabla
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla para tokens de refresh (para gestión de sesiones prolongadas)
CREATE TABLE IF NOT EXISTS cliente_refresh_tokens (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cliente_id INT NOT NULL,
    token VARCHAR(500) NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (cliente_id) REFERENCES clientes(id) ON DELETE CASCADE,
    INDEX idx_token (token),
    INDEX idx_expires (expires_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de auditoría de accesos (opcional pero recomendada)
CREATE TABLE IF NOT EXISTS cliente_login_history (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cliente_id INT NOT NULL,
    login_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ip_address VARCHAR(45),
    user_agent VARCHAR(500),
    success BOOLEAN DEFAULT true,
    FOREIGN KEY (cliente_id) REFERENCES clientes(id) ON DELETE CASCADE,
    INDEX idx_cliente_login (cliente_id, login_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla de sesiones de sincronización (para el patrón de sincronización por sesiones)
CREATE TABLE IF NOT EXISTS customer_sync_sessions (
    id VARCHAR(36) PRIMARY KEY, -- UUID
    empresa_id INT NOT NULL,
    source VARCHAR(50),
    status VARCHAR(20) DEFAULT 'active', -- active, completed, failed
    total_processed INT DEFAULT 0,
    total_created INT DEFAULT 0,
    total_updated INT DEFAULT 0,
    total_unchanged INT DEFAULT 0,
    total_errors INT DEFAULT 0,
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP NULL,
    created_by VARCHAR(100),
    FOREIGN KEY (empresa_id) REFERENCES empresas(id),
    INDEX idx_empresa_status (empresa_id, status),
    INDEX idx_started_at (started_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Comentarios adicionales
-- 1. El campo 'cuit' es flexible y permite duplicados según requerimiento
-- 2. El campo 'altura' es VARCHAR para manejar valores como "4.301" del XML
-- 3. Username es único por empresa, no globalmente
-- 4. is_active controla el acceso, activo controla el soft delete
-- 5. La sincronización usa sesiones UUID para tracking similar a productos