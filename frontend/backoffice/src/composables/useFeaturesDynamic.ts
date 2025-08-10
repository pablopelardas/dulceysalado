import { ref, computed } from 'vue'

interface FeatureFlag {
  codigo: string
  nombre: string
  descripcion: string
  habilitado: boolean
  valor: any
  tipo_valor: string
  categoria: string
  metadata: any
  updated_at: string
  updated_by: string | null
}

interface FeatureFlags {
  [key: string]: boolean | string | number
}

/**
 * Composable para gestionar feature flags dinámicos desde la API
 * Obtiene la configuración real de la base de datos
 */
export const useFeaturesDynamic = () => {
  const api = useApi()
  
  // Estado reactivo
  const featuresData = ref<FeatureFlag[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const loaded = ref(false)

  // Helper para procesar features en un objeto más fácil de usar
  const processedFeatures = computed<FeatureFlags>(() => {
    const processed: FeatureFlags = {}
    
    for (const feature of featuresData.value) {
      // Solo incluir features habilitados
      if (feature.habilitado) {
        // Usar el valor si existe, sino usar true para boolean
        if (feature.valor !== null && feature.valor !== undefined) {
          processed[feature.codigo] = feature.valor
        } else {
          // Para features tipo boolean sin valor específico, usar habilitado
          processed[feature.codigo] = feature.tipo_valor === 'Boolean' ? true : feature.valor
        }
      }
    }
    
    return processed
  })

  // Obtener features desde la API
  const fetchFeatures = async () => {
    if (loading.value) return // Evitar múltiples peticiones simultáneas
    
    loading.value = true
    error.value = null
    
    try {
      // Endpoint para obtener configuración de features
      // Ajusta la ruta según tu API
      const response = await api.get<FeatureFlag[]>('/api/configuracion/features')
      featuresData.value = response || []
      loaded.value = true
      
      return processedFeatures.value
    } catch (err: any) {
      error.value = err.message || 'Error al cargar features'
      console.error('Error cargando features:', err)
      
      // En caso de error, usar features por defecto o vacío
      featuresData.value = []
      return {}
    } finally {
      loading.value = false
    }
  }

  // Verificar si un feature específico está habilitado
  const isFeatureEnabled = (featureCode: string): boolean => {
    return !!processedFeatures.value[featureCode]
  }

  // Obtener el valor de un feature
  const getFeatureValue = (featureCode: string): any => {
    return processedFeatures.value[featureCode] || null
  }

  // Obtener información completa de un feature
  const getFeatureInfo = (featureCode: string): FeatureFlag | null => {
    return featuresData.value.find(f => f.codigo === featureCode) || null
  }

  // Features específicos calculados (compatibilidad con sistema anterior)
  const empresaProducts = computed(() => isFeatureEnabled('empresa_products'))
  const empresaCategories = computed(() => isFeatureEnabled('empresa_categories'))
  const cliente_autenticacion = computed(() => isFeatureEnabled('cliente_autenticacion'))

  return {
    // Estado
    loading: readonly(loading),
    error: readonly(error),
    loaded: readonly(loaded),
    featuresData: readonly(featuresData),
    features: processedFeatures,

    // Acciones
    fetchFeatures,
    
    // Helpers
    isFeatureEnabled,
    getFeatureValue,
    getFeatureInfo,

    // Features específicos (compatibilidad)
    empresaProducts,
    empresaCategories,
    cliente_autenticacion,

    // Helper para verificar si alguna feature de empresa está activa
    hasAnyEmpresaFeature: computed(() => 
      empresaProducts.value || empresaCategories.value
    )
  }
}