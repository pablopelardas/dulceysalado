import { ref, computed } from 'vue'
import { useApi } from '~/composables/useApi'

interface SyncLog {
  id: number
  archivo_nombre: string
  fecha_procesamiento: string
  productos_actualizados: number
  productos_nuevos: number
  errores: number
  tiempo_procesamiento_ms: number
  estado: string
  usuario_proceso: string
}

interface SyncLogsResponse {
  success: boolean
  logs: SyncLog[]
}

interface DailyStats {
  fecha: string
  syncs: number
  productos: number
  errores: number
  tiempo_promedio_ms: number
}

interface Performance {
  tiempo_minimo_ms: number
  tiempo_maximo_ms: number
  sesiones_con_advertencias: number
  productividad_promedio: number
}

interface Statistics {
  total_syncs: number
  productos_totales: number
  productos_actualizados: number
  productos_nuevos: number
  errores_totales: number
  tiempo_promedio_ms: number
  syncs_exitosos: number
  syncs_con_errores: number
  syncs_fallidos: number
  ultima_sincronizacion: string
  tasa_exito_promedio: number
  productos_por_segundo_promedio: number
  performance: Performance
  estadisticas_diarias: DailyStats[]
}

interface SyncStatsResponse {
  success: boolean
  empresa: string
  periodo_dias: number
  estadisticas: Statistics
  timestamp: string
}

export const useSyncLogs = () => {
  const api = useApi()
  const logs = ref<SyncLog[]>([])
  const stats = ref<SyncStatsResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Agrupar logs por fecha
  const logsByDate = computed(() => {
    const grouped = new Map<string, SyncLog[]>()
    
    logs.value.forEach(log => {
      const date = new Date(log.fecha_procesamiento).toLocaleDateString('es-AR')
      if (!grouped.has(date)) {
        grouped.set(date, [])
      }
      grouped.get(date)!.push(log)
    })
    
    return grouped
  })

  // Obtener estado del día (verde si todos exitosos, rojo si alguno falló)
  const getDayStatus = (date: string): 'success' | 'error' | 'none' => {
    const dayLogs = logsByDate.value.get(date)
    if (!dayLogs || dayLogs.length === 0) return 'none'
    
    const hasErrors = dayLogs.some(log => log.estado !== 'Exitoso' || log.errores > 0)
    return hasErrors ? 'error' : 'success'
  }

  // Obtener resumen del día
  const getDaySummary = (date: string) => {
    const dayLogs = logsByDate.value.get(date) || []
    
    const totalSyncs = dayLogs.length
    const successfulSyncs = dayLogs.filter(log => log.estado === 'Exitoso' && log.errores === 0).length
    const totalProductsUpdated = dayLogs.reduce((sum, log) => sum + log.productos_actualizados, 0)
    const totalProductsNew = dayLogs.reduce((sum, log) => sum + log.productos_nuevos, 0)
    const totalErrors = dayLogs.reduce((sum, log) => sum + log.errores, 0)
    const avgProcessingTime = dayLogs.length > 0 
      ? Math.round(dayLogs.reduce((sum, log) => sum + log.tiempo_procesamiento_ms, 0) / dayLogs.length)
      : 0

    return {
      totalSyncs,
      successfulSyncs,
      totalProductsUpdated,
      totalProductsNew,
      totalErrors,
      avgProcessingTime,
      logs: dayLogs.sort((a, b) => 
        new Date(b.fecha_procesamiento).getTime() - new Date(a.fecha_procesamiento).getTime()
      )
    }
  }

  // Cargar logs
  const fetchLogs = async (take: number = 100) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<SyncLogsResponse>(`/api/sync/logs?take=${take}`)
      
      if (response.success) {
        logs.value = response.logs
      } else {
        throw new Error('Error al cargar los logs')
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Error desconocido'
      console.error('Error fetching sync logs:', err)
    } finally {
      loading.value = false
    }
  }

  // Cargar estadísticas
  const fetchStats = async (days: number = 30) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<SyncStatsResponse>(`/api/sync/stats?days=${days}`)
      
      if (response.success) {
        stats.value = response
      } else {
        throw new Error('Error al cargar las estadísticas')
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Error desconocido'
      console.error('Error fetching sync stats:', err)
    } finally {
      loading.value = false
    }
  }

  return {
    logs,
    stats,
    loading,
    error,
    logsByDate,
    fetchLogs,
    fetchStats,
    getDayStatus,
    getDaySummary
  }
}