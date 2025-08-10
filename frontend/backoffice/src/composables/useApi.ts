import type { ApiError } from '~/types/auth'
import { parseApiError } from '~/utils/errorParser'
import { getEnvironment } from '~/utils/environment'

export const useApi = () => {
  console.log('🚀 useApi composable initializing...')
  const authStore = useAuthStore()
  
  // Obtener la URL base de la configuración del entorno
  const getApiBaseUrl = () => {
    try {
      const environment = getEnvironment()
      return environment.apiBaseUrl
    } catch (error) {
      console.error('❌ Error in getApiBaseUrl:', error)
      return 'https://localhost:7000'
    }
  }

  const getHeaders = (isFormData = false) => {
    const headers: Record<string, string> = {}
    
    // No establecer Content-Type para FormData, el navegador lo hará automáticamente
    if (!isFormData) {
      headers['Content-Type'] = 'application/json'
    }
    
    if (authStore.accessToken) {
      headers['Authorization'] = `Bearer ${authStore.accessToken}`
    }
    
    return headers
  }

  // Para llamadas de datos (usar solo en setup inicial)
  const handleInitialRequest = <T>(url: string, options: any = {}) => {
    // Detectar si el body es FormData
    const isFormData = options.body instanceof FormData
    
    const fetchOptions = {
      baseURL: getApiBaseUrl(),
      headers: getHeaders(isFormData),
      ...options,
      onResponseError({ response }: any) {
        // Manejar errores de autenticación
        if (response.status === 401) {
          authStore.refreshToken().then(() => {
            // El refresh se maneja automáticamente
          }).catch(() => {
            authStore.logout()
            throw new Error('Sesión expirada')
          })
        }

        const parsed = parseApiError(response._data || response)
        const apiError: ApiError = {
          error: parsed.message,
          message: parsed.message,
          originalError: parsed.originalError
        }
        
        throw apiError
      }
    }

    return useFetch<T>(url, fetchOptions)
  }

  // Para todas las demás llamadas (funciona tanto en SSR como cliente)
  const handleRequest = async <T>(url: string, options: any = {}): Promise<T> => {
    try {
      // Detectar si el body es FormData
      const isFormData = options.body instanceof FormData
      
      return await $fetch<T>(url, {
        baseURL: getApiBaseUrl(),
        headers: getHeaders(isFormData),
        ...options
      })
    } catch (error: any) {
      // Manejar errores de autenticación
      if (error.status === 401) {
        try {
          await authStore.refreshToken()
          
          // Detectar si el body es FormData para el retry
          const isFormData = options.body instanceof FormData
          
          // Reintentar la petición con el nuevo token
          return await $fetch<T>(url, {
            baseURL: getApiBaseUrl(),
            headers: getHeaders(isFormData),
            ...options
          })
        } catch (refreshError) {
          authStore.logout()
          throw new Error('Sesión expirada')
        }
      }

      // Manejar otros errores
      const parsed = parseApiError(error)
      const apiError: ApiError = {
        error: parsed.message,
        message: parsed.message,
        originalError: parsed.originalError
      }
      
      throw apiError
    }
  }

  return {
    // Para carga inicial de datos (solo usar en setup)
    initialGet: <T>(url: string, options?: any) => 
      handleInitialRequest<T>(url, { method: 'GET', ...options }),
    
    // Métodos normales (funcionan siempre)
    get: <T>(url: string, options?: any) => 
      handleRequest<T>(url, { method: 'GET', ...options }),
    
    post: <T>(url: string, body?: any, options?: any) => 
      handleRequest<T>(url, { method: 'POST', body, ...options }),
    
    put: <T>(url: string, body?: any, options?: any) => 
      handleRequest<T>(url, { method: 'PUT', body, ...options }),
    
    patch: <T>(url: string, body?: any, options?: any) => 
      handleRequest<T>(url, { method: 'PATCH', body, ...options }),
    
    delete: <T>(url: string, options?: any) => 
      handleRequest<T>(url, { method: 'DELETE', ...options })
  }
}