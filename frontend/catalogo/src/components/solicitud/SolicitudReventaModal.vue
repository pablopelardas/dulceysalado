<template>
  <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
    <div class="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
      <!-- Header -->
      <div class="sticky top-0 bg-white border-b px-6 py-4">
        <div class="flex items-center justify-between">
          <h2 class="text-xl font-semibold text-gray-900">
            Solicitud de Cuenta de Comerciante
          </h2>
          <button @click="close" class="text-gray-400 hover:text-gray-500 cursor-pointer">
            <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </div>

      <!-- Content -->
      <div class="p-6">
        <!-- Estado de solicitud existente -->
        <div v-if="solicitudExistente" class="mb-6">
          <div v-if="solicitudExistente.estado === 'Pendiente'" class="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
            <div class="flex">
              <div class="flex-shrink-0">
                <svg class="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                  <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
                </svg>
              </div>
              <div class="ml-3">
                <h3 class="text-sm font-medium text-yellow-800">Solicitud en revisión</h3>
                <div class="mt-2 text-sm text-yellow-700">
                  <p>Tu solicitud de cuenta de comerciante está siendo revisada. Te notificaremos por email cuando tengamos una respuesta.</p>
                  <p class="mt-1">Fecha de solicitud: {{ formatDate(solicitudExistente.fecha_solicitud) }}</p>
                </div>
              </div>
            </div>
          </div>

          <div v-else-if="solicitudExistente.estado === 'Aprobada'" class="bg-green-50 border border-green-200 rounded-lg p-4">
            <div class="flex">
              <div class="flex-shrink-0">
                <svg class="h-5 w-5 text-green-400" viewBox="0 0 20 20" fill="currentColor">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
                </svg>
              </div>
              <div class="ml-3">
                <h3 class="text-sm font-medium text-green-800">Solicitud aprobada</h3>
                <div class="mt-2 text-sm text-green-700">
                  <p>¡Felicitaciones! Tu cuenta ya tiene acceso a los precios de comerciante.</p>
                  <p class="mt-1">Fecha de aprobación: {{ formatDate(solicitudExistente.fecha_respuesta) }}</p>
                </div>
              </div>
            </div>
          </div>

          <div v-else-if="solicitudExistente.estado === 'Rechazada'" class="bg-red-50 border border-red-200 rounded-lg p-4">
            <div class="flex">
              <div class="flex-shrink-0">
                <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
                </svg>
              </div>
              <div class="ml-3">
                <h3 class="text-sm font-medium text-red-800">Solicitud rechazada</h3>
                <div class="mt-2 text-sm text-red-700">
                  <p>Tu solicitud no fue aprobada en esta ocasión.</p>
                  <p v-if="solicitudExistente.comentario_respuesta" class="mt-2">
                    <strong>Motivo:</strong> {{ solicitudExistente.comentario_respuesta }}
                  </p>
                  <p class="mt-1">Fecha: {{ formatDate(solicitudExistente.fecha_respuesta) }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Formulario -->
        <form v-if="!solicitudExistente || solicitudExistente.estado === 'Rechazada'" @submit.prevent="handleSubmit">
          <div class="space-y-6">
            <div>
              <p class="text-sm text-gray-600 mb-4">
                Completa este formulario para solicitar acceso a precios especiales de comerciante. 
                Revisaremos tu solicitud y te contactaremos pronto.
              </p>
            </div>

            <!-- Datos de la empresa -->
            <div class="space-y-4">
              <h3 class="text-lg font-medium text-gray-900">Datos de la Empresa</h3>
              
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label for="razon_social" class="block text-sm font-medium text-gray-700">
                    Razón Social *
                  </label>
                  <input
                    v-model="formData.razon_social"
                    type="text"
                    id="razon_social"
                    required
                    class="mt-1 block w-full px-4 py-3 rounded-md border-gray-300 shadow-sm focus:border-red-500 focus:ring-red-500 text-base"
                  />
                </div>

                <div>
                  <label for="cuit" class="block text-sm font-medium text-gray-700">
                    CUIT *
                  </label>
                  <input
                    v-model="formData.cuit"
                    type="text"
                    id="cuit"
                    required
                    placeholder="XX-XXXXXXXX-X"
                    class="mt-1 block w-full px-4 py-3 rounded-md border-gray-300 shadow-sm focus:border-red-500 focus:ring-red-500 text-base"
                  />
                </div>

                <div>
                  <label for="categoria_iva" class="block text-sm font-medium text-gray-700">
                    Categoría IVA *
                  </label>
                  <select
                    v-model="formData.categoria_iva"
                    id="categoria_iva"
                    required
                    class="mt-1 block w-full px-4 py-3 rounded-md border-gray-300 shadow-sm focus:border-red-500 focus:ring-red-500 text-base"
                  >
                    <option value="">Seleccionar...</option>
                    <option value="Responsable Inscripto">Responsable Inscripto</option>
                    <option value="Monotributista">Monotributista</option>
                    <option value="Exento">Exento</option>
                    <option value="Consumidor Final">Consumidor Final</option>
                  </select>
                </div>

                <div>
                  <label for="email_comercial" class="block text-sm font-medium text-gray-700">
                    Email Comercial *
                  </label>
                  <input
                    v-model="formData.email_comercial"
                    type="email"
                    id="email_comercial"
                    required
                    class="mt-1 block w-full px-4 py-3 rounded-md border-gray-300 shadow-sm focus:border-red-500 focus:ring-red-500 text-base"
                  />
                </div>

                <div>
                  <label for="telefono_comercial" class="block text-sm font-medium text-gray-700">
                    Teléfono Comercial *
                  </label>
                  <input
                    v-model="formData.telefono_comercial"
                    type="tel"
                    id="telefono_comercial"
                    required
                    class="mt-1 block w-full px-4 py-3 rounded-md border-gray-300 shadow-sm focus:border-red-500 focus:ring-red-500 text-base"
                  />
                </div>
              </div>
            </div>

            <!-- Dirección -->
            <div class="space-y-4">
              <h3 class="text-lg font-medium text-gray-900">Dirección Comercial</h3>
              
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div class="md:col-span-2">
                  <label for="direccion_comercial" class="block text-sm font-medium text-gray-700">
                    Dirección *
                  </label>
                  <input
                    v-model="formData.direccion_comercial"
                    type="text"
                    id="direccion_comercial"
                    required
                    class="mt-1 block w-full px-4 py-3 rounded-md border-gray-300 shadow-sm focus:border-red-500 focus:ring-red-500 text-base"
                  />
                </div>

                <div>
                  <label for="localidad" class="block text-sm font-medium text-gray-700">
                    Localidad *
                  </label>
                  <input
                    v-model="formData.localidad"
                    type="text"
                    id="localidad"
                    required
                    class="mt-1 block w-full px-4 py-3 rounded-md border-gray-300 shadow-sm focus:border-red-500 focus:ring-red-500 text-base"
                  />
                </div>

                <div>
                  <label for="provincia" class="block text-sm font-medium text-gray-700">
                    Provincia *
                  </label>
                  <select
                    v-model="formData.provincia"
                    id="provincia"
                    required
                    class="mt-1 block w-full px-4 py-3 rounded-md border-gray-300 shadow-sm focus:border-red-500 focus:ring-red-500 text-base"
                  >
                    <option value="">Seleccionar...</option>
                    <option value="Buenos Aires">Buenos Aires</option>
                    <option value="CABA">Ciudad Autónoma de Buenos Aires</option>
                    <option value="Catamarca">Catamarca</option>
                    <option value="Chaco">Chaco</option>
                    <option value="Chubut">Chubut</option>
                    <option value="Córdoba">Córdoba</option>
                    <option value="Corrientes">Corrientes</option>
                    <option value="Entre Ríos">Entre Ríos</option>
                    <option value="Formosa">Formosa</option>
                    <option value="Jujuy">Jujuy</option>
                    <option value="La Pampa">La Pampa</option>
                    <option value="La Rioja">La Rioja</option>
                    <option value="Mendoza">Mendoza</option>
                    <option value="Misiones">Misiones</option>
                    <option value="Neuquén">Neuquén</option>
                    <option value="Río Negro">Río Negro</option>
                    <option value="Salta">Salta</option>
                    <option value="San Juan">San Juan</option>
                    <option value="San Luis">San Luis</option>
                    <option value="Santa Cruz">Santa Cruz</option>
                    <option value="Santa Fe">Santa Fe</option>
                    <option value="Santiago del Estero">Santiago del Estero</option>
                    <option value="Tierra del Fuego">Tierra del Fuego</option>
                    <option value="Tucumán">Tucumán</option>
                  </select>
                </div>

                <div>
                  <label for="codigo_postal" class="block text-sm font-medium text-gray-700">
                    Código Postal
                  </label>
                  <input
                    v-model="formData.codigo_postal"
                    type="text"
                    id="codigo_postal"
                    class="mt-1 block w-full px-4 py-3 rounded-md border-gray-300 shadow-sm focus:border-red-500 focus:ring-red-500 text-base"
                  />
                </div>
              </div>
            </div>

            <!-- Error message -->
            <div v-if="error" class="rounded-md bg-red-50 p-4">
              <div class="flex">
                <div class="flex-shrink-0">
                  <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                    <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
                  </svg>
                </div>
                <div class="ml-3">
                  <h3 class="text-sm font-medium text-red-800">Error</h3>
                  <div class="mt-2 text-sm text-red-700">
                    <p>{{ error }}</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Success message -->
            <div v-if="success" class="rounded-md bg-green-50 p-4">
              <div class="flex">
                <div class="flex-shrink-0">
                  <svg class="h-5 w-5 text-green-400" viewBox="0 0 20 20" fill="currentColor">
                    <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
                  </svg>
                </div>
                <div class="ml-3">
                  <h3 class="text-sm font-medium text-green-800">¡Solicitud enviada!</h3>
                  <div class="mt-2 text-sm text-green-700">
                    <p>Tu solicitud ha sido enviada correctamente. Te notificaremos por email cuando tengamos una respuesta.</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Footer -->
          <div class="mt-6 flex justify-end space-x-3">
            <button
              type="button"
              @click="close"
              class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
            >
              Cancelar
            </button>
            <button
              type="submit"
              :disabled="loading || success"
              class="px-4 py-2 text-sm font-medium text-white bg-red-600 border border-transparent rounded-md hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <span v-if="loading">Enviando...</span>
              <span v-else-if="solicitudExistente?.estado === 'Rechazada'">Enviar Nueva Solicitud</span>
              <span v-else>Enviar Solicitud</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { solicitudReventaService, type SolicitudReventaResponse, type CreateSolicitudReventaRequest } from '@/services/api'

const props = defineProps<{
  isOpen: boolean
}>()

const emit = defineEmits<{
  close: []
}>()

const authStore = useAuthStore()

const loading = ref(false)
const error = ref('')
const success = ref(false)
const solicitudExistente = ref<SolicitudReventaResponse | null>(null)

const formData = ref<CreateSolicitudReventaRequest>({
  razon_social: '',
  cuit: '',
  categoria_iva: '',
  email_comercial: '',
  telefono_comercial: '',
  direccion_comercial: '',
  localidad: '',
  provincia: '',
  codigo_postal: ''
})

const formatDate = (dateString: string | undefined) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('es-AR', { 
    year: 'numeric', 
    month: 'long', 
    day: 'numeric' 
  })
}

