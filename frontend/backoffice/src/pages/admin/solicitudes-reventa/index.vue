<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center justify-between mb-4">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
              Solicitudes de Reventa
            </h1>
            <p class="text-gray-600 dark:text-gray-400 mt-2">
              Administra las solicitudes de cuentas de reventa de clientes
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
          <div v-if="estadisticas" class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
            <UCard class="p-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-blue-600 dark:text-blue-400">
                  {{ estadisticas.total }}
                </div>
                <div class="text-sm text-gray-500">Total</div>
              </div>
            </UCard>
            <UCard class="p-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-orange-600 dark:text-orange-400">
                  {{ estadisticas.pendientes }}
                </div>
                <div class="text-sm text-gray-500">Pendientes</div>
              </div>
            </UCard>
            <UCard class="p-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-green-600 dark:text-green-400">
                  {{ estadisticas.aprobadas }}
                </div>
                <div class="text-sm text-gray-500">Aprobadas</div>
              </div>
            </UCard>
            <UCard class="p-4">
              <div class="text-center">
                <div class="text-2xl font-bold text-red-600 dark:text-red-400">
                  {{ estadisticas.rechazadas }}
                </div>
                <div class="text-sm text-gray-500">Rechazadas</div>
              </div>
            </UCard>
          </div>
          <template #fallback>
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
              <USkeleton v-for="i in 4" :key="i" class="h-20 w-full" />
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
              @click="clearAllFilters"
              color="gray"
              variant="ghost"
              size="sm"
            >
              Limpiar
            </UButton>
          </div>
        </template>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <!-- Búsqueda -->
          <UFormField label="Buscar Cliente">
            <UInput
              v-model="searchQuery"
              placeholder="Nombre, email, CUIT, razón social..."
              icon="i-heroicons-magnifying-glass"
              @input="debouncedSearch"
            />
          </UFormField>

          <!-- Estado -->
          <UFormField label="Estado">
            <USelectMenu
              v-model="estadoFilter"
              :items="estadoOptions"
              value-key="value"
              placeholder="Todos los estados"
              @change="applyFilters"
            />
          </UFormField>
        </div>

        <!-- Resultado del filtro -->
        <div class="flex justify-between items-center mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
          <div class="text-sm text-gray-500 dark:text-gray-400">
            {{ pagination.total }} solicitud{{ pagination.total !== 1 ? 'es' : '' }} encontrada{{ pagination.total !== 1 ? 's' : '' }}
          </div>
        </div>
      </UCard>

      <!-- Lista de Solicitudes -->
      <UCard>
        <div v-if="loading || initialLoading" class="space-y-4">
          <!-- Header skeleton -->
          <div class="flex justify-between items-center p-4 border-b border-gray-200 dark:border-gray-700">
            <div class="flex space-x-4">
              <USkeleton class="h-4 w-20" />
              <USkeleton class="h-4 w-32" />
              <USkeleton class="h-4 w-24" />
              <USkeleton class="h-4 w-20" />
              <USkeleton class="h-4 w-16" />
              <USkeleton class="h-4 w-24" />
            </div>
          </div>
          
          <!-- Rows skeleton -->
          <div class="space-y-3 p-4">
            <div v-for="n in 5" :key="n" class="flex justify-between items-center py-3 border-b border-gray-100 dark:border-gray-800">
              <div class="flex items-center space-x-4 flex-1">
                <div class="w-32">
                  <USkeleton class="h-4 w-28" />
                </div>
                <div class="flex-1">
                  <USkeleton class="h-4 w-48 mb-1" />
                  <USkeleton class="h-3 w-32" />
                </div>
                <div class="w-24">
                  <USkeleton class="h-6 w-20 rounded-full" />
                </div>
                <div class="w-32">
                  <USkeleton class="h-4 w-28" />
                </div>
                <div class="w-24">
                  <div class="flex space-x-1">
                    <USkeleton class="h-8 w-8 rounded" />
                    <USkeleton class="h-8 w-8 rounded" />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div v-else-if="error && !initialLoading" class="text-center py-8">
          <UIcon name="i-heroicons-exclamation-triangle" class="h-12 w-12 text-red-500 mx-auto mb-2" />
          <p class="text-red-600 dark:text-red-400">{{ error }}</p>
          <UButton
            color="red"
            variant="ghost"
            class="mt-2"
            @click="fetchSolicitudes()"
          >
            Reintentar
          </UButton>
        </div>

        <div v-else-if="solicitudes.length === 0 && !initialLoading" class="text-center py-8">
          <UIcon name="i-heroicons-document-text" class="h-12 w-12 text-gray-400 mx-auto mb-2" />
          <p class="text-gray-600 dark:text-gray-400">No se encontraron solicitudes</p>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
            Las solicitudes aparecerán aquí cuando los clientes las envíen desde el catálogo
          </p>
        </div>

        <ClientOnly>
          <SolicitudesReventaTable
            v-if="!loading && !initialLoading && !error && solicitudes.length > 0"
            :solicitudes="solicitudes"
            :loading="false"
            :sort-by="filters.sortBy"
            :sort-order="filters.sortOrder"
            @view="viewSolicitud"
            @approve="approveSolicitud"
            @reject="rejectSolicitud"
            @sort="handleSort"
          />
        </ClientOnly>

        <!-- Paginación -->
        <div v-if="pagination.total > pagination.limit" class="flex justify-center mt-6">
          <UPagination
            v-model:page="currentPage"
            :total="Number(pagination.total)"
            :items-per-page="Number(pagination.limit)"
            :sibling-count="2"
            :size="'sm'"
            show-edges
            @update:page="changePage"
          />
        </div>
      </UCard>
    </div>
  </div>

  <!-- Modal de Respuesta -->
  <UModal v-model:open="showResponseModal" :ui="{ width: 'sm:max-w-2xl' }">
    <template #header>
      <div class="flex items-center">
        <UIcon 
          :name="responseAction === 'approve' ? 'i-heroicons-check-circle' : 'i-heroicons-x-circle'" 
          :class="responseAction === 'approve' ? 'text-green-500' : 'text-red-500'" 
          class="h-6 w-6 mr-2" 
        />
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          {{ responseAction === 'approve' ? 'Aprobar Solicitud' : 'Rechazar Solicitud' }}
        </h3>
      </div>
    </template>
    
    <template #body>
      <div v-if="selectedSolicitud" class="space-y-4">
        <!-- Información del cliente -->
        <div class="bg-gray-50 dark:bg-gray-800 rounded-lg p-4">
          <div class="flex items-center justify-between mb-2">
            <h4 class="font-semibold text-gray-900 dark:text-gray-100">Cliente</h4>
            <!-- Enlaces de contacto rápido -->
            <div class="flex space-x-2">
              <UButton
                v-if="selectedSolicitud.cliente_email"
                :to="generarLinkEmail(selectedSolicitud.cliente_email, 'Solicitud de Reventa', `Estimado/a ${selectedSolicitud.cliente_nombre}, te contactamos respecto a tu solicitud de cuenta de reventa.`)"
                target="_blank"
                color="blue"
                variant="soft"
                size="xs"
                icon="i-heroicons-envelope"
                :title="`Enviar email a ${selectedSolicitud.cliente_email}`"
              >
                Email
              </UButton>
              <UButton
                v-if="selectedSolicitud.telefono_comercial"
                :to="generarLinkWhatsApp(selectedSolicitud.telefono_comercial, `Hola ${selectedSolicitud.cliente_nombre}, te contactamos respecto a tu solicitud de cuenta de reventa. ¡Gracias por elegirnos!`)"
                target="_blank"
                color="green"
                variant="soft"
                size="xs"
                icon="i-simple-icons-whatsapp"
                :title="`Contactar por WhatsApp: ${selectedSolicitud.telefono_comercial}`"
              >
                WhatsApp
              </UButton>
            </div>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-2 text-sm">
            <div><strong>Nombre:</strong> {{ selectedSolicitud.cliente_nombre }}</div>
            <div><strong>Email:</strong> {{ selectedSolicitud.cliente_email }}</div>
            <div><strong>CUIT:</strong> {{ selectedSolicitud.cuit }}</div>
            <div><strong>Razón Social:</strong> {{ selectedSolicitud.razon_social }}</div>
            <div><strong>Teléfono:</strong> {{ selectedSolicitud.telefono_comercial }}</div>
            <div><strong>Dirección:</strong> {{ selectedSolicitud.direccion_comercial }}</div>
          </div>
        </div>

        <!-- Formulario de respuesta -->
        <div class="w-full">
          <UFormField
            :label="responseAction === 'approve' ? 'Comentario de aprobación (opcional)' : 'Motivo de rechazo'"
            :required="responseAction === 'reject'"
            class="w-full"
          >
            <UTextarea
              v-model="comentarioRespuesta"
              :placeholder="responseAction === 'approve' ? 'Comentario adicional...' : 'Indique el motivo del rechazo...'"
              :rows="responseAction === 'reject' ? 5 : 3"
              class="w-full"
              :ui="{ base: 'w-full' }"
            />
          </UFormField>
        </div>

        <div v-if="responseAction === 'approve'" class="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg p-3">
          <div class="flex">
            <UIcon name="i-heroicons-information-circle" class="h-5 w-5 text-green-400 mr-2 mt-0.5" />
            <div>
              <p class="text-sm text-green-700 dark:text-green-300">
                Al aprobar esta solicitud, el cliente será automáticamente asignado a la lista de precios de reventa (código "2")
                y recibirá una notificación por email.
              </p>
            </div>
          </div>
        </div>

        <div v-if="responseAction === 'reject'" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-3">
          <div class="flex">
            <UIcon name="i-heroicons-exclamation-triangle" class="h-5 w-5 text-red-400 mr-2 mt-0.5" />
            <div>
              <p class="text-sm text-red-700 dark:text-red-300">
                Al rechazar esta solicitud, el cliente recibirá una notificación por email con el motivo especificado.
                Es importante proporcionar una explicación clara y profesional.
              </p>
            </div>
          </div>
        </div>
      </div>
    </template>
    
    <template #footer>
      <div class="flex justify-end space-x-3">
        <UButton
          variant="ghost"
          color="gray"
          @click="showResponseModal = false"
        >
          Cancelar
        </UButton>
        <UButton
          :color="responseAction === 'approve' ? 'green' : 'red'"
          :loading="loading"
          @click="confirmResponse"
        >
          {{ responseAction === 'approve' ? 'Aprobar Solicitud' : 'Rechazar Solicitud' }}
        </UButton>
      </div>
    </template>
  </UModal>

  <!-- Modal de Vista Detallada -->
  <UModal v-model:open="showViewModal" :ui="{ width: 'sm:max-w-4xl' }">
    <template #header>
      <div class="flex items-center">
        <UIcon name="i-heroicons-document-text" class="h-6 w-6 text-blue-500 mr-2" />
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Detalles de Solicitud de Reventa
        </h3>
      </div>
    </template>

    <template #body>
      <div v-if="selectedSolicitud" class="space-y-6">
        <!-- Estado y fecha -->
        <div class="flex justify-between items-start">
          <div>
            <div class="flex items-center space-x-3 mb-2">
              <UBadge
                :color="getEstadoColor(selectedSolicitud.estado)"
                variant="soft"
                size="lg"
              >
                {{ selectedSolicitud.estado }}
              </UBadge>
              <span class="text-sm text-gray-500">ID: #{{ selectedSolicitud.id }}</span>
            </div>
            <p class="text-sm text-gray-600 dark:text-gray-400">
              Solicitud enviada el {{ formatDate(selectedSolicitud.fecha_solicitud) }} a las {{ formatTime(selectedSolicitud.fecha_solicitud) }}
            </p>
          </div>
        </div>

        <!-- Información del Cliente -->
        <UCard>
          <template #header>
            <div class="flex items-center justify-between">
              <h4 class="font-semibold text-gray-900 dark:text-gray-100 flex items-center">
                <UIcon name="i-heroicons-user" class="h-5 w-5 mr-2" />
                Información del Cliente
              </h4>
              <!-- Enlaces de contacto rápido -->
              <div class="flex space-x-2">
                <UButton
                  v-if="selectedSolicitud.cliente_email"
                  :to="generarLinkEmail(selectedSolicitud.cliente_email, 'Solicitud de Reventa', `Estimado/a ${selectedSolicitud.cliente_nombre}, te contactamos respecto a tu solicitud de cuenta de reventa.`)"
                  target="_blank"
                  color="blue"
                  variant="soft"
                  size="xs"
                  icon="i-heroicons-envelope"
                  :title="`Enviar email a ${selectedSolicitud.cliente_email}`"
                >
                  Enviar Email
                </UButton>
                <UButton
                  v-if="selectedSolicitud.telefono_comercial"
                  :to="generarLinkWhatsApp(selectedSolicitud.telefono_comercial, `Hola ${selectedSolicitud.cliente_nombre}, te contactamos respecto a tu solicitud de cuenta de reventa. ¡Gracias por elegirnos!`)"
                  target="_blank"
                  color="green"
                  variant="soft"
                  size="xs"
                  icon="i-simple-icons-whatsapp"
                  :title="`Contactar por WhatsApp: ${selectedSolicitud.telefono_comercial}`"
                >
                  WhatsApp
                </UButton>
              </div>
            </div>
          </template>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Nombre</label>
              <p class="text-gray-900 dark:text-gray-100">{{ selectedSolicitud.cliente_nombre || 'No especificado' }}</p>
            </div>
            <div>
              <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Email</label>
              <p class="text-gray-900 dark:text-gray-100">{{ selectedSolicitud.cliente_email || 'No especificado' }}</p>
            </div>
          </div>
        </UCard>

        <!-- Información de la Empresa -->
        <UCard>
          <template #header>
            <h4 class="font-semibold text-gray-900 dark:text-gray-100 flex items-center">
              <UIcon name="i-heroicons-building-office" class="h-5 w-5 mr-2" />
              Información de la Empresa
            </h4>
          </template>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Razón Social</label>
              <p class="text-gray-900 dark:text-gray-100 font-medium">{{ selectedSolicitud.razon_social || 'No especificado' }}</p>
            </div>
            <div>
              <label class="text-sm font-medium text-gray-500 dark:text-gray-400">CUIT</label>
              <p class="text-gray-900 dark:text-gray-100 font-mono">{{ selectedSolicitud.cuit || 'No especificado' }}</p>
            </div>
            <div>
              <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Categoría IVA</label>
              <p class="text-gray-900 dark:text-gray-100">{{ selectedSolicitud.categoria_iva || 'No especificado' }}</p>
            </div>
            <div>
              <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Email Comercial</label>
              <p class="text-gray-900 dark:text-gray-100">{{ selectedSolicitud.email_comercial || 'No especificado' }}</p>
            </div>
            <div>
              <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Teléfono Comercial</label>
              <p class="text-gray-900 dark:text-gray-100">{{ selectedSolicitud.telefono_comercial || 'No especificado' }}</p>
            </div>
          </div>
        </UCard>

        <!-- Dirección Comercial -->
        <UCard>
          <template #header>
            <h4 class="font-semibold text-gray-900 dark:text-gray-100 flex items-center">
              <UIcon name="i-heroicons-map-pin" class="h-5 w-5 mr-2" />
              Dirección Comercial
            </h4>
          </template>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="md:col-span-2">
              <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Dirección</label>
              <p class="text-gray-900 dark:text-gray-100">{{ selectedSolicitud.direccion_comercial || 'No especificada' }}</p>
            </div>
            <div>
              <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Código Postal</label>
              <p class="text-gray-900 dark:text-gray-100">{{ selectedSolicitud.codigo_postal || 'No especificado' }}</p>
            </div>
            <div>
              <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Localidad</label>
              <p class="text-gray-900 dark:text-gray-100">{{ selectedSolicitud.localidad || 'No especificada' }}</p>
            </div>
            <div>
              <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Provincia</label>
              <p class="text-gray-900 dark:text-gray-100">{{ selectedSolicitud.provincia || 'No especificada' }}</p>
            </div>
          </div>
        </UCard>

        <!-- Información de Respuesta (si existe) -->
        <UCard v-if="selectedSolicitud.fecha_respuesta">
          <template #header>
            <h4 class="font-semibold text-gray-900 dark:text-gray-100 flex items-center">
              <UIcon name="i-heroicons-chat-bubble-left-right" class="h-5 w-5 mr-2" />
              Respuesta Administrativa
            </h4>
          </template>

          <div class="space-y-3">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Fecha de Respuesta</label>
                <p class="text-gray-900 dark:text-gray-100">{{ formatDate(selectedSolicitud.fecha_respuesta) }}</p>
              </div>
              <div>
                <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Respondido por</label>
                <p class="text-gray-900 dark:text-gray-100">{{ selectedSolicitud.respondido_por || 'Sistema' }}</p>
              </div>
            </div>
            <div v-if="selectedSolicitud.comentario_respuesta">
              <label class="text-sm font-medium text-gray-500 dark:text-gray-400">Comentario</label>
              <div class="mt-1 p-3 bg-gray-50 dark:bg-gray-800 rounded-lg">
                <p class="text-gray-900 dark:text-gray-100 italic">{{ selectedSolicitud.comentario_respuesta }}</p>
              </div>
            </div>
          </div>
        </UCard>
      </div>
    </template>

    <template #footer>
      <div class="flex justify-between items-center">
        <!-- Acciones de respuesta (solo si está pendiente) -->
        <div v-if="selectedSolicitud?.estado === 'Pendiente'" class="flex space-x-3">
          <UButton
            @click="approveFromView"
            color="green"
            icon="i-heroicons-check"
          >
            Aprobar Solicitud
          </UButton>
          <UButton
            @click="rejectFromView"
            color="red"
            variant="outline"
            icon="i-heroicons-x-mark"
          >
            Rechazar Solicitud
          </UButton>
        </div>
        <div v-else></div>

        <!-- Cerrar -->
        <UButton
          variant="ghost"
          color="gray"
          @click="showViewModal = false"
        >
          Cerrar
        </UButton>
      </div>
    </template>
  </UModal>
