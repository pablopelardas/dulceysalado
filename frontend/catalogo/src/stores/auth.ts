import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApiService } from '@/services/api'

interface User {
  id: number
  codigo: string
  nombre?: string
  apellido?: string
  email?: string
  telefono?: string
  direccion?: string
  empresa_id: number
  lista_precio?: {
    id: number
    codigo: string
    nombre: string
    descripcion?: string
    activo: boolean
  }
}

interface LoginCredentials {
  username: string
  password: string
}

interface RegisterData {
  nombre: string
  email: string
  password: string
  telefono?: string
  direccion?: string
}

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const token = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const userFullName = computed(() => {
    if (!user.value) return ''
    const nombre = user.value.nombre || ''
    const apellido = user.value.apellido || ''
    return `${nombre} ${apellido}`.trim() || user.value.codigo
  })

  // Actions
  const login = async (credentials: LoginCredentials): Promise<boolean> => {
    loading.value = true
    error.value = null

    try {
      const response = await authApiService.login(credentials.username, credentials.password)
      
      user.value = response.cliente
      token.value = response.access_token
      refreshToken.value = response.refresh_token

      // Guardar en localStorage
      localStorage.setItem('auth_token', response.access_token)
      localStorage.setItem('refresh_token', response.refresh_token)
      localStorage.setItem('user_data', JSON.stringify(response.cliente))

      return true
    } catch (err) {
      // Intentar extraer el mensaje del error JSON si existe
      let errorMessage = 'Error al iniciar sesión'
      
      if (err instanceof Error) {
        try {
          // Intentar parsear el mensaje como JSON para extraer solo el mensaje
          const errorData = JSON.parse(err.message)
          if (errorData.message) {
            errorMessage = errorData.message
          } else {
            errorMessage = err.message
          }
        } catch {
          // Si no es JSON válido, usar el mensaje tal como está
          errorMessage = err.message
        }
      }
      
      error.value = errorMessage
      return false
    } finally {
      loading.value = false
    }
  }

  const register = async (registerData: RegisterData): Promise<boolean> => {
    loading.value = true
    error.value = null

    try {
      const response = await authApiService.register({
        empresa_id: 1, // Dulce y Salado
        nombre: registerData.nombre,
        email: registerData.email,
        password: registerData.password,
        telefono: registerData.telefono,
        direccion: registerData.direccion
      })

      // Mapear la respuesta del backend al formato esperado por el frontend
      user.value = {
        id: response.cliente.id,
        codigo: response.cliente.codigo,
        nombre: response.cliente.nombre,
        email: response.cliente.email,
        telefono: response.cliente.telefono,
        direccion: response.cliente.direccion,
        empresa_id: response.cliente.empresa_id,
        lista_precio: response.cliente.lista_precio ? {
          ...response.cliente.lista_precio,
          activo: true // Valor por defecto si no viene del backend
        } : undefined
      }
      
      token.value = response.access_token
      refreshToken.value = response.refresh_token

      // Guardar en localStorage
      localStorage.setItem('auth_token', response.access_token)
      localStorage.setItem('refresh_token', response.refresh_token)
      localStorage.setItem('user_data', JSON.stringify(user.value))

      console.log('Registro completado exitosamente:', user.value)
      return true
    } catch (err) {
      // Intentar extraer el mensaje del error JSON si existe
      let errorMessage = 'Error al registrarse'
      
      if (err instanceof Error) {
        try {
          // Intentar parsear el mensaje como JSON para extraer solo el mensaje
          const errorData = JSON.parse(err.message)
          if (errorData.message) {
            errorMessage = errorData.message
          } else {
            errorMessage = err.message
          }
        } catch {
          // Si no es JSON válido, usar el mensaje tal como está
          errorMessage = err.message
        }
      }
      
      error.value = errorMessage
      return false
    } finally {
      loading.value = false
    }
  }

  const loginWithGoogle = async (): Promise<boolean> => {
    loading.value = true
    error.value = null

    try {
      await authApiService.loginWithGoogle()
      // La redirección manejará el resto del flujo
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Error al iniciar sesión con Google'
      return false
    } finally {
      loading.value = false
    }
  }

  const logout = async () => {
    // Intentar hacer logout en el servidor si tenemos tokens
    if (token.value && refreshToken.value) {
      try {
        await authApiService.logout(refreshToken.value, token.value)
      } catch (err) {
        console.warn('Error al hacer logout en el servidor:', err)
        // Continuar con el logout local aunque falle el servidor
      }
    }

    // Limpiar estado local
    user.value = null
    token.value = null
    refreshToken.value = null
    error.value = null

    // Limpiar localStorage
    localStorage.removeItem('auth_token')
    localStorage.removeItem('refresh_token')
    localStorage.removeItem('user_data')
  }

  const initializeAuth = () => {
    // Recuperar datos de autenticación del localStorage
    const savedToken = localStorage.getItem('auth_token')
    const savedRefreshToken = localStorage.getItem('refresh_token')
    const savedUser = localStorage.getItem('user_data')

    if (savedToken && savedRefreshToken && savedUser) {
      try {
        token.value = savedToken
        refreshToken.value = savedRefreshToken
        user.value = JSON.parse(savedUser)
      } catch (err) {
        console.error('Error al recuperar datos de autenticación:', err)
        logout()
      }
    }
  }

  const updateProfile = async (profileData: Record<string, unknown>): Promise<boolean> => {
    loading.value = true
    error.value = null

    if (!token.value) {
      error.value = 'No hay token de autenticación'
      loading.value = false
      return false
    }

    try {
      const response = await authApiService.updateProfile(profileData, token.value)
      
      // Actualizar usuario con los datos retornados del servidor
      user.value = response.cliente
      // No actualizar localStorage aquí - dejamos que se actualice cuando se refresque el perfil

      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Error al actualizar perfil'
      return false
    } finally {
      loading.value = false
    }
  }

  const refreshUserProfile = async (): Promise<boolean> => {
    if (!token.value) {
      return false
    }

    try {
      const profile = await authApiService.getProfile(token.value)
      user.value = profile

      // Actualizar localStorage solo con datos verificados del servidor
      localStorage.setItem('user_data', JSON.stringify(profile))

      return true
    } catch (err) {
      console.error('Error al refrescar perfil:', err)
      // Si falla, intentar refrescar token
      return await refreshAccessToken()
    }
  }

  const clearError = () => {
    error.value = null
  }

  const refreshAccessToken = async (): Promise<boolean> => {
    if (!refreshToken.value) {
      return false
    }

    try {
      const response = await authApiService.refreshToken(refreshToken.value)
      
      token.value = response.access_token
      refreshToken.value = response.refresh_token
      user.value = response.cliente

      // Actualizar localStorage
      localStorage.setItem('auth_token', response.access_token)
      localStorage.setItem('refresh_token', response.refresh_token)
      localStorage.setItem('user_data', JSON.stringify(response.cliente))

      return true
    } catch (err) {
      console.error('Error al refrescar token:', err)
      logout()
      return false
    }
  }

  const fetchUserProfile = async (): Promise<boolean> => {
    if (!token.value) {
      return false
    }

    try {
      const profile = await authApiService.getProfile(token.value)
      user.value = profile

      // Actualizar localStorage
      localStorage.setItem('user_data', JSON.stringify(profile))

      return true
    } catch (err) {
      console.error('Error al obtener perfil:', err)
      // Si falla, intentar refrescar token
      return await refreshAccessToken()
    }
  }

  // Helper para ejecutar llamadas autenticadas con retry automático
  const executeWithAuth = async <T>(
    apiCall: (token: string) => Promise<T>
  ): Promise<T> => {
    if (!token.value) {
      throw new Error('No hay token de autenticación')
    }

    try {
      // Intentar la llamada original
      return await apiCall(token.value)
    } catch (error) {
      // Si es un error 401, intentar refrescar el token
      const errorMessage = (error as Error).message || ''
      const is401 = errorMessage.includes('401') || 
                    errorMessage.includes('Unauthorized') ||
                    errorMessage.includes('Token expired')
      
      if (is401) {
        console.log('Token expirado, intentando refrescar...')
        
        const refreshed = await refreshAccessToken()
        
        if (refreshed && token.value) {
          // Reintentar la llamada con el nuevo token
          console.log('Token refrescado, reintentando llamada...')
          return await apiCall(token.value)
        } else {
          // Si no se pudo refrescar, ya se hizo logout
          throw new Error('Sesión expirada. Por favor, inicia sesión nuevamente.')
        }
      }
      
      // Si no es un error 401, propagar el error original
      throw error
    }
  }

  return {
    // State
    user,
    token,
    refreshToken,
    loading,
    error,
    
    // Getters
    isAuthenticated,
    userFullName,
    
    // Actions
    login,
    register,
    loginWithGoogle,
    logout,
    initializeAuth,
    updateProfile,
    refreshUserProfile,
    clearError,
    refreshAccessToken,
    fetchUserProfile,
    executeWithAuth
  }
})