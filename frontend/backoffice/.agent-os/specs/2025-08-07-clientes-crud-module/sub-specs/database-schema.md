# Esquema de Base de Datos

Esta es la implementación del esquema de base de datos para la especificación detallada en @.agent-os/specs/2025-08-07-clientes-crud-module/spec.md

> Creado: 2025-08-07
> Versión: 1.0.0

## Cambios en el Esquema

### Nueva Tabla: `clientes`

```sql
CREATE TABLE clientes (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  empresa_id INTEGER NOT NULL,
  nombre VARCHAR(100) NOT NULL,
  email VARCHAR(255) NOT NULL,
  username VARCHAR(100) NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  lista_precio_id INTEGER NULL,
  activo BOOLEAN DEFAULT true,
  ultimo_login DATETIME NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  
  -- Claves foráneas
  FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE,
  FOREIGN KEY (lista_precio_id) REFERENCES listas_precio(id) ON DELETE SET NULL,
  
  -- Índices únicos
  UNIQUE KEY unique_username_per_empresa (username, empresa_id),
  UNIQUE KEY unique_email_per_empresa (email, empresa_id)
);
```

### Índices de Rendimiento

```sql
-- Índice para búsquedas de autenticación (crítico para login)
CREATE INDEX idx_clientes_login ON clientes (username, empresa_id, activo);

-- Índice para búsquedas por email
CREATE INDEX idx_clientes_email ON clientes (email, empresa_id);

-- Índice para consultas por empresa
CREATE INDEX idx_clientes_empresa ON clientes (empresa_id, activo);

-- Índice para consultas por lista de precios
CREATE INDEX idx_clientes_lista_precio ON clientes (lista_precio_id);

-- Índice para ordenamiento por nombre
CREATE INDEX idx_clientes_nombre ON clientes (nombre);

-- Índice para filtros de fecha de creación
CREATE INDEX idx_clientes_created_at ON clientes (created_at);
```

### Triggers para Auditoría

```sql
-- Trigger para actualizar updated_at automáticamente
CREATE TRIGGER trigger_clientes_updated_at
    BEFORE UPDATE ON clientes
    FOR EACH ROW
    WHEN NEW.updated_at = OLD.updated_at
    BEGIN
        UPDATE clientes SET updated_at = CURRENT_TIMESTAMP WHERE id = OLD.id;
    END;
```

## Relaciones con Tablas Existentes

### Relación con `empresas`
- **Tipo**: Many-to-One (muchos clientes pertenecen a una empresa)
- **Columna**: `clientes.empresa_id` → `empresas.id`
- **Restricción**: ON DELETE CASCADE (al eliminar empresa, se eliminan sus clientes)
- **Validación**: Solo empresas con `tipo_empresa = 'principal'` pueden crear clientes

### Relación con `listas_precio`
- **Tipo**: Many-to-One (muchos clientes pueden tener la misma lista de precios)
- **Columna**: `clientes.lista_precio_id` → `listas_precio.id`
- **Restricción**: ON DELETE SET NULL (al eliminar lista, cliente mantiene referencia NULL)
- **Comportamiento**: Si cliente no tiene lista asignada, usa la lista predeterminada de la empresa

## Migraciones

### Migración Inicial (v1.0.0)

