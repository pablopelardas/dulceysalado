<template>
  <Transition name="modal-fade">
    <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <!-- Backdrop -->
      <div 
        class="absolute inset-0 bg-black/60"
        @click="closeModal"
      ></div>
      
      <!-- Modal -->
      <div class="relative bg-white rounded-2xl shadow-2xl max-w-xl w-full max-h-[90vh] overflow-y-auto">
        <!-- Header -->
        <div class="sticky top-0 bg-white border-b border-gray-100 px-6 py-4 rounded-t-2xl">
          <div class="flex items-center justify-between">
            <h3 class="text-xl font-semibold text-gray-900">
              Confirmar Pedido
            </h3>
            <button 
              @click="closeModal"
              class="p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
            >
              <XMarkIcon class="w-5 h-5 text-gray-500" />
            </button>
          </div>
        </div>
        
        <!-- Content -->
        <div class="p-6">
          <!-- Authentication Check -->
          <div v-if="!authStore.isAuthenticated" class="text-center">
            <div class="mb-6">
              <UserIcon class="w-16 h-16 text-gray-300 mx-auto mb-4" />
              <h4 class="text-lg font-medium text-gray-900 mb-2">Iniciar Sesión Requerido</h4>
              <p class="text-gray-600">Debes iniciar sesión para enviar pedidos</p>
            </div>
            <div class="flex gap-3">
              <button 
                @click="closeModal"
                class="flex-1 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
              >
                Cancelar
              </button>
              <RouterLink 
                to="/login"
                @click="closeModal"
                class="flex-1 px-4 py-2 text-white rounded-lg text-center font-medium transition-colors"
                :style="{ background: 'var(--theme-accent)' }"
              >
                Iniciar Sesión
              </RouterLink>
            </div>
          </div>

          <!-- Order Form -->
          <div v-else>
            <!-- Order Summary -->
            <div class="mb-6 p-4 bg-gray-50 rounded-lg">
              <h4 class="font-medium text-gray-900 mb-2">Resumen del pedido</h4>
              <div class="text-sm text-gray-600 space-y-1">
                <div class="flex justify-between">
                  <span>Productos:</span>
                  <span>{{ cartStore.itemCount }}</span>
                </div>
                <div class="flex justify-between">
                  <span>Unidades:</span>
                  <span>{{ cartStore.totalItems }}</span>
                </div>
                <div class="flex justify-between font-semibold text-gray-900 pt-1 border-t border-gray-200">
                  <span>Total:</span>
                  <span>${{ cartStore.totalAmount.toFixed(2) }}</span>
                </div>
              </div>
            </div>

            <!-- Order Form -->
            <form @submit.prevent="handleSubmit" class="space-y-6">
              <!-- Tipo de Entrega -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">
                  Tipo de entrega
                </label>
                <div class="grid grid-cols-2 gap-3">
                  <button
                    type="button"
                    @click="tipoEntrega = 'envio'"
                    :class="[
                      'px-4 py-3 rounded-lg border-2 transition-all font-medium cursor-pointer',
                      tipoEntrega === 'envio'
                        ? 'border-red-500 bg-red-50 text-red-700'
                        : 'border-gray-200 bg-white text-gray-700 hover:bg-gray-50'
                    ]"
                  >
                    <svg class="w-5 h-5 mx-auto mb-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                    </svg>
                    Envío a domicilio
                  </button>
                  <button
                    type="button"
                    @click="tipoEntrega = 'retiro'"
                    :class="[
                      'px-4 py-3 rounded-lg border-2 transition-all font-medium cursor-pointer',
                      tipoEntrega === 'retiro'
                        ? 'border-red-500 bg-red-50 text-red-700'
                        : 'border-gray-200 bg-white text-gray-700 hover:bg-gray-50'
                    ]"
                  >
                    <svg class="w-5 h-5 mx-auto mb-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                    </svg>
                    Retiro en el local
                  </button>
                </div>
              </div>
              <!-- Delivery Address -->
              <div v-if="tipoEntrega === 'envio'">
                <label class="block text-sm font-medium text-gray-700 mb-2">
                  Dirección de entrega
                </label>
                
                <!-- Address Selection -->
                <div class="space-y-4">
                  <!-- Use Profile Address Option -->
                  <div v-if="userAddress" class="p-4 border border-gray-200 rounded-lg bg-gray-50">
                    <div class="flex items-start gap-3">
                      <input
                        id="use_profile_address"
                        v-model="useProfileAddress"
                        type="checkbox"
                        class="mt-0.5 h-4 w-4 text-red-600 focus:ring-red-500 border-gray-300 rounded cursor-pointer"
                        @change="handleAddressChange"
                      />
                      <div class="flex-1 min-w-0">
                        <label for="use_profile_address" class="text-sm font-medium text-gray-900 cursor-pointer block">
                          Usar mi dirección guardada
                        </label>
                        <p class="text-sm text-gray-600 mt-1 break-words">{{ userAddress }}</p>
                      </div>
                    </div>
                  </div>
                  
                  <!-- Custom Address Input -->
                  <div>
                    <input
                      id="direccion_entrega"
                      v-model="orderForm.direccion_entrega"
                      type="text"
                      class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 disabled:bg-gray-100 disabled:text-gray-500"
                      :placeholder="useProfileAddress && userAddress ? 'Dirección cargada desde tu perfil' : 'Ej: Av. Corrientes 1234, CABA'"
                      :disabled="loading || (useProfileAddress && !!userAddress)"
                    />
                  </div>
                  
                  <p class="text-xs text-gray-500 leading-relaxed">
                    <span v-if="userAddress">
                      {{ useProfileAddress ? 'Puedes desmarcar la opción de arriba para usar otra dirección.' : 'Marca la opción de arriba para usar tu dirección guardada.' }}
                    </span>
                    <span v-else>
                      Ingresa la dirección donde quieres recibir tu pedido.
                    </span>
                  </p>
                </div>

                <!-- Boton para ver zona de cobertura -->
                <div class="mt-3">
                  <button
                    type="button"
                    @click="showDeliveryMap = !showDeliveryMap"
                    class="text-sm text-red-600 hover:text-red-700 font-medium flex items-center gap-1 cursor-pointer"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                    {{ showDeliveryMap ? 'Ocultar' : 'Ver' }} zona de cobertura
                  </button>

                  <!-- Mapa de zona de entrega -->
                  <DeliveryZoneMap v-if="showDeliveryMap" class="mt-3" />
                </div>
              </div>

              <!-- Mensaje de retiro en local -->
              <div v-else class="p-4 bg-blue-50 rounded-lg border border-blue-200">
                <div class="flex items-start gap-3">
                  <svg class="w-5 h-5 text-blue-600 mt-0.5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <div>
                    <p class="text-sm font-medium text-blue-900">
                      Retiro en el local
                    </p>
                    <p class="text-sm text-blue-700 mt-1">
                      <strong>Dirección:</strong> Av. Bernardo Ader 161, Boulogne
                    </p>
                    <p class="text-sm text-blue-700">
                      B1609 - San Isidro, Provincia de Buenos Aires
                    </p>
                    <p class="text-sm text-blue-700 mt-1">
                      <strong>Horario:</strong> Lunes a Viernes de 9:00 a 19:00, Sábados de 9:00 a 14:00
                    </p>
                  </div>
                </div>
              </div>

              <!-- Delivery Slot Selection (solo para envío) -->
              <DeliverySlotSelector
                v-if="tipoEntrega === 'envio'"
                v-model="deliverySlotData"
                :disabled="loading"
              />

              <!-- Observations -->
              <div>
                <label for="observaciones" class="block text-sm font-medium text-gray-700 mb-2">
                  Observaciones (opcional)
                </label>
                <textarea
                  id="observaciones"
                  v-model="orderForm.observaciones"
                  rows="3"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 resize-none"
                  placeholder="Instrucciones especiales, comentarios, etc."
                  :disabled="loading"
                ></textarea>
              </div>

              <!-- Error message -->
              <div v-if="error" class="rounded-md bg-red-50 p-4">
                <div class="flex">
                  <div class="flex-shrink-0">
                    <svg class="h-5 w-5 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
                    </svg>
                  </div>
                  <div class="ml-3">
                    <p class="text-sm font-medium text-red-800">
                      {{ error }}
                    </p>
                  </div>
                </div>
              </div>

              <!-- Success message -->
              <div v-if="success" class="rounded-md bg-green-50 p-4">
                <div class="flex">
                  <div class="flex-shrink-0">
                    <svg class="h-5 w-5 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
                    </svg>
                  </div>
                  <div class="ml-3">
                    <p class="text-sm font-medium text-green-800">
                      {{ success }}
                    </p>
                  </div>
                </div>
              </div>

              <!-- Actions -->
              <div class="flex gap-3 pt-4">
                <button 
                  type="button"
                  @click="closeModal"
                  :disabled="loading"
                  class="flex-1 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
                >
                  Cancelar
                </button>
                <button 
                  type="submit"
                  :disabled="loading || cartStore.isEmpty"
                  class="flex-1 px-4 py-2 text-white rounded-lg font-medium transition-colors disabled:opacity-50"
                  :style="{ background: 'var(--theme-accent)' }"
                >
                  <span v-if="loading" class="flex items-center justify-center">
                    <svg class="animate-spin -ml-1 mr-3 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 714 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Enviando...
                  </span>
                  <span v-else>Confirmar Pedido</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { useCartStore, type OrderData } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'
