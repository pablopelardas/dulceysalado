import type { Environment } from '~/types/environment'

export const environment: Environment = {
  production: false,
  // apiBaseUrl: 'https://api.districatalogo.com',
  apiBaseUrl: 'https://localhost:7000',
  features: {
    empresaProducts: false,
    empresaCategories: false,
    cliente_autenticacion: false
  }
}