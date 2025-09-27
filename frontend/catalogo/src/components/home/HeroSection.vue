<!-- HeroSection.vue - Sección principal simplificada -->
<template>
  <section class="min-h-[70vh] lg:min-h-[80vh] relative overflow-hidden pt-8">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-full flex flex-col justify-center gap-8 lg:gap-12">
      <!-- Contenido del Hero -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 lg:gap-12 items-center text-center lg:text-left">
        <div class="max-w-2xl lg:max-w-none">
          <h1 class="font-extrabold text-4xl md:text-5xl lg:text-6xl leading-tight text-white mb-8">
            <span 
              class="block text-4xl md:text-5xl lg:text-6xl"
              :style="{ color: 'var(--theme-accent)' }"
            >
              {{ EMPRESA_CONFIG.nombre }}
            </span>
          </h1>
          
          <!-- CTA Grande -->
          <div class="flex justify-center lg:justify-start">
            <RouterLink 
              to="/catalogo" 
              class="inline-flex items-center gap-3 px-10 py-5 text-white font-bold text-xl border-2 border-transparent rounded-xl transition-all duration-200 hover:-translate-y-1 hover:shadow-2xl transform"
              :style="{ backgroundColor: 'var(--theme-accent)' }"
            >
              <span>Ver Catálogo</span>
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8l4 4m0 0l-4 4m4-4H3"/>
              </svg>
            </RouterLink>
          </div>
        </div>
        
        <!-- Slider del Hero -->
        <div class="relative h-80 md:h-96 lg:h-[500px]">
          <div class="relative w-full h-full rounded-2xl overflow-hidden shadow-2xl" @mouseenter="stopAutoSlide" @mouseleave="startAutoSlide">
            <!-- Slides -->
            <div class="absolute inset-0 flex transition-transform duration-500 ease-in-out" :style="{ transform: `translateX(-${currentSlide * 100}%)` }">
              <div class="w-full flex-shrink-0 relative">
                <img src="/entrada.webp" :alt="`Entrada del local ${EMPRESA_CONFIG.nombre}`" class="w-full h-full object-cover">
                <div class="absolute bottom-4 left-4 bg-black/60 backdrop-blur-sm text-white px-3 py-1 rounded-full text-sm">
                  Entrada del local
                </div>
              </div>
              <div class="w-full flex-shrink-0 relative">
                <img src="/local-1.webp" :alt="`Interior del local ${EMPRESA_CONFIG.nombre}`" class="w-full h-full object-cover">
                <div class="absolute bottom-4 left-4 bg-black/60 backdrop-blur-sm text-white px-3 py-1 rounded-full text-sm">
                  Interior del local
                </div>
              </div>
              <div class="w-full flex-shrink-0 relative">
                <img src="/local-2.webp" :alt="`Productos en el local ${EMPRESA_CONFIG.nombre}`" class="w-full h-full object-cover">
                <div class="absolute bottom-4 left-4 bg-black/60 backdrop-blur-sm text-white px-3 py-1 rounded-full text-sm">
                  Nuestros productos
                </div>
              </div>
            </div>
            
            <!-- Controles de navegación -->
            <button 
              @click="previousSlide"
              class="absolute left-4 top-1/2 -translate-y-1/2 bg-white/90 hover:bg-white text-gray-800 rounded-full p-2 shadow-lg transition-all duration-200 hover:scale-110"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
              </svg>
            </button>
            <button 
              @click="nextSlide"
              class="absolute right-4 top-1/2 -translate-y-1/2 bg-white/90 hover:bg-white text-gray-800 rounded-full p-2 shadow-lg transition-all duration-200 hover:scale-110"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
              </svg>
            </button>
            
            <!-- Indicadores de puntos -->
            <div class="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2">
              <button 
                v-for="(slide, index) in 3" 
                :key="index"
                @click="goToSlide(index)"
                :class="currentSlide === index ? 'bg-white' : 'bg-white/50'"
                class="w-2 h-2 rounded-full transition-all duration-200 hover:bg-white"
              ></button>
            </div>
          </div>
        </div>
      </div>
    </div>
    
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { RouterLink } from 'vue-router'
import { EMPRESA_CONFIG } from '@/config/empresa.config'

// Slider state
const currentSlide = ref(0)
const totalSlides = 3
let autoSlideInterval: number | null = null

// Slider methods
const nextSlide = () => {
  currentSlide.value = (currentSlide.value + 1) % totalSlides
}

const previousSlide = () => {
  currentSlide.value = currentSlide.value === 0 ? totalSlides - 1 : currentSlide.value - 1
}

const goToSlide = (index: number) => {
  currentSlide.value = index
}

// Auto slide functionality
const startAutoSlide = () => {
  autoSlideInterval = window.setInterval(() => {
    nextSlide()
  }, 5000) // Cambiar slide cada 5 segundos
}

const stopAutoSlide = () => {
  if (autoSlideInterval) {
    clearInterval(autoSlideInterval)
    autoSlideInterval = null
  }
}

// Scroll method
const scrollToCategorias = () => {
  const categoriasElement = document.getElementById('categorias')
  if (categoriasElement) {
    const yOffset = -120 // Account for fixed header height
    const y = categoriasElement.getBoundingClientRect().top + window.pageYOffset + yOffset
    
    window.scrollTo({
      top: y,
      behavior: 'smooth'
    })
  }
}

// Lifecycle
onMounted(() => {
  startAutoSlide()
})

onUnmounted(() => {
  stopAutoSlide()
})
</script>

<style scoped>
/* Animaciones personalizadas para elementos decorativos */
@keyframes float-slow {
  0%, 100% { 
    transform: translateY(0px) rotate(0deg); 
  }
  33% { 
    transform: translateY(-20px) rotate(5deg); 
  }
  66% { 
    transform: translateY(10px) rotate(-3deg); 
  }
}

@keyframes float-medium {
  0%, 100% { 
    transform: translateY(0px) rotate(0deg); 
  }
  33% { 
    transform: translateY(-15px) rotate(-3deg); 
  }
  66% { 
    transform: translateY(8px) rotate(2deg); 
  }
}

@keyframes float-fast {
  0%, 100% { 
    transform: translateY(0px) rotate(0deg); 
  }
  33% { 
    transform: translateY(-10px) rotate(3deg); 
  }
  66% { 
    transform: translateY(5px) rotate(-2deg); 
  }
}

.animate-float-slow {
  animation: float-slow 6s ease-in-out infinite;
  animation-delay: -2s;
}

.animate-float-medium {
  animation: float-medium 7s ease-in-out infinite;
  animation-delay: -4s;
}

.animate-float-fast {
  animation: float-fast 5s ease-in-out infinite;
  animation-delay: -1s;
}
</style>