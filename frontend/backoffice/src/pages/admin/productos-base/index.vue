<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="flex justify-between items-center mb-8">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
            Gestión de Productos Base
          </h1>
          <ClientOnly>
            <p class="mt-2 text-gray-600 dark:text-gray-400">
              Administra el catálogo base de productos
            </p>
            <template #fallback>
              <USkeleton class="h-4 w-64 mt-2" />
            </template>
          </ClientOnly>
        </div>
        
        <ClientOnly>
          <UButton
            v-if="userPermissions.canManageProductosBase"
            to="/admin/productos-base/create"
            color="primary"
            size="lg"
          >
            <UIcon name="i-heroicons-plus" class="mr-2" />
            Nuevo Producto
          </UButton>
          <template #fallback>
            <USkeleton class="h-11 w-36 rounded" />
          </template>
        </ClientOnly>
      </div>


      <!-- Filtros -->
      <UCard class="mb-6">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
          <!-- Lista de Precios -->
          <ListaPreciosSelector
            v-model="listaSeleccionada"
            :listas="listasDisponibles"
            @change="handleListaChange"
          />

          <!-- Búsqueda -->
          <UFormField label="Buscar">
            <UInput
              v-model="searchQuery"
              placeholder="Código, descripción..."
              icon="i-heroicons-magnifying-glass"
              @input="debouncedSearch"
            />
          </UFormField>

          <!-- Filtro por rubro -->
          <UFormField label="Rubro">
            <UInput
              v-model="rubroFilter"
              type="number"
              placeholder="Código de rubro"
              @input="handleRubroInput"
            />
          </UFormField>

          <!-- Filtro visible -->
          <UFormField label="Visibilidad">
            <USelectMenu
              v-model="visibleFilter"
              :items="visibilityOptions"
              value-key="value"
              placeholder="Todos"
              @change="applyFilters"
            />
          </UFormField>

          <!-- Filtros con checkboxes -->
          <div class="space-y-2">
            <UFormField>
              <UCheckbox
                v-model="soloSinConfiguracion"
                label="Solo productos sin configuración"
                @change="applyFilters"
              />
            </UFormField>
            <UFormField>
              <UCheckbox
                v-model="incluirSinExistencia"
                label="Incluir productos sin stock"
                @change="applyFilters"
              />
            </UFormField>
          </div>
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
            {{ pagination.total }} producto{{ pagination.total !== 1 ? 's' : '' }} encontrado{{ pagination.total !== 1 ? 's' : '' }}
          </div>
        </div>
      </UCard>

      <!-- Tabla de productos -->
      <UCard>
        <div v-if="loading || initialLoading" class="space-y-4">
          <!-- Header skeleton -->
          <div class="flex justify-between items-center p-4 border-b border-gray-200 dark:border-gray-700">
            <div class="flex space-x-4">
              <USkeleton class="h-4 w-16" />
              <USkeleton class="h-4 w-20" />
              <USkeleton class="h-4 w-32" />
              <USkeleton class="h-4 w-20" />
              <USkeleton class="h-4 w-20" />
              <USkeleton class="h-4 w-16" />
              <USkeleton class="h-4 w-20" />
              <USkeleton class="h-4 w-24" />
            </div>
          </div>
          
          <!-- Rows skeleton -->
          <div class="space-y-3 p-4">
            <div v-for="n in 5" :key="n" class="flex justify-between items-center py-3 border-b border-gray-100 dark:border-gray-800">
              <div class="flex items-center space-x-4 flex-1">
                <!-- Imagen -->
                <USkeleton class="h-12 w-12 rounded" />
                
                <!-- Código -->
                <div class="w-20">
                  <USkeleton class="h-4 w-16" />
                </div>
                
                <!-- Descripción -->
                <div class="flex-1">
                  <USkeleton class="h-4 w-48 mb-1" />
                  <USkeleton class="h-3 w-32" />
                </div>
                
                <!-- Precio -->
                <div class="w-24">
                  <USkeleton class="h-4 w-20" />
                </div>
                
                <!-- Stock -->
                <div class="w-20">
                  <USkeleton class="h-4 w-16" />
                </div>
                
                <!-- Visible -->
                <div class="w-16">
                  <USkeleton class="h-6 w-14 rounded-full" />
                </div>
                
                <!-- Destacado -->
                <div class="w-20">
                  <USkeleton class="h-6 w-18 rounded-full" />
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
            @click="fetchProductos()"
          >
            Reintentar
          </UButton>
        </div>

        <div v-else-if="productos.length === 0 && !initialLoading" class="text-center py-8">
          <UIcon name="i-heroicons-cube" class="h-12 w-12 text-gray-400 mx-auto mb-2" />
          <p class="text-gray-600 dark:text-gray-400">No se encontraron productos</p>
          <ClientOnly>
            <UButton
              v-if="userPermissions.canManageProductosBase"
              to="/admin/productos-base/create"
              color="primary"
              variant="ghost"
              class="mt-2"
            >
              Crear el primer producto
            </UButton>
            <template #fallback>
              <USkeleton class="h-9 w-40 mx-auto mt-2 rounded" />
            </template>
          </ClientOnly>
        </div>

        <ClientOnly>
          <ProductosBaseTable
            v-if="!loading && !initialLoading && !error && productos.length > 0"
            :productos="productos"
            :loading="false"
            :sort-by="filters.sortBy"
            :sort-order="filters.sortOrder"
            :lista-seleccionada="listaSeleccionada"
            :read-only="!userPermissions.canManageProductosBase"
            @edit="editProducto"
            @delete="deleteProducto"
            @sort="handleSort"
            @edit-image="editProductoImage"
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
        ¿Estás seguro de que deseas eliminar el producto 
        <strong class="text-gray-900 dark:text-gray-100">{{ productoToDelete?.descripcion }}</strong> 
        (Código: {{ productoToDelete?.codigo }})?
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
          Eliminar Producto
        </UButton>
      </div>
    </template>
  </UModal>

  <!-- Modal de edición de imagen -->
  <ImageEditModal
    v-model="showImageEditModal"
    :producto="productoToEditImage"
    product-type="base"
    @image-updated="handleImageUpdated"
  />
