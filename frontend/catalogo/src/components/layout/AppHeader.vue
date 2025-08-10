<!-- AppHeader.vue - Header con Tailwind CSS -->
<template>
  <div>
    <!-- Main Header -->
    <header class="fixed top-0 left-0 right-0 z-50 h-12 md:h-14 lg:h-16 bg-white shadow-md border-b border-gray-200">
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
          
          <!-- Search Bar (centro del header) -->
          <div 
            class="hidden md:block flex-1 max-w-lg mx-4 mt-1"
          >
            <SearchBar 
              v-model="searchQuery"
              @search="handleSearch"
              @searchWithScroll="handleSearchWithScroll"
              placeholder="Buscar productos..."
              compact
            />
          </div>
          
          <!-- Navegación desktop -->
          <nav class="hidden lg:flex items-start gap-6 mt-1">
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
          
          <!-- Cart Button (solo si está habilitado) -->
          <div v-if="allowOrders" class="hidden lg:block">
            <FloatingCart 
              :always-show="true" 
              :compact="true"
              @open-summary="$emit('openCartSummary')"
              @open-export="$emit('openExportOptions')"
            />
          </div>
          
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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { useCatalogStore } from '@/stores/catalog'
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

// State
const searchQuery = ref('')
const showMobileMenu = ref(false)

// Computed
const showSearch = computed(() => {
  return ['catalogo', 'ofertas'].includes(route.name as string)
})

const allowOrders = computed(() => EMPRESA_CONFIG.permitirPedidos)

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

// Watch for search query changes from store
watch(() => catalogStore.searchQuery, (newQuery) => {
  searchQuery.value = newQuery
})

onMounted(() => {
  searchQuery.value = catalogStore.searchQuery
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
</style>