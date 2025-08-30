<template>
  <UDropdownMenu 
    :items="userMenuItems" 
    :ui="{
      content: 'w-72',

    }"
  >
    <UButton 
      color="neutral" 
      variant="ghost" 
      class="!p-0.5 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-full transition-colors cursor-pointer"
    >
      <UAvatar
        :alt="userDisplayName"
        icon="i-heroicons-user-circle"
        size="sm"
      />
    </UButton>
    
    <!-- Template para cuenta de usuario -->
    <template #custom-account>
      <div class="px-3 py-3 border-b border-gray-200 w-full flex dark:border-gray-700 cursor-default">
        <div class="flex items-center space-x-3">
          <UAvatar
            :alt="userDisplayName"
            icon="i-heroicons-user-circle"
            size="md"
          />
          <div class="flex-1 min-w-0">
            <p class="text-sm font-semibold text-gray-900 dark:text-white truncate">
              {{ userDisplayName }}
            </p>
            <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
              {{ user?.email }}
            </p>
            <div class="flex items-center gap-1.5 mt-1.5">
              <UBadge 
                :color="getRoleColor(user?.rol)" 
                variant="subtle" 
                size="xs"
              >
                {{ user?.rol?.toUpperCase() }}
              </UBadge>
              <UBadge 
                :color="isEmpresaPrincipal ? 'primary' : 'success'" 
                variant="subtle" 
                size="xs"
              >
                {{ empresa?.tipo_empresa?.toUpperCase() }}
              </UBadge>
            </div>
          </div>
        </div>
      </div>
    </template>

    <!-- Template para empresa -->
    <template #custom-empresa>
      <div class="px-3 py-2.5 bg-gray-50 w-full flex dark:bg-gray-800/50 border-b border-gray-200 dark:border-gray-700 cursor-default">
        <div class="flex items-center gap-2">
          <div class="flex-shrink-0">
            <div class="w-8 h-8 bg-primary-100 dark:bg-primary-900/20 rounded-lg flex items-center justify-center">
              <UIcon name="i-heroicons-building-office" class="w-4 h-4 text-primary-600 dark:text-primary-400" />
            </div>
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 dark:text-white truncate">
              {{ empresa?.nombre }}
            </p>
            <p class="text-xs text-gray-500 dark:text-gray-400">
              Código: {{ empresa?.codigo }}
            </p>
          </div>
        </div>
      </div>
    </template>

  </UDropdownMenu>
</template>

<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

const { user, empresa, logout, isEmpresaPrincipal, userPermissions } = useAuth()

// Nombre para mostrar del usuario
const userDisplayName = computed(() => {
  if (!user.value) return 'Usuario'
  return `${user.value.nombre} ${user.value.apellido}`.trim()
})

const getRoleColor = (role: string | undefined) => {
  const colors: Record<string, 'primary' | 'error' | 'info' | 'neutral'> = {
    'admin': 'error',
    'editor': 'primary',
    'viewer': 'info',
    'default': 'neutral'
  }
  return colors[role || 'default'] || colors.default 
}

// Elementos del menú adaptados al nuevo formato
const userMenuItems = computed<DropdownMenuItem[][]>(() => [
  // Sección de cuenta
  [
    {
      label: '',
      slot: 'custom-account',
      disabled: true
    }
  ],
  // Sección de empresa
  [
    {
      label: '',
      slot: 'custom-empresa',
      disabled: true
    }
  ],
  // Navegación principal
  [
    {
      label: 'Dashboard',
      icon: 'i-heroicons-home',
      to: '/',
      shortcuts: ['D']
    },
    {
      label: 'Pedidos',
      icon: 'i-heroicons-shopping-bag',
      to: '/admin/pedidos',
      shortcuts: ['P']
    },
    {
      label: 'Mi Perfil',
      icon: 'i-heroicons-user',
      to: '/profile',
      shortcuts: ['U']
    }
  ],
  // Gestión (solo empresa principal)
  ...(isEmpresaPrincipal.value ? [
    [
      {
        label: 'Listas de Precios',
        icon: 'i-heroicons-currency-dollar',
        to: '/admin/listas-precios',
        shortcuts: ['L']
      }
    ]
  ] : []),
  // Cerrar sesión
  [
    {
      label: 'Cerrar Sesión',
      icon: 'i-heroicons-arrow-right-on-rectangle',
      class: 'text-red-600 dark:text-red-400 hover:text-red-800 dark:hover:text-red-300 transition-colors cursor-pointer',
      to: '/auth/logout',
      color: 'error' as const
    }
  ]
])
</script>