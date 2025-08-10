<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 py-12 px-4 sm:px-6 lg:px-8 transition-colors duration-200">
    <div class="max-w-lg w-full space-y-8">
      <div class="text-center">
        <!-- Icono de acceso denegado -->
        <div class="mx-auto h-20 w-20 bg-yellow-100 dark:bg-yellow-900/20 rounded-full flex items-center justify-center mb-6">
          <UIcon 
            name="i-heroicons-shield-exclamation" 
            class="h-12 w-12 text-yellow-600 dark:text-yellow-400"
          />
        </div>
        
        <!-- Título -->
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100 mb-2">
          Acceso Denegado
        </h1>
        
        <!-- Mensaje principal -->
        <p class="text-lg text-gray-600 dark:text-gray-400 mb-6">
          No tienes permisos suficientes para acceder a esta sección
        </p>
        
        <!-- Información detallada -->
        <div class="bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg p-4 mb-8 text-left">
          <div class="flex">
            <UIcon name="i-heroicons-information-circle" class="h-5 w-5 text-yellow-600 dark:text-yellow-400 mt-0.5 mr-3 flex-shrink-0" />
            <div class="text-sm text-yellow-800 dark:text-yellow-200">
              <p class="font-medium mb-2">Detalles del error:</p>
              <ul class="space-y-1 text-xs">
                <li><strong>Ruta solicitada:</strong> {{ attemptedRoute }}</li>
                <li><strong>Permisos requeridos:</strong> {{ formattedRequiredPermissions }}</li>
                <li><strong>Tu rol actual:</strong> {{ currentUser?.rol || 'No disponible' }}</li>
              </ul>
              <p class="mt-3 text-sm">
                💡 <strong>Solución:</strong> Contacta a tu administrador para solicitar los permisos necesarios.
              </p>
            </div>
          </div>
        </div>
        
        <!-- Botones de acción -->
        <div class="flex flex-col sm:flex-row gap-4 justify-center">
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
            color="neutral"
            variant="soft"
            size="lg"
            @click="goBack"
            class="flex items-center justify-center"
          >
            <UIcon name="i-heroicons-arrow-left" class="mr-2 h-5 w-5" />
            Regresar
          </UButton>
        </div>
        
        <!-- Información adicional -->
        <div class="mt-8 pt-6 border-t border-gray-200 dark:border-gray-700">
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Si crees que esto es un error, contacta al soporte técnico.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Configuración de página
definePageMeta({
  layout: 'default',
  middleware: 'auth' // Solo auth, no permissions para esta página
})

useHead({
  title: 'Acceso Denegado - 403',
  meta: [
    { name: 'description', content: 'No tienes permisos para acceder a esta página' },
    { name: 'robots', content: 'noindex' }
  ]
})

// Composables
const { user: currentUser } = useAuth()
const route = useRoute()

// Computed para obtener información desde query parameters
const attemptedRoute = computed(() => {
  return route.query.route as string || 'Ruta desconocida'
})

const requiredPermissions = computed(() => {
  const required = route.query.required as string
  return required ? required.split(',') : []
})

const errorMessage = computed(() => {
  return route.query.message as string || 'Permisos insuficientes'
})

const formattedRequiredPermissions = computed(() => {
  if (!requiredPermissions.value.length) return 'No especificado'
  
  const permissionLabels: Record<string, string> = {
    'canManageUsuarios': 'Gestionar Usuarios',
    'canManageProductosBase': 'Gestionar Productos Base',
    'canManageProductosEmpresa': 'Gestionar Productos de Empresa',
    'canManageCategoriasBase': 'Gestionar Categorías Base',
    'canManageCategoriasEmpresa': 'Gestionar Categorías de Empresa',
    'canViewEstadisticas': 'Ver Estadísticas'
  }
  
  return requiredPermissions.value
    .map((perm: string) => permissionLabels[perm] || perm)
    .join(', ')
})

// Métodos
const goBack = () => {
  if (window.history.length > 1) {
    window.history.back()
  } else {
    navigateTo('/')
  }
}

</script>