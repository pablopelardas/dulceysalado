<template>
  <UCard>
    <template #header>
      <div class="flex items-center justify-between">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Filtros de Búsqueda
        </h3>
        <UButton
          variant="ghost"
          size="sm"
          color="gray"
          @click="clearAllFilters"
        >
          <UIcon name="i-heroicons-x-mark" class="mr-1" />
          Limpiar
        </UButton>
      </div>
    </template>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
      <!-- Búsqueda general -->
      <UFormField label="Buscar">
        <UInput
          v-model="localFilters.busqueda"
          placeholder="Código, descripción, marca..."
          icon="i-heroicons-magnifying-glass"
          @input="debouncedApplyFilters"
        />
      </UFormField>

      <!-- Filtro por rubro -->
      <UFormField label="Código de Rubro">
        <UInput
          v-model.number="localFilters.codigoRubro"
          type="number"
          placeholder="Filtrar por rubro"
          @input="applyFilters"
        />
      </UFormField>

      <!-- Filtro de visibilidad -->
      <UFormField label="Visibilidad">
        <USelectMenu
          v-model="localFilters.visible"
          :options="visibilityOptions"
          placeholder="Todos"
          @change="applyFilters"
        />
      </UFormField>

      <!-- Filtro destacado -->
      <UFormField label="Destacado">
        <USelectMenu
          v-model="localFilters.destacado"
          :options="destacadoOptions"
          placeholder="Todos"
          @change="applyFilters"
        />
      </UFormField>

      <!-- Rango de precios -->
      <UFormField label="Precio Mínimo" class="md:col-span-1">
        <UInput
          v-model.number="localFilters.precioMin"
          type="number"
          step="0.01"
          placeholder="0.00"
          @input="debouncedApplyFilters"
        />
      </UFormField>

      <UFormField label="Precio Máximo" class="md:col-span-1">
        <UInput
          v-model.number="localFilters.precioMax"
          type="number"
          step="0.01"
          placeholder="999999.99"
          @input="debouncedApplyFilters"
        />
      </UFormField>

      <!-- Filtro de stock -->
      <UFormField label="Stock Mínimo">
        <UInput
          v-model.number="localFilters.stockMin"
          type="number"
          placeholder="0"
          @input="debouncedApplyFilters"
        />
      </UFormField>

      <!-- Filtro por marca -->
      <UFormField label="Marca">
        <UInput
          v-model="localFilters.marca"
          placeholder="Filtrar por marca"
          @input="debouncedApplyFilters"
        />
      </UFormField>
    </div>

    <!-- Filtros avanzados (colapsable) -->
    <UCollapsible v-model="showAdvancedFilters" class="mt-4">
      <template #header>
        <div class="flex items-center gap-2">
          <UIcon name="i-heroicons-adjustments-horizontal" class="h-4 w-4" />
          <span class="text-sm font-medium">Filtros Avanzados</span>
        </div>
      </template>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
        <!-- Con imagen -->
        <UFormField label="Imágenes">
          <USelectMenu
            v-model="localFilters.conImagen"
            :options="imageOptions"
            placeholder="Todos"
            @change="applyFilters"
          />
        </UFormField>

        <!-- Con código de barras -->
        <UFormField label="Código de Barras">
          <USelectMenu
            v-model="localFilters.conCodigoBarras"
            :options="barcodeOptions"
            placeholder="Todos"
            @change="applyFilters"
          />
        </UFormField>

        <!-- Orden en categoría -->
        <UFormField label="Con Orden Definido">
          <USelectMenu
            v-model="localFilters.conOrden"
            :options="orderOptions"
            placeholder="Todos"
            @change="applyFilters"
          />
        </UFormField>
      </div>
    </UCollapsible>

    <!-- Resumen de filtros activos -->
    <div v-if="hasActiveFilters" class="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
      <div class="flex flex-wrap gap-2">
        <span class="text-sm text-gray-600 dark:text-gray-400">Filtros activos:</span>
        
        <UBadge
          v-if="localFilters.busqueda"
          variant="soft"
          color="blue"
          @click="clearFilter('busqueda')"
          class="cursor-pointer"
        >
          Búsqueda: {{ localFilters.busqueda }}
          <UIcon name="i-heroicons-x-mark" class="ml-1 h-3 w-3" />
        </UBadge>

        <UBadge
          v-if="localFilters.codigoRubro"
          variant="soft"
          color="green"
          @click="clearFilter('codigoRubro')"
          class="cursor-pointer"
        >
          Rubro: {{ localFilters.codigoRubro }}
          <UIcon name="i-heroicons-x-mark" class="ml-1 h-3 w-3" />
        </UBadge>

        <UBadge
          v-if="localFilters.visible !== undefined && localFilters.visible !== 'all'"
          variant="soft"
          color="purple"
          @click="clearFilter('visible')"
          class="cursor-pointer"
        >
          {{ localFilters.visible ? 'Visible' : 'Oculto' }}
          <UIcon name="i-heroicons-x-mark" class="ml-1 h-3 w-3" />
        </UBadge>

        <UBadge
          v-if="localFilters.destacado !== undefined && localFilters.destacado !== 'all'"
          variant="soft"
          color="yellow"
          @click="clearFilter('destacado')"
          class="cursor-pointer"
        >
          {{ localFilters.destacado ? 'Destacado' : 'No destacado' }}
          <UIcon name="i-heroicons-x-mark" class="ml-1 h-3 w-3" />
        </UBadge>

        <UBadge
          v-if="localFilters.marca"
          variant="soft"
          color="orange"
          @click="clearFilter('marca')"
          class="cursor-pointer"
        >
          Marca: {{ localFilters.marca }}
          <UIcon name="i-heroicons-x-mark" class="ml-1 h-3 w-3" />
        </UBadge>
      </div>
    </div>

    <!-- Estadísticas -->
    <div class="flex justify-between items-center mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
      <div class="text-sm text-gray-500 dark:text-gray-400">
        {{ resultCount }} producto{{ resultCount !== 1 ? 's' : '' }} encontrado{{ resultCount !== 1 ? 's' : '' }}
      </div>
      
      <UButton
        variant="ghost"
        size="sm"
        color="blue"
        @click="exportFilters"
      >
        <UIcon name="i-heroicons-arrow-down-tray" class="mr-1" />
        Exportar
      </UButton>
    </div>
  </UCard>
