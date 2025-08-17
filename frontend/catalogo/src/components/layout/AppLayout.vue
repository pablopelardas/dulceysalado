<!-- AppLayout.vue actualizado con mejor estructura -->
<template>
  <div class="min-h-screen flex flex-col" :style="{ background: `linear-gradient(135deg, var(--theme-primary), var(--theme-gray-dark))` }">
    <!-- Header -->
    <AppHeader 
      @open-cart-summary="showCartSummary = true"
      @open-export-options="showExportOptions = true"
    />
    
    <!-- Header Spacer -->
    <div class="h-12 md:h-14 lg:h-16"></div>
    
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
    
    <!-- Floating WhatsApp Button -->
    <FloatingWhatsApp />
    
    <!-- Scroll to Top Button -->
    <ScrollToTop />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, provide } from 'vue'
import { useCartStore } from '@/stores/cart'
import { useWelcomeBack } from '@/composables/useWelcomeBack'
import AppHeader from './AppHeader.vue'
import AppFooter from './AppFooter.vue'
import CartSummary from '@/components/cart/CartSummary.vue'
import ExportOptions from '@/components/cart/ExportOptions.vue'
import OrderModal from '@/components/cart/OrderModal.vue'
import AddToCartModal from '@/components/cart/AddToCartModal.vue'
import WelcomeBackModal from '@/components/ui/WelcomeBackModal.vue'
import FloatingWhatsApp from '@/components/ui/FloatingWhatsApp.vue'
import ScrollToTop from '@/components/ui/ScrollToTop.vue'

// Stores
const cartStore = useCartStore()

// Composables
const { showWelcomeModal, checkWelcomeBack, handleKeepList, handleClearList } = useWelcomeBack()

// Modal state
const showCartSummary = ref(false)
const showExportOptions = ref(false)
const showOrderModal = ref(false)
const showAddToCartModal = ref(false)
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