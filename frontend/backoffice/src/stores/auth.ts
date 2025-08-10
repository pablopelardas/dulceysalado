import { defineStore } from 'pinia'
import type { 
  AuthState, 
  LoginRequest, 
  LoginResponse, 
  RefreshTokenResponse,
  Usuario,
  Empresa 
} from '~/types/auth'

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    user: null,
    empresa: null,
    accessToken: null,
    refreshTokenValue: null,
    isAuthenticated: false,
    isLoading: false,
    error: null
  }),

  getters: {
    userRole: (state): string | null => state.user?.rol || null,
    
    isEmpresaPrincipal: (state): boolean => 
      state.empresa?.tipo_empresa === 'principal',
    
    isEmpresaCliente: (state): boolean => 
      state.empresa?.tipo_empresa === 'cliente',
    
    userPermissions: (state) => ({
      canManageProductosBase: state.user?.puede_gestionar_productos_base || false,
      canManageCategoriasBase: state.user?.puede_gestionar_categorias_base || false,
      canManageUsuarios: state.user?.puede_gestionar_usuarios || false,
      canViewEstadisticas: state.user?.puede_ver_estadisticas || false
    }),

    empresaColors: (state) => state.empresa?.colores_tema || {
      primario: '#4A90E2',
      secundario: '#FF6B35',
      acento: '#8BC34A'
    }
  },

  actions: {
    async login(credentials: LoginRequest): Promise<void> {
      this.isLoading = true
      this.error = null

      try {
        const { getEnvironment } = await import('~/utils/environment')
        const environment = getEnvironment()
        
        const response = await $fetch<LoginResponse>('/api/auth/login', {
          baseURL: environment.apiBaseUrl,
          method: 'POST',
          body: credentials
        })

        this.setAuthData(response)
        
        // Persistir tokens en cookies
        const accessTokenCookie = useCookie('authToken', {
          maxAge: 60 * 60 * 24, // 24 horas
          secure: false, // HTTP en desarrollo
          httpOnly: false,
          sameSite: 'lax'
        })
        
        const refreshTokenCookie = useCookie('refreshToken', {
          maxAge: 60 * 60 * 24 * 7, // 7 días
          secure: false, // HTTP en desarrollo
          httpOnly: false,
          sameSite: 'lax'
        })

        accessTokenCookie.value = response.access_token
        refreshTokenCookie.value = response.refresh_token

        
        // Verificar inmediatamente si las cookies se pueden leer
        setTimeout(() => {
          const testAccessToken = useCookie('authToken', { secure: false, httpOnly: false, sameSite: 'lax' })
          const testRefreshToken = useCookie('refreshToken', { secure: false, httpOnly: false, sameSite: 'lax' })

        }, 100)

      } catch (error: any) {
        this.error = error.data?.message || 'Error en el login'
        throw error
      } finally {
        this.isLoading = false
      }
    },

    async refreshToken(): Promise<void> {
      const refreshTokenCookie = useCookie('refreshToken')
      
      if (!refreshTokenCookie.value) {
        throw new Error('No refresh token available')
      }

      try {
        const { getEnvironment } = await import('~/utils/environment')
        const environment = getEnvironment()
        
        const response = await $fetch<RefreshTokenResponse>('/api/auth/refresh', {
          baseURL: environment.apiBaseUrl,
          method: 'POST',
          body: { refresh_token: refreshTokenCookie.value }
        })

        this.accessToken = response.access_token
        this.refreshTokenValue = response.refresh_token

        // Actualizar cookies
        const accessTokenCookie = useCookie('authToken', {
          maxAge: 60 * 60 * 24, // 24 horas
          secure: false, // HTTP en desarrollo
          httpOnly: false,
          sameSite: 'lax'
        })
        
        const newRefreshTokenCookie = useCookie('refreshToken', {
          maxAge: 60 * 60 * 24 * 7, // 7 días
          secure: false, // HTTP en desarrollo
          httpOnly: false,
          sameSite: 'lax'
        })
        
        accessTokenCookie.value = response.access_token
        newRefreshTokenCookie.value = response.refresh_token

      } catch (error) {
        this.logout()
        throw error
      }
    },

    async me(): Promise<void> {

      
      if (!this.accessToken) {
        return
      }

      try {
        const { getEnvironment } = await import('~/utils/environment')
        const environment = getEnvironment()
        
        // Configuración de fetch para desarrollo
        const fetchOptions: any = {
          baseURL: environment.apiBaseUrl,
          headers: {
            Authorization: `Bearer ${this.accessToken}`
          }
        }
        
        
        const response = await $fetch<{ user: Usuario, empresa: Empresa }>('/api/auth/me', fetchOptions)

        this.user = response.user
        this.empresa = response.empresa
        this.isAuthenticated = true

      } catch (error) {
        console.error('[Auth Store] Error en me():', error)
        
        // En el servidor, solo limpiar el estado
        if (import.meta.server) {
          this.clearAuthState()
        } else {
          // En el cliente, hacer logout completo
          this.logout()
        }
        throw error
      }
    },

    logout(): void {
      
      this.user = null
      this.empresa = null
      this.accessToken = null
      this.refreshTokenValue = null
      this.isAuthenticated = false
      this.error = null

      // Solo limpiar cookies y navegar en el cliente
      if (import.meta.client) {
        // Limpiar cookies
        const accessTokenCookie = useCookie('authToken', {
          secure: false,
          httpOnly: false,
          sameSite: 'lax'
        })
        
        const refreshTokenCookie = useCookie('refreshToken', {
          secure: false,
          httpOnly: false,
          sameSite: 'lax'
        })
        
        accessTokenCookie.value = null
        refreshTokenCookie.value = null

        // Redireccionar a login
        navigateTo('/login')
      }
    },

    // Método para limpiar solo el estado (sin cookies ni navegación)
    clearAuthState(): void {
      this.user = null
      this.empresa = null
      this.accessToken = null
      this.refreshTokenValue = null
      this.isAuthenticated = false
      this.error = null
    },

    setAuthData(data: LoginResponse): void {
      this.user = data.user
      this.empresa = data.empresa
      this.accessToken = data.access_token
      this.refreshTokenValue = data.refresh_token
      this.isAuthenticated = true
    },

    initializeFromCookies(): void {
      const accessTokenCookie = useCookie('authToken', {
        secure: false, // HTTP en desarrollo
        httpOnly: false,
        sameSite: 'lax'
      })
      
      const refreshTokenCookie = useCookie('refreshToken', {
        secure: false, // HTTP en desarrollo
        httpOnly: false,
        sameSite: 'lax'
      })

      if (accessTokenCookie.value && refreshTokenCookie.value) {
        this.accessToken = accessTokenCookie.value
        this.refreshTokenValue = refreshTokenCookie.value
        
        // Intentar obtener datos del usuario
        this.me().catch(() => {
          // Si falla, limpiar tokens
          this.logout()
        })
      }
    }
  }
})