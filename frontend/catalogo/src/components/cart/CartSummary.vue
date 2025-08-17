<template>
  <Transition name="modal-fade">
    <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <!-- Backdrop -->
      <div 
        class="absolute inset-0 bg-black/60"
        @click="closeModal"
      ></div>
      
      <!-- Modal -->
      <div class="relative bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-hidden">
        <!-- Header -->
        <div class="sticky top-0 bg-white border-b border-gray-100 px-6 py-4 rounded-t-2xl">
          <div class="flex items-center justify-between">
            <h3 class="text-xl font-semibold text-gray-900">
              Carrito de compras
            </h3>
            <button 
              @click="closeModal"
              class="p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
            >
              <XMarkIcon class="w-5 h-5 text-gray-500" />
            </button>
          </div>
          <p class="text-sm text-gray-500 mt-1">
            {{ cartStore.itemCount }} {{ cartStore.itemCount === 1 ? 'producto' : 'productos' }} - {{ cartStore.totalItems }} {{ cartStore.totalItems === 1 ? 'unidad' : 'unidades' }} total
          </p>
          <div class="mt-2 p-3 bg-green-50 rounded-lg border border-green-200">
            <p class="text-xs text-green-700 flex items-center gap-2">
              <svg class="w-4 h-4 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
              </svg>
              Envía tu pedido directamente o exporta como lista de compras para planificar.
            </p>
          </div>
        </div>
        
        <!-- Cart Items -->
        <div class="flex-1 overflow-y-auto max-h-96">
          <div v-if="cartStore.isEmpty" class="p-12 text-center">
            <ClipboardDocumentListIcon class="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h4 class="text-lg font-medium text-gray-500 mb-2">Tu lista está vacía</h4>
            <p class="text-gray-400">Agrega productos para comenzar a planificar tu compra</p>
          </div>
          
          <div v-else class="p-6 pb-48">
            <div 
              v-for="item in cartStore.items" 
              :key="item.codigo"
              class="border border-gray-100 rounded-lg mb-4 last:mb-0 overflow-hidden"
            >
              <!-- Mobile Layout -->
              <div class="block sm:hidden">
                <!-- Product Header -->
                <div class="flex items-start gap-3 p-4 pb-2">
                  <div class="w-16 h-16 bg-gray-50 rounded-lg flex-shrink-0 overflow-hidden">
                    <img 
                      v-if="item.imagen_urls?.length" 
                      :src="item.imagen_urls[0]" 
                      :alt="item.nombre"
                      class="w-full h-full object-cover"
                    />
                    <div v-else class="w-full h-full flex items-center justify-center">
                      <ShoppingBagIcon class="w-6 h-6 text-gray-300" />
                    </div>
                  </div>
                  <div class="flex-1 min-w-0">
                    <h4 class="font-medium text-gray-900 mb-1 text-sm leading-tight">
                      {{ item.nombre }}
                    </h4>
                    <p class="text-xs text-gray-500">
                      Código: {{ item.codigo }}
                    </p>
                  </div>
                </div>
                
                <!-- Product Details -->
                <div class="px-4 py-2 bg-gray-50">
                  <div class="flex items-center justify-between mb-3">
                    <div class="text-sm text-gray-600">
                      ${{ item.precio.toFixed(2) }} × {{ item.cantidad }}
                    </div>
                    <div class="text-lg font-semibold text-gray-900">
                      ${{ (item.precio * item.cantidad).toFixed(2) }}
                    </div>
                  </div>
                  
                  <!-- Quantity Controls Mobile -->
                  <div class="flex items-center justify-between">
                    <div class="flex items-center gap-2">
                      <button 
                        @click="cartStore.decrementItem(item.codigo)"
                        class="w-8 h-8 rounded border border-gray-300 flex items-center justify-center hover:bg-gray-50 transition-colors cursor-pointer"
                      >
                        <MinusIcon class="w-4 h-4" />
                      </button>
                      <input
                        :value="item.cantidad"
                        @input="updateQuantity(item.codigo, $event)"
                        @blur="validateQuantity(item.codigo, $event)"
                        type="number"
                        min="1"
                        max="9999"
                        class="w-14 px-2 py-1 text-center text-sm font-medium border border-gray-300 rounded focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                      />
                      <button 
                        @click="cartStore.incrementItem(item.codigo)"
                        class="w-8 h-8 rounded border border-gray-300 flex items-center justify-center hover:bg-gray-50 transition-colors cursor-pointer"
                      >
                        <PlusIcon class="w-4 h-4" />
                      </button>
                    </div>
                    <button 
                      @click="cartStore.removeItem(item.codigo)"
                      class="text-sm text-red-600 hover:text-red-800 transition-colors cursor-pointer px-2 py-1"
                    >
                      Eliminar
                    </button>
                  </div>
                </div>
              </div>
              
              <!-- Desktop Layout -->
              <div class="hidden sm:flex items-center gap-4 p-4">
                <!-- Product Image -->
                <div class="w-20 h-20 bg-gray-50 rounded-lg flex-shrink-0 overflow-hidden">
                  <img 
                    v-if="item.imagen_urls?.length" 
                    :src="item.imagen_urls[0]" 
                    :alt="item.nombre"
                    class="w-full h-full object-cover"
                  />
                  <div v-else class="w-full h-full flex items-center justify-center">
                    <ShoppingBagIcon class="w-8 h-8 text-gray-300" />
                  </div>
                </div>
                
                <!-- Product Info -->
                <div class="flex-1 min-w-0">
                  <h4 class="font-medium text-gray-900 mb-1">
                    {{ item.nombre }}
                  </h4>
                  <p class="text-sm text-gray-500 mb-2">
                    Código: {{ item.codigo }}
                  </p>
                  <div class="flex items-center justify-between">
                    <div class="text-sm text-gray-600">
                      ${{ item.precio.toFixed(2) }} × {{ item.cantidad }}
                    </div>
                    <div class="text-lg font-semibold text-gray-900">
                      ${{ (item.precio * item.cantidad).toFixed(2) }}
                    </div>
                  </div>
                </div>
                
                <!-- Quantity Controls Desktop -->
                <div class="flex flex-col items-center gap-2">
                  <div class="flex items-center gap-2">
                    <button 
                      @click="cartStore.decrementItem(item.codigo)"
                      class="w-8 h-8 rounded border border-gray-300 flex items-center justify-center hover:bg-gray-50 transition-colors cursor-pointer"
                    >
                      <MinusIcon class="w-4 h-4" />
                    </button>
                    <input
                      :value="item.cantidad"
                      @input="updateQuantity(item.codigo, $event)"
                      @blur="validateQuantity(item.codigo, $event)"
                      type="number"
                      min="1"
                      max="9999"
                      class="w-16 px-2 py-1 text-center text-lg font-medium border border-gray-300 rounded focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    />
                    <button 
                      @click="cartStore.incrementItem(item.codigo)"
                      class="w-8 h-8 rounded border border-gray-300 flex items-center justify-center hover:bg-gray-50 transition-colors cursor-pointer"
                    >
                      <PlusIcon class="w-4 h-4" />
                    </button>
                  </div>
                  <button 
                    @click="cartStore.removeItem(item.codigo)"
                    class="text-xs text-red-600 hover:text-red-800 transition-colors cursor-pointer"
                  >
                    Eliminar
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Footer -->
        <div v-if="!cartStore.isEmpty" class="sticky bottom-0 bg-white border-t border-gray-100 px-6 py-4 rounded-b-2xl">
          <!-- Totals -->
          <div class="bg-gray-50 rounded-lg p-4 mb-4">
            <div class="flex justify-between items-center mb-2">
              <span class="text-sm text-gray-600">Productos diferentes:</span>
              <span class="font-medium">{{ cartStore.itemCount }}</span>
            </div>
            <div class="flex justify-between items-center mb-2">
              <span class="text-sm text-gray-600">Unidades totales:</span>
              <span class="font-medium">{{ cartStore.totalItems }}</span>
            </div>
            <div class="border-t border-gray-200 pt-2">
              <div class="flex justify-between items-center">
                <span class="text-lg font-semibold text-gray-900">Total:</span>
                <span class="text-2xl font-bold text-gray-900">
                  ${{ cartStore.totalAmount.toFixed(2) }}
                </span>
              </div>
            </div>
          </div>
          
          <!-- Actions -->
          <div class="space-y-3">
            <!-- Primary Action: Send Order -->
            <button 
              @click="openOrderModal"
              class="w-full flex items-center justify-center gap-2 px-6 py-4 text-white rounded-lg font-semibold text-lg transition-colors cursor-pointer shadow-md hover:shadow-lg"
              :style="{ background: 'var(--theme-accent)' }"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17M17 13v6a2 2 0 01-2 2H9a2 2 0 01-2-2v-6m8 0V9a2 2 0 00-2-2H9a2 2 0 00-2 2v4"/>
              </svg>
              🛒 Enviar Pedido
            </button>
            
            <!-- Secondary Actions -->
            <div class="grid grid-cols-2 gap-3">
              <button 
                @click="openExportOptions"
                class="flex items-center justify-center gap-2 px-4 py-3 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg font-medium transition-colors cursor-pointer border border-gray-200"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                </svg>
                📋 Exportar Lista
              </button>
              <button 
                @click="clearCart"
                class="flex items-center justify-center gap-2 px-4 py-3 text-red-700 bg-red-50 hover:bg-red-100 rounded-lg font-medium transition-colors cursor-pointer border border-red-200"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                </svg>
                Vaciar
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { watch } from 'vue'
import { useCartStore } from '@/stores/cart'
import { 
  XMarkIcon,
  ClipboardDocumentListIcon,
  ShoppingBagIcon,
  PlusIcon,
  MinusIcon
} from '@heroicons/vue/24/outline'

