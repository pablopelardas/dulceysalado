# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-08-05-modulo-clientes/spec.md

## Endpoints de Sincronización

### POST /api/sync/customers/start-session

**Purpose:** Iniciar sesión de sincronización de clientes
**Auth:** Bearer token (admin)
**Body:**
```json
{
  "source": "GECOM",
  "timestamp": "2025-08-05T10:00:00Z"
}
```
**Response:**
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "active",
  "createdAt": "2025-08-05T10:00:00Z"
}
```

### POST /api/sync/customers/process-bulk

**Purpose:** Procesar lote de clientes en la sesión activa
**Auth:** Bearer token (admin)
**Body:**
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "customers": [
    {
      "codigo": "000183",
      "nombre": "PLUS SERVICE SRL",
      "direccion": "CHACABUCO 4301",
      "localidad": "Villa Ballester",
      "telefono": "",
      "cuit": "33-64448451-9",
      "altura": "4.301",
      "provincia": "BUENOS AIRES",
      "email": "",
      "tipoIva": "INSCRIPTO",
      "listaPrecio": "2"
    }
  ],
  "createCredentials": false
}
```
**Response:**
```json
{
  "processed": 100,
  "created": 10,
  "updated": 20,
  "unchanged": 70,
  "errors": []
}
```

### POST /api/sync/customers/end-session

**Purpose:** Finalizar sesión de sincronización
**Auth:** Bearer token (admin)
**Body:**
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000"
}
```
**Response:**
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "completed",
  "summary": {
    "totalProcessed": 1000,
    "totalCreated": 50,
    "totalUpdated": 150,
    "totalUnchanged": 800,
    "totalErrors": 0,
    "duration": "00:02:35"
  }
}
```

### GET /api/sync/customers/session/{sessionId}

**Purpose:** Obtener estado de sesión de sincronización
**Auth:** Bearer token (admin)
**Response:** Detalles de la sesión con estadísticas actuales

## Endpoints de Gestión

### GET /api/clientes

**Purpose:** Listar clientes con filtros y paginación
**Auth:** Bearer token (admin/user)
**Parameters:**
- codigo (query): Filtrar por código
- nombre (query): Búsqueda por nombre (LIKE)
- cuit (query): Filtrar por CUIT
- localidad (query): Filtrar por localidad
- tieneAcceso (query): Filtrar por clientes con/sin acceso
- page (query): Número de página (default: 1)
- pageSize (query): Items por página (default: 20)
**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "codigo": "000183",
      "nombre": "PLUS SERVICE SRL",
      "cuit": "33-64448451-9",
      "tieneAcceso": true,
      "username": "plus.service",
      "listaPrecio": {
        "id": 2,
        "nombre": "Mayorista"
      }
    }
  ],
  "totalCount": 1000,
  "page": 1,
  "pageSize": 20,
  "totalPages": 50
}
```

### GET /api/clientes/{id}

**Purpose:** Obtener detalle completo de un cliente
**Auth:** Bearer token (admin/user)
**Response:** Cliente completo con información de acceso

### POST /api/clientes

**Purpose:** Crear nuevo cliente
**Auth:** Bearer token (admin)
**Body:**
```json
{
  "codigo": "009999",
  "nombre": "NUEVO CLIENTE SA",
  "direccion": "Av. Ejemplo 123",
  "cuit": "30-12345678-9",
  "email": "contacto@nuevocliente.com",
  "listaPrecioId": 2
}
```

### PUT /api/clientes/{id}

**Purpose:** Actualizar cliente existente
**Auth:** Bearer token (admin)

### DELETE /api/clientes/{id}

**Purpose:** Eliminar cliente (soft delete)
**Auth:** Bearer token (admin)

## Endpoints de Credenciales

### POST /api/clientes/{id}/credenciales

**Purpose:** Crear o actualizar credenciales de acceso
**Auth:** Bearer token (admin)
**Body:**
```json
{
  "username": "plus.service",
  "password": "NuevaContraseña123",
  "isActive": true
}
```

### DELETE /api/clientes/{id}/credenciales

**Purpose:** Revocar acceso al cliente
**Auth:** Bearer token (admin)

### POST /api/clientes/{id}/reset-password

**Purpose:** Forzar cambio de contraseña
**Auth:** Bearer token (admin)
**Body:**
```json
{
  "newPassword": "TempPassword123"
}
```

## Endpoints de Autenticación (Frontend)

### POST /api/auth/cliente/login

**Purpose:** Autenticación de clientes para el catálogo
**Public:** No requiere auth
**Body:**
```json
{
  "username": "plus.service",
  "password": "MiContraseña123",
  "empresaId": 1  // Opcional si se resuelve por subdominio
}
```
**Response:**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "eyJ...",
  "cliente": {
    "id": 123,
    "nombre": "PLUS SERVICE SRL",
    "codigo": "000183",
    "listaPrecioId": 2
  },
  "expiresIn": 3600
}
```

### POST /api/auth/cliente/refresh

**Purpose:** Renovar token de acceso
**Body:**
```json
{
  "refreshToken": "eyJ..."
}
```

### POST /api/auth/cliente/logout

**Purpose:** Cerrar sesión y revocar tokens
**Auth:** Bearer token (cliente)

### GET /api/auth/cliente/me

**Purpose:** Obtener información del cliente autenticado
**Auth:** Bearer token (cliente)
**Response:**
```json
{
  "id": 123,
  "codigo": "000183",
  "nombre": "PLUS SERVICE SRL",
  "email": "contacto@plus.com",
  "listaPrecio": {
    "id": 2,
    "codigo": "2",
    "nombre": "Mayorista"
  }
}
```

### POST /api/auth/cliente/change-password

**Purpose:** Cambiar contraseña propia
**Auth:** Bearer token (cliente)
**Body:**
```json
{
  "currentPassword": "ContraseñaActual123",
  "newPassword": "NuevaContraseña456"
}
```

## Consideraciones de Seguridad

- Rate limiting: 5 intentos de login por minuto por IP
- Tokens JWT con expiración configurable
- Validación de empresa en todos los endpoints
- Auditoría de accesos opcionales
- Passwords con requisitos mínimos configurables
- Sesiones de sincronización con timeout de 30 minutos