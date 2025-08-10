interface ApiResponse<T> {
  data?: T
  error?: string
}

interface Product {
  codigo: string
  nombre: string
  descripcion: string
  descripcion_corta: string | null
  precio: number | null
  destacado: boolean
  imagen_urls: string[]
  stock: number | null
  tags: string[]
  marca: string
  unidad: string
  codigo_barras: string | null
  codigo_rubro: number
  imagen_alt: string | null
  tipo_producto: string
  lista_precio_id: number | null
  lista_precio_nombre: string | null
  lista_precio_codigo: string | null
}

interface Category {
  id: number
  codigo_rubro: number
  nombre: string
  descripcion: string
  icono: string
  color: string
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
    
    const queryString = Object.entries(params)
      .filter(([_, value]) => value !== undefined && value !== null)
      .map(([key, value]) => `${key}=${encodeURIComponent(value)}`)
      .join('&')
    
    return queryString ? `?${queryString}` : ''
  }

  // Generic fetch method
  private async fetch<T>(endpoint: string, params: Record<string, any> = {}, signal?: AbortSignal): Promise<ApiResponse<T>> {
    try {
      const queryString = this.buildQueryString(params)
      const url = `${this.baseUrl}${endpoint}${queryString}`
      
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

// Export singleton instance
export const apiService = new ApiService()

// Export types for use in components
export type { 
  Product, 
  Category, 
  Company, 
  Feature,
  CatalogResponse, 
  CatalogFilters, 
  ApiResponse 
}