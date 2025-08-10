import type {
  AgrupacionWithVisibility,
  EmpresaAgrupacionesResponse,
  ConfigureEmpresaVisibilityCommand,
  BulkConfigureVisibilityCommand,
  BulkConfigureVisibilityResult,
  DragDropAgrupacion,
  DragDropState
} from '~/types/agrupaciones'
import { parseApiError } from '~/utils/errorParser'

export const useAgrupacionesEmpresa = () => {
  const api = useApi()
  const toast = useToast()

  // Estado reactivo
  const agrupacionesEmpresa = ref<AgrupacionWithVisibility[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const currentEmpresaId = ref<number | null>(null)

  // Helper para extraer mensaje de error
  const getErrorMessage = (err: any): string => {
    const parsed = parseApiError(err)
    return parsed.message
  }

  // Obtener agrupaciones con estado de visibilidad para una empresa
  const fetchAgrupacionesEmpresa = async (empresaId: number) => {
    loading.value = true
    error.value = null
    currentEmpresaId.value = empresaId
    
    try {
      const response = await api.get<EmpresaAgrupacionesResponse>(`/api/empresas/${empresaId}/agrupaciones`)
      
      if (response.agrupaciones) {
        agrupacionesEmpresa.value = response.agrupaciones
      } else {
        agrupacionesEmpresa.value = []
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar agrupaciones de empresa',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Configurar visibilidad para una empresa específica
  const updateVisibilidadEmpresa = async (empresaId: number, agrupacionesIds: number[]) => {
    loading.value = true
    error.value = null
    
    try {
      const data: ConfigureEmpresaVisibilityCommand = {
        agrupaciones_ids: agrupacionesIds
      }
      
      const response = await api.put<any>(`/api/empresas/${empresaId}/agrupaciones`, data)
      
      toast.add({
        title: 'Configuración actualizada',
        description: `Se han configurado ${agrupacionesIds.length} agrupaciones para la empresa`,
        color: 'green'
      })
      
      // Actualizar estado local
      agrupacionesEmpresa.value = agrupacionesEmpresa.value.map(agrupacion => ({
        ...agrupacion,
        visible: agrupacionesIds.includes(agrupacion.id)
      }))
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al actualizar configuración',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Configuración masiva para múltiples empresas (bulk update)
  const bulkUpdateVisibilidad = async (configuraciones: BulkConfigureVisibilityCommand) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.put<BulkConfigureVisibilityResult>('/api/empresas/agrupaciones/bulk', configuraciones)
      
      const { empresas_exitosas, empresas_con_errores } = response
      
      if (empresas_con_errores > 0) {
        toast.add({
          title: 'Configuración parcialmente exitosa',
          description: `${empresas_exitosas} empresas actualizadas, ${empresas_con_errores} con errores`,
          color: 'orange'
        })
      } else {
        toast.add({
          title: 'Configuración masiva exitosa',
          description: `Se actualizaron ${empresas_exitosas} empresas correctamente`,
          color: 'green'
        })
      }
      
      return response
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error en configuración masiva',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Preparar datos para drag & drop
  const prepareDragDropData = (): DragDropState => {
    const visibles: DragDropAgrupacion[] = []
    const noVisibles: DragDropAgrupacion[] = []
    
    agrupacionesEmpresa.value.forEach(agrupacion => {
      const dragDropItem: DragDropAgrupacion = {
        id: agrupacion.id,
        codigo: agrupacion.codigo,
        nombre: agrupacion.nombre,
        descripcion: agrupacion.descripcion,
        activa: agrupacion.activa
      }
      
      if (agrupacion.visible) {
        visibles.push(dragDropItem)
      } else {
        noVisibles.push(dragDropItem)
      }
    })
    
    // Ordenar por código
    visibles.sort((a, b) => a.codigo - b.codigo)
    noVisibles.sort((a, b) => a.codigo - b.codigo)
    
    return {
      visibles,
      noVisibles,
      loading: loading.value,
      hasChanges: false
    }
  }

  // Aplicar cambios desde drag & drop
  const applyDragDropChanges = async (empresaId: number, visibleIds: number[]) => {
    await updateVisibilidadEmpresa(empresaId, visibleIds)
  }

  // Obtener agrupaciones visibles para una empresa
  const getVisibleAgrupaciones = () => {
    return agrupacionesEmpresa.value.filter(agrupacion => agrupacion.visible)
  }

  // Obtener agrupaciones no visibles para una empresa
  const getNoVisibleAgrupaciones = () => {
    return agrupacionesEmpresa.value.filter(agrupacion => !agrupacion.visible)
  }

  // Contar agrupaciones por estado
  const getStats = () => {
    const total = agrupacionesEmpresa.value.length
    const visibles = agrupacionesEmpresa.value.filter(a => a.visible).length
    const noVisibles = total - visibles
    const activas = agrupacionesEmpresa.value.filter(a => a.activa).length
    
    return {
      total,
      visibles,
      noVisibles,
      activas,
      inactivas: total - activas
    }
  }

  // Buscar agrupaciones
  const searchAgrupaciones = (query: string) => {
    if (!query.trim()) return agrupacionesEmpresa.value
    
    const searchTerm = query.toLowerCase()
    return agrupacionesEmpresa.value.filter(agrupacion =>
      agrupacion.nombre.toLowerCase().includes(searchTerm) ||
      agrupacion.codigo.toString().includes(searchTerm) ||
      (agrupacion.descripcion && agrupacion.descripcion.toLowerCase().includes(searchTerm))
    )
  }

  // Filtrar por visibilidad
  const filterByVisibility = (visible?: boolean) => {
    if (visible === undefined) return agrupacionesEmpresa.value
    return agrupacionesEmpresa.value.filter(agrupacion => agrupacion.visible === visible)
  }

  // Filtrar por estado activo
  const filterByStatus = (activa?: boolean) => {
    if (activa === undefined) return agrupacionesEmpresa.value
    return agrupacionesEmpresa.value.filter(agrupacion => agrupacion.activa === activa)
  }

  // Limpiar estado
  const clearState = () => {
    agrupacionesEmpresa.value = []
    error.value = null
    currentEmpresaId.value = null
  }

  // Refrescar datos
  const refresh = async () => {
    if (currentEmpresaId.value) {
      await fetchAgrupacionesEmpresa(currentEmpresaId.value)
    }
  }

  // Validar si una empresa tiene configuración
  const hasConfiguration = () => {
    return agrupacionesEmpresa.value.some(agrupacion => agrupacion.visible)
  }

  return {
    // Estado
    agrupacionesEmpresa: readonly(agrupacionesEmpresa),
    loading: readonly(loading),
    error: readonly(error),
    currentEmpresaId: readonly(currentEmpresaId),
    
    // Métodos principales
    fetchAgrupacionesEmpresa,
    updateVisibilidadEmpresa,
    bulkUpdateVisibilidad,
    
    // Métodos para drag & drop
    prepareDragDropData,
    applyDragDropChanges,
    
    // Métodos auxiliares
    getVisibleAgrupaciones,
    getNoVisibleAgrupaciones,
    getStats,
    searchAgrupaciones,
    filterByVisibility,
    filterByStatus,
    hasConfiguration,
    clearState,
    refresh
  }
}