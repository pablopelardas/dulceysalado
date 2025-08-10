import type {
  ListaPrecioDto,
  CreateListaPrecioCommand,
  UpdateListaPrecioCommand,
  GetListasPreciosQueryResult,
  GetListaPrecioByIdQueryResult,
  ListaPrecioFilters
} from '~/types/listas-precios'

export const useListasPreciosCrud = () => {
  const api = useApi()
  const toast = useToast()

  // Estado reactivo
  const listas = ref<ListaPrecioDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const total = ref(0)

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

  // Obtener todas las listas de precios
  const fetchListas = async (filters: ListaPrecioFilters = {}) => {
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

      const url = `/api/listas-precios${params.toString() ? `?${params.toString()}` : ''}`
      const response = await api.get<any>(url)
      
      // Manejar diferentes estructuras de respuesta
      if (response.listas) {
        listas.value = response.listas
        total.value = response.total || response.listas.length
      } else if (Array.isArray(response)) {
        listas.value = response
        total.value = response.length
      } else {
        listas.value = []
        total.value = 0
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar listas de precios',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Obtener lista por ID
  const fetchListaById = async (id: number) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<any>(`/api/listas-precios/${id}`)
      
      // Manejar diferentes estructuras de respuesta
      if (response.lista) {
        return response.lista
      } else if (response.id) {
        // La respuesta es directamente la lista
        return response
      }
      
      throw new Error('No se encontró la lista de precios')
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar lista de precios',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Crear una nueva lista de precios
  const createLista = async (data: CreateListaPrecioCommand) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.post<any>('/api/listas-precios', data)
      
      toast.add({
        title: 'Lista de precios creada',
        description: `La lista "${data.nombre}" ha sido creada exitosamente`,
        color: 'green'
      })
      
      // Refrescar la lista
      await fetchListas()
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al crear lista de precios',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Actualizar una lista de precios
  const updateLista = async (id: number, data: Omit<UpdateListaPrecioCommand, 'id'>) => {
    loading.value = true
    error.value = null
    
    try {
      const updateData = { ...data, id }
      const response = await api.put<any>(`/api/listas-precios/${id}`, updateData)
      
      toast.add({
        title: 'Lista de precios actualizada',
        description: `La lista "${data.nombre}" ha sido actualizada exitosamente`,
        color: 'green'
      })
      
      // Actualizar la lista en el estado local
      const index = listas.value.findIndex(lista => lista.id === id)
      if (index !== -1) {
        listas.value[index] = { ...listas.value[index], ...data }
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al actualizar lista de precios',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Eliminar una lista de precios
  const deleteLista = async (id: number) => {
    loading.value = true
    error.value = null
    
    try {
      const authStore = useAuthStore()
      
      // Obtener el empresa_id del usuario actual
      const empresaId = authStore.empresa?.id
      
      // Intentar primero sin body
      try {
        await api.delete(`/api/listas-precios/${id}`)
      } catch (firstError: any) {
        console.log('Primer intento falló:', firstError)
        console.log('Intentando con body que incluye empresa_id...')
        
        // Si falla, intentar con un body que incluya el ID y empresa_id
        try {
          await api.delete(`/api/listas-precios/${id}`, { 
            id,
            empresa_id: empresaId,
            requesting_user_id: authStore.user?.id || 0
          })
        } catch (secondError: any) {
          console.log('Segundo intento también falló')
          throw secondError
        }
      }
      
      toast.add({
        title: 'Lista de precios eliminada',
        description: 'La lista ha sido eliminada exitosamente',
        color: 'green'
      })
      
      // Remover de la lista local
      listas.value = listas.value.filter(lista => lista.id !== id)
      total.value = Math.max(0, total.value - 1)
      
      return true
    } catch (err: any) {
      console.error('Error completo al eliminar:', err)
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al eliminar lista de precios',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Validar datos de lista
  const validateLista = (data: Partial<CreateListaPrecioCommand | UpdateListaPrecioCommand>): string[] => {
    const errors: string[] = []
    
    if (!data.codigo?.trim()) {
      errors.push('El código es requerido')
    } else if (data.codigo.trim().length < 2) {
      errors.push('El código debe tener al menos 2 caracteres')
    }
    
    if (!data.nombre?.trim()) {
      errors.push('El nombre es requerido')
    } else if (data.nombre.trim().length < 3) {
      errors.push('El nombre debe tener al menos 3 caracteres')
    }
    
    if (data.orden !== undefined && (data.orden < 0 || data.orden > 9999)) {
      errors.push('El orden debe estar entre 0 y 9999')
    }
    
    return errors
  }

  // Obtener lista por ID (de la lista local)
  const getListaById = (id: number): ListaPrecioDto | undefined => {
    return listas.value.find(lista => lista.id === id)
  }

  // Formatear listas para selects
  const getListaOptions = () => {
    return listas.value.map(lista => ({
      value: lista.id,
      label: `${lista.codigo} - ${lista.nombre}`,
      descripcion: lista.descripcion,
      es_predeterminada: lista.es_predeterminada
    }))
  }

  // Obtener lista predeterminada
  const getListaPredeterminada = () => {
    return listas.value.find(lista => lista.es_predeterminada)
  }

  // Limpiar estado
  const clearState = () => {
    listas.value = []
    error.value = null
    total.value = 0
  }

  return {
    // Estado
    listas: readonly(listas),
    loading: readonly(loading),
    error: readonly(error),
    total: readonly(total),
    
    // Métodos
    fetchListas,
    fetchListaById,
    createLista,
    updateLista,
    deleteLista,
    validateLista,
    getListaById,
    getListaOptions,
    getListaPredeterminada,
    clearState
  }
}