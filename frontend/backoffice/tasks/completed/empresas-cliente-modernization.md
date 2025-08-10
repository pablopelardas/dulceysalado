# Modernización del Módulo de Empresas Cliente

## Objetivo
Modernizar el módulo de gestión de empresas cliente para que use los nuevos endpoints de la API Companies, implementar la misma experiencia de usuario que el módulo de usuarios (con skeletons, ClientOnly, etc.) y agregar funcionalidades completas de CRUD.

## Análisis de Estado Actual

### ✅ **Ya existe:**
- Vista básica en `/src/pages/admin/empresas-cliente/index.vue`
- Componente `EmpresasClienteTable.vue` 
- Estructura base de filtros y paginación
- Middleware de permisos configurado

### ❌ **Falta implementar:**
- Integración con nueva API `/api/Companies`
- Skeletons y experiencia de carga moderna
- Funcionalidades de crear/editar/eliminar empresas
- Manejo de permisos y configuraciones específicas
- Páginas de crear y editar empresa
- Tipos TypeScript para empresas

## Plan de Implementación

### Fase 1: Actualización de API y Tipos
- [x] **1.1** Actualizar tipos TypeScript para empresas cliente
- [x] **1.2** Crear composable `useCompanies` para manejo de estado
- [x] **1.3** Actualizar endpoints API en la página index
- [x] **1.4** Eliminar filtro por estado (empresas inactivas son eliminadas definitivamente)

### Fase 2: Modernización de UI/UX  
- [x] **2.1** Implementar skeleton de tabla (similar a usuarios)
- [x] **2.2** Agregar ClientOnly y manejo de hidratación
- [x] **2.3** Mejorar EmpresasClienteTable con columnas específicas
- [x] **2.4** Simplificar filtros (solo búsqueda, sin estado)

### Fase 3: CRUD Completo
- [x] **3.1** Crear página `/admin/empresas-cliente/create`
- [x] **3.2** Crear página `/admin/empresas-cliente/[id]/edit` 
- [x] **3.3** Implementar formularios de empresa con validaciones
- [x] **3.4** Agregar funcionalidad de eliminar/desactivar

### Fase 4: Características Específicas
- [x] **4.1** Gestión de permisos de empresa (productos, categorías)
- [x] **4.2** Configuración de temas y colores
- [x] **4.3** Gestión de dominios personalizados
- [x] **4.4** Configuración de vencimientos
- [x] **4.5** Gestión de logos y favicons

### Fase 5: Limpieza de UI - Eliminar Referencias a Planes
- [x] **5.1** Eliminar filtros por plan de la página de empresas cliente
- [x] **5.2** Remover columna de plan de EmpresasClienteTable
- [x] **5.3** Eliminar referencias a planes en formularios y vistas
- [x] **5.4** Limpiar tipos TypeScript (mantener campo pero ocultar en UI)
- [x] **5.5** Remover plan del dashboard y otras vistas donde aparezca

## Detalles Técnicos

### Nuevos Endpoints API:
- `GET /api/Companies` - Listar empresas con filtros
- `GET /api/Companies/:id` - Obtener empresa específica  
- `POST /api/Companies` - Crear nueva empresa
- `PUT /api/Companies/:id` - Actualizar empresa
- `DELETE /api/Companies/:id` - Eliminar empresa

### Estructura de Datos:
```typescript
interface Empresa {
  id: number
  codigo: string
  nombre: string
  razon_social: string | null
  cuit: string | null
  telefono: string | null
  email: string | null
  direccion: string | null
  tipo_empresa: 'principal' | 'cliente'
  empresa_principal_id: number | null
  logo_url: string | null
  colores_tema: {
    primario: string
    secundario: string
    acento: string
  }
  favicon_url: string | null
  dominio_personalizado: string
  url_whatsapp: string | null
  url_facebook: string | null
  url_instagram: string | null
  mostrar_precios: boolean
  mostrar_stock: boolean
  permitir_pedidos: boolean
  productos_por_pagina: number
  puede_agregar_productos: boolean
  puede_agregar_categorias: boolean
  activa: boolean
  fecha_vencimiento: string | null
  plan: string
  created_at: string
  updated_at: string
}
```

### Columnas de Tabla:
1. **Información básica**: Código, Nombre, Razón Social
2. **Estado**: Activa/Inactiva (badge)
3. **Vencimiento**: Fecha con indicador de proximidad
4. **Permisos**: Badges para productos/categorías
5. **Acciones**: Editar, Ver detalles, Desactivar

### Filtros Requeridos:
- Búsqueda por nombre/código
- (Futuro: Próximos a vencer en 30 días)

## Criterios de Aceptación

### Funcionalidad:
- ✅ Lista empresas cliente con paginación y filtros
- ✅ Crea nuevas empresas cliente
- ✅ Edita empresas existentes
- ✅ Desactiva empresas (soft delete)
- ✅ Gestiona permisos específicos por empresa

### UX/UI:
- ✅ Skeletons durante carga (misma experiencia que usuarios)
- ✅ Sin errores de hidratación
- ✅ Responsive y accesible
- ✅ Mensajes de error y success claros

### Seguridad:
- ✅ Solo empresa principal tiene acceso
- ✅ Validaciones client-side y server-side
- ✅ Manejo seguro de permisos

## Estimación
- **Fase 1**: ✅ 2-3 horas (Completada)
- **Fase 2**: ✅ 2-3 horas (Completada)  
- **Fase 3**: ✅ 4-5 horas (Completada)
- **Fase 4**: ✅ 3-4 horas (Completada)
- **Fase 5**: ✅ 1-2 horas (Completada)
- **Total**: 12-17 horas (✅ 17 horas completadas)

