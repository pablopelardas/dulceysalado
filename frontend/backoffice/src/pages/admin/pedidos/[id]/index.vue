<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-6xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center gap-3 mb-4">
          <UButton
            variant="ghost"
            color="gray"
            icon="i-heroicons-arrow-left"
            to="/admin/pedidos"
          >
            Volver
          </UButton>
          <div class="h-6 w-px bg-gray-300 dark:bg-gray-600"></div>
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
              <span v-if="pedido">Pedido #{{ pedido.numero }}</span>
              <span v-else>Cargando pedido...</span>
            </h1>
            <p v-if="pedido" class="text-gray-600 dark:text-gray-400">
              {{ formatDate(pedido.fecha_pedido) }} - {{ formatTime(pedido.fecha_pedido) }}
            </p>
          </div>
        </div>
      </div>

      <!-- Loading state -->
      <div v-if="loading" class="space-y-6">
        <UCard>
          <div class="space-y-4">
            <USkeleton class="h-4 w-48" />
            <USkeleton class="h-20 w-full" />
          </div>
        </UCard>
        <UCard>
          <div class="space-y-4">
            <USkeleton class="h-4 w-32" />
            <USkeleton class="h-32 w-full" />
          </div>
        </UCard>
      </div>

      <!-- Error state -->
      <div v-else-if="error" class="text-center py-12">
        <UIcon name="i-heroicons-exclamation-triangle" class="h-12 w-12 text-red-500 mx-auto mb-4" />
        <p class="text-red-600 dark:text-red-400 mb-4">{{ error }}</p>
        <UButton @click="loadPedido" color="red" variant="outline">
          Reintentar
        </UButton>
      </div>

      <!-- Contenido principal -->
      <div v-else-if="pedido" class="space-y-6">
        <!-- Header del pedido con estado y total -->
        <UCard>
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div class="flex items-center gap-4">
              <div>
                <h2 class="text-2xl font-bold text-gray-900 dark:text-gray-100">
                  Pedido #{{ pedido.numero }}
                </h2>
                <p class="text-gray-600 dark:text-gray-400">
                  {{ formatDate(pedido.fecha_pedido) }} - {{ formatTime(pedido.fecha_pedido) }}
                </p>
              </div>
              <UBadge 
                :color="formatEstadoPedido(pedido.estado).color"
                variant="soft"
                size="lg"
              >
                <UIcon 
                  :name="formatEstadoPedido(pedido.estado).icon" 
                  class="w-4 h-4 mr-2" 
                />
                {{ formatEstadoPedido(pedido.estado).label }}
              </UBadge>
            </div>
            <div class="text-right">
              <p class="text-sm text-gray-500 dark:text-gray-400">Total del pedido</p>
              <p class="text-3xl font-bold text-green-600 dark:text-green-400">
                ${{ pedido.monto_total.toLocaleString() }}
              </p>
            </div>
          </div>
        </UCard>

        <!-- Información de entrega -->
        <UCard>
          <template #header>
            <div class="flex items-center gap-2">
              <UIcon name="i-heroicons-truck" class="w-5 h-5 text-blue-600" />
              <h3 class="text-lg font-semibold">Información de Entrega</h3>
            </div>
          </template>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                <UIcon name="i-heroicons-map-pin" class="w-4 h-4 inline mr-1" />
                Dirección de Entrega
              </label>
              <div class="bg-gray-50 dark:bg-gray-800 p-3 rounded-lg">
                <p class="text-gray-900 dark:text-gray-100 font-medium">{{ pedido.direccion_entrega }}</p>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                <UIcon name="i-heroicons-calendar-days" class="w-4 h-4 inline mr-1" />
                Fecha Solicitada
              </label>
              <div class="bg-gray-50 dark:bg-gray-800 p-3 rounded-lg">
                <p class="text-gray-900 dark:text-gray-100 font-medium">
                  {{ formatDate(pedido.fecha_entrega) }}
                </p>
                <p class="text-xs text-gray-500 mt-1">
                  {{ getDaysUntilDelivery(pedido.fecha_entrega) }}
                </p>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                <UIcon name="i-heroicons-clock" class="w-4 h-4 inline mr-1" />
                Horario Preferido
              </label>
              <div class="bg-gray-50 dark:bg-gray-800 p-3 rounded-lg">
                <p class="text-gray-900 dark:text-gray-100 font-medium">{{ pedido.horario_entrega }}</p>
              </div>
            </div>
          </div>
        </UCard>

        <!-- Información principal en dos columnas -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <!-- Información del cliente -->
          <UCard>
            <template #header>
              <div class="flex items-center gap-2">
                <UIcon name="i-heroicons-user" class="w-5 h-5" />
                <h3 class="text-lg font-semibold">Cliente</h3>
              </div>
            </template>

            <div class="space-y-4">
              <div>
                <p class="text-lg font-semibold text-gray-900 dark:text-gray-100">
                  {{ pedido.cliente_nombre }}
                </p>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ pedido.cliente_email }}
                </p>
                <p v-if="pedido.cliente_telefono" class="text-sm text-gray-600 dark:text-gray-400">
                  {{ pedido.cliente_telefono }}
                </p>
              </div>

              <!-- Acciones de comunicación -->
              <div class="space-y-2 pt-4 border-t">
                <UButton
                  v-if="pedido.cliente_telefono"
                  @click="contactWhatsApp"
                  color="green"
                  variant="outline"
                  size="sm"
                  class="w-full"
                  icon="i-simple-icons-whatsapp"
                >
                  WhatsApp
                </UButton>

                <UButton
                  @click="contactEmail"
                  color="blue"
                  variant="outline"
                  size="sm"
                  class="w-full"
                  icon="i-heroicons-envelope"
                >
                  Email
                </UButton>
              </div>
            </div>
          </UCard>

          <!-- Detalles adicionales -->
          <UCard>
            <template #header>
              <div class="flex items-center gap-2">
                <UIcon name="i-heroicons-information-circle" class="w-5 h-5" />
                <h3 class="text-lg font-semibold">Detalles</h3>
              </div>
            </template>

            <div class="space-y-3 text-sm">
              <div class="flex justify-between">
                <span class="text-gray-600 dark:text-gray-400">ID:</span>
                <span class="font-medium">{{ pedido.id }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-gray-600 dark:text-gray-400">Productos:</span>
                <span class="font-medium">{{ pedido.items.length }}</span>
              </div>

              <!-- Fechas de gestión -->
              <div v-if="pedido.fecha_aceptado" class="pt-2 border-t">
                <div class="flex justify-between">
                  <span class="text-blue-600 dark:text-blue-400">Aceptado:</span>
                  <span class="text-xs">{{ formatDateTime(pedido.fecha_aceptado) }}</span>
                </div>
              </div>
              <div v-if="pedido.fecha_rechazado" class="pt-2 border-t">
                <div class="flex justify-between">
                  <span class="text-red-600 dark:text-red-400">Rechazado:</span>
                  <span class="text-xs">{{ formatDateTime(pedido.fecha_rechazado) }}</span>
                </div>
              </div>
              <div v-if="pedido.fecha_completado" class="pt-2 border-t">
                <div class="flex justify-between">
                  <span class="text-green-600 dark:text-green-400">Completado:</span>
                  <span class="text-xs">{{ formatDateTime(pedido.fecha_completado) }}</span>
                </div>
              </div>

              <!-- Usuario de gestión -->
              <div v-if="pedido.usuario_gestion_nombre" class="pt-2 border-t">
                <div class="flex justify-between">
                  <span class="text-gray-600 dark:text-gray-400">Gestionado por:</span>
                  <span class="font-medium">{{ pedido.usuario_gestion_nombre }}</span>
                </div>
              </div>
            </div>
          </UCard>

          <!-- Acciones del pedido -->
          <UCard>
            <template #header>
              <div class="flex items-center gap-2">
                <UIcon name="i-heroicons-cog-6-tooth" class="w-5 h-5" />
                <h3 class="text-lg font-semibold">Acciones</h3>
              </div>
            </template>

            <div class="space-y-3">
              <!-- Acciones según estado -->
              <template v-if="pedido.estado === 'Pendiente' || pedido.estado === 'CorreccionRechazada'">
                <div v-if="pedido.estado === 'CorreccionRechazada'" class="mb-3 p-3 bg-amber-50 dark:bg-amber-900/20 rounded-lg">
                  <div class="flex items-start gap-2">
                    <UIcon name="i-heroicons-exclamation-triangle" class="w-5 h-5 text-amber-600 dark:text-amber-400 flex-shrink-0 mt-0.5" />
                    <div class="text-sm">
                      <p class="font-medium text-amber-900 dark:text-amber-100">El cliente rechazó la corrección anterior</p>
                      <p class="text-amber-700 dark:text-amber-300 mt-1">Puedes volver a corregir el pedido después de contactar al cliente</p>
                    </div>
                  </div>
                </div>
                
                <UButton
                  @click="handleCorregir"
                  color="yellow"
                  class="w-full"
                  icon="i-heroicons-pencil-square"
                  variant="outline"
                >
                  {{ pedido.estado === 'CorreccionRechazada' ? 'Volver a Corregir' : 'Corregir' }}
                </UButton>
                
                <template v-if="pedido.estado === 'Pendiente'">
                  <UButton
                    @click="handleAceptar"
                    color="green"
                    class="w-full"
                    icon="i-heroicons-check"
                    :loading="loading"
                  >
                    Aceptar
                  </UButton>
                  <UButton
                    @click="handleRechazar"
                    color="red"
                    variant="outline"
                    class="w-full"
                    icon="i-heroicons-x-mark"
                  >
                    Rechazar
                  </UButton>
                </template>
              </template>

              <template v-if="pedido.estado === 'Aceptado'">
                <UButton
                  @click="handleCompletar"
                  color="green"
                  class="w-full"
                  icon="i-heroicons-check-badge"
                  :loading="loading"
                >
                  Completar
                </UButton>
                <UButton
                  @click="handleRechazar"
                  color="red"
                  variant="outline"
                  class="w-full"
                  icon="i-heroicons-x-mark"
                >
                  Rechazar
                </UButton>
              </template>

              <!-- Estados que permiten rechazar -->
              <template v-if="pedido.estado === 'EnCorreccion' || pedido.estado === 'CorreccionPendiente'">
                <UButton
                  @click="handleRechazar"
                  color="red"
                  variant="outline"
                  class="w-full"
                  icon="i-heroicons-x-mark"
                >
                  Rechazar
                </UButton>
              </template>

              <div v-if="pedido.estado === 'Rechazado' || pedido.estado === 'Completado' || pedido.estado === 'Cancelado'">
                <div class="text-center py-6 text-gray-500">
                  <UIcon name="i-heroicons-lock-closed" class="w-8 h-8 mx-auto mb-2 opacity-50" />
                  <p class="text-sm">Pedido finalizado</p>
                </div>
              </div>
            </div>
          </UCard>
        </div>

        <!-- Observaciones del cliente -->
        <UCard v-if="pedido.observaciones">
          <template #header>
            <div class="flex items-center gap-2">
              <UIcon name="i-heroicons-chat-bubble-left-ellipsis" class="w-5 h-5" />
              <h3 class="text-lg font-semibold">Observaciones del Cliente</h3>
            </div>
          </template>

          <div class="bg-blue-50 dark:bg-blue-900/20 p-4 rounded-lg">
            <p class="text-gray-800 dark:text-gray-200">{{ pedido.observaciones }}</p>
          </div>
        </UCard>

        <!-- Motivo de rechazo -->
        <UCard v-if="pedido.motivo_rechazo">
          <template #header>
            <div class="flex items-center gap-2">
              <UIcon name="i-heroicons-exclamation-triangle" class="w-5 h-5 text-red-500" />
              <h3 class="text-lg font-semibold text-red-700 dark:text-red-400">Motivo de Rechazo</h3>
            </div>
          </template>

          <div class="bg-red-50 dark:bg-red-900/20 p-4 rounded-lg border border-red-200 dark:border-red-800">
            <p class="text-red-800 dark:text-red-200">{{ pedido.motivo_rechazo }}</p>
          </div>
        </UCard>

        <!-- Productos del pedido -->
        <UCard>
          <template #header>
            <h3 class="text-lg font-semibold">
              Productos ({{ pedido.items.length }})
            </h3>
          </template>

          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead class="bg-gray-50 dark:bg-gray-800">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Producto
                  </th>
                  <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Cantidad
                  </th>
                  <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Precio Unitario
                  </th>
                  <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Subtotal
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white dark:bg-gray-900 divide-y divide-gray-200 dark:divide-gray-700">
                <tr v-for="item in pedido.items" :key="item.id">
                  <td class="px-6 py-4">
                    <div>
                      <div class="text-sm font-medium text-gray-900 dark:text-gray-100">
                        {{ item.nombre_producto }}
                      </div>
                      <div class="text-xs text-gray-500">
                        Código: {{ item.codigo_producto }}
                      </div>
                      <div v-if="item.observaciones" class="text-xs text-amber-600 dark:text-amber-400 mt-1">
                        <UIcon name="i-heroicons-exclamation-triangle" class="w-3 h-3 inline mr-1" />
                        {{ item.observaciones }}
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4 text-right text-sm text-gray-900 dark:text-gray-100">
                    {{ item.cantidad }}
                  </td>
                  <td class="px-6 py-4 text-right text-sm text-gray-900 dark:text-gray-100">
                    ${{ item.precio_unitario.toLocaleString() }}
                  </td>
                  <td class="px-6 py-4 text-right text-sm font-medium text-gray-900 dark:text-gray-100">
                    ${{ item.subtotal.toLocaleString() }}
                  </td>
                </tr>
              </tbody>
              <tfoot class="bg-gray-50 dark:bg-gray-800">
                <tr>
                  <td colspan="3" class="px-6 py-4 text-right text-sm font-medium text-gray-900 dark:text-gray-100">
                    Total:
                  </td>
                  <td class="px-6 py-4 text-right text-lg font-bold text-green-600 dark:text-green-400">
                    ${{ pedido.monto_total.toLocaleString() }}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>
        </UCard>

        <!-- Historial de correcciones -->
        <UCard v-if="pedido.correcciones && pedido.correcciones.length > 0">
          <template #header>
            <div class="flex items-center gap-2">
              <UIcon name="i-heroicons-clock" class="w-5 h-5 text-amber-600" />
              <h3 class="text-lg font-semibold">Historial de Correcciones ({{ pedido.correcciones.length }})</h3>
            </div>
          </template>

          <div class="space-y-4">
            <div 
              v-for="(correccion, index) in pedido.correcciones" 
              :key="correccion.id"
              class="border border-gray-200 dark:border-gray-700 rounded-lg p-4"
              :class="{ 'bg-amber-50 dark:bg-amber-900/20': !correccion.utilizado, 'bg-gray-50 dark:bg-gray-800': correccion.utilizado }"
            >
              <div class="flex items-start justify-between gap-4">
                <div class="flex-1 space-y-2">
                  <!-- Header de la corrección -->
                  <div class="flex items-center gap-3">
                    <UBadge 
                      :color="correccion.utilizado ? 'green' : 'amber'"
                      variant="soft"
                    >
                      <UIcon 
                        :name="correccion.utilizado ? 'i-heroicons-check-circle' : 'i-heroicons-clock'" 
                        class="w-3 h-3 mr-1" 
                      />
                      {{ correccion.utilizado ? 'Completada' : 'Pendiente' }}
                    </UBadge>
                    
                    <span class="text-xs text-gray-500 dark:text-gray-400">
                      {{ formatDateTime(correccion.fecha_creacion) }}
                    </span>
                    
                    <span v-if="correccion.utilizado && correccion.fecha_uso" class="text-xs text-green-600 dark:text-green-400">
                      Usada: {{ formatDateTime(correccion.fecha_uso) }}
                    </span>
                  </div>

                  <!-- Motivo de corrección -->
                  <div v-if="correccion.motivo_correccion">
                    <p class="text-sm font-medium text-gray-700 dark:text-gray-300">
                      <UIcon name="i-heroicons-exclamation-triangle" class="w-4 h-4 inline mr-1 text-amber-600" />
                      Motivo: {{ correccion.motivo_correccion }}
                    </p>
                  </div>

                  <!-- Detalles de los cambios (si hay JSON del pedido original) -->
                  <div v-if="correccion.pedido_original_json">
                    <details class="group">
                      <summary class="cursor-pointer text-sm text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 flex items-center gap-1">
                        <UIcon name="i-heroicons-chevron-right" class="w-4 h-4 group-open:rotate-90 transition-transform" />
                        Ver cambios realizados
                      </summary>
                      <div class="mt-2 p-3 bg-gray-100 dark:bg-gray-800 rounded-md">
                        <p class="text-xs text-gray-600 dark:text-gray-400 mb-2">Cambios aplicados al pedido:</p>
                        <pre class="text-xs text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{{ formatPedidoOriginal(correccion.pedido_original_json) }}</pre>
                      </div>
                    </details>
                  </div>

                  <!-- Token para corrección pendiente -->
                  <div v-if="!correccion.utilizado" class="flex items-center gap-2 pt-2">
                    <span class="text-xs text-gray-500 dark:text-gray-400">Token:</span>
                    <code class="text-xs bg-gray-200 dark:bg-gray-700 px-2 py-1 rounded">{{ correccion.token }}</code>
                    <UButton 
                      @click="copyToClipboard(`http://localhost:5174/correccion/${correccion.token}`)"
                      color="gray" 
                      variant="ghost" 
                      size="xs"
                      icon="i-heroicons-clipboard"
                    >
                      Copiar Link
                    </UButton>
                  </div>
                </div>

                <!-- Estado visual -->
                <div class="flex-shrink-0">
                  <div 
                    class="w-3 h-3 rounded-full"
                    :class="correccion.utilizado ? 'bg-green-500' : 'bg-amber-500'"
                  ></div>
                </div>
              </div>
            </div>
          </div>
        </UCard>
      </div>

      <!-- Not found -->
      <div v-else class="text-center py-12">
        <UIcon name="i-heroicons-document-x" class="h-12 w-12 text-gray-400 mx-auto mb-4" />
        <p class="text-gray-500 dark:text-gray-400 mb-4">
          No se encontró el pedido solicitado
        </p>
        <UButton to="/admin/pedidos" variant="outline">
          Volver a Pedidos
        </UButton>
      </div>
    </div>

    <!-- Modal de rechazo -->
    <UModal 
      v-model:open="showRejectModal"
      prevent-close
    >
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold">Rechazar Pedido</h3>
            <UButton 
              color="gray" 
              variant="ghost" 
              icon="i-heroicons-x-mark-20-solid" 
              class="-my-1" 
              @click="showRejectModal = false" 
            />
          </div>
        </template>

        <template #body>
          <div class="space-y-4">
            <p class="text-sm text-gray-600 dark:text-gray-400">
              ¿Estás seguro de que deseas rechazar el pedido #{{ pedido?.numero }}?
            </p>
            
            <UFormGroup label="Motivo de rechazo" required>
              <div class="space-y-3">
                <!-- Opciones predefinidas -->
                <div class="grid gap-2">
                  <label class="flex items-center space-x-3 cursor-pointer">
                    <input 
                      type="radio" 
                      name="reject-reason" 
                      value="Falta de stock"
                      v-model="selectedRejectReason"
                      class="w-4 h-4 text-primary-600 border-gray-300 focus:ring-primary-500"
                    />
                    <span class="text-sm text-gray-700 dark:text-gray-300">Falta de stock</span>
                  </label>
                  
                  <label class="flex items-center space-x-3 cursor-pointer">
                    <input 
                      type="radio" 
                      name="reject-reason" 
                      value="Compra mínima del producto"
                      v-model="selectedRejectReason"
                      class="w-4 h-4 text-primary-600 border-gray-300 focus:ring-primary-500"
                    />
                    <span class="text-sm text-gray-700 dark:text-gray-300">Compra mínima del producto</span>
                  </label>
                  
                  <label class="flex items-center space-x-3 cursor-pointer">
                    <input 
                      type="radio" 
                      name="reject-reason" 
                      value="Producto descontinuado"
                      v-model="selectedRejectReason"
                      class="w-4 h-4 text-primary-600 border-gray-300 focus:ring-primary-500"
                    />
                    <span class="text-sm text-gray-700 dark:text-gray-300">Producto descontinuado</span>
                  </label>
                  
                  <label class="flex items-center space-x-3 cursor-pointer">
                    <input 
                      type="radio" 
                      name="reject-reason" 
                      value="otro"
                      v-model="selectedRejectReason"
                      class="w-4 h-4 text-primary-600 border-gray-300 focus:ring-primary-500"
                    />
                    <span class="text-sm text-gray-700 dark:text-gray-300">Otro motivo</span>
                  </label>
                </div>
                
                <!-- Campo de texto personalizado -->
                <div v-if="selectedRejectReason === 'otro'" class="mt-3 w-full">
                  <UTextarea
                    v-model="customRejectReason"
                    placeholder="Especifica el motivo del rechazo..."
                    :rows="3"
                    class="w-full"
                  />
                </div>
              </div>
            </UFormGroup>
          </div>
        </template>

        <template #footer>
          <div class="flex justify-end gap-3">
            <UButton @click="showRejectModal = false" color="gray" variant="ghost">
              Cancelar
            </UButton>
            <UButton
              @click="confirmRechazar"
              color="red"
              :disabled="!hasValidRejectReason"
              :loading="loading"
            >
              Rechazar Pedido
            </UButton>
          </div>
        </template>
    </UModal>


    <!-- Modal de corrección -->
    <CorrectionModal
      :show="showCorrectionModal"
      :pedido="pedido"
      :loading="loading"
      @close="closeCorrectionModal"
      @confirm="handleCorrectionConfirm"
    />
  </div>
</template>

<script setup lang="ts">
import { formatEstadoPedido } from '~/types/pedidos'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Detalle de Pedido - Dulce y Salado',
  meta: [
    { name: 'description', content: 'Ver detalles del pedido' }
  ]
})

