<template>
  <NuxtLayout name="default">
    <div class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 py-12 px-4 sm:px-6 lg:px-8 transition-colors duration-200">
      <div class="max-w-md w-full space-y-8">
        <div class="text-center">
          <!-- Icono de error -->
          <UIcon 
            :name="errorIcon" 
            :class="errorIconClass"
            class="mx-auto h-16 w-16 mb-6"
          />
          
          <!-- Título del error -->
          <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100 mb-2">
            {{ errorTitle }}
          </h1>
          
          <!-- Mensaje del error -->
          <p class="text-lg text-gray-600 dark:text-gray-400 mb-8">
            {{ errorMessage }}
          </p>
          
          <!-- Información adicional para errores de permisos -->
          <div v-if="error.statusCode === 403" class="bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg p-4 mb-6">
            <div class="flex">
              <UIcon name="i-heroicons-exclamation-triangle" class="h-5 w-5 text-yellow-400 dark:text-yellow-300 mt-0.5 mr-3" />
              <div class="text-sm text-yellow-800 dark:text-yellow-200">
                <p class="font-medium">Acceso restringido</p>
                <p class="mt-1">
                  Tu cuenta no tiene los permisos necesarios para acceder a esta sección.
                  Contacta a tu administrador si necesitas acceso.
                </p>
              </div>
            </div>
          </div>
          
          <!-- Botones de acción -->
          <div class="flex flex-col sm:flex-row gap-3 justify-center">
            <UButton
              to="/"
              color="primary"
              size="lg"
              class="flex items-center justify-center"
            >
              <UIcon name="i-heroicons-home" class="mr-2 h-5 w-5" />
              Ir al Dashboard
            </UButton>
            
            <UButton
              color="gray"
              variant="soft"
              size="lg"
              @click="goBack"
              class="flex items-center justify-center"
            >
              <UIcon name="i-heroicons-arrow-left" class="mr-2 h-5 w-5" />
              Volver
            </UButton>
          </div>
          
          <!-- Información de debug (solo en desarrollo) -->
          <details v-if="isDevelopment" class="mt-8 text-left">
            <summary class="cursor-pointer text-sm text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300">
              Detalles técnicos (desarrollo)
            </summary>
            <div class="mt-2 bg-gray-100 dark:bg-gray-800 rounded-lg p-4 text-xs text-gray-700 dark:text-gray-300 overflow-auto border border-gray-200 dark:border-gray-700">
              <pre>{{ JSON.stringify(error, null, 2) }}</pre>
            </div>
          </details>
        </div>
      </div>
    </div>
  </NuxtLayout>
</template>

<script setup lang="ts">
interface ErrorProps {
  error: {
    statusCode: number
    statusMessage: string
    message?: string
    stack?: string
  }
}

const props = defineProps<ErrorProps>()

// Configuración de la página
useHead({
  title: `Error ${props.error.statusCode}`,
  meta: [
    { name: 'robots', content: 'noindex' }
  ]
})

// Variables reactivas
const isDevelopment = process.dev

// Computed para personalizar el error según el tipo
const errorIcon = computed(() => {
  switch (props.error.statusCode) {
    case 403:
      return 'i-heroicons-shield-exclamation'
    case 404:
      return 'i-heroicons-magnifying-glass'
    case 500:
      return 'i-heroicons-exclamation-triangle'
    default:
      return 'i-heroicons-exclamation-circle'
  }
})

const errorIconClass = computed(() => {
  switch (props.error.statusCode) {
    case 403:
      return 'text-yellow-500'
    case 404:
      return 'text-blue-500'
    case 500:
      return 'text-red-500'
    default:
      return 'text-gray-500'
  }
})

const errorTitle = computed(() => {
  switch (props.error.statusCode) {
    case 403:
      return 'Acceso Denegado'
    case 404:
      return 'Página no Encontrada'
    case 500:
      return 'Error del Servidor'
    default:
      return `Error ${props.error.statusCode}`
  }
})

const errorMessage = computed(() => {
  switch (props.error.statusCode) {
    case 403:
      return 'No tienes permisos suficientes para acceder a esta página.'
    case 404:
      return 'La página que buscas no existe o ha sido movida.'
    case 500:
      return 'Ocurrió un error interno del servidor. Inténtalo de nuevo más tarde.'
    default:
      return props.error.statusMessage || 'Ha ocurrido un error inesperado.'
  }
})

// Métodos
const goBack = () => {
  if (window.history.length > 1) {
    window.history.back()
  } else {
    navigateTo('/')
  }
}

// Limpiar el error al salir de la página
onBeforeUnmount(() => {
  clearError()
})
</script>