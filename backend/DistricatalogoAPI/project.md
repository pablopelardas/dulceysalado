# DistriCatalogo - Documentaci�n para Frontend

## Visi�n General del Proyecto

DistriCatalogo es una plataforma que permite a distribuidoras gestionar cat�logos web bajo un modelo **Hub-and-Spoke**. El sistema permite a una empresa principal administrar un cat�logo base que es consumido por m�ltiples empresas cliente, cada una con su propia personalizaci�n visual y configuraci�n.

## Arquitectura del Sistema

### Modelo Hub-and-Spoke

**Empresa Principal (Hub):**
- Administra el cat�logo base de productos y categor�as
- Actualiza precios, stock e informaci�n de productos
- Sincroniza datos desde sistema Gecom
- Gestiona las empresas cliente y sus configuraciones
- Tiene acceso completo a todas las funcionalidades del sistema

**Empresas Cliente (Spokes):**
- Consumen el cat�logo base de la empresa principal
- Personalizan su presentaci�n: logo, colores, t�tulo, datos de contacto
- Pueden agregar productos y categor�as propias (seg�n permisos)
- Acceden v�a subdominio personalizado (ej: `cliente1.districatalogo.com`)

## Acceso al Sistema

### Subdominio Detection
- Las empresas cliente acceden via subdominio: `{codigo-empresa}.districatalogo.com`
- El middleware `extractSubdomain` detecta autom�ticamente la empresa
- La empresa principal accede sin subdominio o con `admin.districatalogo.com`

### Autenticaci�n
- JWT Bearer Token authentication
- Cada usuario pertenece a una empresa espec�fica
- Permisos granulares por funcionalidad

## Tipos de Usuario y Permisos

### Empresa Principal
- **Admin Principal**: Acceso completo a todo el sistema
- **Editor Principal**: Gesti�n de cat�logo base y empresas cliente
- **Viewer Principal**: Solo lectura de reportes y estad�sticas

### Empresa Cliente
- **Admin Cliente**: Gesti�n completa de su empresa (usuarios, productos propios, configuraci�n)
- **Editor Cliente**: Gesti�n de productos y categor�as propias (si tiene permisos)
- **Viewer Cliente**: Solo visualizaci�n del cat�logo

### Permisos Espec�ficos
```javascript
{
  puede_gestionar_productos_base: boolean,      // Solo empresa principal
  puede_gestionar_categorias_base: boolean,     // Solo empresa principal
  puede_gestionar_productos_empresa: boolean,   // Productos propios
  puede_gestionar_categorias_empresa: boolean,  // Categor�as propias
  puede_gestionar_usuarios: boolean,            // Usuarios de su empresa
  puede_gestionar_empresas_cliente: boolean,    // Solo empresa principal
  puede_ver_reportes: boolean,                  // Solo empresa principal
  puede_gestionar_sync: boolean,                // Solo empresa principal
  puede_ver_estadisticas: boolean               // Estad�sticas b�sicas
}
```

## Estructura de Datos Principales

### Empresas
```javascript
{
  id: number,
  codigo: string,                    // C�digo �nico (usado para subdominio)
  nombre: string,                    // Nombre comercial
  razon_social: string,
  tipo_empresa: 'principal' | 'cliente',
  empresa_principal_id: number,      // Referencia a empresa principal
  
  // Configuraci�n visual
  colores_tema: {
    primario: string,    // #4A90E2
    secundario: string,  // #FF6B35  
    acento: string       // #8BC34A
  },
  logo_url: string,
  favicon_url: string,
  
  // Configuraci�n del cat�logo
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

### Categor�as
Similar estructura para `categorias_base` y `categorias_empresa`.

### Im�genes de Productos
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
  tama�o: number,
  tipo_mime: string
}
```

## Endpoints Principales de la API

### Autenticaci�n
- `POST /auth/login` - Login con email/password
- `POST /auth/refresh` - Renovar token
- `POST /auth/logout` - Cerrar sesi�n

