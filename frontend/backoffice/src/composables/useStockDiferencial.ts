import type { ProductoBaseDto } from '~/types/productos'

// Tipos específicos para stock diferencial
export interface StockDiferencialOptions {
  empresaId: number
  productoId: number
}

export interface StockDiferencialData {
  empresaId: number
  productoId: number
  existencia: number
  esStockDiferencial: boolean
  stockBase?: number
}

export interface StockDiferencialUpdateRequest {
  empresaId: number
  productoId: number
  existencia: number
}

export interface StockDiferencialResponse {
  success: boolean
  data?: StockDiferencialData
  message?: string
  error?: string
}

/**
 * Composable para gestionar stock diferencial por empresa
 * Permite obtener, actualizar y resetear stock específico por empresa
 */
export const useStockDiferencial = () => {
  const api = useApi()
  const toast = useToast()
  
  // Estado reactivo
  const loading = ref(false)
  const error = ref<string | null>(null)
  const stockCache = ref<Map<string, StockDiferencialData>>(new Map())
  
  /**
   * Genera clave única para cache basada en empresaId y productoId
   */
  const getCacheKey = (empresaId: number, productoId: number): string => {
    return `${empresaId}-${productoId}`
  }
  
  /**
   * Helper para manejar errores de API
   */
  const handleApiError = (err: any): string => {
    if (err?.message) return err.message
    if (err?.data?.message) return err.data.message
    if (err?.data?.errors) {
      const errors = err.data.errors
      const firstError = Object.values(errors)[0] as string[]
      return firstError[0] || 'Error de validación'
    }
    return err?.statusText || 'Error desconocido'
  }
  
  /**
   * Obtiene el stock específico de una empresa para un producto
   * @param options - Configuración con empresaId y productoId
   * @returns Datos del stock diferencial o null si no existe
   */
  const getStockByEmpresa = async (options: StockDiferencialOptions): Promise<StockDiferencialData | null> => {
    loading.value = true
    error.value = null
    
    try {
      // Verificar cache primero
      const cacheKey = getCacheKey(options.empresaId, options.productoId)
      const cached = stockCache.value.get(cacheKey)
      if (cached) {
        loading.value = false
        return cached
      }
      
      // Llamada a API para obtener producto con stock específico de empresa
      const response = await api.get<ProductoBaseDto>(`/api/ProductosBase/${options.productoId}`, {
        query: { empresaId: options.empresaId }
      })
      
      if (!response) {
        loading.value = false
        return null
      }
      
      // Construir datos de stock diferencial
      const stockData: StockDiferencialData = {
        empresaId: options.empresaId,
        productoId: options.productoId,
        existencia: response.existencia ?? 0,
        esStockDiferencial: true, // Asumimos que si viene con empresaId es diferencial
        stockBase: response.existencia // Para referencia
      }
      
      // Guardar en cache
      stockCache.value.set(cacheKey, stockData)
      
      return stockData
    } catch (err: any) {
      const errorMessage = handleApiError(err)
      error.value = errorMessage
      
      toast.add({
        title: 'Error al obtener stock',
        description: errorMessage,
        color: 'red'
      })
      
      return null
    } finally {
      loading.value = false
    }
  }
  
  /**
   * Actualiza el stock específico de una empresa para un producto
   * @param request - Datos de actualización con empresaId, productoId y nueva existencia
   * @returns Respuesta con resultado de la operación
   */
  const updateStockByEmpresa = async (request: StockDiferencialUpdateRequest): Promise<StockDiferencialResponse> => {
    loading.value = true
    error.value = null
    
    try {
      // Validaciones básicas
      if (request.existencia < 0) {
        throw new Error('El stock no puede ser negativo')
      }
      
      // Llamada a API para actualizar stock específico
      const response = await api.put<ProductoBaseDto>(
        `/api/ProductosBase/${request.productoId}?empresaId=${request.empresaId}`,
        { existencia: request.existencia }
      )
      
      if (!response) {
        throw new Error('No se recibió respuesta del servidor')
      }
      
      // Actualizar cache
      const cacheKey = getCacheKey(request.empresaId, request.productoId)
      const updatedData: StockDiferencialData = {
        empresaId: request.empresaId,
        productoId: request.productoId,
        existencia: request.existencia,
        esStockDiferencial: true,
        stockBase: response.existencia
      }
      stockCache.value.set(cacheKey, updatedData)
      
      toast.add({
        title: 'Stock actualizado',
        description: `Stock actualizado correctamente a ${request.existencia}`,
        color: 'green'
      })
      
      return {
        success: true,
        data: updatedData,
        message: 'Stock actualizado correctamente'
      }
    } catch (err: any) {
      const errorMessage = handleApiError(err)
      error.value = errorMessage
      
      toast.add({
        title: 'Error al actualizar stock',
        description: errorMessage,
        color: 'red'
      })
      
      return {
        success: false,
        error: errorMessage
      }
    } finally {
      loading.value = false
    }
  }
  
  /**
   * Resetea el stock diferencial, volviendo al stock base del producto
   * @param options - Configuración con empresaId y productoId
   * @returns Respuesta con resultado de la operación
   */
  const resetStockDiferencial = async (options: StockDiferencialOptions): Promise<StockDiferencialResponse> => {
    loading.value = true
    error.value = null
    
    try {
      // Obtener stock base del producto (sin empresaId)
      const baseProduct = await api.get<ProductoBaseDto>(`/api/ProductosBase/${options.productoId}`)
      
      if (!baseProduct) {
        throw new Error('No se pudo obtener el producto base')
      }
      
      const stockBase = baseProduct.existencia ?? 0
      
      // Actualizar con el stock base
      const response = await updateStockByEmpresa({
        empresaId: options.empresaId,
        productoId: options.productoId,
        existencia: stockBase
      })
      
      if (response.success) {
        // Limpiar cache
        const cacheKey = getCacheKey(options.empresaId, options.productoId)
        stockCache.value.delete(cacheKey)
        
        toast.add({
          title: 'Stock reseteado',
          description: `Stock reseteado al valor base: ${stockBase}`,
          color: 'green'
        })
      }
      
      return response
    } catch (err: any) {
      const errorMessage = handleApiError(err)
      error.value = errorMessage
      
      toast.add({
        title: 'Error al resetear stock',
        description: errorMessage,
        color: 'red'
      })
      
      return {
        success: false,
        error: errorMessage
      }
    } finally {
      loading.value = false
    }
  }
  
  /**
   * Limpia el cache de stock diferencial
   * @param empresaId - ID de empresa específica (opcional, limpia toda la cache si no se especifica)
   */
  const clearStockCache = (empresaId?: number) => {
    if (empresaId) {
      // Limpiar solo cache de una empresa específica
      const keysToDelete = Array.from(stockCache.value.keys())
        .filter(key => key.startsWith(`${empresaId}-`))
      
      keysToDelete.forEach(key => stockCache.value.delete(key))
    } else {
      // Limpiar toda la cache
      stockCache.value.clear()
    }
  }
  
  /**
   * Obtiene todos los datos de stock en cache para una empresa
   * @param empresaId - ID de la empresa
   * @returns Array de datos de stock diferencial
   */
  const getEmpresaStockCache = (empresaId: number): StockDiferencialData[] => {
    return Array.from(stockCache.value.values())
      .filter(item => item.empresaId === empresaId)
  }
  
  return {
    // Estado reactivo (readonly para proteger mutaciones externas)
    loading: readonly(loading),
    error: readonly(error),
    
    // Métodos principales
    getStockByEmpresa,
    updateStockByEmpresa,
    resetStockDiferencial,
    
    // Utilidades de cache
    clearStockCache,
    getEmpresaStockCache,
    
    // Computadas útiles
    hasError: computed(() => error.value !== null),
    isLoading: computed(() => loading.value),
    cacheSize: computed(() => stockCache.value.size)
  }
}