</template>

<script setup lang="ts">
import type { SolicitudReventaDto } from '~/composables/useSolicitudesReventa'

// Configuración de página
definePageMeta({
  middleware: ['auth'],
  layout: 'default'
})

useHead({
  title: 'Solicitudes de Reventa',
  meta: [
    { name: 'description', content: 'Administra las solicitudes de cuentas de reventa de clientes' }
  ]
})

// Composables
const { 
  solicitudes,
  loading,
  error,
  pagination,
  filters,
  estadisticas,
  fetchSolicitudes,
  responderSolicitud,
  applyFilters: applyFiltersAction,
  clearFilters,
  changePage: changePageAction,
  applySorting
} = useSolicitudesReventa()

// Estado reactivo
const searchQuery = ref('')
const estadoFilter = ref<string>('all')
const currentPage = ref(1)
const initialLoading = ref(true)

// Modal de respuesta
const showResponseModal = ref(false)
const selectedSolicitud = ref<SolicitudReventaDto | null>(null)
const responseAction = ref<'approve' | 'reject'>('approve')
const comentarioRespuesta = ref('')

// Modal de vista detallada
const showViewModal = ref(false)

// Opciones para filtros
const estadoOptions = [
  { label: 'Todos los estados', value: 'all' },
  { label: 'Pendientes', value: 'Pendiente' },
  { label: 'Aprobadas', value: 'Aprobada' },
  { label: 'Rechazadas', value: 'Rechazada' }
]

