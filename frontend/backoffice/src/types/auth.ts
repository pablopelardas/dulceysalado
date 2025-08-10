export interface Usuario {
  id: number
  empresa_id: number
  email: string
  nombre: string
  apellido: string
  rol: 'admin' | 'editor' | 'viewer'
  puede_gestionar_productos_base: boolean
  puede_gestionar_productos_empresa: boolean
  puede_gestionar_categorias_base: boolean
  puede_gestionar_categorias_empresa: boolean
  puede_gestionar_usuarios: boolean
  puede_ver_estadisticas: boolean
  activo: boolean
  ultimo_login: string
  created_at: string
  updated_at: string
  empresa: Empresa
}

export interface FeatureFlag {
  codigo: string
  nombre: string
  descripcion: string
  tipo_valor: string
  categoria: string
  habilitado: boolean
  valor: any
  metadata: any
  updated_at: string
  updated_by: string | null
}

export interface Empresa {
  id: number
  codigo: string
  nombre: string
  razon_social: string | null
  cuit: string | null
  telefono: string | null
  email: string | null
  direccion: string | null
  tipo_empresa: 'principal' | 'cliente'
  empresa_principal_id: number | null
  logo_url: string | null
  colores_tema: {
    acento: string
    primario: string
    secundario: string
  }
  favicon_url: string | null
  dominio_personalizado: string
  url_whatsapp: string | null
  url_facebook: string | null
  url_instagram: string | null
  mostrar_precios: boolean
  mostrar_stock: boolean
  permitir_pedidos: boolean
  productos_por_pagina: number
  puede_agregar_productos: boolean
  puede_agregar_categorias: boolean
  lista_precio_predeterminada_id: number | null
  activa: boolean
  fecha_vencimiento: string | null
  plan: string // Genérico para no limitar valores futuros
  created_at: string
  updated_at: string
  features?: FeatureFlag[] // Features dinámicos de la empresa
}

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  message: string
  user: Usuario
  empresa: Empresa
  access_token: string
  refresh_token: string
  expiresIn: string
}

export interface RefreshTokenRequest {
  refresh_token: string
}

export interface RefreshTokenResponse {
  message: string
  access_token: string
  refresh_token: string
  expiresIn: string
}

export interface AuthState {
  user: Usuario | null
  empresa: Empresa | null
  accessToken: string | null
  refreshTokenValue: string | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
}

export interface ApiError {
  error: string
  message: string
  code?: string
  originalError?: any
}

// Tipos específicos para gestión de empresas cliente
export interface CompaniesListResponse {
  empresas: Empresa[]
  pagination: {
    page: number
    limit: number
    total: number
    pages: number
  }
}

export interface CreateCompanyRequest {
  codigo: string
  nombre: string
  razon_social?: string
  cuit?: string
  telefono?: string
  email?: string
  direccion?: string
  dominio_personalizado: string
  logo_url?: string
  colores_tema?: {
    primario: string
    secundario: string
    acento: string
  }
  favicon_url?: string
  url_whatsapp?: string
  url_facebook?: string
  url_instagram?: string
  mostrar_precios?: boolean
  mostrar_stock?: boolean
  permitir_pedidos?: boolean
  productos_por_pagina?: number
  puede_agregar_productos?: boolean
  puede_agregar_categorias?: boolean
  plan?: string
  fecha_vencimiento?: string
  lista_precio_predeterminada_id?: number | null
  requesting_user_id: number
}

export interface UpdateCompanyRequest {
  company_id: number
  codigo?: string
  nombre?: string
  razon_social?: string
  cuit?: string
  telefono?: string
  email?: string
  direccion?: string
  logo_url?: string
  colores_tema?: {
    primario: string
    secundario: string
    acento: string
  }
  favicon_url?: string
  dominio_personalizado?: string
  url_whatsapp?: string
  url_facebook?: string
  url_instagram?: string
  mostrar_precios?: boolean
  mostrar_stock?: boolean
  permitir_pedidos?: boolean
  productos_por_pagina?: number
  puede_agregar_productos?: boolean
  puede_agregar_categorias?: boolean
  plan?: string
  fecha_vencimiento?: string
  lista_precio_predeterminada_id?: number | null
  requesting_user_id: number
}

export interface CompaniesFilters {
  page?: number
  pageSize?: number
  principalCompanyId?: number
  search?: string
}