interface Props {
  isOpen: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
  openExport: []
  openOrder: []
}>()

// Store
const cartStore = useCartStore()

// Methods
const closeModal = () => {
  emit('close')
}

const clearCart = () => {
  if (confirm('¿Estás seguro de que quieres vaciar el carrito?')) {
    cartStore.clearCart()
  }
}

const openExportOptions = () => {
  emit('openExport')
}

const openOrderModal = () => {
  emit('openOrder')
}

const updateQuantity = (codigo: string, event: Event) => {
  const target = event.target as HTMLInputElement
  const newQuantity = parseInt(target.value) || 1
  cartStore.updateQuantity(codigo, newQuantity)
}

const validateQuantity = (codigo: string, event: Event) => {
  const target = event.target as HTMLInputElement
  let quantity = parseInt(target.value) || 1
  
  if (quantity < 1) {
    quantity = 1
  } else if (quantity > 9999) {
    quantity = 9999
  }
  
  target.value = quantity.toString()
  cartStore.updateQuantity(codigo, quantity)
}

// Prevent body scroll when modal is open
watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    document.body.style.overflow = 'hidden'
  } else {
    document.body.style.overflow = ''
  }
})
</script>

<style scoped>
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: all 0.15s ease;
}

.modal-fade-enter-from {
  opacity: 0;
  transform: scale(0.9);
}

.modal-fade-leave-to {
  opacity: 0;
  transform: scale(0.9);
}
</style>