-- =============================================
-- Creación del sistema de franjas horarias de entrega
-- Fecha: 2025-08-31
-- =============================================

-- Crear tabla delivery_settings
CREATE TABLE delivery_settings (
    id INT NOT NULL AUTO_INCREMENT,
    empresa_id INT NOT NULL,
    min_slots_ahead INT NOT NULL DEFAULT 2 COMMENT 'Mínimo de slots que se deben reservar con anticipación',
    max_capacity_morning INT NOT NULL DEFAULT 10 COMMENT 'Capacidad máxima franja mañana (9-12)',
    max_capacity_afternoon INT NOT NULL DEFAULT 10 COMMENT 'Capacidad máxima franja tarde (14-18)',
    monday_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    tuesday_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    wednesday_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    thursday_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    friday_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    saturday_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    sunday_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY idx_delivery_settings_empresa (empresa_id),
    CONSTRAINT fk_delivery_settings_empresa 
        FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Configuración de franjas horarias por empresa';

-- Crear tabla delivery_schedules (horarios personalizados por día)
CREATE TABLE delivery_schedules (
    id INT NOT NULL AUTO_INCREMENT,
    delivery_settings_id INT NOT NULL,
    date DATE NOT NULL COMMENT 'Fecha específica para configuración personalizada',
    morning_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    afternoon_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    custom_max_capacity_morning INT NULL COMMENT 'Capacidad personalizada para mañana este día',
    custom_max_capacity_afternoon INT NULL COMMENT 'Capacidad personalizada para tarde este día',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY idx_delivery_schedule_settings_date (delivery_settings_id, date),
    CONSTRAINT fk_delivery_schedule_settings 
        FOREIGN KEY (delivery_settings_id) REFERENCES delivery_settings(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Configuración de horarios específicos por día';

-- Crear tabla delivery_slots (slots reservados)
CREATE TABLE delivery_slots (
    id INT NOT NULL AUTO_INCREMENT,
    delivery_settings_id INT NOT NULL,
    delivery_schedule_id INT NULL COMMENT 'Referencia opcional a configuración específica del día',
    date DATE NOT NULL,
    slot_type INT NOT NULL COMMENT '0=Mañana (9-12), 1=Tarde (14-18)',
    current_capacity INT NOT NULL DEFAULT 0 COMMENT 'Capacidad actual utilizada',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY idx_delivery_slot_unique (delivery_settings_id, date, slot_type),
    KEY idx_delivery_slot_date_type (date, slot_type),
    CONSTRAINT fk_delivery_slot_settings 
        FOREIGN KEY (delivery_settings_id) REFERENCES delivery_settings(id) ON DELETE CASCADE,
    CONSTRAINT fk_delivery_slot_schedule 
        FOREIGN KEY (delivery_schedule_id) REFERENCES delivery_schedules(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Slots de entrega reservados por fecha y franja';

-- Insertar configuración por defecto para empresa 1 (ejemplo)
INSERT INTO delivery_settings (
    empresa_id, 
    min_slots_ahead, 
    max_capacity_morning, 
    max_capacity_afternoon,
    monday_enabled,
    tuesday_enabled,
    wednesday_enabled,
    thursday_enabled,
    friday_enabled,
    saturday_enabled,
    sunday_enabled
) VALUES (
    1, 
    2, 
    10, 
    10,
    TRUE,
    TRUE,
    TRUE,
    TRUE,
    TRUE,
    FALSE,
    FALSE
) ON DUPLICATE KEY UPDATE 
    min_slots_ahead = VALUES(min_slots_ahead),
    max_capacity_morning = VALUES(max_capacity_morning),
    max_capacity_afternoon = VALUES(max_capacity_afternoon);

-- Índices adicionales para optimización
CREATE INDEX idx_delivery_settings_created_at ON delivery_settings(created_at);
CREATE INDEX idx_delivery_schedules_date ON delivery_schedules(date);
CREATE INDEX idx_delivery_slots_capacity ON delivery_slots(current_capacity);