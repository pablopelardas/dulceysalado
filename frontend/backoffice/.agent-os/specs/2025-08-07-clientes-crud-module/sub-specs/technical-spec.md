# Especificación Técnica

Esta es la especificación técnica para la spec detallada en @.agent-os/specs/2025-08-07-clientes-crud-module/spec.md

> Created: 2025-08-07
> Version: 1.0.0

## Technical Requirements

### Framework y Tecnologías Base
- **Frontend:** Nuxt 3 + Vue 3 + TypeScript
- **Estado:** Pinia store siguiendo patrón `stores/auth.ts`
- **API:** Composables siguiendo patrón `useProductosBase.ts`
- **Componentes:** Nuxt UI existente (UForm, UCard, UInput, UButton, etc.)
- **Validación:** Zod schema siguiendo patrón `ProductoBaseForm.vue`
- **Enrutamiento:** File-based routing de Nuxt
- **Autenticación:** JWT integrado con `useAuth()` composable

### Stack Tecnológico Específico
```json
{
  "dependencies": {
    "nuxt": "^3.17.5",
    "vue": "^3.5.17", 
    "@nuxt/ui": "^3.1.3",
    "@pinia/nuxt": "^0.11.1",
    "typescript": "^5.8.3",
    "tailwindcss": "^4.1.10"
  }
}
```

## Approach

### 1. Tipos TypeScript
```typescript
// types/clientes.ts
export interface ClienteDto {
  id: number
  codigo?: string
  nombre: string
  email?: string
  telefono?: string
  direccion?: string
  ciudad?: string
  provincia?: string
  codigo_postal?: string
  empresa_id: number
  activo: boolean
  tipo_cliente: 'minorista' | 'mayorista' | 'distribuidor'
  lista_precio_id?: number
  limite_credito?: number
  observaciones?: string
  created_at: string
  updated_at: string
}

export interface CreateClienteCommand {
  nombre: string
  email?: string
  telefono?: string
  direccion?: string
  ciudad?: string
  provincia?: string
  codigo_postal?: string
  activo?: boolean
  tipo_cliente: 'minorista' | 'mayorista' | 'distribuidor'
  lista_precio_id?: number
  limite_credito?: number
  observaciones?: string
}

export interface UpdateClienteCommand extends CreateClienteCommand {
  id: number
}

export interface ClientesFilters {
  busqueda?: string
  activo?: boolean
  tipo_cliente?: string
  ciudad?: string
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}
```

### 2. Composable
```typescript
// composables/useClientes.ts
import type { ClienteDto, CreateClienteCommand, UpdateClienteCommand, ClientesFilters } from '~/types/clientes'

export const useClientes = () => {
  const api = useApi()
  const auth = useAuth()
  const toast = useToast()
  
  // Estado reactivo
  const clientes = ref<ClienteDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  // Paginación
  const pagination = ref({
    page: 1,
    limit: 20,
    total: 0,
    pages: 0
  })
  
  // Filtros activos
  const filters = ref<ClientesFilters>({
    busqueda: '',
    activo: undefined,
    tipo_cliente: undefined,
    page: 1,
    pageSize: 20,
    sortBy: 'nombre',
    sortOrder: 'asc'
  })

  // Métodos CRUD siguiendo patrón useProductosBase
  const fetchClientes = async (customFilters?: ClientesFilters) => {
    // Implementación siguiendo patrón de useProductosBase.ts líneas 68-137
  }
  
  const fetchCliente = async (id: number) => {
    // Implementación siguiendo patrón de useProductosBase.ts líneas 140-159
  }
  
  const createCliente = async (clienteData: CreateClienteCommand) => {
    // Implementación siguiendo patrón de useProductosBase.ts líneas 184-213
  }
  
  const updateCliente = async (id: number, clienteData: UpdateClienteCommand) => {
    // Implementación siguiendo patrón de useProductosBase.ts líneas 216-253
  }
  
  const deleteCliente = async (id: number) => {
    // Implementación siguiendo patrón de useProductosBase.ts líneas 256-285
  }
  
  return {
    // Estado reactivo
    clientes: readonly(clientes),
    loading: readonly(loading),
    error: readonly(error),
    pagination: readonly(pagination),
    filters,
    
    // Acciones CRUD
    fetchClientes,
    fetchCliente,
    createCliente,
    updateCliente,
    deleteCliente
  }
}
```

