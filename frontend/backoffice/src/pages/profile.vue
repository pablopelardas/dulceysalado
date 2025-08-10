<script setup lang="ts">
import ChangePasswordModal from '~/components/users/ChangePasswordModal.vue'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Mi Perfil',
  meta: [
    { name: 'description', content: 'Gestiona tu información personal y configuración de cuenta' }
  ]
})

const { user, empresa, userPermissions, isEmpresaPrincipal } = useAuth()

// Estado para modal de cambio de contraseña
const showPasswordModal = ref(false)

// Helper functions
const getRoleColor = (role: string | undefined) => {
  switch (role) {
    case 'admin': return 'red'
    case 'editor': return 'blue'
    case 'viewer': return 'green'
    default: return 'gray'
  }
}

const formatDate = (dateString: string | undefined) => {
  if (!dateString) return 'N/A'
  // Usar toISOString() y luego formatear para evitar problemas de hidratación
  const date = new Date(dateString)
  return date.toLocaleDateString('es-ES', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  }) + ', ' + date.toLocaleTimeString('es-ES', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
}

// Métodos
const handlePasswordChangeSuccess = () => {
  // No necesitamos hacer nada especial aquí para el cambio de contraseña propio
  // El toast ya se muestra desde el modal
}
</script>
<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100 mb-8">
        Mi Perfil
      </h1>

      <!-- Información del usuario -->
      <UCard class="mb-6">
        <template #header>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Información de Usuario</h2>
        </template>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Usuario</p>
            <p class="text-lg text-gray-900 dark:text-gray-100">{{ user?.nombre }} {{ user?.apellido }}</p>
          </div>
          <div>
            <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Email</p>
            <p class="text-lg text-gray-900 dark:text-gray-100">{{ user?.email }}</p>
          </div>
          <div>
            <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Rol</p>
            <UBadge :color="getRoleColor(user?.rol)" variant="subtle">
              {{ user?.rol?.toUpperCase() }}
            </UBadge>
          </div>
          <div>
            <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Último Login</p>
            <ClientOnly>
              <p class="text-lg text-gray-900 dark:text-gray-100">{{ formatDate(user?.ultimo_login) }}</p>
              <template #fallback>
                <p class="text-lg text-gray-500 dark:text-gray-400">Cargando...</p>
              </template>
            </ClientOnly>
          </div>
        </div>
      </UCard>

      <!-- Información de la empresa -->
      <UCard class="mb-6">
        <template #header>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Información de Empresa</h2>
        </template>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Empresa</p>
            <p class="text-lg text-gray-900 dark:text-gray-100">{{ empresa?.nombre }}</p>
          </div>
          <div>
            <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Código</p>
            <p class="text-lg font-mono text-gray-900 dark:text-gray-100">{{ empresa?.codigo }}</p>
          </div>
          <div>
            <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Tipo</p>
            <UBadge :color="isEmpresaPrincipal ? 'blue' : 'green'" variant="subtle">
              {{ empresa?.tipo_empresa?.toUpperCase() }}
            </UBadge>
          </div>
        </div>
      </UCard>

      <!-- Permisos del usuario -->
      <UCard class="mb-6">
        <template #header>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Permisos de Usuario</h2>
        </template>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="flex items-center space-x-2">
            <UIcon 
              :name="userPermissions.canManageProductosBase ? 'i-heroicons-check-circle' : 'i-heroicons-x-circle'"
              :class="userPermissions.canManageProductosBase ? 'text-green-500' : 'text-red-500'"
            />
            <span class="text-gray-900 dark:text-gray-100">Gestionar Productos Base</span>
          </div>
          <div class="flex items-center space-x-2">
            <UIcon 
              :name="userPermissions.canManageProductosEmpresa ? 'i-heroicons-check-circle' : 'i-heroicons-x-circle'"
              :class="userPermissions.canManageProductosEmpresa ? 'text-green-500' : 'text-red-500'"
            />
            <span class="text-gray-900 dark:text-gray-100">Gestionar Productos Empresa</span>
          </div>
          <div class="flex items-center space-x-2">
            <UIcon 
              :name="userPermissions.canManageCategoriasBase ? 'i-heroicons-check-circle' : 'i-heroicons-x-circle'"
              :class="userPermissions.canManageCategoriasBase ? 'text-green-500' : 'text-red-500'"
            />
            <span class="text-gray-900 dark:text-gray-100">Gestionar Categorías Base</span>
          </div>
          <div class="flex items-center space-x-2">
            <UIcon 
              :name="userPermissions.canManageCategoriasEmpresa ? 'i-heroicons-check-circle' : 'i-heroicons-x-circle'"
              :class="userPermissions.canManageCategoriasEmpresa ? 'text-green-500' : 'text-red-500'"
            />
            <span class="text-gray-900 dark:text-gray-100">Gestionar Categorías Empresa</span>
          </div>
          <div class="flex items-center space-x-2">
            <UIcon 
              :name="userPermissions.canManageUsuarios ? 'i-heroicons-check-circle' : 'i-heroicons-x-circle'"
              :class="userPermissions.canManageUsuarios ? 'text-green-500' : 'text-red-500'"
            />
            <span class="text-gray-900 dark:text-gray-100">Gestionar Usuarios</span>
          </div>
          <div class="flex items-center space-x-2">
            <UIcon 
              :name="userPermissions.canViewEstadisticas ? 'i-heroicons-check-circle' : 'i-heroicons-x-circle'"
              :class="userPermissions.canViewEstadisticas ? 'text-green-500' : 'text-red-500'"
            />
            <span class="text-gray-900 dark:text-gray-100">Ver Estadísticas</span>
          </div>
        </div>
      </UCard>

      <!-- Seguridad -->
      <UCard class="mb-6">
        <template #header>
          <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Seguridad</h2>
        </template>

        <div class="space-y-4">
          <div class="flex items-center justify-between p-4 border border-gray-200 dark:border-gray-700 rounded-lg">
            <div>
              <h3 class="text-sm font-medium text-gray-900 dark:text-gray-100">Contraseña</h3>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                Actualiza tu contraseña para mantener tu cuenta segura
              </p>
            </div>
            <UButton
              color="primary"
              variant="outline"
              class="cursor-pointer"
              @click="showPasswordModal = true"
            >
              <UIcon name="i-heroicons-key" class="mr-2" />
              Cambiar Contraseña
            </UButton>
          </div>
        </div>
      </UCard>

      <!-- Acciones -->
      <div class="flex gap-4">
        <UButton to="/" icon="i-heroicons-home" variant="outline">
          Ir al Dashboard
        </UButton>
        <UButton to="/auth/logout" icon="i-heroicons-arrow-right-on-rectangle" color="red">
          Cerrar Sesión
        </UButton>
      </div>
    </div>

    <!-- Modal de cambio de contraseña -->
    <ChangePasswordModal
      v-model="showPasswordModal"
      mode="self"
      :user="user"
      @success="handlePasswordChangeSuccess"
    />
  </div>
</template>