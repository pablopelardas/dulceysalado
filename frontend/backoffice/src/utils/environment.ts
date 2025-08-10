import type { Environment } from '~/types/environment'

// Importaciones directas de los archivos de configuración
import { environment as devEnvironment } from '~/environments/env.dev'
import { environment as prodEnvironment } from '~/environments/env.prod'

/**
 * Carga la configuración del entorno de forma síncrona
 */
export const getEnvironment = (): Environment => {
  
  // Evitar el uso directo de process.env.NODE_ENV para evitar problemas en el build
  // En su lugar, usar import.meta.env o detectar basado en otros criterios
  let isDevelopment = true
  
  // Detectar entorno basado en import.meta
  if (import.meta.env) {
    isDevelopment = import.meta.env.DEV || import.meta.env.MODE === 'development'
  } else if (typeof process !== 'undefined' && process.env) {
    isDevelopment = process.env.NODE_ENV !== 'production'
  }
  

  let environment = isDevelopment ? devEnvironment : prodEnvironment

  return environment
}