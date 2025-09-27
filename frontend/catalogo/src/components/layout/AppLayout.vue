<!-- AppLayout.vue actualizado con mejor estructura -->
<template>
  <div class="min-h-screen flex flex-col" :style="{ background: `linear-gradient(135deg, var(--theme-primary), var(--theme-gray-dark))` }">
    <!-- Header -->
    <AppHeader 
      @open-cart-summary="showCartSummary = true"
      @open-export-options="showExportOptions = true"
    />
    
    <!-- Header Spacer -->
    <div class="h-14 lg:h-16"></div>
    
    <!-- CTA Solicitud Comerciante (solo para usuarios autenticados sin lista de precios 2) -->
    <div v-if="shouldShowComercianteCTA" class="bg-gradient-to-r from-yellow-400 to-orange-500 text-white shadow-lg">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-5">
        <div class="flex flex-col lg:flex-row items-start lg:items-center justify-between gap-4 lg:gap-8">
          <div class="flex items-start gap-3 flex-1 min-w-0 ml-16 md:ml-20 lg:ml-32 xl:ml-40">
            <div class="flex-shrink-0 mt-0.5">
              <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div class="flex-1 pr-6">
              <p class="font-bold text-xl leading-tight mb-2">
                ¿Tenés un comercio?
              </p>
              <p class="text-yellow-100 text-base font-medium">
                Accedé a precios especiales de comerciante con descuentos exclusivos
              </p>
            </div>
          </div>
          <div class="flex-shrink-0 w-full lg:w-auto self-center">
            <button
              @click="showSolicitudModal = true"
              class="w-full lg:w-auto px-8 py-3 bg-white text-orange-600 font-bold rounded-lg hover:bg-gray-50 transition-colors shadow-md hover:shadow-lg text-sm whitespace-nowrap cursor-pointer"
            >
              Solicitar Cuenta de Comerciante
            </button>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Main Content con container apropiado -->
    <main class="flex-1">
      <div class="container">
        <slot />
      </div>
    </main>
    
    <!-- Footer -->
    <AppFooter />
    
    <!-- Cart Summary Modal -->
    <CartSummary 
      :is-open="showCartSummary"
      @close="showCartSummary = false"
      @open-export="openExportFromSummary"
      @open-order="openOrderFromSummary"
    />
    
    <!-- Export Options Modal -->
    <ExportOptions 
      :is-open="showExportOptions"
      @close="showExportOptions = false"
      @exported="onExported"
    />
    
    <!-- Order Modal -->
    <OrderModal
      :is-open="showOrderModal"
      @close="showOrderModal = false"
      @order-created="onOrderCreated"
    />
    
    <!-- Add to Cart Modal -->
    <AddToCartModal
      :is-open="showAddToCartModal"
      :product="selectedProduct"
      @close="showAddToCartModal = false"
      @added="onProductAdded"
    />
    
    <!-- Welcome Back Modal -->
    <WelcomeBackModal
      :is-open="showWelcomeModal"
      :item-count="cartStore.itemCount"
      @keep="handleKeepList"
      @clear="handleClearList"
    />
    
    <!-- Floating Cart Button (solo en desktop y ciertas vistas) -->
    <div class="hidden md:block">
      <FloatingCart 
        v-if="shouldShowFloatingCart"
        @open-summary="showCartSummary = true"
        @open-export="showExportOptions = true"
      />
    </div>
    
    <!-- Floating WhatsApp Button -->
    <FloatingWhatsApp />
    
    <!-- Scroll to Top Button -->
    <ScrollToTop />
    
    <!-- Modal Solicitud Comerciante -->
    <SolicitudReventaModal
      :is-open="showSolicitudModal"
      @close="showSolicitudModal = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, provide } from 'vue'
import { useRoute } from 'vue-router'
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'
import { useWelcomeBack } from '@/composables/useWelcomeBack'
import AppHeader from './AppHeader.vue'
import AppFooter from './AppFooter.vue'
import CartSummary from '@/components/cart/CartSummary.vue'
import ExportOptions from '@/components/cart/ExportOptions.vue'
import OrderModal from '@/components/cart/OrderModal.vue'
import AddToCartModal from '@/components/cart/AddToCartModal.vue'
import WelcomeBackModal from '@/components/ui/WelcomeBackModal.vue'
import FloatingCart from '@/components/cart/FloatingCart.vue'
import FloatingWhatsApp from '@/components/ui/FloatingWhatsApp.vue'
import ScrollToTop from '@/components/ui/ScrollToTop.vue'
import SolicitudReventaModal from '@/components/solicitud/SolicitudReventaModal.vue'

// Stores
const cartStore = useCartStore()
const authStore = useAuthStore()
const route = useRoute()

// Composables
const { showWelcomeModal, checkWelcomeBack, handleKeepList, handleClearList } = useWelcomeBack()

// Computed
const shouldShowFloatingCart = computed(() => {
  // Solo mostrar el carrito flotante en pantallas grandes en vistas donde tiene sentido
  // En móvil, el carrito está en el header
  // No mostrar en la vista de inicio
  const routesWithCart = ['catalogo', 'Category', 'Product']
  const isHomeRoute = route.name === 'home'
  return routesWithCart.includes(route.name as string) && !isHomeRoute
})

// Mostrar CTA de comerciante solo para usuarios autenticados que NO tengan lista de precios 2
const shouldShowComercianteCTA = computed(() => {
  return authStore.isAuthenticated && 
         authStore.user?.lista_precio && 
         authStore.user.lista_precio.codigo !== "2"  // Solo mostrar si NO tiene la lista de código "2" (comerciante)
})

// Modal state
const showCartSummary = ref(false)
const showExportOptions = ref(false)
const showOrderModal = ref(false)
const showAddToCartModal = ref(false)
const showSolicitudModal = ref(false)
const selectedProduct = ref(null)

// Methods
const openExportFromSummary = () => {
  showCartSummary.value = false
  showExportOptions.value = true
}

const openOrderFromSummary = () => {
  showCartSummary.value = false
  showOrderModal.value = true
}

const onExported = (type: string) => {
  console.log(`Exported as: ${type}`)
  // Could show a toast notification here
}

const onOrderCreated = (orderNumber: string) => {
  console.log(`Order created: ${orderNumber}`)
  // Could show a success notification here
  // Could also redirect to order history page
}

const onProductAdded = (product: any, quantity: number) => {
  console.log(`Added ${quantity} of ${product.nombre} to cart`)
  // Could show a success notification here
}

// Provide cart modal access globally
const openCartModal = (product: any) => {
  selectedProduct.value = product
  showAddToCartModal.value = true
}

provide('openCartModal', openCartModal)

// Initialize welcome back check
onMounted(() => {
  // Small delay to ensure all stores are initialized
  setTimeout(checkWelcomeBack, 500)
})
</script>