### 3. Store Pinia
```typescript
// stores/clientes.ts
import { defineStore } from 'pinia'
import type { ClienteDto, ClientesFilters } from '~/types/clientes'

export const useClientesStore = defineStore('clientes', {
  state: () => ({
    clientes: [] as ClienteDto[],
    selectedCliente: null as ClienteDto | null,
    filters: {
      busqueda: '',
      activo: true,
      page: 1,
      pageSize: 20
    } as ClientesFilters,
    loading: false,
    error: null as string | null
  }),

  getters: {
    clientesActivos: (state) => state.clientes.filter(c => c.activo),
    clientesPorTipo: (state) => (tipo: string) => 
      state.clientes.filter(c => c.tipo_cliente === tipo),
    totalClientes: (state) => state.clientes.length
  },

  actions: {
    // Acciones siguiendo patrón de stores/auth.ts
  }
})
```

### 4. Componente Formulario Principal
```vue
<!-- components/clientes/ClienteForm.vue -->
<template>
  <UForm 
    ref="form"
    :schema="schema" 
    :state="formData"
    @submit="onSubmit"
    @error="onError"
    class="space-y-6"
  >
    <!-- Información básica -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Información Básica del Cliente
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Nombre" name="nombre" required>
          <UInput 
            v-model="formData.nombre"
            placeholder="Nombre del cliente"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Email" name="email">
          <UInput 
            v-model="formData.email"
            type="email"
            placeholder="email@ejemplo.com"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Teléfono" name="telefono">
          <UInput 
            v-model="formData.telefono"
            placeholder="Número de teléfono"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Tipo de Cliente" name="tipo_cliente" required>
          <USelectMenu
            v-model="formData.tipo_cliente"
            :items="tiposCliente"
            placeholder="Selecciona el tipo"
            :disabled="loading"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Dirección y ubicación -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Dirección y Ubicación
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <!-- Campos de dirección siguiendo patrón ProductoBaseForm -->
      </div>
    </UCard>

    <!-- Configuración comercial -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Configuración Comercial
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <!-- Lista de precios, límite crédito, etc. -->
      </div>
    </UCard>

    <!-- Botones de acción siguiendo patrón ProductoBaseForm líneas 310-327 -->
    <div class="flex justify-end space-x-3 pt-6">
      <UButton variant="ghost" color="gray" @click="onCancel" :disabled="loading">
        Cancelar
      </UButton>
      <UButton type="submit" color="primary" :loading="loading">
        {{ mode === 'create' ? 'Crear Cliente' : 'Actualizar Cliente' }}
      </UButton>
    </div>
  </UForm>
</template>

<script setup lang="ts">
// Implementación siguiendo patrón ProductoBaseForm.vue líneas 331-615
</script>
```

### 5. Páginas Principales
```vue
<!-- pages/clientes/index.vue -->
<template>
  <div class="space-y-6">
    <div class="flex justify-between items-center">
      <h1 class="text-2xl font-bold">Gestión de Clientes</h1>
      <UButton 
        v-if="userPermissions.canManageClientes"
        @click="navigateTo('/clientes/crear')"
        icon="i-heroicons-plus"
      >
        Nuevo Cliente
      </UButton>
    </div>

    <!-- Filtros y búsqueda -->
    <ClientesFiltros v-model="filters" @apply="applyFilters" />

    <!-- Lista de clientes con tabla -->
    <ClientesTable 
      :clientes="clientes"
      :loading="loading"
      :pagination="pagination"
      @edit="editCliente"
      @delete="deleteCliente"
      @page-change="changePage"
    />
  </div>
</template>

<!-- pages/clientes/crear.vue -->
<template>
  <div class="space-y-6">
    <div class="flex items-center gap-3">
      <UButton variant="ghost" icon="i-heroicons-arrow-left" @click="navigateBack" />
      <h1 class="text-2xl font-bold">Crear Cliente</h1>
    </div>

    <ClienteForm
      mode="create"
      :loading="loading"
      @submit="createCliente"
      @cancel="navigateBack"
    />
  </div>
</template>

<!-- pages/clientes/[id]/editar.vue -->
<template>
  <div class="space-y-6">
    <div class="flex items-center gap-3">
      <UButton variant="ghost" icon="i-heroicons-arrow-left" @click="navigateBack" />
      <h1 class="text-2xl font-bold">Editar Cliente</h1>
    </div>

    <ClienteForm
      mode="edit"
      :initial-data="cliente"
      :loading="loading"
      @submit="updateCliente"
      @cancel="navigateBack"
    />
  </div>
</template>
```

