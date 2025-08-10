import type { Environment } from '~/types/environment'

export const environment: Environment = {
  production: true,
  // apiBaseUrl: 'https://api.districatalogo.com',
  apiBaseUrl: 'https://api.dulceysaladomax.com',
  features: {
    empresaProducts: false,
    empresaCategories: false,
    cliente_autenticacion: true
  }
}