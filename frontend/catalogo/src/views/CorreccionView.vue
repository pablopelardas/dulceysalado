<template>
  <div class="min-h-screen bg-black py-8">
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
      <!-- Header -->
      <div class="text-center mb-8">
        <img src="/assets/logo-dulceysalado.png" alt="Dulce y Salado" class="h-16 mx-auto mb-4">
        <h1 class="text-3xl font-bold text-white">Revisión de Pedido</h1>
        <p class="text-gray-300 mt-2">Tu pedido necesita algunos ajustes</p>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-orange-500 mx-auto"></div>
        <p class="text-gray-300 mt-4">Cargando corrección...</p>
      </div>

      <!-- Error -->
      <div v-else-if="error" class="bg-white/95 border border-gray-200 rounded-lg p-6 text-center shadow-lg">
        <div class="text-red-400 mb-4">
          <svg class="h-12 w-12 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <h3 class="text-lg font-medium text-red-600 mb-2">Error</h3>
        <p class="text-red-500">{{ error }}</p>
      </div>

      <!-- Corrección válida -->
      <div v-else-if="correccion && !processed" class="space-y-6">
        <!-- Info del pedido -->
        <div class="bg-white/95 rounded-lg shadow-lg p-6">
          <div class="flex items-center justify-between mb-4">
            <div>
              <h2 class="text-xl font-semibold text-gray-900">Pedido #{{ correccion.pedido_numero }}</h2>
              <p class="text-sm text-gray-600">{{ correccion.cliente_nombre }}</p>
            </div>
            <div class="text-right">
              <span class="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                ⏰ Expira: {{ formatExpiration(correccion.fecha_expiracion) }}
              </span>
            </div>
          </div>

          <!-- Motivo si existe -->
          <div v-if="correccion.pedido_corregido.motivo_correccion" class="bg-yellow-50 border border-yellow-300 rounded-lg p-4 mb-6">
            <h4 class="font-medium text-yellow-700 mb-2">📝 Motivo de la corrección:</h4>
            <p class="text-yellow-600">{{ correccion.pedido_corregido.motivo_correccion }}</p>
          </div>
        </div>

        <!-- Comparación de items -->
        <div class="bg-white/95 rounded-lg shadow-lg overflow-hidden">
          <div class="px-6 py-4 border-b border-gray-200 bg-gray-50">
            <h3 class="text-lg font-medium text-gray-900">Cambios en tu pedido</h3>
          </div>

          <div class="divide-y divide-gray-200">
            <div v-for="(itemOriginal, index) in correccion.pedido_original.items" :key="itemOriginal.codigo_producto" class="p-6">
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

                      <!-- Motivo si existe y hay cambios reales en esta corrección -->
                      <div v-if="getItemCorregido(itemOriginal.codigo_producto)!.motivo && (itemOriginal.cantidad !== getItemCorregido(itemOriginal.codigo_producto)!.cantidad)" class="text-xs text-orange-600 bg-orange-50 px-2 py-1 rounded mt-1">
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
              <span class="font-medium">${{ correccion.pedido_original.monto_total.toLocaleString() }}</span>
            </div>
            <div class="flex justify-between text-lg font-bold">
              <span class="text-gray-900 font-bold">Total ajustado:</span>
              <span class="text-green-600 font-bold">${{ correccion.pedido_corregido.monto_total.toLocaleString() }}</span>
            </div>
            <div v-if="correccion.pedido_original.monto_total !== correccion.pedido_corregido.monto_total" class="text-sm text-gray-500 mt-1">
              Diferencia: ${{ Math.abs(correccion.pedido_original.monto_total - correccion.pedido_corregido.monto_total).toLocaleString() }}
              {{ correccion.pedido_original.monto_total > correccion.pedido_corregido.monto_total ? 'menos' : 'más' }}
            </div>
          </div>
        </div>

        <!-- Historial de correcciones -->
        <div v-if="correccion.historial_correcciones && correccion.historial_correcciones.length > 1" class="bg-white/95 rounded-lg shadow-lg p-6">
          <h3 class="text-lg font-medium text-gray-900 mb-4">📜 Historial de Correcciones</h3>
          <p class="text-sm text-gray-600 mb-4">Este pedido ha tenido {{ correccion.historial_correcciones.length }} correcciones:</p>
          
          <div class="space-y-3">
            <div 
              v-for="(historialItem, index) in correccion.historial_correcciones" 
              :key="historialItem.id"
              class="border rounded-lg p-4"
              :class="{ 
                'bg-amber-50 border-amber-300': historialItem.token === correccion.token,
                'bg-gray-50 border-gray-200': historialItem.token !== correccion.token 
              }"
            >
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-3">
                  <span 
                    class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium"
                    :class="{ 
                      'bg-amber-100 text-amber-800': historialItem.token === correccion.token,
                      'bg-green-100 text-green-800': historialItem.utilizado && historialItem.token !== correccion.token,
                      'bg-gray-100 text-gray-800': !historialItem.utilizado && historialItem.token !== correccion.token
                    }"
                  >
                    {{ historialItem.token === correccion.token ? '📝 Actual' : historialItem.utilizado ? '✅ Completada' : '⏰ Expirada' }}
                  </span>
                  
                  <span class="text-xs text-gray-500">
                    {{ formatExpiration(historialItem.fecha_creacion) }}
                  </span>
                </div>
                
                <span v-if="historialItem.token === correccion.token" class="text-xs text-amber-600 font-medium">
                  Esta corrección
                </span>
              </div>
              
              <!-- Motivo si existe -->
              <div v-if="historialItem.motivo_correccion" class="mt-2">
                <p class="text-sm text-gray-700">
                  <span class="font-medium">Motivo:</span> {{ historialItem.motivo_correccion }}
                </p>
              </div>

              
              <!-- Fecha de uso si está completada -->
              <div v-if="historialItem.utilizado && historialItem.fecha_uso" class="mt-1">
                <p class="text-xs text-green-600">
                  Respondida el {{ formatExpiration(historialItem.fecha_uso) }}
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- Comentario opcional -->
        <div class="bg-white/95 rounded-lg shadow-lg p-6">
          <h3 class="text-lg font-medium text-gray-900 mb-4">Comentario (opcional)</h3>
          <textarea
            v-model="comentario"
            rows="3"
            class="w-full border border-gray-300 bg-white text-gray-900 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent placeholder-gray-400"
            placeholder="Deja un comentario sobre estos cambios..."
          ></textarea>
        </div>

        <!-- Botones de acción -->
        <div class="bg-white/95 rounded-lg shadow-lg p-6">
          <div class="flex flex-col sm:flex-row gap-4">
            <button
              @click="aprobar"
              :disabled="processing"
              class="flex-1 bg-green-600 hover:bg-green-700 disabled:bg-gray-400 text-white font-medium py-3 px-6 rounded-lg transition-colors cursor-pointer disabled:cursor-not-allowed"
            >
              <span v-if="!processing">✅ Aprobar Cambios</span>
              <span v-else>Procesando...</span>
            </button>
            
            <button
              @click="rechazar"
              :disabled="processing"
              class="flex-1 bg-red-600 hover:bg-red-700 disabled:bg-gray-400 text-white font-medium py-3 px-6 rounded-lg transition-colors cursor-pointer disabled:cursor-not-allowed"
            >
              <span v-if="!processing">❌ Rechazar Cambios</span>
              <span v-else>Procesando...</span>
            </button>
          </div>
        </div>
      </div>

      <!-- Procesado exitosamente -->
      <div v-else-if="processed" class="bg-white/95 rounded-lg shadow-lg p-8 text-center">
        <div class="text-green-500 mb-4">
          <svg class="h-16 w-16 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <h3 class="text-xl font-semibold text-gray-900 mb-2">¡Respuesta enviada!</h3>
        <p class="text-gray-600 mb-6">
          {{ processedAction === 'aprobar' 
             ? 'Has aprobado los cambios. Procesaremos tu pedido con las nuevas cantidades.' 
             : 'Has rechazado los cambios. Nos pondremos en contacto contigo.' }}
        </p>
        <p class="text-sm text-gray-500">Gracias por tu respuesta. Te mantendremos informado sobre el estado de tu pedido.</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { correccionService, type CorreccionData } from '@/services/correccion'
