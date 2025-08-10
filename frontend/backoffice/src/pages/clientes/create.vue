<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/clientes" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Clientes</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Crear Cliente</span>
        </nav>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Crear Nuevo Cliente
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Completa los datos para registrar un nuevo cliente
        </p>
      </div>

      <!-- Formulario de creación -->
      <ClienteForm
        mode="create"
        :loading="creating"
        @submit="handleSubmit"
        @cancel="handleCancel"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { CreateClienteCommand } from '~/types/clientes'
import ClienteForm from '~/components/clientes/ClienteForm.vue'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'feature-flag'],
  layout: 'default',
  featureFlag: 'cliente_autenticacion'
})

useHead({
  title: 'Crear Cliente',
  meta: [
    { name: 'description', content: 'Crear un nuevo cliente en el sistema' }
  ]
})

// Composables
const { createCliente, createClienteCredentials } = useClientes()

// Estado
const creating = ref(false)

// Métodos
const handleSubmit = async (clienteData: CreateClienteCommand) => {
  creating.value = true
  
  try {
    // Separar datos del cliente de los datos de credenciales
    const { username, password, is_active, ...clienteBasicData } = clienteData as any
    
    // Crear cliente básico
    const newCliente = await createCliente(clienteBasicData)
    
    // Si se proporcionaron credenciales, crearlas
    if (username && password && newCliente?.id) {
      await createClienteCredentials({
        cliente_id: newCliente.id,
        username,
        password,
        is_active: is_active ?? true
      })
    }
    
    // Redirigir a la lista de clientes
    await navigateTo('/clientes')
  } catch (error) {
    // Error ya manejado en el composable
    console.error('Error al crear cliente:', error)
  } finally {
    creating.value = false
  }
}

const handleCancel = () => {
  navigateTo('/clientes')
}
</script>