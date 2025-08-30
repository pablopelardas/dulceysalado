interface ApiResponse<T> {
  data?: T
  error?: string
}

// Auth interfaces
interface LoginRequest {
  empresa_id: number
  username: string
  password: string
}

interface LoginResponse {
  access_token: string
  refresh_token: string
  expires_in: number
  token_type: string
  cliente: {
    id: number
    codigo: string
    nombre?: string
    apellido?: string
    email?: string
    telefono?: string
    direccion?: string
    empresa_id: number
    lista_precio_id: number
  }
}

interface RefreshRequest {
  refresh_token: string
}

interface ChangePasswordRequest {
  current_password: string
  new_password: string
}

interface UpdateProfileRequest {
  nombre?: string
  direccion?: string
  localidad?: string
  telefono?: string
  cuit?: string
  altura?: string
  provincia?: string
  tipo_iva?: string
}

interface UpdateProfileResponse {
  message: string
  cliente: UserProfile
}

interface RegisterRequest {
  empresa_id: number
  nombre: string
  email: string
  password: string
  telefono?: string
  direccion?: string
}

interface RegisterResponse {
  message: string
  cliente: {
    id: number
    empresa_id: number
    codigo: string
    nombre: string
    email: string
    telefono?: string
    direccion?: string
    username: string
    tiene_acceso: boolean
    activo: boolean
    last_login?: string
    lista_precio?: {
      id: number
      codigo: string
      nombre: string
    }
  }
  access_token: string
  refresh_token: string
  expires_in: string
}

interface UserProfile {
  id: number
  codigo: string
  nombre?: string
  apellido?: string
  email?: string
  telefono?: string
  direccion?: string
  empresa_id: number
  lista_precio_id: number
}

interface CreateOrderRequest {
  items: OrderItem[]
  observaciones?: string
  direccion_entrega?: string
  fecha_entrega?: string
  horario_entrega?: string
}

interface OrderItem {
  codigo_producto: string
  cantidad: number
  precio_unitario: number
}

interface OrderResponse {
  id: number
  cliente_id: number
  empresa_id: number
  numero: string
  fecha_pedido: string
  fecha_entrega?: string
  horario_entrega?: string
  direccion_entrega?: string
  observaciones?: string
  monto_total: number
  estado: string
  motivo_rechazo?: string | null
  usuario_gestion_id?: number | null
  fecha_gestion?: string | null
  created_at: string
  updated_at: string
  cliente_nombre: string
  cliente_email: string
  cliente_telefono: string
  items: {
    id: number
    pedido_id: number
    codigo_producto: string
    nombre_producto: string
    cantidad: number
    precio_unitario: number
    subtotal: number
    observaciones?: string | null
  }[]
}

interface PaginatedOrdersResponse {
  items: OrderResponse[]
  total_count: number
  page: number
  page_size: number
  total_pages: number
  has_next_page: boolean
  has_previous_page: boolean
}

interface Product {
  codigo: string
  nombre: string
  descripcion: string
  descripcion_corta?: string | null
  precio: number | null
  precio_anterior?: number | null
  destacado: boolean
  imagen_urls: string[]
  stock: number | null
  tags?: string[]
  marca: string
  unidad?: string
  codigo_barras?: string | null
  codigo_rubro?: number
  imagen_alt?: string | null
  tipo_producto?: string
  lista_precio_id?: number | null
  lista_precio_nombre?: string | null
  lista_precio_codigo?: string | null
  lista?: string
  novedad?: boolean
  oferta?: boolean
  categoria?: string
}

interface Category {
  id: number
  codigo_rubro: number
  nombre: string
  descripcion: string
  icono: string
  color: string
  imagen_url?: string
  orden: number
  product_count: number
}

interface CatalogResponse {
  productos: Product[]
  total_count: number
  page: number
  page_size: number
  total_pages: number
  categorias: Category[]
}

interface Feature {
  codigo: string
  nombre: string
  descripcion?: string
  habilitado: boolean
  valor?: string
  metadata?: {
    mensaje_template?: string
    [key: string]: any
  }
}

interface Company {
  id: number
  codigo: string
  nombre: string
  razon_social: string
  telefono: string
  email: string
  direccion: string
  descripcion?: string
  logo_url: string
  colores_tema: string
  favicon_url: string
  dominio_personalizado: string
  url_whatsapp: string
  url_facebook: string
  url_instagram: string
  mostrar_precios: boolean
  mostrar_stock: boolean
  permitir_pedidos: boolean
  productos_por_pagina: number
  plan: string
  activa: boolean
  features?: Feature[]
}