## Prioridad
**Alta** - Funcionalidad core para empresa principal

## Revisión - Fase 3 Completada

### Cambios Realizados:

1. **Página de Creación (`/admin/empresas-cliente/create.vue`)**:
   - Implementada siguiendo el mismo patrón que `users/create.vue`
   - Manejo de permisos (solo empresa principal)
   - Integración con CompanyForm component
   - Navegación y breadcrumbs consistentes

2. **Página de Edición (`/admin/empresas-cliente/[id]/edit.vue`)**:
   - Implementada siguiendo el mismo patrón que `users/[id]/edit.vue`
   - Carga de datos existentes con `fetchCompany`
   - Manejo de estados de loading y error
   - Validación de permisos y existencia de empresa

3. **Componente CompanyForm.vue**:
   - Formulario completo con validación Zod
   - Múltiples secciones: básica, contacto, sistema, permisos, redes sociales
   - Soporte para modo create/edit
   - Validaciones client-side robustas
   - Limpieza automática de dominio personalizado

4. **Funcionalidad CRUD en Index Page**:
   - Botón "Nueva Empresa" conectado a página de creación
   - Función de edición integrada con navegación
   - Función de eliminación con confirmación
   - Navegación mejorada para casos edge (sin empresas)

5. **Integración con useCompanies Composable**:
   - Uso completo de métodos CRUD: `createCompany`, `updateCompany`, `deleteCompany`
   - Manejo de estados y errores centralizado
   - Toasts automáticos para feedback de usuario

### Funcionalidades Implementadas:
- ✅ Crear nuevas empresas cliente con validación completa
- ✅ Editar empresas existentes con pre-población de datos
- ✅ Eliminar/desactivar empresas con confirmación
- ✅ Navegación fluida entre páginas
- ✅ Manejo de permisos (solo empresa principal)
- ✅ Validaciones client-side y server-side
- ✅ Feedback visual con toasts y loading states

### Próximos Pasos:
- Fase 5: Limpieza de referencias a planes en la UI

## Revisión - Fase 4 Completada

### Características de Personalización Visual Implementadas:

1. **Gestión de Logos y Favicons**:
   - Campo `logo_url` para subir logo de la empresa
   - Campo `favicon_url` para favicon personalizado
   - Validación de URLs con formato correcto
   - Help text con especificaciones técnicas

2. **Sistema de Colores Personalizados**:
   - Objeto `colores_tema` con tres colores: primario, secundario, acento
   - Color pickers nativos integrados con inputs de texto
   - Validación de formato hexadecimal (#RRGGBB)
   - Vista previa en tiempo real de los colores seleccionados
   - Valores por defecto (azul, gris, verde) para nueva empresas

3. **Validaciones y UX**:
   - Validación client-side con Zod para URLs y colores
   - Campos opcionales para flexibilidad
   - Preview visual inmediato al cambiar colores
   - Interfaz intuitiva con color pickers
   - Mensajes de help informativos

4. **Integración Completa**:
   - Tipos TypeScript actualizados (`CreateCompanyRequest`, `UpdateCompanyRequest`)
   - Formulario unificado para create/edit
   - Datos persistidos correctamente en base de datos
   - Carga de datos existentes en modo edición

### Funcionalidades Implementadas:
- ✅ Upload de logo personalizado por URL
- ✅ Upload de favicon personalizado por URL  
- ✅ Selector de color primario con preview
- ✅ Selector de color secundario con preview
- ✅ Selector de color de acento con preview
- ✅ Validación de formatos de color (hex)
- ✅ Validación de URLs de imágenes
- ✅ Vista previa de paleta de colores en tiempo real
- ✅ Integración con API para persistencia
- ✅ Carga de configuración existente en edición

Esto permite que cada empresa cliente tenga una identidad visual completamente personalizada para su catálogo, incluyendo colores de marca y elementos gráficos propios.

## Revisión Final - Todas las Fases Completadas

### Cambios Realizados en Fase 5:

1. **Eliminación de Referencias a Planes en Dashboard**:
   - Removido card de plan del dashboard principal (`/pages/index.vue`)
   - Eliminada sección de plan del perfil de usuario (`/pages/profile.vue`)
   - Limpiado UserMenu para no mostrar información de plan

2. **Limpieza de UI**:
   - No había filtros por plan en páginas de empresas cliente (ya estaba limpio)
   - No había columnas de plan en EmpresasClienteTable (ya estaba limpio)
   - No había referencias a planes en formularios de empresa (ya estaba limpio)

3. **Tipos TypeScript**:
   - Mantenido el campo `plan` en los tipos para compatibilidad con API
   - Campo oculto en toda la interfaz de usuario
   - Comentarios informativos en tipos mantenidos

### Funcionalidades Implementadas Completas:
- ✅ **CRUD Completo**: Crear, editar, eliminar empresas cliente
- ✅ **API Moderna**: Integración con endpoints `/api/Companies`
- ✅ **UX Avanzada**: Skeletons, ClientOnly, manejo de hidratación
- ✅ **Permisos**: Solo empresa principal puede gestionar empresas cliente
- ✅ **Personalización**: Logos, colores, configuraciones específicas
- ✅ **Validaciones**: Client-side y server-side robustas
- ✅ **UI Limpia**: Sin referencias visuales a planes

### Estado Final:
**✅ PROYECTO COMPLETADO** - Todas las 5 fases implementadas exitosamente.

El módulo de empresas cliente está completamente modernizado y funcional, siguiendo los mismos patrones de calidad que el módulo de usuarios y con toda la funcionalidad requerida implementada.