# DistriCatalogo - Documentación para Frontend

## Visión General del Proyecto

DistriCatalogo es una plataforma que permite a distribuidoras gestionar catálogos web bajo un modelo **Hub-and-Spoke**. El sistema permite a una empresa principal administrar un catálogo base que es consumido por múltiples empresas cliente, cada una con su propia personalización visual y configuración.

## Arquitectura del Sistema

### Modelo Hub-and-Spoke

**Empresa Principal (Hub):**
- Administra el catálogo base de productos y categorías
- Actualiza precios, stock e información de productos
- Sincroniza datos desde sistema Gecom
- Gestiona las empresas cliente y sus configuraciones
- Tiene acceso completo a todas las funcionalidades del sistema

**Empresas Cliente (Spokes):**
- Consumen el catálogo base de la empresa principal
- Personalizan su presentación: logo, colores, título, datos de contacto
- Pueden agregar productos y categorías propias (según permisos)
- Acceden vía subdominio personalizado (ej: `cliente1.districatalogo.com`)

## Acceso al Sistema

### Subdominio Detection
- Las empresas cliente acceden via subdominio: `{codigo-empresa}.districatalogo.com`
- El middleware `extractSubdomain` detecta automáticamente la empresa
- La empresa principal accede sin subdominio o con `admin.districatalogo.com`

### Autenticación
- JWT Bearer Token authentication
- Cada usuario pertenece a una empresa específica
- Permisos granulares por funcionalidad

## Tipos de Usuario y Permisos

### Empresa Principal
- **Admin Principal**: Acceso completo a todo el sistema
- **Editor Principal**: Gestión de catálogo base y empresas cliente
- **Viewer Principal**: Solo lectura de reportes y estadísticas

### Empresa Cliente
- **Admin Cliente**: Gestión completa de su empresa (usuarios, productos propios, configuración)
- **Editor Cliente**: Gestión de productos y categorías propias (si tiene permisos)
- **Viewer Cliente**: Solo visualización del catálogo

### Permisos Específicos
```javascript
{
  puede_gestionar_productos_base: boolean,      // Solo empresa principal
  puede_gestionar_categorias_base: boolean,     // Solo empresa principal
  puede_gestionar_productos_empresa: boolean,   // Productos propios
  puede_gestionar_categorias_empresa: boolean,  // Categorías propias
  puede_gestionar_usuarios: boolean,            // Usuarios de su empresa
  puede_gestionar_empresas_cliente: boolean,    // Solo empresa principal
  puede_ver_reportes: boolean,                  // Solo empresa principal
  puede_gestionar_sync: boolean,                // Solo empresa principal
  puede_ver_estadisticas: boolean               // Estadísticas básicas
}
```

## Estructura de Datos Principales

### Empresas
```javascript
{
  id: number,
  codigo: string,                    // Código único (usado para subdominio)
  nombre: string,                    // Nombre comercial
  razon_social: string,
  tipo_empresa: 'principal' | 'cliente',
  empresa_principal_id: number,      // Referencia a empresa principal
  
  // Configuración visual
  colores_tema: {
    primario: string,    // #4A90E2
    secundario: string,  // #FF6B35  
    acento: string       // #8BC34A
  },
  logo_url: string,
  favicon_url: string,
  
  // Configuración del catálogo
  mostrar_precios: boolean,
  mostrar_stock: boolean,
  permitir_pedidos: boolean,
  productos_por_pagina: number,
  
  // Permisos
  puede_agregar_productos: boolean,
  puede_agregar_categorias: boolean,
  
  // Contacto y redes
  email: string,
  telefono: string,
  url_whatsapp: string,
  url_facebook: string,
  url_instagram: string,
  
  // Plan y estado
  plan: 'basico' | 'premium' | 'empresarial',
  activa: boolean,
  fecha_vencimiento: date
}
```

### Productos
```javascript
// Productos Base (gestionados por empresa principal)
{
  id: number,
  codigo: string,
  nombre: string,
  descripcion: string,
  precio: decimal,
  stock: number,
  categoria_id: number,
  activo: boolean,
  imagenes: ProductoImagen[]
}

// Productos Empresa (gestionados por empresa cliente)
{
  id: number,
  empresa_id: number,
  codigo: string,
  nombre: string,
  descripcion: string,
  precio: decimal,
  stock: number,
  categoria_id: number,
  activo: boolean,
  imagenes: ProductoImagen[]
}
```

