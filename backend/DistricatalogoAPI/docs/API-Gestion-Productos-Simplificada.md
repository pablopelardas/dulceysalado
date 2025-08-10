# API de Gestión de Productos y Catálogo - DistriCatalogoAPI (Simplificada)

## Descripción General

El sistema maneja dos tipos de empresas con **4 entidades principales**:
- **Empresa Principal**: Gestiona productos base y categorías base
- **Empresas Cliente**: Gestiona productos propios y categorías propias

## Entidades del Sistema

### 1. **ProductosBase** (Empresa Principal)
Productos administrados por la empresa principal, sincronizados desde ERP y con gestión web completa.

### 2. **ProductosEmpresa** (Empresas Cliente)  
Productos propios que cada empresa cliente puede crear y gestionar independientemente.

### 3. **CategoriasBase** (Empresa Principal)
Categorías globales creadas por la empresa principal, disponibles para todas las empresas.

### 4. **CategoriasEmpresa** (Empresas Cliente)
Categorías personalizadas que cada empresa cliente puede crear para organizar sus productos.

---

## 📋 GESTIÓN DE PRODUCTOS BASE (Empresa Principal)

### 🛍️ ProductosBase - `/api/productosbase`

#### 1. Listar Productos Base
```http
GET /api/productosbase
```

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Query Parameters:**
- `visible` (opcional): `true`/`false` para filtrar por visibilidad
- `destacado` (opcional): `true`/`false` para productos destacados
- `codigoRubro` (opcional): ID de categoría para filtrar
- `busqueda` (opcional): Texto de búsqueda en descripción/tags
- `page` (opcional): Número de página (default: 1)
- `pageSize` (opcional): Tamaño de página (default: 20)

**Response 200:**
```json
{
  "productos": [
    {
      "id": 1,
      "codigo": 12345,
      "descripcion": "Producto Ejemplo",
      "codigoRubro": 1,
      "precio": 99.99,
      "existencia": 10,
      "visible": true,
      "destacado": false,
      "ordenCategoria": 1,
      "imagenUrl": "https://ejemplo.com/imagen.jpg",
      "imagenAlt": "Descripción imagen",
      "descripcionCorta": "Descripción breve",
      "descripcionLarga": "Descripción detallada del producto...",
      "tags": "tag1,tag2,tag3",
      "codigoBarras": "1234567890",
      "marca": "Marca Ejemplo",
      "unidadMedida": "UN",
      "administradoPorEmpresaId": 1,
      "actualizadoGecom": "2023-01-01T00:00:00Z",
      "createdAt": "2023-01-01T00:00:00Z",
      "updatedAt": "2023-01-01T00:00:00Z"
    }
  ],
  "total": 150,
  "page": 1,
  "pageSize": 20
}
```

#### 2. Obtener Producto por ID
```http
GET /api/productosbase/{id}
```

#### 3. Obtener Producto por Código
```http
GET /api/productosbase/by-codigo/{codigo}
```

#### 4. Crear Producto Base
```http
POST /api/productosbase
```

**Request Body:**
```json
{
  "codigo": 12345,
  "descripcion": "Nuevo Producto",
  "codigoRubro": 1,
  "precio": 150.00,
  "existencia": 25,
  "visible": true,
  "destacado": false,
  "descripcionCorta": "Descripción breve",
  "descripcionLarga": "Descripción detallada...",
  "imagenUrl": "https://ejemplo.com/imagen.jpg",
  "imagenAlt": "Texto alternativo",
  "tags": "nuevo,categoria,ejemplo",
  "marca": "Mi Marca",
  "unidadMedida": "UN",
  "administradoPorEmpresaId": 1
}
```

#### 5. Actualizar Producto Base
```http
PUT /api/productosbase/{id}
```

#### 6. Eliminar Producto Base
```http
DELETE /api/productosbase/{id}
```

---

## 🏢 GESTIÓN DE PRODUCTOS EMPRESA (Empresas Cliente)

### 🛒 ProductosEmpresa - `/api/productosempresa`

#### 1. Listar Productos de Empresa
```http
GET /api/productosempresa?empresaId={empresaId}
```

**Query Parameters:**
- `empresaId` (obligatorio): ID de la empresa
- `visible` (opcional): `true`/`false` para filtrar por visibilidad
- `destacado` (opcional): `true`/`false` para productos destacados
- `codigoRubro` (opcional): ID de categoría para filtrar
- `busqueda` (opcional): Texto de búsqueda
- `page` (opcional): Número de página (default: 1)
- `pageSize` (opcional): Tamaño de página (default: 20)

**Response 200:**
```json
{
  "productos": [
    {
      "id": 1,
      "empresaId": 2,
      "codigo": 98765,
      "descripcion": "Producto Empresa",
      "codigoRubro": 2,
      "precio": 75.50,
      "existencia": 15,
      "visible": true,
      "destacado": true,
      "ordenCategoria": 1,
      "imagenUrl": "https://ejemplo.com/producto-empresa.jpg",
      "imagenAlt": "Producto de empresa",
      "descripcionCorta": "Producto exclusivo",
      "descripcionLarga": "Producto exclusivo de nuestra empresa...",
      "tags": "exclusivo,premium",
      "codigoBarras": "9876543210",
      "marca": "Marca Propia",
      "unidadMedida": "UN",
      "createdAt": "2023-01-01T00:00:00Z",
      "updatedAt": "2023-01-01T00:00:00Z"
    }
  ],
  "total": 45,
  "page": 1,
  "pageSize": 20
}
```

