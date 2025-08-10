<template>
  <Transition name="modal-fade">
    <div v-if="isOpen" class="fixed inset-0 z-[60] flex items-center justify-center p-4">
      <!-- Backdrop -->
      <div 
        class="absolute inset-0 bg-black/60"
        @click="closeModal"
      ></div>
      
      <!-- Modal -->
      <div class="relative bg-white rounded-2xl shadow-2xl max-w-md w-full max-h-[90vh] overflow-y-auto">
        <!-- Header -->
        <div class="sticky top-0 bg-white border-b border-gray-100 px-6 py-4 rounded-t-2xl">
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold text-gray-900">
              Agregar a mi lista
            </h3>
            <button 
              @click="closeModal"
              class="p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
            >
              <XMarkIcon class="w-5 h-5 text-gray-500" />
            </button>
          </div>
        </div>
        
        <!-- Product Info -->
        <div class="px-6 py-4">
          <div class="flex gap-4 mb-6">
            <!-- Product Image -->
            <div class="w-20 h-20 bg-gray-50 rounded-lg flex-shrink-0 overflow-hidden">
              <img 
                v-if="product?.imagen_urls?.length" 
                :src="product.imagen_urls[0]" 
                :alt="product.nombre"
                class="w-full h-full object-cover"
              />
              <div v-else class="w-full h-full flex items-center justify-center">
                <ShoppingBagIcon class="w-8 h-8 text-gray-300" />
              </div>
            </div>
            
            <!-- Product Details -->
            <div class="flex-1 min-w-0">
              <h4 class="font-medium text-gray-900 mb-1 line-clamp-2">
                {{ product?.nombre }}
              </h4>
              <p class="text-sm text-gray-500 mb-2">
                Código: {{ product?.codigo }}
              </p>
              <div class="text-lg font-semibold text-gray-900">
                ${{ product?.precio?.toFixed(2) || '0.00' }}
                <span class="text-sm font-normal text-gray-500">por unidad</span>
              </div>
            </div>
          </div>
          
          <!-- Quantity Selection -->
          <div class="mb-6">
            <label class="block text-sm font-medium text-gray-700 mb-3">
              Cantidad
            </label>
            
            <!-- Manual Input -->
            <div class="flex items-center gap-3 mb-4">
              <button 
                @click="decrementQuantity"
                :disabled="quantity <= 1"
                class="w-10 h-10 rounded-lg border border-gray-300 flex items-center justify-center hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors cursor-pointer"
              >
                <MinusIcon class="w-4 h-4" />
              </button>
              
              <input 
                v-model.number="quantity"
                type="number"
                min="1"
                max="9999"
                class="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-center font-medium focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                @blur="validateQuantity"
              />
              
              <button 
                @click="incrementQuantity"
                class="w-10 h-10 rounded-lg border border-gray-300 flex items-center justify-center hover:bg-gray-50 transition-colors cursor-pointer"
              >
                <PlusIcon class="w-4 h-4" />
              </button>
            </div>
            
            <!-- Quick Add/Subtract Buttons -->
            <div class="grid grid-cols-2 gap-4">
              <!-- Subtract Buttons -->
              <div>
                <label class="block text-xs font-medium text-gray-500 mb-2">Restar cantidad</label>
                <div class="grid grid-cols-3 gap-2">
                  <button
                    v-for="amount in quickAmounts"
                    :key="`sub-${amount}`"
                    @click="subtractQuickAmount(amount)"
                    :disabled="quantity <= amount"
                    class="px-2 py-2 text-sm font-medium text-red-700 bg-red-50 hover:bg-red-100 rounded-lg transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    -{{ amount }}
                  </button>
                </div>
              </div>
              
              <!-- Add Buttons -->
              <div>
                <label class="block text-xs font-medium text-gray-500 mb-2">Sumar cantidad</label>
                <div class="grid grid-cols-3 gap-2">
                  <button
                    v-for="amount in quickAmounts"
                    :key="`add-${amount}`"
                    @click="addQuickAmount(amount)"
                    class="px-2 py-2 text-sm font-medium text-green-700 bg-green-50 hover:bg-green-100 rounded-lg transition-colors cursor-pointer"
                  >
                    +{{ amount }}
                  </button>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Price Summary -->
          <div class="bg-gray-50 rounded-lg p-4 mb-6">
            <div class="flex justify-between items-center mb-2">
              <span class="text-sm text-gray-600">Precio unitario:</span>
              <span class="font-medium">${{ product?.precio?.toFixed(2) || '0.00' }}</span>
            </div>
            <div class="flex justify-between items-center mb-2">
              <span class="text-sm text-gray-600">Cantidad:</span>
              <span class="font-medium">{{ quantity }}</span>
            </div>
            <div class="border-t border-gray-200 pt-2">
              <div class="flex justify-between items-center">
                <span class="font-semibold text-gray-900">Total:</span>
                <span class="text-xl font-bold text-gray-900">
                  ${{ totalPrice.toFixed(2) }}
                </span>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Actions -->
        <div class="sticky bottom-0 bg-white border-t border-gray-100 px-6 py-4 rounded-b-2xl">
          <div class="flex gap-3">
            <button 
              @click="closeModal"
              class="flex-1 px-4 py-3 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg font-medium transition-colors cursor-pointer"
            >
              Cancelar
            </button>
            <button 
              @click="addToCart"
              class="flex-1 px-4 py-3 text-white rounded-lg font-medium transition-colors cursor-pointer"
              :style="{ background: 'var(--theme-primary)' }"
            >
              Agregar a mi lista
            </button>
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useCartStore } from '@/stores/cart'
import { 
  XMarkIcon, 
  ShoppingBagIcon,
  PlusIcon,
  MinusIcon
} from '@heroicons/vue/24/outline'

interface Props {
  isOpen: boolean
  product: any
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
  added: [product: any, quantity: number]
}>()

// Store
const cartStore = useCartStore()

// State
const quantity = ref(1)

// Quick add amounts for mayoristas
const quickAmounts = [1, 5, 10, 25, 50, 100]

// Computed
const totalPrice = computed(() => {
  return (props.product?.precio || 0) * quantity.value
})

// Methods
const incrementQuantity = () => {
  quantity.value += 1
}

const decrementQuantity = () => {
  if (quantity.value > 1) {
    quantity.value -= 1
  }
}

const addQuickAmount = (amount: number) => {
  quantity.value += amount
}

const subtractQuickAmount = (amount: number) => {
  if (quantity.value > amount) {
    quantity.value -= amount
  } else {
    quantity.value = 1
  }
}

const validateQuantity = () => {
  if (quantity.value < 1) {
    quantity.value = 1
  } else if (quantity.value > 9999) {
    quantity.value = 9999
  }
}

const addToCart = () => {
  if (props.product && quantity.value > 0) {
    cartStore.addItem(props.product, quantity.value)
    emit('added', props.product, quantity.value)
    closeModal()
  }
}

const closeModal = () => {
  emit('close')
  // Reset quantity when closing
  setTimeout(() => {
    quantity.value = 1
  }, 150)
}

// Reset quantity when product changes
watch(() => props.product, () => {
  quantity.value = 1
})

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

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>