</template>

<script setup lang="ts">
import ListaPreciosSelector from '~/components/ui/ListaPreciosSelector.vue'
import ImageEditModal from '~/components/ui/ImageEditModal.vue'
import type { ProductoBaseDto } from '~/types/productos'

// Configuración de página
definePageMeta({
  middleware: ['auth'],
  layout: 'default'
})

useHead({
  title: 'Gestión de Productos Base',
  meta: [
    { name: 'description', content: 'Administra el catálogo base de productos' }
  ]
})

// Composables
const { 
  productos: productosReadonly, 
  loading, 
  error, 
  pagination, 
  filters,
  listaSeleccionada,
  listasDisponibles: listasDisponiblesReadonly,
  fetchProductos, 
  deleteProducto: deleteProductoAction, 
  changePage: changePageAction, 
  applyFilters: applyFiltersAction, 
  clearFilters,
  applySorting,
  changeListaPrecios
} = useProductosBase()
const { userPermissions } = useAuth()
const { init: initListasPrecios } = useListasPrecios()

// Convertir productos readonly a array mutable para el componente
const productos = computed(() => [...productosReadonly.value])
const listasDisponibles = computed(() => {
  return [...listasDisponiblesReadonly.value]
})

// Estado reactivo
const searchQuery = ref('')
const rubroFilter = ref('')
const visibleFilter = ref<boolean | undefined | 'all'>('all')
const destacadoFilter = ref<boolean | undefined | 'all'>('all')
const soloSinConfiguracion = ref(false)
const incluirSinExistencia = ref(false)
const currentPage = ref(1)
const initialLoading = ref(true)


// Modales
const showDeleteModal = ref(false)
const productoToDelete = ref<ProductoBaseDto | null>(null)
const showImageEditModal = ref(false)
const productoToEditImage = ref<ProductoBaseDto | null>(null)

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

// Búsqueda debounced
let debounceTimer: NodeJS.Timeout
const debouncedSearch = () => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    applyFilters()
  }, 300)
}

// Métodos
const handleRubroInput = () => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    applyFilters()
  }, 300)
}

const applyFilters = async () => {
  await applyFiltersAction({
    busqueda: searchQuery.value,
    codigoRubro: rubroFilter.value ? parseInt(rubroFilter.value) : undefined,
    visible: visibleFilter.value !== 'all' ? visibleFilter.value as boolean : undefined,
    destacado: destacadoFilter.value !== 'all' ? destacadoFilter.value as boolean : undefined,
    soloSinConfiguracion: soloSinConfiguracion.value || undefined,
    incluirSinExistencia: incluirSinExistencia.value || undefined,
    page: 1
  })
  currentPage.value = 1
}

const clearAllFilters = async () => {
  searchQuery.value = ''
  rubroFilter.value = ''
  visibleFilter.value = 'all'
  destacadoFilter.value = 'all'
  soloSinConfiguracion.value = false
  incluirSinExistencia.value = false
  currentPage.value = 1
  await clearFilters()
}

const changePage = async (page: number) => {
  currentPage.value = page
  await changePageAction(page)
}

const editProducto = (producto: ProductoBaseDto) => {
  navigateTo(`/admin/productos-base/${producto.id}/edit`)
}

const editProductoImage = (producto: ProductoBaseDto) => {
  productoToEditImage.value = producto
  showImageEditModal.value = true
}

const handleImageUpdated = async (productId: number, imageUrl: string | null, imageAlt: string | null) => {
  // Refrescar la lista de productos para mostrar los cambios
  await fetchProductos()
  
  // Cerrar modal
  showImageEditModal.value = false
  productoToEditImage.value = null
}

const deleteProducto = (producto: ProductoBaseDto) => {
  productoToDelete.value = producto
  showDeleteModal.value = true
}

const confirmDelete = async () => {
  if (productoToDelete.value) {
    try {
      await deleteProductoAction(productoToDelete.value.id)
      showDeleteModal.value = false
      productoToDelete.value = null
    } catch (error) {
      console.error('Error en confirmDelete:', error)
      // Error ya manejado en el composable
    }
  } else {
    console.log('No hay producto para eliminar')
  }
}

const handleSort = async (column: string, direction: 'asc' | 'desc') => {
  await applySorting(column, direction)
}

const handleListaChange = async (lista: any) => {
  if (lista) {
    await changeListaPrecios(lista.id)
  }
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
    // Inicializar listas de precios primero
    await initListasPrecios()
    
    // Cargar productos
    await fetchProductos()
  } finally {
    initialLoading.value = false
  }
})
</script>