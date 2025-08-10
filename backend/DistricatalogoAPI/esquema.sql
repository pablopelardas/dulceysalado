-- Schema DistriCatalogo - Modelo Hub-and-Spoke
-- Empresa principal administra catálogo base + empresas cliente lo personalizan

USE districatalogo;

-- Tabla de empresas
CREATE TABLE empresas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(50) UNIQUE NOT NULL,
    nombre VARCHAR(200) NOT NULL,
    razon_social VARCHAR(250),
    cuit VARCHAR(15),
    telefono VARCHAR(50),
    email VARCHAR(100),
    direccion TEXT,
    
    -- Tipo de empresa
    tipo_empresa ENUM('principal', 'cliente') DEFAULT 'cliente',
    empresa_principal_id INT DEFAULT NULL, -- NULL si es la principal
    
    -- Personalización visual
    logo_url VARCHAR(500),
    colores_tema JSON, -- {"primario": "#4A90E2", "secundario": "#FF6B35"}
    favicon_url VARCHAR(500),
    
    -- URLs y dominios
    dominio_personalizado VARCHAR(100), -- miempresa.districatalogo.com
    url_whatsapp VARCHAR(200),
    url_facebook VARCHAR(200),
    url_instagram VARCHAR(200),
    
    -- Configuración del catálogo
    mostrar_precios BOOLEAN DEFAULT TRUE,
    mostrar_stock BOOLEAN DEFAULT FALSE,
    permitir_pedidos BOOLEAN DEFAULT FALSE,
    productos_por_pagina INT DEFAULT 20,
    
    -- Permisos para productos propios
    puede_agregar_productos BOOLEAN DEFAULT FALSE,
    puede_agregar_categorias BOOLEAN DEFAULT FALSE,
    
    -- Control
    activa BOOLEAN DEFAULT TRUE,
    fecha_vencimiento DATE DEFAULT NULL,
    plan ENUM('basico', 'premium', 'enterprise') DEFAULT 'basico',
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_codigo (codigo),
    INDEX idx_tipo_empresa (tipo_empresa),
    INDEX idx_activa (activa),
    INDEX idx_dominio (dominio_personalizado),
    
    FOREIGN KEY (empresa_principal_id) REFERENCES empresas(id) ON DELETE SET NULL
);

-- Tabla de categorías del catálogo base (solo empresa principal puede modificar)
CREATE TABLE categorias_base (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo_rubro INT UNIQUE NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    icono VARCHAR(50) DEFAULT '🏪',
    visible BOOLEAN DEFAULT TRUE,
    orden INT DEFAULT 0,
    color VARCHAR(7) DEFAULT '#4A90E2',
    descripcion TEXT,
    
    -- Solo la empresa principal puede crear/modificar
    created_by_empresa_id INT NOT NULL,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_codigo_rubro (codigo_rubro),
    INDEX idx_visible_orden (visible, orden),
    
    FOREIGN KEY (created_by_empresa_id) REFERENCES empresas(id) ON DELETE RESTRICT
);

-- Categorías adicionales por empresa cliente (complementan las base)
CREATE TABLE categorias_empresa (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_id INT NOT NULL,
    codigo_rubro INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    icono VARCHAR(50) DEFAULT '🏪',
    visible BOOLEAN DEFAULT TRUE,
    orden INT DEFAULT 0,
    color VARCHAR(7) DEFAULT '#4A90E2',
    descripcion TEXT,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_empresa_rubro (empresa_id, codigo_rubro),
    INDEX idx_visible_orden (visible, orden),
    UNIQUE KEY unique_empresa_rubro (empresa_id, codigo_rubro),
    
    FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE
);

-- Vista unificada de todas las categorías disponibles para una empresa
CREATE VIEW vista_categorias_empresa AS
SELECT 
    cb.id,
    cb.codigo_rubro,
    cb.nombre,
    cb.icono,
    cb.visible,
    cb.orden,
    cb.color,
    cb.descripcion,
    'base' as tipo_categoria,
    e.id as empresa_id,
    e.nombre as empresa_nombre
