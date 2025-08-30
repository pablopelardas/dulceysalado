<template>
  <header class="border-b border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 shadow-sm transition-colors duration-200">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between items-center h-16">
        <!-- Logo y título -->
        <div class="flex items-center">
          <div class="flex-shrink-0">
            <UIcon name="i-heroicons-building-office" class="h-8 w-8 text-blue-600 dark:text-blue-400" />
          </div>
          <div class="ml-3">
            <h1 class="text-xl font-semibold text-gray-900 dark:text-gray-100">
              Dulce y Salado Admin
            </h1>
          </div>
        </div>

        <!-- Navegación central -->
        <nav class="hidden md:flex space-x-8">
          <NuxtLink 
            to="/" 
            class="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100 px-3 py-2 text-sm font-medium transition-colors"
            active-class="text-blue-600 dark:text-blue-400 border-b-2 border-blue-600 dark:border-blue-400"
          >
            Dashboard
          </NuxtLink>
          <NuxtLink 
            to="/admin/pedidos" 
            class="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100 px-3 py-2 text-sm font-medium transition-colors"
            active-class="text-blue-600 dark:text-blue-400 border-b-2 border-blue-600 dark:border-blue-400"
          >
            Pedidos
          </NuxtLink>
          <NuxtLink 
            to="/profile" 
            class="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100 px-3 py-2 text-sm font-medium transition-colors"
            active-class="text-blue-600 dark:text-blue-400 border-b-2 border-blue-600 dark:border-blue-400"
          >
            Mi Perfil
          </NuxtLink>
        </nav>

        <!-- Menú de usuario -->
        <div class="flex items-center space-x-2">
          <!-- Información de empresa para empresa cliente -->
          <ClientOnly>
            <div v-if="empresa && !isEmpresaPrincipal" class="hidden sm:block text-right mr-2">
              <p class="text-sm font-medium text-gray-900 dark:text-gray-100">{{ empresa.nombre }}</p>
              <p class="text-xs text-gray-500 dark:text-gray-400">{{ empresa.codigo }}</p>
            </div>
          </ClientOnly>

          <!-- Menú rápido de cuentas guardadas (solo desarrollo) -->
          <ClientOnly>
            <div v-if="isDevelopment && savedAccounts.length > 0" class="hidden md:block mr-2 min-w-[180px]">
              <USelectMenu
                v-model="selectedAccount"
                :items="savedAccountOptions"
                placeholder="Cambiar cuenta (dev)"
                icon="i-heroicons-user-circle"
                size="sm"
                @update:modelValue="onSelectAccount"
              />
            </div>
          </ClientOnly>
          <!-- Theme Toggle -->
          <ThemeToggle />

          <!-- User Menu -->
          <AuthUserMenu />
        </div>
      </div>
    </div>

    <!-- Navegación móvil -->
    <div class="md:hidden border-t border-gray-200 dark:border-gray-700">
      <div class="px-2 pt-2 pb-3 space-y-1">
        <NuxtLink 
          to="/" 
          class="block px-3 py-2 text-base font-medium text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100 hover:bg-gray-50 dark:hover:bg-gray-700 rounded-md transition-colors"
          active-class="text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/50"
        >
          Dashboard
        </NuxtLink>
        <NuxtLink 
          to="/admin/pedidos" 
          class="block px-3 py-2 text-base font-medium text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100 hover:bg-gray-50 dark:hover:bg-gray-700 rounded-md transition-colors"
          active-class="text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/50"
        >
          Pedidos
        </NuxtLink>
        <NuxtLink 
          to="/profile" 
          class="block px-3 py-2 text-base font-medium text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100 hover:bg-gray-50 dark:hover:bg-gray-700 rounded-md transition-colors"
          active-class="text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/50"
        >
          Mi Perfil
        </NuxtLink>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import ThemeToggle from './ThemeToggle.vue'

const { empresa, isEmpresaPrincipal } = useAuth()

// Interfaces para cuentas guardadas (igual que en AuthLoginForm)
interface SavedAccount {
  email: string
  password: string
  lastUsed: string
  displayName?: string
}

// Variables de desarrollo para cambio rápido de cuenta
const isDevelopment = process.env.NODE_ENV === 'development'
const STORAGE_KEY = 'dev-login-accounts'
const savedAccounts = ref<SavedAccount[]>([])
const selectedAccount = ref<{ label: string; value: string; description: string } | undefined>(undefined)

const savedAccountOptions = computed(() => {
  return savedAccounts.value.map(account => ({
    label: account.displayName || account.email,
    value: account.email,
    description: `Último uso: ${new Date(account.lastUsed).toLocaleDateString()}`
  }))
})

const loadSavedAccounts = () => {
  if (!isDevelopment || typeof window === 'undefined') return
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored) {
      savedAccounts.value = JSON.parse(stored)
    }
  } catch (error) {
    savedAccounts.value = []
  }
}

const onSelectAccount = async (value: { label: string; value: string; description: string } | undefined) => {
  if (!value) return
  const email = value.value
  const account = savedAccounts.value.find(acc => acc.email === email)
  if (account) {
    // Simular login rápido cambiando el usuario en el store
    if (typeof useAuth === 'function') {
      const auth = useAuth()
      if (auth && typeof auth.login === 'function') {
        await auth.login({ email: account.email, password: account.password })
      }
    }
    selectedAccount.value = value
  }
}

onMounted(() => {
  loadSavedAccounts()
})
</script>