-- Migración: Sistema de Feature Flags
-- Fecha: 2025-08-05
-- Descripción: Crea las tablas para el sistema de configuración de features por empresa

-- Crear tabla de definiciones de features
CREATE TABLE IF NOT EXISTS feature_definitions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(100) NOT NULL UNIQUE,
    nombre VARCHAR(255) NOT NULL,
    descripcion TEXT,
    tipo_valor ENUM('Boolean', 'String', 'Number', 'Json') DEFAULT 'Boolean',
    valor_defecto TEXT,
    categoria VARCHAR(100),
    activo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_codigo (codigo),
    INDEX idx_categoria (categoria),
    INDEX idx_activo (activo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Crear tabla de configuración de features por empresa
CREATE TABLE IF NOT EXISTS empresa_features (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_id INT NOT NULL,
    feature_id INT NOT NULL,
    habilitado BOOLEAN DEFAULT true,
    valor TEXT,
    metadata JSON,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by VARCHAR(255),
    updated_by VARCHAR(255),
    
    UNIQUE KEY uk_empresa_feature (empresa_id, feature_id),
    FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE,
    FOREIGN KEY (feature_id) REFERENCES feature_definitions(id) ON DELETE CASCADE,
    
    INDEX idx_empresa (empresa_id),
    INDEX idx_feature (feature_id),
    INDEX idx_habilitado (habilitado)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO feature_definitions (codigo, nombre, descripcion, tipo_valor, categoria) VALUES
-- Features de Pedidos
('pedido_whatsapp', 'Pedidos por WhatsApp', 'Permite enviar pedidos directamente a WhatsApp en lugar de exportar lista', 'String', 'pedidos'),
('pedido_campos_requeridos', 'Campos Requeridos en Pedido', 'Define campos adicionales obligatorios al realizar pedidos', 'Json', 'pedidos'),

-- Features de Autenticación
('cliente_autenticacion', 'Autenticación Obligatoria', 'Requiere que clientes se autentiquen para navegar el catálogo', 'Boolean', 'seguridad'),
('cliente_mayoristas', 'Clientes Mayoristas', 'Permite a clientes mayoristas acceder a precios especiales', 'Boolean', 'seguridad'),

-- Features de Catálogo
('catalogo_lista_publico', 'Lista de precios sin autenticar', 'Muestra una lista especifica si no se esta autenticado', 'String', 'catalogo'),



-- Verificar que se insertaron correctamente
SELECT 
    COUNT(*) as total_features,
    COUNT(CASE WHEN categoria = 'pedidos' THEN 1 END) as pedidos,
    COUNT(CASE WHEN categoria = 'seguridad' THEN 1 END) as seguridad,
    COUNT(CASE WHEN categoria = 'catalogo' THEN 1 END) as catalogo,
    COUNT(CASE WHEN categoria = 'notificaciones' THEN 1 END) as notificaciones
FROM feature_definitions
WHERE activo = true;