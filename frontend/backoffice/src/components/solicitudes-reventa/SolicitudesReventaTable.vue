<template>
  <div class="overflow-x-auto">
    <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
      <thead class="bg-gray-50 dark:bg-gray-800">
        <tr>
          <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-700"
              @click="handleSort('fechaSolicitud')">
            <div class="flex items-center">
              Fecha
              <UIcon :name="getSortIcon('fechaSolicitud')" class="ml-1 h-4 w-4" />
            </div>
          </th>
          <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-700"
              @click="handleSort('clienteNombre')">
            <div class="flex items-center">
              Cliente
              <UIcon :name="getSortIcon('clienteNombre')" class="ml-1 h-4 w-4" />
            </div>
          </th>
          <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
            Empresa
          </th>
          <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-700"
              @click="handleSort('estado')">
            <div class="flex items-center">
              Estado
              <UIcon :name="getSortIcon('estado')" class="ml-1 h-4 w-4" />
            </div>
          </th>
          <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
            Respuesta
          </th>
          <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
            Acciones
          </th>
        </tr>
      </thead>
      <tbody class="bg-white dark:bg-gray-900 divide-y divide-gray-200 dark:divide-gray-700">
        <tr v-for="solicitud in solicitudes" 
            :key="solicitud.id" 
            class="hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
          
          <!-- Fecha -->
          <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-gray-100">
            <div>
              <div class="font-medium">
                {{ formatDate(solicitud.fecha_solicitud) }}
              </div>
              <div class="text-gray-500 dark:text-gray-400 text-xs">
                {{ formatTime(solicitud.fecha_solicitud) }}
              </div>
            </div>
          </td>

          <!-- Cliente -->
          <td class="px-6 py-4 text-sm text-gray-900 dark:text-gray-100">
            <div>
              <div class="font-medium">{{ solicitud.cliente_nombre }}</div>
              <div class="flex items-center space-x-2">
                <a 
                  v-if="solicitud.cliente_email"
                  :href="generarLinkEmail(solicitud.cliente_email, 'Solicitud de Reventa', `Estimado/a ${solicitud.cliente_nombre}, te contactamos respecto a tu solicitud de cuenta de reventa.`)"
                  class="text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 text-xs flex items-center"
                  target="_blank"
                  :title="`Enviar email a ${solicitud.cliente_email}`"
                >
                  <UIcon name="i-heroicons-envelope" class="h-3 w-3 mr-1" />
                  {{ solicitud.cliente_email }}
                </a>
                <span v-else class="text-gray-500 dark:text-gray-400 text-xs">{{ solicitud.cliente_email }}</span>
              </div>
              <div v-if="solicitud.telefono_comercial" class="flex items-center space-x-1 mt-1">
                <a 
                  :href="generarLinkWhatsApp(solicitud.telefono_comercial, `Hola ${solicitud.cliente_nombre}, te contactamos respecto a tu solicitud de cuenta de reventa. ¡Gracias por elegirnos!`)"
                  class="text-green-600 dark:text-green-400 hover:text-green-800 dark:hover:text-green-300 text-xs flex items-center"
                  target="_blank"
                  :title="`Contactar por WhatsApp: ${solicitud.telefono_comercial}`"
                >
                  <UIcon name="i-simple-icons-whatsapp" class="h-3 w-3 mr-1" />
                  {{ solicitud.telefono_comercial }}
                </a>
              </div>
            </div>
          </td>

          <!-- Empresa -->
          <td class="px-6 py-4 text-sm text-gray-900 dark:text-gray-100">
            <div>
              <div class="font-medium">{{ solicitud.razon_social }}</div>
              <div class="text-gray-500 dark:text-gray-400">CUIT: {{ solicitud.cuit }}</div>
            </div>
          </td>

          <!-- Estado -->
          <td class="px-6 py-4 whitespace-nowrap">
            <UBadge
              :color="getEstadoColor(solicitud.estado)"
              variant="soft"
              size="sm"
            >
              {{ solicitud.estado }}
            </UBadge>
          </td>

          <!-- Respuesta -->
          <td class="px-6 py-4 text-sm text-gray-500 dark:text-gray-400">
            <div v-if="solicitud.fecha_respuesta">
              <div class="text-xs">{{ formatDate(solicitud.fecha_respuesta) }}</div>
              <div class="text-xs">Por: {{ solicitud.respondido_por }}</div>
              <div v-if="solicitud.comentario_respuesta" class="text-xs mt-1 italic">
                "{{ solicitud.comentario_respuesta }}"
              </div>
            </div>
            <span v-else class="text-gray-400">-</span>
          </td>

          <!-- Acciones -->
          <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
            <div class="flex justify-end space-x-2">
              <!-- Ver detalles -->
              <UButton
                @click="$emit('view', solicitud)"
                color="blue"
                variant="ghost"
                size="sm"
                icon="i-heroicons-eye"
                :ui="{ rounded: 'rounded-full' }"
                class="p-1"
              />

              <!-- Aprobar (solo si está pendiente o rechazada) -->
              <UButton
                v-if="solicitud.estado !== 'Aprobada'"
                @click="$emit('approve', solicitud)"
                color="green"
                variant="ghost"
                size="sm"
                icon="i-heroicons-check"
                :ui="{ rounded: 'rounded-full' }"
                class="p-1"
              />

              <!-- Rechazar (solo si está pendiente) -->
              <UButton
                v-if="solicitud.estado === 'Pendiente'"
                @click="$emit('reject', solicitud)"
                color="red"
                variant="ghost"
                size="sm"
                icon="i-heroicons-x-mark"
                :ui="{ rounded: 'rounded-full' }"
                class="p-1"
              />
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import type { SolicitudReventaDto } from '~/composables/useSolicitudesReventa'

interface Props {
  solicitudes: SolicitudReventaDto[]
  loading: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

interface Emits {
  view: [solicitud: SolicitudReventaDto]
  approve: [solicitud: SolicitudReventaDto]
  reject: [solicitud: SolicitudReventaDto]
  sort: [column: string, direction: 'asc' | 'desc']
}

const props = withDefaults(defineProps<Props>(), {
  sortBy: 'fechaSolicitud',
  sortOrder: 'desc'
})

const emit = defineEmits<Emits>()

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

const getSortIcon = (column: string) => {
  if (props.sortBy !== column) {
    return 'i-heroicons-arrows-up-down'
  }
  return props.sortOrder === 'asc' 
    ? 'i-heroicons-chevron-up' 
    : 'i-heroicons-chevron-down'
}

const handleSort = (column: string) => {
  const newOrder = props.sortBy === column && props.sortOrder === 'asc' ? 'desc' : 'asc'
  emit('sort', column, newOrder)
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
</script>