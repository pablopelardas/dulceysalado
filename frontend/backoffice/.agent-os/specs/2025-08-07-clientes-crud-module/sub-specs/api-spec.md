# Especificación de API - Módulo Clientes CRUD

Esta es la especificación de API para el spec detallado en @.agent-os/specs/2025-08-07-clientes-crud-module/spec.md

> Creado: 2025-08-07
> Versión: 1.0.0

## Endpoints de API

### 1. Listar Clientes

**Endpoint:** `GET /api/clientes`

**Descripción:** Obtiene una lista paginada de clientes con capacidades de filtrado

**Autenticación:** JWT Bearer Token requerido

**Permisos:** Solo usuarios de empresa principal

**Parámetros de Query:**
```
page: number (opcional, default: 1)
limit: number (opcional, default: 20, max: 100)
search: string (opcional, búsqueda por nombre o email)
estado: string (opcional, "activo" | "inactivo")
fecha_desde: string (opcional, formato: YYYY-MM-DD)
fecha_hasta: string (opcional, formato: YYYY-MM-DD)
lista_precio_id: number (opcional, filtrar por lista de precios)
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "clientes": [
      {
        "id": 1,
        "nombre": "Juan Pérez",
        "email": "juan@empresa.com",
        "telefono": "+5491123456789",
        "direccion": "Av. Corrientes 1234",
        "ciudad": "Buenos Aires",
        "codigo_postal": "C1043AAZ",
        "lista_precio_id": 2,
        "lista_precio_nombre": "Mayorista",
        "estado": "activo",
        "fecha_registro": "2025-01-15T10:30:00Z",
        "ultimo_login": "2025-08-06T14:22:00Z",
        "empresa_id": 123
      }
    ],
    "pagination": {
      "current_page": 1,
      "total_pages": 5,
      "total_items": 87,
      "items_per_page": 20
    }
  }
}
```

**Errores:**
- `401`: Token no válido o expirado
- `403`: Usuario no tiene permisos (no es empresa principal)
- `400`: Parámetros de query inválidos

### 2. Obtener Cliente Individual

**Endpoint:** `GET /api/clientes/{id}`

**Descripción:** Obtiene los detalles completos de un cliente específico

**Autenticación:** JWT Bearer Token requerido

**Permisos:** Solo usuarios de empresa principal