### Usuarios
- `GET /api/users` - Listar usuarios de la empresa
- `POST /api/users` - Crear usuario
- `PUT /api/users/:id` - Actualizar usuario
- `DELETE /api/users/:id` - Eliminar usuario
- `PUT /api/users/:id/password` - Cambiar contrase�a

**Nota**: Empresa principal puede gestionar usuarios de empresas cliente enviando `empresa_id` en el body/query.

### Empresas Cliente (Solo empresa principal)
- `GET /api/admin/empresas-cliente` - Listar empresas cliente
- `POST /api/admin/empresas-cliente` - Crear empresa cliente
- `PUT /api/admin/empresas-cliente/:id` - Actualizar empresa cliente
- `DELETE /api/admin/empresas-cliente/:id` - Eliminar/desactivar empresa
- `PUT /api/admin/empresas-cliente/:id/permisos` - Configurar permisos

### Cat�logo Base (Solo empresa principal)
- `GET /api/admin/categorias-base` - Listar categor�as base
- `POST /api/admin/categorias-base` - Crear categor�a base
- `PUT /api/admin/categorias-base/:id` - Actualizar categor�a
- `DELETE /api/admin/categorias-base/:id` - Eliminar categor�a

- `GET /api/admin/productos-base` - Listar productos base
- `POST /api/admin/productos-base` - Crear producto base
- `PUT /api/admin/productos-base/:id` - Actualizar producto
- `DELETE /api/admin/productos-base/:id` - Eliminar producto

### Cat�logo Empresa (Empresa cliente con permisos)
- `GET /api/productos-empresa` - Listar productos propios
- `POST /api/productos-empresa` - Crear producto propio
- `PUT /api/productos-empresa/:id` - Actualizar producto propio
- `DELETE /api/productos-empresa/:id` - Eliminar producto propio

- `GET /api/categorias-empresa` - Listar categor�as propias
- `POST /api/categorias-empresa` - Crear categor�a propia
- `PUT /api/categorias-empresa/:id` - Actualizar categor�a propia
- `DELETE /api/categorias-empresa/:id` - Eliminar categor�a propia

### Configuraci�n Visual (Empresa cliente)
- `GET /api/configuracion/visual` - Obtener configuraci�n visual
- `PUT /api/configuracion/visual` - Actualizar colores del tema
- `POST /api/configuracion/imagenes/logo` - Subir logo
- `POST /api/configuracion/imagenes/favicon` - Subir favicon
- `DELETE /api/configuracion/imagenes/logo` - Eliminar logo
- `DELETE /api/configuracion/imagenes/favicon` - Eliminar favicon

- `GET /api/configuracion/contacto` - Obtener datos de contacto
- `PUT /api/configuracion/contacto` - Actualizar datos de contacto

### Im�genes de Productos
- `POST /api/productos/:id/imagenes` - Subir m�ltiples im�genes
- `GET /api/productos/:id/imagenes` - Listar im�genes del producto
- `PUT /api/productos/:id/imagenes/orden` - Reordenar im�genes
- `PUT /api/productos/:id/imagenes/:imagen_id/principal` - Establecer imagen principal
- `DELETE /api/productos/:id/imagenes/:imagen_id` - Eliminar imagen

### Cat�logo P�blico (Sin autenticaci�n)
- `GET /api/catalogo/:codigo_empresa` - Obtener cat�logo completo de la empresa
- `GET /api/catalogo/:codigo_empresa/productos` - Listar productos con filtros
- `GET /api/catalogo/:codigo_empresa/categorias` - Listar categor�as
- `GET /api/catalogo/:codigo_empresa/empresa` - Datos de la empresa (logo, colores, contacto)

### Reportes y Analytics (Solo empresa principal)
- `GET /api/admin/reportes/estadisticas` - Estad�sticas generales
- `GET /api/admin/reportes/empresas` - Reportes por empresa
- `GET /api/admin/reportes/performance` - M�tricas de performance
- `GET /api/admin/reportes/storage` - Uso de almacenamiento

