<!-- HomeHeader.vue - Header específico para la página de inicio -->
<template>
  <header class="fixed top-0 left-0 right-0 z-50 h-14 lg:h-16 bg-white/95 backdrop-blur-sm shadow-md border-b border-gray-200 pointer-events-none">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pointer-events-auto">
      <!-- Desktop Layout -->
      <div class="hidden lg:flex items-start justify-between h-full gap-2 lg:gap-4 pt-2">
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
        
        <!-- Navegación desktop -->
        <nav class="hidden md:flex items-start gap-3 lg:gap-6 mt-1 pointer-events-auto">
          <a 
            href="#categorias"
            @click.prevent="scrollToSection('categorias')"
            class="font-semibold text-sm lg:text-base text-gray-900 px-2 lg:px-3 py-2 rounded transition-all duration-200 hover:text-red-600 hover:bg-gray-50 whitespace-nowrap"
          >
            Categorías
          </a>
          <a 
            href="#novedades"
            @click.prevent="scrollToSection('novedades')"
            class="font-semibold text-sm lg:text-base text-gray-900 px-2 lg:px-3 py-2 rounded transition-all duration-200 hover:text-red-600 hover:bg-gray-50 whitespace-nowrap"
          >
            Novedades
          </a>
          <a 
            href="#ubicacion"
            @click.prevent="scrollToSection('ubicacion')"
            class="font-semibold text-sm lg:text-base text-gray-900 px-2 lg:px-3 py-2 rounded transition-all duration-200 hover:text-red-600 hover:bg-gray-50 whitespace-nowrap"
          >
            Ubicación
          </a>
          <RouterLink 
            to="/catalogo" 
            class="inline-flex items-center gap-1 lg:gap-2 px-3 lg:px-4 py-2 text-white font-semibold text-sm lg:text-base rounded-lg transition-colors duration-200 whitespace-nowrap"
            :style="{ backgroundColor: 'var(--theme-accent)' }"
            @mouseenter="$event.target.style.backgroundColor = '#CC0000'"
            @mouseleave="$event.target.style.backgroundColor = 'var(--theme-accent)'"
          >
            <span class="hidden lg:inline">Ver </span>Catálogo
            <svg class="w-3 h-3 lg:w-4 lg:h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3"/>
            </svg>
          </RouterLink>
        </nav>
        
        <!-- Auth Section -->
        <div class="hidden lg:flex items-start gap-4 mt-1 pointer-events-auto">
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
      <div class="lg:hidden flex items-center justify-between h-full gap-3 py-3">
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
          class="flex items-center justify-center w-10 h-10 text-gray-900 rounded hover:bg-gray-100 transition-colors duration-200 cursor-pointer"
          @click="closeMobileMenu"
          aria-label="Cerrar menú"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
          </svg>
        </button>
      </div>

      <div class="flex-1 py-4">
        <a 
          href="#categorias"
          @click="handleMobileNavClick('categorias')"
          class="flex items-center gap-3 px-6 py-4 text-gray-900 font-medium text-lg transition-all duration-200 border-l-4 border-transparent hover:bg-gray-50 hover:border-red-600"
        >
          <span class="text-xl">📋</span>
          Categorías
        </a>
        <a 
          href="#novedades"
          @click="handleMobileNavClick('novedades')"
          class="flex items-center gap-3 px-6 py-4 text-gray-900 font-medium text-lg transition-all duration-200 border-l-4 border-transparent hover:bg-gray-50 hover:border-red-600"
        >
          <span class="text-xl">🆕</span>
          Novedades
        </a>
        <a 
          href="#ubicacion"
          @click="handleMobileNavClick('ubicacion')"
          class="flex items-center gap-3 px-6 py-4 text-gray-900 font-medium text-lg transition-all duration-200 border-l-4 border-transparent hover:bg-gray-50 hover:border-red-600"
        >
          <span class="text-xl">📍</span>
          Ubicación
        </a>
        <RouterLink 
          to="/catalogo" 
          class="flex items-center gap-3 px-6 py-4 text-gray-900 font-medium text-lg transition-all duration-200 border-l-4 border-transparent hover:bg-gray-50 hover:border-red-600"
          @click="closeMobileMenu"
        >
          <span class="text-xl">🛒</span>
          Ver Catálogo
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
        
        <!-- Search en móvil -->
        <div class="mb-4">
          <h3 class="text-sm font-semibold text-gray-700 mb-2">Buscar productos</h3>
          <SearchBar 
            v-model="searchQuery"
            @search="handleMobileSearch"
            placeholder="Buscar productos..."
          />
        </div>
      </div>
    </nav>
  </Transition>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { EMPRESA_CONFIG } from '@/config/empresa.config'
import SearchBar from '@/components/catalog/SearchBar.vue'

// State
const showMobileMenu = ref(false)
const showUserMenu = ref(false)
const searchQuery = ref('')
const router = useRouter()

// Stores
const authStore = useAuthStore()

// Methods
const scrollToSection = (sectionId: string) => {
  const element = document.getElementById(sectionId)
  if (element) {
    const yOffset = -100 // Account for fixed header height
    const y = element.getBoundingClientRect().top + window.pageYOffset + yOffset
    
    window.scrollTo({
      top: y,
      behavior: 'smooth'
    })
  }
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

const handleMobileNavClick = (sectionId: string) => {
  closeMobileMenu()
  setTimeout(() => scrollToSection(sectionId), 100)
}

const handleMobileSearch = () => {
  closeMobileMenu()
  // Redirect to catalog with search query
  router.push({
    path: '/catalogo',
    query: { q: searchQuery.value }
  })
}

const toggleUserMenu = () => {
  showUserMenu.value = !showUserMenu.value
}

const handleLogout = async () => {
  await authStore.logout()
  showUserMenu.value = false
  closeMobileMenu()
}

// Initialize authentication
onMounted(() => {
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