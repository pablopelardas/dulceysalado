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
      <div v-else-if="error" class="rounded-md bg-white/95 p-4 border border-gray-200 shadow-lg">
        <div class="flex">
          <div class="flex-shrink-0">
            <svg class="h-5 w-5 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
          </div>
          <div class="ml-3">
            <h3 class="text-sm font-medium text-red-600">Error al cargar pedidos</h3>
            <p class="text-sm text-red-500 mt-1">{{ error }}</p>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else-if="orders.length === 0" class="text-center py-12 bg-white/95 rounded-lg shadow-lg">
        <svg class="w-24 h-24 text-gray-400 mx-auto mb-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
        </svg>
        <h3 class="text-xl font-medium text-gray-700 mb-2">No tienes pedidos aún</h3>
        <p class="text-gray-600 mb-6">Cuando realices pedidos aparecerán aquí</p>
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
          class="bg-white/95 rounded-lg shadow-lg overflow-hidden"
        >
          <!-- Order Header (Clickeable) -->
          <div 
            @click="toggleOrder(order.id)"
            class="px-6 py-4 bg-gray-50 border-b border-gray-200 cursor-pointer hover:bg-gray-100 transition-colors"
          >
            <div class="flex items-center justify-between">
              <div class="flex-1">
                <div class="flex items-center gap-3">
                  <h3 class="text-lg font-semibold text-gray-900">
                    Pedido #{{ order.numero }}
                  </h3>
                  <!-- Expand/Collapse Icon -->
                  <div class="transition-transform duration-200" :class="{ 'rotate-180': isOrderExpanded(order.id) }">
                    <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
                    </svg>
                  </div>
                </div>
                <div class="mt-1 space-y-1">
                  <p class="text-sm text-gray-600">
                    <span class="font-medium">Pedido:</span> {{ formatDate(order.fecha_pedido) }}
                  </p>
                  <p v-if="order.fecha_entrega" class="text-sm text-gray-600">
                    <span class="font-medium">Entrega:</span> {{ formatDeliveryDate(order.fecha_entrega) }}
                    <span v-if="order.horario_entrega" class="ml-2">- {{ order.horario_entrega }}</span>
                  </p>
                  <p v-if="order.direccion_entrega" class="text-sm text-gray-600">
                    <span class="font-medium">Dirección:</span> {{ order.direccion_entrega }}
                  </p>
                </div>
              </div>
              <div class="text-right ml-4">
                <div class="text-2xl font-bold text-gray-900">
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
                    class="px-4 py-2 text-xs font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors cursor-pointer shadow-sm"
                  >
                    Cancelar
                  </button>
                  <button 
                    v-if="hasCorrection(order.estado)"
                    @click.stop="openCorreccionModal(order)"
                    class="px-4 py-2 text-xs font-medium text-white bg-purple-600 hover:bg-purple-700 rounded-lg transition-colors cursor-pointer shadow-sm"
                  >
                    Ver Corrección
                  </button>
                  <div v-if="canApproveCorrection(order.estado)" class="flex gap-1">
                    <button 
                      @click.stop="approveCorrection(order.id)"
                      :disabled="processingCorreccion"
                      class="px-4 py-2 text-xs font-medium text-white bg-green-600 hover:bg-green-700 rounded-lg transition-colors cursor-pointer shadow-sm disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      Aprobar
                    </button>
                    <button 
                      @click.stop="rejectCorrection(order.id)"
                      :disabled="processingCorreccion"
                      class="px-4 py-2 text-xs font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors cursor-pointer shadow-sm disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      Rechazar
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Order Items (Expandable) -->
          <Transition name="expand">
            <div v-if="isOrderExpanded(order.id)" class="border-t border-gray-700">
              <div class="px-6 py-4">
                <h4 class="text-sm font-medium text-gray-900 mb-3">Productos ({{ order.items.length }})</h4>
                <div class="space-y-3">
                  <div 
                    v-for="item in order.items" 
                    :key="item.id"
                    class="flex items-center justify-between py-2 border-b border-gray-700 last:border-b-0"
                  >
                    <div class="flex-1">
                      <p class="text-gray-900 font-medium">{{ item.nombre_producto }}</p>
                      <p class="text-sm text-gray-600">
                        Código: {{ item.codigo_producto }}
                      </p>
                      <p class="text-sm text-gray-700">
                        {{ item.cantidad }} × ${{ item.precio_unitario.toFixed(2) }}
                      </p>
                      <p v-if="item.observaciones" class="text-xs text-gray-500 mt-1">
                        Obs: {{ item.observaciones }}
                      </p>
                    </div>
                    <div class="text-gray-900 font-semibold">
                      ${{ item.subtotal.toFixed(2) }}
                    </div>
                  </div>
                </div>
                
                <!-- Order Notes -->
                <div v-if="order.observaciones" class="mt-4 p-3 bg-gray-50 rounded-lg">
                  <h5 class="text-sm font-medium text-gray-700 mb-1">Observaciones del pedido</h5>
                  <p class="text-sm text-gray-600">{{ order.observaciones }}</p>
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
              class="flex-1 px-4 py-2 bg-gray-600 hover:bg-gray-700 text-white rounded-lg font-medium transition-colors cursor-pointer shadow-sm"
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
    
    <!-- Modal de Corrección -->
    <Transition name="modal">
      <div v-if="showCorreccionModal" class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-black/60" @click="closeCorreccionModal"></div>
        <div class="relative bg-white/95 rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
          <!-- Header -->
          <div class="px-6 py-4 border-b border-gray-200">
            <div class="flex items-center justify-between">
              <h3 class="text-lg font-semibold text-gray-900">Corrección de Pedido #{{ selectedOrder?.numero }}</h3>
              <button @click="closeCorreccionModal" class="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-colors">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
                </svg>
              </button>
            </div>
          </div>
          
          <!-- Body -->
          <div class="p-6">
            <!-- Loading state -->
            <div v-if="!selectedCorreccion" class="text-center py-8">
              <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-purple-500 mx-auto"></div>
              <p class="text-gray-600 mt-4">Cargando detalles de corrección...</p>
            </div>

            <!-- Corrección válida -->
            <div v-else class="space-y-6">
              <!-- Info del pedido -->
              <div class="bg-gray-50 rounded-lg p-6 border border-gray-200">
                <div class="flex items-center justify-between mb-4">
                  <div>
                    <h2 class="text-xl font-semibold text-gray-900">Pedido #{{ selectedCorreccion.pedido_numero }}</h2>
                    <p class="text-sm text-gray-600">{{ selectedCorreccion.cliente_nombre }}</p>
                  </div>
                  <div class="text-right">
                    <div class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium"
                         :class="getStatusClasses(selectedOrder?.estado || '')">
                      {{ selectedOrder?.estado }}
                    </div>
                  </div>
                </div>

                <!-- Motivo si existe -->
                <div v-if="selectedCorreccion.pedido_corregido.motivo_correccion" class="bg-yellow-50 border border-yellow-300 rounded-lg p-4 mb-6">
                  <h4 class="font-medium text-yellow-700 mb-2">📝 Motivo de la corrección:</h4>
                  <p class="text-yellow-600">{{ selectedCorreccion.pedido_corregido.motivo_correccion }}</p>
                </div>
              </div>

              <!-- Comparación de items -->
              <div class="bg-white/95 rounded-lg shadow-lg overflow-hidden border border-gray-200">
                <div class="px-6 py-4 border-b border-gray-200 bg-gray-50">
                  <h3 class="text-lg font-medium text-gray-900">Cambios en tu pedido</h3>
                </div>

                <div class="divide-y divide-gray-200">
                  <div v-for="(itemOriginal, index) in selectedCorreccion.pedido_original.items" :key="itemOriginal.codigo_producto" class="p-6">
                    <div class="flex items-start justify-between">
                      <div class="flex-1">
                        <h4 class="font-medium text-gray-900">{{ itemOriginal.nombre_producto }}</h4>
                        <p class="text-sm text-gray-500">Código: {{ itemOriginal.codigo_producto }}</p>
                        <p class="text-sm text-gray-600">Precio: ${{ itemOriginal.precio_unitario.toLocaleString() }}</p>
                      </div>

                      <div class="text-right ml-4">
                        <!-- Item encontrado en corrección -->
                        <template v-if="getItemCorregido(itemOriginal.codigo_producto)">
                          <div class="space-y-2">
                            <!-- Cantidad original -->
                            <div class="text-sm">
                              <span class="text-gray-600">Original: </span>
                              <span class="font-medium ml-1">{{ itemOriginal.cantidad }} unidades</span>
                              <span class="text-gray-600 ml-2">${{ itemOriginal.subtotal.toLocaleString() }}</span>
                            </div>
                            
                            <!-- Cantidad corregida - Solo si hay cambios -->
                            <div v-if="itemOriginal.cantidad !== getItemCorregido(itemOriginal.codigo_producto)!.cantidad" class="text-sm">
                              <span class="text-green-600 font-medium">Ajustado: </span>
                              <span class="font-medium text-green-600 ml-1">{{ getItemCorregido(itemOriginal.codigo_producto)!.cantidad }} unidades</span>
                              <span class="text-green-600 ml-2">${{ getItemCorregido(itemOriginal.codigo_producto)!.subtotal.toLocaleString() }}</span>
                            </div>

                            <!-- Motivo si existe -->
                            <div v-if="getItemCorregido(itemOriginal.codigo_producto)!.motivo" class="text-xs text-orange-600 bg-orange-50 px-2 py-1 rounded mt-1">
                              {{ getItemCorregido(itemOriginal.codigo_producto)!.motivo }}
                            </div>

                            <!-- Diferencia -->
                            <div v-if="itemOriginal.cantidad !== getItemCorregido(itemOriginal.codigo_producto)!.cantidad" 
                                 class="text-xs font-medium mt-1"
                                 :class="itemOriginal.cantidad > getItemCorregido(itemOriginal.codigo_producto)!.cantidad ? 'text-red-600' : 'text-green-600'">
                              {{ itemOriginal.cantidad > getItemCorregido(itemOriginal.codigo_producto)!.cantidad ? '-' : '+' }}{{ Math.abs(itemOriginal.cantidad - getItemCorregido(itemOriginal.codigo_producto)!.cantidad) }} unidades
                            </div>
                          </div>
                        </template>

                        <!-- Item eliminado -->
                        <template v-else>
                          <div class="space-y-2">
                            <div class="text-sm">
                              <span class="text-gray-600">Original:</span>
                              <span class="font-medium line-through">{{ itemOriginal.cantidad }} unidades</span>
                            </div>
                            <div class="text-sm text-red-600 font-medium">
                              ❌ Producto no disponible
                            </div>
                          </div>
                        </template>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Resumen de totales -->
                <div class="px-6 py-4 border-t border-gray-200 bg-gray-50">
                  <div class="flex justify-between text-sm mb-2">
                    <span class="text-gray-600">Total original:</span>
                    <span class="font-medium">${{ selectedCorreccion.pedido_original.monto_total.toLocaleString() }}</span>
                  </div>
                  <div class="flex justify-between text-lg font-bold">
                    <span class="text-gray-900 font-bold">Total ajustado:</span>
                    <span class="text-green-600 font-bold">${{ selectedCorreccion.pedido_corregido.monto_total.toLocaleString() }}</span>
                  </div>
                  <div v-if="selectedCorreccion.pedido_original.monto_total !== selectedCorreccion.pedido_corregido.monto_total" class="text-sm text-gray-500 mt-1">
                    Diferencia: ${{ Math.abs(selectedCorreccion.pedido_original.monto_total - selectedCorreccion.pedido_corregido.monto_total).toLocaleString() }}
                    {{ selectedCorreccion.pedido_original.monto_total > selectedCorreccion.pedido_corregido.monto_total ? 'menos' : 'más' }}
                  </div>
                </div>
              </div>

              <!-- Historial de correcciones -->
              <div v-if="selectedCorreccion.historial_correcciones && selectedCorreccion.historial_correcciones.length > 1" class="bg-white/95 rounded-lg shadow-lg border border-gray-200 p-6">
                <h3 class="text-lg font-medium text-gray-900 mb-4 flex items-center gap-2">
                  <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
                  </svg>
                  Historial de Correcciones
                </h3>
                <p class="text-sm text-gray-600 mb-4">Este pedido ha tenido {{ selectedCorreccion.historial_correcciones.length }} correcciones:</p>
                
                <div class="space-y-3">
                  <div 
                    v-for="(historialItem, index) in selectedCorreccion.historial_correcciones" 
                    :key="historialItem.id"
                    class="border rounded-lg p-4 transition-colors"
                    :class="{ 
                      'bg-amber-50 border-amber-200': historialItem.token === selectedCorreccion.token,
                      'bg-gray-50 border-gray-200': historialItem.token !== selectedCorreccion.token 
                    }"
                  >
                    <div class="flex items-center justify-between mb-2">
                      <div class="flex items-center gap-3">
                        <span 
                          class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium"
                          :class="{ 
                            'bg-amber-100 text-amber-800': historialItem.token === selectedCorreccion.token,
                            'bg-green-100 text-green-800': historialItem.utilizado && historialItem.token !== selectedCorreccion.token,
                            'bg-gray-100 text-gray-700': !historialItem.utilizado && historialItem.token !== selectedCorreccion.token
                          }"
                        >
                          <svg 
                            class="w-3 h-3 mr-1" 
                            fill="none" 
                            stroke="currentColor" 
                            viewBox="0 0 24 24"
                            :class="{ 
                              'text-amber-600': historialItem.token === selectedCorreccion.token,
                              'text-green-600': historialItem.utilizado && historialItem.token !== selectedCorreccion.token,
                              'text-gray-500': !historialItem.utilizado && historialItem.token !== selectedCorreccion.token
                            }"
                          >
                            <path 
                              v-if="historialItem.token === selectedCorreccion.token"
                              stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                              d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"
                            />
                            <path 
                              v-else-if="historialItem.utilizado"
                              stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                              d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                            />
                            <path 
                              v-else
                              stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                              d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                            />
                          </svg>
                          {{ historialItem.token === selectedCorreccion.token ? 'Actual' : historialItem.utilizado ? 'Completada' : 'Expirada' }}
                        </span>
                        
                        <span class="text-xs text-gray-500">
                          {{ formatCorreccionDate(historialItem.fecha_creacion) }}
                        </span>
                      </div>
                      
                      <span v-if="historialItem.token === selectedCorreccion.token" class="text-xs text-amber-600 font-medium">
                        Esta corrección
                      </span>
                    </div>
                    
                    <!-- Motivo si existe -->
                    <div v-if="historialItem.motivo_correccion" class="mt-2">
                      <p class="text-sm text-gray-700">
                        <span class="font-medium text-gray-800">Motivo:</span> 
                        <span class="text-gray-600">{{ historialItem.motivo_correccion }}</span>
                      </p>
                    </div>

                    <!-- Detalles de productos modificados si hay JSON -->
                    <div v-if="historialItem.pedido_original_json" class="mt-2">
                      <details class="group">
                        <summary class="cursor-pointer text-xs text-blue-600 hover:text-blue-800 flex items-center gap-1">
                          <svg class="w-3 h-3 group-open:rotate-90 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
                          </svg>
                          Ver productos modificados
                        </summary>
                        <div class="mt-2 p-2 bg-gray-50 rounded text-xs">
                          <div v-html="formatPedidoOriginalHtml(historialItem.pedido_original_json)"></div>
                        </div>
                      </details>
                    </div>
                    
                    <!-- Fecha de uso si está completada -->
                    <div v-if="historialItem.utilizado && historialItem.fecha_uso" class="mt-1">
                      <p class="text-xs text-green-600 flex items-center gap-1">
                        <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
                        </svg>
                        Respondida el {{ formatCorreccionDate(historialItem.fecha_uso) }}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
              
              <!-- Botones de acción para correcciones pendientes -->
              <div v-if="canApproveCorrection(selectedOrder?.estado || '')" class="flex gap-4 pt-4">
                <button
                  @click="approveCorrection(selectedOrder?.id || 0)"
                  :disabled="processingCorreccion"
                  class="flex-1 bg-green-600 hover:bg-green-700 disabled:bg-gray-600 text-white font-medium py-3 px-6 rounded-lg transition-colors"
                >
                  <span v-if="!processingCorreccion">✅ Aprobar Cambios</span>
                  <span v-else>Procesando...</span>
                </button>
                
                <button
                  @click="rejectCorrection(selectedOrder?.id || 0)"
                  :disabled="processingCorreccion"
                  class="flex-1 bg-red-600 hover:bg-red-700 disabled:bg-gray-600 text-white font-medium py-3 px-6 rounded-lg transition-colors"
                >
                  <span v-if="!processingCorreccion">❌ Rechazar Cambios</span>
                  <span v-else>Procesando...</span>
                </button>
              </div>
            </div>
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
import { correccionService, type CorreccionData } from '@/services/correccion'
import { useToast } from '@/composables/useToast'

