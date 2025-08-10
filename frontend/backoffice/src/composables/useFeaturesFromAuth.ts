import type { FeatureFlag } from '~/types/auth'

/**
 * Composable para obtener features desde el sistema de autenticación
 * Usa los features que vienen en la respuesta de login/me
 */
export const useFeaturesFromAuth = () => {
  const { empresa } = useAuth()

  // Helper para procesar features en un objeto más fácil de usar
  const processedFeatures = computed(() => {
    if (!empresa.value?.features) return {}
    
    const processed: Record<string, boolean | string | number> = {}
    
    for (const feature of empresa.value.features) {
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
    if (!empresa.value?.features) return null
    return empresa.value.features.find(f => f.codigo === featureCode) || null
  }

  // Features específicos para compatibilidad con el sistema anterior
  const empresaProducts = computed(() => isFeatureEnabled('empresa_products'))
  const empresaCategories = computed(() => isFeatureEnabled('empresa_categories'))
  const cliente_autenticacion = computed(() => isFeatureEnabled('cliente_autenticacion'))

  return {
    // Estado
    features: processedFeatures,
    featuresRaw: computed(() => empresa.value?.features || []),
    
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