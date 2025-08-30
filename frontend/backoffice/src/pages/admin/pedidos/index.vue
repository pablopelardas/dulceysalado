<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center justify-between mb-4">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
              Gestión de Pedidos
            </h1>
            <p class="text-gray-600 dark:text-gray-400 mt-2">
              Administra todos los pedidos del sistema
            </p>
          </div>
          <UButton
            @click="refreshData"
            :loading="loading"
            color="primary"
            variant="outline"
            icon="i-heroicons-arrow-path"
          >
            Actualizar
          </UButton>
        </div>

        <!-- Estadísticas -->
        <ClientOnly>
          <div v-if="estadisticas" class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4 mb-6">
            <UCard class="p-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-blue-600 dark:text-blue-400">
                  {{ (estadisticas.total_pendientes + estadisticas.total_aceptados + estadisticas.total_rechazados + estadisticas.total_completados + estadisticas.total_cancelados) }}
                </div>
                <div class="text-sm text-gray-500">Total</div>
              </div>
            </UCard>
            <UCard class="p-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-orange-600 dark:text-orange-400">
                  {{ estadisticas.total_pendientes }}
                </div>
                <div class="text-sm text-gray-500">Pendientes</div>
              </div>
            </UCard>
            <UCard class="p-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-blue-600 dark:text-blue-400">
                  {{ estadisticas.total_aceptados }}
                </div>
                <div class="text-sm text-gray-500">Aceptados</div>
              </div>
            </UCard>
            <UCard class="p-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-red-600 dark:text-red-400">
                  {{ estadisticas.total_rechazados }}
                </div>
                <div class="text-sm text-gray-500">Rechazados</div>
              </div>
            </UCard>
            <UCard class="p-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-green-600 dark:text-green-400">
                  {{ estadisticas.total_completados }}
                </div>
                <div class="text-sm text-gray-500">Completados</div>
              </div>
            </UCard>
            <UCard class="p-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-purple-600 dark:text-purple-400">
                  ${{ estadisticas.monto_total_mes.toLocaleString() }}
                </div>
                <div class="text-sm text-gray-500">Ventas</div>
              </div>
            </UCard>
          </div>
          <template #fallback>
            <div class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4 mb-6">
              <USkeleton v-for="i in 6" :key="i" class="h-20 w-full" />
            </div>
          </template>
        </ClientOnly>
      </div>

      <!-- Filtros -->
      <UCard class="mb-6">
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold">Filtros</h3>
            <UButton
              @click="clearFilters"
              color="gray"
              variant="ghost"
              size="sm"
            >
              Limpiar
            </UButton>
          </div>
        </template>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <UFormField label="Estado">
            <USelect
              v-model="filters.estado"
              :options="estadoOptions"
              placeholder="Todos los estados"
              @change="applyFilters"
            />
          </UFormField>

          <UFormField label="Número de pedido">
            <UInput
              v-model="filters.numeroContiene"
              placeholder="Buscar por número..."
              @keyup.enter="applyFilters"
            />
          </UFormField>

          <UFormField label="Fecha desde">
            <UInput
              v-model="filters.fechaDesde"
              type="date"
              @change="applyFilters"
            />
          </UFormField>

          <UFormField label="Fecha hasta">
            <UInput
              v-model="filters.fechaHasta"
              type="date"
              @change="applyFilters"
            />
          </UFormField>
        </div>

        <div class="flex justify-end mt-4">
          <UButton @click="applyFilters" color="primary">
            Aplicar Filtros
          </UButton>
        </div>
      </UCard>

      <!-- Tabla de pedidos -->
      <UCard>
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold">
              Pedidos ({{ totalCount }})
            </h3>
            <UButton
              @click="refreshData"
              :loading="loading"
              color="gray"
              variant="ghost"
              size="sm"
              icon="i-heroicons-arrow-path"
              square
              title="Actualizar lista de pedidos"
            />
          </div>
        </template>

        <!-- Loading state -->
        <div v-if="loading" class="space-y-4">
          <USkeleton v-for="i in 5" :key="i" class="h-16 w-full" />
        </div>

        <!-- Error state -->
        <div v-else-if="error" class="text-center py-8">
          <UIcon name="i-heroicons-exclamation-triangle" class="h-12 w-12 text-red-500 mx-auto mb-4" />
          <p class="text-red-600 dark:text-red-400 mb-4">{{ error }}</p>
          <UButton @click="refreshData" color="red" variant="outline">
            Reintentar
          </UButton>
        </div>

        <!-- Tabla -->
        <ClientOnly>
          <div v-if="!loading && !error && pedidos.length > 0" class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead class="bg-gray-50 dark:bg-gray-800">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Pedido
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Cliente
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Fecha Pedido
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Entrega
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Estado
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Total
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white dark:bg-gray-900 divide-y divide-gray-200 dark:divide-gray-700">
                <tr v-for="pedido in pedidos" :key="pedido.id" class="hover:bg-gray-50 dark:hover:bg-gray-800">
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm font-medium text-gray-900 dark:text-gray-100">
                      #{{ pedido.numero }}
                    </div>
                    <div class="text-xs text-gray-500">
                      ID: {{ pedido.id }}
                    </div>
                  </td>
                  <td class="px-6 py-4">
                    <div class="text-sm font-medium text-gray-900 dark:text-gray-100">
                      {{ pedido.cliente_nombre }}
                    </div>
                    <div class="text-xs text-gray-500">
                      {{ pedido.cliente_email }}
                    </div>
                    <div v-if="pedido.cliente_telefono" class="text-xs text-gray-500">
                      {{ pedido.cliente_telefono }}
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm text-gray-900 dark:text-gray-100">
                      {{ formatDate(pedido.fecha_pedido) }}
                    </div>
                    <div class="text-xs text-gray-500">
                      {{ formatTime(pedido.fecha_pedido) }}
                    </div>
                  </td>
                  <td class="px-6 py-4">
                    <div class="text-sm text-gray-900 dark:text-gray-100">
                      {{ formatDate(pedido.fecha_entrega) }}
                    </div>
                    <div class="text-xs text-gray-500">
                      {{ pedido.horario_entrega }}
                    </div>
                    <div class="text-xs text-blue-600 dark:text-blue-400 font-medium">
                      {{ getDaysUntilDelivery(pedido.fecha_entrega) }}
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <UBadge 
                      :color="formatEstadoPedido(pedido.estado).color"
                      variant="soft"
                    >
                      <UIcon 
                        :name="formatEstadoPedido(pedido.estado).icon" 
                        class="w-3 h-3 mr-1" 
                      />
                      {{ formatEstadoPedido(pedido.estado).label }}
                    </UBadge>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm font-medium text-gray-900 dark:text-gray-100">
                      ${{ pedido.monto_total.toLocaleString() }}
                    </div>
                    <div class="text-xs text-gray-500">
                      {{ pedido.items.length }} productos
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="flex items-center justify-center space-x-1">
                      <!-- Acción principal según estado -->
                      <template v-if="pedido.estado === 'Pendiente'">
                        <UButton
                          @click="handleCorregir(pedido)"
                          color="yellow"
                          variant="soft"
                          size="xs"
                          icon="i-heroicons-pencil"
                        />
                        <UButton
                          @click="handleAceptar(pedido)"
                          color="green"
                          variant="soft"
                          size="xs"
                          icon="i-heroicons-check"
                        />
                        <UButton
                          @click="handleRechazar(pedido)"
                          color="red"
                          variant="soft"
                          size="xs"
                          icon="i-heroicons-x-mark"
                        />
                      </template>
                      
                      <template v-else-if="pedido.estado === 'EnCorreccion'">
                        <UButton
                          @click="handleCorregir(pedido)"
                          color="yellow"
                          variant="soft"
                          size="xs"
                          icon="i-heroicons-pencil"
                        />
                      </template>

                      <template v-else-if="pedido.estado === 'Aceptado'">
                        <UButton
                          @click="handleCompletar(pedido)"
                          color="green"
                          variant="soft"
                          size="xs"
                          icon="i-heroicons-check-badge"
                        />
                      </template>
                      
                      <!-- Menú de acciones adicionales -->
                      <UDropdownMenu :items="getActionMenuItems(pedido)">
                        <UButton
                          color="gray"
                          variant="ghost"
                          size="xs"
                          icon="i-heroicons-ellipsis-horizontal"
                        />
                      </UDropdownMenu>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </ClientOnly>

        <!-- Empty state -->
        <ClientOnly>
          <div v-if="!loading && !error && pedidos.length === 0" class="text-center py-12">
            <UIcon name="i-heroicons-inbox" class="h-12 w-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-500 dark:text-gray-400">
              No se encontraron pedidos con los filtros aplicados
            </p>
          </div>
        </ClientOnly>

        <!-- Paginación -->
        <template v-if="totalPages > 1" #footer>
          <div class="flex items-center justify-between px-6 py-3">
            <div class="text-sm text-gray-500">
              Mostrando {{ pedidos.length }} de {{ totalCount }} pedidos
            </div>
            <UPagination
              v-model="currentPage"
              :page-count="pageSize"
              :total="totalCount"
              @update:model-value="handlePageChange"
            />
          </div>
        </template>
      </UCard>
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
              @click="closeRejectModal" 
            />
          </div>
        </template>
        <template #body>
          <div class="space-y-4">
            <p class="text-sm text-gray-600 dark:text-gray-400">
              ¿Estás seguro de que deseas rechazar el pedido #{{ selectedPedido?.numero }}?
            </p>
            
            <UFormField label="Motivo de rechazo" required>
              <UTextarea
                v-model="rejectReason"
                placeholder="Explica el motivo del rechazo..."
                :rows="4"
                class="w-full"
                resize
              />
            </UFormField>
          </div>
        </template>

        <template #footer>
          <div class="flex justify-end gap-3">
            <UButton @click="closeRejectModal" color="gray" variant="ghost">
              Cancelar
            </UButton>
            <UButton
              @click="confirmRechazar"
              color="red"
              :disabled="!rejectReason.trim()"
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
      :pedido="selectedPedidoCorrection"
      :loading="loading"
      @close="closeCorrectionModal"
      @confirm="handleCorrectionConfirm"
    />
  </div>