const router = useRouter()
const authStore = useAuthStore()
const cartStore = useCartStore()
const { success: showSuccessToast, error: showErrorToast } = useToast()

// State
const loading = ref(false)
const error = ref<string | null>(null)
const orders = ref<OrderResponse[]>([])
const showCancelModal = ref(false)
const selectedOrder = ref<OrderResponse | null>(null)
const cancelReason = ref('')
const cancellingOrder = ref(false)
const expandedOrders = ref<Set<number>>(new Set())
const correcciones = ref<Map<number, CorreccionData>>(new Map())
const showCorreccionModal = ref(false)
const selectedCorreccion = ref<CorreccionData | null>(null)
const processingCorreccion = ref(false)

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

const formatCorreccionDate = (dateString: string) => {
  const date = new Date(dateString.replace('T', ' ').replace(/\.\d{3}/, ''))
  return new Intl.DateTimeFormat('es-AR', { 
    day: 'numeric', 
    month: 'short', 
    hour: '2-digit', 
    minute: '2-digit'
  }).format(date)
}

// Formatear pedido original como HTML con motivos de productos
const formatPedidoOriginalHtml = (jsonString: string) => {
  try {
    const originalPedido = JSON.parse(jsonString)
    
    let html = `<div class="space-y-2">`
    
    if (originalPedido.items && Array.isArray(originalPedido.items)) {
      html += `<div><strong class="text-gray-700">Productos modificados:</strong></div>`
      originalPedido.items.forEach((item: any, index: number) => {
        html += `<div class="border-l-2 border-blue-200 pl-2 py-1">`
        html += `<div class="font-medium text-gray-800">${item.nombre_producto || item.codigo_producto}</div>`
        html += `<div class="text-gray-600">Cantidad: ${item.cantidad} × $${item.precio_unitario?.toLocaleString() || 'N/A'}</div>`
        
        if (item.observaciones) {
          html += `<div class="text-xs text-orange-700 bg-orange-100 px-2 py-1 rounded mt-1">`
          html += `<strong>Motivo:</strong> ${item.observaciones}`
          html += `</div>`
        }
        
        html += `</div>`
      })
    }
    
    html += `</div>`
    return html
  } catch (error) {
    console.error('Error parsing pedido original JSON:', error)
    return '<div class="text-red-600">Error al mostrar detalles</div>'
  }
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
      return 'bg-yellow-500 text-white'
    case 'confirmado':
      return 'bg-blue-600 text-white'
    case 'en preparacion':
    case 'en_preparacion':
      return 'bg-orange-600 text-white'
    case 'en correcci\u00f3n':
    case 'en_correccion':
    case 'encorreccion':
      return 'bg-purple-600 text-white'
    case 'correcci\u00f3n pendiente':
    case 'correccion_pendiente':
    case 'correccionpendiente':
      return 'bg-amber-600 text-white'
    case 'correcci\u00f3n rechazada':
    case 'correccion_rechazada':
    case 'correccionrechazada':
      return 'bg-red-600 text-white'
    case 'listo':
      return 'bg-green-600 text-white'
    case 'entregado':
      return 'bg-green-600 text-white'
    case 'cancelado':
      return 'bg-red-600 text-white'
    default:
      return 'bg-gray-600 text-white'
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
    showSuccessToast('Pedido Cancelado', 'Tu pedido ha sido cancelado exitosamente')
  } catch (err) {
    console.error('Error cancelling order:', err)
    showErrorToast('Error al cancelar', 'No se pudo cancelar el pedido. Intenta nuevamente.')
  } finally {
    cancellingOrder.value = false
  }
}