FROM categorias_base cb
CROSS JOIN empresas e
WHERE e.tipo_empresa = 'cliente' AND cb.visible = TRUE

UNION ALL

SELECT 
    ce.id,
    ce.codigo_rubro,
    ce.nombre,
    ce.icono,
    ce.visible,
    ce.orden,
    ce.color,
    ce.descripcion,
    'empresa' as tipo_categoria,
    ce.empresa_id,
    e.nombre as empresa_nombre
FROM categorias_empresa ce
JOIN empresas e ON ce.empresa_id = e.id
WHERE ce.visible = TRUE;

-- Tabla de productos del catálogo base (administrados por empresa principal)
CREATE TABLE productos_base (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo INT UNIQUE NOT NULL,
    descripcion VARCHAR(500) NOT NULL,
    codigo_rubro INT,
    precio DECIMAL(10,3) DEFAULT 0.000,
    existencia DECIMAL(8,2) DEFAULT 0.00,
    grupo1 INT DEFAULT NULL,
    grupo2 INT DEFAULT NULL,
    grupo3 INT DEFAULT NULL,
    fecha_alta DATE,
    fecha_modi DATE,
    imputable CHAR(1) DEFAULT 'S',
    disponible CHAR(1) DEFAULT 'S',
    codigo_ubicacion VARCHAR(50) DEFAULT NULL,
    
    -- Campos adicionales para el catálogo web
    visible BOOLEAN DEFAULT TRUE,
    destacado BOOLEAN DEFAULT FALSE,
    orden_categoria INT DEFAULT 0,
    imagen_url VARCHAR(500) DEFAULT NULL,
    imagen_alt VARCHAR(255) DEFAULT NULL,
    descripcion_corta VARCHAR(200) DEFAULT NULL,
    descripcion_larga TEXT DEFAULT NULL,
    tags VARCHAR(500) DEFAULT NULL,
    codigo_barras VARCHAR(100) DEFAULT NULL,
    marca VARCHAR(100) DEFAULT NULL,
    unidad_medida VARCHAR(20) DEFAULT 'UN',
    
    -- Control de administración (solo empresa principal)
    administrado_por_empresa_id INT NOT NULL,
    actualizado_gecom TIMESTAMP NULL,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Índices para performance
    INDEX idx_codigo (codigo),
    INDEX idx_codigo_rubro (codigo_rubro),
    INDEX idx_visible_categoria (visible, codigo_rubro),
    INDEX idx_destacado (destacado),
    INDEX idx_precio (precio),
    INDEX idx_actualizado_gecom (actualizado_gecom),
    INDEX idx_marca (marca),
    INDEX idx_codigo_barras (codigo_barras),
    FULLTEXT idx_busqueda (descripcion, descripcion_corta, descripcion_larga, tags, marca),
    
    FOREIGN KEY (codigo_rubro) REFERENCES categorias_base(codigo_rubro) 
        ON DELETE SET NULL ON UPDATE CASCADE,
    FOREIGN KEY (administrado_por_empresa_id) REFERENCES empresas(id) ON DELETE RESTRICT
);

