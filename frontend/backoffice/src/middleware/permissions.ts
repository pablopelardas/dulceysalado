export default defineNuxtRouteMiddleware(async (to, from) => {
  const authStore = useAuthStore()
  const features = useFeatures()

  // Si ya está autenticado, continuar con verificación de permisos
  if (authStore.isAuthenticated) {
  } else {
    // Verificar si hay tokens en cookies para restaurar sesión
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


    if (!accessToken.value || !refreshToken.value) {
      return navigateTo('/login')
    }

    // Restaurar tokens en el store
    if (!authStore.isAuthenticated && accessToken.value) {
      
      authStore.accessToken = accessToken.value
      authStore.refreshTokenValue = refreshToken.value

      // Intentar verificar sesión
      try {
        await authStore.me()
        
        if (!authStore.isAuthenticated) {
          return navigateTo('/login')
        }
        
      } catch (error) {
        console.error('[Permissions Middleware] Error al restaurar sesión:', error)
        
        // En el servidor, solo limpiar el estado
        if (import.meta.server) {
          console.log('[Permissions Middleware] Error en servidor, limpiando estado')
          authStore.clearAuthState()
          return
        }
        
        // En el cliente, limpiar y redireccionar
        authStore.clearAuthState()
        return navigateTo('/login')
      }
    }
  }

  const user = authStore.user
  const userPermissions = authStore.userPermissions

  // Definir permisos requeridos por ruta
  const routePermissions: Record<string, string[]> = {
    '/admin/productos-base': ['canManageProductosBase'],
    '/admin/categorias-base': ['canManageCategoriasBase'],
    '/admin/empresas-cliente': ['canManageUsuarios'], // Solo empresa principal
    '/admin/agrupaciones': ['canManageProductosBase'], // Solo empresa principal
    '/productos-empresa': ['canManageProductosEmpresa'],
    '/categorias-empresa': ['canManageCategoriasEmpresa'],
    '/users': ['canManageUsuarios'], // Cambiado de /usuarios a /users
    '/estadisticas': ['canViewEstadisticas'],
    '/reportes': ['canViewEstadisticas']
  }

  // Rutas que requieren ser empresa principal
  const empresaPrincipalRoutes = [
    '/admin/productos-base',
    '/admin/categorias-base', 
    '/admin/empresas-cliente',
    '/admin/agrupaciones',
    '/reportes'
  ]

  // Rutas específicas para empresas cliente (ya no hay restricciones)
  const empresaClienteRoutes: string[] = []

  const currentPath = to.path

  // Verificar si las features están habilitadas para rutas de empresa
  const empresaProductsRoutes = ['/admin/productos-empresa', '/productos-empresa']
  const empresaCategoriesRoutes = ['/admin/categorias-empresa', '/categorias-empresa']

  if (empresaProductsRoutes.some(route => currentPath.startsWith(route)) && !features.empresaProducts) {
    throw createError({
      statusCode: 404,
      statusMessage: 'Funcionalidad no disponible'
    })
  }

  if (empresaCategoriesRoutes.some(route => currentPath.startsWith(route)) && !features.empresaCategories) {
    throw createError({
      statusCode: 404,
      statusMessage: 'Funcionalidad no disponible'
    })
  }

  // Verificar si la ruta requiere ser empresa principal
  if (empresaPrincipalRoutes.some(route => currentPath.startsWith(route))) {
    if (!authStore.isEmpresaPrincipal) {
      throw createError({
        statusCode: 403,
        statusMessage: 'Acceso denegado - Solo empresa principal'
      })
    }
  }

  // Verificar si la ruta es específica para empresa cliente
  if (empresaClienteRoutes.some(route => currentPath.startsWith(route))) {
    if (authStore.isEmpresaPrincipal) {
      throw createError({
        statusCode: 403,
        statusMessage: 'Acceso denegado - Solo empresas cliente pueden acceder a esta configuración'
      })
    }
  }

  // Verificar permisos específicos de la ruta
  for (const [route, permissions] of Object.entries(routePermissions)) {
    if (currentPath.startsWith(route)) {
      const hasPermission = permissions.some(permission => 
        userPermissions[permission as keyof typeof userPermissions]
      )
      
      // Debug logging
      console.log('🔒 Verificando permisos:', {
        ruta: currentPath,
        permisoRequerido: permissions,
        permisosUsuario: userPermissions,
        tienePermiso: hasPermission,
        usuario: user?.email
      })
      
      if (!hasPermission) {
        // Crear query parameters con la información del error
        const queryParams = new URLSearchParams({
          route: currentPath,
          required: permissions.join(','),
          message: `Permisos insuficientes para acceder a ${currentPath}`
        })
        
        return navigateTo(`/error/403?${queryParams.toString()}`)
      }
      break
    }
  }

  // Verificar rol de admin para rutas de gestión críticas
  const adminOnlyRoutes = ['/admin/empresas-cliente', '/admin/usuarios']
  
  if (adminOnlyRoutes.some(route => currentPath.startsWith(route))) {
    if (user?.rol !== 'admin') {
      throw createError({
        statusCode: 403,
        statusMessage: 'Acceso denegado - Se requiere rol de administrador'
      })
    }
  }
})