// Composables
const { 
  pedido, 
  loading, 
  error,
  fetchPedidoById,
  aceptarPedido,
  rechazarPedido,
  completarPedido,
  corregirPedido,
  generarLinkWhatsApp,
  generarLinkEmail,
  clearError,
  clearPedido
} = usePedidos()

const route = useRoute()
const router = useRouter()

// Estado reactivo
const showRejectModal = ref(false)
const rejectReason = ref('')
const selectedRejectReason = ref('')
const customRejectReason = ref('')

// Estado para modal de corrección
const showCorrectionModal = ref(false)

// Computed para validar si hay un motivo seleccionado
const hasValidRejectReason = computed(() => {
  if (selectedRejectReason.value === 'otro') {
    return customRejectReason.value.trim().length > 0
  }
  return selectedRejectReason.value.length > 0
})

// Obtener el ID del pedido
const pedidoId = computed(() => {
  const id = Array.isArray(route.params.id) ? route.params.id[0] : route.params.id
  return parseInt(id)
})

// Métodos
const loadPedido = async () => {
  clearError()
  clearPedido()
  
  try {
    await fetchPedidoById(pedidoId.value)
  } catch (error) {
    // Error ya manejado en el composable
  }
}

// Acciones de pedidos
const handleAceptar = async () => {
  if (!pedido.value) return

  try {
    await aceptarPedido(pedido.value.id)
    // El pedido se actualiza automáticamente en el composable
  } catch (error) {
    // Error ya manejado en el composable
  }
}