-- Productos propios de cada empresa cliente (complementan el catálogo base)
CREATE TABLE productos_empresa (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_id INT NOT NULL,
    codigo INT NOT NULL, -- Código interno de la empresa
    descripcion VARCHAR(500) NOT NULL,
    codigo_rubro INT,
    precio DECIMAL(10,3) DEFAULT 0.000,
    existencia DECIMAL(8,2) DEFAULT 0.00,
    
    -- Campos adicionales
    visible BOOLEAN DEFAULT TRUE,
    destacado BOOLEAN DEFAULT FALSE,
    orden_categoria INT DEFAULT 0,
    imagen_url VARCHAR(500) DEFAULT NULL,
    imagen_alt VARCHAR(255) DEFAULT NULL,
    descripcion_corta VARCHAR(200) DEFAULT NULL,
    descripcion_larga TEXT DEFAULT NULL,
    tags VARCHAR(500) DEFAULT NULL,
    codigo_barras VARCHAR(100) DEFAULT NULL,
    marca VARCHAR(100) DEFAULT NULL,
    unidad_medida VARCHAR(20) DEFAULT 'UN',
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Índices
    INDEX idx_empresa_codigo (empresa_id, codigo),
    INDEX idx_codigo_rubro (codigo_rubro),
    INDEX idx_visible_categoria (visible, codigo_rubro),
    INDEX idx_destacado (destacado),
    INDEX idx_precio (precio),
    FULLTEXT idx_busqueda (descripcion, descripcion_corta, descripcion_larga, tags, marca),
    
    UNIQUE KEY unique_empresa_codigo (empresa_id, codigo),
    
    FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE
);

-- Vista unificada del catálogo completo para cada empresa
CREATE VIEW vista_catalogo_empresa AS
SELECT 
    pb.id,
    pb.codigo,
    pb.descripcion,
    pb.codigo_rubro,
    pb.precio,
    pb.existencia,
    pb.visible,
    pb.destacado,
    pb.orden_categoria,
    pb.imagen_url,
    pb.imagen_alt,
    pb.descripcion_corta,
    pb.descripcion_larga,
    pb.tags,
    pb.codigo_barras,
    pb.marca,
    pb.unidad_medida,
    'base' as tipo_producto,
    e.id as empresa_id,
    e.nombre as empresa_nombre,
    pb.updated_at
FROM productos_base pb
CROSS JOIN empresas e
WHERE e.tipo_empresa = 'cliente' AND pb.visible = TRUE

UNION ALL

SELECT 
    pe.id,
    pe.codigo,
    pe.descripcion,
    pe.codigo_rubro,
    pe.precio,
    pe.existencia,
    pe.visible,
    pe.destacado,
    pe.orden_categoria,
    pe.imagen_url,
    pe.imagen_alt,
    pe.descripcion_corta,
    pe.descripcion_larga,
    pe.tags,
    pe.codigo_barras,
    pe.marca,
    pe.unidad_medida,
    'empresa' as tipo_producto,
    pe.empresa_id,
    e.nombre as empresa_nombre,
    pe.updated_at
FROM productos_empresa pe
JOIN empresas e ON pe.empresa_id = e.id
WHERE pe.visible = TRUE;

-- Tabla de imágenes de productos (funciona para ambos tipos)
CREATE TABLE producto_imagenes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    tipo_producto ENUM('base', 'empresa') NOT NULL,
    producto_id INT NOT NULL,
    empresa_id INT DEFAULT NULL, -- NULL para productos base, requerido para productos empresa
    url_imagen VARCHAR(500) NOT NULL,
    alt_text VARCHAR(255) DEFAULT NULL,
    es_principal BOOLEAN DEFAULT FALSE,
    orden INT DEFAULT 0,
    tipo_imagen ENUM('principal', 'galeria', 'miniatura') DEFAULT 'galeria',
    size_bytes INT DEFAULT NULL,
    width_px INT DEFAULT NULL,
    height_px INT DEFAULT NULL,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_tipo_producto_id (tipo_producto, producto_id),
    INDEX idx_empresa_id (empresa_id),
    INDEX idx_es_principal (es_principal),
    INDEX idx_tipo_orden (tipo_imagen, orden)
);

-- Tabla de usuarios/administradores
CREATE TABLE usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_id INT NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    rol ENUM('admin', 'editor', 'viewer') DEFAULT 'editor',
    
    -- Permisos específicos
    puede_gestionar_productos_base BOOLEAN DEFAULT FALSE, -- Solo para empresa principal
    puede_gestionar_productos_empresa BOOLEAN DEFAULT FALSE,
    puede_gestionar_categorias_base BOOLEAN DEFAULT FALSE, -- Solo para empresa principal
    puede_gestionar_categorias_empresa BOOLEAN DEFAULT FALSE,
    puede_gestionar_usuarios BOOLEAN DEFAULT FALSE,
    puede_ver_estadisticas BOOLEAN DEFAULT TRUE,
    
    activo BOOLEAN DEFAULT TRUE,
    ultimo_login TIMESTAMP NULL,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_empresa_email (empresa_id, email),
    INDEX idx_activo (activo),
    
    FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE
);

