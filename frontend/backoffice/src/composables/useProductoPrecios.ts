import type { 
  ProductType,
  PrecioListaDto,
  GetPreciosPorProductoQueryResult,
  GetPrecioPorProductoYListaQueryResult,
  UpsertPrecioProductoCommand,
  UpsertPrecioProductoCommandResult,
  UpdatePrecioProductoRequest,
  UpdatePrecioProductoCommandResult
} from '~/types/productos'

export const useProductoPrecios = () => {
  const api = useApi()
  const toast = useToast()

  // Estado reactivo
  const loading = ref(false)
  const error = ref<string | null>(null)

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

  // Generar URLs dinámicas según el tipo de producto
  const getBaseUrl = (type: ProductType) => {
    return type === 'base' ? '/api/productos-precios' : '/api/productos-empresa-precios'
  }
  
  // Helper para obtener el nombre del campo ID según el tipo
  const getProductIdField = (type: ProductType): string => {
    return type === 'base' ? 'producto_id' : 'producto_empresa_id'
  }

  // Obtener todos los precios de un producto
  const fetchProductoPrecios = async (productoId: number, type: ProductType = 'base') => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<GetPreciosPorProductoQueryResult>(
        `${getBaseUrl(type)}/producto/${productoId}`
      )
      return response.precios || []
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar precios',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Obtener precio específico de un producto en una lista
  const fetchPrecioEspecifico = async (
    productoId: number, 
    listaPrecioId: number, 
    type: ProductType = 'base'
  ) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<GetPrecioPorProductoYListaQueryResult>(
        `${getBaseUrl(type)}/producto/${productoId}/lista/${listaPrecioId}`
      )
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      // No mostrar toast para este endpoint ya que puede ser normal que no exista
      throw err
    } finally {
      loading.value = false
    }
  }

  // Crear o actualizar precio de un producto (Upsert)
  const upsertPrecio = async (
    productoId: number,
    listaPrecioId: number,
    precio: number,
    type: ProductType = 'base'
  ) => {
    loading.value = true
    error.value = null
    
    try {
      const command: UpsertPrecioProductoCommand = {
        [getProductIdField(type)]: productoId,
        lista_precio_id: listaPrecioId,
        precio
      }
      
      const response = await api.post<UpsertPrecioProductoCommandResult>(
        `${getBaseUrl(type)}`,
        command
      )
      
      const action = response.was_created ? 'creado' : 'actualizado'
      toast.add({
        title: `Precio ${action}`,
        description: `El precio ha sido ${action} exitosamente`,
        color: 'green'
      })
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al guardar precio',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Actualizar precio existente
  const updatePrecio = async (
    productoId: number,
    listaPrecioId: number,
    precio: number,
    type: ProductType = 'base'
  ) => {
    loading.value = true
    error.value = null
    
    try {
      const request: UpdatePrecioProductoRequest = { precio }
      
      const response = await api.put<UpdatePrecioProductoCommandResult>(
        `${getBaseUrl(type)}/producto/${productoId}/lista/${listaPrecioId}`,
        request
      )
      
      toast.add({
        title: 'Precio actualizado',
        description: 'El precio ha sido actualizado exitosamente',
        color: 'green'
      })
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al actualizar precio',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Crear nuevo precio
  const createPrecio = async (
    productoId: number,
    listaPrecioId: number,
    precio: number,
    type: ProductType = 'base'
  ) => {
    return await upsertPrecio(productoId, listaPrecioId, precio, type)
  }

  // Eliminar precio de una lista específica
  const deletePrecio = async (
    productoId: number,
    listaPrecioId: number,
    type: ProductType = 'base'
  ) => {
    loading.value = true
    error.value = null
    
    try {
      await api.delete(
        `${getBaseUrl(type)}/producto/${productoId}/lista/${listaPrecioId}`
      )
      
      toast.add({
        title: 'Precio eliminado',
        description: 'El precio ha sido eliminado de la lista',
        color: 'green'
      })
      
      return true
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al eliminar precio',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Validar formato de precio
  const validatePrecio = (precio: number | string): boolean => {
    const numericPrice = typeof precio === 'string' ? parseFloat(precio) : precio
    return !isNaN(numericPrice) && numericPrice >= 0
  }

  // Formatear precio para mostrar
  const formatPrecio = (precio: number | null | undefined): string => {
    if (!precio && precio !== 0) return '-'
    return `$${precio.toLocaleString('es-AR', { 
      minimumFractionDigits: 2,
      maximumFractionDigits: 2 
    })}`
  }

  // Parsear precio desde string de input
  const parsePrecio = (precioString: string): number => {
    // Remover caracteres no numéricos excepto punto y coma
    let cleaned = precioString.replace(/[^\d.,]/g, '')
    
    // Detectar formato: si tiene punto y coma, asumimos formato argentino (1.500,00)
    // Si solo tiene punto o solo coma, necesitamos determinar qué es
    const hasDot = cleaned.includes('.')
    const hasComma = cleaned.includes(',')
    
    if (hasDot && hasComma) {
      // Formato argentino: punto para miles, coma para decimales
      cleaned = cleaned.replace(/\./g, '').replace(',', '.')
    } else if (hasComma) {
      // Solo coma: probablemente decimal
      cleaned = cleaned.replace(',', '.')
    }
    // Si solo tiene punto, lo dejamos como está (formato inglés)
    
    return parseFloat(cleaned) || 0
  }

  // Buscar precio en una lista de precios
  const findPrecioInList = (
    precios: PrecioListaDto[], 
    listaPrecioId: number
  ): PrecioListaDto | undefined => {
    return precios.find(precio => precio.lista_precio_id === listaPrecioId)
  }

  // Verificar si un producto tiene precio en una lista específica
  const hasPrecioInLista = (
    precios: PrecioListaDto[], 
    listaPrecioId: number
  ): boolean => {
    return !!findPrecioInList(precios, listaPrecioId)
  }

  // Obtener precio formateado de una lista específica
  const getPrecioFormateado = (
    precios: PrecioListaDto[], 
    listaPrecioId: number
  ): string => {
    const precio = findPrecioInList(precios, listaPrecioId)
    return formatPrecio(precio?.precio)
  }

  return {
    // Estado
    loading: readonly(loading),
    error: readonly(error),
    
    // Acciones CRUD
    fetchProductoPrecios,
    fetchPrecioEspecifico,
    upsertPrecio,
    updatePrecio,
    createPrecio,
    deletePrecio,
    
    // Utilidades
    validatePrecio,
    formatPrecio,
    parsePrecio,
    findPrecioInList,
    hasPrecioInLista,
    getPrecioFormateado
  }
}