### Categorías
Similar estructura para `categorias_base` y `categorias_empresa`.

### Imágenes de Productos
```javascript
{
  id: number,
  producto_id: number,
  tipo_producto: 'base' | 'empresa',
  nombre_archivo: string,
  url_original: string,
  url_thumbnail: string,
  url_medium: string,
  orden: number,
  es_principal: boolean,
  tamaño: number,
  tipo_mime: string
}
```

## Endpoints Principales de la API

### Autenticación
- `POST /auth/login` - Login con email/password
- `POST /auth/refresh` - Renovar token
- `POST /auth/logout` - Cerrar sesión

### Usuarios
- `GET /api/users` - Listar usuarios de la empresa
- `POST /api/users` - Crear usuario
- `PUT /api/users/:id` - Actualizar usuario
- `DELETE /api/users/:id` - Eliminar usuario
- `PUT /api/users/:id/password` - Cambiar contraseña

**Nota**: Empresa principal puede gestionar usuarios de empresas cliente enviando `empresa_id` en el body/query.

### Empresas Cliente (Solo empresa principal)
- `GET /api/admin/empresas-cliente` - Listar empresas cliente
- `POST /api/admin/empresas-cliente` - Crear empresa cliente
- `PUT /api/admin/empresas-cliente/:id` - Actualizar empresa cliente
- `DELETE /api/admin/empresas-cliente/:id` - Eliminar/desactivar empresa
- `PUT /api/admin/empresas-cliente/:id/permisos` - Configurar permisos

### Catálogo Base (Solo empresa principal)
- `GET /api/admin/categorias-base` - Listar categorías base
- `POST /api/admin/categorias-base` - Crear categoría base
- `PUT /api/admin/categorias-base/:id` - Actualizar categoría
- `DELETE /api/admin/categorias-base/:id` - Eliminar categoría

- `GET /api/admin/productos-base` - Listar productos base
- `POST /api/admin/productos-base` - Crear producto base
- `PUT /api/admin/productos-base/:id` - Actualizar producto
- `DELETE /api/admin/productos-base/:id` - Eliminar producto

### Catálogo Empresa (Empresa cliente con permisos)
- `GET /api/productos-empresa` - Listar productos propios
- `POST /api/productos-empresa` - Crear producto propio
- `PUT /api/productos-empresa/:id` - Actualizar producto propio
- `DELETE /api/productos-empresa/:id` - Eliminar producto propio

- `GET /api/categorias-empresa` - Listar categorías propias
- `POST /api/categorias-empresa` - Crear categoría propia
- `PUT /api/categorias-empresa/:id` - Actualizar categoría propia
- `DELETE /api/categorias-empresa/:id` - Eliminar categoría propia

### Configuración Visual (Empresa cliente)
- `GET /api/configuracion/visual` - Obtener configuración visual
- `PUT /api/configuracion/visual` - Actualizar colores del tema
- `POST /api/configuracion/imagenes/logo` - Subir logo
- `POST /api/configuracion/imagenes/favicon` - Subir favicon
- `DELETE /api/configuracion/imagenes/logo` - Eliminar logo
- `DELETE /api/configuracion/imagenes/favicon` - Eliminar favicon

- `GET /api/configuracion/contacto` - Obtener datos de contacto
- `PUT /api/configuracion/contacto` - Actualizar datos de contacto

### Imágenes de Productos
- `POST /api/productos/:id/imagenes` - Subir múltiples imágenes
- `GET /api/productos/:id/imagenes` - Listar imágenes del producto
- `PUT /api/productos/:id/imagenes/orden` - Reordenar imágenes
- `PUT /api/productos/:id/imagenes/:imagen_id/principal` - Establecer imagen principal
- `DELETE /api/productos/:id/imagenes/:imagen_id` - Eliminar imagen

### Catálogo Público (Sin autenticación)
- `GET /api/catalogo/:codigo_empresa` - Obtener catálogo completo de la empresa
- `GET /api/catalogo/:codigo_empresa/productos` - Listar productos con filtros
- `GET /api/catalogo/:codigo_empresa/categorias` - Listar categorías
- `GET /api/catalogo/:codigo_empresa/empresa` - Datos de la empresa (logo, colores, contacto)