</template>

<script setup lang="ts">
import type { Pedido, PedidoFiltros } from '~/types/pedidos'
import { formatEstadoPedido, PedidoEstado } from '~/types/pedidos'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Gestión de Pedidos - Dulce y Salado',
  meta: [
    { name: 'description', content: 'Administra todos los pedidos del sistema' }
  ]
})

// Composables
const { 
  pedidos, 
  estadisticas, 
  loading, 
  error, 
  totalPages, 
  totalCount,
  fetchPedidos, 
  fetchEstadisticas,
  aceptarPedido,
  rechazarPedido,
  completarPedido,
  corregirPedido,
  generarLinkWhatsApp,
  generarLinkEmail,
  clearError
} = usePedidos()

// Estado reactivo
const currentPage = ref(1)
const pageSize = ref(20)
const filters = ref<PedidoFiltros>({
  page: 1,
  pageSize: 20
})

const showRejectModal = ref(false)
const selectedPedido = ref<Pedido | null>(null)
const rejectReason = ref('')

// Estado para modal de corrección
const showCorrectionModal = ref(false)
const selectedPedidoCorrection = ref<Pedido | null>(null)


// Opciones para filtros
const estadoOptions = [
  { label: 'Todos', value: undefined },
  { label: 'Pendiente', value: 'Pendiente' },
  { label: 'Aceptado', value: 'Aceptado' },
  { label: 'Rechazado', value: 'Rechazado' },
  { label: 'Completado', value: 'Completado' },
  { label: 'Cancelado', value: 'Cancelado' }
]