**Parámetros de Path:**
- `id`: number (ID del cliente)

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "nombre": "Juan Pérez",
    "email": "juan@empresa.com",
    "telefono": "+5491123456789",
    "direccion": "Av. Corrientes 1234",
    "ciudad": "Buenos Aires",
    "provincia": "Buenos Aires",
    "codigo_postal": "C1043AAZ",
    "cuit_cuil": "20-12345678-9",
    "lista_precio_id": 2,
    "lista_precio_nombre": "Mayorista",
    "estado": "activo",
    "notas": "Cliente preferencial",
    "fecha_registro": "2025-01-15T10:30:00Z",
    "ultimo_login": "2025-08-06T14:22:00Z",
    "empresa_id": 123,
    "configuracion": {
      "recibir_notificaciones": true,
      "mostrar_precios": true
    }
  }
}
```

**Errores:**
- `401`: Token no válido o expirado
- `403`: Usuario no tiene permisos
- `404`: Cliente no encontrado

### 3. Crear Nuevo Cliente

**Endpoint:** `POST /api/clientes`

**Descripción:** Crea un nuevo cliente en el sistema

**Autenticación:** JWT Bearer Token requerido

**Permisos:** Solo usuarios de empresa principal

**Request Body:**
```json
{
  "nombre": "María González",
  "email": "maria@empresa.com",
  "password": "TempPassword123!",
  "telefono": "+5491198765432",
  "direccion": "Av. Santa Fe 5678",
  "ciudad": "Rosario",
  "provincia": "Santa Fe",
  "codigo_postal": "S2000ABC",
  "cuit_cuil": "27-98765432-1",
  "lista_precio_id": 1,
  "notas": "Cliente nuevo con descuentos especiales"
}
```

**Validaciones:**
- `nombre`: requerido, min 2 caracteres, max 100
- `email`: requerido, formato email válido, único en el sistema
- `password`: requerido, min 8 caracteres, debe contener mayúscula, minúscula y número
- `telefono`: opcional, formato válido si se proporciona
- `lista_precio_id`: requerido, debe existir y estar activa
- `cuit_cuil`: opcional, formato CUIT/CUIL válido si se proporciona

**Response 201:**
```json
{
  "success": true,
  "message": "Cliente creado exitosamente",
  "data": {
    "id": 25,
    "nombre": "María González",
    "email": "maria@empresa.com",
    "estado": "activo",
    "fecha_registro": "2025-08-07T15:30:00Z",
    "empresa_id": 123
  }
}
```

**Errores:**
- `400`: Datos de entrada inválidos, email ya existe
- `401`: Token no válido
- `403`: Sin permisos
- `422`: Feature flag "cliente_autenticacion" no está habilitado

### 4. Actualizar Cliente

**Endpoint:** `PUT /api/clientes/{id}`

**Descripción:** Actualiza los datos de un cliente existente

**Autenticación:** JWT Bearer Token requerido

**Permisos:** Solo usuarios de empresa principal

**Parámetros de Path:**
- `id`: number (ID del cliente)

**Request Body:**
```json
{
  "nombre": "María González Actualizada",
  "telefono": "+5491198765433",
  "direccion": "Av. Santa Fe 9999",
  "ciudad": "Rosario",
  "provincia": "Santa Fe",
  "codigo_postal": "S2000XYZ",
  "lista_precio_id": 2,
  "notas": "Cliente con descuentos actualizados",
  "estado": "activo"
}
```

**Nota:** El email y password no se pueden actualizar a través de este endpoint

**Response 200:**
```json
{
  "success": true,
  "message": "Cliente actualizado exitosamente",
  "data": {
    "id": 25,
    "nombre": "María González Actualizada",
    "email": "maria@empresa.com",
    "telefono": "+5491198765433",
    "estado": "activo",
    "fecha_actualizacion": "2025-08-07T16:45:00Z"
  }
}
```

**Errores:**
- `400`: Datos inválidos
- `401`: Token no válido
- `403`: Sin permisos
- `404`: Cliente no encontrado

### 5. Desactivar Cliente

**Endpoint:** `DELETE /api/clientes/{id}`

**Descripción:** Desactiva un cliente (soft delete)

**Autenticación:** JWT Bearer Token requerido

**Permisos:** Solo usuarios de empresa principal

**Parámetros de Path:**
- `id`: number (ID del cliente)

**Response 200:**
```json
{
  "success": true,
  "message": "Cliente desactivado exitosamente",
  "data": {
    "id": 25,
    "estado": "inactivo",
    "fecha_desactivacion": "2025-08-07T17:00:00Z"
  }
}
```

**Errores:**
- `401`: Token no válido
- `403`: Sin permisos
- `404`: Cliente no encontrado
- `409`: Cliente ya está inactivo

### 6. Resetear Contraseña de Cliente

**Endpoint:** `POST /api/clientes/{id}/reset-password`

**Descripción:** Permite al administrador resetear la contraseña de un cliente

**Autenticación:** JWT Bearer Token requerido

**Permisos:** Solo usuarios de empresa principal con rol admin

**Parámetros de Path:**
- `id`: number (ID del cliente)

**Request Body:**
```json
{
  "nueva_password": "NuevaPassword123!",
  "enviar_email": true
}
```

**Validaciones:**
- `nueva_password`: requerido, min 8 caracteres, debe contener mayúscula, minúscula y número
- `enviar_email`: opcional, default false

**Response 200:**
```json
{
  "success": true,
  "message": "Contraseña reseteada exitosamente",
  "data": {
    "password_actualizada": true,
    "email_enviado": true,
    "fecha_reset": "2025-08-07T17:30:00Z"
  }
}
```

**Errores:**
- `400`: Nueva contraseña no cumple criterios
- `401`: Token no válido
- `403`: Sin permisos de admin
- `404`: Cliente no encontrado

## Controladores

### ClientesController

**Ubicación:** `src/controllers/ClientesController.ts`

**Métodos principales:**
- `index()`: Maneja GET /api/clientes
- `show()`: Maneja GET /api/clientes/{id}  
- `store()`: Maneja POST /api/clientes
- `update()`: Maneja PUT /api/clientes/{id}
- `destroy()`: Maneja DELETE /api/clientes/{id}
- `resetPassword()`: Maneja POST /api/clientes/{id}/reset-password

**Middleware aplicados:**
- `AuthMiddleware`: Validación de JWT
- `EmpresaPrincipalMiddleware`: Verificación de permisos de empresa principal
- `FeatureFlagMiddleware`: Validación de feature flag "cliente_autenticacion"
- `ValidationMiddleware`: Validación de datos de entrada

### Servicios auxiliares

**ClienteService:**
- Lógica de negocio para CRUD de clientes
- Validaciones adicionales
- Integración con sistema de notificaciones

**PasswordService:**
- Hashing de contraseñas con bcrypt
- Generación de contraseñas temporales
- Validación de criterios de seguridad

## Autenticación y Autorización

### Validación de JWT Token
```typescript
// Extracto del middleware de autenticación
const token = req.headers.authorization?.replace('Bearer ', '');
const decoded = jwt.verify(token, process.env.JWT_SECRET);
req.user = decoded;
```

### Validación de Empresa Principal
```typescript
// Middleware para verificar permisos
if (req.user.empresa_tipo !== 'principal') {
  return res.status(403).json({
    success: false,
    message: 'Solo empresas principales pueden gestionar clientes'
  });
}
```

### Validación de Feature Flag
```typescript
// Verificación del feature flag
const isClienteAuthEnabled = await FeatureFlagService.isEnabled(
  'cliente_autenticacion', 
  req.user.empresa_id
);

if (!isClienteAuthEnabled) {
  return res.status(422).json({
    success: false,
    message: 'Funcionalidad de clientes no está habilitada'
  });
}
```

## Códigos de Estado HTTP

- `200`: Operación exitosa
- `201`: Recurso creado exitosamente
- `400`: Datos de entrada inválidos
- `401`: Token no válido o expirado
- `403`: Sin permisos suficientes
- `404`: Recurso no encontrado
- `409`: Conflicto (ej: cliente ya inactivo)
- `422`: Feature flag no habilitado
- `500`: Error interno del servidor

## Integración con Sistemas Externos

### Sistema de Listas de Precios
- Validación de existencia de lista de precios al crear/actualizar cliente
- Obtención de nombre de lista de precios para responses

### Sistema de Notificaciones
- Envío de email de bienvenida al crear cliente
- Notificación de reset de contraseña
- Notificaciones de cambios importantes

### Sistema de Logs
- Registro de todas las operaciones CRUD
- Tracking de accesos y modificaciones
- Auditoría de cambios de contraseñas

## Consideraciones de Seguridad

1. **Hashing de Contraseñas:** Uso de bcrypt con salt rounds configurables
2. **Validación de Entrada:** Sanitización de todos los inputs
3. **Rate Limiting:** Implementación de límites de requests por IP
4. **Logs de Auditoría:** Registro completo de operaciones sensibles
5. **Validación de Empresa:** Aislamiento estricto por contexto de empresa