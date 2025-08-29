# API de Catálogo Público - Documentación para Desarrolladores

## Descripción General

La API de Catálogo Público permite acceder a los productos y categorías de las empresas registradas en el sistema. La API implementa resolución automática de empresas basada en subdominios y soporte para múltiples listas de precios.

## Base URL y Resolución de Empresas

### Resolución por Subdominio (Recomendado)
```
https://empresa1.dulceysalado.com/api/catalog
https://empresa2.dulceysalado.com/api/catalog
```

### Resolución por Query Parameter (Para Testing)
```
https://localhost:7000/api/catalog?empresaId=1
https://api.dulceysalado.com/api/catalog?empresaId=1
```

## Autenticación

La API es pública y no requiere autenticación para acceder a los catálogos.

## Endpoints Principales

### 1. Obtener Catálogo de Productos

**GET** `/api/catalog`

Obtiene el catálogo completo de productos con filtros opcionales.

#### Parámetros de Query

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `empresaId` | int | No | Override para testing. Si se omite, se resuelve automáticamente desde el subdominio |
| `listaPrecioId` | int | No | ID de la lista de precios a utilizar |
| `categoria` | string | No | Nombre de la categoría para filtrar |
| `busqueda` | string | No | Término de búsqueda en nombre y descripción |
| `destacados` | bool | No | Filtrar solo productos destacados |
| `codigoRubro` | int | No | Código de rubro/categoría para filtrar |
| `page` | int | No | Número de página (default: 1) |
| `pageSize` | int | No | Elementos por página (default: 20, máx: 100) |

#### Ejemplo de Petición
```http
GET /api/catalog?listaPrecioId=1&page=1&pageSize=20&codigoRubro=4
Host: empresa1.dulceysalado.com
```

#### Ejemplo de Respuesta
```json
{
  "productos": [
    {
      "codigo": "92",
      "nombre": "VINO LLUVIA NEGRA X 750",
      "descripcion": "VINO LLUVIA NEGRA X 750",
      "descripcion_corta": null,
      "precio": 245.320,
      "destacado": false,
      "imagen_urls": [
        "https://localhost:7000/api/images/92-vino-lluvia-negra-x-750.png"
      ],
      "stock": null,
      "tags": [],
      "marca": "",
      "unidad": "UN",
      "codigo_barras": null,
      "codigo_rubro": 4,
      "imagen_alt": null,
      "tipo_producto": "base",
      "lista_precio_id": 1,
      "lista_precio_nombre": "Lista Mayorista",
      "lista_precio_codigo": "MAY"
    }
  ],
  "total_count": 6,
  "page": 1,
  "page_size": 20,
  "total_pages": 1,
  "categorias": []
}
```

### 2. Obtener Categorías

**GET** `/api/catalog/categorias`

Obtiene todas las categorías disponibles para la empresa.

#### Parámetros de Query

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `empresaId` | int | No | Override para testing |

#### Ejemplo de Petición
```http
GET /api/catalog/categorias
Host: empresa1.dulceysalado.com
```

#### Ejemplo de Respuesta
```json
{
  "categorias": [
    {
      "id": 1,
      "codigo_rubro": 4,
      "nombre": "Vinos y Bebidas",
      "descripcion": "Productos alcohólicos y bebidas",
      "icono": "wine-glass",
      "color": "#8B0000",
      "orden": 1,
      "product_count": 12
    },
    {
      "id": 2,
      "codigo_rubro": 10,
      "nombre": "Adhesivos",
      "descripcion": "Pegamentos y adhesivos",
      "icono": "glue",
      "color": "#FFD700",
      "orden": 2,
      "product_count": 8
    }
  ]
}
```

### 3. Obtener Detalles de Producto

**GET** `/api/catalog/producto/{productoCodigo}`

Obtiene los detalles completos de un producto específico.