-- Tabla de configuración global del sistema
CREATE TABLE configuracion_sistema (
    id INT AUTO_INCREMENT PRIMARY KEY,
    clave VARCHAR(100) UNIQUE NOT NULL,
    valor TEXT,
    tipo ENUM('string', 'number', 'boolean', 'json') DEFAULT 'string',
    descripcion VARCHAR(255) DEFAULT NULL,
    publico BOOLEAN DEFAULT FALSE,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_clave (clave),
    INDEX idx_publico (publico)
);

-- Logs de sincronización con Gecom (solo para empresa principal)
CREATE TABLE sync_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_principal_id INT NOT NULL,
    archivo_nombre VARCHAR(255) NOT NULL,
    fecha_procesamiento TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    productos_actualizados INT DEFAULT 0,
    productos_nuevos INT DEFAULT 0,
    errores INT DEFAULT 0,
    tiempo_procesamiento_ms INT DEFAULT NULL,
    estado ENUM('exitoso', 'con_errores', 'fallido') DEFAULT 'exitoso',
    detalles_errores TEXT DEFAULT NULL,
    usuario_proceso VARCHAR(100) DEFAULT NULL,
    
    INDEX idx_empresa_fecha (empresa_principal_id, fecha_procesamiento),
    INDEX idx_estado (estado),
    
    FOREIGN KEY (empresa_principal_id) REFERENCES empresas(id) ON DELETE CASCADE
);

-- ===== TRIGGERS PARA VALIDACIONES DE NEGOCIO =====

-- Trigger para validar que solo empresas principales pueden crear categorías base
DELIMITER $
CREATE TRIGGER trg_categorias_base_check_empresa 
BEFORE INSERT ON categorias_base
FOR EACH ROW
BEGIN
    DECLARE empresa_tipo VARCHAR(20);
    
    SELECT tipo_empresa INTO empresa_tipo 
    FROM empresas 
    WHERE id = NEW.created_by_empresa_id;
    
    IF empresa_tipo != 'principal' THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Solo las empresas principales pueden crear categorías base';
    END IF;
END$

-- Trigger para validar que solo empresas principales pueden administrar productos base
CREATE TRIGGER trg_productos_base_check_empresa 
BEFORE INSERT ON productos_base
FOR EACH ROW
BEGIN
    DECLARE empresa_tipo VARCHAR(20);
    
    SELECT tipo_empresa INTO empresa_tipo 
    FROM empresas 
    WHERE id = NEW.administrado_por_empresa_id;
    
    IF empresa_tipo != 'principal' THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Solo las empresas principales pueden administrar productos base';
    END IF;
END$

-- Trigger para validar permisos de productos empresa
CREATE TRIGGER trg_productos_empresa_check_permisos 
BEFORE INSERT ON productos_empresa
FOR EACH ROW
BEGIN
    DECLARE empresa_tipo VARCHAR(20);
    DECLARE puede_productos BOOLEAN;
    
    SELECT tipo_empresa, puede_agregar_productos 
    INTO empresa_tipo, puede_productos
    FROM empresas 
    WHERE id = NEW.empresa_id;
    
    IF empresa_tipo != 'cliente' OR puede_productos != TRUE THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'La empresa no tiene permisos para agregar productos propios';
    END IF;
END$

