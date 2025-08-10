import type {
  CategoryBaseDto,
  CreateCategoryBaseCommand,
  UpdateCategoryBaseCommand,
  GetCategoriesBaseQueryResult,
  CategoryBaseFilters
} from '~/types/categorias'

export const useCategoriesBase = () => {
  const api = useApi()
  const toast = useToast()

  // Estado reactivo
  const categories = ref<CategoryBaseDto[]>([])
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

  // Obtener todas las categorías base
  const fetchCategories = async (filters: CategoryBaseFilters = {}) => {
    loading.value = true
    error.value = null
    
    try {
      const params = new URLSearchParams()
      if (filters.visibleOnly !== undefined) {
        params.append('visibleOnly', filters.visibleOnly.toString())
      }
      if (filters.empresaId) {
        params.append('empresaId', filters.empresaId.toString())
      }

      const url = `/api/categories/base${params.toString() ? `?${params.toString()}` : ''}`
      const response = await api.get<any>(url)
      
      
      // Mapear los datos desde la estructura real de la API
      let rawCategories = []
      if (response.categories) {
        rawCategories = response.categories
      } else if (response.categorias) {
        rawCategories = response.categorias
      } else if (Array.isArray(response)) {
        rawCategories = response
      }
      
      // Normalizar los datos para mantener compatibilidad
      const normalizedCategories = rawCategories.map((cat: any) => ({
        ...cat,
        fecha_creacion: cat.created_at || cat.fecha_creacion,
        fecha_actualizacion: cat.updated_at || cat.fecha_actualizacion,
        cantidad_productos: cat.product_count || cat.cantidad_productos || 0
      }))
      
      categories.value = normalizedCategories
      total.value = response.total || normalizedCategories.length
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar categorías',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Crear una nueva categoría base
  const createCategory = async (data: CreateCategoryBaseCommand) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.post<any>('/api/categories/base', data)
      
      toast.add({
        title: 'Categoría creada',
        description: `La categoría "${data.nombre}" ha sido creada exitosamente`,
        color: 'green'
      })
      
      // Refrescar la lista
      await fetchCategories()
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al crear categoría',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Actualizar una categoría base
  const updateCategory = async (id: number, data: Omit<UpdateCategoryBaseCommand, 'id'>) => {
    loading.value = true
    error.value = null
    
    try {
      const updateData = { ...data, id }
      const response = await api.put<any>(`/api/categories/base/${id}`, updateData)
      
      toast.add({
        title: 'Categoría actualizada',
        description: `La categoría "${data.nombre}" ha sido actualizada exitosamente`,
        color: 'green'
      })
      
      // Actualizar la categoría en la lista local
      const index = categories.value.findIndex(cat => cat.id === id)
      if (index !== -1) {
        categories.value[index] = { ...categories.value[index], ...data }
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al actualizar categoría',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Eliminar una categoría base
  const deleteCategory = async (id: number) => {
    loading.value = true
    error.value = null
    
    try {
      await api.delete(`/api/categories/base/${id}`)
      
      toast.add({
        title: 'Categoría eliminada',
        description: 'La categoría ha sido eliminada exitosamente',
        color: 'green'
      })
      
      // Remover de la lista local
      categories.value = categories.value.filter(cat => cat.id !== id)
      total.value = Math.max(0, total.value - 1)
      
      return true
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al eliminar categoría',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Validar datos de categoría
  const validateCategory = (data: Partial<CreateCategoryBaseCommand | UpdateCategoryBaseCommand>): string[] => {
    const errors: string[] = []
    
    if (!data.nombre?.trim()) {
      errors.push('El nombre es requerido')
    } else if (data.nombre.trim().length < 2) {
      errors.push('El nombre debe tener al menos 2 caracteres')
    }
    
    if (data.orden !== undefined && (data.orden < 0 || data.orden > 9999)) {
      errors.push('El orden debe estar entre 0 y 9999')
    }
    
    return errors
  }

  // Obtener categoría por ID (desde la API)
  const fetchCategoryById = async (id: number): Promise<CategoryBaseDto> => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<any>(`/api/categories/base/${id}`)
      
      // Normalizar los datos
      const category = {
        ...response.categoria || response,
        fecha_creacion: response.categoria?.created_at || response.created_at || response.categoria?.fecha_creacion || response.fecha_creacion,
        fecha_actualizacion: response.categoria?.updated_at || response.updated_at || response.categoria?.fecha_actualizacion || response.fecha_actualizacion,
        cantidad_productos: response.categoria?.product_count || response.product_count || response.categoria?.cantidad_productos || response.cantidad_productos || 0
      }
      
      return category
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar categoría',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Obtener categoría por ID (desde la lista local)
  const getCategoryById = (id: number): CategoryBaseDto | undefined => {
    return categories.value.find(cat => cat.id === id)
  }

  // Formatear categorías para selects
  const getCategoryOptions = () => {
    return categories.value.map(cat => ({
      value: cat.id,
      label: `[${cat.codigo_rubro}] ${cat.nombre}`,
      descripcion: cat.descripcion,
      codigo_rubro: cat.codigo_rubro
    }))
  }

  // Limpiar estado
  const clearState = () => {
    categories.value = []
    error.value = null
    total.value = 0
  }

  return {
    // Estado
    categories: readonly(categories),
    loading: readonly(loading),
    error: readonly(error),
    total: readonly(total),
    
    // Métodos
    fetchCategories,
    fetchCategoryById,
    createCategory,
    updateCategory,
    deleteCategory,
    validateCategory,
    getCategoryById,
    getCategoryOptions,
    clearState
  }
}