/**
 * Composable especializado para validaciones de stock diferencial
 * Separado para mantener principio de responsabilidad única
 */
export const useStockDiferencialValidation = () => {
  /**
   * Valida los datos de actualización de stock
   * @param request - Datos a validar
   * @returns Resultado de validación con errores si los hay
   */
  const validateStockUpdate = (request: StockDiferencialUpdateRequest) => {
    const errors: string[] = []
    
    if (!request.empresaId || request.empresaId <= 0) {
      errors.push('ID de empresa es requerido y debe ser mayor a 0')
    }
    
    if (!request.productoId || request.productoId <= 0) {
      errors.push('ID de producto es requerido y debe ser mayor a 0')
    }
    
    if (request.existencia === undefined || request.existencia === null) {
      errors.push('La existencia es requerida')
    } else if (request.existencia < 0) {
      errors.push('La existencia no puede ser negativa')
    } else if (!Number.isInteger(request.existencia)) {
      errors.push('La existencia debe ser un número entero')
    }
    
    return {
      isValid: errors.length === 0,
      errors
    }
  }
  
  /**
   * Valida que un valor sea un stock válido
   * @param stock - Valor a validar
   * @returns true si es válido
   */
  const isValidStock = (stock: any): stock is number => {
    return typeof stock === 'number' && 
           stock >= 0 && 
           Number.isInteger(stock)
  }
  
  return {
    validateStockUpdate,
    isValidStock
  }
}