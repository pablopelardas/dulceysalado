<script setup lang="ts">
import type { DragDropAgrupacion } from '~/types/agrupaciones'

definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Gestionar Ofertas',
  meta: [
    { name: 'description', content: 'Configura qué agrupaciones aparecen como ofertas especiales en el catálogo público' }
  ]
})

const route = useRoute()
const router = useRouter()

// Composables
const { user, empresa, isEmpresaPrincipal } = useAuth()
const { 
  fetchOfertas, 
  fetchTodasLasAgrupaciones,
  updateOfertas, 
  isLoading, 
  error,
  selectedIds,
  todasLasAgrupaciones
} = useOfertas()

// Estado
const empresaSeleccionada = ref<any>(null)
const empresas = ref<any[]>([])
const loadingEmpresas = ref(false)
const errorEmpresas = ref<string | null>(null)
const searchQuery = ref('')

// Computed
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

const filteredEmpresas = computed(() => {
  if (!searchQuery.value.trim()) return empresas.value
  
  const query = searchQuery.value.toLowerCase()
  return empresas.value.filter(empresa =>
    empresa.nombre.toLowerCase().includes(query) ||
    empresa.codigo.toLowerCase().includes(query) ||
    (empresa.email && empresa.email.toLowerCase().includes(query))
  )
})

const empresaClienteCount = computed(() => {
  return empresas.value.filter(empresa => empresa.tipo_empresa === 'cliente').length
})

const empresaActivaCount = computed(() => {
  return empresas.value.filter(empresa => empresa.activa).length
})

// Métodos
const fetchEmpresas = async () => {
  loadingEmpresas.value = true
  errorEmpresas.value = null
  
  try {
    const api = useApi()
    const response = await api.get('/api/Companies', {
      params: {
        page: 1,
        pageSize: 100,
        includeInactive: false
      }
    })
    
    if (response.empresas) {
      empresas.value = response.empresas
    }
  } catch (err: any) {
    errorEmpresas.value = 'Error al cargar las empresas'
    console.error('Error fetching empresas:', err)
  } finally {
    loadingEmpresas.value = false
  }
}

const configureEmpresa = (empresaId: number) => {
  const empresa = empresas.value.find(emp => emp.id === empresaId)
  if (!empresa) return
  
  empresaSeleccionada.value = empresa
  
  // Cargar datos de la empresa seleccionada
  Promise.all([
    fetchOfertas(empresaId),
    fetchTodasLasAgrupaciones()
  ])
}

const onSave = async (newVisibleIds: number[]) => {
  if (!empresaSeleccionada.value) return
  
  try {
    await updateOfertas(empresaSeleccionada.value.id, newVisibleIds)
    
    // Mostrar toast de éxito
    const toast = useToast()
    toast.add({
      title: 'Ofertas actualizadas',
      description: `Se han actualizado las ofertas para ${empresaSeleccionada.value.nombre}`,
      color: 'green'
    })
  } catch (err) {
    console.error('Error saving ofertas:', err)
  }
}

const onReset = () => {
  if (empresaSeleccionada.value) {
    onEmpresaChange(empresaSeleccionada.value.id)
  }
}

const goBack = () => {
  router.push('/')
}

