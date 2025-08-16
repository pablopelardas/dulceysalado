<!-- CategoryGrid.vue - Grid de categorías con Tailwind CSS -->
<template>
  <section class="py-16 md:py-12" style="background-color: #404040;">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <!-- Título de la sección -->
      <div class="text-center mb-12 md:mb-8">
        <h2 class="font-extrabold text-3xl md:text-4xl text-white mb-4">
          Explorá nuestras categorías
        </h2>
        <p class="text-lg md:text-base text-gray-300 leading-relaxed max-w-2xl mx-auto">
          Encontrá exactamente lo que buscás navegando por nuestras categorías de productos
        </p>
      </div>

      <!-- Loading state -->
      <div v-if="loading" class="w-full">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 md:gap-4">
          <div 
            v-for="n in 8" 
            :key="n" 
            class="h-48 bg-gradient-to-r from-gray-600 via-gray-500 to-gray-600 bg-[length:200px_100%] bg-no-repeat rounded-xl animate-pulse"
            style="animation: shimmer 1.5s infinite; background-position: -200px 0;"
          ></div>
        </div>
      </div>

      <!-- Grid de categorías -->
      <div v-else-if="categories.length" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 md:gap-4">
        <div
          v-for="category in displayCategories"
          :key="category.id"
          class="relative flex flex-col bg-gray-700 rounded-xl shadow-sm cursor-pointer overflow-hidden transition-all duration-200 hover:shadow-lg hover:-translate-y-1 group"
          @click="handleCategoryClick(category)"
        >
          <!-- Imagen de la categoría -->
          <div 
            class="w-full h-32 flex items-center justify-center transition-transform duration-200 group-hover:scale-105 overflow-hidden"
            :style="!category.imagen_url ? { backgroundColor: category.color || getDefaultColor(displayCategories.indexOf(category)) } : {}"
          >
            <img 
              v-if="category.imagen_url"
              :src="category.imagen_url"
              :alt="category.nombre"
              class="w-full h-full object-cover"
              @error="handleImageError"
            />
            <span 
              v-else
              class="font-bold text-3xl drop-shadow-lg"
              :class="getTextColor(category.color || getDefaultColor(displayCategories.indexOf(category)))"
            >
              {{ category.icono || getCategoryInitial(category.nombre) }}
            </span>
          </div>

          <!-- Información de la categoría -->
          <div class="text-center p-4 flex-1">
            <h3 class="font-bold text-lg text-white leading-tight mb-2">
              {{ category.nombre }}
            </h3>
            
            <div class="flex items-center justify-center gap-2 mb-2">
              <span 
                class="text-xs font-semibold px-2 py-1 bg-gray-600 text-white rounded-full"
              >
                {{ category.product_count }} productos
              </span>
            </div>

            <p v-if="category.descripcion" class="text-xs text-gray-300 leading-normal line-clamp-2">
              {{ category.descripcion }}
            </p>
          </div>

          <!-- Hover effect overlay -->
          <div 
            class="absolute inset-0 bg-white/5 opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none"
          ></div>
        </div>
      </div>

      <!-- Estado vacío -->
      <div v-else class="text-center py-16 md:py-12 px-4">
        <div class="w-16 h-16 mx-auto mb-4 text-gray-400">
          <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"/>
          </svg>
        </div>
        <h3 class="font-bold text-2xl text-gray-900 mb-2">No hay categorías disponibles</h3>
        <p class="text-base text-gray-600 leading-relaxed">
          Las categorías se cargarán cuando estén disponibles
        </p>
      </div>

      <!-- Botón para ver todo el catálogo -->
      <div v-if="categories.length" class="mt-12 md:mt-8 text-center">
        <RouterLink 
          to="/catalogo" 
          class="inline-flex items-center gap-2 px-6 py-4 text-white font-semibold text-base rounded-lg transition-all duration-200 hover:-translate-y-0.5 hover:shadow-lg group"
          :style="{ backgroundColor: 'var(--theme-accent)' }"
        >
          <span>Ver todo el catálogo</span>
          <svg class="w-5 h-5 transition-transform duration-200 group-hover:translate-x-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3"/>
          </svg>
        </RouterLink>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import type { Category } from '@/services/api'

interface Props {
  categories: Category[]
  loading?: boolean
  maxCategories?: number
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

const router = useRouter()

// Computed
const displayCategories = computed(() => {
  const filteredCategories = props.categories.filter(cat => cat.product_count > 0) // Solo mostrar categorías con productos
  
  // Si maxCategories no se pasa explícitamente desde HomeView, mostrar todas
  if (props.maxCategories === undefined) {
    return filteredCategories
  }
  
  return filteredCategories.slice(0, props.maxCategories)
})

// Methods
const getCategoryInitial = (name: string): string => {
  return name.charAt(0).toUpperCase()
}

const handleCategoryClick = (category: Category) => {
  // Navegar al catálogo con filtro de categoría
  router.push({
    name: 'catalogo',
    query: { categoria: category.codigo_rubro.toString() }
  })
}

const handleImageError = (event: Event) => {
  // Si falla la carga de la imagen, ocultar el elemento img para mostrar el fallback
  const img = event.target as HTMLImageElement
  if (img) {
    img.style.display = 'none'
  }
}

// Colores predefinidos para categorías sin color específico
const getDefaultColor = (index: number): string => {
  const colors = [
    '#FF6B6B', // Rojo
    '#4ECDC4', // Turquesa
    '#45B7D1', // Azul
    '#96CEB4', // Verde
    '#FFEAA7', // Amarillo
    '#DDA0DD', // Púrpura
    '#98D8C8', // Verde agua
    '#F7DC6F'  // Dorado
  ]
  return colors[index % colors.length]
}

// Función para determinar si usar texto blanco o negro basado en la luminosidad del color
const getTextColor = (backgroundColor: string): string => {
  // Convertir hex a RGB
  const hex = backgroundColor.replace('#', '')
  const r = parseInt(hex.substr(0, 2), 16)
  const g = parseInt(hex.substr(2, 2), 16)
  const b = parseInt(hex.substr(4, 2), 16)
  
  // Calcular luminosidad
  const luminosity = (0.299 * r + 0.587 * g + 0.114 * b) / 255
  
  // Usar texto blanco en fondos oscuros, texto negro en fondos claros
  return luminosity > 0.5 ? 'text-gray-800' : 'text-white'
}
</script>

<style scoped>
/* Animación shimmer para loading */
@keyframes shimmer {
  0% {
    background-position: -200px 0;
  }
  100% {
    background-position: calc(200px + 100%) 0;
  }
}
</style>