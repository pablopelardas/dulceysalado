<script setup lang="ts">
import NovedadesDragDrop from '~/components/marketing/NovedadesDragDrop.vue'
import type { DragDropAgrupacion } from '~/types/agrupaciones'

definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Configurar Novedades - Empresa',
  meta: [
    { name: 'description', content: 'Configura las novedades visibles para la empresa' }
  ]
})

const route = useRoute()
const router = useRouter()
const empresaId = parseInt(route.params.id as string)

// Composables
const { 
  fetchNovedades, 
  fetchTodasLasAgrupaciones,
  updateNovedades, 
  isLoading, 
  error,
  selectedIds,
  todasLasAgrupaciones
} = useNovedades()

// Estado
const empresa = ref<any>(null)
const loadingEmpresa = ref(false)
const errorEmpresa = ref<string | null>(null)

// Computed
const loading = computed(() => 
  isLoading.value || loadingEmpresa.value
)

const errorState = computed(() => 
  error.value || errorEmpresa.value
)

const dragDropData = computed((): DragDropAgrupacion[] => {
  return todasLasAgrupaciones.value.map(agrupacion => ({
    id: agrupacion.id,
    codigo: agrupacion.codigo,
    nombre: agrupacion.nombre,
    descripcion: agrupacion.descripcion,
    activa: agrupacion.activa
  }))
})

const visibleIds = computed(() => selectedIds.value)

// Métodos
const fetchEmpresa = async () => {
  loadingEmpresa.value = true
  errorEmpresa.value = null
  
  try {
    const api = useApi()
    const response = await api.get(`/api/Companies/${empresaId}`)
    empresa.value = response
  } catch (err: any) {
    errorEmpresa.value = 'Error al cargar información de la empresa'
    console.error('Error fetching empresa:', err)
  } finally {
    loadingEmpresa.value = false
  }
}

const onSave = async (newVisibleIds: number[]) => {
  try {
    await updateNovedades(empresaId, newVisibleIds)
    await fetchNovedades(empresaId)
    
    // Mostrar toast de éxito
    const toast = useToast()
    toast.add({
      title: 'Novedades actualizadas',
      description: `Se han actualizado las novedades para ${empresa.value?.nombre}`,
      color: 'green'
    })
  } catch (err) {
    console.error('Error saving novedades:', err)
  }
}

const onReset = () => {
  // El reset se maneja en el componente interno
}

const goBack = () => {
  router.push('/admin/marketing/novedades/empresas')
}

// Cargar datos iniciales
onMounted(async () => {
  if (isNaN(empresaId)) {
    await router.push('/admin/marketing/novedades/empresas')
    return
  }
  
  await Promise.all([
    fetchEmpresa(),
    fetchTodasLasAgrupaciones(),
    fetchNovedades(empresaId)
  ])
})
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-600 dark:text-gray-400 mb-4">
          <NuxtLink to="/" class="hover:text-gray-900 dark:hover:text-gray-100">
            Dashboard
          </NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <NuxtLink to="/admin/marketing/novedades/empresas" class="hover:text-gray-900 dark:hover:text-gray-100">
            Novedades
          </NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Configurar Empresa</span>
        </nav>
        
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
              Configurar Novedades
            </h1>
            <p class="mt-2 text-gray-600 dark:text-gray-400">
              <template v-if="empresa">
                Configura las novedades visibles para: <strong>{{ empresa.nombre }}</strong>
              </template>
              <template v-else>
                Configura las novedades visibles para la empresa
              </template>
            </p>
          </div>
          <div class="flex items-center space-x-3">
            <UButton
              color="gray"
              variant="outline"
              @click="goBack"
            >
              <UIcon name="i-heroicons-arrow-left" class="mr-2" />
              Volver
            </UButton>
          </div>
        </div>
      </div>

      <!-- Error State -->
      <div v-if="errorState" class="mb-6">
        <UAlert
          color="red"
          variant="soft"
          :title="errorState"
          :close-button="{ icon: 'i-heroicons-x-mark-20-solid', color: 'red', variant: 'link' }"
        />
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="space-y-6">
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
          <div class="animate-pulse">
            <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-1/3 mb-4"></div>
            <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-2/3"></div>
          </div>
        </div>
        
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div v-for="i in 3" :key="i" class="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
            <div class="animate-pulse">
              <div class="h-8 w-8 bg-gray-200 dark:bg-gray-700 rounded mb-3"></div>
              <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2 mb-2"></div>
              <div class="h-8 bg-gray-200 dark:bg-gray-700 rounded w-1/3"></div>
            </div>
          </div>
        </div>
        
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div v-for="i in 2" :key="i" class="bg-white dark:bg-gray-800 rounded-lg shadow-sm">
            <div class="p-4 border-b border-gray-200 dark:border-gray-700">
              <div class="animate-pulse">
                <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-1/2 mb-2"></div>
                <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4"></div>
              </div>
            </div>
            <div class="p-4">
              <div class="animate-pulse space-y-3">
                <div v-for="j in 3" :key="j" class="h-16 bg-gray-200 dark:bg-gray-700 rounded"></div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Content -->
      <div v-else-if="!loading && !errorState">
        <NovedadesDragDrop
          :empresa-id="empresaId"
          :agrupaciones="dragDropData"
          :visible-ids="visibleIds"
          :loading="false"
          @save="onSave"
          @reset="onReset"
        />
      </div>
    </div>
  </div>
</template>