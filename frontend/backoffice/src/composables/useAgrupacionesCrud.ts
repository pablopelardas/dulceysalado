import type {
  AgrupacionDto,
  UpdateAgrupacionCommand,
  GetAgrupacionesQueryResult,
  GetAgrupacionByIdQueryResult,
  AgrupacionFilters,
  AgrupacionesStats,
  AgrupacionSelectOption
} from '~/types/agrupaciones'
import { parseApiError } from '~/utils/errorParser'

export const useAgrupacionesCrud = () => {
  const api = useApi()
  const toast = useToast()

  // Estado reactivo
  const agrupaciones = ref<AgrupacionDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const total = ref(0)

  // Helper para extraer mensaje de error (usando el nuevo parser)
  const getErrorMessage = (err: any): string => {
    const parsed = parseApiError(err)
    return parsed.message
  }

  // Obtener todas las agrupaciones
  const fetchAgrupaciones = async (filters: AgrupacionFilters = {}) => {
    loading.value = true
    error.value = null
    
    try {
      const params = new URLSearchParams()
      
      if (filters.activa !== undefined) {
        params.append('activa', filters.activa.toString())
      }
      if (filters.busqueda) {
        params.append('busqueda', filters.busqueda)
      }
      if (filters.ordenarPor) {
        params.append('ordenarPor', filters.ordenarPor)
      }
      if (filters.ordenDireccion) {
        params.append('ordenDireccion', filters.ordenDireccion)
      }
      if (filters.page) {
        params.append('page', filters.page.toString())
      }
      if (filters.pageSize) {
        params.append('pageSize', filters.pageSize.toString())
      }

      const url = `/api/agrupaciones${params.toString() ? `?${params.toString()}` : ''}`
      const response = await api.get<GetAgrupacionesQueryResult>(url)
      
      // Manejar diferentes estructuras de respuesta
      if (response.agrupaciones) {
        agrupaciones.value = response.agrupaciones
        total.value = response.pagination?.total || response.agrupaciones.length
      } else if (Array.isArray(response)) {
        // Respuesta directa como array
        agrupaciones.value = response as any
        total.value = response.length
      } else {
        agrupaciones.value = []
        total.value = 0
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar agrupaciones',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Obtener agrupación por ID
  const fetchAgrupacionById = async (id: number) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<GetAgrupacionByIdQueryResult>(`/api/agrupaciones/${id}`)
      
      // Manejar diferentes estructuras de respuesta
      if (response.agrupacion) {
        return response.agrupacion
      } else if (response.id) {
        // La respuesta es directamente la agrupación
        return response as any as AgrupacionDto
      }
      
      throw new Error('No se encontró la agrupación')
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar agrupación',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Actualizar una agrupación (solo campos editables)
  const updateAgrupacion = async (id: number, data: Omit<UpdateAgrupacionCommand, 'id'>) => {
    loading.value = true
    error.value = null
    
    try {
      const updateData = { ...data, id }
      const response = await api.put<any>(`/api/agrupaciones/${id}`, updateData)
      
      toast.add({
        title: 'Agrupación actualizada',
        description: `La agrupación "${data.nombre}" ha sido actualizada exitosamente`,
        color: 'green'
      })
      
      // Actualizar la agrupación en el estado local
      const index = agrupaciones.value.findIndex(agrupacion => agrupacion.id === id)
      if (index !== -1) {
        agrupaciones.value[index] = { ...agrupaciones.value[index], ...data }
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al actualizar agrupación',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Obtener estadísticas de agrupaciones
  const fetchStats = async (): Promise<AgrupacionesStats | null> => {
    try {
      const response = await api.get<{ success: boolean; estadisticas: AgrupacionesStats }>('/api/agrupaciones/stats')
      return response.estadisticas
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      console.error('Error al cargar estadísticas:', errorMessage)
      return null
    }
  }

  // Validar datos de agrupación
  const validateAgrupacion = (data: Partial<UpdateAgrupacionCommand>): string[] => {
    const errors: string[] = []
    
    if (!data.nombre?.trim()) {
      errors.push('El nombre es requerido')
    } else if (data.nombre.trim().length < 3) {
      errors.push('El nombre debe tener al menos 3 caracteres')
    } else if (data.nombre.trim().length > 100) {
      errors.push('El nombre no puede exceder 100 caracteres')
    }
    
    if (data.descripcion && data.descripcion.trim().length > 500) {
      errors.push('La descripción no puede exceder 500 caracteres')
    }
    
    return errors
  }

  // Obtener agrupación por ID (de la lista local)
  const getAgrupacionById = (id: number): AgrupacionDto | undefined => {
    return agrupaciones.value.find(agrupacion => agrupacion.id === id)
  }

  // Formatear agrupaciones para selects
  const getAgrupacionOptions = (): AgrupacionSelectOption[] => {
    return agrupaciones.value
      .filter(agrupacion => agrupacion.activa) // Solo las activas
      .map(agrupacion => ({
        value: agrupacion.id,
        label: `[${agrupacion.codigo}] ${agrupacion.nombre}`,
        descripcion: agrupacion.descripcion,
        activa: agrupacion.activa,
        codigo: agrupacion.codigo
      }))
      .sort((a, b) => a.codigo - b.codigo) // Ordenar por código
  }

  // Obtener todas las agrupaciones para drag & drop
  const getAllAgrupaciones = () => {
    return agrupaciones.value.map(agrupacion => ({
      id: agrupacion.id,
      codigo: agrupacion.codigo,
      nombre: agrupacion.nombre,
      descripcion: agrupacion.descripcion,
      activa: agrupacion.activa
    }))
  }

  // Buscar agrupaciones (local)
  const searchAgrupaciones = (query: string) => {
    if (!query.trim()) return agrupaciones.value
    
    const searchTerm = query.toLowerCase()
    return agrupaciones.value.filter(agrupacion =>
      agrupacion.nombre.toLowerCase().includes(searchTerm) ||
      agrupacion.codigo.toString().includes(searchTerm) ||
      (agrupacion.descripcion && agrupacion.descripcion.toLowerCase().includes(searchTerm))
    )
  }

  // Filtrar por estado
  const filterByStatus = (activa?: boolean) => {
    if (activa === undefined) return agrupaciones.value
    return agrupaciones.value.filter(agrupacion => agrupacion.activa === activa)
  }

  // Limpiar estado
  const clearState = () => {
    agrupaciones.value = []
    error.value = null
    total.value = 0
  }

  // Refrescar datos
  const refresh = async (filters?: AgrupacionFilters) => {
    await fetchAgrupaciones(filters)
  }

  return {
    // Estado
    agrupaciones: readonly(agrupaciones),
    loading: readonly(loading),
    error: readonly(error),
    total: readonly(total),
    
    // Métodos principales
    fetchAgrupaciones,
    fetchAgrupacionById,
    updateAgrupacion,
    fetchStats,
    
    // Métodos auxiliares
    validateAgrupacion,
    getAgrupacionById,
    getAgrupacionOptions,
    getAllAgrupaciones,
    searchAgrupaciones,
    filterByStatus,
    clearState,
    refresh
  }
}