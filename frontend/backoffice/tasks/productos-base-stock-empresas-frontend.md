# Especificación Frontend: Stock Diferencial por Empresa en Productos Base

## ID: productos-base-stock-empresas-frontend
## Slug: stock-diferencial-empresas

## Resumen
Modificar el CRUD de productos base en el frontend para agregar un selector de empresas que permita visualizar y editar el stock específico de cada empresa. La API ya está lista y solo necesita recibir el parámetro `empresaId` en los endpoints GET y PUT.

## Contexto Técnico
- **API lista**: Los endpoints ya soportan `empresaId` como query parameter
- **Endpoints afectados**:
  - `GET /api/ProductosBase?empresaId={id}` - Para obtener productos con stock específico
  - `PUT /api/ProductosBase/{id}?empresaId={id}` - Para actualizar stock de empresa específica

## Cambios Requeridos

### 1. Modificar Composable `useProductosBase.ts`

#### 1.1 Agregar estado para empresa seleccionada
```typescript
// Nuevo estado
const empresaSeleccionada = ref<number | null>(null)
```

#### 1.2 Modificar función `fetchProductos`
```typescript
// Línea ~92: Agregar empresaId a query params
const response = await api.get<GetAllProductosBaseQueryResult>('/api/ProductosBase', {
  query: {
    ...queryFilters,
    ...(empresaId && { empresaId })
  }
})
```

#### 1.3 Modificar función `updateProducto`
```typescript  
// Línea ~211: Agregar empresaId como query param
const response = await api.put<UpdateProductoBaseCommandResult>(
  `/api/ProductosBase/${id}${empresaId ? `?empresaId=${empresaId}` : ''}`, 
  productoData
)
```

### 2. Crear Componente Selector de Empresa

#### 2.1 Nuevo componente: `EmpresaSelector.vue`
```vue
<template>
  <div v-if="canSelectEmpresa" class="mb-4">
    <label class="block text-sm font-medium text-gray-700 mb-2">
      Ver stock de empresa
    </label>
    <select 
      :model-value="modelValue"
      @update:model-value="$emit('update:modelValue', $event)"
      class="block w-64 rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
    >
      <option :value="empresaPrincipal?.id">
        {{ empresaPrincipal?.nombre }} (Principal)
      </option>
      <option 
        v-for="empresa in empresasCliente" 
        :key="empresa.id" 
        :value="empresa.id"
      >
        {{ empresa.nombre }} (Cliente)
      </option>
    </select>
  </div>
</template>

<script setup lang="ts">
interface Props {
  modelValue?: number | null
}

interface Emits {
  (e: 'update:modelValue', value: number): void
}

defineProps<Props>()
defineEmits<Emits>()

// Lógica para obtener empresas disponibles
const { user } = useAuthStore()
const canSelectEmpresa = computed(() => user?.empresa?.tipo_empresa === 'principal')
</script>
```

### 3. Modificar Página Principal `/admin/productos-base/index.vue`

#### 3.1 Agregar selector y estado de empresa
```vue
<template>
  <div>
    <!-- Selector de empresa -->
    <EmpresaSelector 
      v-model="empresaSeleccionada"
      @update:model-value="onEmpresaChange"
    />
    
    <!-- Resto del contenido existente -->
  </div>
</template>

<script setup lang="ts">
// Estado para empresa seleccionada
const empresaSeleccionada = ref<number>()

// Inicializar según tipo de usuario
onMounted(() => {
  const { user } = useAuthStore()
  if (user?.empresa?.tipo_empresa === 'cliente') {
    // Empresas cliente solo ven su stock
    empresaSeleccionada.value = user.empresa.id
  } else {
    // Empresa principal ve su propio stock por defecto
    empresaSeleccionada.value = user?.empresa?.id
  }
  
  // Cargar productos con empresa inicial
  if (empresaSeleccionada.value) {
    fetchProductos()
  }
})

// Handler para cambio de empresa
const onEmpresaChange = (empresaId: number) => {
  empresaSeleccionada.value = empresaId
  // Actualizar query params en URL
  router.push({ 
    query: { ...route.query, empresaId: empresaId.toString() } 
  })
  // Recargar productos
  fetchProductos()
}

// Modificar fetchProductos para usar empresa seleccionada
const fetchProductos = () => {
  productosStore.fetchProductos({
    ...filters.value,
    empresaId: empresaSeleccionada.value
  })
}
</script>
```

### 4. Modificar Página de Edición `/admin/productos-base/[id]/edit.vue`

#### 4.1 Obtener empresaId desde query params
```vue
<script setup lang="ts">
const route = useRoute()

// Obtener empresaId desde query params o contexto de usuario
const empresaId = computed(() => {
  const queryEmpresaId = route.query.empresaId as string
  if (queryEmpresaId) return parseInt(queryEmpresaId)
  
  const { user } = useAuthStore()
  return user?.empresa?.id
})

// Pasar empresaId al formulario
const formProps = computed(() => ({
  empresaId: empresaId.value
}))
</script>

<template>
  <ProductoBaseForm 
    :producto-id="parseInt(route.params.id as string)"
    :empresa-id="empresaId"
  />
</template>
```

### 5. Modificar Formulario `ProductoBaseForm.vue`

