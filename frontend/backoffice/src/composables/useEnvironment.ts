/**
 * Composable para gestionar la configuración del entorno
 * Selecciona automáticamente entre dev y prod según NODE_ENV
 */
export const useEnvironment = () => {
  // Determinar entorno basado en NODE_ENV
  const isDevelopment = process.env.NODE_ENV !== 'production'
  
  // Importación dinámica basada en el entorno
  const getEnvironment = async () => {
    if (isDevelopment) {
      const { environment } = await import('~/environments/env.dev')
      return environment
    } else {
      const { environment } = await import('~/environments/env.prod')
      return environment
    }
  }
  
  return {
    getEnvironment,
    isDevelopment,
    isProduction: !isDevelopment
  }
}