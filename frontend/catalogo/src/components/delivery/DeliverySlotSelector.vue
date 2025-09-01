<template>
  <div class="space-y-4">
    <!-- Fecha de entrega -->
    <div>
      <label class="block text-sm font-medium text-gray-700 mb-2">
        Fecha de entrega preferida
        <span v-if="required" class="text-red-500">*</span>
      </label>
      <DatePicker
        v-model="selectedDate"
        :disabled="disabled"
        :min-date="minDate"
        placeholder="Seleccionar fecha de entrega"
        @update:model-value="onDateChange"
      />
    </div>

    <!-- Franja horaria -->
    <div>
      <label for="delivery_slot" class="block text-sm font-medium text-gray-700 mb-2">
        Franja horaria
        <span v-if="required" class="text-red-500">*</span>
      </label>
      
      <!-- Loading state -->
      <div v-if="delivery.loading.value" class="flex items-center justify-center py-8">
        <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-red-500"></div>
        <span class="ml-2 text-sm text-gray-600">Cargando franjas disponibles...</span>
      </div>
      
      <!-- Error state -->
      <div v-else-if="delivery.error.value" class="rounded-md bg-red-50 border border-red-200 p-4">
        <div class="flex">
          <div class="flex-shrink-0">
            <svg class="h-5 w-5 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
          </div>
          <div class="ml-3">
            <p class="text-sm text-red-800">{{ delivery.error.value }}</p>
          </div>
        </div>
      </div>
      
      <!-- No slots available -->
      <div v-else-if="delivery.availableSlotOptions.value.length === 0 && selectedDate" 
           class="rounded-md bg-yellow-50 border border-yellow-200 p-4">
        <div class="flex">
          <div class="flex-shrink-0">
            <svg class="h-5 w-5 text-yellow-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.864-.833-2.633 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"/>
            </svg>
          </div>
          <div class="ml-3">
            <p class="text-sm text-yellow-800">
              No hay franjas disponibles para la fecha seleccionada. Por favor, elige otra fecha.
            </p>
          </div>
        </div>
      </div>
      
      <!-- Slot selector -->
      <div v-else>
        <select
          id="delivery_slot"
          v-model="selectedSlot"
          class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500"
          :disabled="disabled || delivery.loading.value"
          @change="onSlotChange"
        >
          <option value="">{{ selectedDate ? 'Seleccionar franja horaria' : 'Primero selecciona una fecha' }}</option>
          <option 
            v-for="option in delivery.availableSlotOptions.value" 
            :key="option.value"
            :value="option.value"
            :disabled="!option.available"
          >
            {{ option.label }}
            <span v-if="option.remaining_capacity <= 3" class="text-orange-600">
              ({{ option.remaining_capacity }} lugares disponibles)
            </span>
          </option>
        </select>
        
        <!-- Slot info -->
        <div v-if="selectedSlotInfo" class="mt-2 text-sm text-gray-600">
          <div class="flex items-center gap-2">
            <svg class="h-4 w-4 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
            <span>Franja disponible - {{ selectedSlotInfo.remaining_capacity }} lugares restantes</span>
          </div>
        </div>
      </div>

      <!-- Help text -->
      <p v-if="!required" class="mt-2 text-xs text-gray-500">
        Opcional: Selecciona una fecha y franja horaria preferida para tu entrega.
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useDelivery, type DeliverySlotOption } from '@/composables/useDelivery'
import { useAuthStore } from '@/stores/auth'
import DatePicker from '@/components/ui/DatePicker.vue'

interface Props {
  modelValue?: {
    fecha_entrega: string
    horario_entrega: string
    delivery_slot?: string
  }
  disabled?: boolean
  required?: boolean
}

interface EmitEvents {
  'update:modelValue': [value: {
    fecha_entrega: string
    horario_entrega: string
    delivery_slot?: string
  }]
}

const props = withDefaults(defineProps<Props>(), {
  disabled: false,
  required: false
})

const emit = defineEmits<EmitEvents>()

// Stores and composables
const authStore = useAuthStore()
const delivery = useDelivery()

// Local state
const selectedDate = ref(props.modelValue?.fecha_entrega || '')
const selectedSlot = ref(props.modelValue?.delivery_slot || '')

// Computed
const minDate = computed(() => {
  const tomorrow = new Date()
  tomorrow.setDate(tomorrow.getDate() + 1)
  return tomorrow.toISOString().split('T')[0]
})

const selectedSlotInfo = computed((): DeliverySlotOption | null => {
  if (!selectedSlot.value) return null
  return delivery.getSlotInfo(selectedSlot.value)
})

// Methods
const loadDeliverySlots = async () => {
  if (!authStore.token || !selectedDate.value) {
    return
  }

  // Cargar slots para la semana que incluye la fecha seleccionada
  const startDate = selectedDate.value
  const endDate = new Date(selectedDate.value)
  endDate.setDate(endDate.getDate() + 7)
  
  await delivery.fetchAvailableSlots(
    authStore.token,
    startDate,
    endDate.toISOString().split('T')[0]
  )
}

const onDateChange = (newDate: string) => {
  selectedDate.value = newDate
  selectedSlot.value = '' // Reset slot selection when date changes
  emitValue()
  
  if (selectedDate.value && authStore.token) {
    loadDeliverySlots()
  }
}

const onSlotChange = () => {
  emitValue()
}

const emitValue = () => {
  const slotInfo = selectedSlotInfo.value
  
  emit('update:modelValue', {
    fecha_entrega: selectedDate.value,
    horario_entrega: slotInfo ? `${slotInfo.slot_type === 'Morning' ? 'Mañana' : 'Tarde'} (${slotInfo.time_range})` : '',
    delivery_slot: selectedSlot.value
  })
}

// Watchers
watch(() => props.modelValue, (newValue) => {
  if (newValue) {
    selectedDate.value = newValue.fecha_entrega || ''
    selectedSlot.value = newValue.delivery_slot || ''
  }
}, { immediate: true })

watch(() => authStore.token, (token) => {
  if (token && selectedDate.value) {
    loadDeliverySlots()
  }
})

// Initialize
onMounted(() => {
  if (authStore.token && selectedDate.value) {
    loadDeliverySlots()
  }
})
</script>