import type { NovedadesResponse, SetNovedadesRequest, NovedadesApiResponse, MarketingState, Agrupacion } from '~/types/marketing'
import { parseApiError } from '~/utils/errorParser'

export const useNovedades = () => {
  const { get, put } = useApi()
  
  // Estado reactivo
  const state = reactive<MarketingState>({
    isLoading: false,
    error: null,
    agrupaciones: [],
    selectedIds: []
  })
  
  // Estado adicional para todas las agrupaciones disponibles
  const todasLasAgrupaciones = ref<Agrupacion[]>([])
  const toast = useToast()

  // Obtener agrupaciones con flags de novedad por empresa
  const fetchNovedades = async (empresaId: number) => {
    state.isLoading = true
    state.error = null

    try {
      const response = await get<NovedadesResponse>(`/api/empresas/${empresaId}/novedades`)

      if (response) {
        state.agrupaciones = response.agrupaciones || []
        state.selectedIds = state.agrupaciones
          .filter(agrupacion => agrupacion.is_novedad)
          .map(agrupacion => agrupacion.id)
      }
    } catch (error: any) {
      state.error = error.message || 'Error al cargar novedades'
      console.error('Error fetching novedades:', error)
    } finally {
      state.isLoading = false
    }
  }

  // Actualizar agrupaciones seleccionadas como novedades
  const updateNovedades = async (empresaId: number, agrupacionIds: number[]) => {
    state.isLoading = true
    state.error = null

    try {
      const payload: SetNovedadesRequest = {
        agrupacion_ids: agrupacionIds
      }

      const response = await put<NovedadesApiResponse>(`/api/empresas/${empresaId}/novedades`, payload)

      if (response?.success) {
        state.selectedIds = agrupacionIds
        // Actualizar el estado local de las agrupaciones
        state.agrupaciones = state.agrupaciones.map(agrupacion => ({
          ...agrupacion,
          es_novedad: agrupacionIds.includes(agrupacion.id)
        }))
      } else {
        throw new Error(response?.message || 'Error al actualizar novedades')
      }
    } catch (error: any) {
      state.error = error.message || 'Error al guardar novedades'
      console.error('Error updating novedades:', error)
      throw error
    } finally {
      state.isLoading = false
    }
  }

  // Obtener agrupaciones disponibles (para el drag-drop)
  const availableAgrupaciones = computed(() => 
    state.agrupaciones.filter(agrupacion => !state.selectedIds.includes(agrupacion.id))
  )

  // Obtener agrupaciones seleccionadas (para el drag-drop)
  const selectedAgrupaciones = computed(() => 
    state.agrupaciones.filter(agrupacion => state.selectedIds.includes(agrupacion.id))
  )

  // Obtener todas las agrupaciones disponibles para novedades (type=1)
  const fetchTodasLasAgrupaciones = async () => {
    state.isLoading = true
    state.error = null
    
    try {
      const response = await get<{agrupaciones: Agrupacion[]}>(`/api/agrupaciones?type=1`)

      if (response.agrupaciones) {
        todasLasAgrupaciones.value = response.agrupaciones
      } else if (Array.isArray(response)) {
        todasLasAgrupaciones.value = response as Agrupacion[]
      } else {
        todasLasAgrupaciones.value = []
      }
      
      return todasLasAgrupaciones.value
    } catch (err: any) {
      const errorMessage = parseApiError(err).message
      state.error = errorMessage
      toast.add({
        title: 'Error al cargar agrupaciones disponibles',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      state.isLoading = false
    }
  }

  // Limpiar estado
  const clearState = () => {
    state.agrupaciones = []
    state.selectedIds = []
    state.error = null
    state.isLoading = false
    todasLasAgrupaciones.value = []
  }

  return {
    // Estado reactivo readonly
    isLoading: readonly(toRef(state, 'isLoading')),
    error: readonly(toRef(state, 'error')),
    agrupaciones: readonly(toRef(state, 'agrupaciones')),
    selectedIds: readonly(toRef(state, 'selectedIds')),
    todasLasAgrupaciones: readonly(todasLasAgrupaciones),

    // Computadas
    availableAgrupaciones,
    selectedAgrupaciones,

    // Acciones
    fetchNovedades,
    fetchTodasLasAgrupaciones,
    updateNovedades,
    clearState
  }
}