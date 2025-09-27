
export interface SolicitudReventaDto {
  id: number
  cliente_id: number
  cliente_nombre?: string
  cliente_email?: string
  cuit: string
  razon_social: string
  direccion_comercial: string
  localidad: string
  provincia: string
  codigo_postal: string
  telefono_comercial: string
  categoria_iva: string
  email_comercial: string
  estado: 'Pendiente' | 'Aprobada' | 'Rechazada'
  comentario_respuesta?: string
  fecha_respuesta?: string
  respondido_por?: string
  fecha_solicitud: string
}

export interface ResponderSolicitudDto {
  aprobar: boolean
  comentarioRespuesta?: string
}

export interface SolicitudesFilters {
  search?: string
  estado?: string
  page?: number
  limit?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export interface SolicitudesPagination {
  page: number
  limit: number
  total: number
  totalPages: number
}

export const useSolicitudesReventa = () => {
  const api = useApi()
  const toast = useToast()

  // Helper para extraer mensaje de error de la API
  const getErrorMessage = (err: any): string => {
    if (err?.message) {
      return err.message
    }
    
    if (err?.data?.message) {
      return err.data.message
    }
    
    if (err?.data?.errors) {
      const errors = err.data.errors
      const firstError = Object.values(errors)[0] as string[]
      return firstError[0] || 'Error de validación'
    }
    
    return err?.statusText || err?.message || 'Error desconocido'
  }

  // Estado reactivo
  const solicitudes = ref<SolicitudReventaDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const pagination = ref<SolicitudesPagination>({
    page: 1,
    limit: 20,
    total: 0,
    totalPages: 0
  })

  const filters = ref<SolicitudesFilters>({
    search: '',
    estado: '',
    page: 1,
    limit: 20,
    sortBy: 'fechaSolicitud',
    sortOrder: 'desc'
  })

  // Estadísticas
  const estadisticas = computed(() => {
    const pendientes = solicitudes.value.filter(s => s.estado === 'Pendiente').length
    const aprobadas = solicitudes.value.filter(s => s.estado === 'Aprobada').length
    const rechazadas = solicitudes.value.filter(s => s.estado === 'Rechazada').length
    
    return {
      total: solicitudes.value.length,
      pendientes,
      aprobadas,
      rechazadas
    }
  })

  // Obtener todas las solicitudes con filtros
  const fetchSolicitudes = async (params: SolicitudesFilters = {}) => {
    loading.value = true
    error.value = null

    try {
      const mergedParams = { ...filters.value, ...params }
      
      const queryParams = new URLSearchParams()
      if (mergedParams.search) queryParams.append('search', mergedParams.search)
      if (mergedParams.estado) queryParams.append('estado', mergedParams.estado)
      if (mergedParams.page) queryParams.append('page', mergedParams.page.toString())
      if (mergedParams.limit) queryParams.append('limit', mergedParams.limit.toString())
      if (mergedParams.sortBy) queryParams.append('sortBy', mergedParams.sortBy)
      if (mergedParams.sortOrder) queryParams.append('sortOrder', mergedParams.sortOrder)

      const queryString = queryParams.toString()
      const url = queryString ? `/api/backoffice/solicitudes-reventa?${queryString}` : '/api/backoffice/solicitudes-reventa'

      const response = await api.get<SolicitudReventaDto[]>(url)
      
      if (Array.isArray(response)) {
        solicitudes.value = response
        
        // Si la respuesta no incluye paginación, calcularla localmente
        const total = response.length
        const currentPage = mergedParams.page || 1
        const currentLimit = mergedParams.limit || 20
        
        pagination.value = {
          page: currentPage,
          limit: currentLimit,
          total,
          totalPages: Math.ceil(total / currentLimit)
        }
      } else {
        // Si la respuesta incluye paginación
        solicitudes.value = (response as any).data || []
        pagination.value = (response as any).pagination || pagination.value
      }
      
      // Actualizar filtros aplicados
      filters.value = mergedParams
      
    } catch (err: any) {
      error.value = getErrorMessage(err)
      console.error('Error fetching solicitudes:', err)
    } finally {
      loading.value = false
    }
  }

  // Responder a una solicitud (aprobar/rechazar)
  const responderSolicitud = async (solicitudId: number, respuesta: ResponderSolicitudDto): Promise<SolicitudReventaDto> => {
    loading.value = true
    error.value = null

    try {
      const response = await api.post<SolicitudReventaDto>(`/api/backoffice/solicitudes-reventa/${solicitudId}/responder`, respuesta)

      // Actualizar la solicitud en la lista local
      const index = solicitudes.value.findIndex(s => s.id === solicitudId)
      if (index !== -1) {
        solicitudes.value[index] = response
      }

      return response
    } catch (err: any) {
      error.value = getErrorMessage(err)
      console.error('Error responder solicitud:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  // Aplicar filtros
  const applyFilters = async (newFilters: Partial<SolicitudesFilters> = {}) => {
    await fetchSolicitudes({ ...filters.value, ...newFilters, page: 1 })
  }

  // Limpiar filtros
  const clearFilters = async () => {
    const defaultFilters: SolicitudesFilters = {
      search: '',
      estado: '',
      page: 1,
      limit: 20,
      sortBy: 'fechaSolicitud',
      sortOrder: 'desc'
    }

    filters.value = defaultFilters
    await fetchSolicitudes(defaultFilters)
  }

  // Cambiar página
  const changePage = async (page: number) => {
    await fetchSolicitudes({ ...filters.value, page })
  }

  // Aplicar ordenamiento
  const applySorting = async (sortBy: string, sortOrder: 'asc' | 'desc') => {
    await fetchSolicitudes({ ...filters.value, sortBy, sortOrder, page: 1 })
  }

  // Filtrar solo solicitudes pendientes
  const fetchSoloPendientes = async () => {
    await fetchSolicitudes({ ...filters.value, estado: 'Pendiente', page: 1 })
  }

  // Buscar por cliente
  const buscarPorCliente = async (searchTerm: string) => {
    await fetchSolicitudes({ ...filters.value, search: searchTerm, page: 1 })
  }

  return {
    // Estado
    solicitudes: readonly(solicitudes),
    loading: readonly(loading),
    error: readonly(error),
    pagination: readonly(pagination),
    filters: readonly(filters),
    estadisticas,

    // Métodos
    fetchSolicitudes,
    responderSolicitud,
    applyFilters,
    clearFilters,
    changePage,
    applySorting,
    fetchSoloPendientes,
    buscarPorCliente
  }
}