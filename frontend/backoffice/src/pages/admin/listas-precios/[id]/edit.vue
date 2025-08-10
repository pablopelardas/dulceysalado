<script setup lang="ts">
import ListaPrecioForm from '~/components/listas-precios/ListaPrecioForm.vue'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Editar Lista de Precios',
  meta: [
    { name: 'description', content: 'Editar lista de precios existente' }
  ]
})

// Composables
const router = useRouter()
const route = useRoute()
const { isEmpresaPrincipal } = useAuth()
const { fetchListaById, updateLista } = useListasPreciosCrud()

// Estado reactivo
const loading = ref(false)
const loadingData = ref(true)
const error = ref<string | null>(null)
const listaData = ref<any>(null)

// Obtener ID de la ruta
const listaId = computed(() => {
  const id = Array.isArray(route.params.id) ? route.params.id[0] : route.params.id
  return parseInt(id)
})

// Handlers
const handleSubmit = async (formData: any) => {
  loading.value = true
  
  try {
    await updateLista(listaId.value, formData)
    router.push('/admin/listas-precios')
  } catch (error) {
    console.error('Error al actualizar lista:', error)
  } finally {
    loading.value = false
  }
}

const handleCancel = () => {
  router.push('/admin/listas-precios')
}

// Cargar datos de la lista
const loadLista = async () => {
  if (!listaId.value || isNaN(listaId.value)) {
    error.value = 'ID de lista inválido'
    return
  }

  loadingData.value = true
  error.value = null
  
  try {
    listaData.value = await fetchListaById(listaId.value)
  } catch (err: any) {
    error.value = err.message || 'Error al cargar la lista de precios'
    console.error('Error cargando lista:', err)
  } finally {
    loadingData.value = false
  }
}

// Cargar datos al montar
onMounted(async () => {
  if (!isEmpresaPrincipal.value) {
    router.replace('/')
    return
  }
  
  await loadLista()
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
          <span class="text-gray-900 dark:text-gray-100">Editar Lista</span>
        </nav>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Editar Lista de Precios
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Modifica los datos de la lista de precios
        </p>
      </div>

      <!-- Estado de carga -->
      <div v-if="loadingData" class="flex justify-center py-12">
        <UIcon name="i-heroicons-arrow-path" class="animate-spin h-8 w-8 text-blue-500" />
      </div>

      <!-- Error al cargar -->
      <UAlert
        v-else-if="error"
        icon="i-heroicons-exclamation-triangle"
        color="error"
        variant="soft"
        :title="error"
        class="mb-6"
      />

      <!-- Formulario -->
      <ListaPrecioForm
        v-else-if="listaData"
        mode="edit"
        :initial-data="listaData"
        :loading="loading"
        @submit="handleSubmit"
        @cancel="handleCancel"
      />
    </div>
  </div>
</template>