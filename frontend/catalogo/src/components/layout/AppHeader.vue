<!-- AppHeader.vue - Header con Tailwind CSS -->
<template>
  <div>
    <!-- Main Header -->
    <header class="fixed top-0 left-0 right-0 z-50 h-14 lg:h-16 bg-white shadow-md border-b border-gray-200 pointer-events-none">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pointer-events-auto">
        <!-- Desktop Layout -->
        <div class="hidden md:flex items-start justify-between h-full gap-2 lg:gap-4 pt-2">
          <!-- Logo y nombre de empresa -->
          <div class="flex items-start gap-2 lg:gap-4 relative z-10 flex-shrink min-w-0">
            <RouterLink to="/" class="text-decoration-none transition-transform duration-200 hover:-translate-y-px pointer-events-auto absolute top-0 left-4 z-20">
              <img 
                :src="EMPRESA_CONFIG.logoUrl" 
                :alt="EMPRESA_CONFIG.nombre"
                class="h-16 md:h-20 lg:h-32 w-auto max-w-[200px] md:max-w-[260px] lg:max-w-[400px] object-contain drop-shadow-lg"
              />
            </RouterLink>
            <RouterLink to="/" class="hidden sm:block text-gray-900 mt-1 text-decoration-none transition-all duration-200 hover:text-red-600 pointer-events-auto ml-16 md:ml-20 lg:ml-32 xl:ml-40 flex-shrink min-w-0">
              <h1 class="font-bold text-lg md:text-xl lg:text-2xl xl:text-3xl whitespace-nowrap overflow-hidden text-ellipsis">
                {{ EMPRESA_CONFIG.nombre }}
              </h1>
            </RouterLink>
          </div>
          
          <!-- Search Bar (centro del header) -->
          <div class="flex-1 max-w-lg mx-4 mt-1 pointer-events-auto">
            <SearchBar 
              v-model="searchQuery"
              @search="handleSearch"
              @searchWithScroll="handleSearchWithScroll"
              placeholder="Buscar productos..."
              compact
            />
          </div>
          
          <!-- Navegación desktop -->
          <nav class="hidden lg:flex items-start gap-6 mt-1 pointer-events-auto">
            <RouterLink 
              to="/" 
              class="relative font-semibold text-base text-gray-900 px-3 py-2 rounded transition-all duration-200 hover:text-red-600 hover:bg-gray-50"
              :class="{ 'text-red-600': $route.name === 'home' }"
            >
              Inicio
              <span 
                v-if="$route.name === 'home'"
                class="absolute bottom-[-8px] left-1/2 transform -translate-x-1/2 w-[30px] h-[3px] rounded-full"
                :style="{ backgroundColor: 'var(--theme-accent)' }"
              ></span>
            </RouterLink>
            <RouterLink 
              to="/catalogo" 
              class="relative font-semibold text-base text-gray-900 px-3 py-2 rounded transition-all duration-200 hover:text-red-600 hover:bg-gray-50"
              :class="{ 'text-red-600': $route.name === 'catalogo' }"
            >
              Catálogo
              <span 
                v-if="$route.name === 'catalogo'"
                class="absolute bottom-[-8px] left-1/2 transform -translate-x-1/2 w-[30px] h-[3px] rounded-full"
                :style="{ backgroundColor: 'var(--theme-accent)' }"
              ></span>
            </RouterLink>
          </nav>
          
          <!-- Auth Section -->
          <div class="hidden lg:flex items-start gap-4 mt-1 pointer-events-auto">
            <!-- Cart Button (solo si está habilitado) -->
            <div v-if="allowOrders">
              <FloatingCart
                :compact="true"
                @open-summary="$emit('openCartSummary')"
                @open-export="$emit('openExportOptions')"
              />
            </div>

            <!-- User Menu -->
            <div v-if="authStore.isAuthenticated" class="relative">
              <button
                @click="toggleUserMenu"
                class="flex items-center gap-2 px-3 py-2 rounded-lg bg-gray-100 hover:bg-gray-200 transition-colors duration-200 cursor-pointer"
              >
                <div class="w-8 h-8 bg-gray-300 rounded-full flex items-center justify-center">
                  <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
                  </svg>
                </div>
                <span class="text-sm font-medium text-gray-900 hidden xl:block">
                  {{ authStore.user?.nombre }}
                </span>
                <svg class="w-4 h-4 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
                </svg>
              </button>

              <!-- User Dropdown -->
              <Transition name="dropdown">
                <div 
                  v-if="showUserMenu"
                  class="absolute right-0 top-full mt-2 w-48 bg-white rounded-lg shadow-lg border border-gray-200 py-2 z-50"
                >
                  <div class="px-4 py-2 border-b border-gray-100">
                    <p class="text-sm font-medium text-gray-900">{{ authStore.userFullName }}</p>
                    <p class="text-xs text-gray-500">{{ authStore.user?.email }}</p>
                  </div>
                  <RouterLink to="/perfil" class="flex items-center gap-2 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50" @click="showUserMenu = false">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
                    </svg>
                    Mi Perfil
                  </RouterLink>
                  <RouterLink to="/pedidos" class="flex items-center gap-2 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50" @click="showUserMenu = false">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
                    </svg>
                    Mis Pedidos
                  </RouterLink>
                  <button 
                    @click="handleLogout"
                    class="flex items-center gap-2 w-full px-4 py-2 text-sm text-red-600 hover:bg-red-50 cursor-pointer"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
                    </svg>
                    Cerrar Sesión
                  </button>
                </div>
              </Transition>
            </div>

            <!-- Login Button -->
            <div v-else class="flex items-center gap-2">
              <RouterLink 
                to="/login"
                class="px-4 py-2 text-sm font-medium text-white rounded-lg transition-colors duration-200 hover:opacity-90"
                :style="{ backgroundColor: 'var(--theme-accent)' }"
              >
                Ingresar
              </RouterLink>
            </div>
          </div>
          
        </div>
        
        <!-- Mobile Layout -->
        <div class="md:hidden flex items-center justify-between h-full gap-3 py-3">
          <!-- Logo chico -->
          <RouterLink to="/" class="flex-shrink-0">
            <img 
              :src="EMPRESA_CONFIG.logoUrl" 
              :alt="EMPRESA_CONFIG.nombre"
              class="h-8 w-auto object-contain"
            />
          </RouterLink>
          
          <!-- Barra de búsqueda móvil -->
          <div class="flex-1 mx-4">
            <SearchBar 
              v-model="searchQuery"
              @search="handleMobileSearch"
              placeholder="Buscar..."
              compact
            />
          </div>
          
          <!-- Carrito compacto móvil (solo si se permiten pedidos y no es la vista de inicio) -->
          <div v-if="allowOrders" class="flex-shrink-0">
            <FloatingCart
              compact
              @open-summary="emit('openCartSummary')"
              @open-export="emit('openExportOptions')"
            />
          </div>
          
          <!-- Menú hamburguesa -->
          <button 
            class="flex-shrink-0 flex flex-col justify-center items-center w-10 h-10 p-2 rounded hover:bg-gray-50 transition-colors duration-200 cursor-pointer"
            @click="toggleMobileMenu"
            aria-label="Abrir menú"
          >
            <span class="w-6 h-0.5 bg-gray-900 rounded-full transition-all duration-200 mb-1"></span>
            <span class="w-6 h-0.5 bg-gray-900 rounded-full transition-all duration-200 mb-1"></span>
            <span class="w-6 h-0.5 bg-gray-900 rounded-full transition-all duration-200"></span>
          </button>
        </div>
      </div>
    </header>

    <!-- Overlay para menú móvil -->
    <Transition name="overlay">
      <div 
        v-if="showMobileMenu" 
        class="fixed inset-0 bg-black/50 z-40 transition-opacity duration-200"
        @click="closeMobileMenu"
      />
    </Transition>

    <!-- Menú móvil -->
    <Transition name="mobile-menu">
      <nav 
        v-if="showMobileMenu" 
        class="fixed top-0 right-0 w-full max-w-xs h-full bg-white z-50 shadow-2xl flex flex-col transition-transform duration-300"
      >
        <div class="flex items-center justify-between p-4 border-b border-gray-200">
          <img 
            :src="EMPRESA_CONFIG.logoUrl" 
            :alt="EMPRESA_CONFIG.nombre"
            class="h-8 w-auto object-contain"
          />
          <button 
            class="flex items-center justify-center w-10 h-10 text-gray-900 rounded hover:bg-gray-100 transition-colors duration-200"
            @click="closeMobileMenu"
            aria-label="Cerrar menú"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>

        <div class="flex-1 py-4">
          <RouterLink 
            to="/" 
            class="flex items-center gap-3 px-6 py-4 text-gray-900 font-medium text-lg transition-all duration-200 border-l-4 border-transparent hover:bg-gray-50 hover:border-red-600"
            :class="{ 'bg-gray-50 border-red-600 text-red-600': $route.name === 'home' }"
            @click="closeMobileMenu"
          >
            <span class="text-xl">🏠</span>
            Inicio
          </RouterLink>
          <RouterLink 
            to="/catalogo" 
            class="flex items-center gap-3 px-6 py-4 text-gray-900 font-medium text-lg transition-all duration-200 border-l-4 border-transparent hover:bg-gray-50 hover:border-red-600"
            :class="{ 'bg-gray-50 border-red-600 text-red-600': $route.name === 'catalogo' }"
            @click="closeMobileMenu"
          >
            <span class="text-xl">📋</span>
            Catálogo
          </RouterLink>
        </div>

        <!-- Authentication section for mobile -->
        <div class="p-4 border-t border-gray-200">
          <!-- User authenticated -->
          <div v-if="authStore.isAuthenticated" class="mb-4">
            <div class="flex items-center gap-3 p-3 bg-gray-50 rounded-lg mb-3">
              <div class="w-10 h-10 bg-gray-300 rounded-full flex items-center justify-center">
                <svg class="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
                </svg>
              </div>
              <div class="flex-1">
                <p class="text-sm font-medium text-gray-900">{{ authStore.userFullName }}</p>
                <p class="text-xs text-gray-500">{{ authStore.user?.email }}</p>
              </div>
            </div>
            <RouterLink 
              to="/perfil" 
              class="flex items-center gap-3 px-4 py-3 text-gray-900 font-medium transition-all duration-200 border-l-4 border-transparent hover:bg-gray-50 hover:border-red-600 rounded-lg mb-2"
              @click="closeMobileMenu"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
              </svg>
              Mi Perfil
            </RouterLink>
            <RouterLink 
              to="/pedidos" 
              class="flex items-center gap-3 px-4 py-3 text-gray-900 font-medium transition-all duration-200 border-l-4 border-transparent hover:bg-gray-50 hover:border-red-600 rounded-lg mb-2"
              @click="closeMobileMenu"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
              </svg>
              Mis Pedidos
            </RouterLink>
            <button 
              @click="handleLogout"
              class="flex items-center gap-3 w-full px-4 py-3 text-red-600 font-medium transition-all duration-200 hover:bg-red-50 rounded-lg cursor-pointer"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
              </svg>
              Cerrar Sesión
            </button>
          </div>
          
          <!-- User not authenticated -->
          <div v-else class="mb-4">
            <RouterLink 
              to="/login"
              class="flex items-center justify-center gap-2 w-full px-4 py-3 text-white font-medium rounded-lg transition-all duration-200 hover:opacity-90"
              :style="{ backgroundColor: 'var(--theme-accent)' }"
              @click="closeMobileMenu"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1"/>
              </svg>
              Ingresar
            </RouterLink>
          </div>
        </div>
      </nav>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { useCatalogStore } from '@/stores/catalog'