import { 
  XMarkIcon,
  UserIcon
} from '@heroicons/vue/24/outline'
import DeliverySlotSelector from '@/components/delivery/DeliverySlotSelector.vue'
import DeliveryZoneMap from '@/components/delivery/DeliveryZoneMap.vue'

interface Props {
  isOpen: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
  orderCreated: [orderNumber: string]
}>()

// Stores
const cartStore = useCartStore()
const authStore = useAuthStore()

// State
const loading = ref(false)
const error = ref<string | null>(null)
const success = ref<string | null>(null)
const useProfileAddress = ref(true) // Default to using profile address

// Form data
const orderForm = ref<OrderData>({
  observaciones: '',
  direccion_entrega: '',
  fecha_entrega: '',
  horario_entrega: '',
  tipo_entrega: 'envio' // 'envio' o 'retiro'
})

// Estado del tipo de entrega
const tipoEntrega = ref<'envio' | 'retiro'>('envio')
const showDeliveryMap = ref(false)

// Delivery slot data
const deliverySlotData = ref({
  fecha_entrega: '',
  horario_entrega: '',
  delivery_slot: ''
})

// Computed
const today = computed(() => {
  const date = new Date()
  return date.toISOString().split('T')[0]
})

const userAddress = computed(() => {
  return authStore.user?.direccion || null
})

