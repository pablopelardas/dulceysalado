export default defineNuxtRouteMiddleware((to, from) => {
  const authStore = useAuthStore()

  // Si está autenticado, redireccionar al dashboard
  if (authStore.isAuthenticated) {
    return navigateTo('/')
  }
})