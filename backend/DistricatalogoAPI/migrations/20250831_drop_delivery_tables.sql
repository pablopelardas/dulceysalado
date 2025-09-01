-- =============================================
-- Borrar y recrear tablas de delivery con diseño correcto
-- Fecha: 2025-08-31
-- =============================================

-- Borrar tablas en orden correcto (respetando foreign keys)
DROP TABLE IF EXISTS delivery_slots;
DROP TABLE IF EXISTS delivery_schedules;
DROP TABLE IF EXISTS delivery_settings;