## Flujos de Trabajo Frontend

### 1. Flujo de Login
1. Detectar subdominio para identificar empresa
2. Mostrar formulario de login
3. Enviar credenciales a `/auth/login`
4. Almacenar JWT token
5. Redireccionar seg�n tipo de empresa y permisos

### 2. Crear Empresa Cliente (Admin Principal)
1. Llenar formulario con datos de empresa
2. **Importante**: Incluir datos del usuario administrador (`admin_nombre`, `admin_email`, `admin_password`)
3. Enviar a `POST /api/admin/empresas-cliente`
4. El sistema autom�ticamente crea el usuario admin para la nueva empresa

### 3. Gesti�n de Usuarios
- Empresa principal puede crear usuarios en cualquier empresa cliente enviando `empresa_id`
- Empresa cliente solo puede crear usuarios en su propia empresa
- Validaciones autom�ticas de permisos seg�n tipo de empresa

### 4. Cat�logo Unificado
- Vista combinada de productos base + productos empresa
- Usar endpoint `/api/catalogo/:codigo_empresa` para obtener cat�logo completo
- Filtros por categor�a, b�squeda, precios disponibles

### 5. Personalizaci�n Visual
- Subir logo/favicon con validaciones de tama�o y formato
- Configurar colores del tema con picker de colores
- Vista previa en tiempo real de cambios

### 6. Gesti�n de Im�genes
- Drag & drop para subir m�ltiples im�genes
- Reordenamiento visual con drag & drop
- Generaci�n autom�tica de thumbnails y tama�os medianos
- Establecer imagen principal

## Consideraciones T�cnicas

### Subdominio Middleware
El sistema detecta autom�ticamente la empresa por subdominio. En desarrollo, usar:
- `localhost:3000` para empresa principal
- `cliente1.localhost:3000` para empresa cliente (requiere configuraci�n DNS local)

### Validaciones de Permisos
- Todas las operaciones validan permisos autom�ticamente
- Empresa principal tiene acceso a gestionar empresas cliente
- Empresa cliente solo accede a sus propios recursos

### Manejo de Im�genes
- Upload con Sharp para redimensionamiento autom�tico
- Formatos soportados: JPG, PNG, WebP
- Tama�os generados: original, medium (800px), thumbnail (200px)
- Almacenamiento en `/uploads/[tipo]/[empresa_id]/`

### Paginaci�n Est�ndar
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

### C�digos de Error Comunes
- `400` - Bad Request (datos inv�lidos)
- `401` - Unauthorized (no autenticado)
- `403` - Forbidden (sin permisos)
- `404` - Not Found
- `409` - Conflict (recurso duplicado)
- `500` - Internal Server Error

## Casos de Uso Principales

### CMS para Empresa Principal
- Dashboard con estad�sticas de todas las empresas cliente
- Gesti�n del cat�logo base (productos y categor�as)
- Administraci�n de empresas cliente y sus configuraciones
- Reportes y analytics del sistema
- Gesti�n de usuarios de todas las empresas

### CMS para Empresa Cliente
- Dashboard con estad�sticas propias
- Visualizaci�n del cat�logo completo (base + propios)
- Gesti�n de productos y categor�as propias (si tiene permisos)
- Configuraci�n visual de su cat�logo
- Gesti�n de usuarios de su empresa

### Cat�logo P�blico
- Visualizaci�n del cat�logo personalizado por empresa
- Filtros por categor�a, b�squeda, precios
- Responsive design con colores y logo de la empresa
- Informaci�n de contacto y redes sociales
- Funcionalidad de pedidos (si est� habilitada)

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

Esta documentaci�n debe complementarse con el archivo OpenAPI generado para tener la especificaci�n completa de todos los endpoints, esquemas de datos y ejemplos de request/response.