<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center gap-3 mb-4">
          <UButton
            variant="ghost"
            color="gray"
            icon="i-heroicons-arrow-left"
            to="/admin/categorias-base"
          >
            Volver
          </UButton>
          <div class="h-6 w-px bg-gray-300 dark:bg-gray-600"></div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
            Editar Categoría Base
          </h1>
        </div>
        <p class="text-gray-600 dark:text-gray-400">
          Modifica la información de la categoría
        </p>
      </div>

      <!-- Loading state -->
      <div v-if="loadingCategory" class="space-y-6">
        <UCard>
          <div class="space-y-4">
            <USkeleton class="h-4 w-32" />
            <USkeleton class="h-10 w-full" />
            <USkeleton class="h-4 w-32" />
            <USkeleton class="h-20 w-full" />
            <USkeleton class="h-4 w-32" />
            <USkeleton class="h-10 w-full" />
          </div>
        </UCard>
      </div>

      <!-- Error state -->
      <div v-else-if="error" class="text-center py-8">
        <UIcon name="i-heroicons-exclamation-triangle" class="h-12 w-12 text-red-500 mx-auto mb-4" />
        <p class="text-red-600 dark:text-red-400 mb-4">{{ error }}</p>
        <UButton @click="loadCategory" color="red" variant="outline">
          Reintentar
        </UButton>
      </div>

      <!-- Formulario -->
      <CategoryBaseForm
        v-else-if="category"
        mode="edit"
        :initial-data="category"
        :loading="loading"
        @submit="handleSubmit"
        @cancel="handleCancel"
      />

      <!-- Not found -->
      <div v-else class="text-center py-8">
        <UIcon name="i-heroicons-folder-x" class="h-12 w-12 text-gray-400 mx-auto mb-4" />
        <p class="text-gray-500 dark:text-gray-400 mb-4">
          No se encontró la categoría solicitada
        </p>
        <UButton to="/admin/categorias-base" variant="outline">
          Volver a Categorías
        </UButton>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { CategoryBaseDto, UpdateCategoryBaseCommand } from '~/types/categorias'
import CategoryBaseForm from '~/components/categorias-base/CategoryBaseForm.vue'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Editar Categoría Base - Dulce y Salado',
  meta: [
    { name: 'description', content: 'Editar categoría base del sistema' }
  ]
})

// Composables
const { fetchCategoryById, updateCategory } = useCategoriesBase()
const route = useRoute()
const router = useRouter()

// Estado reactivo
const category = ref<CategoryBaseDto | null>(null)
const loading = ref(false)
const loadingCategory = ref(true)
const error = ref<string | null>(null)

// Obtener el ID de la ruta
const categoryId = computed(() => {
  const id = Array.isArray(route.params.id) ? route.params.id[0] : route.params.id
  return parseInt(id)
})

// Métodos
const loadCategory = async () => {
  loadingCategory.value = true
  error.value = null
  
  try {
    // Obtener la categoría directamente de la API
    const fetchedCategory = await fetchCategoryById(categoryId.value)
    category.value = fetchedCategory
  } catch (err: any) {
    error.value = err.message || 'Error al cargar la categoría'
  } finally {
    loadingCategory.value = false
  }
}

const handleSubmit = async (data: UpdateCategoryBaseCommand) => {
  loading.value = true
  try {
    await updateCategory(categoryId.value, data)
    await router.push('/admin/categorias-base')
  } catch (error) {
    // Error ya manejado en el composable
  } finally {
    loading.value = false
  }
}

const handleCancel = () => {
  router.push('/admin/categorias-base')
}

// Cargar categoría al montar
onMounted(async () => {
  await loadCategory()
})

// Watch para cambios en el ID
watch(() => categoryId.value, async (newId) => {
  if (newId) {
    await loadCategory()
  }
})
</script>