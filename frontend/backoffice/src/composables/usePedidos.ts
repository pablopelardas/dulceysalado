import type { 
  Pedido, 
  PedidosPagedResult, 
  PedidoFiltros, 
  PedidoEstadisticas, 
  RechazarPedidoRequest,
  CorregirPedidoRequest,
  PedidoEstado 
} from '~/types/pedidos'

export const usePedidos = () => {
  const { get, post, put, delete: del } = useApi()
  const toast = useToast()

  // Estados reactivos
  const pedidos = ref<Pedido[]>([])
  const pedido = ref<Pedido | null>(null)
  const estadisticas = ref<PedidoEstadisticas | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalPages = ref(0)
  const totalCount = ref(0)

  // Helper para extraer mensaje de error
  const getErrorMessage = (err: any): string => {
    if (err?.message) return err.message
    if (err?.data?.message) return err.data.message
    if (err?.data?.errors) {
      const errors = err.data.errors
      const firstError = Object.values(errors)[0] as string[]
      return firstError[0] || 'Error de validación'
    }
    return err?.statusText || err?.message || 'Error desconocido'
  }

  // Obtener pedidos con filtros
  const fetchPedidos = async (filtros: PedidoFiltros = {}) => {
    loading.value = true
    error.value = null

    try {
      const params = new URLSearchParams()
      
      if (filtros.page) params.append('page', filtros.page.toString())
      if (filtros.pageSize) params.append('pageSize', filtros.pageSize.toString())
      if (filtros.estado !== undefined) params.append('estado', filtros.estado.toString())
      if (filtros.fechaDesde) params.append('fechaDesde', filtros.fechaDesde)
      if (filtros.fechaHasta) params.append('fechaHasta', filtros.fechaHasta)
      if (filtros.clienteId) params.append('clienteId', filtros.clienteId.toString())
      if (filtros.numeroContiene) params.append('numeroContiene', filtros.numeroContiene)

      const response = await get<PedidosPagedResult>(`/api/backoffice/pedidos?${params}`)
      
      pedidos.value = response.items
      totalPages.value = response.total_pages
      totalCount.value = response.total_count

      return response
    } catch (err: any) {
      error.value = getErrorMessage(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  // Obtener pedido por ID
  const fetchPedidoById = async (id: number) => {
    loading.value = true
    error.value = null

    try {
      const response = await get<Pedido>(`/api/backoffice/pedidos/${id}`)
      pedido.value = response
      return response
    } catch (err: any) {
      error.value = getErrorMessage(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  // Aceptar pedido
  const aceptarPedido = async (id: number) => {
    loading.value = true
    error.value = null

    try {
      await put(`/api/backoffice/pedidos/${id}/aceptar`)

      // Actualizar el pedido en el estado local si existe
      if (pedido.value?.id === id) {
        pedido.value.estado = 'Aceptado'
        pedido.value.fecha_aceptado = new Date().toISOString()
      }

      // Actualizar en la lista si existe
      const index = pedidos.value.findIndex(p => p.id === id)
      if (index !== -1) {
        pedidos.value[index].estado = 'Aceptado'
        pedidos.value[index].fecha_aceptado = new Date().toISOString()
      }

      toast.add({
        title: 'Éxito',
        description: 'Pedido aceptado correctamente',
        color: 'green'
      })

      return true
    } catch (err: any) {
      error.value = getErrorMessage(err)
      toast.add({
        title: 'Error',
        description: error.value,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Rechazar pedido
  const rechazarPedido = async (id: number, motivo: string) => {
    loading.value = true
    error.value = null

    try {
      const request: RechazarPedidoRequest = { motivo }
      
      await put(`/api/backoffice/pedidos/${id}/rechazar`, request)

      // Actualizar el pedido en el estado local si existe
      if (pedido.value?.id === id) {
        pedido.value.estado = 'Rechazado'
        pedido.value.fecha_rechazado = new Date().toISOString()
        pedido.value.motivo_rechazo = motivo
      }

      // Actualizar en la lista si existe
      const index = pedidos.value.findIndex(p => p.id === id)
      if (index !== -1) {
        pedidos.value[index].estado = 'Rechazado'
        pedidos.value[index].fecha_rechazado = new Date().toISOString()
        pedidos.value[index].motivo_rechazo = motivo
      }

      toast.add({
        title: 'Éxito',
        description: 'Pedido rechazado correctamente',
        color: 'green'
      })

      return true
    } catch (err: any) {
      error.value = getErrorMessage(err)
      toast.add({
        title: 'Error',
        description: error.value,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Completar pedido
  const completarPedido = async (id: number) => {
    loading.value = true
    error.value = null

    try {
      await put(`/api/backoffice/pedidos/${id}/completar`)

      // Actualizar el pedido en el estado local si existe
      if (pedido.value?.id === id) {
        pedido.value.estado = 'Completado'
        pedido.value.fecha_completado = new Date().toISOString()
      }

      // Actualizar en la lista si existe
      const index = pedidos.value.findIndex(p => p.id === id)
      if (index !== -1) {
        pedidos.value[index].estado = 'Completado'
        pedidos.value[index].fecha_completado = new Date().toISOString()
      }

      toast.add({
        title: 'Éxito',
        description: 'Pedido completado correctamente',
        color: 'green'
      })

      return true
    } catch (err: any) {
      error.value = getErrorMessage(err)
      toast.add({
        title: 'Error',
        description: error.value,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Corregir pedido
  const corregirPedido = async (id: number, correccion: CorregirPedidoRequest) => {
    loading.value = true
    error.value = null

    try {
      const response = await put(`/api/backoffice/pedidos/${id}/corregir`, correccion)

      // Actualizar el pedido en el estado local si existe
      if (pedido.value?.id === id) {
        pedido.value.estado = correccion.enviar_al_cliente ? 'CorreccionPendiente' : 'EnCorreccion'
        // Actualizar items corregidos
        correccion.items.forEach(itemCorreccion => {
          const item = pedido.value!.items.find(i => i.codigo_producto === itemCorreccion.codigo_producto)
          if (item) {
            if (itemCorreccion.nueva_cantidad === 0) {
              const index = pedido.value!.items.indexOf(item)
              pedido.value!.items.splice(index, 1)
            } else {
              item.cantidad = itemCorreccion.nueva_cantidad
              item.subtotal = item.cantidad * item.precio_unitario
            }
          }
        })
        // Recalcular total
        pedido.value.monto_total = pedido.value.items.reduce((sum, item) => sum + item.subtotal, 0)
      }

      // Actualizar en la lista si existe
      const index = pedidos.value.findIndex(p => p.id === id)
      if (index !== -1) {
        pedidos.value[index].estado = correccion.enviar_al_cliente ? 'CorreccionPendiente' : 'EnCorreccion'
      }

      const mensaje = correccion.enviar_al_cliente 
        ? 'Corrección enviada al cliente exitosamente'
        : 'Pedido corregido exitosamente'

      toast.add({
        title: 'Éxito',
        description: mensaje,
        color: 'green'
      })

      return {
        success: true,
        token: response.token,
        enviado_al_cliente: response.enviado_al_cliente
      }
    } catch (err: any) {
      error.value = getErrorMessage(err)
      toast.add({
        title: 'Error',
        description: error.value,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Obtener estadísticas
  const fetchEstadisticas = async () => {
    loading.value = true
    error.value = null

    try {
      const response = await get<PedidoEstadisticas>('/api/backoffice/pedidos/estadisticas')
      estadisticas.value = response
      return response
    } catch (err: any) {
      error.value = getErrorMessage(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  // Obtener pedidos pendientes
  const fetchPedidosPendientes = async () => {
    loading.value = true
    error.value = null

    try {
      const response = await get<Pedido[]>('/api/backoffice/pedidos/pendientes')
      return response
    } catch (err: any) {
      error.value = getErrorMessage(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  // Helpers para comunicación
  const generarLinkWhatsApp = (telefono: string, mensaje: string = '') => {
    // Limpiar el número de teléfono
    const numeroLimpio = telefono.replace(/\D/g, '')
    const mensajeCodificado = encodeURIComponent(mensaje)
    return `https://wa.me/${numeroLimpio}?text=${mensajeCodificado}`
  }

  const generarLinkEmail = (email: string, asunto: string = '', mensaje: string = '') => {
    const asuntoCodificado = encodeURIComponent(asunto)
    const mensajeCodificado = encodeURIComponent(mensaje)
    return `mailto:${email}?subject=${asuntoCodificado}&body=${mensajeCodificado}`
  }

  // Limpiar estados
  const clearError = () => {
    error.value = null
  }

  const clearPedido = () => {
    pedido.value = null
  }

  const clearPedidos = () => {
    pedidos.value = []
    totalPages.value = 0
    totalCount.value = 0
  }

  return {
    // Estados reactivos
    pedidos: pedidos,
    pedido: pedido,
    estadisticas: estadisticas,
    loading: loading,
    error: error,
    totalPages: totalPages,
    totalCount: totalCount,

    // Métodos principales
    fetchPedidos,
    fetchPedidoById,
    aceptarPedido,
    rechazarPedido,
    completarPedido,
    corregirPedido,
    fetchEstadisticas,
    fetchPedidosPendientes,

    // Helpers
    generarLinkWhatsApp,
    generarLinkEmail,

    // Utilidades
    clearError,
    clearPedido,
    clearPedidos
  }
}