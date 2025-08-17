<template>
  <div class="relative">
    <!-- Floating Cart Button -->
    <Transition name="cart-float">
      <button 
        v-if="cartStore.itemCount > 0 || alwaysShow"
        @click="toggleCart"
        class="group cursor-pointer"
        :class="[
          compact ? 'relative' : 'fixed top-6 right-6 z-40',
          { 'animate-bounce': justAdded && !compact }
        ]"
      >
        <!-- Button -->
        <div class="relative">
          <div 
            class="flex items-center justify-center rounded-full shadow-lg group-hover:shadow-xl transform group-hover:scale-110 transition-all duration-300"
            :class="compact ? 'w-10 h-10' : 'w-14 h-14'"
            :style="{ background: 'var(--theme-accent)' }"
          >
            <ShoppingCartIcon 
              class="text-white"
              :class="compact ? 'w-5 h-5' : 'w-7 h-7'"
            />
          </div>
          
          <!-- Item Count Badge -->
          <Transition name="badge-pop">
            <div 
              v-if="cartStore.itemCount > 0"
              class="absolute -top-2 -right-2 min-w-[22px] h-6 bg-red-500 text-white text-xs font-bold rounded-full flex items-center justify-center px-1 shadow-lg"
            >
              {{ cartStore.itemCount > 99 ? '99+' : cartStore.itemCount }}
            </div>
          </Transition>
          
          <!-- Pulse animation (only when not compact) -->
          <div 
            v-if="!compact"
            class="absolute inset-0 rounded-full animate-ping opacity-30" 
            :style="{ background: 'var(--theme-accent)' }"
          ></div>
        </div>
        
        <!-- Tooltip -->
        <div class="absolute top-1/2 right-full transform -translate-y-1/2 mr-3 opacity-0 group-hover:opacity-100 transition-opacity duration-300 pointer-events-none">
          <div class="bg-gray-900 text-white text-sm rounded-lg px-3 py-2 whitespace-nowrap">
            {{ cartStore.itemCount }} {{ cartStore.itemCount === 1 ? 'producto' : 'productos' }}
            <div class="absolute top-1/2 left-full transform -translate-y-1/2 w-0 h-0 border-t-4 border-b-4 border-l-4 border-transparent border-l-gray-900"></div>
          </div>
        </div>
      </button>
    </Transition>
    
    <!-- Cart Dropdown -->
    <Transition name="dropdown-slide">
      <div 
        v-if="showDropdown"
        class="w-80 bg-white rounded-xl shadow-2xl border border-gray-100 z-[9999]"
        :class="compact ? 'absolute top-full right-0 mt-2' : 'fixed top-20 right-6'"
        @click.stop
      >
        <!-- Header -->
        <div class="p-4 border-b border-gray-100">
          <div class="flex items-center justify-between">
            <h3 class="font-semibold text-gray-900">
              Mi lista de compras
            </h3>
            <button 
              @click="showDropdown = false"
              class="p-1 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
            >
              <XMarkIcon class="w-4 h-4 text-gray-500" />
            </button>
          </div>
          <p class="text-sm text-gray-500 mt-1">
            {{ cartStore.itemCount }} {{ cartStore.itemCount === 1 ? 'producto' : 'productos' }}
          </p>
        </div>
        
        <!-- Cart Items -->
        <div class="max-h-64 overflow-y-auto">
          <div v-if="cartStore.isEmpty" class="p-6 text-center">
            <ShoppingCartIcon class="w-12 h-12 text-gray-300 mx-auto mb-3" />
            <p class="text-gray-500">Tu lista está vacía</p>
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
                  class="w-6 h-6 rounded border border-gray-300 flex items-center justify-center hover:bg-gray-50 transition-colors cursor-pointer"
                >
                  <MinusIcon class="w-3 h-3" />
                </button>
                <span class="text-sm font-medium min-w-[20px] text-center">
                  {{ item.cantidad }}
                </span>
                <button 
                  @click="cartStore.incrementItem(item.codigo)"
                  class="w-6 h-6 rounded border border-gray-300 flex items-center justify-center hover:bg-gray-50 transition-colors cursor-pointer"
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
          <div class="space-y-2">
            <!-- Primary Action: Send Order -->
            <button 
              @click="openCartSummary"
              class="w-full px-4 py-3 text-sm font-medium text-white rounded-lg transition-colors cursor-pointer"
              :style="{ background: 'var(--theme-accent)' }"
            >
              🛒 Enviar Pedido
            </button>
            
            <!-- Secondary Actions (when expanded) -->
            <div v-if="showDropdown" class="pt-2 border-t border-gray-100">
              <button 
                @click="openExportOptions"
                class="w-full px-3 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors cursor-pointer"
              >
                📋 Exportar Lista
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
    
    <!-- Overlay -->
    <div 
      v-if="showDropdown"
      class="fixed inset-0 z-[9998]"
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

interface Props {
  alwaysShow?: boolean
  compact?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  alwaysShow: false,
  compact: false
})

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
const previousItemCount = ref(cartStore.itemCount)
watch(() => cartStore.itemCount, (newCount) => {
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
.cart-float-enter-active,
.cart-float-leave-active {
  transition: all 0.4s ease;
}

.cart-float-enter-from {
  opacity: 0;
  transform: translateY(20px) scale(0.8);
}

.cart-float-leave-to {
  opacity: 0;
  transform: translateY(20px) scale(0.8);
}

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
  transform: translateY(10px) scale(0.95);
}

.dropdown-slide-leave-to {
  opacity: 0;
  transform: translateY(10px) scale(0.95);
}
</style>