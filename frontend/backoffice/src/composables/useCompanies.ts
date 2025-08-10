import type {
  CompaniesListResponse,
  CompaniesFilters,
  CreateCompanyRequest,
  UpdateCompanyRequest,
  Empresa
} from '~/types/auth'

export const useCompanies = () => {
  const api = useApi()
  const auth = useAuth()
  const toast = useToast()
  
  // Helper para extraer mensaje de error de la API
  const getErrorMessage = (err: any): string => {
    // Si el error tiene un mensaje directo de la API
    if (err?.message) {
      return err.message
    }
    
    // Si es un error de respuesta HTTP con datos
    if (err?.data?.message) {
      return err.data.message
    }
    
    // Si es un error de validación con múltiples errores
    if (err?.data?.errors) {
      const errors = err.data.errors
      const firstError = Object.values(errors)[0] as string[]
      return firstError[0] || 'Error de validación'
    }
    
    // Fallback al mensaje genérico
    return err?.statusText || err?.message || 'Error desconocido'
  }
  
  // Estado reactivo para lista de empresas
  const companies = ref<Empresa[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  // Estado de paginación
  const pagination = ref({
    page: 1,
    limit: 20,
    total: 0,
    pages: 0
  })
  
  // Filtros activos
  const filters = ref<CompaniesFilters>({
    search: '',
    page: 1,
    pageSize: 20
  })
  
  // Obtener lista de empresas cliente
  const fetchCompanies = async (customFilters?: CompaniesFilters) => {
    loading.value = true
    error.value = null
    
    try {
      // Combinar filtros
      const queryFilters = {
        ...filters.value,
        ...customFilters
      }
      
      // Limpiar filtros vacíos
      Object.keys(queryFilters).forEach(key => {
        if (queryFilters[key as keyof CompaniesFilters] === '' || 
            queryFilters[key as keyof CompaniesFilters] === undefined) {
          delete queryFilters[key as keyof CompaniesFilters]
        }
      })
      
      const response = await api.get<CompaniesListResponse>('/api/Companies', {
        query: queryFilters
      })
      
      companies.value = response.empresas || []
      pagination.value = response.pagination || {
        page: 1,
        limit: 20,
        total: 0,
        pages: 0
      }
      
      // Actualizar filtros activos
      if (customFilters) {
        filters.value = { ...filters.value, ...customFilters }
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar empresas',
        description: errorMessage,
        color: 'error'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Obtener una empresa específica
  const fetchCompany = async (id: number) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<Empresa>(`/api/Companies/${id}`)
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar empresa',
        description: errorMessage,
        color: 'error'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Crear nueva empresa cliente
  const createCompany = async (companyData: CreateCompanyRequest) => {
    loading.value = true
    error.value = null
    
    try {
      // Preparar datos para la API
      const requestData = {
        ...companyData,
        requesting_user_id: auth.user.value?.id || 0,
      }
      
      const response = await api.post<Empresa>('/api/Companies', requestData)
      
      toast.add({
        title: 'Empresa creada',
        description: 'La empresa cliente ha sido creada exitosamente',
        color: 'success'
      })
      
      // Refrescar lista
      await fetchCompanies()
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al crear empresa',
        description: errorMessage,
        color: 'error'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Actualizar empresa
  const updateCompany = async (id: number, companyData: UpdateCompanyRequest) => {
    loading.value = true
    error.value = null
    
    try {
      // Preparar datos para la API
      const requestData = {
        ...companyData,
        company_id: id,
        requesting_user_id: auth.user.value?.id || 0,
      }
      
      const response = await api.put<Empresa>(`/api/Companies/${id}`, requestData)
      
      toast.add({
        title: 'Empresa actualizada',
        description: 'La empresa ha sido actualizada exitosamente',
        color: 'success'
      })
      
      // Actualizar en la lista local
      const index = companies.value.findIndex((c: Empresa) => c.id === id)
      if (index !== -1) {
        companies.value[index] = response
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al actualizar empresa',
        description: errorMessage,
        color: 'error'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Eliminar/desactivar empresa
  const deleteCompany = async (id: number) => {
    loading.value = true
    error.value = null
    
    try {
      await api.delete(`/api/Companies/${id}`)
      
      toast.add({
        title: 'Empresa eliminada',
        description: 'La empresa ha sido desactivada exitosamente',
        color: 'success'
      })
      
      // Refrescar lista
      await fetchCompanies()
      
      return true
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al eliminar empresa',
        description: errorMessage,
        color: 'error'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Cambiar página
  const changePage = async (page: number) => {
    await fetchCompanies({ page })
  }
  
  // Aplicar filtros
  const applyFilters = async (newFilters: CompaniesFilters) => {
    await fetchCompanies({ ...newFilters, page: 1 })
  }
  
  // Limpiar filtros
  const clearFilters = async () => {
    filters.value = {
      search: '',
      page: 1,
      pageSize: 20
    }
    await fetchCompanies(filters.value)
  }
  
  // Verificar si puede editar una empresa
  const canEditCompany = (company: Empresa) => {
    // Solo empresa principal puede editar empresas cliente
    if (!auth.isEmpresaPrincipal.value) {
      return false
    }
    
    // No puede editar su propia empresa desde esta vista
    if (company.id === auth.empresa.value?.id) {
      return false
    }
    
    return true
  }
  
  // Verificar si puede eliminar una empresa
  const canDeleteCompany = (company: Empresa) => {
    return canEditCompany(company)
  }
  
  // Filtrar solo empresas cliente (no principales)
  const clientCompanies = computed(() => {
    return companies.value.filter(company => company.tipo_empresa === 'cliente')
  })
  
  // Verificar si una empresa está próxima a vencer (30 días)
  const isNearExpiration = (company: Empresa) => {
    if (!company.fecha_vencimiento) return false
    
    const expirationDate = new Date(company.fecha_vencimiento)
    const today = new Date()
    const thirtyDaysFromNow = new Date()
    thirtyDaysFromNow.setDate(today.getDate() + 30)
    
    return expirationDate <= thirtyDaysFromNow && expirationDate > today
  }
  
  // Verificar si una empresa está vencida
  const isExpired = (company: Empresa) => {
    if (!company.fecha_vencimiento) return false
    
    const expirationDate = new Date(company.fecha_vencimiento)
    const today = new Date()
    
    return expirationDate < today
  }
  
  return {
    // Estado
    companies: readonly(companies),
    loading: readonly(loading),
    error: readonly(error),
    pagination: readonly(pagination),
    filters: readonly(filters),
    
    // Computeds
    clientCompanies: readonly(clientCompanies),
    
    // Acciones
    fetchCompanies,
    fetchCompany,
    createCompany,
    updateCompany,
    deleteCompany,
    
    // Navegación y filtros
    changePage,
    applyFilters,
    clearFilters,
    
    // Helpers
    canEditCompany,
    canDeleteCompany,
    isNearExpiration,
    isExpired
  }
}