#### Parámetros

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `productoCodigo` | string | Sí | Código del producto en la URL |
| `empresaId` | int | No | Override para testing |
| `listaPrecioId` | int | No | ID de la lista de precios |

#### Ejemplo de Petición
```http
GET /api/catalog/producto/92?listaPrecioId=1
Host: empresa1.dulceysalado.com
```

### 4. Obtener Productos Destacados

**GET** `/api/catalog/destacados`

Obtiene los productos marcados como destacados.

#### Parámetros de Query

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `empresaId` | int | No | Override para testing |
| `listaPrecioId` | int | No | ID de la lista de precios |
| `limit` | int | No | Cantidad máxima de productos (default: 10, máx: 50) |

#### Ejemplo de Petición
```http
GET /api/catalog/destacados?listaPrecioId=1&limit=5
Host: empresa1.dulceysalado.com
```

### 5. Obtener Información de la Empresa

**GET** `/api/catalog/empresa`

Obtiene la información y configuración de la empresa para customizar el catálogo.

#### Parámetros de Query

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `empresaId` | int | No | Override para testing |

#### Ejemplo de Petición
```http
GET /api/catalog/empresa
Host: empresa1.dulceysalado.com
```

#### Ejemplo de Respuesta
```json
{
  "id": 1,
  "codigo": "EMP001",
  "nombre": "Empresa Demo",
  "razon_social": "Empresa Demo S.A.",
  "telefono": "+54 11 1234-5678",
  "email": "contacto@empresa1.com",
  "direccion": "Av. Principal 123, Buenos Aires",
  "logo_url": "https://localhost:7000/api/images/empresa1-logo.png",
  "colores_tema": "#FF6B35,#004E89,#F7F7FF",
  "favicon_url": "https://localhost:7000/api/images/empresa1-favicon.png",
  "dominio_personalizado": "empresa1.dulceysalado.com",
  "url_whatsapp": "https://wa.me/5491112345678",
  "url_facebook": "https://facebook.com/empresa1",
  "url_instagram": "https://instagram.com/empresa1",
  "mostrar_precios": true,
  "mostrar_stock": false,
  "permitir_pedidos": true,
  "productos_por_pagina": 20,
  "plan": "premium",
  "activa": true
}
```

## Configuraciones Dinámicas de Empresa

### Comportamiento Basado en Configuraciones

La API respeta las configuraciones específicas de cada empresa obtenidas del endpoint `/api/catalog/empresa`:

#### Mostrar Precios (`mostrar_precios`)
- **true**: Los campos `precio`, `lista_precio_id`, `lista_precio_nombre` y `lista_precio_codigo` se incluyen en las respuestas
- **false**: Estos campos se retornan como `null`

#### Mostrar Stock (`mostrar_stock`)
- **true**: El campo `stock` se incluye en las respuestas
- **false**: El campo `stock` se retorna como `null`

#### Permitir Pedidos (`permitir_pedidos`)
- Indica si la empresa permite realizar pedidos desde el catálogo público
- El frontend puede usar esta configuración para mostrar/ocultar botones de "Agregar al carrito"

#### Productos por Página (`productos_por_pagina`)
- Define el tamaño de página predeterminado para la empresa
- El frontend puede usar este valor como default para la paginación

### Ejemplo de Respuesta Condicionada

**Empresa con precios ocultos:**
```json
{
  "codigo": "92",
  "nombre": "VINO LLUVIA NEGRA X 750",
  "precio": null,
  "stock": 50,
  "lista_precio_id": null,
  "lista_precio_nombre": null,
  "lista_precio_codigo": null
}
```

**Empresa con stock oculto:**
```json
{
  "codigo": "92", 
  "nombre": "VINO LLUVIA NEGRA X 750",
  "precio": 245.320,
  "stock": null,
  "lista_precio_id": 1,
  "lista_precio_nombre": "Lista Mayorista",
  "lista_precio_codigo": "MAY"
}
```

## Manejo de Listas de Precios

