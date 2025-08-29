<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="flex items-center justify-between mb-8">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
            Categorías Base
          </h1>
          <p class="mt-2 text-gray-600 dark:text-gray-400">
            Gestiona las categorías principales del sistema
          </p>
        </div>
        
        <ClientOnly>
          <UButton
            v-if="userPermissions.canManageCategoriasBase"
            color="primary"
            size="lg"
            icon="i-heroicons-plus"
            to="/admin/categorias-base/create"
          >
            Nueva Categoría
          </UButton>
          <template #fallback>
            <USkeleton class="h-10 w-36 rounded" />
          </template>
        </ClientOnly>
      </div>

      <!-- Filtros -->
      <UCard class="mb-6">
        <div class="flex flex-col sm:flex-row gap-4">
          <div class="flex-1">
            <UInput
              v-model="searchQuery"
              placeholder="Buscar categorías..."
              icon="i-heroicons-magnifying-glass"
              @input="debouncedSearch"
            />
          </div>
          
          <div class="flex gap-3">
            <USelectMenu
              v-model="visibilityFilter"
              :items="visibilityOptions"
              placeholder="Visibilidad"
              class="w-40"
            />
            
            <UButton
              variant="outline"
              icon="i-heroicons-arrow-path"
              @click="refreshData"
              :loading="loading"
            >
              Actualizar
            </UButton>
          </div>
        </div>
      </UCard>

      <!-- Tabla -->
      <ClientOnly>
        <UCard>
          <template #header>
            <div class="flex items-center justify-between">
              <h2 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
                Categorías ({{ total }})
              </h2>
            </div>
          </template>

          <!-- Loading State -->
          <div v-if="loading" class="space-y-3">
            <USkeleton class="h-12 w-full" v-for="i in 5" :key="i" />
          </div>

          <!-- Error State -->
          <div v-else-if="error" class="text-center py-8">
            <UIcon name="i-heroicons-exclamation-triangle" class="h-12 w-12 text-red-500 mx-auto mb-4" />
            <p class="text-red-600 dark:text-red-400 mb-4">{{ error }}</p>
            <UButton @click="refreshData" color="red" variant="outline">
              Reintentar
            </UButton>
          </div>

          <!-- Table -->
          <CategoriasBaseTable
            v-if="filteredCategories.length > 0"
            :categories="filteredCategories"
            :loading="loading"
            @edit="handleEdit"
            @delete="confirmDelete"
          />

          <!-- Empty State -->
          <div v-else-if="!loading && filteredCategories.length === 0" class="text-center py-12">
            <UIcon name="i-heroicons-folder" class="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-500 dark:text-gray-400 mb-4">
              {{ searchQuery ? 'No se encontraron categorías que coincidan con tu búsqueda' : 'No hay categorías creadas aún' }}
            </p>
            <ClientOnly>
              <UButton
                v-if="!searchQuery && userPermissions.canManageCategoriasBase"
                color="primary"
                to="/admin/categorias-base/create"
              >
                Crear Primera Categoría
              </UButton>
            </ClientOnly>
          </div>
        </UCard>

        <template #fallback>
          <UCard>
            <template #header>
              <USkeleton class="h-6 w-32" />
            </template>
            <div class="space-y-3">
              <USkeleton class="h-12 w-full" v-for="i in 5" :key="i" />
            </div>
          </UCard>
        </template>
      </ClientOnly>
    </div>
  </div>

  <!-- Modal de confirmación -->
  <UModal v-model:open="showDeleteModal">
    <template #header>
      <h3 class="text-lg font-semibold text-red-600">Eliminar Categoría</h3>
    </template>

    <template #body>
      <div class="space-y-4">
        <p class="text-gray-600 dark:text-gray-400">
          ¿Estás seguro de que deseas eliminar la categoría 
          <strong>{{ categoryToDelete?.nombre }}</strong>?
        </p>
        <div v-if="categoryToDelete?.cantidad_productos" class="p-3 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg">
          <div class="flex items-center gap-2">
            <UIcon name="i-heroicons-exclamation-triangle" class="h-4 w-4 text-yellow-500" />
            <span class="text-sm text-yellow-700 dark:text-yellow-300">
              Esta categoría tiene {{ categoryToDelete.cantidad_productos }} productos asociados
            </span>
          </div>
        </div>
        <p class="text-sm text-gray-500">
          Esta acción no se puede deshacer.
        </p>
      </div>
    </template>

    <template #footer>
      <div class="flex items-center justify-end gap-3">
        <UButton
          variant="ghost"
          @click="showDeleteModal = false"
        >
          Cancelar
        </UButton>
        <UButton
          color="red"
          :loading="deletingId !== null"
          @click="handleDelete"
        >
          Eliminar
        </UButton>
      </div>
    </template>
  </UModal>
</template>

<script setup lang="ts">
import type { CategoryBaseDto } from '~/types/categorias'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Categorías Base - Dulce y Salado',
  meta: [
    { name: 'description', content: 'Gestión de categorías base del sistema' }
  ]
})

// Composables
const { userPermissions } = useAuth()
const { categories, loading, error, total, fetchCategories, deleteCategory } = useCategoriesBase()

// Estado reactivo
const searchQuery = ref('')
const visibilityFilter = ref<{ value: boolean | null; label: string } | null>({ value: null, label: 'Todas' })
const showDeleteModal = ref(false)
const categoryToDelete = ref<CategoryBaseDto | null>(null)
const deletingId = ref<number | null>(null)

// Opciones para filtros
const visibilityOptions = [
  { value: null, label: 'Todas' },
  { value: true, label: 'Visibles' },
  { value: false, label: 'Ocultas' }
]


// Computed
const filteredCategories = computed(() => {
  let filtered = [...categories.value]
  
  // Filtrar por búsqueda
  if (searchQuery.value.trim()) {
    const query = searchQuery.value.toLowerCase().trim()
    filtered = filtered.filter(cat => 
      cat.nombre.toLowerCase().includes(query) ||
      cat.descripcion?.toLowerCase().includes(query)
    )
  }
  
  // Filtrar por visibilidad
  if (visibilityFilter.value !== null && visibilityFilter.value.value !== null) {
    filtered = filtered.filter(cat => cat.visible === visibilityFilter.value!.value)
  }
  
  return filtered
})

// Métodos
const refreshData = async () => {
  await fetchCategories({
    visibleOnly: visibilityFilter.value?.value || undefined
  })
}

const handleEdit = (category: CategoryBaseDto) => {
  navigateTo(`/admin/categorias-base/${category.id}/edit`)
}

const confirmDelete = (category: CategoryBaseDto) => {
  categoryToDelete.value = category
  showDeleteModal.value = true
}

const handleDelete = async () => {
  if (!categoryToDelete.value) return
  
  deletingId.value = categoryToDelete.value.id
  try {
    await deleteCategory(categoryToDelete.value.id)
    showDeleteModal.value = false
    categoryToDelete.value = null
  } catch (error) {
    // Error ya manejado en el composable
  } finally {
    deletingId.value = null
  }
}

// Búsqueda con debounce
const debouncedSearch = () => {
  // La búsqueda se hace en el computed reactivo
}

// Cargar datos al montar
onMounted(async () => {
  await refreshData()
})

// Limpiar estado al desmontar
onUnmounted(() => {
  // No necesitamos limpiar nada específico
})
</script>