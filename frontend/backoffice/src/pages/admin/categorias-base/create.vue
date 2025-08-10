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
            Nueva Categoría Base
          </h1>
        </div>
        <p class="text-gray-600 dark:text-gray-400">
          Crea una nueva categoría para organizar los productos del sistema
        </p>
      </div>

      <!-- Formulario -->
      <CategoryBaseForm
        mode="create"
        :loading="loading"
        @submit="handleSubmit"
        @cancel="handleCancel"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import CategoryBaseForm from '../../../components/categorias-base/CategoryBaseForm.vue'
import type { CreateCategoryBaseCommand } from '~/types/categorias'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Nueva Categoría Base - DistriCatalogo',
  meta: [
    { name: 'description', content: 'Crear nueva categoría base del sistema' }
  ]
})

// Composables
const { createCategory } = useCategoriesBase()
const router = useRouter()

// Estado reactivo
const loading = ref(false)

// Métodos
const handleSubmit = async (data: CreateCategoryBaseCommand) => {
  loading.value = true
  try {
    await createCategory(data)
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
</script>