const handleRechazar = () => {
  rejectReason.value = ''
  selectedRejectReason.value = ''
  customRejectReason.value = ''
  showRejectModal.value = true
}

const confirmRechazar = async () => {
  if (!pedido.value) return

  // Determinar el motivo final
  let finalReason = ''
  if (selectedRejectReason.value === 'otro') {
    finalReason = customRejectReason.value.trim()
  } else {
    finalReason = selectedRejectReason.value
  }

  if (!finalReason) return

  try {
    await rechazarPedido(pedido.value.id, finalReason)
    showRejectModal.value = false
    // El pedido se actualiza automáticamente en el composable
  } catch (error) {
    // Error ya manejado en el composable
  }
}

const handleCompletar = async () => {
  if (!pedido.value) return

  try {
    await completarPedido(pedido.value.id)
    // El pedido se actualiza automáticamente en el composable
  } catch (error) {
    // Error ya manejado en el composable
  }
}


// Comunicación
const contactWhatsApp = () => {
  if (!pedido.value?.cliente_telefono) return
  
  const mensaje = `Hola ${pedido.value.cliente_nombre}, te contactamos respecto a tu pedido #${pedido.value.numero}. ¡Gracias por elegirnos!`
  const link = generarLinkWhatsApp(pedido.value.cliente_telefono, mensaje)
  window.open(link, '_blank')
}