const checkExistingSolicitud = async () => {
  if (!authStore.token) return
  
  try {
    const solicitud = await solicitudReventaService.getMiSolicitud(authStore.token)
    solicitudExistente.value = solicitud
  } catch (err) {
    console.error('Error al verificar solicitud existente:', err)
  }
}

const handleSubmit = async () => {
  loading.value = true
  error.value = ''
  success.value = false

  try {
    if (!authStore.token) {
      throw new Error('Debes iniciar sesión para enviar una solicitud')
    }

    await solicitudReventaService.createSolicitud(formData.value, authStore.token)
    success.value = true
    
    // Recargar la solicitud existente
    await checkExistingSolicitud()
    
    // Cerrar el modal después de 3 segundos
    setTimeout(() => {
      close()
    }, 3000)
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al enviar la solicitud'
  } finally {
    loading.value = false
  }
}

const close = () => {
  emit('close')
  // Resetear el formulario si fue exitoso
  if (success.value) {
    formData.value = {
      razon_social: '',
      cuit: '',
      categoria_iva: '',
      email_comercial: '',
      telefono_comercial: '',
      direccion_comercial: '',
      localidad: '',
      provincia: '',
      codigo_postal: ''
    }
    success.value = false
    error.value = ''
  }
}

onMounted(() => {
  checkExistingSolicitud()
})
</script>