// Methods
const closeModal = () => {
  emit('close')
}

const resetForm = () => {
  orderForm.value = {
    observaciones: '',
    direccion_entrega: '',
    fecha_entrega: '',
    horario_entrega: '',
    tipo_entrega: 'envio'
  }
  tipoEntrega.value = 'envio'
  deliverySlotData.value = {
    fecha_entrega: '',
    horario_entrega: '',
    delivery_slot: ''
  }
  error.value = null
  success.value = null
}

const handleAddressChange = () => {
  if (useProfileAddress.value && userAddress.value) {
    orderForm.value.direccion_entrega = userAddress.value
  } else {
    orderForm.value.direccion_entrega = ''
  }
}

const handleSubmit = async () => {
  if (!authStore.isAuthenticated || !authStore.token) {
    error.value = 'Debes iniciar sesión para enviar pedidos'
    return
  }

  if (cartStore.isEmpty) {
    error.value = 'El carrito está vacío'
    return
  }

  loading.value = true
  error.value = null
  success.value = null

  try {
    // Si es retiro en el local, setear la dirección automáticamente
    if (tipoEntrega.value === 'retiro') {
      orderForm.value.direccion_entrega = 'Retiro en el local'
      orderForm.value.tipo_entrega = 'retiro'
    } else {
      orderForm.value.tipo_entrega = 'envio'
    }

    // Merge delivery slot data with order form
    const orderData = {
      ...orderForm.value,
      fecha_entrega: deliverySlotData.value.fecha_entrega || orderForm.value.fecha_entrega,
      horario_entrega: deliverySlotData.value.horario_entrega || orderForm.value.horario_entrega,
      delivery_slot: deliverySlotData.value.delivery_slot || undefined
    }

    const orderResponse = await authStore.executeWithAuth(
      (token) => cartStore.createOrder(orderData, token)
    )
    
    success.value = `Pedido #${orderResponse.numero} creado exitosamente`
    
    // Emit order created event
    emit('orderCreated', orderResponse.numero)
    
    // Close modal after a delay
    setTimeout(() => {
      closeModal()
    }, 2000)
    
  } catch (err) {
    console.error('Error creating order:', err)
    error.value = err instanceof Error ? err.message : 'Error al crear el pedido'
    
    // Si la sesión expiró, cerrar modal y redirigir
    if (err instanceof Error && err.message.includes('Sesión expirada')) {
      closeModal()
      // El executeWithAuth ya hace logout, solo necesitamos informar al usuario
    }
  } finally {
    loading.value = false
  }
}

// Prevent body scroll when modal is open and initialize address
watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    document.body.style.overflow = 'hidden'
    resetForm()
    // Initialize address from profile if available
    if (userAddress.value && useProfileAddress.value) {
      orderForm.value.direccion_entrega = userAddress.value
    }
  } else {
    document.body.style.overflow = ''
  }
})

// Watch for user address changes
watch(userAddress, (newAddress) => {
  if (newAddress && useProfileAddress.value) {
    orderForm.value.direccion_entrega = newAddress
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