const contactEmail = () => {
  if (!pedido.value) return
  
  const asunto = `Pedido #${pedido.value.numero} - Dulce y Salado`
  const mensaje = `Estimado/a ${pedido.value.cliente_nombre},\n\nTe contactamos respecto a tu pedido #${pedido.value.numero}.\n\nDetalles del pedido:\n- Total: $${pedido.value.total.toLocaleString()}\n- Productos: ${pedido.value.items.length}\n\nSaludos,\nEquipo Dulce y Salado`
  const link = generarLinkEmail(pedido.value.cliente_email, asunto, mensaje)
  window.open(link, '_blank')
}

// Funciones del modal de corrección
const handleCorregir = () => {
  if (!pedido.value) return
  showCorrectionModal.value = true
}

const closeCorrectionModal = () => {
  showCorrectionModal.value = false
}

const handleCorrectionConfirm = async (data: any) => {
  if (!pedido.value) return

  try {
    const resultado = await corregirPedido(pedido.value.id, data)
    
    // Si se envió al cliente y tenemos token, mostrar opciones de notificación
    if (resultado.success && resultado.token && resultado.enviado_al_cliente) {
      showNotificationOptions(pedido.value, resultado.token, resultado.correction_url, data.motivo_final)
    }
    
    closeCorrectionModal()
    await loadPedido() // Recargar el pedido para mostrar el nuevo estado
  } catch (error) {
    // Error ya manejado en el composable
  }
}

