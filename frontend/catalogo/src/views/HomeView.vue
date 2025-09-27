<!-- HomeView.vue - Página de inicio de Dulce y Salado -->
<template>
  <div class="min-h-screen" :style="{ background: `linear-gradient(135deg, var(--theme-primary), var(--theme-gray-dark))` }">
    <!-- Home Header -->
    <HomeHeader />
    
    <!-- Header Spacer -->
    <div class="h-14 lg:h-16"></div>
    
    <!-- Hero Section -->
    <HeroSection />
    
    <!-- Spacing after hero -->
    <div class="py-8"></div>
    
    <!-- Novedades Section -->
    <section
      v-if="catalogStore.novedades.length > 0"
      id="novedades"
      class="py-16 md:py-12"
      style="background-color: #1E1E1E;"
    >
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="text-center mb-12 md:mb-8">
          <h2 class="text-3xl md:text-4xl font-bold text-white mb-4">
            🆕 Novedades
          </h2>
          <p class="text-lg text-gray-300">
            Los productos más nuevos en nuestro catálogo
          </p>
        </div>

        <ProductCarousel
          title=""
          :products="catalogStore.novedades"
          :icon="null"
          @open-cart="handleAddToCart"
        />

      </div>
    </section>

    <!-- Ofertas Section -->
    <section
      v-if="catalogStore.ofertas.length > 0"
      id="ofertas"
      class="py-16 md:py-12"
      style="background-color: #2A2A2A;"
    >
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="text-center mb-12 md:mb-8">
          <h2 class="text-3xl md:text-4xl font-bold text-white mb-4">
            🔥 Ofertas Especiales
          </h2>
          <p class="text-lg text-gray-300">
            Los mejores precios que no podés dejar pasar
          </p>
        </div>

        <ProductCarousel
          title=""
          :products="catalogStore.ofertas"
          :icon="null"
          @open-cart="handleAddToCart"
        />

      </div>
    </section>
    
    <!-- Ubicación Section -->
    <section id="ubicacion" class="py-16 md:py-12" style="background-color: #333333;">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="text-center mb-12">
          <h2 class="text-3xl md:text-4xl font-bold text-white mb-4">
            📍 Visitanos
          </h2>
          <p class="text-lg text-gray-300">
            Encontrá nuestro local y vení a conocer todos nuestros productos
          </p>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 items-start">
          <!-- Información del local -->
          <div class="space-y-6">
            <div class="bg-gray-800 rounded-xl p-6 shadow-sm">
              <h3 class="text-xl font-bold text-white mb-4">Información del Local</h3>
              
              <div class="space-y-4">
                <div class="flex items-start gap-3">
                  <svg class="w-5 h-5 text-red-600 mt-1 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"/>
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"/>
                  </svg>
                  <div>
                    <h4 class="font-semibold text-white">Dirección</h4>
                    <p class="text-gray-300">{{ EMPRESA_CONFIG.direccion || 'Dirección del local' }}</p>
                  </div>
                </div>

                <div class="flex items-start gap-3">
                  <svg class="w-5 h-5 text-red-600 mt-1 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
                  </svg>
                  <div>
                    <h4 class="font-semibold text-white">Horarios</h4>
                    <div class="text-gray-300 space-y-1">
                      <p>Lunes a Viernes: 8:00 - 20:00</p>
                      <p>Sábados: 9:00 - 18:00</p>
                      <p>Domingos: 10:00 - 16:00</p>
                    </div>
                  </div>
                </div>

                <div class="flex items-start gap-3">
                  <svg class="w-5 h-5 text-red-600 mt-1 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"/>
                  </svg>
                  <div>
                    <h4 class="font-semibold text-white">Teléfono</h4>
                    <p class="text-gray-300">{{ EMPRESA_CONFIG.telefono || 'Teléfono del local' }}</p>
                  </div>
                </div>
              </div>

              <!-- Botones de acción -->
              <div class="flex flex-col sm:flex-row gap-3 mt-6">
                <a 
                  :href="`https://maps.google.com/maps?q=${encodeURIComponent(EMPRESA_CONFIG.direccion || 'Dulce y Salado')}`"
                  target="_blank"
                  class="inline-flex items-center justify-center gap-2 px-4 py-2 text-white font-medium rounded-lg transition-colors duration-200"
                  :style="{ backgroundColor: 'var(--theme-accent)' }"
                  @mouseenter="handleButtonHover"
                  @mouseleave="handleButtonLeave"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"/>
                  </svg>
                  Abrir en Google Maps
                </a>
                <a 
                  v-if="EMPRESA_CONFIG.whatsapp"
                  :href="`https://wa.me/${EMPRESA_CONFIG.whatsapp.replace(/\D/g, '')}?text=${encodeURIComponent('¡Hola! ¿Podrían darme información sobre cómo llegar al local?')}`"
                  target="_blank"
                  class="inline-flex items-center justify-center gap-2 px-4 py-2 bg-green-600 text-white font-medium rounded-lg hover:bg-green-700 transition-colors duration-200"
                >
                  <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893A11.821 11.821 0 0020.485 3.097"/>
                  </svg>
                  Consultar ubicación
                </a>
              </div>
            </div>
          </div>

          <!-- Mapa -->
          <div class="bg-gray-800 rounded-xl overflow-hidden shadow-sm">
            <div class="h-80 lg:h-96 bg-gray-200 relative">
              <!-- Google Maps Embed -->
              <iframe
                :src="`https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3290.4642789942877!2d-58.5692773!3d-34.4956473!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x95bcb0b8e2f5c6b9%3A0x12345678901234!2s${encodeURIComponent(EMPRESA_CONFIG.direccion)}!5e0!3m2!1ses!2sar!4v1234567890123!5m2!1ses!2sar`"
                width="100%"
                height="100%"
                style="border:0;"
                allowfullscreen
                loading="lazy"
                referrerpolicy="no-referrer-when-downgrade"
                class="w-full h-full"
              ></iframe>
              
              <!-- Fallback si no hay API key o falla -->
              <div v-if="!mapLoaded" class="absolute inset-0 bg-gray-700 flex items-center justify-center">
                <div class="text-center text-gray-300">
                  <svg class="w-12 h-12 mx-auto mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"/>
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"/>
                  </svg>
                  <p class="text-sm">Mapa de ubicación</p>
                  <p class="text-xs mt-1">Click en "Abrir en Google Maps"</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
    
    <!-- Floating Buttons -->
    <FloatingWhatsApp />
    <ScrollToTop />
    <FloatingCart
      v-if="EMPRESA_CONFIG.permitirPedidos"
      :always-show="false"
      :compact="false"
      @open-summary="openCartSummary"
      @open-export="openExportOptions"
    />
    
    <!-- WhatsApp Message Popup -->
    <Transition name="message-popup">
      <div 
        v-if="showMessagePopup" 
        class="fixed bottom-20 left-6 w-70 md:w-60 bg-white rounded-xl shadow-2xl overflow-hidden border border-gray-200 z-40"
      >
        <div class="flex p-4 gap-3">
          <div class="flex-shrink-0 w-10 h-10 rounded-full overflow-hidden bg-gray-100">
            <img 
              :src="EMPRESA_CONFIG.logoUrl" 
              :alt="EMPRESA_CONFIG.nombre"
              class="w-full h-full object-cover"
            />
          </div>
          <div class="flex-1 min-w-0">
            <div class="flex justify-between items-center mb-1">
              <strong class="font-semibold text-sm text-gray-900">{{ EMPRESA_CONFIG.nombre }}</strong>
              <span class="text-xs text-gray-500">{{ currentTime }}</span>
            </div>
            <div class="text-sm text-gray-900 leading-normal">
              ¡Hola! 👋 ¿En qué podemos ayudarte? Consultanos sobre nuestros productos.
            </div>
          </div>
        </div>
        
        <button 
          class="absolute top-2 right-2 w-6 h-6 flex items-center justify-center text-gray-400 rounded transition-colors duration-200 hover:bg-gray-100 hover:text-gray-900 cursor-pointer"
          @click="hideMessagePopup"
          aria-label="Cerrar mensaje"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
          </svg>
        </button>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