### Conceptos Clave

- **Lista de Precios**: Cada empresa puede tener múltiples listas de precios (ej: mayorista, minorista, promocional)
- **Precios Dinámicos**: Los precios mostrados dependen de la lista seleccionada
- **Información de Lista**: Cada producto incluye información sobre qué lista de precios se utilizó (solo si `mostrar_precios` = true)

### Parámetros de Lista de Precios

- `listaPrecioId`: Especifica qué lista de precios utilizar
- Si no se especifica, se utilizará la lista predeterminada de la empresa

### Campos de Respuesta Relacionados

```json
{
  "precio": 245.320,
  "lista_precio_id": 1,
  "lista_precio_nombre": "Lista Mayorista", 
  "lista_precio_codigo": "MAY"
}
```

## Resolución de Empresas

### Por Subdominio (Producción)
```
empresa1.dulceysalado.com → EmpresaId: 1
empresa2.dulceysalado.com → EmpresaId: 2
```

### Por Query Parameter (Testing)
```
?empresaId=1 → Fuerza el uso de EmpresaId: 1
```

**Nota**: El parámetro `empresaId` es principalmente para testing y sobrescribe la resolución automática por subdominio.

## Códigos de Estado HTTP

| Código | Descripción |
|--------|-------------|
| 200 | Éxito |
| 404 | Producto/Empresa no encontrada |
| 500 | Error interno del servidor |

## Ejemplos de Uso Común

### 1. Cargar página principal del catálogo
```http
GET /api/catalog?page=1&pageSize=20
Host: empresa1.dulceysalado.com
```

### 2. Filtrar por categoría específica
```http
GET /api/catalog?codigoRubro=4&listaPrecioId=1
Host: empresa1.dulceysalado.com
```

### 3. Buscar productos
```http
GET /api/catalog?busqueda=vino&listaPrecioId=1
Host: empresa1.dulceysalado.com
```

### 4. Cargar categorías para navegación
```http
GET /api/catalog/categorias
Host: empresa1.dulceysalado.com
```

### 5. Ver productos destacados en homepage
```http
GET /api/catalog/destacados?limit=8&listaPrecioId=1
Host: empresa1.dulceysalado.com
```

### 6. Obtener configuración de empresa para personalización
```http
GET /api/catalog/empresa
Host: empresa1.dulceysalado.com
```

## Consideraciones de Implementación

1. **Paginación**: Siempre implementar paginación para listas de productos
2. **Caché**: Los datos de categorías pueden cachearse por períodos cortos
3. **Listas de Precios**: Permitir al usuario seleccionar diferentes listas si aplica
4. **Búsqueda**: Implementar debouncing para búsquedas en tiempo real
5. **Imágenes**: Las URLs de imágenes son absolutas y apuntan al servidor API
6. **Responsive**: Los endpoints soportan diferentes tamaños de página para mobile/desktop

## Notas Técnicas

- Todos los endpoints son **GET**
- Los responses utilizan **snake_case** para los nombres de propiedades JSON
- Las fechas están en formato ISO 8601 UTC
- Los precios son números decimales con hasta 3 decimales
- Los IDs son números enteros
- Los campos opcionales pueden ser `null`

## URLs de Testing Completas

### Con subdominio (Recomendado)
```
https://empresa1.dulceysalado.com/api/catalog
https://empresa1.dulceysalado.com/api/catalog/categorias
https://empresa1.dulceysalado.com/api/catalog/producto/92
https://empresa1.dulceysalado.com/api/catalog/destacados
```

### Con empresaId (Para testing local)
```
http://localhost:7000/api/catalog?empresaId=1
http://localhost:7000/api/catalog/categorias?empresaId=1
http://localhost:7000/api/catalog/producto/92?empresaId=1
http://localhost:7000/api/catalog/destacados?empresaId=1
http://localhost:7000/api/catalog/empresa?empresaId=1
```