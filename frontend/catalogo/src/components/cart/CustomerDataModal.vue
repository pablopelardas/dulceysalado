<template>
  <Transition name="modal-fade">
    <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <!-- Backdrop -->
      <div 
        class="absolute inset-0 bg-black/60"
        @click="closeModal"
      ></div>
      
      <!-- Modal -->
      <div class="relative bg-white rounded-2xl shadow-2xl max-w-md w-full">
        <!-- Header -->
        <div class="px-6 py-4 border-b border-gray-100 rounded-t-2xl">
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold text-gray-900">
              Datos para el pedido
            </h3>
            <button 
              @click="closeModal"
              class="p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
            >
              <XMarkIcon class="w-5 h-5 text-gray-500" />
            </button>
          </div>
          <p class="text-sm text-gray-500 mt-1">
            Por favor completa los siguientes datos para enviar tu pedido
          </p>
        </div>
        
        <!-- Form -->
        <form @submit.prevent="handleSubmit" class="p-6">
          <div class="space-y-4">
            <!-- Dynamic fields based on requiredFields -->
            <div v-for="field in requiredFields" :key="field">
              <label :for="field" class="block text-sm font-medium text-gray-700 mb-1">
                {{ getFieldLabel(field) }} *
              </label>
              
              <!-- Textarea for longer fields -->
              <textarea
                v-if="isTextareaField(field)"
                v-model="formData[field]"
                :id="field"
                required
                rows="2"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[--theme-primary] focus:border-transparent"
                :placeholder="getFieldPlaceholder(field)"
              />
              
              <!-- Email input -->
              <input
                v-else-if="field === 'email'"
                v-model="formData[field]"
                type="email"
                :id="field"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[--theme-primary] focus:border-transparent"
                :placeholder="getFieldPlaceholder(field)"
              />
              
              <!-- Phone input -->
              <input
                v-else-if="field === 'telefono' || field === 'celular' || field === 'whatsapp'"
                v-model="formData[field]"
                type="tel"
                :id="field"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[--theme-primary] focus:border-transparent"
                :placeholder="getFieldPlaceholder(field)"
              />
              
              <!-- Number input -->
              <input
                v-else-if="field === 'numero_cliente' || field === 'dni' || field === 'cuit'"
                v-model="formData[field]"
                type="text"
                :id="field"
                required
                pattern="[0-9\-]+"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[--theme-primary] focus:border-transparent"
                :placeholder="getFieldPlaceholder(field)"
              />
              
              <!-- Date input -->
              <input
                v-else-if="field === 'fecha_entrega' || field.includes('fecha')"
                v-model="formData[field]"
                type="date"
                :id="field"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[--theme-primary] focus:border-transparent"
              />
              
              <!-- Default text input -->
              <input
                v-else
                v-model="formData[field]"
                type="text"
                :id="field"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[--theme-primary] focus:border-transparent"
                :placeholder="getFieldPlaceholder(field)"
              />
            </div>
          </div>
          
          <!-- Guardar datos para futuros pedidos -->
          <div class="mt-4">
            <label class="flex items-center gap-2 text-sm text-gray-600 cursor-pointer">
              <input
                v-model="saveForLater"
                type="checkbox"
                class="rounded border-gray-300 text-[--theme-primary] focus:ring-[--theme-primary]"
              />
              <span>Guardar estos datos para futuros pedidos</span>
            </label>
          </div>
          
          <!-- Buttons -->
          <div class="flex gap-3 mt-6">
            <button
              type="button"
              @click="closeModal"
              class="flex-1 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors"
            >
              Cancelar
            </button>
            <button
              type="submit"
              class="flex-1 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors flex items-center justify-center gap-2"
            >
              <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893A11.821 11.821 0 0020.885 3.106"/>
              </svg>
              Enviar pedido
            </button>
          </div>
        </form>
        
        <!-- Footer note -->
        <div class="px-6 py-3 bg-gray-50 rounded-b-2xl">
          <p class="text-xs text-gray-500 text-center">
            Los campos marcados con * son obligatorios
          </p>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { XMarkIcon } from '@heroicons/vue/24/outline'

export interface CustomerData {
  [key: string]: string | undefined
}

interface Props {
  isOpen: boolean
  requiredFields: string[]
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
  submit: [data: CustomerData]
}>()

// Form data
const formData = ref<CustomerData>({})
const saveForLater = ref(false)

// Field configuration helpers
const getFieldLabel = (field: string): string => {
  const labels: Record<string, string> = {
    nombre: 'Nombre completo',
    numero_cliente: 'Número de cliente',
    telefono: 'Teléfono',
    direccion_entrega: 'Dirección de entrega',
    direccion: 'Dirección',
    email: 'Email',
    observaciones: 'Observaciones',
    dni: 'DNI',
    cuit: 'CUIT',
    empresa: 'Empresa',
    horario: 'Horario de entrega',
    fecha_entrega: 'Fecha de entrega',
    celular: 'Celular',
    whatsapp: 'WhatsApp',
    localidad: 'Localidad',
    codigo_postal: 'Código postal',
    provincia: 'Provincia'
  }
  
  return labels[field] || field.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase())
}

const getFieldPlaceholder = (field: string): string => {
  const placeholders: Record<string, string> = {
    nombre: 'Juan Pérez',
    numero_cliente: '12345',
    telefono: '+54 9 11 1234-5678',
    direccion_entrega: 'Av. Corrientes 1234, Piso 5, CABA',
    direccion: 'Av. Corrientes 1234',
    email: 'juan@ejemplo.com',
    observaciones: 'Instrucciones especiales...',
    dni: '12345678',
    cuit: '20-12345678-9',
    empresa: 'Mi Empresa S.A.',
    horario: '9:00 a 18:00',
    celular: '+54 9 11 1234-5678',
    whatsapp: '+54 9 11 1234-5678',
    localidad: 'Buenos Aires',
    codigo_postal: '1234',
    provincia: 'Buenos Aires'
  }
  
  return placeholders[field] || 'Ingrese ' + getFieldLabel(field).toLowerCase()
}

const isTextareaField = (field: string): boolean => {
  return field === 'direccion_entrega' || 
         field === 'direccion' || 
         field === 'observaciones' ||
         field === 'comentarios' ||
         field === 'notas'
}

// Load saved data from localStorage
const loadSavedData = () => {
  try {
    const saved = localStorage.getItem('customer-data')
    if (saved) {
      const savedData = JSON.parse(saved)
      formData.value = { ...formData.value, ...savedData }
      saveForLater.value = true
    }
  } catch (error) {
    console.error('Error loading saved customer data:', error)
  }
}

// Save data to localStorage
const saveData = () => {
  if (saveForLater.value) {
    try {
      localStorage.setItem('customer-data', JSON.stringify(formData.value))
    } catch (error) {
      console.error('Error saving customer data:', error)
    }
  } else {
    localStorage.removeItem('customer-data')
  }
}

// Methods
const closeModal = () => {
  emit('close')
}

const handleSubmit = () => {
  // Filter only required fields
  const dataToSubmit: CustomerData = {}
  props.requiredFields.forEach(field => {
    if (formData.value[field as keyof CustomerData]) {
      dataToSubmit[field as keyof CustomerData] = formData.value[field as keyof CustomerData]
    }
  })
  
  saveData()
  emit('submit', dataToSubmit)
}

// Load saved data on mount
onMounted(() => {
  loadSavedData()
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
</style>