<!-- OrdersView.vue - Vista del historial de pedidos -->
<template>
  <div class="min-h-screen bg-black">
    <!-- Header spacer -->
    <div class="h-12 md:h-14 lg:h-16"></div>
    
    <div class="max-w-6xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center gap-4">
          <RouterLink 
            to="/catalogo"
            class="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-300 hover:text-white transition-colors duration-200"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/>
            </svg>
            Volver al Catálogo
          </RouterLink>
        </div>
        <h1 class="text-3xl font-bold text-white mt-4">Mis Pedidos</h1>
        <p class="mt-2 text-gray-300">Historial de pedidos realizados</p>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center py-12">
        <div class="flex items-center gap-3 text-gray-300">
          <svg class="animate-spin h-8 w-8" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 714 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <span>Cargando pedidos...</span>
        </div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="rounded-md bg-red-900 p-4 border border-red-700">
        <div class="flex">
          <div class="flex-shrink-0">
            <svg class="h-5 w-5 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
          </div>
          <div class="ml-3">
            <h3 class="text-sm font-medium text-red-200">Error al cargar pedidos</h3>
            <p class="text-sm text-red-300 mt-1">{{ error }}</p>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else-if="orders.length === 0" class="text-center py-12">
        <svg class="w-24 h-24 text-gray-600 mx-auto mb-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
        </svg>
        <h3 class="text-xl font-medium text-gray-400 mb-2">No tienes pedidos aún</h3>
        <p class="text-gray-500 mb-6">Cuando realices pedidos aparecerán aquí</p>
        <RouterLink 
          to="/catalogo"
          class="inline-flex items-center gap-2 px-6 py-3 text-white rounded-lg font-medium transition-colors"
          :style="{ background: 'var(--theme-accent)' }"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"/>
          </svg>
          Explorar Productos
        </RouterLink>
      </div>

      <!-- Orders List -->
      <div v-else class="space-y-6">
        <div 
          v-for="order in orders" 
          :key="order.id"
          class="bg-gray-900 rounded-lg border border-gray-700 overflow-hidden"
        >
          <!-- Order Header (Clickeable) -->
          <div 
            @click="toggleOrder(order.id)"
            class="px-6 py-4 bg-gray-800 border-b border-gray-700 cursor-pointer hover:bg-gray-750 transition-colors"
          >
            <div class="flex items-center justify-between">
              <div class="flex-1">
                <div class="flex items-center gap-3">
                  <h3 class="text-lg font-semibold text-white">
                    Pedido #{{ order.numero }}
                  </h3>
                  <!-- Expand/Collapse Icon -->
                  <div class="transition-transform duration-200" :class="{ 'rotate-180': isOrderExpanded(order.id) }">
                    <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
                    </svg>
                  </div>
                </div>
                <div class="mt-1 space-y-1">
                  <p class="text-sm text-gray-400">
                    <span class="font-medium">Pedido:</span> {{ formatDate(order.fecha_pedido) }}
                  </p>
                  <p v-if="order.fecha_entrega" class="text-sm text-gray-400">
                    <span class="font-medium">Entrega:</span> {{ formatDeliveryDate(order.fecha_entrega) }}
                    <span v-if="order.horario_entrega" class="ml-2">- {{ order.horario_entrega }}</span>
                  </p>
                  <p v-if="order.direccion_entrega" class="text-sm text-gray-400">
                    <span class="font-medium">Dirección:</span> {{ order.direccion_entrega }}
                  </p>
                </div>
              </div>
              <div class="text-right ml-4">
                <div class="text-2xl font-bold text-white">
                  ${{ order.monto_total.toFixed(2) }}
                </div>
                <div class="flex items-center gap-2 justify-end mt-2">
                  <div class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                       :class="getStatusClasses(order.estado)">
                    {{ order.estado }}
                  </div>
                  <button 
                    v-if="canCancelOrder(order.estado)"
                    @click.stop="openCancelModal(order)"
                    class="px-3 py-1 text-xs font-medium text-red-600 bg-red-100 hover:bg-red-200 rounded-lg transition-colors cursor-pointer"
                  >
                    Cancelar
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Order Items (Expandable) -->
          <Transition name="expand">
            <div v-if="isOrderExpanded(order.id)" class="border-t border-gray-700">
              <div class="px-6 py-4">
                <h4 class="text-sm font-medium text-gray-300 mb-3">Productos ({{ order.items.length }})</h4>
                <div class="space-y-3">
                  <div 
                    v-for="item in order.items" 
                    :key="item.id"
                    class="flex items-center justify-between py-2 border-b border-gray-700 last:border-b-0"
                  >
                    <div class="flex-1">
                      <p class="text-white font-medium">{{ item.nombre_producto }}</p>
                      <p class="text-sm text-gray-400">
                        Código: {{ item.codigo_producto }}
                      </p>
                      <p class="text-sm text-gray-400">
                        {{ item.cantidad }} × ${{ item.precio_unitario.toFixed(2) }}
                      </p>
                      <p v-if="item.observaciones" class="text-xs text-gray-500 mt-1">
                        Obs: {{ item.observaciones }}
                      </p>
                    </div>
                    <div class="text-white font-semibold">
                      ${{ item.subtotal.toFixed(2) }}
                    </div>
                  </div>
                </div>
                
                <!-- Order Notes -->
                <div v-if="order.observaciones" class="mt-4 p-3 bg-gray-800 rounded-lg">
                  <h5 class="text-sm font-medium text-gray-300 mb-1">Observaciones del pedido</h5>
                  <p class="text-sm text-gray-400">{{ order.observaciones }}</p>
                </div>
              </div>
            </div>
          </Transition>
        </div>
      </div>
    </div>
    
    <!-- Modal de Cancelación -->
    <Transition name="modal">
      <div v-if="showCancelModal" class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-black/60" @click="closeCancelModal"></div>
        <div class="relative bg-white rounded-lg shadow-xl max-w-md w-full p-6">
          <h3 class="text-lg font-semibold text-gray-900 mb-4">Cancelar Pedido</h3>
          <p class="text-gray-600 mb-4">
            ¿Estás seguro que deseas cancelar el pedido #{{ selectedOrder?.numero }}?
          </p>
          <div class="mb-4">
            <label for="motivo" class="block text-sm font-medium text-gray-700 mb-2">
              Motivo de cancelación (opcional)
            </label>
            <textarea
              v-model="cancelReason"
              id="motivo"
              rows="3"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Ingresa el motivo..."
            ></textarea>
          </div>
          <div class="flex gap-3">
            <button
              @click="closeCancelModal"
              class="flex-1 px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg font-medium transition-colors cursor-pointer"
            >
              Volver
            </button>
            <button
              @click="confirmCancelOrder"
              :disabled="cancellingOrder"
              class="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium transition-colors cursor-pointer disabled:opacity-50"
            >
              {{ cancellingOrder ? 'Cancelando...' : 'Confirmar Cancelación' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useCartStore, type OrderResponse } from '@/stores/cart'
import { authApiService } from '@/services/api'

const router = useRouter()
const authStore = useAuthStore()
const cartStore = useCartStore()

// State
const loading = ref(false)
const error = ref<string | null>(null)
const orders = ref<OrderResponse[]>([])
const showCancelModal = ref(false)
const selectedOrder = ref<OrderResponse | null>(null)
const cancelReason = ref('')
const cancellingOrder = ref(false)
const expandedOrders = ref<Set<number>>(new Set())

// Methods
const loadOrders = async () => {
  if (!authStore.isAuthenticated || !authStore.token) {
    router.push('/login')
    return
  }

  loading.value = true
  error.value = null

  try {
    const ordersList = await authStore.executeWithAuth(
      (token) => cartStore.getOrderHistory(token)
    )
    orders.value = ordersList
    console.log('Loaded orders:', ordersList.length, 'orders')
  } catch (err) {
    console.error('Error loading orders:', err)
    error.value = err instanceof Error ? err.message : 'Error al cargar los pedidos'
    
    // Si la sesión expiró, redirigir al login
    if (err instanceof Error && err.message.includes('Sesión expirada')) {
      router.push('/login')
    }
  } finally {
    loading.value = false
  }
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('es-AR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatDeliveryDate = (dateString?: string) => {
  if (!dateString) return null
  const date = new Date(dateString)
  return date.toLocaleDateString('es-AR', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const isOrderExpanded = (orderId: number) => {
  return expandedOrders.value.has(orderId)
}

const toggleOrder = (orderId: number) => {
  if (expandedOrders.value.has(orderId)) {
    expandedOrders.value.delete(orderId)
  } else {
    expandedOrders.value.add(orderId)
  }
}

const getStatusClasses = (status: string) => {
  switch (status.toLowerCase()) {
    case 'pendiente':
      return 'bg-yellow-100 text-yellow-800'
    case 'confirmado':
      return 'bg-blue-100 text-blue-800'
    case 'en preparacion':
    case 'en_preparacion':
      return 'bg-orange-100 text-orange-800'
    case 'listo':
      return 'bg-green-100 text-green-800'
    case 'entregado':
      return 'bg-green-100 text-green-800'
    case 'cancelado':
      return 'bg-red-100 text-red-800'
    default:
      return 'bg-gray-100 text-gray-800'
  }
}

const canCancelOrder = (status: string): boolean => {
  const lowerStatus = status.toLowerCase()
  return lowerStatus === 'pendiente' || lowerStatus === 'confirmado'
}

const openCancelModal = (order: OrderResponse) => {
  selectedOrder.value = order
  showCancelModal.value = true
  cancelReason.value = ''
}

const closeCancelModal = () => {
  showCancelModal.value = false
  selectedOrder.value = null
  cancelReason.value = ''
}

const confirmCancelOrder = async () => {
  if (!selectedOrder.value || !authStore.token) return
  
  cancellingOrder.value = true
  error.value = null
  
  try {
    await authStore.executeWithAuth(
      (token) => authApiService.cancelOrder(
        selectedOrder.value!.id,
        cancelReason.value || 'Cancelado por el cliente',
        token
      )
    )
    
    // Actualizar el estado del pedido en la lista
    const orderIndex = orders.value.findIndex(o => o.id === selectedOrder.value!.id)
    if (orderIndex !== -1) {
      orders.value[orderIndex].estado = 'Cancelado'
    }
    
    closeCancelModal()
  } catch (err) {
    console.error('Error cancelling order:', err)
    error.value = err instanceof Error ? err.message : 'Error al cancelar el pedido'
  } finally {
    cancellingOrder.value = false
  }
}

// Lifecycle
onMounted(async () => {
  // Verificar autenticación
  if (!authStore.isAuthenticated) {
    router.push('/login')
    return
  }
  
  await loadOrders()
})
</script>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: all 0.15s ease;
}

.modal-enter-from {
  opacity: 0;
  transform: scale(0.9);
}

.modal-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

.expand-enter-active,
.expand-leave-active {
  transition: all 0.3s ease;
  overflow: hidden;
}

.expand-enter-from,
.expand-leave-to {
  max-height: 0;
  opacity: 0;
}

.expand-enter-to,
.expand-leave-from {
  max-height: 500px;
  opacity: 1;
}

/* Hover effect para el header del pedido */
.hover\:bg-gray-750:hover {
  background-color: #374151;
}
</style>