### 6. Integración con Autenticación JWT
```typescript
// Feature flag integration
const { hasFeature } = useFeatureFlags()
const canAccessClientes = computed(() => hasFeature('cliente_autenticacion'))

// Permisos de usuario
const { userPermissions } = useAuth()
const canManageClientes = computed(() => userPermissions.value.canManageClientes)
```

### 7. Validación con Zod
```typescript
// Siguiendo patrón ProductoBaseForm.vue líneas 384-403
const schema = z.object({
  nombre: z.string({ required_error: 'El nombre es requerido' })
    .min(1, 'El nombre no puede estar vacío'),
  email: z.string().email('Email inválido').optional(),
  telefono: z.string().optional(),
  tipo_cliente: z.enum(['minorista', 'mayorista', 'distribuidor'], {
    required_error: 'Debes seleccionar un tipo de cliente'
  }),
  activo: z.boolean().optional(),
  limite_credito: z.number().min(0, 'El límite no puede ser negativo').optional()
})
```

### 8. Componente Card para Dashboard
```vue
<!-- components/dashboard/ClientesCard.vue -->
<template>
  <UCard>
    <template #header>
      <div class="flex items-center justify-between">
        <h3 class="text-lg font-semibold">Clientes</h3>
        <UIcon name="i-heroicons-users" class="h-5 w-5 text-blue-500" />
      </div>
    </template>

    <div class="space-y-4">
      <div class="grid grid-cols-2 gap-4">
        <div>
          <p class="text-2xl font-bold text-blue-600">{{ stats.total }}</p>
          <p class="text-sm text-gray-500">Total Clientes</p>
        </div>
        <div>
          <p class="text-2xl font-bold text-green-600">{{ stats.activos }}</p>
          <p class="text-sm text-gray-500">Activos</p>
        </div>
      </div>
      
      <UButton 
        v-if="canManageClientes"
        block 
        variant="outline" 
        @click="navigateTo('/clientes')"
      >
        Gestionar Clientes
      </UButton>
    </div>
  </UCard>
</template>
```

### 9. Manejo de Errores y Loading
```typescript
// Siguiendo patrón useProductosBase.ts líneas 18-35
const getErrorMessage = (err: any): string => {
  if (err?.message) return err.message
  if (err?.data?.message) return err.data.message
  if (err?.data?.errors) {
    const errors = err.data.errors
    const firstError = Object.values(errors)[0] as string[]
    return firstError[0] || 'Error de validación'
  }
  return err?.statusText || err?.message || 'Error desconocido'
}
```

### 10. Responsive Design
- **Mobile First:** Grid columns que colapsan en mobile (`grid-cols-1 md:grid-cols-2`)
- **Breakpoints:** Tailwind CSS breakpoints estándar
- **Touch Friendly:** Botones y elementos táctiles apropiados
- **Navigation:** Drawer/sidebar para navegación móvil

## External Dependencies

### APIs Requeridas (Backend)
- **GET** `/api/clientes` - Listar clientes con filtros y paginación
- **GET** `/api/clientes/{id}` - Obtener cliente específico
- **POST** `/api/clientes` - Crear nuevo cliente
- **PUT** `/api/clientes/{id}` - Actualizar cliente existente
- **DELETE** `/api/clientes/{id}` - Eliminar cliente

### Endpoints de Soporte
- **GET** `/api/listas-precios` - Para asignar lista de precios a cliente
- **GET** `/api/feature-flags` - Para verificar feature flag "cliente_autenticacion"

### Composables Existentes a Utilizar
- `useApi()` - HTTP client configurado
- `useAuth()` - Autenticación JWT y permisos
- `useToast()` - Notificaciones al usuario
- `useFeatureFlags()` - Gestión de feature flags

### Stores Existentes
- `useAuthStore()` - Información de usuario y permisos
- Store de empresas para contexto empresarial

### Componentes Nuxt UI
- `UForm`, `UCard`, `UInput`, `UButton`, `USelectMenu`
- `UTable`, `UPagination`, `UBadge`, `UIcon`
- `UModal`, `UConfirmModal` para eliminaciones
- `UAlert` para mensajes de estado

### Validación y Esquemas
- `zod` para validación de formularios
- Patrones de validación existentes del proyecto

### Sin Dependencias Externas Nuevas
- Todo el desarrollo utiliza las dependencias ya presentes en package.json
- No se requiere instalación de nuevas librerías
- Aprovecha completamente el stack tecnológico existente