// Búsqueda debounced
let debounceTimer: NodeJS.Timeout
const debouncedSearch = () => {
  clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    applyFilters()
  }, 300)
}

// Métodos
const refreshData = async () => {
  await fetchSolicitudes()
}

const applyFilters = async () => {
  await applyFiltersAction({
    search: searchQuery.value,
    estado: estadoFilter.value === 'all' ? '' : estadoFilter.value || '',
    page: 1
  })
  currentPage.value = 1
}

const clearAllFilters = async () => {
  searchQuery.value = ''
  estadoFilter.value = 'all'
  currentPage.value = 1
  await clearFilters()
}

const changePage = async (page: number) => {
  currentPage.value = page
  await changePageAction(page)
}

const handleSort = async (column: string, direction: 'asc' | 'desc') => {
  await applySorting(column, direction)
}

const viewSolicitud = (solicitud: SolicitudReventaDto) => {
  selectedSolicitud.value = solicitud
  showViewModal.value = true
}

const approveSolicitud = (solicitud: SolicitudReventaDto) => {
  selectedSolicitud.value = solicitud
  responseAction.value = 'approve'
  comentarioRespuesta.value = ''
  showResponseModal.value = true
}

const rejectSolicitud = (solicitud: SolicitudReventaDto) => {
  selectedSolicitud.value = solicitud
  responseAction.value = 'reject'
  comentarioRespuesta.value = ''
  showResponseModal.value = true
}

