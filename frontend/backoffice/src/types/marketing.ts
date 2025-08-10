export interface Agrupacion {
  id: number
  nombre: string
  descripcion: string | null
  nivel: number
  empresa_principal_id: number | null
  activa: boolean
  is_novedad?: boolean
  is_oferta?: boolean
  created_at: string
  updated_at: string
}

export interface NovedadesResponse {
  agrupaciones: Agrupacion[]
}

export interface OfertasResponse {
  agrupaciones: Agrupacion[]
}

export interface SetNovedadesRequest {
  agrupacion_ids: number[]
}

export interface SetOfertasRequest {
  agrupacion_ids: number[]
}

export interface NovedadesApiResponse {
  success: boolean
  message: string
}

export interface OfertasApiResponse {
  success: boolean
  message: string
}

// Estados de loading y error para composables
export interface MarketingState {
  isLoading: boolean
  error: string | null
  agrupaciones: Agrupacion[]
  selectedIds: number[]
}