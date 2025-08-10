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
          <span class="text-gray-900 dark:text-gray-100">Editar</span>
        </nav>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Editar Cliente
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Modifica los datos del cliente {{ cliente?.nombre }}
        </p>
      </div>

      <!-- Loading state -->
      <div v-if="loadingCliente" class="space-y-6">
        <UCard>
          <USkeleton class="h-8 w-64 mb-4" />
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

      <!-- Formulario de edición -->
      <div v-else-if="cliente">
        <ClienteForm
          mode="edit"
          :cliente="cliente"
          :loading="updating"
          @submit="handleSubmit"
          @cancel="handleCancel"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { UpdateClienteCommand } from '~/types/clientes'
import ClienteForm from '~/components/clientes/ClienteForm.vue'

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
const { fetchCliente, updateCliente } = useClientes()

// Estado
const cliente = ref(null)
const loadingCliente = ref(true)
const updating = ref(false)
const error = ref<string | null>(null)

// useHead después de declarar las variables
useHead({
  title: computed(() => {
    if (cliente.value?.nombre) {
      return `Editar ${cliente.value.nombre}`
    }
    return 'Editar Cliente'
  }),
  meta: [
    { name: 'description', content: 'Editar información del cliente' }
  ]
})

// Métodos
const handleSubmit = async (clienteData: UpdateClienteCommand) => {
  updating.value = true
  
  try {
    await updateCliente(id.value, clienteData)
    
    // Usar nextTick para asegurar que la UI se actualice antes de navegar
    await nextTick()
    
    // Redirigir a la lista de clientes en lugar del detalle para evitar problemas
    await navigateTo('/clientes')
  } catch (error) {
    // Error ya manejado en el composable
    console.error('Error al actualizar cliente:', error)
  } finally {
    updating.value = false
  }
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