const confirmResponse = async () => {
  if (!selectedSolicitud.value) return

  try {
    await responderSolicitud(selectedSolicitud.value.id, {
      aprobar: responseAction.value === 'approve',
      comentarioRespuesta: comentarioRespuesta.value || undefined
    })

    showResponseModal.value = false

    // Mostrar notificación de éxito
    const toast = useToast()
    toast.add({
      title: responseAction.value === 'approve' ? 'Solicitud aprobada' : 'Solicitud rechazada',
      description: `La solicitud de ${selectedSolicitud.value.cliente_nombre} ha sido ${responseAction.value === 'approve' ? 'aprobada' : 'rechazada'} exitosamente.`,
      icon: responseAction.value === 'approve' ? 'i-heroicons-check-circle' : 'i-heroicons-x-circle',
      color: responseAction.value === 'approve' ? 'green' : 'red'
    })

  } catch (error) {
    console.error('Error al responder solicitud:', error)
  }
}

// Métodos para aprobar/rechazar desde el modal de vista
const approveFromView = () => {
  showViewModal.value = false
  if (selectedSolicitud.value) {
    approveSolicitud(selectedSolicitud.value)
  }
}

const rejectFromView = () => {
  showViewModal.value = false
  if (selectedSolicitud.value) {
    rejectSolicitud(selectedSolicitud.value)
  }
}