-- Trigger para validar permisos de categorías empresa
CREATE TRIGGER trg_categorias_empresa_check_permisos 
BEFORE INSERT ON categorias_empresa
FOR EACH ROW
BEGIN
    DECLARE empresa_tipo VARCHAR(20);
    DECLARE puede_categorias BOOLEAN;
    
    SELECT tipo_empresa, puede_agregar_categorias 
    INTO empresa_tipo, puede_categorias
    FROM empresas 
    WHERE id = NEW.empresa_id;
    
    IF empresa_tipo != 'cliente' OR puede_categorias != TRUE THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'La empresa no tiene permisos para agregar categorías propias';
    END IF;
END$

-- Trigger para validar logs de sync solo para empresas principales
CREATE TRIGGER trg_sync_logs_check_empresa 
BEFORE INSERT ON sync_logs
FOR EACH ROW
BEGIN
    DECLARE empresa_tipo VARCHAR(20);
    
    SELECT tipo_empresa INTO empresa_tipo 
    FROM empresas 
    WHERE id = NEW.empresa_principal_id;
    
    IF empresa_tipo != 'principal' THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Solo las empresas principales pueden tener logs de sincronización';
    END IF;
END$

DELIMITER ;

-- ===== DATOS INICIALES =====

-- Empresa principal (administra el catálogo base)
INSERT INTO empresas (codigo, nombre, razon_social, tipo_empresa, colores_tema, activa) VALUES
('PRINCIPAL', 'DistriCatalogo Master', 'DistriCatalogo S.A.', 'principal', 
 '{"primario": "#4A90E2", "secundario": "#FF6B35", "acento": "#8BC34A"}', TRUE);

SET @empresa_principal_id = LAST_INSERT_ID();

-- Empresa cliente de demo
INSERT INTO empresas (codigo, nombre, razon_social, tipo_empresa, empresa_principal_id, 
                     puede_agregar_productos, puede_agregar_categorias, colores_tema, activa) VALUES
('DEMO_CLIENTE', 'Distribuidora Demo Cliente', 'Distribuidora Demo Cliente S.R.L.', 'cliente', 
 @empresa_principal_id, TRUE, TRUE, 
 '{"primario": "#9333ea", "secundario": "#f59e0b", "acento": "#10b981"}', TRUE);

SET @empresa_cliente_id = LAST_INSERT_ID();

-- Categorías base (administradas por empresa principal)
INSERT INTO categorias_base (codigo_rubro, nombre, icono, orden, color, created_by_empresa_id) VALUES
(1, 'Quesos y Fiambres', '🧀', 10, '#FF6B35', @empresa_principal_id),
(2, 'Almacén', '🛒', 20, '#4A90E2', @empresa_principal_id),
(4, 'Congelados', '❄️', 30, '#38bdf8', @empresa_principal_id),
(7, 'Lácteos', '🥛', 40, '#8BC34A', @empresa_principal_id),
(10, 'Varios', '🏪', 50, '#9333ea', @empresa_principal_id);

-- Configuraciones globales del sistema
INSERT INTO configuracion_sistema (clave, valor, tipo, descripcion, publico) VALUES
('app_nombre', 'DistriCatalogo', 'string', 'Nombre de la aplicación', TRUE),
('app_version', '1.0.0', 'string', 'Versión de la aplicación', TRUE),
('app_descripcion', 'Plataforma de catálogos para distribuidoras', 'string', 'Descripción de la aplicación', TRUE),
('max_empresas_cliente', '100', 'number', 'Máximo número de empresas cliente permitidas', FALSE),
('upload_max_size', '10485760', 'number', 'Tamaño máximo de upload en bytes (10MB)', FALSE),
('backup_retention_days', '30', 'number', 'Días de retención de backups', FALSE),
('sync_timeout_minutes', '30', 'number', 'Timeout para sincronización en minutos', FALSE);

-- Usuario admin de empresa principal
INSERT INTO usuarios (empresa_id, email, password_hash, nombre, apellido, rol, 
                     puede_gestionar_productos_base, puede_gestionar_categorias_base, 
                     puede_gestionar_usuarios, activo) VALUES
