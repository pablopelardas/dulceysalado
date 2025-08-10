/**
 * Types for Agrupaciones (Product Groupings) System
 * Based on API documentation from docs/api-agrupaciones.md
 */

// Base Agrupacion entity
export interface AgrupacionDto {
  id: number
  codigo: number // Viene del campo Grupo3 de GECOM
  nombre: string
  descripcion: string | null
  activa: boolean
  empresa_principal_id: number
  created_at: string
  updated_at: string
}

// Para actualizar agrupaciones (campos editables)
export interface UpdateAgrupacionCommand {
  id: number
  nombre: string
  descripcion?: string | null
  activa: boolean
}

// Filtros para búsqueda de agrupaciones
export interface AgrupacionFilters {
  activa?: boolean
  busqueda?: string
  ordenarPor?: 'codigo' | 'nombre' | 'created_at' | 'updated_at'
  ordenDireccion?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

// Respuesta de la API para listado
export interface GetAgrupacionesQueryResult {
  success: boolean
  agrupaciones: AgrupacionDto[]
  pagination?: {
    total: number
    page: number
    page_size: number
    total_pages: number
  }
}

// Respuesta de la API para agrupación individual
export interface GetAgrupacionByIdQueryResult {
  success: boolean
  agrupacion: AgrupacionDto
}

// Estadísticas de agrupaciones
export interface AgrupacionesStats {
  total_agrupaciones: number
  agrupaciones_activas: number
  agrupaciones_inactivas: number
  empresa_principal_id: number
}

// Para configuración de visibilidad por empresa
export interface AgrupacionWithVisibility extends AgrupacionDto {
  visible: boolean // Si está visible para la empresa específica
}

// Respuesta para agrupaciones de una empresa
export interface EmpresaAgrupacionesResponse {
  success: boolean
  empresa_id: number
  agrupaciones: AgrupacionWithVisibility[]
}

// Para configurar visibilidad de una empresa
export interface ConfigureEmpresaVisibilityCommand {
  agrupaciones_ids: number[]
}

// Para configuración masiva (bulk update)
export interface BulkVisibilityConfiguration {
  empresa_id: number
  agrupaciones_ids: number[]
}

export interface BulkConfigureVisibilityCommand {
  configuraciones: BulkVisibilityConfiguration[]
}

// Respuesta del bulk update
export interface BulkConfigureVisibilityResult {
  success: boolean
  message: string
  empresas_procesadas: number
  empresas_exitosas: number
  empresas_con_errores: number
  resultados: {
    empresa_id: number
    agrupaciones_configuradas: number
    success: boolean
  }[]
  errores: any[]
}

// Para drag & drop component
export interface DragDropAgrupacion {
  id: number
  codigo: number
  nombre: string
  descripcion: string | null
  activa: boolean
}

// Estados del drag & drop
export interface DragDropState {
  visibles: DragDropAgrupacion[]
  noVisibles: DragDropAgrupacion[]
  loading: boolean
  hasChanges: boolean
}

// Para opciones de select
export interface AgrupacionSelectOption {
  value: number
  label: string
  descripcion?: string | null
  activa: boolean
  codigo: number
}

// API Error responses
export interface AgrupacionError {
  success: false
  message: string
  errors?: Record<string, string[]>
}