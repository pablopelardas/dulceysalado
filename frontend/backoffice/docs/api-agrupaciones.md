# API Documentation - Sistema de Agrupaciones

## Descripción General

El sistema de agrupaciones permite administrar grupos de productos basados en el campo `Grupo3` que viene desde GECOM, y configurar qué agrupaciones son visibles para cada empresa cliente.

## Arquitectura del Sistema

```
┌─────────────────┐    ┌──────────────────┐    ┌────────────────────┐
│   GECOM Sync   │───▶│   Agrupaciones   │───▶│  Visibilidad por   │
│  (Grupo3 → )   │    │   (Auto-creadas) │    │     Empresa        │
└─────────────────┘    └──────────────────┘    └────────────────────┘
                                │
                                ▼
                       ┌──────────────────┐
                       │ Catálogo Público │
                       │   (Filtrado)     │
                       └──────────────────┘
```

---

## 📋 Endpoints - Gestión de Agrupaciones

### 1. Listar Agrupaciones

**`GET /api/agrupaciones`**

Lista todas las agrupaciones de la empresa principal con paginación y filtros.

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Query Parameters:**
- `page` (int, opcional): Número de página (default: 1)
- `pageSize` (int, opcional): Elementos por página (default: 20)
- `activa` (bool, opcional): Filtrar por estado activo/inactivo
- `busqueda` (string, opcional): Buscar en nombre y descripción

**Ejemplo Request:**
```http
GET /api/agrupaciones?page=1&pageSize=10&activa=true&busqueda=electrodomesticos
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response 200:**
```json
{
  "success": true,
  "agrupaciones": [
    {
      "id": 1,
      "codigo": 17,
      "nombre": "Electrodomésticos",
      "descripcion": "Agrupación auto-creada desde sincronización para código Grupo3: 17",
      "activa": true,
      "empresa_principal_id": 1,
      "created_at": "2025-01-07T10:30:00Z",
      "updated_at": "2025-01-07T10:30:00Z"
    }
  ],
  "pagination": {
    "total": 25,
    "page": 1,
    "page_size": 10,
    "total_pages": 3
  }
}
```

---

### 2. Obtener Agrupación Específica

**`GET /api/agrupaciones/{id}`**

Obtiene los detalles de una agrupación específica.

**Ejemplo Request:**
```http
GET /api/agrupaciones/1
Authorization: Bearer {token}
```

**Response 200:**
```json
{
  "success": true,
  "agrupacion": {
    "id": 1,
    "codigo": 17,
    "nombre": "Electrodomésticos",
    "descripcion": "Productos de línea blanca y electrónicos",
    "activa": true,
    "empresa_principal_id": 1,
    "created_at": "2025-01-07T10:30:00Z",
    "updated_at": "2025-01-07T10:30:00Z"
  }
}
```

---

### 3. Actualizar Agrupación

**`PUT /api/agrupaciones/{id}`**

Actualiza los datos editables de una agrupación (nombre, descripción, activa).

**Request Body:**
```json
{
  "nombre": "Electrodomésticos Premium",
  "descripcion": "Productos de línea blanca de gama alta",
  "activa": true
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Agrupación actualizada exitosamente",
  "agrupacion": {
    "id": 1,
    "codigo": 17,
    "nombre": "Electrodomésticos Premium",
    "descripcion": "Productos de línea blanca de gama alta",
    "activa": true
  }
}
```

---

### 4. Estadísticas de Agrupaciones

**`GET /api/agrupaciones/stats`**

Obtiene estadísticas generales de agrupaciones de la empresa.

**Response 200:**
```json
{
  "success": true,
  "estadisticas": {
    "total_agrupaciones": 25,
    "agrupaciones_activas": 23,
    "agrupaciones_inactivas": 2,
    "empresa_principal_id": 1
  }
}
```

---

## 🏢 Endpoints - Configuración por Empresa

### 5. Ver Agrupaciones Visibles para una Empresa

**`GET /api/empresas/{empresaId}/agrupaciones`**

Obtiene todas las agrupaciones disponibles y marca cuáles son visibles para una empresa específica.

**Ejemplo Request:**
```http
GET /api/empresas/3/agrupaciones
Authorization: Bearer {token}
```

**Response 200:**
```json
{
  "success": true,
  "empresa_id": 3,
  "agrupaciones": [
    {
      "id": 1,
      "codigo": 17,
      "nombre": "Electrodomésticos",
      "descripcion": "Productos de línea blanca",
      "activa": true,
      "visible": true
    },
    {
      "id": 2,
      "codigo": 25,
      "nombre": "Herramientas",
      "descripcion": "Herramientas y equipos",
      "activa": true,
      "visible": false
    }
  ]
}
```

---

### 6. Configurar Visibilidad Individual

**`PUT /api/empresas/{empresaId}/agrupaciones`**

Configura qué agrupaciones son visibles para una empresa específica.

**Request Body:**
```json
{
  "agrupaciones_ids": [1, 3, 5, 8]
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Configuración de visibilidad actualizada correctamente",
  "empresa_id": 3,
  "agrupaciones_visibles": 4
}
```

---

### 7. 🚀 Configuración Masiva (Bulk Update)

**`PUT /api/empresas/agrupaciones/bulk`**

**¡NUEVO!** Configura visibilidad de agrupaciones para múltiples empresas de forma masiva. Perfecto para drag & drop.

**Request Body:**
```json
{
  "configuraciones": [
    {
      "empresa_id": 3,
      "agrupaciones_ids": [1, 2, 5]
    },
    {
      "empresa_id": 4,
      "agrupaciones_ids": [1, 3, 5, 8, 10]
    },
    {
      "empresa_id": 5,
      "agrupaciones_ids": []
    }
  ]
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Configuración bulk completada exitosamente",
  "empresas_procesadas": 3,
  "empresas_exitosas": 3,
  "empresas_con_errores": 0,
  "resultados": [
    {
      "empresa_id": 3,
      "agrupaciones_configuradas": 3,
      "success": true
    },
    {
      "empresa_id": 4,
      "agrupaciones_configuradas": 5,
      "success": true
    },
    {
      "empresa_id": 5,
      "agrupaciones_configuradas": 0,
      "success": true
    }
  ],
  "errores": []
}
```

---

## 🎯 Casos de Uso para Frontend

### Caso 1: Panel de Administración de Agrupaciones

```javascript
// Cargar agrupaciones con filtros
async function loadAgrupaciones(page = 1, search = '') {
  const response = await fetch(`/api/agrupaciones?page=${page}&pageSize=20&busqueda=${search}`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  return await response.json();
}

// Editar agrupación
async function updateAgrupacion(id, data) {
  const response = await fetch(`/api/agrupaciones/${id}`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
  });
  return await response.json();
}
```

### Caso 2: Drag & Drop Multi-Empresa

```javascript
// Obtener estado actual de múltiples empresas
async function loadEmpresasAgrupaciones(empresasIds) {
  const promises = empresasIds.map(id => 
    fetch(`/api/empresas/${id}/agrupaciones`, {
      headers: { 'Authorization': `Bearer ${token}` }
    }).then(r => r.json())
  );
  return await Promise.all(promises);
}