const hasCorrection = (status: string): boolean => {
  const lowerStatus = status.toLowerCase()
  return lowerStatus.includes('correcci') || lowerStatus.includes('correccion')
}

const canApproveCorrection = (status: string): boolean => {
  const lowerStatus = status.toLowerCase()
  return lowerStatus === 'corrección pendiente' || lowerStatus === 'correccion_pendiente' || lowerStatus === 'correccionpendiente'
}

const loadCorrection = async (pedidoId: number) => {
  // Esta función necesitaría un endpoint específico o token
  // Por ahora, mostraremos un placeholder
  console.log('Loading correction for order:', pedidoId)
}

const openCorreccionModal = async (order: OrderResponse) => {
  selectedOrder.value = order
  showCorreccionModal.value = true
  
  // Cargar datos detallados de corrección
  try {
    if (!authStore.token) {
      throw new Error('No hay sesión activa')
    }
    
    const correccionData = await authApiService.getOrderCorrection(order.id, authStore.token)
    selectedCorreccion.value = correccionData
    console.log('Correction data loaded:', correccionData)
  } catch (error: any) {
    console.error('Error loading correction details:', error)
    showErrorToast('Error al cargar corrección', error.message || 'No se pudieron cargar los detalles de la corrección')
    selectedCorreccion.value = null
  }
}

