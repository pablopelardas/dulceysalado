# Database Schema

This is the database schema implementation for the spec detailed in @.agent-os/specs/2025-08-01-sistema-novedades-ofertas/spec.md

## Schema Changes

### 1. Modificación de Tabla Existente: `agrupaciones`

```sql
-- Agregar campo tipo a tabla agrupaciones existente
ALTER TABLE agrupaciones 
ADD COLUMN tipo TINYINT NOT NULL DEFAULT 3 
COMMENT '1=Grupo1(Novedades/Ofertas), 2=Grupo2(Futuro), 3=Grupo3(Actual)';

-- Crear índice para optimizar consultas por tipo
CREATE INDEX idx_agrupaciones_tipo ON agrupaciones(tipo);
CREATE INDEX idx_agrupaciones_tipo_empresa ON agrupaciones(tipo, empresa_principal_id);
```

### 2. Nueva Tabla: `empresas_novedades`

```sql
CREATE TABLE empresas_novedades (
    id INT PRIMARY KEY AUTO_INCREMENT,
    empresa_id INT NOT NULL,
    agrupacion_id INT NOT NULL,
    visible TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Foreign Keys
    CONSTRAINT fk_empresas_novedades_empresa 
        FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE,
    CONSTRAINT fk_empresas_novedades_agrupacion 
        FOREIGN KEY (agrupacion_id) REFERENCES agrupaciones(id) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT chk_empresas_novedades_agrupacion_tipo1 
        CHECK ((SELECT tipo FROM agrupaciones WHERE id = agrupacion_id) = 1),
    
    -- Unique constraint
    UNIQUE KEY uk_empresas_novedades_empresa_agrupacion (empresa_id, agrupacion_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 3. Nueva Tabla: `empresas_ofertas`

```sql
CREATE TABLE empresas_ofertas (
    id INT PRIMARY KEY AUTO_INCREMENT,
    empresa_id INT NOT NULL,
    agrupacion_id INT NOT NULL,
    visible TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Foreign Keys
    CONSTRAINT fk_empresas_ofertas_empresa 
        FOREIGN KEY (empresa_id) REFERENCES empresas(id) ON DELETE CASCADE,
    CONSTRAINT fk_empresas_ofertas_agrupacion 
        FOREIGN KEY (agrupacion_id) REFERENCES agrupaciones(id) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT chk_empresas_ofertas_agrupacion_tipo1 
        CHECK ((SELECT tipo FROM agrupaciones WHERE id = agrupacion_id) = 1),
    
    -- Unique constraint
    UNIQUE KEY uk_empresas_ofertas_empresa_agrupacion (empresa_id, agrupacion_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 4. Índices para Performance

```sql
-- Índices para empresas_novedades
CREATE INDEX idx_empresas_novedades_empresa ON empresas_novedades(empresa_id);
CREATE INDEX idx_empresas_novedades_agrupacion ON empresas_novedades(agrupacion_id);
CREATE INDEX idx_empresas_novedades_visible ON empresas_novedades(visible);
CREATE INDEX idx_empresas_novedades_empresa_visible ON empresas_novedades(empresa_id, visible);

-- Índices para empresas_ofertas
CREATE INDEX idx_empresas_ofertas_empresa ON empresas_ofertas(empresa_id);
CREATE INDEX idx_empresas_ofertas_agrupacion ON empresas_ofertas(agrupacion_id);
CREATE INDEX idx_empresas_ofertas_visible ON empresas_ofertas(visible);
CREATE INDEX idx_empresas_ofertas_empresa_visible ON empresas_ofertas(empresa_id, visible);
```

## Migration Strategy

### Paso 1: Preparar Estructura
```sql
-- Ejecutar en orden:
-- 1. Agregar campo tipo a agrupaciones
-- 2. Crear tablas empresas_novedades y empresas_ofertas
-- 3. Crear índices
```

### Paso 2: Poblar Datos Iniciales (Opcional)
```sql
-- Actualizar tipos de agrupaciones existentes si se conoce la clasificación
-- Por defecto quedan como tipo 3 (Grupo 3 actual)

-- Ejemplo de actualización si hay agrupaciones conocidas de Grupo 1:
-- UPDATE agrupaciones SET tipo = 1 WHERE codigo IN (lista_de_codigos_grupo1);
```

### Paso 3: Validar Integridad
```sql
-- Verificar que no hay agrupaciones sin tipo válido
SELECT COUNT(*) FROM agrupaciones WHERE tipo NOT IN (1, 2, 3);

-- Verificar que las nuevas tablas están vacías inicialmente
SELECT COUNT(*) FROM empresas_novedades;
SELECT COUNT(*) FROM empresas_ofertas;
```

## Data Integrity Rules

### Constraints Implementados
1. **Tipo de Agrupación**: Solo agrupaciones tipo 1 pueden estar en novedades/ofertas
2. **Unicidad**: Una empresa no puede tener la misma agrupación duplicada en novedades u ofertas
3. **Referencias**: Cascada de eliminación para mantener integridad referencial
4. **Visibilidad**: Campo boolean para activar/desactivar sin eliminar registros

### Validaciones de Negocio
- Solo empresas principales pueden gestionar novedades/ofertas
- Una agrupación puede estar simultáneamente en novedades y ofertas
- El campo `visible` permite desactivar temporalmente sin perder configuración

## Performance Considerations

### Consultas Optimizadas
- Índices compuestos para consultas frecuentes (empresa_id, visible)
- Índices individuales para joins con otras tablas
- Constraint checks optimizados para validar tipo de agrupación

### Estimación de Volumen
- **agrupaciones**: ~100-500 registros (actual + nuevos tipo 1)
- **empresas_novedades**: ~50-200 registros (pocas agrupaciones por empresa)
- **empresas_ofertas**: ~50-200 registros (pocas agrupaciones por empresa)

Total impacto: Minimal en términos de storage y performance.