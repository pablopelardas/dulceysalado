# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-08-01-novedades-ofertas-modules/spec.md

> Created: 2025-08-01
> Version: 1.0.0

## Endpoints

### GET /api/empresas/{empresaId}/novedades

**Purpose:** Obtiene todas las agrupaciones con información sobre cuáles están marcadas como novedades para la empresa específica

**Parameters:** 
- empresaId (path): ID de la empresa cliente
- Authorization header con JWT token

**Response:** 
```json
{
  "agrupaciones": [
    {
      "id": 1,
      "nombre": "Electrodomésticos",
      "nivel": 1,
      "es_novedad": true
    },
    {
      "id": 2,
      "nombre": "Herramientas",
      "nivel": 1,
      "es_novedad": false
    }
  ]
}
```

**Errors:** 401 (Unauthorized), 403 (Forbidden), 404 (Empresa not found)

### PUT /api/empresas/{empresaId}/novedades

**Purpose:** Actualiza masivamente qué agrupaciones son novedades para la empresa

**Parameters:**
- empresaId (path): ID de la empresa
- Body: array de IDs de agrupaciones que serán novedades

**Request Body:**
```json
{
  "agrupacion_ids": [1, 5, 8]
}
```

**Response:** 
```json
{
  "success": true,
  "message": "Novedades actualizadas correctamente"
}
```

**Errors:** 400 (Invalid data), 401 (Unauthorized), 403 (Forbidden)

### GET /api/empresas/{empresaId}/ofertas

**Purpose:** Obtiene todas las agrupaciones con información sobre cuáles están marcadas como ofertas para la empresa específica

**Parameters:** 
- empresaId (path): ID de la empresa cliente
- Authorization header con JWT token

**Response:** 
```json
{
  "agrupaciones": [
    {
      "id": 1,
      "nombre": "Electrodomésticos",
      "nivel": 1,
      "es_oferta": true
    },
    {
      "id": 3,
      "nombre": "Jardín",
      "nivel": 1,
      "es_oferta": false
    }
  ]
}
```

**Errors:** 401 (Unauthorized), 403 (Forbidden), 404 (Empresa not found)

### PUT /api/empresas/{empresaId}/ofertas

**Purpose:** Actualiza masivamente qué agrupaciones son ofertas para la empresa

**Parameters:**
- empresaId (path): ID de la empresa
- Body: array de IDs de agrupaciones que serán ofertas

**Request Body:**
```json
{
  "agrupacion_ids": [1, 3, 7]
}
```

**Response:** 
```json
{
  "success": true,
  "message": "Ofertas actualizadas correctamente"
}
```

**Errors:** 400 (Invalid data), 401 (Unauthorized), 403 (Forbidden)

## Controllers

Los endpoints reutilizarán la lógica existente del sistema de agrupaciones, siguiendo el mismo patrón de autenticación y autorización implementado en el backend actual.