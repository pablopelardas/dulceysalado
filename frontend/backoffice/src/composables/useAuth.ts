import type { LoginRequest } from '~/types/auth'

export const useAuth = () => {
  const authStore = useAuthStore()

  return {
    // Estado reactivo
    user: readonly(toRef(authStore, 'user')),
    empresa: readonly(toRef(authStore, 'empresa')),
    isAuthenticated: readonly(toRef(authStore, 'isAuthenticated')),
    isLoading: readonly(toRef(authStore, 'isLoading')),
    error: readonly(toRef(authStore, 'error')),

    // Getters
    userRole: computed(() => authStore.userRole),
    isEmpresaPrincipal: computed(() => authStore.isEmpresaPrincipal),
    isEmpresaCliente: computed(() => authStore.isEmpresaCliente),
    userPermissions: computed(() => authStore.userPermissions),
    empresaColors: computed(() => authStore.empresaColors),
    
    // Estado de carga de permisos
    isPermissionsLoaded: computed(() => 
      authStore.isAuthenticated && 
      authStore.user !== null && 
      authStore.empresa !== null && 
      !authStore.isLoading
    ),

    // Acciones
    login: async (credentials: LoginRequest) => {
      await authStore.login(credentials)
    },

    logout: () => {
      authStore.logout()
    },

    refreshToken: async () => {
      await authStore.refreshToken()
    },

    me: async () => {
      await authStore.me()
    },

    // Helpers de permisos
    can: (permission: keyof typeof authStore.userPermissions) => {
      return authStore.userPermissions[permission]
    },

    canAny: (permissions: Array<keyof typeof authStore.userPermissions>) => {
      return permissions.some(permission => authStore.userPermissions[permission])
    },

    canAll: (permissions: Array<keyof typeof authStore.userPermissions>) => {
      return permissions.every(permission => authStore.userPermissions[permission])
    },

    // Helpers de rol
    hasRole: (role: string | string[]) => {
      if (Array.isArray(role)) {
        return role.includes(authStore.userRole || '')
      }
      return authStore.userRole === role
    },

    // Helper para verificar tipo de empresa
    isRole: (role: 'admin' | 'editor' | 'viewer') => {
      return authStore.userRole === role
    }
  }
}