#### 5.1 Agregar prop empresaId y mostrar contexto
```vue
<script setup lang="ts">
interface Props {
  productoId?: number
  empresaId?: number
}

const props = defineProps<Props>()

// Obtener nombre de empresa para mostrar contexto
const empresaNombre = ref<string>('')
onMounted(async () => {
  if (props.empresaId) {
    // Obtener nombre de empresa para mostrar en el formulario
    const empresas = await fetchEmpresas()
    const empresa = empresas.find(e => e.id === props.empresaId)
    empresaNombre.value = empresa?.nombre || ''
  }
})

// Modificar submit para incluir empresaId
const onSubmit = async () => {
  if (props.productoId) {
    await updateProducto(props.productoId, form.value, props.empresaId)
  } else {
    await createProducto(form.value)
  }
}
</script>

<template>
  <form @submit.prevent="onSubmit">
    <!-- Indicador de empresa activa -->
    <div v-if="empresaNombre" class="mb-4 p-3 bg-blue-50 rounded-lg">
      <p class="text-sm text-blue-700">
        Editando stock para: <strong>{{ empresaNombre }}</strong>
      </p>
    </div>
    
    <!-- Campos del formulario existentes -->
    
    <!-- Campo existencia con indicador de contexto -->
    <div class="mb-4">
      <label class="block text-sm font-medium text-gray-700 mb-2">
        Stock {{ empresaNombre ? `(${empresaNombre})` : '' }}
      </label>
      <input
        v-model.number="form.existencia"
        type="number"
        min="0"
        class="block w-full rounded-md border-gray-300"
      />
      <p v-if="empresaNombre" class="text-xs text-gray-500 mt-1">
        Este stock es específico para {{ empresaNombre }}
      </p>
    </div>
  </form>
</template>
```

### 6. Modificaciones en Tabla `ProductosBaseTable.vue`

#### 6.1 Agregar indicador de empresa en header
```vue
<template>
  <div>
    <!-- Header con indicador de empresa -->
    <div v-if="empresaContexto" class="mb-4 p-3 bg-gray-50 rounded-lg">
      <p class="text-sm text-gray-600">
        Mostrando stock para: <strong>{{ empresaContexto.nombre }}</strong>
      </p>
    </div>
    
    <!-- Tabla existente -->
    <table class="min-w-full">
      <thead>
        <tr>
          <!-- Columnas existentes -->
          <th>Stock</th>
          <!-- Más columnas -->
        </tr>
      </thead>
      <tbody>
        <tr v-for="producto in productos" :key="producto.id">
          <!-- Columnas existentes -->
          <td>
            <span class="inline-flex items-center">
              {{ producto.existencia ?? 0 }}
              <span v-if="empresaContexto?.tipo_empresa === 'cliente'" 
                    class="ml-1 text-xs text-blue-600">
                ({{ empresaContexto.nombre }})
              </span>
            </span>
          </td>
          <!-- Más columnas -->
        </tr>
      </tbody>
    </table>
  </div>
</template>
```

## Plan de Tareas

### Fase 1: Composable y Estado
- [ ] Modificar `useProductosBase.ts` para agregar `empresaSeleccionada`
- [ ] Actualizar `fetchProductos` para incluir `empresaId` en query
- [ ] Actualizar `updateProducto` para incluir `empresaId` en URL

### Fase 2: Componente Selector
- [ ] Crear componente `EmpresaSelector.vue`
- [ ] Implementar lógica de permisos (solo empresa principal puede seleccionar)
- [ ] Agregar estilos y validaciones

### Fase 3: Páginas Principal
- [ ] Modificar `/admin/productos-base/index.vue`
- [ ] Agregar selector de empresa
- [ ] Implementar query params para persistir selección
- [ ] Manejar estado inicial según tipo de usuario

### Fase 4: Página de Edición
- [ ] Modificar `/admin/productos-base/[id]/edit.vue`
- [ ] Obtener `empresaId` desde query params
- [ ] Pasar contexto al formulario

### Fase 5: Formulario
- [ ] Modificar `ProductoBaseForm.vue` 
- [ ] Agregar indicador de empresa activa
- [ ] Incluir `empresaId` en operaciones de actualización

### Fase 6: Tabla y UX
- [ ] Actualizar `ProductosBaseTable.vue`
- [ ] Agregar indicadores de contexto de empresa
- [ ] Mejorar UX con indicadores visuales

### Fase 7: Testing
- [ ] Probar flujo empresa principal seleccionando diferentes empresas
- [ ] Probar flujo empresa cliente (sin selector)
- [ ] Verificar query params y navegación
- [ ] Validar actualización de stock por empresa

## Archivos a Modificar

1. `/src/composables/useProductosBase.ts` - Agregar soporte empresaId
2. `/src/pages/admin/productos-base/index.vue` - Selector y estado
3. `/src/pages/admin/productos-base/[id]/edit.vue` - Query params
4. `/src/components/productos/ProductoBaseForm.vue` - Contexto empresa
5. `/src/components/productos/ProductosBaseTable.vue` - Indicadores UX
6. `/src/components/empresas/EmpresaSelector.vue` - **NUEVO** componente

## Consideraciones UX

### Para Empresa Principal
- Selector visible con todas las empresas
- Indicadores claros de qué empresa está visualizando
- Query params para mantener selección en navegación

### Para Empresa Cliente  
- Sin selector (automático a su empresa)
- Solo lectura en productos base
- Indicadores de que está viendo su stock específico

## Estimación de Tiempo
- **Fase 1-2**: 1-2 días
- **Fase 3-4**: 1-2 días  
- **Fase 5-6**: 1-2 días
- **Fase 7**: 1 día

**Total**: 4-7 días de desarrollo frontend