# API Specification - Sistema Feature Flags

## Endpoints Overview

### Administrative Endpoints (Require Authentication)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/features/definitions` | Lista todas las feature definitions disponibles |
| GET | `/api/features/empresa/{empresaId}` | Obtiene configuración de features para una empresa |
| POST | `/api/features/empresa/{empresaId}` | Configura una feature para una empresa |
| PUT | `/api/features/empresa/{empresaId}/{featureCode}` | Actualiza valor de una feature |
| DELETE | `/api/features/empresa/{empresaId}/{featureCode}` | Deshabilita una feature |

### Public Endpoints (No Authentication)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/public/features` | Obtiene features de la empresa (por subdominio) |
| GET | `/api/public/features/{featureCode}` | Verifica una feature específica |

## Detailed Endpoint Specifications

### 1. GET /api/features/definitions

**Description**: Obtiene el catálogo completo de features disponibles en el sistema.

**Headers**:
```
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
[
  {
    "id": 1,
    "codigo": "pedido_whatsapp",
    "nombre": "Pedidos por WhatsApp",
    "descripcion": "Permite enviar pedidos directamente a WhatsApp en lugar de exportar lista",
    "tipo_valor": "string",
    "valor_defecto": null,
    "categoria": "pedidos",
    "activo": true
  },
  {
    "id": 2,
    "codigo": "pedido_campos_requeridos",
    "nombre": "Campos Requeridos en Pedido",
    "descripcion": "Define campos adicionales obligatorios al realizar pedidos",
    "tipo_valor": "json",
    "valor_defecto": "[]",
    "categoria": "pedidos",
    "activo": true
  }
]
```

### 2. GET /api/features/empresa/{empresaId}

**Description**: Obtiene todas las features configuradas para una empresa específica, combinando valores configurados con valores por defecto.

**Headers**:
```
Authorization: Bearer {token}
```

**Response** (200 OK):
```json
[
  {
    "codigo": "pedido_whatsapp",
    "nombre": "Pedidos por WhatsApp",
    "descripcion": "Permite enviar pedidos directamente a WhatsApp",
    "tipo_valor": "string",
    "categoria": "pedidos",
    "habilitado": true,
    "valor": "+5491123456789",
    "metadata": {
      "mensaje_template": "Hola, quiero hacer el siguiente pedido:",
      "incluir_total": true
    },
    "updated_at": "2025-08-05T10:30:00Z",
    "updated_by": "admin@empresa.com"
  },
  {
    "codigo": "cliente_autenticacion",
    "nombre": "Autenticación de Clientes",
    "descripcion": "Requiere que clientes se autentiquen para navegar",
    "tipo_valor": "boolean",
    "categoria": "seguridad",
    "habilitado": false,
    "valor": null,
    "metadata": null,
    "updated_at": null,
    "updated_by": null
  }
]
```

### 3. POST /api/features/empresa/{empresaId}

**Description**: Configura o actualiza una feature para una empresa.

**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body**:
```json
{
  "feature_code": "pedido_campos_requeridos",
  "habilitado": true,
  "valor": "[\"nombre\", \"telefono\", \"numero_cliente\", \"direccion_entrega\"]",
  "metadata": {
    "validaciones": {
      "numero_cliente": {
        "regex": "^[0-9]{6}$",
        "mensaje": "El número de cliente debe tener 6 dígitos"
      }
    },
    "orden_campos": ["nombre", "numero_cliente", "telefono", "direccion_entrega"]
  }
}
```

**Response** (200 OK):
```json
{
  "id": 123,
  "empresa_id": 1,
  "feature_id": 2,
  "codigo": "pedido_campos_requeridos",
  "habilitado": true,
  "valor": "[\"nombre\", \"telefono\", \"numero_cliente\", \"direccion_entrega\"]",
  "metadata": {
    "validaciones": {
      "numero_cliente": {
        "regex": "^[0-9]{6}$",
        "mensaje": "El número de cliente debe tener 6 dígitos"
      }
    }
  },
  "created_at": "2025-08-05T10:30:00Z",
  "updated_at": "2025-08-05T10:30:00Z",
  "created_by": "admin@empresa.com"
}
```

**Error Responses**:
- 400 Bad Request: Datos inválidos
- 404 Not Found: Empresa o feature no existe
- 422 Unprocessable Entity: Valor no compatible con tipo de feature

### 4. PUT /api/features/empresa/{empresaId}/{featureCode}

**Description**: Actualiza únicamente el valor y metadata de una feature existente.

**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body**:
```json
{
  "valor": "+5491199999999",
  "metadata": {
    "mensaje_template": "Nuevo pedido desde catálogo:",
    "incluir_total": true,
    "incluir_direccion": false
  }
}
```

