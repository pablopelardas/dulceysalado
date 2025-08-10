# Database Schema - Sistema Feature Flags

## Tablas Principales

### 1. feature_definitions

Catálogo maestro de todas las features disponibles en el sistema.

```sql
CREATE TABLE feature_definitions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(100) NOT NULL UNIQUE,
    nombre VARCHAR(255) NOT NULL,
    descripcion TEXT,
    tipo_valor ENUM('boolean', 'string', 'number', 'json') DEFAULT 'boolean',
    valor_defecto TEXT,
    categoria VARCHAR(100),
    activo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_codigo (codigo),
    INDEX idx_categoria (categoria),
    INDEX idx_activo (activo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

**Columnas**:
- `id`: Identificador único
- `codigo`: Código único de la feature (ej: "pedido_whatsapp")
- `nombre`: Nombre descriptivo para UI
- `descripcion`: Explicación detallada de la funcionalidad
- `tipo_valor`: Tipo de dato que acepta la feature
- `valor_defecto`: Valor por defecto si no está configurado
- `categoria`: Agrupación lógica (pedidos, seguridad, catalogo, etc)
- `activo`: Si la feature está disponible para configurar

### 2. empresa_features

Configuración específica de features por empresa.

```sql
CREATE TABLE empresa_features (
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
```

**Columnas**:
- `id`: Identificador único
- `empresa_id`: FK a la empresa
- `feature_id`: FK a la definición de feature
- `habilitado`: Si está activa para esta empresa
- `valor`: Valor configurado (respetando el tipo definido)
- `metadata`: Datos adicionales en formato JSON
- `created_by/updated_by`: Auditoría de usuario

## Datos de Ejemplo

### Insertar Feature Definitions

```sql
-- Features de Pedidos
INSERT INTO feature_definitions (codigo, nombre, descripcion, tipo_valor, categoria) VALUES
('pedido_whatsapp', 'Pedidos por WhatsApp', 'Permite enviar pedidos directamente a WhatsApp', 'string', 'pedidos'),
('pedido_campos_requeridos', 'Campos Requeridos en Pedido', 'Lista de campos adicionales obligatorios', 'json', 'pedidos'),
('pedido_datos_adicionales', 'Capturar Datos Extra', 'Habilita formulario de datos adicionales', 'boolean', 'pedidos');

-- Features de Autenticación
INSERT INTO feature_definitions (codigo, nombre, descripcion, tipo_valor, categoria) VALUES
('cliente_autenticacion', 'Autenticación Obligatoria', 'Requiere login para navegar catálogo', 'boolean', 'seguridad'),
('cliente_registro_publico', 'Registro Público', 'Permite registro sin aprobación', 'boolean', 'seguridad');

-- Features de Catálogo
INSERT INTO feature_definitions (codigo, nombre, descripcion, tipo_valor, valor_defecto, categoria) VALUES
('catalogo_precios_ocultos', 'Ocultar Precios', 'No muestra precios hasta autenticarse', 'boolean', 'false', 'catalogo'),
('catalogo_stock_visible', 'Mostrar Stock', 'Muestra cantidad disponible', 'boolean', 'true', 'catalogo'),
('catalogo_descuento_maximo', 'Descuento Máximo %', 'Porcentaje máximo de descuento', 'number', '30', 'catalogo');

-- Features de Notificaciones
INSERT INTO feature_definitions (codigo, nombre, descripcion, tipo_valor, categoria) VALUES
('notificacion_email', 'Notificaciones Email', 'Enviar notificaciones por email', 'boolean', 'notificaciones'),
('notificacion_sms', 'Notificaciones SMS', 'Enviar notificaciones por SMS', 'boolean', 'notificaciones'),
('notificacion_webhook', 'Webhook URL', 'URL para notificaciones webhook', 'string', 'notificaciones');
```

### Configurar Features para Empresa

```sql
-- Empresa 1: Configuración WhatsApp
INSERT INTO empresa_features (empresa_id, feature_id, habilitado, valor, metadata, created_by) 
SELECT 1, id, true, '+5491123456789', 
    '{"mensaje_template": "Nuevo pedido:\\n{{items}}\\nTotal: ${{total}}", "incluir_direccion": true}',
    'admin@empresa1.com'
FROM feature_definitions WHERE codigo = 'pedido_whatsapp';

-- Empresa 1: Campos requeridos
INSERT INTO empresa_features (empresa_id, feature_id, habilitado, valor, metadata, created_by)
SELECT 1, id, true, 
    '["nombre", "numero_cliente", "telefono", "direccion_entrega"]',
    '{"validaciones": {"numero_cliente": {"regex": "^[0-9]{6}$", "mensaje": "Debe ser 6 dígitos"}}}',
    'admin@empresa1.com'
FROM feature_definitions WHERE codigo = 'pedido_campos_requeridos';

-- Empresa 2: Sin autenticación pero precios ocultos
INSERT INTO empresa_features (empresa_id, feature_id, habilitado, valor, created_by)
SELECT 2, id, false, null, 'admin@empresa2.com'
FROM feature_definitions WHERE codigo = 'cliente_autenticacion';

INSERT INTO empresa_features (empresa_id, feature_id, habilitado, valor, metadata, created_by)
SELECT 2, id, true, 'true', '{"mensaje": "Contacte para ver precios"}', 'admin@empresa2.com'
FROM feature_definitions WHERE codigo = 'catalogo_precios_ocultos';
```

## Consultas Útiles

### 1. Obtener todas las features de una empresa

```sql
SELECT 
    fd.codigo,
    fd.nombre,
    fd.tipo_valor,
    COALESCE(ef.habilitado, false) as habilitado,
    COALESCE(ef.valor, fd.valor_defecto) as valor,
    ef.metadata,
    ef.updated_at,
    ef.updated_by
FROM feature_definitions fd
LEFT JOIN empresa_features ef 
    ON fd.id = ef.feature_id 
    AND ef.empresa_id = ?
WHERE fd.activo = true
ORDER BY fd.categoria, fd.nombre;
```

### 2. Verificar si una feature está habilitada

```sql
SELECT 
    COALESCE(ef.habilitado, false) as habilitado,
    COALESCE(ef.valor, fd.valor_defecto) as valor,
    ef.metadata
FROM feature_definitions fd
LEFT JOIN empresa_features ef 
    ON fd.id = ef.feature_id 
    AND ef.empresa_id = ?
WHERE fd.codigo = ?
    AND fd.activo = true;
```

### 3. Features más usadas

```sql
SELECT 
    fd.codigo,
    fd.nombre,
    COUNT(DISTINCT ef.empresa_id) as empresas_usando,
    COUNT(DISTINCT CASE WHEN ef.habilitado = true THEN ef.empresa_id END) as empresas_activas
FROM feature_definitions fd
LEFT JOIN empresa_features ef ON fd.id = ef.feature_id
GROUP BY fd.id
ORDER BY empresas_activas DESC;
```

### 4. Auditoría de cambios recientes

```sql
SELECT 
    e.nombre as empresa,
    fd.codigo as feature,
    ef.habilitado,
    ef.valor,
    ef.updated_at,
    ef.updated_by
FROM empresa_features ef
JOIN empresas e ON ef.empresa_id = e.id
JOIN feature_definitions fd ON ef.feature_id = fd.id
WHERE ef.updated_at > DATE_SUB(NOW(), INTERVAL 7 DAY)
ORDER BY ef.updated_at DESC;
```

## Índices Recomendados

```sql
-- Para búsquedas rápidas por empresa y feature
CREATE INDEX idx_empresa_feature_lookup 
ON empresa_features(empresa_id, feature_id, habilitado) 
INCLUDE (valor);

-- Para filtrado por categoría
CREATE INDEX idx_feature_category_active 
ON feature_definitions(categoria, activo);

-- Para auditoría
CREATE INDEX idx_updated_at 
ON empresa_features(updated_at);
```

## Migraciones

### Migration Up

```sql
-- Create feature_definitions table
CREATE TABLE IF NOT EXISTS feature_definitions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    codigo VARCHAR(100) NOT NULL UNIQUE,
    nombre VARCHAR(255) NOT NULL,
    descripcion TEXT,
    tipo_valor ENUM('boolean', 'string', 'number', 'json') DEFAULT 'boolean',
    valor_defecto TEXT,
    categoria VARCHAR(100),
    activo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_codigo (codigo),
    INDEX idx_categoria (categoria),
    INDEX idx_activo (activo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create empresa_features table
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
```

### Migration Down

```sql
DROP TABLE IF EXISTS empresa_features;
DROP TABLE IF EXISTS feature_definitions;
```

## Consideraciones

### Performance
1. Las consultas de features se realizan frecuentemente, implementar caché
2. El campo JSON metadata debe usarse con moderación
3. Índices compuestos para optimizar lookups

### Seguridad
1. Solo usuarios autorizados pueden modificar features
2. Validar que el tipo de valor coincida antes de guardar
3. Sanitizar contenido JSON en metadata

### Mantenibilidad
1. Usar categorías consistentes
2. Documentar cada feature nueva
3. Mantener valor_defecto cuando sea aplicable