(@empresa_principal_id, 'admin@principal.com', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 
 'Admin', 'Principal', 'admin', TRUE, TRUE, TRUE, TRUE);

-- Usuario admin de empresa cliente demo
INSERT INTO usuarios (empresa_id, email, password_hash, nombre, apellido, rol, 
                     puede_gestionar_productos_empresa, puede_gestionar_categorias_empresa, activo) VALUES
(@empresa_cliente_id, 'admin@cliente.com', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 
 'Admin', 'Cliente', 'admin', TRUE, TRUE, TRUE);

-- Ver estructura creada
SHOW TABLES;

-- Verificar empresas y sus relaciones
SELECT 
    e1.codigo,
    e1.nombre,
    e1.tipo_empresa,
    e2.nombre as empresa_principal,
    e1.puede_agregar_productos,
    e1.puede_agregar_categorias
FROM empresas e1
LEFT JOIN empresas e2 ON e1.empresa_principal_id = e2.id
ORDER BY e1.tipo_empresa, e1.nombre;url VARCHAR(500) DEFAULT NULL,
    imagen_alt VARCHAR(255) DEFAULT NULL,
    descripcion_corta VARCHAR(200) DEFAULT NULL,
    descripcion_larga TEXT DEFAULT NULL,
    tags VARCHAR(500) DEFAULT NULL,
    codigo_barras VARCHAR(100) DEFAULT NULL,
    marca VARCHAR(100) DEFAULT NULL,
    unidad_medida VARCHAR(20) DEFAULT 'UN',
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Índices
    INDEX idx_empresa_codigo (empresa_id, codigo),
    INDEX idx_codigo_rubro (codigo_rubro),
    INDEX idx_visible_categoria (visible, codigo_rubro),
    INDEX idx_destacado (destacado),
    INDEX idx_precio (precio),
    FULLTEXT idx_busqueda (descripcion, descripcion_corta, descripcion_larga, tags, marca),
    
    UNIQUE KEY unique_empresa_codigo (empresa_id, codigo),
    
    FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE,
    
    -- Solo empresas con permiso pueden tener productos propios
    CONSTRAINT chk_producto_empresa_permiso CHECK (
        empresa_id IN (
            SELECT id FROM empresas 
            WHERE tipo_empresa = 'cliente' AND puede_agregar_productos = TRUE
        )
    ),
    
    -- No puede duplicar códigos de productos base
    CONSTRAINT chk_no_duplicar_producto_base CHECK (
        codigo NOT IN (SELECT codigo FROM productos_base)
    )
);

-- Vista unificada del catálogo completo para cada empresa
CREATE VIEW vista_catalogo_empresa AS
SELECT 
    pb.id,
    pb.codigo,
    pb.descripcion,
    pb.codigo_rubro,
    pb.precio,
    pb.existencia,
    pb.visible,
    pb.destacado,
    pb.orden_categoria,
    pb.imagen_url,
    pb.imagen_alt,
    pb.descripcion_corta,
    pb.descripcion_larga,
    pb.tags,
    pb.codigo_barras,
    pb.marca,
    pb.unidad_medida,
    'base' as tipo_producto,
    e.id as empresa_id,
    e.nombre as empresa_nombre,
    pb.updated_at
FROM productos_base pb
CROSS JOIN empresas e
WHERE e.tipo_empresa = 'cliente' AND pb.visible = TRUE

UNION ALL

SELECT 
    pe.id,
    pe.codigo,
    pe.descripcion,
    pe.codigo_rubro,
    pe.precio,
    pe.existencia,
    pe.visible,
    pe.destacado,
    pe.orden_categoria,
    pe.imagen_url,
    pe.imagen_alt,
    pe.descripcion_corta,
    pe.descripcion_larga,
    pe.tags,
    pe.codigo_barras,
    pe.marca,
    pe.unidad_medida,
    'empresa' as tipo_producto,
    pe.empresa_id,
    e.nombre as empresa_nombre,
    pe.updated_at
FROM productos_empresa pe
JOIN empresas e ON pe.empresa_id = e.id
WHERE pe.visible = TRUE;

