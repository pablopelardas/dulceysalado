<template>
  <footer class="bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700 transition-colors duration-200">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-8">
        <!-- Información de la empresa -->
        <div class="md:col-span-2">
          <div class="flex items-center mb-4">
            <UIcon name="i-heroicons-building-office" class="h-8 w-8 text-blue-600 dark:text-blue-400 mr-3" />
            <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
              Dulce y Salado Admin
            </h3>
          </div>
          <p class="text-gray-600 dark:text-gray-400 text-sm leading-6 max-w-md">
            Sistema de gestión de catálogo para Dulce y Salado. 
            Administra productos, categorías, usuarios y visualiza estadísticas de manera eficiente.
          </p>
          <div class="mt-4 flex items-center space-x-4">
            <div class="flex items-center text-sm text-gray-500 dark:text-gray-400">
              <UIcon name="i-heroicons-shield-check" class="h-4 w-4 mr-1 text-green-500" />
              Sistema Seguro
            </div>
            <div class="flex items-center text-sm text-gray-500 dark:text-gray-400">
              <UIcon name="i-heroicons-cloud" class="h-4 w-4 mr-1 text-blue-500" />
              En la Nube
            </div>
          </div>
        </div>

        <!-- Enlaces rápidos -->
        <div>
          <h4 class="text-sm font-semibold text-gray-900 dark:text-gray-100 uppercase tracking-wider mb-4">
            Navegación
          </h4>
          <ul class="space-y-2">
            <li>
              <NuxtLink 
                to="/" 
                class="text-sm text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
              >
                Dashboard
              </NuxtLink>
            </li>
            <li>
              <NuxtLink 
                to="/profile" 
                class="text-sm text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
              >
                Mi Perfil
              </NuxtLink>
            </li>
            <ClientOnly>
              <li v-if="userPermissions.canManageUsuarios">
                <NuxtLink 
                  to="/users" 
                  class="text-sm text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 transition-colors"
                >
                  Gestión de Usuarios
                </NuxtLink>
              </li>
            </ClientOnly>
          </ul>
        </div>

        <!-- Información del sistema -->
        <div>
          <h4 class="text-sm font-semibold text-gray-900 dark:text-gray-100 uppercase tracking-wider mb-4">
            Sistema
          </h4>
          <ul class="space-y-2 text-sm">
            <li class="text-gray-600 dark:text-gray-400">
              <span class="font-medium">Versión:</span> 1.0.0
            </li>
            <li class="text-gray-600 dark:text-gray-400">
              <span class="font-medium">Empresa:</span> 
              <ClientOnly>
                <span class="ml-1">{{ empresa?.nombre || 'No disponible' }}</span>
                <template #fallback>
                  <USkeleton class="h-4 w-32 ml-1" />
                </template>
              </ClientOnly>
            </li>
            <li class="text-gray-600 dark:text-gray-400">
              <span class="font-medium">Tipo:</span> 
              <ClientOnly>
                <UBadge 
                  :color="isEmpresaPrincipal ? 'blue' : 'green'" 
                  variant="subtle" 
                  size="xs"
                  class="ml-1"
                >
                  {{ empresa?.tipo_empresa?.toUpperCase() || 'N/A' }}
                </UBadge>
                <template #fallback>
                  <USkeleton class="h-5 w-20 ml-1 rounded" />
                </template>
              </ClientOnly>
            </li>
            <li class="text-gray-600 dark:text-gray-400">
              <span class="font-medium">Usuario:</span> 
              <ClientOnly>
                <span class="ml-1">{{ user?.nombre }} {{ user?.apellido }}</span>
                <template #fallback>
                  <USkeleton class="h-4 w-28 ml-1" />
                </template>
              </ClientOnly>
            </li>
          </ul>
        </div>
      </div>

      <!-- Línea divisoria -->
      <div class="mt-8 pt-6 border-t border-gray-200 dark:border-gray-700">
        <div class="flex flex-col md:flex-row justify-between items-center">
          <!-- Copyright -->
          <div class="flex items-center space-x-4">
            <p class="text-sm text-gray-500 dark:text-gray-400">
              &copy; {{ currentYear }} Dulce y Salado. Todos los derechos reservados.
            </p>
          </div>

          <!-- Enlaces adicionales y estado -->
          <div class="flex items-center space-x-6 mt-4 md:mt-0">
            <!-- Indicador de conexión -->
            <div class="flex items-center space-x-2">
              <div class="h-2 w-2 bg-green-500 rounded-full animate-pulse"></div>
              <span class="text-xs text-gray-500 dark:text-gray-400">Conectado</span>
            </div>
            
            <!-- Links de soporte -->
            <div class="flex items-center space-x-4 text-xs">
              <button 
                class="text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 transition-colors"
                @click="showSupportInfo"
              >
                Soporte
              </button>
              <span class="text-gray-300 dark:text-gray-600">|</span>
              <button 
                class="text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 transition-colors"
                @click="showPrivacyInfo"
              >
                Privacidad
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Información adicional para desarrollo -->
      <div v-if="isDevelopment" class="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
        <details class="text-xs text-gray-500 dark:text-gray-400">
          <summary class="cursor-pointer hover:text-gray-700 dark:hover:text-gray-300 mb-2">
            Información de desarrollo
          </summary>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-xs">
            <div>
              <span class="font-medium">Entorno:</span> {{ isDevelopment ? 'Desarrollo' : 'Producción' }}
            </div>
            <div>
              <span class="font-medium">Tema:</span> 
              <ClientOnly>
                {{ colorMode.preference }}
                <template #fallback>
                  <USkeleton class="h-4 w-16" />
                </template>
              </ClientOnly>
            </div>
            <div>
              <span class="font-medium">Ruta:</span> {{ $route.path }}
            </div>
            <div>
              <span class="font-medium">Tiempo:</span> 
              <ClientOnly>
                {{ formatTime(new Date()) }}
                <template #fallback>
                  <USkeleton class="h-4 w-12" />
                </template>
              </ClientOnly>
            </div>
          </div>
        </details>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
const { user, empresa, userPermissions, isEmpresaPrincipal } = useAuth()
const colorMode = useColorMode()
const toast = useToast()

// Variables reactivas
const isDevelopment = process.env.NODE_ENV === 'development'
const currentYear = new Date().getFullYear()

// Métodos
const showSupportInfo = () => {
  toast.add({
    title: 'Información de Soporte',
    description: 'Para soporte técnico, contacta a tu administrador del sistema.',
    icon: 'i-heroicons-information-circle',
    color: 'blue'
  })
}

const showPrivacyInfo = () => {
  toast.add({
    title: 'Política de Privacidad',
    description: 'Los datos se manejan conforme a las políticas de privacidad de la empresa.',
    icon: 'i-heroicons-shield-check',
    color: 'green'
  })
}

const formatTime = (date: Date) => {
  return date.toLocaleTimeString('es-ES', {
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>