<template>
  <div class="relative">
    <!-- Cart Button -->
    <button 
      @click="toggleCart"
      class="relative p-3 rounded-full transition-all duration-300 cursor-pointer"
      :style="{ 
        background: cartStore.totalItems > 0 ? 'var(--theme-accent)' : 'rgba(255, 255, 255, 0.1)',
        transform: cartStore.totalItems > 0 ? 'scale(1.1)' : 'scale(1)'
      }"
      :class="{ 'animate-bounce': justAdded }"
    >
      <ShoppingCartIcon class="w-6 h-6 text-white" />
      
      <!-- Item Count Badge -->
      <Transition name="badge-pop">
        <div 
          v-if="cartStore.totalItems > 0"
          class="absolute -top-1 -right-1 min-w-[20px] h-5 bg-red-500 text-white text-xs font-bold rounded-full flex items-center justify-center px-1"
        >
          {{ cartStore.totalItems > 99 ? '99+' : cartStore.totalItems }}
        </div>
      </Transition>
    </button>
    
    <!-- Cart Dropdown -->
    <Transition name="dropdown-slide">
      <div 
        v-if="showDropdown"
        class="absolute right-0 top-full mt-2 w-80 bg-white rounded-xl shadow-2xl border border-gray-100 z-50"
        @click.stop
      >
        <!-- Header -->
        <div class="p-4 border-b border-gray-100">
          <div class="flex items-center justify-between">
            <h3 class="font-semibold text-gray-900">
              Carrito de compras
            </h3>
            <button 
              @click="showDropdown = false"
              class="p-1 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <XMarkIcon class="w-4 h-4 text-gray-500" />
            </button>
          </div>
          <p class="text-sm text-gray-500 mt-1">
            {{ cartStore.totalItems }} {{ cartStore.totalItems === 1 ? 'producto' : 'productos' }}
          </p>
        </div>
        
        <!-- Cart Items -->
        <div class="max-h-64 overflow-y-auto">
          <div v-if="cartStore.isEmpty" class="p-6 text-center">
            <ShoppingCartIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
            <p class="text-gray-500">Tu carrito está vacío</p>
          </div>
          
          <div v-else class="p-2">
            <div 
              v-for="item in cartStore.items" 
              :key="item.codigo"
              class="flex items-center gap-3 p-2 hover:bg-gray-50 rounded-lg transition-colors"
            >
              <!-- Product Image -->
              <div class="w-12 h-12 bg-gray-100 rounded-lg flex-shrink-0 overflow-hidden">
                <img 
                  v-if="item.imagen_urls?.length" 
                  :src="item.imagen_urls[0]" 
                  :alt="item.nombre"
                  class="w-full h-full object-cover"
                />
                <div v-else class="w-full h-full flex items-center justify-center">
                  <ShoppingBagIcon class="w-4 h-4 text-gray-300" />
                </div>
              </div>
              
              <!-- Product Info -->
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-gray-900 truncate">
                  {{ item.nombre }}
                </p>
                <p class="text-xs text-gray-500">
                  ${{ item.precio.toFixed(2) }} × {{ item.cantidad }}
                </p>
              </div>
              
              <!-- Actions -->
              <div class="flex items-center gap-1">
                <button 
                  @click="cartStore.decrementItem(item.codigo)"
                  class="w-6 h-6 rounded border border-gray-300 flex items-center justify-center hover:bg-gray-50 transition-colors"
                >
                  <MinusIcon class="w-3 h-3" />
                </button>
                <span class="text-sm font-medium min-w-[20px] text-center">
                  {{ item.cantidad }}
                </span>
                <button 
                  @click="cartStore.incrementItem(item.codigo)"
                  class="w-6 h-6 rounded border border-gray-300 flex items-center justify-center hover:bg-gray-50 transition-colors"
                >
                  <PlusIcon class="w-3 h-3" />
                </button>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Footer -->
        <div v-if="!cartStore.isEmpty" class="p-4 border-t border-gray-100">
          <!-- Total -->
          <div class="flex justify-between items-center mb-3">
            <span class="font-semibold text-gray-900">Total:</span>
            <span class="text-lg font-bold text-gray-900">
              ${{ cartStore.totalAmount.toFixed(2) }}
            </span>
          </div>
          
          <!-- Actions -->
          <div class="flex gap-2">
            <button 
              @click="openCartSummary"
              class="flex-1 px-3 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
            >
              Ver carrito
            </button>
            <button 
              @click="openExportOptions"
              class="flex-1 px-3 py-2 text-sm font-medium text-white rounded-lg transition-colors"
              :style="{ background: 'var(--theme-primary)' }"
            >
              Exportar
            </button>
          </div>
        </div>
      </div>
    </Transition>
    
    <!-- Overlay -->
    <div 
      v-if="showDropdown"
      class="fixed inset-0 z-40"
      @click="showDropdown = false"
    ></div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useCartStore } from '@/stores/cart'
import { 
  ShoppingCartIcon,
  ShoppingBagIcon,
  XMarkIcon,
  PlusIcon,
  MinusIcon 
} from '@heroicons/vue/24/outline'

const emit = defineEmits<{
  openSummary: []
  openExport: []
}>()

// Store
const cartStore = useCartStore()

// State
const showDropdown = ref(false)
const justAdded = ref(false)

// Methods
const toggleCart = () => {
  showDropdown.value = !showDropdown.value
}

const openCartSummary = () => {
  showDropdown.value = false
  emit('openSummary')
}

const openExportOptions = () => {
  showDropdown.value = false
  emit('openExport')
}

// Watch for items being added to trigger animation
const previousItemCount = ref(cartStore.totalItems)
watch(() => cartStore.totalItems, (newCount) => {
  if (newCount > previousItemCount.value) {
    justAdded.value = true
    setTimeout(() => {
      justAdded.value = false
    }, 600)
  }
  previousItemCount.value = newCount
})
</script>

<style scoped>
.badge-pop-enter-active,
.badge-pop-leave-active {
  transition: all 0.3s ease;
}

.badge-pop-enter-from {
  opacity: 0;
  transform: scale(0);
}

.badge-pop-leave-to {
  opacity: 0;
  transform: scale(0);
}

.dropdown-slide-enter-active,
.dropdown-slide-leave-active {
  transition: all 0.3s ease;
}

.dropdown-slide-enter-from {
  opacity: 0;
  transform: translateY(-10px) scale(0.95);
}

.dropdown-slide-leave-to {
  opacity: 0;
  transform: translateY(-10px) scale(0.95);
}
</style>