-- Tabla de imágenes de productos (funciona para ambos tipos)
CREATE TABLE producto_imagenes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    tipo_producto ENUM('base', 'empresa') NOT NULL,
    producto_id INT NOT NULL,
    empresa_id INT DEFAULT NULL, -- NULL para productos base, requerido para productos empresa
    url_imagen VARCHAR(500) NOT NULL,
    alt_text VARCHAR(255) DEFAULT NULL,
    es_principal BOOLEAN DEFAULT FALSE,
    orden INT DEFAULT 0,
    tipo_imagen ENUM('principal', 'galeria', 'miniatura') DEFAULT 'galeria',
    size_bytes INT DEFAULT NULL,
    width_px INT DEFAULT NULL,
    height_px INT DEFAULT NULL,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_tipo_producto_id (tipo_producto, producto_id),
    INDEX idx_empresa_id (empresa_id),
    INDEX idx_es_principal (es_principal),
    INDEX idx_tipo_orden (tipo_imagen, orden),
    
    CONSTRAINT chk_empresa_id_logic CHECK (
        (tipo_producto = 'base' AND empresa_id IS NULL) OR
        (tipo_producto = 'empresa' AND empresa_id IS NOT NULL)
    )
);

-- Tabla de usuarios/administradores
CREATE TABLE usuarios (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_id INT NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100) NOT NULL,
    rol ENUM('admin', 'editor', 'viewer') DEFAULT 'editor',
    
    -- Permisos específicos
    puede_gestionar_productos_base BOOLEAN DEFAULT FALSE, -- Solo para empresa principal
    puede_gestionar_productos_empresa BOOLEAN DEFAULT FALSE,
    puede_gestionar_categorias_base BOOLEAN DEFAULT FALSE, -- Solo para empresa principal
    puede_gestionar_categorias_empresa BOOLEAN DEFAULT FALSE,
    puede_gestionar_usuarios BOOLEAN DEFAULT FALSE,
    puede_ver_estadisticas BOOLEAN DEFAULT TRUE,
    
    activo BOOLEAN DEFAULT TRUE,
    ultimo_login TIMESTAMP NULL,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_empresa_email (empresa_id, email),
    INDEX idx_activo (activo),
    
    FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE
);

-- Tabla de configuración global del sistema
CREATE TABLE configuracion_sistema (
    id INT AUTO_INCREMENT PRIMARY KEY,
    clave VARCHAR(100) UNIQUE NOT NULL,
    valor TEXT,
    tipo ENUM('string', 'number', 'boolean', 'json') DEFAULT 'string',
    descripcion VARCHAR(255) DEFAULT NULL,
    publico BOOLEAN DEFAULT FALSE,
    
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_clave (clave),
    INDEX idx_publico (publico)
);

-- Logs de sincronización con Gecom (solo para empresa principal)
CREATE TABLE sync_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    empresa_principal_id INT NOT NULL,
    archivo_nombre VARCHAR(255) NOT NULL,
    fecha_procesamiento TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    productos_actualizados INT DEFAULT 0,
    productos_nuevos INT DEFAULT 0,
    errores INT DEFAULT 0,
    tiempo_procesamiento_ms INT DEFAULT NULL,
    estado ENUM('exitoso', 'con_errores', 'fallido') DEFAULT 'exitoso',
    detalles_errores TEXT DEFAULT NULL,
    usuario_proceso VARCHAR(100) DEFAULT NULL,
    
    INDEX idx_empresa_fecha (empresa_principal_id, fecha_procesamiento),
    INDEX idx_estado (estado),
    
    FOREIGN KEY (empresa_principal_id) REFERENCES empresas(id) ON DELETE CASCADE,
    
    -- Solo empresas principales pueden tener logs de sync
    CONSTRAINT chk_sync_empresa_principal CHECK (
        empresa_principal_id IN (
            SELECT id FROM empresas WHERE tipo_empresa = 'principal'
        )
    )
);

