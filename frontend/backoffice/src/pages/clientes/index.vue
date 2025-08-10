<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="flex justify-between items-center mb-8">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
            Gestión de Clientes
          </h1>
          <ClientOnly>
            <p class="mt-2 text-gray-600 dark:text-gray-400">
              Administra los clientes y sus credenciales de acceso
            </p>
            <template #fallback>
              <USkeleton class="h-4 w-64 mt-2" />
            </template>
          </ClientOnly>
        </div>
        
        <ClientOnly>
          <UButton
            to="/clientes/create"
            color="primary"
            size="lg"
          >
            <UIcon name="i-heroicons-plus" class="mr-2" />
            Nuevo Cliente
          </UButton>
          <template #fallback>
            <USkeleton class="h-11 w-36 rounded" />
          </template>
        </ClientOnly>
      </div>

      <!-- Filtros -->
      <UCard class="mb-6">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <!-- Búsqueda -->
          <UFormField label="Buscar">
            <UInput
              v-model="searchQuery"
              placeholder="Nombre, email, código..."
              icon="i-heroicons-magnifying-glass"
              @input="debouncedSearch"
            />
          </UFormField>

          <!-- Lista de Precios -->
          <UFormField label="Lista de Precios">
            <USelectMenu
              v-model="listaPrecioFilter"
              :items="listasDisponiblesOptions"
              value-key="value"
              placeholder="Todas las listas"
              @change="applyFilters"
            />
          </UFormField>

          <!-- Incluir Eliminados -->
          <UFormField label="Mostrar Inactivos">
            <UCheckbox
              v-model="includeDeletedFilter"
              label="Incluir clientes inactivos"
              @change="applyFilters"
            />
            <template #help>
              <span class="text-sm text-gray-500 dark:text-gray-400">
                Mostrar clientes marcados como inactivos
              </span>
            </template>
          </UFormField>
        </div>

        <!-- Botones de acción -->
        <div class="flex justify-between items-center mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
          <UButton
            variant="ghost"
            class="cursor-pointer"
            color="blue"
            @click="clearAllFilters"
          >
            Limpiar Filtros
          </UButton>
          
          <div class="text-sm text-gray-500 dark:text-gray-400">
            {{ pagination.total }} cliente{{ pagination.total !== 1 ? 's' : '' }} encontrado{{ pagination.total !== 1 ? 's' : '' }}
          </div>
        </div>
      </UCard>

      <!-- Tabla de clientes -->
      <UCard>
        <div v-if="loading || initialLoading" class="space-y-4">
          <!-- Header skeleton -->
          <div class="flex justify-between items-center p-4 border-b border-gray-200 dark:border-gray-700">
            <div class="flex space-x-4">
              <USkeleton class="h-4 w-16" />
              <USkeleton class="h-4 w-20" />
              <USkeleton class="h-4 w-32" />
              <USkeleton class="h-4 w-24" />
              <USkeleton class="h-4 w-20" />
              <USkeleton class="h-4 w-16" />
              <USkeleton class="h-4 w-24" />
            </div>
          </div>
          
          <!-- Rows skeleton -->
          <div class="space-y-3 p-4">
            <div v-for="n in 5" :key="n" class="flex justify-between items-center py-3 border-b border-gray-100 dark:border-gray-800">
              <div class="flex items-center space-x-4 flex-1">
                <!-- Código -->
                <div class="w-20">
                  <USkeleton class="h-4 w-16" />
                </div>
                
                <!-- Nombre -->
                <div class="flex-1">
                  <USkeleton class="h-4 w-48 mb-1" />
                  <USkeleton class="h-3 w-32" />
                </div>
                
                <!-- Email -->
                <div class="w-48">
                  <USkeleton class="h-4 w-40" />
                </div>
                
                <!-- Lista Precio -->
                <div class="w-32">
                  <USkeleton class="h-4 w-28" />
                </div>
                
                <!-- Credenciales -->
                <div class="w-20">
                  <USkeleton class="h-6 w-16 rounded-full" />
                </div>
                
                <!-- Estado -->
                <div class="w-16">
                  <USkeleton class="h-6 w-14 rounded-full" />
                </div>
                
                <!-- Acciones -->
                <div class="w-24">
                  <div class="flex space-x-1">
                    <USkeleton class="h-8 w-8 rounded" />
                    <USkeleton class="h-8 w-8 rounded" />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div v-else-if="error && !initialLoading" class="text-center py-8">
          <UIcon name="i-heroicons-exclamation-triangle" class="h-12 w-12 text-red-500 mx-auto mb-2" />
          <p class="text-red-600 dark:text-red-400">{{ error }}</p>
          <UButton
            color="red"
            variant="ghost"
            class="mt-2"
            @click="fetchClientes()"
          >
            Reintentar
          </UButton>
        </div>

        <div v-else-if="clientes.length === 0 && !initialLoading" class="text-center py-8">
          <UIcon name="i-heroicons-user-group" class="h-12 w-12 text-gray-400 mx-auto mb-2" />
          <p class="text-gray-600 dark:text-gray-400">No se encontraron clientes</p>
          <ClientOnly>
            <UButton
              to="/clientes/create"
              color="primary"
              variant="ghost"
              class="mt-2"
            >
              Crear el primer cliente
            </UButton>
            <template #fallback>
              <USkeleton class="h-9 w-40 mx-auto mt-2 rounded" />
            </template>
          </ClientOnly>
        </div>

        <ClientOnly>
          <ClientesTable
            v-if="!loading && !initialLoading && !error && clientes.length > 0"
            :clientes="clientes"
            :loading="false"
            :sort-by="filters.sortBy"
            :sort-order="filters.sortOrder"
            @edit="editCliente"
            @delete="deleteCliente"
            @sort="handleSort"
            @manage-credentials="manageCredentials"
          />
        </ClientOnly>

        <!-- Paginación -->
        <div v-if="pagination.total > pagination.limit" class="flex justify-center mt-6">
          <UPagination
            v-model:page="currentPage"
            :total="Number(pagination.total)"
            :items-per-page="Number(pagination.limit)"
            :sibling-count="2"
            :size="'sm'"
            show-edges
            @update:page="changePage"
          />
        </div>
      </UCard>
    </div>
  </div>

  <!-- Modal de confirmación de eliminación -->
  <UModal v-model:open="showDeleteModal">
    <template #header>
      <div class="flex items-center">
        <UIcon name="i-heroicons-exclamation-triangle" class="h-6 w-6 text-red-500 mr-2" />
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">Confirmar eliminación</h3>
      </div>
    </template>
    <template #body>
      <p class="text-gray-600 dark:text-gray-400">
        ¿Estás seguro de que deseas eliminar el cliente 
        <strong class="text-gray-900 dark:text-gray-100">{{ clienteToDelete?.nombre }}</strong> 
        (Código: {{ clienteToDelete?.codigo }})?
        Esta acción no se puede deshacer.
      </p>
    </template>
    <template #footer>
      <div class="flex justify-end space-x-3">
        <UButton
          variant="ghost"
          color="gray"
          @click="showDeleteModal = false"
        >
          Cancelar
        </UButton>
        <UButton
          color="red"
          :loading="loading"
          @click="confirmDelete"
        >
          Eliminar Cliente
        </UButton>
      </div>
    </template>
  </UModal>