// Métodos
const loadData = async () => {
  try {
    await Promise.all([
      fetchPedidos(filters.value),
      fetchEstadisticas()
    ])
  } catch (error) {
    // Errores ya manejados en el composable
  }
}

const refreshData = async () => {
  clearError()
  await loadData()
}

const applyFilters = async () => {
  filters.value.page = 1
  currentPage.value = 1
  await fetchPedidos(filters.value)
}

const clearFilters = async () => {
  filters.value = {
    page: 1,
    pageSize: pageSize.value
  }
  currentPage.value = 1
  await fetchPedidos(filters.value)
}

const handlePageChange = async (page: number) => {
  filters.value.page = page
  await fetchPedidos(filters.value)
}

// Acciones de pedidos
const handleAceptar = async (pedido: Pedido) => {
  try {
    await aceptarPedido(pedido.id)
    await refreshData()
  } catch (error) {
    // Error ya manejado en el composable
  }
}

const handleRechazar = (pedido: Pedido) => {
  console.log('RECHAZANDO')
  selectedPedido.value = pedido
  rejectReason.value = ''
  showRejectModal.value = true
}

const closeRejectModal = () => {
  showRejectModal.value = false
  selectedPedido.value = null
  rejectReason.value = ''
}

const confirmRechazar = async () => {
  if (!selectedPedido.value || !rejectReason.value.trim()) return

  try {
    await rechazarPedido(selectedPedido.value.id, rejectReason.value)
    closeRejectModal()
    await refreshData()
  } catch (error) {
    // Error ya manejado en el composable
  }
}

const handleCompletar = async (pedido: Pedido) => {
  try {
    await completarPedido(pedido.id)
    await refreshData()
  } catch (error) {
    // Error ya manejado en el composable
  }
}

// Manejo de correcciones
const handleCorregir = (pedido: Pedido) => {
  selectedPedidoCorrection.value = pedido
  showCorrectionModal.value = true
}