-- ===== DATOS INICIALES =====

-- Empresa principal (administra el catálogo base)
INSERT INTO empresas (codigo, nombre, razon_social, tipo_empresa, colores_tema, activa) VALUES
('PRINCIPAL', 'DistriCatalogo Master', 'DistriCatalogo S.A.', 'principal', 
 '{"primario": "#4A90E2", "secundario": "#FF6B35", "acento": "#8BC34A"}', TRUE);

SET @empresa_principal_id = LAST_INSERT_ID();

-- Empresa cliente de demo
INSERT INTO empresas (codigo, nombre, razon_social, tipo_empresa, empresa_principal_id, 
                     puede_agregar_productos, puede_agregar_categorias, colores_tema, activa) VALUES
('DEMO_CLIENTE', 'Distribuidora Demo Cliente', 'Distribuidora Demo Cliente S.R.L.', 'cliente', 
 @empresa_principal_id, TRUE, TRUE, 
 '{"primario": "#9333ea", "secundario": "#f59e0b", "acento": "#10b981"}', TRUE);

SET @empresa_cliente_id = LAST_INSERT_ID();

-- Categorías base (administradas por empresa principal)
INSERT INTO categorias_base (codigo_rubro, nombre, icono, orden, color, created_by_empresa_id) VALUES
(1, 'Quesos y Fiambres', '🧀', 10, '#FF6B35', @empresa_principal_id),
(2, 'Almacén', '🛒', 20, '#4A90E2', @empresa_principal_id),
(4, 'Congelados', '❄️', 30, '#38bdf8', @empresa_principal_id),
(7, 'Lácteos', '🥛', 40, '#8BC34A', @empresa_principal_id),
(10, 'Varios', '🏪', 50, '#9333ea', @empresa_principal_id);

-- Configuraciones globales del sistema
INSERT INTO configuracion_sistema (clave, valor, tipo, descripcion, publico) VALUES
('app_nombre', 'DistriCatalogo', 'string', 'Nombre de la aplicación', TRUE),
('app_version', '1.0.0', 'string', 'Versión de la aplicación', TRUE),
('app_descripcion', 'Plataforma de catálogos para distribuidoras', 'string', 'Descripción de la aplicación', TRUE),
('max_empresas_cliente', '100', 'number', 'Máximo número de empresas cliente permitidas', FALSE),
('upload_max_size', '10485760', 'number', 'Tamaño máximo de upload en bytes (10MB)', FALSE),
('backup_retention_days', '30', 'number', 'Días de retención de backups', FALSE),
('sync_timeout_minutes', '30', 'number', 'Timeout para sincronización en minutos', FALSE);

-- Usuario admin de empresa principal
INSERT INTO usuarios (empresa_id, email, password_hash, nombre, apellido, rol, 
                     puede_gestionar_productos_base, puede_gestionar_categorias_base, 
                     puede_gestionar_usuarios, activo) VALUES
(@empresa_principal_id, 'admin@principal.com', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 
 'Admin', 'Principal', 'admin', TRUE, TRUE, TRUE, TRUE);

-- Usuario admin de empresa cliente demo
INSERT INTO usuarios (empresa_id, email, password_hash, nombre, apellido, rol, 
                     puede_gestionar_productos_empresa, puede_gestionar_categorias_empresa, activo) VALUES
(@empresa_cliente_id, 'admin@cliente.com', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 
 'Admin', 'Cliente', 'admin', TRUE, TRUE, TRUE);

-- Ver estructura creada
SHOW TABLES;

-- Verificar empresas y sus relaciones
SELECT 
    e1.codigo,
    e1.nombre,
    e1.tipo_empresa,
    e2.nombre as empresa_principal,
    e1.puede_agregar_productos,
    e1.puede_agregar_categorias
FROM empresas e1
LEFT JOIN empresas e2 ON e1.empresa_principal_id = e2.id
ORDER BY e1.tipo_empresa, e1.nombre;