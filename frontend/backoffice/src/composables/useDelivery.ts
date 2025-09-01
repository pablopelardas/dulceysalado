import type { 
  DeliverySettings,
  CreateDeliverySettingsRequest,
  UpdateDeliverySettingsRequest,
  AvailableDeliverySlot,
  DeliverySchedule,
  CreateDeliveryScheduleRequest,
  UpdateDeliveryScheduleRequest
} from '~/types/delivery'

export const useDelivery = () => {
  const { get, post, put, delete: deleteRequest } = useApi()
  const toast = useToast()

  // Estados reactivos
  const deliverySettings = ref<DeliverySettings | null>(null)
  const availableSlots = ref<AvailableDeliverySlot[]>([])
  const deliverySchedules = ref<DeliverySchedule[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Helper para extraer mensaje de error
  const getErrorMessage = (err: any): string => {
    if (err?.message) return err.message
    if (err?.data?.message) return err.data.message
    if (err?.data?.errors) {
      const errors = err.data.errors
      const firstError = Object.values(errors)[0] as string[]
      return firstError[0] || 'Error de validación'
    }
    return err?.statusText || err?.message || 'Error desconocido'
  }

  // Obtener configuración de delivery para una empresa
  const fetchDeliverySettings = async (empresaId: number) => {
    console.log('useDelivery.fetchDeliverySettings - empresaId:', empresaId)
    loading.value = true
    error.value = null

    try {
      console.log('Haciendo GET a:', `/api/backoffice/delivery/settings/${empresaId}`)
      const response = await get(`/api/backoffice/delivery/settings/${empresaId}`)
      console.log('Respuesta completa del GET:', response)
      
      // $fetch devuelve directamente los datos, no un objeto con .data
      deliverySettings.value = response
      console.log('deliverySettings.value después de asignar:', deliverySettings.value)
      return response
    } catch (err: any) {
      console.log('Error en fetchDeliverySettings:', err)
      if (err.status === 404) {
        console.log('Error 404 - no existe configuración')
        deliverySettings.value = null
        return null
      }
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      console.log('Error message:', errorMessage)
      toast.add({
        title: 'Error',
        description: `Error al cargar configuración de delivery: ${errorMessage}`,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Crear configuración de delivery
  const createDeliverySettings = async (data: CreateDeliverySettingsRequest) => {
    loading.value = true
    error.value = null

    try {
      const response = await post('/api/backoffice/delivery/settings', data)
      deliverySettings.value = response
      toast.add({
        title: 'Éxito',
        description: 'Configuración de delivery creada exitosamente',
        color: 'green'
      })
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error',
        description: `Error al crear configuración: ${errorMessage}`,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Actualizar configuración de delivery
  const updateDeliverySettings = async (id: number, data: UpdateDeliverySettingsRequest) => {
    loading.value = true
    error.value = null

    try {
      const response = await put(`/api/backoffice/delivery/settings/${id}`, data)
      deliverySettings.value = response
      toast.add({
        title: 'Éxito',
        description: 'Configuración de delivery actualizada exitosamente',
        color: 'green'
      })
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error',
        description: `Error al actualizar configuración: ${errorMessage}`,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Obtener slots disponibles (vista administrativa)
  const fetchAdminSlots = async (empresaId: number, params: {
    startDate?: string
    endDate?: string
    slotType?: number
    onlyAvailable?: boolean
  } = {}) => {
    loading.value = true
    error.value = null

    try {
      const searchParams = new URLSearchParams()
      if (params.startDate) searchParams.append('startDate', params.startDate)
      if (params.endDate) searchParams.append('endDate', params.endDate)
      if (params.slotType !== undefined) searchParams.append('slotType', params.slotType.toString())
      if (params.onlyAvailable !== undefined) searchParams.append('onlyAvailable', params.onlyAvailable.toString())

      const response = await get(`/api/backoffice/delivery/slots/${empresaId}/admin?${searchParams.toString()}`)
      availableSlots.value = response
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error',
        description: `Error al cargar slots: ${errorMessage}`,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Obtener schedules para una configuración de delivery
  const fetchDeliverySchedules = async (deliverySettingsId: number, futureOnly: boolean = true) => {
    loading.value = true
    error.value = null

    try {
      const params = futureOnly ? '?futureOnly=true' : ''
      const response = await get(`/api/backoffice/delivery/schedules/${deliverySettingsId}${params}`)
      deliverySchedules.value = response
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error',
        description: `Error al cargar fechas especiales: ${errorMessage}`,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Crear horario específico para una fecha
  const createDeliverySchedule = async (data: CreateDeliveryScheduleRequest) => {
    loading.value = true
    error.value = null

    try {
      const response = await post('/api/backoffice/delivery/schedules', data)
      
      // Actualizar la lista local
      deliverySchedules.value.push(response)
      
      toast.add({
        title: 'Éxito',
        description: 'Fecha especial creada exitosamente',
        color: 'green'
      })
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error',
        description: `Error al crear fecha especial: ${errorMessage}`,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Actualizar horario específico
  const updateDeliverySchedule = async (id: number, data: UpdateDeliveryScheduleRequest) => {
    loading.value = true
    error.value = null

    try {
      const response = await put(`/api/backoffice/delivery/schedules/${id}`, data)
      
      // Actualizar la lista local
      const index = deliverySchedules.value.findIndex(s => s.id === id)
      if (index !== -1) {
        deliverySchedules.value[index] = response
      }
      
      toast.add({
        title: 'Éxito',
        description: 'Fecha especial actualizada exitosamente',
        color: 'green'
      })
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error',
        description: `Error al actualizar fecha especial: ${errorMessage}`,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Eliminar horario específico
  const deleteDeliverySchedule = async (id: number) => {
    loading.value = true
    error.value = null

    try {
      await deleteRequest(`/api/backoffice/delivery/schedules/${id}`)
      
      // Actualizar la lista local
      const index = deliverySchedules.value.findIndex(s => s.id === id)
      if (index !== -1) {
        deliverySchedules.value.splice(index, 1)
      }
      
      toast.add({
        title: 'Éxito',
        description: 'Fecha especial eliminada exitosamente',
        color: 'green'
      })
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error',
        description: `Error al eliminar fecha especial: ${errorMessage}`,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // Estados
    deliverySettings: readonly(deliverySettings),
    availableSlots: readonly(availableSlots),
    deliverySchedules: readonly(deliverySchedules),
    loading: readonly(loading),
    error: readonly(error),

    // Métodos
    fetchDeliverySettings,
    createDeliverySettings,
    updateDeliverySettings,
    fetchAdminSlots,
    fetchDeliverySchedules,
    createDeliverySchedule,
    updateDeliverySchedule,
    deleteDeliverySchedule
  }
}