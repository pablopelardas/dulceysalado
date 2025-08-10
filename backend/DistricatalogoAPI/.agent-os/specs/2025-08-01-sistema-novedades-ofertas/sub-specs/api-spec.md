# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-08-01-sistema-novedades-ofertas/spec.md

## Endpoints Overview

### Administrativos (Requieren Autenticación JWT)
- **Gestión de Novedades**: `/api/empresas-novedades/*`
- **Gestión de Ofertas**: `/api/empresas-ofertas/*`

### Públicos (Sin Autenticación)
- **Catálogo de Novedades**: `/api/catalog/novedades`
- **Catálogo de Ofertas**: `/api/catalog/ofertas`

## Administrative Endpoints

### Gestión de Novedades

#### GET /api/empresas/{empresaId}/novedades
**Purpose:** Obtener todas las agrupaciones con indicador de cuáles están marcadas como novedades para la empresa específica
**Authorization:** JWT Bearer (solo empresa principal)
**Parameters:** 
- `empresaId` (path): ID de la empresa a consultar

**Response:**
```json
{
  "success": true,
  "empresa_id": 2,
  "agrupaciones": [
    {
      "id": 5,
      "codigo": 15,
      "nombre": "Productos Nuevos Enero",
      "descripcion": "Descripción de la agrupación",
      "activa": true,
      "visible": true
    },
    {
      "id": 6,
      "codigo": 16,
      "nombre": "Otra Agrupación",
      "descripcion": "Otra descripción",
      "activa": true,
      "visible": false
    }
  ]
}
```

#### PUT /api/empresas/{empresaId}/novedades
**Purpose:** Configurar qué agrupaciones de Grupo 1 son visibles como novedades para una empresa específica
**Authorization:** JWT Bearer (solo empresa principal)
**Body:**
```json
{
  "agrupaciones_ids": [5, 8, 12]
}
```

**Response:**
```json
{
  "success": true,
  "message": "Configuración de novedades actualizada correctamente",
  "empresa_id": 2,
  "agrupaciones_visibles": 3
}
```

#### GET /api/empresas-novedades
**Purpose:** Obtener todas las configuraciones de novedades de la empresa principal
**Authorization:** JWT Bearer (solo empresa principal)
**Parameters:** 
- `page` (query, optional, default=1)
- `pageSize` (query, optional, default=100)

**Response:**
```json
{
  "success": true,
  "novedades": [
    {
      "id": 1,
      "empresa_id": 2,
      "agrupacion_id": 5,
      "visible": true,
      "empresa": {
        "id": 2,
        "nombre": "Sucursal Centro",
        "codigo": "SUC001"
      },
      "agrupacion": {
        "id": 5,
        "codigo": 15,
        "nombre": "Productos Nuevos Enero",
        "tipo": 1
      },
      "created_at": "2025-08-01T12:00:00Z",
      "updated_at": "2025-08-01T12:00:00Z"
    }
  ],
  "pagination": {
    "total": 15,
    "page": 1,
    "page_size": 100,
    "total_pages": 1
  }
}
```

#### POST /api/empresas-novedades
**Purpose:** Crear nueva configuración de novedad para una empresa
**Authorization:** JWT Bearer (solo empresa principal)
**Body:**
```json
{
  "empresa_id": 2,
  "agrupacion_id": 5,
  "visible": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "Configuración de novedad creada exitosamente",
  "novedad": {
    "id": 1,
    "empresa_id": 2,
    "agrupacion_id": 5,
    "visible": true
  }
}
```

#### PUT /api/empresas-novedades/{id}
**Purpose:** Actualizar configuración de novedad existente
**Authorization:** JWT Bearer (solo empresa principal)
**Body:**
```json
{
  "visible": false
}
```

#### DELETE /api/empresas-novedades/{id}
**Purpose:** Eliminar configuración de novedad
**Authorization:** JWT Bearer (solo empresa principal)

### Gestión de Ofertas

#### GET /api/empresas/{empresaId}/ofertas
**Purpose:** Obtener todas las agrupaciones con indicador de cuáles están marcadas como ofertas para la empresa específica
**Authorization:** JWT Bearer (solo empresa principal)
**Parameters:** 
- `empresaId` (path): ID de la empresa a consultar
**Response:** Estructura similar a novedades

#### PUT /api/empresas/{empresaId}/ofertas
**Purpose:** Configurar qué agrupaciones de Grupo 1 son visibles como ofertas para una empresa específica
**Authorization:** JWT Bearer (solo empresa principal)
**Body:**
```json
{
  "agrupaciones_ids": [3, 7, 9, 15]
}
```

#### GET /api/empresas-ofertas
**Purpose:** Obtener todas las configuraciones de ofertas de la empresa principal (vista global)
**Authorization:** JWT Bearer (solo empresa principal)
**Parameters:** Similares a novedades
**Response:** Estructura similar a novedades

#### POST /api/empresas-ofertas
**Purpose:** Crear nueva configuración de oferta individual
**Authorization:** JWT Bearer (solo empresa principal)
**Body:** Similar a novedades