import { useToast } from '@/composables/useToast'

// Props y estado
const route = useRoute()
const { showToast } = useToast()

const token = ref(route.params.token as string)
const loading = ref(true)
const processing = ref(false)
const error = ref<string | null>(null)
const correccion = ref<CorreccionData | null>(null)
const comentario = ref('')
const processed = ref(false)
const processedAction = ref<'aprobar' | 'rechazar' | null>(null)

// Cargar corrección
const cargarCorreccion = async () => {
  loading.value = true
  error.value = null

  try {
    correccion.value = await correccionService.obtenerCorreccion(token.value)
  } catch (err: any) {
    error.value = err.message || 'Error al cargar la corrección'
  } finally {
    loading.value = false
  }
}

// Encontrar item corregido
const getItemCorregido = (codigoProducto: string) => {
  return correccion.value?.pedido_corregido.items.find(item => item.codigo_producto === codigoProducto)
}


// Aprobar corrección
const aprobar = async () => {
  if (!correccion.value) return
  
  processing.value = true
  try {
    await correccionService.aprobarCorreccion(token.value, comentario.value)
    processed.value = true
    processedAction.value = 'aprobar'
    showToast('Cambios aprobados exitosamente', 'success')
  } catch (err: any) {
    showToast(err.message || 'Error al aprobar', 'error')
  } finally {
    processing.value = false
  }
}

