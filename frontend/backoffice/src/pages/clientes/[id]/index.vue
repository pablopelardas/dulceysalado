<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/clientes" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Clientes</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">{{ cliente?.nombre }}</span>
        </nav>
        
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
              {{ cliente?.nombre }}
            </h1>
            <div class="flex items-center gap-4 mt-2">
              <p class="text-gray-600 dark:text-gray-400">Código: {{ cliente?.codigo }}</p>
              <UBadge v-if="cliente?.tiene_acceso" :color="cliente?.activo ? 'green' : 'red'" variant="solid" size="sm">
                {{ cliente?.activo ? 'Activo' : 'Inactivo' }}
              </UBadge>
              <UBadge v-else color="gray" variant="soft" size="sm">
                Sin credenciales
              </UBadge>
            </div>
          </div>
          
          <div class="flex gap-3">
            <UButton
              :to="`/clientes/${cliente?.id}/edit`"
              color="gray"
              variant="solid"
            >
              <UIcon name="i-heroicons-pencil-square" class="mr-2" />
              Editar
            </UButton>
            
            <UButton
              :to="`/clientes/${cliente?.id}/credentials`"
              color="primary"
              variant="solid"
            >
              <UIcon name="i-heroicons-key" class="mr-2" />
              {{ cliente?.tiene_acceso ? 'Gestionar Credenciales' : 'Crear Credenciales' }}
            </UButton>
          </div>
        </div>
      </div>

      <!-- Loading state -->
      <div v-if="loading" class="space-y-6">
        <UCard>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <USkeleton class="h-20 w-full" />
            <USkeleton class="h-20 w-full" />
            <USkeleton class="h-20 w-full" />
            <USkeleton class="h-20 w-full" />
          </div>
        </UCard>
      </div>

      <!-- Error state -->
      <UAlert
        v-else-if="error"
        icon="i-heroicons-exclamation-triangle"
        color="red"
        variant="soft"
        :title="error"
        class="mb-6"
      />

      <!-- Información del cliente -->
      <div v-else-if="cliente" class="space-y-6">
        <!-- Información básica -->
        <UCard>
          <template #header>
            <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
              Información Básica
            </h3>
          </template>
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Email</label>
              <p class="text-gray-900 dark:text-gray-100">{{ cliente.email }}</p>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Teléfono</label>
              <p class="text-gray-900 dark:text-gray-100">{{ cliente.telefono || 'No especificado' }}</p>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">CUIT</label>
              <p class="text-gray-900 dark:text-gray-100">{{ cliente.cuit || 'No especificado' }}</p>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Tipo de IVA</label>
              <p class="text-gray-900 dark:text-gray-100">{{ cliente.tipo_iva || 'No especificado' }}</p>
            </div>
          </div>
        </UCard>

        <!-- Información de domicilio -->
        <UCard v-if="cliente.direccion || cliente.localidad || cliente.provincia">
          <template #header>
            <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
              Información de Domicilio
            </h3>
          </template>
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div v-if="cliente.direccion">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Dirección</label>
              <p class="text-gray-900 dark:text-gray-100">{{ cliente.direccion }} {{ cliente.altura || '' }}</p>
            </div>
            
            <div v-if="cliente.localidad">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Localidad</label>
              <p class="text-gray-900 dark:text-gray-100">{{ cliente.localidad }}</p>
            </div>
            
            <div v-if="cliente.provincia">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Provincia</label>
              <p class="text-gray-900 dark:text-gray-100">{{ cliente.provincia }}</p>
            </div>
          </div>
        </UCard>

        <!-- Configuración comercial -->
        <UCard>
          <template #header>
            <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
              Configuración Comercial
            </h3>
          </template>
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Lista de Precios</label>
              <div class="flex items-center gap-2">
                <UBadge v-if="cliente.lista_precio" color="blue" variant="soft">
                  {{ cliente.lista_precio.nombre }}
                </UBadge>
                <UBadge v-else color="gray" variant="soft">
                  Sin lista asignada
                </UBadge>
              </div>
            </div>
          </div>
        </UCard>

        <!-- Información de credenciales -->
        <UCard v-if="cliente.tiene_acceso">
          <template #header>
            <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
              Credenciales de Acceso
            </h3>
          </template>
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Usuario</label>
              <p class="text-gray-900 dark:text-gray-100 font-mono">{{ cliente.username }}</p>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Estado</label>
              <UBadge :color="cliente.activo ? 'green' : 'red'" variant="solid">
                {{ cliente.activo ? 'Activo' : 'Inactivo' }}
              </UBadge>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Último Login</label>
              <p class="text-gray-900 dark:text-gray-100">
                {{ cliente.last_login ? formatDate(cliente.last_login) : 'Nunca' }}
              </p>
            </div>
          </div>
        </UCard>

        <!-- Información de auditoría -->
        <UCard>
          <template #header>
            <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
              Información de Auditoría
            </h3>
          </template>
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Creado por</label>
              <p class="text-gray-900 dark:text-gray-100">{{ cliente.created_by || 'Sistema' }}</p>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Fecha de creación</label>
              <p class="text-gray-900 dark:text-gray-100">
                {{ cliente.created_at ? formatDate(cliente.created_at) : 'No disponible' }}
              </p>
            </div>
            
            <div v-if="cliente.updated_at">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Última actualización</label>
              <p class="text-gray-900 dark:text-gray-100">
                {{ formatDate(cliente.updated_at) }}
              </p>
            </div>
          </div>
        </UCard>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Configuración de página
definePageMeta({
  middleware: ['auth', 'feature-flag'],
  layout: 'default',
  featureFlag: 'cliente_autenticacion'
})

// Obtener ID del cliente desde la ruta
const route = useRoute()
const id = computed(() => parseInt(route.params.id as string))

// Composables
const { fetchCliente } = useClientes()

// Estado
const cliente = ref(null)
const loading = ref(true)
const error = ref<string | null>(null)

// useHead después de declarar las variables
useHead({
  title: computed(() => {
    if (cliente.value?.nombre) {
      return cliente.value.nombre
    }
    return 'Cliente'
  }),
  meta: [
    { name: 'description', content: 'Información detallada del cliente' }
  ]
})

// Helper para formatear fechas
const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Cargar datos del cliente
const loadCliente = async () => {
  if (!id.value || isNaN(id.value)) {
    error.value = 'ID de cliente inválido'
    return
  }

  try {
    loading.value = true
    error.value = null
    
    cliente.value = await fetchCliente(id.value)
  } catch (err: any) {
    error.value = err.message || 'Error al cargar el cliente'
  } finally {
    loading.value = false
  }
}

// Cargar datos al montar el componente
onMounted(() => {
  loadCliente()
})

// Recargar si cambia el ID
watch(() => id.value, () => {
  if (id.value) {
    loadCliente()
  }
})
</script>