// Aplicar cambios masivos después del drag & drop
async function applyBulkChanges(configuraciones) {
  const response = await fetch('/api/empresas/agrupaciones/bulk', {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ configuraciones })
  });
  return await response.json();
}

// Ejemplo de uso con drag & drop
function handleDragDrop(draggedAgrupaciones, targetEmpresaId) {
  const configuraciones = [{
    empresa_id: targetEmpresaId,
    agrupaciones_ids: draggedAgrupaciones.map(a => a.id)
  }];
  
  return applyBulkChanges(configuraciones);
}
```

### Caso 3: Vista Empresa Individual

```javascript
// Cargar y editar agrupaciones de una empresa
async function manageEmpresaAgrupaciones(empresaId) {
  // Cargar estado actual
  const current = await fetch(`/api/empresas/${empresaId}/agrupaciones`, {
    headers: { 'Authorization': `Bearer ${token}` }
  }).then(r => r.json());
  
  // Después de cambios en UI, guardar
  const selectedIds = getSelectedAgrupacionIds(); // desde tu UI
  
  const result = await fetch(`/api/empresas/${empresaId}/agrupaciones`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ agrupaciones_ids: selectedIds })
  });
  
  return await result.json();
}
```

---

## 🔒 Autenticación y Permisos

**Requerimientos:**
- Todos los endpoints requieren autenticación JWT
- Solo empresas **principales** pueden gestionar agrupaciones
- Las agrupaciones pertenecen a la empresa principal del token
- Las empresas cliente solo pueden configurarse por su empresa principal

**Headers obligatorios:**
```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

---

## ⚡ Performance y Consideraciones

### Optimizaciones Implementadas
- **Bulk operations** para crear agrupaciones durante sync
- **Lazy loading** en endpoints con paginación
- **Batch processing** en configuración masiva
- **Database views** optimizadas para catálogo

### Límites Recomendados
- Máximo 100 agrupaciones por request bulk
- Paginación recomendada: 20-50 elementos
- Cache de 5 minutos para estadísticas

### Monitoreo
- Los logs incluyen métricas de performance
- Errores detallados en responses para debugging
- Tracking de operaciones bulk en logs

---

## 📊 Integración con Catálogo

Las agrupaciones se integran automáticamente en el catálogo público a través de la vista `vista_productos_precios_empresa`, que incluye:

- `agrupacion_codigo`: Código de la agrupación (desde Grupo3)
- `agrupacion_nombre`: Nombre editable de la agrupación  
- `agrupacion_activa`: Estado de la agrupación

**Filtrado automático:**
- Solo productos de agrupaciones visibles para la empresa
- Productos sin agrupación (grupo3 = null) siempre visibles
- Empresas sin configuración ven todas las agrupaciones

---

## 🧪 Testing

### Endpoints de Prueba
```bash
# Listar agrupaciones
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5250/api/agrupaciones"

# Configurar empresa
curl -X PUT -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"agrupaciones_ids": [1,2,3]}' \
  "http://localhost:5250/api/empresas/3/agrupaciones"

# Bulk update
curl -X PUT -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"configuraciones": [{"empresa_id": 3, "agrupaciones_ids": [1,2]}]}' \
  "http://localhost:5250/api/empresas/agrupaciones/bulk"
```

### Flujo de Testing Completo
1. **Sync productos** con Grupo3 → Verifica auto-creación
2. **Gestionar agrupaciones** → Editar nombres/descripciones  
3. **Configurar visibilidad** → Individual y bulk
4. **Verificar catálogo** → Productos filtrados correctamente

¡Listo para implementar tu drag & drop! 🎉