const closeCorrectionModal = () => {
  showCorrectionModal.value = false
  selectedPedidoCorrection.value = null
}

const handleCorrectionConfirm = async (data: any) => {
  if (!selectedPedidoCorrection.value) return

  try {
    const resultado = await corregirPedido(selectedPedidoCorrection.value.id, data)
    
    // Si se envió al cliente y tenemos token, mostrar opciones de notificación
    if (resultado.success && resultado.token && resultado.enviado_al_cliente) {
      showNotificationOptions(selectedPedidoCorrection.value, resultado.token, resultado.correction_url, data.motivo_final)
    }
    
    closeCorrectionModal()
    await refreshData()
  } catch (error) {
    // Error ya manejado en el composable
  }
}

// Notificaciones
const showNotificationOptions = (pedido: Pedido, token: string, correctionUrl?: string, motivo?: string) => {
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

const notifyByWhatsApp = (pedido: Pedido, url: string, motivo?: string) => {
  let mensaje = `Hola ${pedido.cliente_nombre}, tu pedido #${pedido.numero} necesita ajustes`
  
  if (motivo) {
    mensaje += ` debido a: ${motivo}.`
  } else {
    mensaje += ` por disponibilidad de stock.`
  }

  mensaje += `

¡Revisa los cambios y apruébalos aquí!
🔗 ${url}

⏰ Link válido por 48 horas`
  
  const whatsappUrl = generarLinkWhatsApp(pedido.cliente_telefono || '', mensaje)
  window.open(whatsappUrl, '_blank')
}

const copyToClipboard = async (text: string) => {
  try {
    await navigator.clipboard.writeText(text)
    const toast = useToast()
    toast.add({
      title: 'Copiado',
      description: 'Link copiado al portapapeles',
      color: 'green'
    })
  } catch (error) {
    console.error('Error copiando al portapapeles:', error)
  }
}

// Menú de acciones
const getActionMenuItems = (pedido: Pedido) => {
  const items = []
  
  // Ver detalles siempre disponible
  items.push({
    label: 'Ver Detalles',
    icon: 'i-heroicons-eye',
    to: `/admin/pedidos/${pedido.id}`
  })
  
  // Comunicación si hay datos de contacto
  if (pedido.cliente_telefono || pedido.cliente_email) {
    const contactItems = []
    
    if (pedido.cliente_telefono) {
      contactItems.push({
        label: 'WhatsApp',
        icon: 'i-simple-icons-whatsapp',
        onSelect: () => contactWhatsApp(pedido)
      })
    }
    
    if (pedido.cliente_email) {
      contactItems.push({
        label: 'Email',
        icon: 'i-heroicons-envelope',
        onSelect: () => contactEmail(pedido)
      })
    }
    
    items.push({
      label: 'Contactar',
      icon: 'i-heroicons-chat-bubble-left-ellipsis',
      children: contactItems
    })
  }
  
  return items
}

// Comunicación (mantener para compatibilidad)
const getContactMenuItems = (pedido: Pedido) => {
  const items = []
  
  if (pedido.cliente_telefono) {
    items.push({
      label: 'WhatsApp',
      icon: 'i-simple-icons-whatsapp',
      onSelect: () => contactWhatsApp(pedido)
    })
  }
  
  if (pedido.cliente_email) {
    items.push({
      label: 'Email',
      icon: 'i-heroicons-envelope',
      onSelect: () => contactEmail(pedido)
    })
  }
  
  return items
}

const contactWhatsApp = (pedido: Pedido) => {
  if (!pedido.cliente_telefono) return
  
  const mensaje = `Hola ${pedido.cliente_nombre}, te contactamos respecto a tu pedido #${pedido.numero}. ¡Gracias por elegirnos!`
  const link = generarLinkWhatsApp(pedido.cliente_telefono, mensaje)
  window.open(link, '_blank')
}

const contactEmail = (pedido: Pedido) => {
  const asunto = `Pedido #${pedido.numero} - Dulce y Salado`
  const mensaje = `Estimado/a ${pedido.cliente_nombre},\n\nTe contactamos respecto a tu pedido #${pedido.numero}.\n\nSaludos,\nEquipo Dulce y Salado`
  const link = generarLinkEmail(pedido.cliente_email, asunto, mensaje)
  window.open(link, '_blank')
}

// Utilidades
const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('es-ES')
}

const formatTime = (dateString: string) => {
  return new Date(dateString).toLocaleTimeString('es-ES', { 
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
    return 'HOY'
  } else if (diffDays === 1) {
    return 'MAÑANA'
  } else {
    return `En ${diffDays} días`
  }
}

// Cargar datos al montar
onMounted(() => {
  loadData()
})
</script>