</template>

<script setup lang="ts">
import type { ClienteDto } from '~/types/clientes'
import ClientesTable from '~/components/clientes/ClientesTable.vue'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'feature-flag'],
  layout: 'default',
  featureFlag: 'cliente_autenticacion'
})

useHead({
  title: 'Gestión de Clientes',
  meta: [
    { name: 'description', content: 'Administra los clientes y sus credenciales de acceso' }
  ]
})

// Composables
const { 
  clientes: clientesReadonly, 
  loading, 
  error, 
  pagination, 
  filters,
  listasDisponibles: listasDisponiblesReadonly,
  fetchClientes, 
  deleteCliente: deleteClienteAction, 
  changePage: changePageAction, 
  applyFilters: applyFiltersAction, 
  clearFilters,
  applySorting
} = useClientes()

const { listas: listasGenerales, init: initListasPrecios } = useListasPrecios()

// Convertir clientes readonly a array mutable para el componente
const clientes = computed(() => [...clientesReadonly.value])
const listasDisponibles = computed(() => {
  return [...listasDisponiblesReadonly.value]
})

// Opciones para listas de precios - usar listas generales primero, luego las específicas del cliente
const listasDisponiblesOptions = computed(() => {
  const listasPrimarias = listasGenerales.value.length > 0 ? listasGenerales.value : listasDisponibles.value
  return [
    { label: 'Todas las listas', value: null },
    ...listasPrimarias.map(lista => ({
      label: lista.nombre || `Lista ${lista.codigo || lista.id}`,
      value: lista.id
    }))
  ]
})

// Estado reactivo
const searchQuery = ref('')
const listaPrecioFilter = ref<number | null>(null)
const includeDeletedFilter = ref(false)
const currentPage = ref(1)
const initialLoading = ref(true)

// Modales
const showDeleteModal = ref(false)
const clienteToDelete = ref<ClienteDto | null>(null)

// Búsqueda debounced
let debounceTimer: NodeJS.Timeout
const debouncedSearch = () => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    applyFilters()
  }, 300)
}

// Métodos
const applyFilters = async () => {
  await applyFiltersAction({
    search: searchQuery.value,
    lista_precio_id: listaPrecioFilter.value,
    include_deleted: includeDeletedFilter.value,
    page: 1
  })
  currentPage.value = 1
}

const clearAllFilters = async () => {
  searchQuery.value = ''
  listaPrecioFilter.value = null
  includeDeletedFilter.value = false
  currentPage.value = 1
  await clearFilters()
}

const changePage = async (page: number) => {
  currentPage.value = page
  await changePageAction(page)
}

const editCliente = (cliente: ClienteDto) => {
  navigateTo(`/clientes/${cliente.id}/edit`)
}

const manageCredentials = (cliente: ClienteDto) => {
  navigateTo(`/clientes/${cliente.id}/credentials`)
}

const deleteCliente = (cliente: ClienteDto) => {
  clienteToDelete.value = cliente
  showDeleteModal.value = true
}

const confirmDelete = async () => {
  if (clienteToDelete.value) {
    try {
      await deleteClienteAction(clienteToDelete.value.id)
      showDeleteModal.value = false
      clienteToDelete.value = null
    } catch (error) {
      console.error('Error en confirmDelete:', error)
      // Error ya manejado en el composable
    }
  }
}

const handleSort = async (column: string, direction: 'asc' | 'desc') => {
  await applySorting(column, direction)
}

// Watcher para sincronizar currentPage con pagination
watch(() => pagination.value.page, (newPage) => {
  if (newPage !== currentPage.value) {
    currentPage.value = newPage
  }
})

// Cargar datos iniciales
onMounted(async () => {
  try {
    // Cargar listas de precios y clientes en paralelo
    await Promise.all([
      initListasPrecios(),
      fetchClientes()
    ])
  } finally {
    initialLoading.value = false
  }
})
</script>