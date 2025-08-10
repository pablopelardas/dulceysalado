# Sistema de Feature Flags por Empresa

## Resumen

Sistema de configuración de funcionalidades (feature flags) que permite activar o desactivar características específicas por empresa, facilitando la personalización del comportamiento del sistema según las necesidades de cada cliente.

## Contexto

### Problema
- Diferentes empresas requieren funcionalidades personalizadas
- Ejemplos actuales:
  - Empresa A: Envío de pedidos por WhatsApp en lugar de exportar lista
  - Empresa B: Captura de datos adicionales del cliente (nombre, número, contacto)
  - Empresa C: Sistema de autenticación para clientes en frontend
- Sin un sistema centralizado, se vuelve complejo mantener estas variaciones

### Solución
Implementar un sistema de Feature Flags que:
- Permita configurar funcionalidades por empresa
- Sea extensible para nuevas features
- Centralice la gestión de configuraciones
- Facilite el mantenimiento y despliegue

## Alcance

### Incluido
- Modelo de dominio para Feature Flags
- CRUD completo con patrón CQRS
- API endpoints para gestión y consulta
- Sistema de caché para optimizar consultas
- Endpoint público para frontend
- Valores por defecto configurables
- Auditoría de cambios

### Excluido
- UI de administración (futuro)
- Feature flags a nivel usuario
- Integración con servicios externos de feature flags
- Versionado de configuraciones

## Especificaciones Técnicas

### Modelo de Datos

```sql
-- Tabla de definición de features disponibles
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
    INDEX idx_categoria (categoria)
);

-- Tabla de configuración por empresa
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
    FOREIGN KEY (empresa_id) REFERENCES empresas(id),
    FOREIGN KEY (feature_id) REFERENCES feature_definitions(id),
    INDEX idx_empresa (empresa_id)
);
```

### Features Iniciales

```csharp
public static class FeatureCodes
{
    // Pedidos
    public const string PEDIDO_WHATSAPP = "pedido_whatsapp";
    public const string PEDIDO_DATOS_ADICIONALES = "pedido_datos_adicionales";
    public const string PEDIDO_CAMPOS_REQUERIDOS = "pedido_campos_requeridos"; // JSON con campos
    
    // Autenticación
    public const string CLIENTE_AUTENTICACION = "cliente_autenticacion";
    public const string CLIENTE_REGISTRO_PUBLICO = "cliente_registro_publico";
    
    // Catálogo
    public const string CATALOGO_PRECIOS_OCULTOS = "catalogo_precios_ocultos";
    public const string CATALOGO_STOCK_VISIBLE = "catalogo_stock_visible";
    
    // Notificaciones
    public const string NOTIFICACION_EMAIL = "notificacion_email";
    public const string NOTIFICACION_SMS = "notificacion_sms";
}
```

### API Endpoints

#### Administración (Requiere autenticación)
```
GET    /api/features/definitions          # Lista todas las features disponibles
GET    /api/features/empresa/{empresaId}  # Features de una empresa
POST   /api/features/empresa/{empresaId}  # Configurar feature para empresa
PUT    /api/features/empresa/{empresaId}/{featureCode} # Actualizar feature
DELETE /api/features/empresa/{empresaId}/{featureCode} # Eliminar configuración
```

#### Público (Para frontend)
```
GET    /api/public/features               # Features de la empresa (por subdominio)
GET    /api/public/features/{featureCode} # Verificar feature específica
```

### Arquitectura

```
DistriCatalogoAPI.Domain/
├── Entities/
│   ├── FeatureDefinition.cs
│   └── EmpresaFeature.cs
├── Interfaces/
│   └── IFeatureRepository.cs
└── ValueObjects/
    └── FeatureValue.cs

DistriCatalogoAPI.Application/
├── Commands/Features/
│   ├── ConfigureFeatureCommand.cs
│   ├── DisableFeatureCommand.cs
│   └── UpdateFeatureValueCommand.cs
├── Queries/Features/
│   ├── GetFeaturesByEmpresaQuery.cs
│   ├── GetFeatureDefinitionsQuery.cs
│   └── CheckFeatureQuery.cs
├── DTOs/Features/
│   ├── FeatureDefinitionDto.cs
│   ├── EmpresaFeatureDto.cs
│   └── FeatureConfigurationDto.cs
└── Services/
    └── IFeatureFlagService.cs

DistriCatalogoAPI.Infrastructure/
├── Repositories/
│   └── FeatureRepository.cs
├── Services/
│   └── FeatureFlagService.cs
└── Persistence/Configurations/
    ├── FeatureDefinitionConfiguration.cs
    └── EmpresaFeatureConfiguration.cs

DistriCatalogoAPI.Api/
└── Controllers/
    ├── FeaturesController.cs
    └── PublicFeaturesController.cs
```

## Casos de Uso

### 1. Configurar WhatsApp para Pedidos
```csharp
// Admin configura feature para empresa
POST /api/features/empresa/123
{
    "featureCode": "pedido_whatsapp",
    "habilitado": true,
    "valor": "+5491123456789", // Número WhatsApp
    "metadata": {
        "mensaje_template": "Hola, quiero hacer el siguiente pedido:"
    }
}
```

### 2. Configurar Campos Adicionales
```csharp
POST /api/features/empresa/123
{
    "featureCode": "pedido_campos_requeridos",
    "habilitado": true,
    "valor": "[\"nombre\", \"telefono\", \"numero_cliente\", \"direccion_entrega\"]",
    "metadata": {
        "validaciones": {
            "numero_cliente": "^[0-9]{6}$"
        }
    }
}
```

### 3. Frontend Consulta Features
```javascript
// Frontend verifica si debe mostrar login
const response = await fetch('/api/public/features/cliente_autenticacion');
const feature = await response.json();
if (feature.habilitado) {
    mostrarLogin();
}
```

## Implementación por Fases

### Fase 1: Infraestructura Base (Esta especificación)
- Modelo de dominio
- Tablas y migrations
- CRUD básico
- Endpoints de consulta

### Fase 2: Optimización
- Sistema de caché Redis
- Bulk operations
- Webhook de cambios

### Fase 3: Administración
- UI de gestión
- Reportes de uso
- A/B testing

## Consideraciones

### Performance
- Caché en memoria para features consultadas frecuentemente
- Índices optimizados en base de datos
- Lazy loading de metadata

### Seguridad
- Solo admins pueden modificar features
- Auditoría completa de cambios
- Validación de valores según tipo

### Mantenibilidad
- Features documentadas con descripción
- Categorización para organización
- Valores por defecto sensatos