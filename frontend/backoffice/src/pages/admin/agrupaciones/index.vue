<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Dashboard</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Agrupaciones</span>
        </nav>
        
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
              Gestión de Agrupaciones
            </h1>
            <p class="mt-2 text-gray-600 dark:text-gray-400">
              Administra las agrupaciones de productos sincronizadas desde GECOM
            </p>
          </div>
          
          <!-- Actions -->
          <div class="flex items-center gap-3">
            <UButton
              color="orange"
              variant="outline"
              to="/admin/agrupaciones/empresas"
            >
              <UIcon name="i-heroicons-cog-6-tooth" class="mr-2" />
              Configurar por Empresa
            </UButton>
            <UBadge color="blue" variant="soft" size="lg">
              <UIcon name="i-heroicons-information-circle" class="mr-1" />
              Auto-sincronizado
            </UBadge>
          </div>
        </div>
      </div>

      <!-- Filtros -->
      <div class="mb-6">
        <div class="flex flex-col sm:flex-row gap-4">
          <!-- Búsqueda -->
          <UInput
            v-model="searchQuery"
            placeholder="Buscar por nombre, código o descripción..."
            icon="i-heroicons-magnifying-glass"
            size="lg"
            class="flex-1 max-w-md"
          />
          
          <!-- Filtro por estado -->
          <USelectMenu
            v-model="statusFilter"
            :items="statusOptions"
            value-key="value"
            placeholder="Todos los estados"
            size="lg"
            class="w-48"
            @change="applyFilters"
          />
          
          <!-- Botón de refrescar -->
          <UButton
            variant="ghost"
            size="lg"
            icon="i-heroicons-arrow-path"
            :loading="loading"
            @click="refresh"
          >
            Actualizar
          </UButton>
        </div>
      </div>

      <!-- Estadísticas rápidas -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <UCard>
          <div class="flex items-center gap-3">
            <div class="p-2 bg-blue-100 dark:bg-blue-900/20 rounded-lg">
              <UIcon name="i-heroicons-squares-2x2" class="h-6 w-6 text-blue-600 dark:text-blue-400" />
            </div>
            <div>
              <p class="text-sm text-gray-500 dark:text-gray-400">Total</p>
              <p class="text-xl font-bold text-gray-900 dark:text-gray-100">{{ total }}</p>
            </div>
          </div>
        </UCard>
        
        <UCard>
          <div class="flex items-center gap-3">
            <div class="p-2 bg-green-100 dark:bg-green-900/20 rounded-lg">
              <UIcon name="i-heroicons-check-circle" class="h-6 w-6 text-green-600 dark:text-green-400" />
            </div>
            <div>
              <p class="text-sm text-gray-500 dark:text-gray-400">Activas</p>
              <p class="text-xl font-bold text-gray-900 dark:text-gray-100">{{ activasCount }}</p>
            </div>
          </div>
        </UCard>
        
        <UCard>
          <div class="flex items-center gap-3">
            <div class="p-2 bg-red-100 dark:bg-red-900/20 rounded-lg">
              <UIcon name="i-heroicons-x-circle" class="h-6 w-6 text-red-600 dark:text-red-400" />
            </div>
            <div>
              <p class="text-sm text-gray-500 dark:text-gray-400">Inactivas</p>
              <p class="text-xl font-bold text-gray-900 dark:text-gray-100">{{ inactivasCount }}</p>
            </div>
          </div>
        </UCard>
      </div>

      <!-- Tabla -->
      <UCard>
        <div v-if="loading && !agrupaciones.length" class="space-y-4 p-6">
          <!-- Loading skeleton -->
          <div class="flex justify-between items-center">
            <USkeleton class="h-8 w-40" />
            <USkeleton class="h-8 w-20" />
          </div>
          <div class="space-y-3">
            <div v-for="n in 5" :key="n" class="flex justify-between items-center">
              <div class="flex items-center gap-4 flex-1">
                <USkeleton class="h-6 w-16" />
                <USkeleton class="h-6 w-48" />
                <USkeleton class="h-6 w-20" />
                <USkeleton class="h-6 w-32" />
              </div>
              <USkeleton class="h-8 w-16" />
            </div>
          </div>
        </div>

        <div v-else-if="error && !agrupaciones.length" class="text-center py-8">
          <UIcon name="i-heroicons-exclamation-triangle" class="h-12 w-12 text-red-500 mx-auto mb-2" />
          <p class="text-red-600 dark:text-red-400">{{ error }}</p>
          <UButton
            color="red"
            variant="ghost"
            class="mt-2"
            @click="refresh"
          >
            Reintentar
          </UButton>
        </div>

        <div v-else-if="filteredAgrupaciones.length === 0" class="text-center py-8">
          <UIcon name="i-heroicons-squares-2x2" class="h-12 w-12 text-gray-400 mx-auto mb-2" />
          <p class="text-gray-600 dark:text-gray-400">
            {{ searchQuery || statusFilter !== 'all' ? 'No se encontraron agrupaciones con los filtros aplicados' : 'No hay agrupaciones disponibles' }}
          </p>
          <UButton
            v-if="searchQuery || statusFilter !== 'all'"
            variant="ghost"
            class="mt-2"
            @click="clearFilters"
          >
            Limpiar filtros
          </UButton>
        </div>

        <AgrupacionesTable
          v-else
          :agrupaciones="filteredAgrupaciones"
          :loading="false"
          :sort-by="sortBy"
          :sort-order="sortOrder"
          @edit="handleEdit"
          @sort="handleSort"
        />
      </UCard>
    </div>
  </div>