interface CatalogFilters {
  listaPrecioCodigo?: string
  categoria?: string
  busqueda?: string
  destacados?: boolean
  codigoRubro?: number
  page?: number
  pageSize?: number
  ordenarPor?: 'precio_asc' | 'precio_desc' | 'nombre_asc' | 'nombre_desc'
}

interface SpecialProductsResponse {
  success: boolean
  message: string | null
  productos: Product[]
  total_productos: number
  empresa_nombre: string
  fecha_consulta: string
}

interface CacheEntry {
  data: Product[]
  timestamp: number
}

class ApiService {
  private baseUrl: string
  private readonly EMPRESA_ID = 1 // Hardcoded para Dulce y Salado
  private cache: Map<string, CacheEntry> = new Map()
  private readonly CACHE_TTL = 5 * 60 * 1000 // 5 minutes

  constructor() {
    // Siempre usar la API de dulceysaladomax
    this.baseUrl = import.meta.env.VITE_API_URL || 'https://api.dulceysaladomax.com'
  }

  // Siempre retorna el ID de Dulce y Salado
  private getCompanyId(): number {
    return this.EMPRESA_ID
  }

  // Build query string with company ID
  private buildQueryString(params: Record<string, any> = {}): string {
    const companyId = this.getCompanyId()
    params.empresaId = companyId
    
    console.log('🔍 buildQueryString input params:', params)
    
    const queryString = Object.entries(params)
      .filter(([, value]) => value !== undefined && value !== null)
      .map(([key, value]) => `${key}=${encodeURIComponent(value)}`)
      .join('&')
    
    console.log('🔍 buildQueryString output:', queryString)
    
    return queryString ? `?${queryString}` : ''
  }

  // Generic fetch method
  private async fetch<T>(endpoint: string, params: Record<string, any> = {}, signal?: AbortSignal): Promise<ApiResponse<T>> {
    try {
      const queryString = this.buildQueryString(params)
      const url = `${this.baseUrl}${endpoint}${queryString}`
      
      console.log('🔍 API fetch URL:', url)
      
      const response = await fetch(url, {
        method: 'GET',
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json'
        },
        signal
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const data = await response.json()
      return { data }
    } catch (error) {
      console.error(`API Error for ${endpoint}:`, error)
      return { error: error instanceof Error ? error.message : 'Unknown error' }
    }
  }

  // Get company information
  async getCompany(): Promise<ApiResponse<Company>> {
    return this.fetch<Company>('/api/catalog/empresa')
  }

  // Get catalog with filters
  async getCatalog(filters: CatalogFilters = {}, signal?: AbortSignal): Promise<ApiResponse<CatalogResponse>> {
    console.log('🔍 getCatalog called with filters:', filters)
    return this.fetch<CatalogResponse>('/api/catalog', filters, signal)
  }

  // Get categories
  async getCategories(): Promise<ApiResponse<{ categorias: Category[] }>> {
    return this.fetch<{ categorias: Category[] }>('/api/catalog/categorias')
  }

  // Get product by code
  async getProduct(codigo: string, listaPrecioCodigo?: string): Promise<ApiResponse<Product>> {
    const params = listaPrecioCodigo ? { listaPrecioCodigo } : {}
    return this.fetch<Product>(`/api/catalog/producto/${codigo}`, params)
  }

  // Get featured products
  async getFeaturedProducts(listaPrecioCodigo?: string, limit: number = 10): Promise<ApiResponse<{ productos: Product[] }>> {
    const params = {
      listaPrecioCodigo,
      limit
    }
    return this.fetch<{ productos: Product[] }>('/api/catalog/destacados', params)
  }

  // Cache management methods
  private getCachedData(key: string): Product[] | null {
    const cached = this.cache.get(key)
    if (cached && Date.now() - cached.timestamp < this.CACHE_TTL) {
      return cached.data
    }
    this.cache.delete(key)
    return null
  }

  private setCachedData(key: string, data: Product[]): void {
    this.cache.set(key, { data, timestamp: Date.now() })
  }

  // Get novedades (products marked as new)
  async getNovedades(): Promise<{ data: Product[], error?: string }> {
    const cacheKey = 'novedades'
    
    // Check cache first
    const cachedData = this.getCachedData(cacheKey)
    if (cachedData) {
      return { data: cachedData }
    }

    try {
      const response = await this.fetch<SpecialProductsResponse>('/api/catalog/novedades')
      
      if (response.error) {
        console.error('Error fetching novedades:', response.error)
        return { data: [] }
      }

      const productos = response.data?.productos || []
      
      // Cache the result
      this.setCachedData(cacheKey, productos)
      
      return { data: productos }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      console.error('Error fetching novedades:', errorMessage)
      return { data: [] }
    }
  }

  // Get ofertas (products marked as offers)
  async getOfertas(): Promise<{ data: Product[], error?: string }> {
    const cacheKey = 'ofertas'
    
    // Check cache first
    const cachedData = this.getCachedData(cacheKey)
    if (cachedData) {
      return { data: cachedData }
    }

    try {
      const response = await this.fetch<SpecialProductsResponse>('/api/catalog/ofertas')
      
      if (response.error) {
        console.error('Error fetching ofertas:', response.error)
        return { data: [] }
      }

      const productos = response.data?.productos || []
      
      // Cache the result
      this.setCachedData(cacheKey, productos)
      
      return { data: productos }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      console.error('Error fetching ofertas:', errorMessage)
      return { data: [] }
    }
  }
}

// Auth API Service
class AuthApiService {
  private baseUrl: string
  private readonly EMPRESA_ID = 1 // Hardcoded para Dulce y Salado

