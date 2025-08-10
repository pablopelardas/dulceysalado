<script setup lang="ts">
import ListasPreciosTable from '~/components/listas-precios/ListasPreciosTable.vue'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Listas de Precios',
  meta: [
    { name: 'description', content: 'Gestión de listas de precios' }
  ]
})

// Composables
const router = useRouter()
const { isEmpresaPrincipal } = useAuth()
const { 
  listas, 
  loading, 
  fetchListas, 
  deleteLista 
} = useListasPreciosCrud()

// Estado reactivo
const searchQuery = ref('')
const sortBy = ref('codigo')
const sortOrder = ref<'asc' | 'desc'>('asc')
const showDeleteModal = ref(false)
const listaToDelete = ref<any>(null)

// Computed
const filteredListas = computed(() => {
  let filtered = [...listas.value]

  // Filtrar por búsqueda
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(lista => 
      lista.nombre.toLowerCase().includes(query) ||
      lista.codigo.toLowerCase().includes(query) ||
      (lista.descripcion && lista.descripcion.toLowerCase().includes(query))
    )
  }


  // Ordenar
  filtered.sort((a, b) => {
    let aVal = a[sortBy.value as keyof typeof a]
    let bVal = b[sortBy.value as keyof typeof b]
    
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

// Métodos
const handleCreate = () => {
  router.push('/admin/listas-precios/new')
}

const handleEdit = (lista: any) => {
  router.push(`/admin/listas-precios/${lista.id}/edit`)
}

const handleDeleteClick = (lista: any) => {
  listaToDelete.value = lista
  showDeleteModal.value = true
}

const handleConfirmDelete = async () => {
  if (!listaToDelete.value) return

  try {
    await deleteLista(listaToDelete.value.id)
    showDeleteModal.value = false
    listaToDelete.value = null
    await fetchListas()
  } catch (error) {
    console.error('Error al eliminar lista:', error)
  }
}

const handleSort = (column: string, order: 'asc' | 'desc') => {
  sortBy.value = column
  sortOrder.value = order
}

// Cargar datos al montar
onMounted(async () => {
  if (!isEmpresaPrincipal.value) {
    router.replace('/')
    return
  }
  
  await fetchListas()
})
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Dashboard</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Listas de Precios</span>
        </nav>
        
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
              Listas de Precios
            </h1>
            <p class="mt-2 text-gray-600 dark:text-gray-400">
              Gestiona las listas de precios del sistema
            </p>
          </div>
          
          <UButton
            color="primary"
            size="lg"
            @click="handleCreate"
          >
            <UIcon name="i-heroicons-plus" class="mr-2" />
            Nueva Lista
          </UButton>
        </div>
      </div>

      <!-- Filtros -->
      <div class="mb-6">
        <UInput
          v-model="searchQuery"
          placeholder="Buscar por nombre, código o descripción..."
          icon="i-heroicons-magnifying-glass"
          size="lg"
          class="max-w-md"
        />
      </div>

      <!-- Tabla -->
      <UCard>
        <ListasPreciosTable
          :listas="filteredListas"
          :loading="loading"
          :sort-by="sortBy"
          :sort-order="sortOrder"
          @edit="handleEdit"
          @delete="handleDeleteClick"
          @sort="handleSort"
        />
      </UCard>

      <!-- Modal de confirmación de eliminación -->
      <UModal v-model:open="showDeleteModal">
          <template #header>
            <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
              Confirmar eliminación
            </h3>
          </template>
          <template #body>
            <p class="text-gray-600 dark:text-gray-400">
              ¿Estás seguro de que deseas eliminar la lista de precios 
              <strong class="text-gray-900 dark:text-gray-100">{{ listaToDelete?.nombre }}</strong>?
            </p>
            
            <p class="mt-2 text-sm text-red-600 dark:text-red-400">
              Esta acción no se puede deshacer.
            </p>
          </template>

          <template #footer>
            <div class="flex justify-end gap-3">
              <UButton
                color="gray"
                variant="ghost"
                @click="showDeleteModal = false"
              >
                Cancelar
              </UButton>
              <UButton
                color="red"
                @click="handleConfirmDelete"
              >
                Eliminar
              </UButton>
            </div>
          </template>
      </UModal>
    </div>
  </div>
</template>