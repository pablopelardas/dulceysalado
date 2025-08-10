export default defineNuxtRouteMiddleware((to) => {
  // Obtener el feature flag requerido desde los meta de la página
  const requiredFeature = to.meta?.featureFlag as string
  
  if (!requiredFeature) {
    // Si no hay feature flag definido, permitir acceso
    return
  }
  
  // Obtener empresa directamente desde auth para verificar features
  const { empresa } = useAuth()
  
  // Si no hay empresa cargada aún, permitir que continue (se manejará en auth middleware)
  if (!empresa.value) {
    return
  }
  
  // Buscar el feature en los features de la empresa
  const feature = empresa.value.features?.find(f => f.codigo === requiredFeature)
  const isFeatureEnabled = feature?.habilitado || false
  
  console.log('Feature flag middleware:', {
    requiredFeature,
    feature,
    isFeatureEnabled
  })
  
  if (!isFeatureEnabled) {
    // Si el feature no está habilitado, mostrar error 404
    throw createError({
      statusCode: 404,
      statusMessage: 'Página no encontrada'
    })
  }
})