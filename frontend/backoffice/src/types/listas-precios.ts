// DTOs para Listas de Precios

export interface ListaPrecioDto {
  id: number
  codigo: string
  nombre: string
  descripcion?: string | null
  es_predeterminada: boolean
  activa: boolean
  orden: number
  fecha_creacion: string
  fecha_actualizacion: string
  cantidad_productos?: number
}

// Commands
export interface CreateListaPrecioCommand {
  codigo: string
  nombre: string
  descripcion?: string | null
  es_predeterminada?: boolean
  activa?: boolean
  orden?: number
}

export interface UpdateListaPrecioCommand {
  id: number
  codigo: string
  nombre: string
  descripcion?: string | null
  es_predeterminada?: boolean
  activa?: boolean
  orden?: number
}

// API Requests
export interface CreateListaPrecioRequest extends CreateListaPrecioCommand {}
export interface UpdateListaPrecioRequest extends UpdateListaPrecioCommand {}

// Query Results
export interface GetListasPreciosQueryResult {
  listas: ListaPrecioDto[]
  total: number
}

export interface GetListaPrecioByIdQueryResult {
  lista: ListaPrecioDto
}

// Filtros
export interface ListaPrecioFilters {
  activa?: boolean
  busqueda?: string
  ordenarPor?: 'nombre' | 'codigo' | 'orden' | 'fecha_creacion'
  ordenDireccion?: 'asc' | 'desc'
}