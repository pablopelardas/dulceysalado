import type {
  CreateUserRequest,
  UpdateUserRequest,
  ChangePasswordRequest,
  AdminChangePasswordRequest,
  UserListResponse,
  CreateUserResponse,
  UpdateUserResponse,
  UserFilters
} from '~/types/users'
import type { Usuario } from '~/types/auth'

export const useUsers = () => {
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
  
  // Estado reactivo para lista de usuarios
  const users = ref<Usuario[]>([])
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
  const filters = ref<UserFilters>({
    search: '',
    rol: '',
    activo: '',
    page: 1,
    limit: 20
  })
  
  // Obtener lista de usuarios
  const fetchUsers = async (customFilters?: UserFilters) => {
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
        if (queryFilters[key as keyof UserFilters] === '' || 
            queryFilters[key as keyof UserFilters] === undefined) {
          delete queryFilters[key as keyof UserFilters]
        }
      })
      
      const response = await api.get<UserListResponse>('/api/users', {
        query: queryFilters
      })
      
      users.value = response.usuarios || []
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
        title: 'Error al cargar usuarios',
        description: errorMessage,
        color: 'error'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Obtener un usuario específico
  const fetchUser = async (id: number) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<Usuario>(`/api/users/${id}`)
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar usuario',
        description: errorMessage,
        color: 'error'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Crear nuevo usuario
  const createUser = async (userData: CreateUserRequest) => {
    loading.value = true
    error.value = null
    
    try {
      // Si es empresa cliente, no enviar empresa_id
      if (!auth.isEmpresaPrincipal.value) {
        delete userData.empresa_id
      }
      
      const response = await api.post<CreateUserResponse & { observaciones?: string[] }>('/api/users', userData)
      
      toast.add({
        title: 'Usuario creado',
        description: response.message || 'El usuario ha sido creado exitosamente',
        color: 'success'
      })
      
      // Si hay observaciones, mostrarlas en un toast adicional
      if (response.observaciones && response.observaciones.length > 0) {
        toast.add({
          title: 'Observaciones',
          description: response.observaciones.join('\n'),
          color: 'warning'
        })
      }
      
      // Refrescar lista
      await fetchUsers()
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al crear usuario',
        description: errorMessage,
        color: 'error'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Actualizar usuario
  const updateUser = async (id: number, userData: UpdateUserRequest) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.put<UpdateUserResponse & { observaciones?: string[] }>(`/api/users/${id}`, userData)
      
      toast.add({
        title: 'Usuario actualizado',
        description: response.message || 'El usuario ha sido actualizado exitosamente',
        color: 'success'
      })
      
      // Si hay observaciones, mostrarlas en un toast adicional
      if (response.observaciones && response.observaciones.length > 0) {
        toast.add({
          title: 'Observaciones',
          description: response.observaciones.join('\n'),
          color: 'warning'
        })
      }
      
      // Actualizar en la lista local
      const index = users.value.findIndex((u: Usuario) => u.id === id)
      if (index !== -1) {
        users.value[index] = response.user
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al actualizar usuario',
        description: errorMessage,
        color: 'error'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Eliminar/desactivar usuario
  const deleteUser = async (id: number) => {
    loading.value = true
    error.value = null
    
    try {
      await api.delete(`/api/users/${id}`)
      
      toast.add({
        title: 'Usuario eliminado',
        description: 'El usuario ha sido desactivado exitosamente',
        color: 'success'
      })
      
      // Refrescar lista
      await fetchUsers()
      
      return true
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al eliminar usuario',
        description: errorMessage,
        color: 'error'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Cambiar contraseña (usuario cambio propia contraseña)
  const changePassword = async (id: number, passwordData: ChangePasswordRequest) => {
    loading.value = true
    error.value = null
    
    try {
      await api.put(`/api/users/${id}/password`, passwordData)
      
      toast.add({
        title: 'Contraseña actualizada',
        description: 'La contraseña ha sido actualizada exitosamente',
        color: 'success'
      })
      
      return true
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cambiar contraseña',
        description: errorMessage,
        color: 'error'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Cambiar contraseña (administrador cambia contraseña de otro usuario)
  const adminChangePassword = async (id: number, passwordData: AdminChangePasswordRequest) => {
    loading.value = true
    error.value = null
    
    try {
      // Para admin cambiando contraseña de otro usuario, solo enviamos newPassword
      await api.put(`/api/users/${id}/password`, {
        NewPassword: passwordData.password
        // currentPassword se omite cuando es admin cambiando contraseña de otro usuario
      })
      
      return true
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cambiar contraseña',
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
    await fetchUsers({ page })
  }
  
  // Aplicar filtros
  const applyFilters = async (newFilters: UserFilters) => {
    await fetchUsers({ ...newFilters, page: 1 })
  }
  
  // Limpiar filtros
  const clearFilters = async () => {
    filters.value = {
      search: '',
      rol: '',
      activo: '',
      page: 1,
      limit: 20
    }
    await fetchUsers(filters.value)
  }
  
  // Verificar si puede editar un usuario
  const canEditUser = (user: Usuario) => {
    // No puede editar su propio usuario
    if (user.id === auth.user.value?.id) {
      return false
    }
    
    // Empresa principal puede editar cualquier usuario
    if (auth.isEmpresaPrincipal.value) {
      return true
    }
    
    // Empresa cliente solo puede editar usuarios de su empresa
    return user.empresa_id === auth.empresa.value?.id
  }
  
  // Verificar si puede eliminar un usuario
  const canDeleteUser = (user: Usuario) => {
    // No puede eliminar su propio usuario
    if (user.id === auth.user.value?.id) {
      return false
    }
    
    return canEditUser(user)
  }
  
  return {
    // Estado
    users: readonly(users),
    loading: readonly(loading),
    error: readonly(error),
    pagination: readonly(pagination),
    filters: readonly(filters),
    
    // Acciones
    fetchUsers,
    fetchUser,
    createUser,
    updateUser,
    deleteUser,
    changePassword,
    adminChangePassword,
    
    // Navegación y filtros
    changePage,
    applyFilters,
    clearFilters,
    
    // Helpers
    canEditUser,
    canDeleteUser
  }
}