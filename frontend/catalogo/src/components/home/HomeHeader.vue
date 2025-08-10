<!-- HomeHeader.vue - Header específico para la página de inicio -->
<template>
  <header class="fixed top-0 left-0 right-0 z-50 h-12 md:h-14 lg:h-16 bg-white/95 backdrop-blur-sm shadow-md border-b border-gray-200">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-start justify-between h-full gap-4 pt-2">
        <!-- Logo y nombre de empresa -->
        <RouterLink to="/" class="flex items-start gap-4 text-decoration-none transition-transform duration-200 hover:-translate-y-px relative z-10">
          <img 
            :src="EMPRESA_CONFIG.logoUrl" 
            :alt="EMPRESA_CONFIG.nombre"
            class="h-16 md:h-20 lg:h-32 w-auto max-w-[200px] md:max-w-[260px] lg:max-w-[400px] object-contain drop-shadow-lg"
          />
          <div class="hidden sm:block text-gray-900 mt-1">
            <h1 class="font-bold text-xl md:text-2xl lg:text-3xl">
              {{ EMPRESA_CONFIG.nombre }}
            </h1>
          </div>
        </RouterLink>
        
        <!-- Navegación desktop -->
        <nav class="hidden lg:flex items-start gap-6 mt-1">
          <a 
            href="#categorias"
            @click.prevent="scrollToSection('categorias')"
            class="font-semibold text-base text-gray-900 px-3 py-2 rounded transition-all duration-200 hover:text-red-600 hover:bg-gray-50"
          >
            Categorías
          </a>
          <a 
            href="#novedades"
            @click.prevent="scrollToSection('novedades')"
            class="font-semibold text-base text-gray-900 px-3 py-2 rounded transition-all duration-200 hover:text-red-600 hover:bg-gray-50"
          >
            Novedades
          </a>
          <a 
            href="#ubicacion"
            @click.prevent="scrollToSection('ubicacion')"
            class="font-semibold text-base text-gray-900 px-3 py-2 rounded transition-all duration-200 hover:text-red-600 hover:bg-gray-50"
          >
            Ubicación
          </a>
          <RouterLink 
            to="/catalogo" 
            class="inline-flex items-center gap-2 px-4 py-2 text-white font-semibold rounded-lg transition-colors duration-200"
            :style="{ backgroundColor: 'var(--theme-accent)' }"
            @mouseenter="$event.target.style.backgroundColor = '#CC0000'"
            @mouseleave="$event.target.style.backgroundColor = 'var(--theme-accent)'"
          >
            Ver Catálogo
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3"/>
            </svg>
          </RouterLink>
        </nav>
        
        <!-- Menú hamburguesa móvil -->
        <button 
          class="lg:hidden flex flex-col justify-center items-center w-10 h-10 p-2 rounded hover:bg-gray-50 transition-colors duration-200 mt-1"
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

      <!-- Search en móvil -->
      <div class="p-4 border-t border-gray-200">
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
import { ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { EMPRESA_CONFIG } from '@/config/empresa.config'
import SearchBar from '@/components/catalog/SearchBar.vue'

// State
const showMobileMenu = ref(false)
const searchQuery = ref('')
const router = useRouter()

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
</style>