</template>

<script setup lang="ts">
import type { ProductosBaseFilters } from '~/types/productos'

interface ExtendedFilters extends ProductosBaseFilters {
  precioMin?: number
  precioMax?: number
  stockMin?: number
  marca?: string
  conImagen?: boolean | 'all'
  conCodigoBarras?: boolean | 'all'
  conOrden?: boolean | 'all'
}

interface Props {
  filters: ProductosBaseFilters
  resultCount?: number
}

interface Emits {
  'update:filters': [filters: ProductosBaseFilters]
  'clear-filters': []
}

const props = withDefaults(defineProps<Props>(), {
  resultCount: 0
})

const emit = defineEmits<Emits>()

// Composables
const { $debounce } = useNuxtApp()

// Estado local
const localFilters = ref<ExtendedFilters>({
  busqueda: props.filters.busqueda || '',
  codigoRubro: props.filters.codigoRubro,
  visible: props.filters.visible ?? 'all',
  destacado: props.filters.destacado ?? 'all',
  precioMin: undefined,
  precioMax: undefined,
  stockMin: undefined,
  marca: '',
  conImagen: 'all',
  conCodigoBarras: 'all',
  conOrden: 'all'
})

const showAdvancedFilters = ref(false)

// Opciones para selects
const visibilityOptions = [
  { label: 'Todos', value: 'all' },
  { label: 'Visible', value: true },
  { label: 'Oculto', value: false }
]

const destacadoOptions = [
  { label: 'Todos', value: 'all' },
  { label: 'Destacado', value: true },
  { label: 'No destacado', value: false }
]

const imageOptions = [
  { label: 'Todos', value: 'all' },
  { label: 'Con imagen', value: true },
  { label: 'Sin imagen', value: false }
]

const barcodeOptions = [
  { label: 'Todos', value: 'all' },
  { label: 'Con código de barras', value: true },
  { label: 'Sin código de barras', value: false }
]

const orderOptions = [
  { label: 'Todos', value: 'all' },
  { label: 'Con orden definido', value: true },
  { label: 'Sin orden definido', value: false }
]

// Computed
const hasActiveFilters = computed(() => {
  return !!(
    localFilters.value.busqueda ||
    localFilters.value.codigoRubro ||
    (localFilters.value.visible !== 'all' && localFilters.value.visible !== undefined) ||
    (localFilters.value.destacado !== 'all' && localFilters.value.destacado !== undefined) ||
    localFilters.value.precioMin ||
    localFilters.value.precioMax ||
    localFilters.value.stockMin ||
    localFilters.value.marca ||
    (localFilters.value.conImagen !== 'all') ||
    (localFilters.value.conCodigoBarras !== 'all') ||
    (localFilters.value.conOrden !== 'all')
  )
})

// Métodos
const applyFilters = () => {
  const filters: ProductosBaseFilters = {
    busqueda: localFilters.value.busqueda || undefined,
    codigoRubro: localFilters.value.codigoRubro || undefined,
    visible: localFilters.value.visible !== 'all' ? localFilters.value.visible as boolean : undefined,
    destacado: localFilters.value.destacado !== 'all' ? localFilters.value.destacado as boolean : undefined,
    page: 1 // Reset page when applying filters
  }

  emit('update:filters', filters)
}

const debouncedApplyFilters = $debounce(applyFilters, 300)

const clearFilter = (filterName: keyof ExtendedFilters) => {
  switch (filterName) {
    case 'busqueda':
    case 'marca':
      localFilters.value[filterName] = ''
      break
    case 'codigoRubro':
    case 'precioMin':
    case 'precioMax':
    case 'stockMin':
      localFilters.value[filterName] = undefined
      break
    case 'visible':
    case 'destacado':
    case 'conImagen':
    case 'conCodigoBarras':
    case 'conOrden':
      localFilters.value[filterName] = 'all'
      break
  }
  applyFilters()
}

const clearAllFilters = () => {
  localFilters.value = {
    busqueda: '',
    codigoRubro: undefined,
    visible: 'all',
    destacado: 'all',
    precioMin: undefined,
    precioMax: undefined,
    stockMin: undefined,
    marca: '',
    conImagen: 'all',
    conCodigoBarras: 'all',
    conOrden: 'all'
  }
  emit('clear-filters')
}

const exportFilters = () => {
  // Funcionalidad para exportar los productos filtrados
  const filterParams = new URLSearchParams()
  
  Object.entries(localFilters.value).forEach(([key, value]) => {
    if (value !== undefined && value !== '' && value !== 'all') {
      filterParams.append(key, String(value))
    }
  })
  
  // Aquí se podría implementar la exportación real
}

// Watchers
watch(() => props.filters, (newFilters) => {
  localFilters.value = {
    ...localFilters.value,
    busqueda: newFilters.busqueda || '',
    codigoRubro: newFilters.codigoRubro,
    visible: newFilters.visible ?? 'all',
    destacado: newFilters.destacado ?? 'all'
  }
}, { deep: true })
</script>