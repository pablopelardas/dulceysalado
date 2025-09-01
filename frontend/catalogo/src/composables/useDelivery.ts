import { ref, computed } from 'vue'
import { authApiService } from '@/services/api'

export interface DeliverySlot {
  slot_type: 'Morning' | 'Afternoon'
  slot_type_name: string
  time_range: string
  is_available: boolean
  current_capacity: number
  max_capacity: number
  remaining_capacity: number
}

export interface AvailableDeliverySlot {
  date: string
  morning_slots: DeliverySlot[]
  afternoon_slots: DeliverySlot[]
}

export interface DeliverySlotOption {
  value: string
  label: string
  date: string
  slot_type: 'Morning' | 'Afternoon'
  time_range: string
  available: boolean
  remaining_capacity: number
}

export const useDelivery = () => {
  const slots = ref<AvailableDeliverySlot[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const fetchAvailableSlots = async (token: string, startDate?: string, endDate?: string) => {
    loading.value = true
    error.value = null

    try {
      const response = await authApiService.getDeliverySlots(token, startDate, endDate)
      slots.value = response
    } catch (err) {
      console.error('Error fetching delivery slots:', err)
      error.value = err instanceof Error ? err.message : 'Error cargando franjas de entrega'
      slots.value = []
    } finally {
      loading.value = false
    }
  }

  const slotOptions = computed((): DeliverySlotOption[] => {
    const options: DeliverySlotOption[] = []

    slots.value.forEach(daySlot => {
      // Agregar slots de mañana
      daySlot.morning_slots.forEach(slot => {
        options.push({
          value: `${daySlot.date}_morning`,
          label: `${formatDate(daySlot.date)} - ${slot.time_range}`,
          date: daySlot.date,
          slot_type: 'Morning',
          time_range: slot.time_range,
          available: slot.is_available,
          remaining_capacity: slot.remaining_capacity
        })
      })

      // Agregar slots de tarde
      daySlot.afternoon_slots.forEach(slot => {
        options.push({
          value: `${daySlot.date}_afternoon`,
          label: `${formatDate(daySlot.date)} - ${slot.time_range}`,
          date: daySlot.date,
          slot_type: 'Afternoon',
          time_range: slot.time_range,
          available: slot.is_available,
          remaining_capacity: slot.remaining_capacity
        })
      })
    })

    return options
  })

  const availableSlotOptions = computed(() => {
    return slotOptions.value.filter(option => option.available)
  })

  const formatDate = (dateStr: string): string => {
    const date = new Date(dateStr + 'T00:00:00')
    return date.toLocaleDateString('es-AR', { 
      weekday: 'long', 
      day: 'numeric', 
      month: 'short' 
    })
  }

  const parseSlotValue = (value: string): { date: string; slotType: 'Morning' | 'Afternoon' } | null => {
    const parts = value.split('_')
    if (parts.length !== 2) return null
    
    const [date, slotType] = parts
    return {
      date,
      slotType: slotType === 'morning' ? 'Morning' : 'Afternoon'
    }
  }

  const getSlotInfo = (value: string): DeliverySlotOption | null => {
    return slotOptions.value.find(option => option.value === value) || null
  }

  return {
    slots,
    loading,
    error,
    slotOptions,
    availableSlotOptions,
    fetchAvailableSlots,
    parseSlotValue,
    getSlotInfo,
    formatDate
  }
}