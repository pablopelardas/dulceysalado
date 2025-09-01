-- =============================================
-- Sistema de franjas horarias de entrega - Diseño final
-- Solo min_slots_ahead general, todo lo demás específico por día
-- Fecha: 2025-08-31
-- =============================================

-- Crear tabla delivery_settings
CREATE TABLE delivery_settings (
    id INT NOT NULL AUTO_INCREMENT,
    empresa_id INT NOT NULL,
    min_slots_ahead INT NOT NULL DEFAULT 2 COMMENT 'Mínimo de slots que se deben reservar con anticipación',
    max_capacity_morning INT NOT NULL DEFAULT 10 COMMENT 'Capacidad máxima franja mañana',
    max_capacity_afternoon INT NOT NULL DEFAULT 10 COMMENT 'Capacidad máxima franja tarde',
    
    -- Configuración por día - Lunes
    monday_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    monday_morning_start TIME DEFAULT '09:00:00',
    monday_morning_end TIME DEFAULT '13:00:00',
    monday_afternoon_start TIME DEFAULT '14:00:00',
    monday_afternoon_end TIME DEFAULT '18:00:00',
    
    -- Configuración por día - Martes
    tuesday_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    tuesday_morning_start TIME DEFAULT '09:00:00',
    tuesday_morning_end TIME DEFAULT '13:00:00',
    tuesday_afternoon_start TIME DEFAULT '14:00:00',
    tuesday_afternoon_end TIME DEFAULT '18:00:00',
    
    -- Configuración por día - Miércoles
    wednesday_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    wednesday_morning_start TIME DEFAULT '09:00:00',
    wednesday_morning_end TIME DEFAULT '13:00:00',
    wednesday_afternoon_start TIME DEFAULT '14:00:00',
    wednesday_afternoon_end TIME DEFAULT '18:00:00',
    
    -- Configuración por día - Jueves
    thursday_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    thursday_morning_start TIME DEFAULT '09:00:00',
    thursday_morning_end TIME DEFAULT '13:00:00',
    thursday_afternoon_start TIME DEFAULT '14:00:00',
    thursday_afternoon_end TIME DEFAULT '18:00:00',
    
    -- Configuración por día - Viernes
    friday_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    friday_morning_start TIME DEFAULT '09:00:00',
    friday_morning_end TIME DEFAULT '13:00:00',
    friday_afternoon_start TIME DEFAULT '14:00:00',
    friday_afternoon_end TIME DEFAULT '18:00:00',
    
    -- Configuración por día - Sábado
    saturday_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    saturday_morning_start TIME DEFAULT '09:00:00',
    saturday_morning_end TIME DEFAULT '12:00:00',
    saturday_afternoon_start TIME DEFAULT NULL,
    saturday_afternoon_end TIME DEFAULT NULL,
    
    -- Configuración por día - Domingo
    sunday_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    sunday_morning_start TIME DEFAULT NULL,
    sunday_morning_end TIME DEFAULT NULL,
    sunday_afternoon_start TIME DEFAULT NULL,
    sunday_afternoon_end TIME DEFAULT NULL,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY idx_delivery_settings_empresa (empresa_id),
    CONSTRAINT fk_delivery_settings_empresa 
        FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Configuración de franjas horarias por empresa';

-- Crear tabla delivery_schedules (horarios personalizados por fecha específica)
CREATE TABLE delivery_schedules (
    id INT NOT NULL AUTO_INCREMENT,
    delivery_settings_id INT NOT NULL,
    date DATE NOT NULL COMMENT 'Fecha específica para configuración personalizada',
    morning_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    afternoon_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    custom_max_capacity_morning INT NULL COMMENT 'Capacidad personalizada para mañana este día',
    custom_max_capacity_afternoon INT NULL COMMENT 'Capacidad personalizada para tarde este día',
    custom_morning_start_time TIME NULL COMMENT 'Hora personalizada inicio mañana',
    custom_morning_end_time TIME NULL COMMENT 'Hora personalizada fin mañana',
    custom_afternoon_start_time TIME NULL COMMENT 'Hora personalizada inicio tarde',
    custom_afternoon_end_time TIME NULL COMMENT 'Hora personalizada fin tarde',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY idx_delivery_schedule_settings_date (delivery_settings_id, date),
    CONSTRAINT fk_delivery_schedule_settings 
        FOREIGN KEY (delivery_settings_id) REFERENCES delivery_settings(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Configuración de horarios específicos por fecha';

-- Crear tabla delivery_slots (slots reservados)
CREATE TABLE delivery_slots (
    id INT NOT NULL AUTO_INCREMENT,
    delivery_settings_id INT NOT NULL,
    delivery_schedule_id INT NULL COMMENT 'Referencia opcional a configuración específica del día',
    date DATE NOT NULL,
    slot_type INT NOT NULL COMMENT '0=Mañana, 1=Tarde',
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

-- Insertar configuración por defecto para empresa 1
INSERT INTO delivery_settings (
    empresa_id, 
    min_slots_ahead,
    max_capacity_morning,
    max_capacity_afternoon,
    -- Lunes a viernes: horario completo
    monday_enabled, monday_morning_start, monday_morning_end, monday_afternoon_start, monday_afternoon_end,
    tuesday_enabled, tuesday_morning_start, tuesday_morning_end, tuesday_afternoon_start, tuesday_afternoon_end,
    wednesday_enabled, wednesday_morning_start, wednesday_morning_end, wednesday_afternoon_start, wednesday_afternoon_end,
    thursday_enabled, thursday_morning_start, thursday_morning_end, thursday_afternoon_start, thursday_afternoon_end,
    friday_enabled, friday_morning_start, friday_morning_end, friday_afternoon_start, friday_afternoon_end,
    -- Sábado: solo mañana
    saturday_enabled, saturday_morning_start, saturday_morning_end, saturday_afternoon_start, saturday_afternoon_end,
    -- Domingo: deshabilitado
    sunday_enabled, sunday_morning_start, sunday_morning_end, sunday_afternoon_start, sunday_afternoon_end
) VALUES (
    1, 
    2,
    10, -- max_capacity_morning general
    10, -- max_capacity_afternoon general
    -- Lunes
    TRUE, '09:00:00', '13:00:00', '14:00:00', '18:00:00',
    -- Martes  
    TRUE, '09:00:00', '13:00:00', '14:00:00', '18:00:00',
    -- Miércoles
    TRUE, '09:00:00', '13:00:00', '14:00:00', '18:00:00',
    -- Jueves
    TRUE, '09:00:00', '13:00:00', '14:00:00', '18:00:00',
    -- Viernes
    TRUE, '09:00:00', '13:00:00', '14:00:00', '18:00:00',
    -- Sábado
    TRUE, '09:00:00', '12:00:00', NULL, NULL,
    -- Domingo  
    FALSE, NULL, NULL, NULL, NULL
) ON DUPLICATE KEY UPDATE 
    min_slots_ahead = VALUES(min_slots_ahead),
    max_capacity_morning = VALUES(max_capacity_morning),
    max_capacity_afternoon = VALUES(max_capacity_afternoon);

-- Índices adicionales para optimización
CREATE INDEX idx_delivery_settings_created_at ON delivery_settings(created_at);
CREATE INDEX idx_delivery_schedules_date ON delivery_schedules(date);
CREATE INDEX idx_delivery_slots_capacity ON delivery_slots(current_capacity);