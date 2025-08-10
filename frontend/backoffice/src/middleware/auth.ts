export default defineNuxtRouteMiddleware(async (to) => {
  const authStore = useAuthStore()

  // Si ya está autenticado, continuar
  if (authStore.isAuthenticated) {
    return
  }

  // Verificar si hay tokens en cookies
  const accessToken = useCookie('authToken', {
    secure: false, // Temporalmente false para debugging
    httpOnly: false,
    sameSite: 'lax'
  })
  
  const refreshToken = useCookie('refreshToken', {
    secure: false, // Temporalmente false para debugging
    httpOnly: false,
    sameSite: 'lax'
  })

  if (!accessToken.value || !refreshToken.value) {
    return navigateTo('/login')
  }

  // Si hay tokens pero no está autenticado en el store, intentar inicializar
  if (!authStore.isAuthenticated && accessToken.value) {
    
    // Restaurar tokens en el store
    authStore.accessToken = accessToken.value
    authStore.refreshTokenValue = refreshToken.value

    // Intentar verificar tanto en servidor como en cliente
    try {
      await authStore.me()
      
      // Si llegamos aquí, la autenticación fue exitosa
      if (authStore.isAuthenticated) {
        console.log('[Auth Middleware] Autenticación exitosa, continuando...')
        return
      }
    } catch (error) {
      console.error('[Auth Middleware] Error al restaurar sesión:', error)
      
      // En el servidor, solo limpiar el estado sin redireccionar
      if (import.meta.server) {
        console.log('[Auth Middleware] Error en servidor, limpiando estado')
        authStore.clearAuthState()
        return
      }
      
      // En el cliente, limpiar y redireccionar
      authStore.clearAuthState()
      return navigateTo('/login')
    }
  }

  // Si llegamos aquí y aún no está autenticado, redireccionar
  if (!authStore.isAuthenticated) {
    console.log('[Auth Middleware] Usuario no autenticado al final, redirigiendo a /login')
    return navigateTo('/login')
  }
})