#### PUT /api/empresas-ofertas/{id}
**Purpose:** Actualizar configuración de oferta individual
**Authorization:** JWT Bearer (solo empresa principal)

#### DELETE /api/empresas-ofertas/{id}
**Purpose:** Eliminar configuración de oferta individual
**Authorization:** JWT Bearer (solo empresa principal)

## Public Catalog Endpoints

### GET /api/catalog/novedades
**Purpose:** Obtener productos marcados como novedades para la empresa (sin paginación)
**Authorization:** Ninguna (endpoint público)
**Company Resolution:** Automática via subdomain o parámetro `empresaId`
**Parameters:**
- `empresaId` (query, optional): Override manual para testing

**Response:**
```json
{
  "success": true,
  "novedades": [
    {
      "id": 123,
      "codigo": "PROD001",
      "descripcion": "Producto Novedad Enero",
      "precio": 1500.00,
      "imagen_url": "https://example.com/imagen.jpg",
      "disponible": true,
      "categoria": {
        "id": 5,
        "nombre": "Electrónicos"
      },
      "marca": "Samsung",
      "agrupacion": {
        "id": 15,
        "nombre": "Productos Nuevos Enero",
        "codigo": 15
      }
    }
  ],
  "total_novedades": 12,
  "empresa": {
    "id": 2,
    "nombre": "Sucursal Centro"
  }
}
```

### GET /api/catalog/ofertas  
**Purpose:** Obtener productos marcados como ofertas para la empresa (sin paginación)
**Authorization:** Ninguna (endpoint público)
**Company Resolution:** Automática via subdomain o parámetro `empresaId`
**Parameters:** Similares a novedades
**Response:** Estructura similar a novedades

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "La agrupación especificada no es de tipo Grupo 1"
}
```

### 401 Unauthorized
```json
{
  "success": false,
  "message": "Token JWT requerido"
}
```

### 403 Forbidden
```json
{
  "success": false,
  "message": "Solo las empresas principales pueden gestionar novedades/ofertas"
}
```

### 404 Not Found
```json
{
  "success": false,
  "message": "Configuración de novedad no encontrada"
}
```

### 409 Conflict
```json
{
  "success": false,
  "message": "La empresa ya tiene configurada esta agrupación como novedad"
}
```

## Controllers Structure

### EmpresasNovedadesController
```csharp
[ApiController]
[Route("api/empresas")]
[Authorize]
public class EmpresasNovedadesController : ControllerBase
{
    // Endpoints drag-and-drop (similares a EmpresasAgrupacionesController)
    [HttpGet("{empresaId}/novedades")]
    public async Task<IActionResult> GetNovedadesForEmpresa(int empresaId)
    
    [HttpPut("{empresaId}/novedades")]
    public async Task<IActionResult> SetNovedadesForEmpresa(int empresaId, SetNovedadesRequest request)
}

// Controlador adicional para vista global
[ApiController]
[Route("api/empresas-novedades")]
[Authorize]
public class GlobalNovedadesController : ControllerBase
{
    // CRUD operations tradicionales para vista global
    [HttpGet]
    public async Task<IActionResult> GetAllNovedades()
    
    [HttpPost]
    public async Task<IActionResult> CreateNovedad(CreateNovedadRequest request)
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNovedad(int id, UpdateNovedadRequest request)
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNovedad(int id)
}
```

### EmpresasOfertasController  
```csharp
[ApiController]
[Route("api/empresas")]
[Authorize]
public class EmpresasOfertasController : ControllerBase
{
    // Endpoints drag-and-drop (similares a EmpresasAgrupacionesController)
    [HttpGet("{empresaId}/ofertas")]
    public async Task<IActionResult> GetOfertasForEmpresa(int empresaId)
    
    [HttpPut("{empresaId}/ofertas")]
    public async Task<IActionResult> SetOfertasForEmpresa(int empresaId, SetOfertasRequest request)
}

// Controlador adicional para vista global
[ApiController]
[Route("api/empresas-ofertas")]
[Authorize]
public class GlobalOfertasController : ControllerBase
{
    // CRUD operations tradicionales para vista global
}
```

### Catalog Controller Extensions
```csharp
[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    // Métodos existentes...
    
    [HttpGet("novedades")]
    public async Task<IActionResult> GetNovedades([FromQuery] int? empresaId = null)
    
    [HttpGet("ofertas")] 
    public async Task<IActionResult> GetOfertas([FromQuery] int? empresaId = null)
}
```

## Integration Points

### Company Resolution Middleware
- Reutilizar `CompanyResolutionMiddleware` existente
- Endpoints públicos resuelven empresa automáticamente por subdomain
- Fallback a parámetro `empresaId` para testing

### Authentication & Authorization
- Endpoints administrativos requieren JWT Bearer
- Validación de tipo de empresa (solo principal puede gestionar)
- Endpoints públicos sin autenticación

### Data Access Pattern
- Siguir patrón CQRS existente con MediatR
- Commands para operaciones de escritura (Create/Update/Delete)
- Queries para operaciones de lectura con DTOs específicos