const showNotificationOptions = (pedido: any, token: string, correctionUrl?: string, motivo?: string) => {
  // Usar la URL del backend si está disponible, sino construir la URL del catálogo
  const finalCorrectionUrl = correctionUrl || `http://localhost:5174/correccion/${token}`
  
  console.log('Correction URL from backend:', correctionUrl)
  console.log('Final correction URL:', finalCorrectionUrl)
  
  // Mostrar toast con opciones
  const toast = useToast()
  
  toast.add({
    title: 'Corrección Enviada',
    description: `Token: ${token}`,
    color: 'green',
    duration: 10000,
    actions: [
      {
        label: 'WhatsApp',
        color: 'green',
        onClick: () => notifyByWhatsApp(pedido, finalCorrectionUrl, motivo)
      },
      {
        label: 'Copiar Link',
        color: 'gray', 
        onClick: () => copyToClipboard(finalCorrectionUrl)
      }
    ]
  })
}

const notifyByWhatsApp = (pedido: any, correctionUrl: string, motivo?: string) => {
  let mensaje = `Hola ${pedido.cliente_nombre}! Tu pedido #${pedido.numero} requiere una pequeña corrección`
  
  if (motivo) {
    mensaje += ` debido a: ${motivo}.`
  } else {
    mensaje += ` por disponibilidad de stock.`
  }
  
  mensaje += ` Por favor revísalo aquí: ${correctionUrl}`
  
  const link = generarLinkWhatsApp(pedido.cliente_telefono, mensaje)
  window.open(link, '_blank')
}