</template>

<script setup lang="ts">
import AgrupacionesTable from '~/components/agrupaciones/AgrupacionesTable.vue'
import type { AgrupacionDto } from '~/types/agrupaciones'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Gestión de Agrupaciones',
  meta: [
    { name: 'description', content: 'Gestión de agrupaciones de productos' }
  ]
})

// Composables
const router = useRouter()
const { isEmpresaPrincipal } = useAuth()
const { 
  agrupaciones, 
  loading, 
  error,
  total,
  fetchAgrupaciones
} = useAgrupacionesCrud()

// Estado reactivo
const searchQuery = ref('')
const statusFilter = ref<'all' | boolean>('all')
const sortBy = ref('codigo')
const sortOrder = ref<'asc' | 'desc'>('asc')

// Opciones para el filtro de estado
const statusOptions = [
  { label: 'Todos los estados', value: 'all' },
  { label: 'Solo activas', value: true },
  { label: 'Solo inactivas', value: false }
]

// Computed
const filteredAgrupaciones = computed(() => {
  let filtered = [...agrupaciones.value]

  // Filtrar por búsqueda
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(agrupacion => 
      agrupacion.nombre.toLowerCase().includes(query) ||
      agrupacion.codigo.toString().includes(query) ||
      (agrupacion.descripcion && agrupacion.descripcion.toLowerCase().includes(query))
    )
  }

  // Filtrar por estado
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter(agrupacion => agrupacion.activa === statusFilter.value)
  }

  // Ordenar
  filtered.sort((a, b) => {
    let aVal = a[sortBy.value as keyof AgrupacionDto]
    let bVal = b[sortBy.value as keyof AgrupacionDto]
    
    if (aVal === null || aVal === undefined) return 1
    if (bVal === null || bVal === undefined) return -1
    
    if (typeof aVal === 'string') {
      aVal = aVal.toLowerCase()
      bVal = (bVal as string).toLowerCase()
    }
    
    if (sortOrder.value === 'asc') {
      return aVal > bVal ? 1 : -1
    } else {
      return aVal < bVal ? 1 : -1
    }
  })

  return filtered
})

const activasCount = computed(() => {
  return agrupaciones.value.filter(a => a.activa).length
})

const inactivasCount = computed(() => {
  return agrupaciones.value.filter(a => !a.activa).length
})

// Métodos
const handleEdit = (agrupacion: AgrupacionDto) => {
  router.push(`/admin/agrupaciones/${agrupacion.id}/edit`)
}

const handleSort = (column: string, order: 'asc' | 'desc') => {
  sortBy.value = column
  sortOrder.value = order
}

const applyFilters = () => {
  // Los filtros se aplican automáticamente via computed
}

const clearFilters = () => {
  searchQuery.value = ''
  statusFilter.value = 'all'
}

const refresh = async () => {
  await fetchAgrupaciones({
    ordenarPor: 'codigo',
    ordenDireccion: 'asc'
  })
}

// Cargar datos al montar
onMounted(async () => {
  if (!isEmpresaPrincipal.value) {
    router.replace('/')
    return
  }
  
  await refresh()
})
</script>