const closeCorreccionModal = () => {
  showCorreccionModal.value = false
  selectedOrder.value = null
  selectedCorreccion.value = null
}

// Helper para encontrar item corregido
const getItemCorregido = (codigoProducto: string) => {
  return selectedCorreccion.value?.pedido_corregido.items.find(item => item.codigo_producto === codigoProducto)
}

const approveCorrection = async (pedidoId: number) => {
  processingCorreccion.value = true
  try {
    if (!authStore.token) {
      throw new Error('No hay sesión activa')
    }

    await authStore.executeWithAuth(
      (token) => authApiService.approveCorrection(pedidoId, undefined, token)
    )
    
    // Actualizar el estado del pedido
    const orderIndex = orders.value.findIndex(o => o.id === pedidoId)
    if (orderIndex !== -1) {
      orders.value[orderIndex].estado = 'Confirmado'
    }
    
    closeCorreccionModal()
    showSuccessToast('Corrección Aprobada', 'Los cambios han sido aceptados y el pedido será procesado')
  } catch (err: any) {
    console.error('Error approving correction:', err)
    showErrorToast('Error al aprobar', err.message || 'No se pudo aprobar la corrección. Intenta nuevamente.')
  } finally {
    processingCorreccion.value = false
  }
}

const rejectCorrection = async (pedidoId: number) => {
  processingCorreccion.value = true
  try {
    if (!authStore.token) {
      throw new Error('No hay sesión activa')
    }

    await authStore.executeWithAuth(
      (token) => authApiService.rejectCorrection(pedidoId, undefined, token)
    )
    
    // Actualizar el estado del pedido
    const orderIndex = orders.value.findIndex(o => o.id === pedidoId)
    if (orderIndex !== -1) {
      orders.value[orderIndex].estado = 'Corrección Rechazada'
    }
    
    closeCorreccionModal()
    showSuccessToast('Corrección Rechazada', 'Los cambios han sido rechazados. Nos pondremos en contacto contigo.')
  } catch (err: any) {
    console.error('Error rejecting correction:', err)
    showErrorToast('Error al rechazar', err.message || 'No se pudo rechazar la corrección. Intenta nuevamente.')
  } finally {
    processingCorreccion.value = false
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