```sql
-- Verificar que las tablas dependientes existan
SELECT name FROM sqlite_master WHERE type='table' AND name IN ('empresas', 'listas_precio');

-- Crear tabla clientes
CREATE TABLE IF NOT EXISTS clientes (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  empresa_id INTEGER NOT NULL,
  nombre VARCHAR(100) NOT NULL,
  email VARCHAR(255) NOT NULL,
  username VARCHAR(100) NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  lista_precio_id INTEGER NULL,
  activo BOOLEAN DEFAULT true,
  ultimo_login DATETIME NULL,
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE,
  FOREIGN KEY (lista_precio_id) REFERENCES listas_precio(id) ON DELETE SET NULL
);

-- Crear índices únicos
CREATE UNIQUE INDEX IF NOT EXISTS unique_username_per_empresa ON clientes (username, empresa_id);
CREATE UNIQUE INDEX IF NOT EXISTS unique_email_per_empresa ON clientes (email, empresa_id);

-- Crear índices de rendimiento
CREATE INDEX IF NOT EXISTS idx_clientes_login ON clientes (username, empresa_id, activo);
CREATE INDEX IF NOT EXISTS idx_clientes_email ON clientes (email, empresa_id);
CREATE INDEX IF NOT EXISTS idx_clientes_empresa ON clientes (empresa_id, activo);
CREATE INDEX IF NOT EXISTS idx_clientes_lista_precio ON clientes (lista_precio_id);
CREATE INDEX IF NOT EXISTS idx_clientes_nombre ON clientes (nombre);
CREATE INDEX IF NOT EXISTS idx_clientes_created_at ON clientes (created_at);

-- Crear trigger de auditoría
CREATE TRIGGER IF NOT EXISTS trigger_clientes_updated_at
    BEFORE UPDATE ON clientes
    FOR EACH ROW
    WHEN NEW.updated_at = OLD.updated_at
    BEGIN
        UPDATE clientes SET updated_at = CURRENT_TIMESTAMP WHERE id = OLD.id;
    END;
```

### Validaciones de Integridad

```sql
-- Verificar que empresa_id corresponde a empresa principal
ALTER TABLE clientes ADD CONSTRAINT check_empresa_principal 
CHECK (
  empresa_id IN (
    SELECT id FROM empresas WHERE tipo_empresa = 'principal'
  )
);

-- Verificar formato de email
ALTER TABLE clientes ADD CONSTRAINT check_email_format 
CHECK (email LIKE '%@%.%');

-- Verificar longitud mínima de username
ALTER TABLE clientes ADD CONSTRAINT check_username_length 
CHECK (LENGTH(username) >= 3);

-- Verificar que password_hash no esté vacío
ALTER TABLE clientes ADD CONSTRAINT check_password_hash 
CHECK (LENGTH(password_hash) >= 60); -- bcrypt hash mínimo
```

## Consideraciones de Seguridad

### Almacenamiento de Contraseñas
- **Algoritmo**: bcrypt con salt factor 12
- **Nunca almacenar**: Contraseñas en texto plano
- **Hash de ejemplo**: `$2b$12$KIXhMpqFKdWr5UnZW8FdZem1dL8vQ5sXjUE1hf1RjJdRb2QjKzW2m`

### Controles de Acceso
- Clientes solo pueden acceder a datos de su propia empresa
- Validación de `empresa_id` en todas las consultas
- Índices optimizados para consultas filtradas por empresa

### Auditoría
- `created_at`: Registro de creación del cliente
- `updated_at`: Última modificación (auto-actualizado)
- `ultimo_login`: Rastreo de accesos para seguridad

## Datos de Prueba

### Cliente de Ejemplo
```sql
INSERT INTO clientes (
  empresa_id,
  nombre,
  email,
  username,
  password_hash,
  lista_precio_id,
  activo
) VALUES (
  1,
  'Juan Pérez',
  'juan.perez@empresa-cliente.com',
  'jperez',
  '$2b$12$KIXhMpqFKdWr5UnZW8FdZem1dL8vQ5sXjUE1hf1RjJdRb2QjKzW2m', -- password: "cliente123"
  2,
  true
);
```

## Impacto en el Rendimiento

### Consultas Optimizadas
- **Login**: Índice `idx_clientes_login` optimiza autenticación
- **Búsqueda**: Índice `idx_clientes_nombre` para filtros por nombre
- **Listado por empresa**: Índice `idx_clientes_empresa` para paginación

### Estimación de Volumen
- **Empresas principales**: ~100-500
- **Clientes por empresa**: ~50-1000
- **Total estimado**: ~50,000 registros
- **Crecimiento**: ~1000 registros/mes

### Monitoreo Requerido
- Tiempo de respuesta de queries de login
- Uso de índices en consultas principales
- Fragmentación de tabla con el crecimiento