const copyToClipboard = async (text: string) => {
  try {
    await navigator.clipboard.writeText(text)
    const toast = useToast()
    toast.add({
      title: 'Link copiado',
      description: 'El enlace se copió al portapapeles',
      color: 'green',
      timeout: 2000
    })
  } catch (error) {
    console.error('Error copying to clipboard:', error)
  }
}

// Utilidades
const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('es-ES', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const formatTime = (dateString: string) => {
  return new Date(dateString).toLocaleTimeString('es-ES', { 
    hour: '2-digit', 
    minute: '2-digit' 
  })
}

const formatDateTime = (dateString: string) => {
  return new Date(dateString).toLocaleString('es-ES', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const getDaysUntilDelivery = (fechaEntrega: string) => {
  const today = new Date()
  const deliveryDate = new Date(fechaEntrega)
  const diffTime = deliveryDate.getTime() - today.getTime()
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  
  if (diffDays < 0) {
    return `Venció hace ${Math.abs(diffDays)} día(s)`
  } else if (diffDays === 0) {
    return 'Entrega HOY'
  } else if (diffDays === 1) {
    return 'Entrega MAÑANA'
  } else {
    return `En ${diffDays} días`
  }
}

const formatPedidoOriginal = (jsonString: string) => {
  try {
    const originalPedido = JSON.parse(jsonString)
    
    let formatted = `Pedido Original:\n`
    formatted += `• Total: $${originalPedido.monto_total?.toLocaleString() || 'No disponible'}\n`
    formatted += `• Productos: ${originalPedido.items?.length || 0} items\n\n`
    
    if (originalPedido.items && Array.isArray(originalPedido.items)) {
      formatted += `Items modificados:\n`
      originalPedido.items.forEach((item: any, index: number) => {
        formatted += `${index + 1}. ${item.nombre_producto || item.codigo_producto}\n`
        formatted += `   Cantidad: ${item.cantidad}\n`
        formatted += `   Subtotal: $${item.subtotal?.toLocaleString() || 'N/A'}\n`
        if (item.observaciones) {
          formatted += `   Motivo: ${item.observaciones}\n`
        }
        formatted += `\n`
      })
    }
    
    return formatted
  } catch (error) {
    console.error('Error parsing pedido original JSON:', error)
    return 'Error al mostrar el pedido original'
  }
}

// Cargar pedido al montar
onMounted(() => {
  loadPedido()
})

// Watch para cambios en el ID
watch(() => pedidoId.value, (newId) => {
  if (newId && !isNaN(newId)) {
    loadPedido()
  }
})

// Actualizar título con número de pedido
watch(pedido, (newPedido) => {
  if (newPedido) {
    useHead({
      title: `Pedido #${newPedido.numero} - Dulce y Salado`
    })
  }
})
</script>