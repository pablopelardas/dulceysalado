-- Script para crear las tablas del sistema de pedidos
-- Fecha: 2025-08-16

-- Tabla principal de pedidos
CREATE TABLE `pedidos` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `cliente_id` INT NOT NULL,
    `empresa_id` INT NOT NULL,
    `numero` VARCHAR(50) NOT NULL,
    `fecha_pedido` DATETIME NOT NULL,
    `fecha_entrega` DATE NULL,
    `horario_entrega` VARCHAR(100) NULL,
    `direccion_entrega` VARCHAR(500) NULL,
    `observaciones` VARCHAR(1000) NULL,
    `monto_total` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    `estado` INT NOT NULL DEFAULT 0 COMMENT '0=Pendiente, 1=Aceptado, 2=Rechazado, 3=Completado, 4=Cancelado',
    `motivo_rechazo` VARCHAR(500) NULL,
    `usuario_gestion_id` INT NULL,
    `fecha_gestion` DATETIME NULL,
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    `created_by` VARCHAR(100) NULL,
    `updated_by` VARCHAR(100) NULL,
    
    -- Índices
    UNIQUE KEY `idx_pedido_numero` (`numero`),
    KEY `idx_pedido_cliente_empresa` (`cliente_id`, `empresa_id`),
    KEY `idx_pedido_estado` (`estado`),
    KEY `idx_pedido_fecha` (`fecha_pedido`),
    KEY `idx_pedido_empresa_estado_fecha` (`empresa_id`, `estado`, `fecha_pedido`),
    
    -- Claves foráneas
    CONSTRAINT `fk_pedidos_cliente` FOREIGN KEY (`cliente_id`) REFERENCES `clientes` (`id`) ON DELETE RESTRICT,
    CONSTRAINT `fk_pedidos_empresa` FOREIGN KEY (`empresa_id`) REFERENCES `empresas` (`id`) ON DELETE RESTRICT
);

-- Tabla de items de pedidos
CREATE TABLE `pedido_items` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `pedido_id` INT NOT NULL,
    `codigo_producto` VARCHAR(100) NOT NULL,
    `nombre_producto` VARCHAR(255) NOT NULL,
    `cantidad` INT NOT NULL,
    `precio_unitario` DECIMAL(10,2) NOT NULL,
    `observaciones` VARCHAR(500) NULL,
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Índices
    KEY `idx_pedido_item_pedido_id` (`pedido_id`),
    KEY `idx_pedido_item_codigo_producto` (`codigo_producto`),
    
    -- Claves foráneas
    CONSTRAINT `fk_pedido_items_pedido` FOREIGN KEY (`pedido_id`) REFERENCES `pedidos` (`id`) ON DELETE CASCADE
);

-- Insertar estados de ejemplo (opcional, para documentación)
-- Estados posibles:
-- 0 = Pendiente
-- 1 = Aceptado  
-- 2 = Rechazado
-- 3 = Completado
-- 4 = Cancelado

-- Verificar que las tablas se crearon correctamente
SHOW TABLES LIKE 'pedido%';