  constructor() {
    this.baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:7000'
  }

  private async authFetch<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`
    
    const response = await fetch(url, {
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'text/plain',
        ...options.headers,
      },
      ...options,
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(errorText || `HTTP error! status: ${response.status}`)
    }

    const text = await response.text()
    
    // Si la respuesta está vacía, retornar un objeto vacío
    if (!text) {
      return {} as T
    }

    try {
      return JSON.parse(text)
    } catch {
      // Si no es JSON válido, retornar el texto como string
      return text as unknown as T
    }
  }

  // Método público para rutas que no requieren autenticación
  async publicFetch<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`
    
    const response = await fetch(url, {
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        ...options.headers,
      },
      ...options,
    })

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: 'Error desconocido' }))
      throw { status: response.status, message: errorData.message || `HTTP error! status: ${response.status}` }
    }

    return response.json()
  }

  async login(username: string, password: string): Promise<LoginResponse> {
    const loginData: LoginRequest = {
      empresa_id: this.EMPRESA_ID,
      username,
      password
    }

    return this.authFetch<LoginResponse>('/api/cliente-auth/login', {
      method: 'POST',
      body: JSON.stringify(loginData)
    })
  }

  async register(registerData: RegisterRequest): Promise<RegisterResponse> {
    // Agregar empresa_id si no está presente
    const completeRegisterData = {
      ...registerData,
      empresa_id: registerData.empresa_id || this.EMPRESA_ID
    }

    return this.authFetch<RegisterResponse>('/api/cliente-auth/register', {
      method: 'POST',
      body: JSON.stringify(completeRegisterData)
    })
  }

  async loginWithGoogle(): Promise<LoginResponse> {
    // Redirigir al endpoint de OAuth Google con callback dinámico
    const callbackUrl = encodeURIComponent(`${window.location.origin}/auth/google/callback`)
    const googleAuthUrl = `${this.baseUrl}/api/cliente-auth/google?empresaId=${this.EMPRESA_ID}&redirect_uri=${callbackUrl}`
    window.location.href = googleAuthUrl
    
    // Esta función no retorna directamente porque hay una redirección
    // El backend redirigirá de vuelta con los tokens
    return new Promise(() => {}) // Never resolves in this flow
  }

  async refreshToken(refreshToken: string): Promise<LoginResponse> {
    const refreshData: RefreshRequest = {
      refresh_token: refreshToken
    }

    return this.authFetch<LoginResponse>('/api/cliente-auth/refresh', {
      method: 'POST',
      body: JSON.stringify(refreshData)
    })
  }

  async logout(refreshToken: string, accessToken: string): Promise<void> {
    const logoutData = {
      refresh_token: refreshToken
    }

    return this.authFetch<void>('/api/cliente-auth/logout', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${accessToken}`
      },
      body: JSON.stringify(logoutData)
    })
  }

  async getProfile(accessToken: string): Promise<UserProfile> {
    return this.authFetch<UserProfile>('/api/cliente-auth/me', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${accessToken}`
      }
    })
  }

  async changePassword(currentPassword: string, newPassword: string, accessToken: string): Promise<void> {
    const passwordData: ChangePasswordRequest = {
      current_password: currentPassword,
      new_password: newPassword
    }

    return this.authFetch<void>('/api/cliente-auth/change-password', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${accessToken}`
      },
      body: JSON.stringify(passwordData)
    })
  }

  async updateProfile(profileData: UpdateProfileRequest, accessToken: string): Promise<UpdateProfileResponse> {
    const url = `${this.baseUrl}/api/cliente-auth/profile`
    
    const response = await fetch(url, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': `Bearer ${accessToken}`
      },
      body: JSON.stringify(profileData)
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(errorText || `HTTP error! status: ${response.status}`)
    }

    return response.json()
  }

  async createOrder(orderData: CreateOrderRequest, accessToken: string): Promise<OrderResponse> {
    const url = `${this.baseUrl}/api/cliente-auth/pedidos`
    
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': `Bearer ${accessToken}`
      },
      body: JSON.stringify(orderData)
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(errorText || `HTTP error! status: ${response.status}`)
    }

    return response.json()
  }

  async getOrderHistory(
    accessToken: string, 
    page: number = 1, 
    pageSize: number = 20,
    estado?: string,
    fechaDesde?: string,
    fechaHasta?: string
  ): Promise<PaginatedOrdersResponse> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString()
    })
    
    if (estado) params.append('estado', estado)
    if (fechaDesde) params.append('fechaDesde', fechaDesde)
    if (fechaHasta) params.append('fechaHasta', fechaHasta)
    
    const url = `${this.baseUrl}/api/cliente-auth/pedidos?${params.toString()}`
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Accept': 'application/json',
        'Authorization': `Bearer ${accessToken}`
      }
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(errorText || `HTTP error! status: ${response.status}`)
    }

    return response.json()
  }
  
  async getOrderById(orderId: number, accessToken: string): Promise<OrderResponse> {
    const url = `${this.baseUrl}/api/cliente-auth/pedidos/${orderId}`
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Accept': 'application/json',
        'Authorization': `Bearer ${accessToken}`
      }
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(errorText || `HTTP error! status: ${response.status}`)
    }

    return response.json()
  }
  
  async cancelOrder(orderId: number, motivo: string, accessToken: string): Promise<void> {
    const url = `${this.baseUrl}/api/cliente-auth/pedidos/${orderId}/cancelar`
    
    const response = await fetch(url, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': `Bearer ${accessToken}`
      },
      body: JSON.stringify({ motivo })
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(errorText || `HTTP error! status: ${response.status}`)
    }
  }

  async approveCorrection(orderId: number, comentario?: string, accessToken?: string): Promise<void> {
    const url = `${this.baseUrl}/api/cliente-auth/pedidos/${orderId}/correccion/aprobar`
    
    const response = await fetch(url, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': `Bearer ${accessToken}`
      },
      body: JSON.stringify({ comentario })
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(errorText || `HTTP error! status: ${response.status}`)
    }
  }

  async rejectCorrection(orderId: number, comentario?: string, accessToken?: string): Promise<void> {
    const url = `${this.baseUrl}/api/cliente-auth/pedidos/${orderId}/correccion/rechazar`
    
    const response = await fetch(url, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': `Bearer ${accessToken}`
      },
      body: JSON.stringify({ comentario })
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(errorText || `HTTP error! status: ${response.status}`)
    }
  }

  async getOrderCorrection(orderId: number, accessToken?: string): Promise<any> {
    const url = `${this.baseUrl}/api/cliente-auth/pedidos/${orderId}/correccion`
    
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': `Bearer ${accessToken}`
      }
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(errorText || `HTTP error! status: ${response.status}`)
    }

    return response.json()
  }
}

// Export singleton instances
export const apiService = new ApiService()
export const authApiService = new AuthApiService()

// Export types for use in components
export type { 
  Product, 
  Category, 
  Company, 
  Feature,
  CatalogResponse, 
  CatalogFilters, 
  ApiResponse,
  LoginRequest,
  LoginResponse,
  RefreshRequest,
  ChangePasswordRequest,
  UpdateProfileRequest,
  UpdateProfileResponse,
  UserProfile,
  CreateOrderRequest,
  OrderItem,
  OrderResponse,
  PaginatedOrdersResponse
}