**Response** (200 OK): Similar a POST response

### 5. DELETE /api/features/empresa/{empresaId}/{featureCode}

**Description**: Deshabilita una feature para una empresa (soft delete).

**Headers**:
```
Authorization: Bearer {token}
```

**Response** (204 No Content)

**Error Responses**:
- 404 Not Found: Feature no configurada para esta empresa

### 6. GET /api/public/features

**Description**: Endpoint público que retorna las features habilitadas para la empresa identificada por subdominio o header.

**Headers** (Optional):
```
X-Company-Id: 123
```

**Response** (200 OK):
```json
[
  {
    "codigo": "pedido_whatsapp",
    "habilitado": true,
    "valor": "+5491123456789",
    "metadata": {
      "mensaje_template": "Hola, quiero hacer el siguiente pedido:"
    }
  },
  {
    "codigo": "cliente_autenticacion",
    "habilitado": false
  },
  {
    "codigo": "catalogo_precios_ocultos",
    "habilitado": true,
    "valor": "true",
    "metadata": {
      "mensaje": "Contacte para consultar precios"
    }
  }
]
```

**Note**: Este endpoint no incluye información sensible como descripción, categoría o auditoría.

### 7. GET /api/public/features/{featureCode}

**Description**: Verifica si una feature específica está habilitada para la empresa.

**Response** (200 OK):
```json
{
  "codigo": "pedido_whatsapp",
  "habilitado": true,
  "valor": "+5491123456789",
  "metadata": {
    "mensaje_template": "Hola, quiero hacer el siguiente pedido:"
  }
}
```

**Response** (404 Not Found): Feature no existe o no está configurada

## Error Response Format

Todos los endpoints siguen el formato estándar de error:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "El valor proporcionado no es compatible con el tipo 'number'",
  "instance": "/api/features/empresa/123",
  "errors": {
    "valor": ["Debe ser un número válido"]
  }
}
```

## Validation Rules

### Feature Code
- Formato: `^[a-z][a-z0-9_]*$`
- Longitud: 3-100 caracteres
- Ejemplos válidos: `pedido_whatsapp`, `cliente_autenticacion`

### Feature Values by Type
- **boolean**: "true" o "false"
- **string**: Cualquier texto hasta 5000 caracteres
- **number**: Número válido (entero o decimal)
- **json**: JSON válido como string

### Metadata
- Debe ser un objeto JSON válido
- Tamaño máximo: 64KB
- No puede contener funciones o referencias circulares

## Rate Limiting

- Administrative endpoints: 100 requests per minute
- Public endpoints: 1000 requests per minute per IP
- Cached responses para public endpoints (TTL: 5 minutos)

## Examples of Use Cases

### 1. Configurar WhatsApp para Pedidos
```bash
POST /api/features/empresa/123
{
  "feature_code": "pedido_whatsapp",
  "habilitado": true,
  "valor": "+5491123456789",
  "metadata": {
    "mensaje_template": "Hola, quiero hacer el siguiente pedido:\n{{items}}\nTotal: {{total}}",
    "incluir_nombre_empresa": true
  }
}
```

### 2. Definir Campos Requeridos para Pedidos
```bash
POST /api/features/empresa/123
{
  "feature_code": "pedido_campos_requeridos",
  "habilitado": true,
  "valor": "[\"nombre\", \"numero_cliente\", \"telefono\", \"email\", \"direccion_entrega\", \"horario_entrega\"]",
  "metadata": {
    "validaciones": {
      "numero_cliente": {
        "regex": "^[0-9]{6}$",
        "requerido": true,
        "mensaje_error": "Número de cliente inválido"
      },
      "telefono": {
        "regex": "^\\+?[0-9]{10,}$",
        "requerido": true
      },
      "email": {
        "tipo": "email",
        "requerido": false
      }
    }
  }
}
```

### 3. Frontend Verifica Autenticación Requerida
```javascript
// Frontend code
const response = await fetch('/api/public/features/cliente_autenticacion');
const feature = await response.json();

if (feature.habilitado) {
    // Redirigir a login
    window.location.href = '/login';
} else {
    // Permitir navegación anónima
    showCatalog();
}
```

### 4. Obtener Configuración Completa para UI
```javascript
// Frontend obtiene todas las features
const response = await fetch('/api/public/features');
const features = await response.json();

// Crear mapa de features
const featureMap = features.reduce((acc, feature) => {
    acc[feature.codigo] = feature;
    return acc;
}, {});

// Usar en la aplicación
if (featureMap.pedido_whatsapp?.habilitado) {
    showWhatsAppButton(featureMap.pedido_whatsapp.valor);
}
```