#### 2. Obtener Producto Empresa por ID
```http
GET /api/productosempresa/{id}
```

#### 3. Crear Producto Empresa
```http
POST /api/productosempresa
```

**Request Body:**
```json
{
  "empresaId": 2,
  "codigo": 98765,
  "descripcion": "Mi Producto",
  "codigoRubro": 2,
  "precio": 89.99,
  "existencia": 20,
  "visible": true,
  "destacado": false,
  "descripcionCorta": "Mi producto exclusivo",
  "descripcionLarga": "Descripción completa de mi producto...",
  "imagenUrl": "https://miempresa.com/producto.jpg",
  "tags": "propio,exclusivo",
  "marca": "Mi Marca",
  "unidadMedida": "UN"
}
```

#### 4. Actualizar Producto Empresa
```http
PUT /api/productosempresa/{id}
```

#### 5. Eliminar Producto Empresa
```http
DELETE /api/productosempresa/{id}
```

---

## 🏷️ GESTIÓN DE CATEGORÍAS

### Categorías Base - `/api/categories/base`
*(Ya implementado - sin cambios)*

### Categorías Empresa - `/api/categories/empresa`
*(Ya implementado - sin cambios)*

---

## 🛒 API PÚBLICA DE CATÁLOGO

### Catálogo Público - `/api/catalog`
*(Ya implementado - sin cambios)*

La vista `vista_catalogo_empresa` unifica automáticamente:
- Productos base visibles para la empresa
- Productos propios de la empresa
- Información de categorías (base + empresa)

---

## 🔐 AUTENTICACIÓN Y AUTORIZACIÓN

### Permisos por Tipo de Empresa

**Empresa Principal PUEDE:**
- ✅ Gestionar productos base (`/api/productosbase`)
- ✅ Gestionar categorías base (`/api/categories/base`)
- ✅ Sincronizar desde ERP (`/api/sync`)
- ✅ Ver todos los catálogos

**Empresas Cliente PUEDEN:**
- ✅ Gestionar productos propios (`/api/productosempresa`)
- ✅ Gestionar categorías propias (`/api/categories/empresa`)
- ✅ Ver catálogo público de productos base
- ❌ NO pueden modificar productos base
- ❌ NO pueden modificar categorías base

### Permisos de Usuario
Los usuarios necesitan estos permisos:
- `puede_gestionar_productos_base`: Para `/api/productosbase`
- `puede_gestionar_productos_empresa`: Para `/api/productosempresa`
- `puede_gestionar_categorias_base`: Para `/api/categories/base`
- `puede_gestionar_categorias_empresa`: Para `/api/categories/empresa`

---

## 🎯 Casos de Uso Prácticos

### Caso 1: Empresa Principal - Gestionar Producto Base

1. **Crear producto base:**
```http
POST /api/productosbase
```
```json
{
  "codigo": 12001,
  "descripcion": "Nuevo Producto ERP",
  "precio": 120.00,
  "existencia": 50,
  "visible": true,
  "imagenUrl": "https://cdn.ejemplo.com/nuevo-producto.jpg",
  "administradoPorEmpresaId": 1
}
```

### Caso 2: Empresa Cliente - Gestionar Producto Propio

1. **Crear producto exclusivo:**
```http
POST /api/productosempresa
```
```json
{
  "empresaId": 3,
  "codigo": 50001,
  "descripcion": "Producto Exclusivo de Mi Empresa",
  "precio": 200.00,
  "existencia": 10,
  "visible": true,
  "destacado": true,
  "imagenUrl": "https://miempresa.com/exclusivo.jpg"
}
```

### Caso 3: Consulta de Catálogo Unificado

```http
GET /api/catalog/empresa/3
```
*Retorna automáticamente productos base + productos empresa*

---

## ⚠️ Notas Importantes

### Diferencias Clave Entre ProductosBase y ProductosEmpresa

| Campo | ProductosBase | ProductosEmpresa |
|-------|---------------|------------------|
| `administradoPorEmpresaId` | ✅ Requerido | ❌ No existe |
| `empresaId` | ❌ No existe | ✅ Requerido |
| `actualizadoGecom` | ✅ Para sync ERP | ❌ No aplicable |
| Gestión | Solo empresa principal | Cada empresa cliente |

### Sincronización ERP
- Solo afecta `ProductosBase`
- Se mantiene el endpoint `/api/sync` existente
- Los `ProductosEmpresa` no se ven afectados por la sincronización

### Vista Unificada
La vista `vista_catalogo_empresa` combina automáticamente:
- Productos base donde la empresa tenga acceso
- Productos empresa propios de esa empresa

---

## 🐛 Códigos de Error

**401 Unauthorized:**
```json
{ "message": "Token de acceso inválido" }
```

**403 Forbidden:**
```json
{ "message": "No tienes permisos para gestionar productos base/empresa" }
```

**404 Not Found:**
```json
{ "message": "Producto no encontrado" }
```

**400 Bad Request:**
```json
{ "message": "Código de producto ya existe" }
```

---

## 📞 Endpoints de Referencia

### Productos
- `GET/POST/PUT/DELETE /api/productosbase` - Gestión productos base
- `GET/POST/PUT/DELETE /api/productosempresa` - Gestión productos empresa

### Categorías  
- `GET/POST/PUT/DELETE /api/categories/base` - Gestión categorías base
- `GET/POST/PUT/DELETE /api/categories/empresa` - Gestión categorías empresa

### Público
- `GET /api/catalog/empresa/{empresaId}` - Catálogo público unificado

### Sincronización
- `POST /api/sync/start` - Iniciar sync desde ERP (solo productos base)