import { useAuthStore } from '@/stores/auth'
import EMPRESA_CONFIG from '@/config/empresa.config'
import SearchBar from '@/components/catalog/SearchBar.vue'
import FloatingCart from '@/components/cart/FloatingCart.vue'

// Emits
const emit = defineEmits<{
  openCartSummary: []
  openExportOptions: []
}>()

// Router
const route = useRoute()

// Stores
const catalogStore = useCatalogStore()
const authStore = useAuthStore()

// State
const searchQuery = ref('')
const showMobileMenu = ref(false)
const showUserMenu = ref(false)

// showSearch computed removed - not used

const allowOrders = computed(() => {
  // Solo permitir pedidos si está habilitado Y no estamos en la vista de inicio
  const isHomeRoute = route.name === 'home'
  return EMPRESA_CONFIG.permitirPedidos && !isHomeRoute
})

// Methods
const handleSearch = async () => {
  await catalogStore.setSearch(searchQuery.value)
}

const handleSearchWithScroll = async () => {
  await catalogStore.setSearch(searchQuery.value)
  scrollToProducts()
}

const handleMobileSearch = async () => {
  await handleSearch()
  closeMobileMenu()
  scrollToProducts()
}

const scrollToProducts = () => {
  requestAnimationFrame(() => {
    const toolbarElement = document.querySelector('.products-toolbar')
    if (toolbarElement) {
      const yOffset = -100
      const y = toolbarElement.getBoundingClientRect().top + window.pageYOffset + yOffset
      
      window.scrollTo({
        top: y,
        behavior: 'smooth'
      })
    }
  })
}

