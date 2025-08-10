import type {
  ProductoBaseDto,
  GetAllProductosBaseQueryResult,
  GetProductoBaseByIdQueryResult,
  GetProductoBaseByCodigoQueryResult,
  CreateProductoBaseCommand,
  CreateProductoBaseCommandResult,
  UpdateProductoBaseCommand,
  UpdateProductoBaseCommandResult,
  ProductosBaseFilters
} from '~/types/productos'

export const useProductosBase = () => {
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
  
  // Estado reactivo para lista de productos
  const productos = ref<ProductoBaseDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  // Estado para listas de precios
  const listaSeleccionada = ref<any>(null)
  const listasDisponibles = ref<any[]>([])
  
  // Estado de paginación
  const pagination = ref({
    page: 1,
    limit: 20,
    total: 0,
    pages: 0
  })
  
  // Filtros activos
  const filters = ref<ProductosBaseFilters>({
    busqueda: '',
    codigoRubro: undefined,
    visible: undefined,
    destacado: undefined,
    page: 1,
    pageSize: 20,
    sortBy: 'codigo',
    sortOrder: 'asc',
    listaPrecioId: undefined
  })
  
  // Obtener lista de productos base con soporte para empresaId
  const fetchProductos = async (customFilters?: ProductosBaseFilters, empresaId?: number) => {
    console.log('fetchProductos called with empresaId:', empresaId)
    
    loading.value = true
    error.value = null
    
    try {
      // Actualizar filtros activos ANTES de hacer la petición
      if (customFilters) {
        filters.value = { ...filters.value, ...customFilters }
      }
      
      // Combinar filtros
      const queryFilters: any = {
        ...filters.value,
        ...customFilters
      }
      
      // Agregar empresaId si se proporciona
      if (empresaId) {
        queryFilters.empresaId = empresaId
        console.log('Adding empresaId to query:', empresaId)
      }
      
      // Limpiar filtros vacíos (pero mantener empresaId aunque sea 0)
      Object.keys(queryFilters).forEach(key => {
        const value = queryFilters[key]
        if ((value === '' || value === undefined) && key !== 'empresaId') {
          delete queryFilters[key]
        }
      })
      
      console.log('Final queryFilters:', queryFilters)
      
      const response = await api.get<GetAllProductosBaseQueryResult>('/api/ProductosBase', {
        query: queryFilters
      })
      
      productos.value = response.productos || []
      
      // Manejar paginación con diferentes formatos posibles de la API
      pagination.value = {
        page: response.page || 1,
        limit: response.page_size || response.page_size || 20,
        total: response.total || response.total || 0,
        pages: response.total_pages || response.total_pages || Math.ceil((response.total || 0) / (response.page_size || response.page_size || 20))
      }

      // Actualizar información de listas de precios si está disponible
      if (response.lista_seleccionada) {
        listaSeleccionada.value = response.lista_seleccionada
      }
      if (response.listas_disponibles) {
        listasDisponibles.value = response.listas_disponibles
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar productos',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Obtener un producto específico
  const fetchProducto = async (id: number) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<GetProductoBaseByIdQueryResult>(`/api/ProductosBase/${id}`)
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar producto',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Obtener producto por código
  const fetchProductoByCodigo = async (codigo: number) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<GetProductoBaseByCodigoQueryResult>(`/api/ProductosBase/by-codigo/${codigo}`)
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar producto',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Crear nuevo producto
  const createProducto = async (productoData: CreateProductoBaseCommand) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.post<CreateProductoBaseCommandResult>('/api/ProductosBase', productoData)
      
      toast.add({
        title: 'Producto creado',
        description: 'El producto ha sido creado exitosamente',
        color: 'green'
      })
      
      // Refrescar lista
      await fetchProductos()
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al crear producto',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Actualizar producto con soporte para empresaId
  const updateProducto = async (id: number, productoData: UpdateProductoBaseCommand, empresaId?: number) => {
    loading.value = true
    error.value = null
    
    try {
      // Construir URL con empresaId si se proporciona
      const url = empresaId 
        ? `/api/ProductosBase/${id}?empresaId=${empresaId}`
        : `/api/ProductosBase/${id}`
      
      const response = await api.put<UpdateProductoBaseCommandResult>(url, productoData)
      
      toast.add({
        title: 'Producto actualizado',
        description: 'El producto ha sido actualizado exitosamente',
        color: 'green'
      })
      
      // Actualizar en la lista local
      const index = productos.value.findIndex(p => p.id === id)
      if (index !== -1 && response) {
        productos.value[index] = response
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al actualizar producto',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Eliminar producto
  const deleteProducto = async (id: number) => {
    loading.value = true
    error.value = null
    
    try {
      await api.delete(`/api/ProductosBase/${id}`)
      
      toast.add({
        title: 'Producto eliminado',
        description: 'El producto ha sido eliminado exitosamente',
        color: 'green'
      })
      
      // Refrescar lista
      await fetchProductos()
      
      return true
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al eliminar producto',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }
  
  // Cambiar página
  const changePage = async (page: number) => {
    await fetchProductos({ ...filters.value, page })
  }
  
  // Aplicar filtros
  const applyFilters = async (newFilters: ProductosBaseFilters) => {
    await fetchProductos({ ...newFilters, page: 1 })
  }
  
  // Limpiar filtros
  const clearFilters = async () => {
    const defaultFilters = {
      busqueda: '',
      codigoRubro: undefined,
      visible: undefined,
      destacado: undefined,
      empresaId: filters.value.empresaId, // Preserve empresaId
      page: 1,
      pageSize: 20,
      sortBy: 'codigo',
      sortOrder: 'asc' as const
    }
    filters.value = defaultFilters
    await fetchProductos(defaultFilters)
  }
  
  // Aplicar ordenamiento
  const applySorting = async (sortBy: string, sortOrder: 'asc' | 'desc') => {
    // Actualizar filtros inmediatamente para que el componente vea el cambio
    filters.value = { ...filters.value, sortBy, sortOrder }
    await fetchProductos({ ...filters.value, page: 1 })
  }

  // Cambiar lista de precios seleccionada
  const changeListaPrecios = async (listaPrecioId: number | null) => {
    await fetchProductos({ ...filters.value, listaPrecioId, page: 1 })
  }

  // Obtener precio de un producto en la lista seleccionada
  const getPrecioSeleccionado = (producto: ProductoBaseDto): number | null => {
    return producto.precio_seleccionado || null
  }

  // Verificar si un producto tiene precio en la lista seleccionada
  const hasPrecioEnListaSeleccionada = (producto: ProductoBaseDto): boolean => {
    return producto.precio_seleccionado !== null && producto.precio_seleccionado !== undefined
  }
  
  // Subir imagen para un producto
  const uploadProductoImage = async (id: number, file: File) => {
    loading.value = true
    error.value = null
    
    try {
      const formData = new FormData()
      formData.append('image', file)
      
      const response = await api.post(`/api/ProductosBase/${id}/upload-image`, formData)
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al subir imagen',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Eliminar imagen de un producto
  const deleteProductoImage = async (id: number) => {
    loading.value = true
    error.value = null
    
    try {
      await api.delete(`/api/ProductosBase/${id}/image`)
      
      return true
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      throw err
    } finally {
      loading.value = false
    }
  }

  // Verificar si un campo será sobrescrito por sincronización
  const isSyncField = (fieldName: string): boolean => {
    const syncFields = [
      'descripcion',
      'codigo_rubro',
      'precio',
      'existencia',
      'grupo1',
      'grupo2', 
      'grupo3',
      'fechaAlta',
      'fechaModi',
      'imputable',
      'disponible',
      'codigoUbicacion'
    ]
    return syncFields.includes(fieldName.toLowerCase()) || syncFields.includes(fieldName)
  }
  
  // Método específico para obtener productos con stock por empresa
  const fetchProductosConStockEmpresa = async (empresaId: number, customFilters?: ProductosBaseFilters) => {
    return await fetchProductos(customFilters, empresaId)
  }
  
  // Método específico para actualizar solo el stock de un producto por empresa
  const updateStockProductoEmpresa = async (id: number, empresaId: number, existencia: number) => {
    return await updateProducto(id, { existencia } as UpdateProductoBaseCommand, empresaId)
  }
  
  return {
    // Estado - NO usar readonly para filters si queremos que sea reactivo
    productos: readonly(productos),
    loading: readonly(loading),
    error: readonly(error),
    pagination: readonly(pagination),
    filters: filters, // Cambiado: sin readonly para mantener reactividad
    
    // Estado de listas de precios
    listaSeleccionada: readonly(listaSeleccionada),
    listasDisponibles: readonly(listasDisponibles),
    
    // Acciones
    fetchProductos,
    fetchProducto,
    fetchProductoByCodigo,
    createProducto,
    updateProducto,
    deleteProducto,
    
    // Gestión de imágenes
    uploadProductoImage,
    deleteProductoImage,
    
    // Navegación y filtros
    changePage,
    applyFilters,
    clearFilters,
    applySorting,
    
    // Gestión de listas de precios
    changeListaPrecios,
    getPrecioSeleccionado,
    hasPrecioEnListaSeleccionada,
    
    // Helpers
    isSyncField,
    
    // Métodos para stock diferencial por empresa
    fetchProductosConStockEmpresa,
    updateStockProductoEmpresa
  }
}