// Watcher para sincronizar currentPage con pagination
watch(() => pagination.value.page, (newPage) => {
  if (newPage !== currentPage.value) {
    currentPage.value = newPage
  }
})

// Métodos de utilidad
const formatDate = (dateString: string) => {
  if (!dateString) return '-'
  try {
    const date = new Date(dateString)
    if (isNaN(date.getTime())) return 'Fecha inválida'

    return date.toLocaleDateString('es-AR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    })
  } catch (error) {
    console.error('Error formatting date:', error)
    return 'Fecha inválida'
  }
}

const formatTime = (dateString: string) => {
  if (!dateString) return '-'
  try {
    const date = new Date(dateString)
    if (isNaN(date.getTime())) return ''

    return date.toLocaleTimeString('es-AR', {
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch (error) {
    console.error('Error formatting time:', error)
    return ''
  }
}

const getEstadoColor = (estado: string) => {
  switch (estado) {
    case 'Pendiente':
      return 'orange'
    case 'Aprobada':
      return 'green'
    case 'Rechazada':
      return 'red'
    default:
      return 'gray'
  }
}

// Generar enlaces de contacto
const generarLinkWhatsApp = (telefono: string, mensaje: string) => {
  const numeroLimpio = telefono.replace(/\D/g, '')
  const mensajeCodificado = encodeURIComponent(mensaje)
  return `https://wa.me/54${numeroLimpio}?text=${mensajeCodificado}`
}

const generarLinkEmail = (email: string, asunto: string, mensaje: string) => {
  const asuntoCodificado = encodeURIComponent(asunto)
  const mensajeCodificado = encodeURIComponent(mensaje)
  return `mailto:${email}?subject=${asuntoCodificado}&body=${mensajeCodificado}`
}

// Cargar datos iniciales
onMounted(async () => {
  try {
    await fetchSolicitudes()
  } finally {
    initialLoading.value = false
  }
})
</script>