// Cargar datos iniciales
onMounted(async () => {
  if (!isEmpresaPrincipal.value) {
    await router.push('/')
    return
  }
  
  await fetchEmpresas()
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
          <span class="text-gray-900 dark:text-gray-100">Gestionar Ofertas</span>
        </nav>
        
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
              Gestionar Ofertas
            </h1>
            <p class="mt-2 text-gray-600 dark:text-gray-400">
              Configura qué agrupaciones aparecen como ofertas especiales en el catálogo público
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

      <!-- Estadísticas -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <UCard>
          <div class="flex items-center">
            <UIcon name="i-heroicons-building-office" class="h-8 w-8 text-blue-600 mr-3" />
            <div>
              <p class="text-sm text-gray-600 dark:text-gray-400">Total Empresas</p>
              <p class="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {{ empresas.length }}
              </p>
            </div>
          </div>
        </UCard>
        <UCard>
          <div class="flex items-center">
            <UIcon name="i-heroicons-user-group" class="h-8 w-8 text-green-600 mr-3" />
            <div>
              <p class="text-sm text-gray-600 dark:text-gray-400">Empresas Cliente</p>
              <p class="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {{ empresaClienteCount }}
              </p>
            </div>
          </div>
        </UCard>
        <UCard>
          <div class="flex items-center">
            <UIcon name="i-heroicons-check-circle" class="h-8 w-8 text-orange-600 mr-3" />
            <div>
              <p class="text-sm text-gray-600 dark:text-gray-400">Empresas Activas</p>
              <p class="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {{ empresaActivaCount }}
              </p>
            </div>
          </div>
        </UCard>
      </div>

      <!-- Filtros y Búsqueda -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6 mb-6">
        <div class="flex flex-col sm:flex-row gap-4">
          <div class="flex-1">
            <UInput
              v-model="searchQuery"
              placeholder="Buscar por nombre, código o email..."
              icon="i-heroicons-magnifying-glass"
              size="lg"
            />
          </div>
        </div>
      </div>

      <!-- Error State -->
      <div v-if="error || errorEmpresas" class="mb-6">
        <UAlert
          color="red"
          variant="soft"
          :title="error || errorEmpresas"
          :close-button="{ icon: 'i-heroicons-x-mark-20-solid', color: 'red', variant: 'link' }"
        />
      </div>

      <!-- Loading State -->
      <div v-if="isLoading" class="space-y-6">
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
          <div class="animate-pulse">
            <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-1/3 mb-4"></div>
            <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-2/3"></div>
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

      <!-- Empresas Grid -->
      <div v-if="!empresaSeleccionada && filteredEmpresas.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <UCard
          v-for="empresa in filteredEmpresas"
          :key="empresa.id"
          class="hover:shadow-lg transition-shadow cursor-pointer"
          @click="configureEmpresa(empresa.id)"
        >
          <div class="p-6">
            <div class="flex items-start justify-between mb-4">
              <div class="flex-1">
                <div class="flex items-center gap-2 mb-2">
                  <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
                    {{ empresa.nombre }}
                  </h3>
                  <UBadge
                    v-if="empresa.tipo_empresa === 'principal'"
                    color="blue"
                    variant="solid"
                    size="xs"
                  >
                    Principal
                  </UBadge>
                </div>
                <div class="space-y-1">
                  <div class="flex items-center text-sm text-gray-600 dark:text-gray-400">
                    <UIcon name="i-heroicons-hashtag" class="h-4 w-4 mr-2" />
                    <span>{{ empresa.codigo }}</span>
                  </div>
                  <div v-if="empresa.email" class="flex items-center text-sm text-gray-600 dark:text-gray-400">
                    <UIcon name="i-heroicons-envelope" class="h-4 w-4 mr-2" />
                    <span>{{ empresa.email }}</span>
                  </div>
                  <div class="flex items-center text-sm text-gray-600 dark:text-gray-400">
                    <UIcon name="i-heroicons-calendar" class="h-4 w-4 mr-2" />
                    <span>{{ new Date(empresa.created_at).toLocaleDateString() }}</span>
                  </div>
                </div>
              </div>
              <UBadge
                :color="empresa.activa ? 'green' : 'red'"
                variant="soft"
              >
                {{ empresa.activa ? 'Activa' : 'Inactiva' }}
              </UBadge>
            </div>
            
            <UButton
              color="orange"
              variant="solid"
              block
              @click.stop="configureEmpresa(empresa.id)"
            >
              <UIcon name="i-heroicons-tag" class="mr-2" />
              Configurar Ofertas
            </UButton>
          </div>
        </UCard>
      </div>

      <!-- Content -->
      <div v-else-if="!isLoading && !error && empresaSeleccionada">
        <div class="mb-6">
          <UButton
            color="gray"
            variant="outline"
            @click="empresaSeleccionada = null"
          >
            <UIcon name="i-heroicons-arrow-left" class="mr-2" />
            Volver a la lista
          </UButton>
        </div>
        
        <OfertasDragDrop
          :empresa-id="empresaSeleccionada.id"
          :agrupaciones="dragDropData"
          :visible-ids="visibleIds"
          :loading="false"
          @save="onSave"
          @reset="onReset"
        />
      </div>

      <!-- Empty State -->
      <div v-else-if="!loadingEmpresas && filteredEmpresas.length === 0" class="text-center py-12">
        <div class="flex flex-col items-center">
          <UIcon name="i-heroicons-building-office" class="h-24 w-24 text-gray-400 mb-4" />
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-2">
            {{ searchQuery ? 'No se encontraron empresas' : 'No hay empresas cliente' }}
          </h3>
          <p class="text-gray-600 dark:text-gray-400 mb-6">
            {{ searchQuery 
              ? `No se encontraron empresas que coincidan con "${searchQuery}"`
              : 'Aún no hay empresas cliente registradas en el sistema'
            }}
          </p>
          <UButton
            v-if="searchQuery"
            color="gray"
            variant="outline"
            @click="searchQuery = ''"
          >
            Limpiar búsqueda
          </UButton>
        </div>
      </div>
    </div>
  </div>
</template>