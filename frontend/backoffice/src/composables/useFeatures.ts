/**
 * Composable para gestionar feature flags de la aplicación
 * Usa los features que vienen desde el sistema de autenticación
 */
export const useFeatures = () => {
  // Usar features desde el sistema de autenticación
  const authFeatures = useFeaturesFromAuth()
  
  return {
    // Features de productos y categorías de empresa
    empresaProducts: authFeatures.empresaProducts,
    empresaCategories: authFeatures.empresaCategories,
    
    // Feature de autenticación de clientes
    cliente_autenticacion: authFeatures.cliente_autenticacion,
    
    // Helper para verificar si alguna feature de empresa está activa
    hasAnyEmpresaFeature: authFeatures.hasAnyEmpresaFeature,
    
    // Exponer todos los features procesados
    features: authFeatures.features,
    featuresRaw: authFeatures.featuresRaw,
    
    // Helpers
    isFeatureEnabled: authFeatures.isFeatureEnabled,
    getFeatureValue: authFeatures.getFeatureValue,
    getFeatureInfo: authFeatures.getFeatureInfo
  }
}