// useRouter import removed - not used
import { useCatalogStore } from '@/stores/catalog'
import { useCartStore } from '@/stores/cart'
import HomeHeader from '@/components/home/HomeHeader.vue'
import HeroSection from '@/components/home/HeroSection.vue'
import ProductCarousel from '@/components/catalog/ProductCarousel.vue'
import FloatingWhatsApp from '@/components/ui/FloatingWhatsApp.vue'
import ScrollToTop from '@/components/ui/ScrollToTop.vue'
import FloatingCart from '@/components/cart/FloatingCart.vue'
import { applyTheme } from '@/utils/theme'
import { EMPRESA_CONFIG } from '@/config/empresa.config'
import type { Product } from '@/services/api'

// Stores
const catalogStore = useCatalogStore()
const cartStore = useCartStore()
// router removed - not used currently

// Map state
const mapLoaded = ref(true) // Asumimos que el mapa se cargará correctamente

// WhatsApp popup state
const showMessagePopup = ref(false)
const currentTime = ref('')
const popupTimeout = ref<number | null>(null)

// Data real obtenida del backend a través del store


// Methods
const handleAddToCart = (product: Product) => {
  cartStore.addItem(product, 1)
}

const handleButtonHover = (event: Event) => {
  const target = event.target as HTMLElement
  if (target) {
    target.style.backgroundColor = '#CC0000'
  }
}

const handleButtonLeave = (event: Event) => {
  const target = event.target as HTMLElement
  if (target) {
    target.style.backgroundColor = 'var(--theme-accent)'
  }
}

// handleProductClick removed - using direct navigation

// Cart methods
const openCartSummary = () => {
  // TODO: Implement cart summary modal if needed
  console.log('Open cart summary')
}

const openExportOptions = () => {
  // TODO: Implement export options if needed  
  console.log('Open export options')
}

// WhatsApp popup methods
const showMessagePopupDelayed = () => {
  popupTimeout.value = window.setTimeout(() => {
    showMessagePopup.value = true
    updateCurrentTime()
  }, 5000) // 5 segundos
}

const hideMessagePopup = () => {
  showMessagePopup.value = false
}

const updateCurrentTime = () => {
  const now = new Date()
  currentTime.value = now.toLocaleTimeString('es-AR', { 
    hour: '2-digit', 
    minute: '2-digit' 
  })
}

// Lifecycle
onMounted(async () => {
  // Aplicar tema
  applyTheme()

  // Cargar novedades y ofertas para la página de inicio
  await Promise.all([
    catalogStore.fetchNovedades(),
    catalogStore.fetchOfertas()
  ])

  // Mostrar popup de WhatsApp después de 5 segundos
  showMessagePopupDelayed()
})

onUnmounted(() => {
  if (popupTimeout.value) {
    window.clearTimeout(popupTimeout.value)
  }
})
</script>

<style scoped>
/* Transiciones para popup de mensaje */
.message-popup-enter-active,
.message-popup-leave-active {
  transition: all 300ms ease;
}

.message-popup-enter-from,
.message-popup-leave-to {
  opacity: 0;
  transform: translateY(20px) scale(0.9);
}
</style>