<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/clientes" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Clientes</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <NuxtLink :to="`/clientes/${id}`" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">{{ cliente?.nombre }}</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Credenciales</span>
        </nav>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Gestionar Credenciales
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          {{ cliente?.tiene_acceso ? 'Administra las credenciales de acceso' : 'Crea credenciales de acceso' }} para {{ cliente?.nombre }}
        </p>
      </div>

      <!-- Loading state -->
      <div v-if="loadingCliente" class="space-y-6">
        <UCard>
          <USkeleton class="h-8 w-64 mb-4" />
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
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

      <!-- Mensaje si el cliente está inactivo -->
      <div v-else-if="cliente && !cliente.activo" class="space-y-6">
        <UAlert
          icon="i-heroicons-exclamation-triangle"
          color="yellow"
          variant="soft"
          title="Cliente inactivo"
          description="Este cliente está inactivo. Para gestionar sus credenciales, primero debe activarlo desde la página de edición."
          class="mb-6"
        />
        
        <div class="flex justify-center">
          <UButton
            :to="`/clientes/${id}/edit`"
            color="primary"
            variant="solid"
          >
            Editar Cliente
          </UButton>
        </div>
      </div>

      <!-- Formularios -->
      <div v-else-if="cliente && cliente.activo" class="space-y-6">
        <!-- Información del cliente -->
        <UCard>
          <template #header>
            <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
              Información del Cliente
            </h3>
          </template>
          
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Nombre</label>
              <p class="text-gray-900 dark:text-gray-100">{{ cliente.nombre }}</p>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Email</label>
              <p class="text-gray-900 dark:text-gray-100">{{ cliente.email }}</p>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Estado de credenciales</label>
              <UBadge v-if="cliente.tiene_acceso" :color="cliente.activo ? 'green' : 'red'" variant="solid">
                {{ cliente.activo ? 'Activo' : 'Inactivo' }}
              </UBadge>
              <UBadge v-else color="gray" variant="soft">
                Sin credenciales
              </UBadge>
            </div>
          </div>
        </UCard>

        <!-- Formulario de credenciales -->
        <UCard>
          <template #header>
            <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
              {{ cliente.tiene_acceso ? 'Editar Credenciales' : 'Crear Credenciales' }}
            </h3>
          </template>
          
          <UForm 
            ref="form"
            :schema="credentialsSchema" 
            :state="credentialsForm"
            @submit="onSubmit"
            @error="onError"
            class="space-y-6"
          >
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <UFormField label="Usuario" name="username" required>
                <UInput 
                  v-model="credentialsForm.username"
                  placeholder="Nombre de usuario"
                  :disabled="loading"
                />
              </UFormField>
              
              <UFormField label="Nueva Contraseña" name="password" :required="!cliente.tiene_acceso">
                <UInput 
                  v-model="credentialsForm.password"
                  type="password"
                  :placeholder="cliente.tiene_acceso ? 'Dejar vacío para mantener actual' : 'Contraseña'"
                  :disabled="loading"
                />
                <template #help>
                  <span class="text-sm text-gray-500 dark:text-gray-400">
                    {{ cliente.tiene_acceso ? 'Dejar vacío para mantener la contraseña actual' : 'Contraseña para el acceso' }}
                  </span>
                </template>
              </UFormField>
            </div>
            

            <!-- Información adicional si ya tiene credenciales -->
            <div v-if="cliente.tiene_acceso" class="bg-gray-50 dark:bg-gray-900 rounded-lg p-4">
              <h4 class="font-medium text-gray-900 dark:text-gray-100 mb-2">Información actual</h4>
              <div class="text-sm space-y-1">
                <p><span class="font-medium">Usuario actual:</span> {{ cliente.username }}</p>
                <p><span class="font-medium">Último login:</span> {{ cliente.last_login ? formatDate(cliente.last_login) : 'Nunca' }}</p>
              </div>
            </div>
            
            <!-- Botones de acción -->
            <div class="flex flex-col sm:flex-row gap-4 justify-end pt-6 border-t border-gray-200 dark:border-gray-700">
              <UButton
                variant="ghost"
                color="gray"
                @click="handleCancel"
                :disabled="loading"
              >
                Cancelar
              </UButton>
              
              <UButton
                type="submit"
                color="primary"
                :loading="loading"
                :disabled="loading"
              >
                {{ cliente.tiene_acceso ? 'Actualizar' : 'Crear' }} Credenciales
              </UButton>
            </div>
          </UForm>
        </UCard>
      </div>
    </div>
  </div>

</template>

<script setup lang="ts">
import { z } from 'zod'

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
const { 
  fetchCliente, 
  createClienteCredentials, 
  updateClientePassword,
  updateCliente
} = useClientes()

// Estado
const cliente = ref(null)
const loadingCliente = ref(true)
const loading = ref(false)
const error = ref<string | null>(null)
const form = ref()

// useHead después de declarar las variables
useHead({
  title: computed(() => cliente.value ? `Credenciales - ${cliente.value.nombre}` : 'Gestionar Credenciales'),
  meta: [
    { name: 'description', content: 'Gestionar credenciales de acceso del cliente' }
  ]
})

// Esquema de validación
const credentialsSchema = z.object({
  username: z.string().min(1, 'El usuario es requerido'),
  password: z.string().optional()
}).refine((data) => {
  // Si el cliente no tiene credenciales, la contraseña es requerida
  if (!cliente.value?.tiene_acceso) {
    return data.password && data.password.length > 0
  }
  return true
}, {
  message: 'La contraseña es requerida para crear credenciales',
  path: ['password']
})

// Datos del formulario
const credentialsForm = reactive({
  username: '',
  password: ''
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

// Métodos
const onSubmit = async (event: any) => {
  loading.value = true
  
  try {
    const data = { ...event.data }
    
    if (cliente.value?.tiene_acceso) {
      // Actualizar contraseña si se proporcionó
      if (data.password) {
        await updateClientePassword(id.value, data.password)
      }
    } else {
      // Crear nuevas credenciales
      await createClienteCredentials({
        cliente_id: id.value,
        username: data.username,
        password: data.password
      })
    }
    
    // Recargar datos del cliente
    await loadCliente()
    
  } catch (error) {
    // Error ya manejado en el composable
    console.error('Error al gestionar credenciales:', error)
  } finally {
    loading.value = false
  }
}

const onError = (event: any) => {
  console.error('Errores de validación:', event.errors)
}


const handleCancel = () => {
  navigateTo(`/clientes/${id.value}`)
}

// Cargar datos del cliente
const loadCliente = async () => {
  if (!id.value || isNaN(id.value)) {
    error.value = 'ID de cliente inválido'
    return
  }

  try {
    loadingCliente.value = true
    error.value = null
    
    cliente.value = await fetchCliente(id.value)
    
    // Actualizar formulario con datos existentes si tiene credenciales
    if (cliente.value?.tiene_acceso) {
      credentialsForm.username = cliente.value.username || ''
      credentialsForm.password = '' // Siempre vacío para actualizaciones
    }
    
  } catch (err: any) {
    error.value = err.message || 'Error al cargar el cliente'
  } finally {
    loadingCliente.value = false
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