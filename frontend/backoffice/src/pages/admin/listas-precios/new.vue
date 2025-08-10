<script setup lang="ts">
import ListaPrecioForm from '~/components/listas-precios/ListaPrecioForm.vue'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Nueva Lista de Precios',
  meta: [
    { name: 'description', content: 'Crear nueva lista de precios' }
  ]
})

// Composables
const router = useRouter()
const { isEmpresaPrincipal } = useAuth()
const { createLista } = useListasPreciosCrud()

// Estado reactivo
const loading = ref(false)

// Handlers
const handleSubmit = async (formData: any) => {
  loading.value = true
  
  try {
    await createLista(formData)
    router.push('/admin/listas-precios')
  } catch (error) {
    console.error('Error al crear lista:', error)
  } finally {
    loading.value = false
  }
}

const handleCancel = () => {
  router.push('/admin/listas-precios')
}

// Verificar permisos al montar
onMounted(() => {
  if (!isEmpresaPrincipal.value) {
    router.replace('/')
  }
})
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-3xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Dashboard</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <NuxtLink to="/admin/listas-precios" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Listas de Precios</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Nueva Lista</span>
        </nav>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Nueva Lista de Precios
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Crea una nueva lista de precios para tus productos
        </p>
      </div>

      <!-- Formulario -->
      <ListaPrecioForm
        mode="create"
        :loading="loading"
        @submit="handleSubmit"
        @cancel="handleCancel"
      />
    </div>
  </div>
</template>