### Reportes y Analytics (Solo empresa principal)
- `GET /api/admin/reportes/estadisticas` - Estadísticas generales
- `GET /api/admin/reportes/empresas` - Reportes por empresa
- `GET /api/admin/reportes/performance` - Métricas de performance
- `GET /api/admin/reportes/storage` - Uso de almacenamiento

## Flujos de Trabajo Frontend

### 1. Flujo de Login
1. Detectar subdominio para identificar empresa
2. Mostrar formulario de login
3. Enviar credenciales a `/auth/login`
4. Almacenar JWT token
5. Redireccionar según tipo de empresa y permisos

### 2. Crear Empresa Cliente (Admin Principal)
1. Llenar formulario con datos de empresa
2. **Importante**: Incluir datos del usuario administrador (`admin_nombre`, `admin_email`, `admin_password`)
3. Enviar a `POST /api/admin/empresas-cliente`
4. El sistema automáticamente crea el usuario admin para la nueva empresa

### 3. Gestión de Usuarios
- Empresa principal puede crear usuarios en cualquier empresa cliente enviando `empresa_id`
- Empresa cliente solo puede crear usuarios en su propia empresa
- Validaciones automáticas de permisos según tipo de empresa

### 4. Catálogo Unificado
- Vista combinada de productos base + productos empresa
- Usar endpoint `/api/catalogo/:codigo_empresa` para obtener catálogo completo
- Filtros por categoría, búsqueda, precios disponibles

### 5. Personalización Visual
- Subir logo/favicon con validaciones de tamaño y formato
- Configurar colores del tema con picker de colores
- Vista previa en tiempo real de cambios

### 6. Gestión de Imágenes
- Drag & drop para subir múltiples imágenes
- Reordenamiento visual con drag & drop
- Generación automática de thumbnails y tamaños medianos
- Establecer imagen principal

## Consideraciones Técnicas

### Subdominio Middleware
El sistema detecta automáticamente la empresa por subdominio. En desarrollo, usar:
- `localhost:3000` para empresa principal
- `cliente1.localhost:3000` para empresa cliente (requiere configuración DNS local)

### Validaciones de Permisos
- Todas las operaciones validan permisos automáticamente
- Empresa principal tiene acceso a gestionar empresas cliente
- Empresa cliente solo accede a sus propios recursos

### Manejo de Imágenes
- Upload con Sharp para redimensionamiento automático
- Formatos soportados: JPG, PNG, WebP
- Tamaños generados: original, medium (800px), thumbnail (200px)
- Almacenamiento en `/uploads/[tipo]/[empresa_id]/`

### Paginación Estándar
```javascript
{
  items: [...],
  pagination: {
    page: 1,
    limit: 20,
    total: 150,
    pages: 8
  }
}
```

### Códigos de Error Comunes
- `400` - Bad Request (datos inválidos)
- `401` - Unauthorized (no autenticado)
- `403` - Forbidden (sin permisos)
- `404` - Not Found
- `409` - Conflict (recurso duplicado)
- `500` - Internal Server Error

## Casos de Uso Principales

### CMS para Empresa Principal
- Dashboard con estadísticas de todas las empresas cliente
- Gestión del catálogo base (productos y categorías)
- Administración de empresas cliente y sus configuraciones
- Reportes y analytics del sistema
- Gestión de usuarios de todas las empresas

### CMS para Empresa Cliente
- Dashboard con estadísticas propias
- Visualización del catálogo completo (base + propios)
- Gestión de productos y categorías propias (si tiene permisos)
- Configuración visual de su catálogo
- Gestión de usuarios de su empresa

### Catálogo Público
- Visualización del catálogo personalizado por empresa
- Filtros por categoría, búsqueda, precios
- Responsive design con colores y logo de la empresa
- Información de contacto y redes sociales
- Funcionalidad de pedidos (si está habilitada)

## Variables de Entorno Requeridas

```env
# Base de datos
DB_HOST=localhost
DB_PORT=3306
DB_NAME=districatalogo
DB_USER=root
DB_PASSWORD=

# JWT
JWT_SECRET=your-secret-key
JWT_EXPIRES_IN=24h

# Uploads
UPLOAD_PATH=./uploads
MAX_FILE_SIZE=5MB

# CORS
FRONTEND_URL=http://localhost:3000
ALLOWED_ORIGINS=http://localhost:3000,https://yourdomain.com
```

Esta documentación debe complementarse con el archivo OpenAPI generado para tener la especificación completa de todos los endpoints, esquemas de datos y ejemplos de request/response.