const toggleMobileMenu = () => {
  showMobileMenu.value = !showMobileMenu.value
  
  // Prevent body scroll when menu is open
  if (showMobileMenu.value) {
    document.body.style.overflow = 'hidden'
  } else {
    document.body.style.overflow = ''
  }
}

const closeMobileMenu = () => {
  showMobileMenu.value = false
  document.body.style.overflow = ''
}

const toggleUserMenu = () => {
  showUserMenu.value = !showUserMenu.value
}

const handleLogout = async () => {
  await authStore.logout()
  showUserMenu.value = false
}

// Watch for search query changes from store
watch(() => catalogStore.searchQuery, (newQuery) => {
  searchQuery.value = newQuery
})

onMounted(() => {
  searchQuery.value = catalogStore.searchQuery
  // Inicializar autenticación
  authStore.initializeAuth()
})
</script>

<style scoped>
/* Transiciones para overlay */
.overlay-enter-active,
.overlay-leave-active {
  transition: opacity 200ms ease;
}

.overlay-enter-from,
.overlay-leave-to {
  opacity: 0;
}

/* Transiciones para menú móvil */
.mobile-menu-enter-active,
.mobile-menu-leave-active {
  transition: transform 300ms ease;
}

.mobile-menu-enter-from,
.mobile-menu-leave-to {
  transform: translateX(100%);
}

/* Transiciones para dropdown de usuario */
.dropdown-enter-active,
.dropdown-leave-active {
  transition: all 200ms ease;
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-10px) scale(0.95);
}
</style>