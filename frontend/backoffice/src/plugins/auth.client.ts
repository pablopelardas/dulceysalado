export default defineNuxtPlugin(async () => {
  const authStore = useAuthStore()
  
  // Solo ejecutar en el cliente
  if (import.meta.server) {
    return
  }

  // Verificar si hay tokens en las cookies
  const accessToken = useCookie('authToken', {
    secure: false,
    httpOnly: false,
    sameSite: 'lax'
  })
  
  const refreshToken = useCookie('refreshToken', {
    secure: false,
    httpOnly: false,
    sameSite: 'lax'
  })


  if (accessToken.value && refreshToken.value) {
    // Restaurar tokens en el store
    authStore.accessToken = accessToken.value
    authStore.refreshTokenValue = refreshToken.value

    try {
      // Intentar obtener información del usuario
      await authStore.me()
    } catch (error) {
      console.error('[Auth Plugin] Error en me(), intentando refresh...', error)
      // Si falla (token expirado), intentar refresh
      try {
        await authStore.refreshToken()
        // Si el refresh es exitoso, intentar obtener usuario de nuevo
        await authStore.me()
        console.log('[Auth Plugin] Refresh exitoso, usuario verificado')
      } catch (refreshError) {
        console.error('[Auth Plugin] Error en refresh, limpiando sesión...', refreshError)
        // Si el refresh también falla, limpiar todo
        authStore.logout()
      }
    }
  } else {
    // Si llegamos aquí y el store dice que está autenticado (por el middleware server)
    // pero no hay cookies, limpiar el estado
    if (authStore.isAuthenticated) {
      console.log('[Auth Plugin] Store dice autenticado pero no hay cookies, limpiando...')
      authStore.clearAuthState()
    }
  }
})