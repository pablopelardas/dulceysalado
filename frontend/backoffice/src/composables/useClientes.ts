import type {
  ClienteDto,
  GetAllClientesQueryResult,
  GetClienteByIdQueryResult,
  GetClienteCredentialsQueryResult,
  CreateClienteCommand,
  CreateClienteCommandResult,
  UpdateClienteCommand,
  UpdateClienteCommandResult,
  CreateClienteCredentialsCommand,
  CreateClienteCredentialsCommandResult,
  UpdateClienteCredentialsCommand,
  UpdateClienteCredentialsCommandResult,
  ResetPasswordClienteCommand,
  ResetPasswordClienteCommandResult,
  ClientesFilters
} from '~/types/clientes'

export const useClientes = () => {
  const api = useApi()
  const auth = useAuth()
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
  
  // Estado reactivo para lista de clientes
  const clientes = ref<ClienteDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  // Estado para listas de precios
  const listasDisponibles = ref<any[]>([])
  
  // Estado de paginación
  const pagination = ref({
    page: 1,
    limit: 20,
    total: 0,
    pages: 0
  })
  
  // Filtros activos
  const filters = ref<ClientesFilters>({
    search: '',
    lista_precio_id: undefined,
    has_credentials: undefined,
    is_active: undefined,
    localidad: '',
    provincia: '',
    include_deleted: false,
    page: 1,
    page_size: 20,
    sortBy: 'nombre',
    sortOrder: 'asc'
  })
  
  // ==================== CRUD DE CLIENTES ====================
  
  // Obtener lista de clientes
  const fetchClientes = async (customFilters?: ClientesFilters) => {
    loading.value = true
    error.value = null
    
    try {
      // Obtener empresa_id desde el contexto de auth
      const empresaId = auth.empresa.value?.id
      if (!empresaId) {
        throw new Error('No se pudo obtener el ID de la empresa')
      }
      
      // Actualizar filtros activos ANTES de hacer la petición
      if (customFilters) {
        filters.value = { ...filters.value, ...customFilters }
      }
      
      // Combinar filtros con empresa_id siempre presente
      const queryFilters: any = {
        empresa_id: empresaId,
        ...filters.value,
        ...customFilters
      }
      
      // Limpiar filtros vacíos (pero mantener empresa_id siempre)
      Object.keys(queryFilters).forEach(key => {
        const value = queryFilters[key]
        if (value === '' || value === undefined) {
          if (key !== 'empresa_id') {
            delete queryFilters[key]
          }
        }
      })
      
      const response = await api.get<GetAllClientesQueryResult>('/api/Clientes', {
        query: queryFilters
      })
      
      clientes.value = response.clientes || []
      
      // Manejar paginación
      pagination.value = {
        page: response.page || 1,
        limit: response.page_size || 20,
        total: response.total || 0,
        pages: response.total_pages || Math.ceil((response.total || 0) / (response.page_size || 20))
      }

      // Actualizar listas de precios disponibles
      if (response.listas_disponibles) {
        listasDisponibles.value = response.listas_disponibles
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar clientes',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Obtener un cliente específico
  const fetchCliente = async (id: number, includeDeleted: boolean = true) => {
    loading.value = true
    error.value = null
    
    try {
      // Obtener empresa_id desde el contexto de auth
      const empresaId = auth.empresa.value?.id
      if (!empresaId) {
        throw new Error('No se pudo obtener el ID de la empresa')
      }
      
      const response = await api.get<GetClienteByIdQueryResult>(`/api/Clientes/${id}`, {
        query: {
          empresa_id: empresaId,
          include_deleted: includeDeleted
        }
      })
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar cliente',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Crear nuevo cliente (solo datos básicos)
  const createCliente = async (clienteData: CreateClienteCommand) => {
    loading.value = true
    error.value = null
    
    try {
      // Agregar empresa_id y created_by desde el contexto de auth
      const empresaId = auth.empresa.value?.id
      const userName = auth.user.value?.nombre || 'Sistema'
      
      if (!empresaId) {
        throw new Error('No se pudo obtener el ID de la empresa')
      }
      
      const dataWithEmpresa = {
        ...clienteData,
        empresa_id: empresaId,
        created_by: userName
      }
      
      const response = await api.post<CreateClienteCommandResult>('/api/Clientes', dataWithEmpresa)
      
      toast.add({
        title: 'Cliente creado',
        description: 'El cliente ha sido creado exitosamente',
        color: 'green'
      })
      
      // Refrescar lista
      await fetchClientes()
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al crear cliente',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Actualizar cliente
  const updateCliente = async (id: number, clienteData: UpdateClienteCommand) => {
    loading.value = true
    error.value = null
    
    try {
      // Obtener empresa_id desde el contexto de auth
      const empresaId = auth.empresa.value?.id
      if (!empresaId) {
        throw new Error('No se pudo obtener el ID de la empresa')
      }
      
      // Agregar empresa_id al body de la petición
      const dataWithEmpresa = {
        ...clienteData,
        empresa_id: empresaId
      }
      
      const response = await api.put<UpdateClienteCommandResult>(`/api/Clientes/${id}`, dataWithEmpresa)
      
      toast.add({
        title: 'Cliente actualizado',
        description: 'El cliente ha sido actualizado exitosamente',
        color: 'green'
      })
      
      // Actualizar en la lista local
      const index = clientes.value.findIndex(c => c.id === id)
      if (index !== -1 && response) {
        clientes.value[index] = response
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al actualizar cliente',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Eliminar cliente
  const deleteCliente = async (id: number) => {
    loading.value = true
    error.value = null
    
    try {
      // Obtener empresa_id desde el contexto de auth
      const empresaId = auth.empresa.value?.id
      if (!empresaId) {
        throw new Error('No se pudo obtener el ID de la empresa')
      }
      
      await api.delete(`/api/Clientes/${id}`, {
        query: {
          empresa_id: empresaId
        }
      })
      
      toast.add({
        title: 'Cliente eliminado',
        description: 'El cliente ha sido eliminado exitosamente',
        color: 'green'
      })
      
      // Refrescar lista
      await fetchClientes()
      
      return true
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al eliminar cliente',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // ==================== GESTIÓN DE CREDENCIALES ====================
  
  
  // Crear credenciales para un cliente
  const createClienteCredentials = async (credentialsData: CreateClienteCredentialsCommand) => {
    loading.value = true
    error.value = null
    
    try {
      // Agregar empresa_id y created_by desde el contexto de auth
      const empresaId = auth.empresa.value?.id
      const userName = auth.user.value?.nombre || 'Sistema'
      
      if (!empresaId) {
        throw new Error('No se pudo obtener el ID de la empresa')
      }
      
      const dataWithEmpresa = {
        ...credentialsData,
        empresa_id: empresaId,
        created_by: userName
      }
      
      const response = await api.post<CreateClienteCredentialsCommandResult>(
        `/api/Clientes/${credentialsData.cliente_id}/credentials`,
        dataWithEmpresa
      )
      
      toast.add({
        title: 'Credenciales creadas',
        description: 'Las credenciales han sido creadas exitosamente',
        color: 'green'
      })
      
      // Refrescar lista para actualizar el estado has_credentials
      await fetchClientes()
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al crear credenciales',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Actualizar contraseña de un cliente
  const updateClientePassword = async (clienteId: number, newPassword: string) => {
    loading.value = true
    error.value = null
    
    try {
      // Obtener empresa_id y updated_by desde el contexto
      const empresaId = auth.empresa.value?.id
      const updatedBy = auth.user.value?.nombre || 'Sistema'
      
      if (!empresaId) {
        throw new Error('No se pudo obtener el ID de la empresa')
      }
      
      const response = await api.put(
        `/api/Clientes/${clienteId}/password`,
        {
          cliente_id: clienteId,
          empresa_id: empresaId,
          new_password: newPassword,
          updated_by: updatedBy
        }
      )
      
      toast.add({
        title: 'Contraseña actualizada',
        description: 'La contraseña ha sido actualizada exitosamente',
        color: 'green'
      })
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al actualizar contraseña',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // ==================== NAVEGACIÓN Y FILTROS ====================
  
  // Cambiar página
  const changePage = async (page: number) => {
    await fetchClientes({ ...filters.value, page })
  }
  
  // Aplicar filtros
  const applyFilters = async (newFilters: ClientesFilters) => {
    await fetchClientes({ ...newFilters, page: 1 })
  }
  
  // Limpiar filtros
  const clearFilters = async () => {
    const defaultFilters: ClientesFilters = {
      search: '',
      lista_precio_id: undefined,
      has_credentials: undefined,
      is_active: undefined,
      localidad: '',
      provincia: '',
      include_deleted: false, // Por defecto no mostrar eliminados
      page: 1,
      page_size: 20,
      sortBy: 'nombre',
      sortOrder: 'asc'
    }
    filters.value = defaultFilters
    await fetchClientes(defaultFilters)
  }
  
  // Aplicar ordenamiento
  const applySorting = async (sortBy: string, sortOrder: 'asc' | 'desc') => {
    filters.value = { ...filters.value, sortBy, sortOrder }
    await fetchClientes({ ...filters.value, page: 1 })
  }
  
  // ==================== HELPERS ====================
  
  // Verificar si un cliente tiene credenciales
  const hasCredentials = (cliente: ClienteDto): boolean => {
    return cliente.tiene_acceso || false
  }
  
  // Verificar si las credenciales están activas
  const isCredentialsActive = (cliente: ClienteDto): boolean => {
    return cliente.activo || false
  }
  
  // Obtener nombre de lista de precios
  const getListaPrecioNombre = (cliente: ClienteDto): string => {
    return cliente.lista_precio?.nombre || 'Sin lista asignada'
  }
  
  return {
    // Estado
    clientes: readonly(clientes),
    loading: readonly(loading),
    error: readonly(error),
    pagination: readonly(pagination),
    filters: filters, // Sin readonly para mantener reactividad
    listasDisponibles: readonly(listasDisponibles),
    
    // CRUD de clientes
    fetchClientes,
    fetchCliente,
    createCliente,
    updateCliente,
    deleteCliente,
    
    // Gestión de credenciales
    createClienteCredentials,
    updateClientePassword,
    
    // Navegación y filtros
    changePage,
    applyFilters,
    clearFilters,
    applySorting,
    
    // Helpers
    hasCredentials,
    isCredentialsActive,
    getListaPrecioNombre
  }
}