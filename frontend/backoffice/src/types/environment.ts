export interface Environment {
  production: boolean
  apiBaseUrl: string
  features: {
    empresaProducts: boolean
    empresaCategories: boolean
    cliente_autenticacion: boolean
  }
}

declare module '#app' {
  interface NuxtApp {
    $environment: Environment
  }
}

declare module 'vue' {
  interface ComponentCustomProperties {
    $environment: Environment
  }
}