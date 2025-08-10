<script setup lang="ts">
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Configurar Ofertas por Empresa',
  meta: [
    { name: 'description', content: 'Selecciona una empresa para configurar sus ofertas' }
  ]
})

// Composables
const api = useApi()
const router = useRouter()

// Estado
const empresas = ref<any[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const searchQuery = ref('')

// Computed
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
  loading.value = true
  error.value = null
  
  try {
    const response = await api.get('/api/Companies', {
      params: {
        page: 1,
        pageSize: 100,
        includeInactive: true
      }
    })
    
    if (!response.empresas) {
      console.error('No se encontraron empresas en la respuesta:', response)
      error.value = 'No se pudieron cargar las empresas'
      return
    }
    
    empresas.value = response.empresas
  } catch (err: any) {
    error.value = 'Error al cargar las empresas'
    console.error('Error fetching empresas:', err)
  } finally {
    loading.value = false
  }
}

const configureEmpresa = (empresaId: number) => {
  router.push(`/admin/marketing/ofertas/${empresaId}/configurar`)
}

const goBack = () => {
  router.push('/')
}

// Cargar datos iniciales
onMounted(() => {
  fetchEmpresas()
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
          <span class="text-gray-900 dark:text-gray-100">Configurar Ofertas por Empresa</span>
        </nav>
        
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
              Configurar Ofertas por Empresa
            </h1>
            <p class="mt-2 text-gray-600 dark:text-gray-400">
              Selecciona una empresa para configurar qué agrupaciones aparecen como ofertas especiales
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
      <div v-if="error" class="mb-6">
        <UAlert
          color="red"
          variant="soft"
          :title="error"
          :close-button="{ icon: 'i-heroicons-x-mark-20-solid', color: 'red', variant: 'link' }"
        />
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div v-for="i in 6" :key="i" class="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
          <div class="animate-pulse">
            <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-4"></div>
            <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2 mb-2"></div>
            <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded w-2/3 mb-4"></div>
            <div class="h-9 bg-gray-200 dark:bg-gray-700 rounded w-full"></div>
          </div>
        </div>
      </div>

      <!-- Empresas Grid -->
      <div v-else-if="filteredEmpresas.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
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

      <!-- Empty State -->
      <div v-else-if="!loading" class="text-center py-12">
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