// Rechazar corrección
const rechazar = async () => {
  if (!correccion.value) return
  
  processing.value = true
  try {
    await correccionService.rechazarCorreccion(token.value, comentario.value)
    processed.value = true
    processedAction.value = 'rechazar'
    showToast('Cambios rechazados', 'success')
  } catch (err: any) {
    showToast(err.message || 'Error al rechazar', 'error')
  } finally {
    processing.value = false
  }
}

// Formatear expiración
const formatExpiration = (dateString: string) => {
  // El backend ya devuelve hora local de Argentina, pero sin timezone info
  // Necesitamos tratarla como hora local sin conversión
  const date = new Date(dateString.replace('T', ' ').replace(/\.\d{3}/, ''))
  return new Intl.DateTimeFormat('es-AR', { 
    day: 'numeric', 
    month: 'short', 
    hour: '2-digit', 
    minute: '2-digit'
  }).format(date)
}

// Formatear historial de corrección específica mostrando solo los cambios de esa corrección
const formatHistorialCorreccionHtml = (historialItem: any, index: number) => {
  try {
    const originalPedido = JSON.parse(historialItem.pedido_original_json)
    
    if (!correccion.value || !correccion.value.historial_correcciones) {
      return '<div class="text-gray-600">No hay datos de corrección disponibles</div>'
    }
    
    // Determinar el pedido de referencia (la corrección anterior o el pedido original inicial)
    let pedidoReferencia: any
    if (index < correccion.value.historial_correcciones.length - 1) {
      // Hay una corrección anterior, usar esa como referencia
      const correccionAnterior = correccion.value.historial_correcciones[index + 1]
      if (correccionAnterior.pedido_original_json) {
        pedidoReferencia = JSON.parse(correccionAnterior.pedido_original_json)
      } else {
        pedidoReferencia = originalPedido
      }
    } else {
      // Es la primera corrección, comparar con el pedido inicial
      pedidoReferencia = originalPedido
    }
    
    let html = `<div class="space-y-2">`
    let hasChanges = false
    
    if (originalPedido.Items && Array.isArray(originalPedido.Items)) {
      originalPedido.Items.forEach((item: any) => {
        const itemReferencia = pedidoReferencia.Items?.find(
          (refItem: any) => refItem.CodigoProducto === item.CodigoProducto
        )
        
        // Solo mostrar si hay cambios respecto a la referencia
        if (!itemReferencia || item.Cantidad !== itemReferencia.Cantidad || item.Observaciones) {
          hasChanges = true
          html += `<div class="border-l-2 border-blue-200 pl-2 py-1 bg-blue-50/50">`
          html += `<div class="font-medium text-gray-800">${item.NombreProducto || item.CodigoProducto}</div>`
          
          if (!itemReferencia) {
            html += `<div class="text-green-600 text-sm">➕ Producto agregado (${item.Cantidad} unidades)</div>`
          } else if (item.Cantidad !== itemReferencia.Cantidad) {
            html += `<div class="text-sm">`
            html += `<span class="text-gray-600">Cantidad: ${itemReferencia.Cantidad} → </span>`
            html += `<span class="font-medium text-blue-600">${item.Cantidad}</span>`
            html += `</div>`
          }
          
          if (item.Observaciones) {
            html += `<div class="text-xs text-orange-700 bg-orange-100 px-2 py-1 rounded mt-1">`
            html += `<strong>Motivo:</strong> ${item.Observaciones}`
            html += `</div>`
          }
          
          html += `</div>`
        }
      })
      
      // Verificar productos eliminados
      if (pedidoReferencia.Items && Array.isArray(pedidoReferencia.Items)) {
        pedidoReferencia.Items.forEach((itemRef: any) => {
          const itemEnCorrecion = originalPedido.Items.find(
            (item: any) => item.CodigoProducto === itemRef.CodigoProducto
          )
          
          if (!itemEnCorrecion) {
            hasChanges = true
            html += `<div class="border-l-2 border-red-200 pl-2 py-1 bg-red-50/50">`
            html += `<div class="font-medium text-gray-800">${itemRef.NombreProducto || itemRef.CodigoProducto}</div>`
            html += `<div class="text-red-600 text-sm">❌ Producto eliminado (${itemRef.Cantidad} unidades)</div>`
            html += `</div>`
          }
        })
      }
    }
    
    if (!hasChanges) {
      html += `<div class="text-gray-600 text-sm">No hay cambios en esta corrección</div>`
    }
    
    html += `</div>`
    return html
  } catch (error) {
    console.error('Error parsing historial de corrección:', error)
    return '<div class="text-red-600">Error al mostrar detalles del historial</div>'
  }
}

// Cargar al montar
onMounted(() => {
  cargarCorreccion()
})
</script>