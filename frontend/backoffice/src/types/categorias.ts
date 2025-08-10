// ===== TIPOS BASE PARA CATEGORÍAS =====

export type CategoryType = 'base' | 'empresa'

// ===== CATEGORÍAS BASE =====

export interface CategoryBaseDto {
  id: number
  codigo_rubro?: number | null
  nombre: string
  descripcion?: string | null
  icono?: string | null
  color?: string | null
  visible: boolean
  orden: number
  created_at: string
  updated_at: string
  created_by_empresa_id?: number | null
  product_count?: number
  // Alias para mantener compatibilidad
  fecha_creacion?: string
  fecha_actualizacion?: string
  cantidad_productos?: number
}

export interface CreateCategoryBaseCommand {
  empresa_id: number
  codigo_rubro: number
  nombre: string
  descripcion?: string | null
  icono?: string | null
  color?: string | null
  visible: boolean
  orden: number
}

export interface CreateCategoryBaseCommandResult {
  id: number
  nombre: string
  descripcion?: string | null
  visible: boolean
  orden: number
  fecha_creacion: string
}

export interface UpdateCategoryBaseCommand {
  id: number
  empresa_id: number
  codigo_rubro: number
  nombre: string
  descripcion?: string | null
  icono?: string | null
  color?: string | null
  visible: boolean
  orden: number
}

export interface UpdateCategoryBaseCommandResult {
  id: number
  nombre: string
  descripcion?: string | null
  visible: boolean
  orden: number
  fecha_actualizacion: string
}

export interface GetCategoriesBaseQueryResult {
  categorias: CategoryBaseDto[]
  total: number
}

// ===== CATEGORÍAS EMPRESA =====

export interface CategoryEmpresaDto {
  id: number
  empresa_id: number
  empresa_nombre?: string
  codigo_rubro?: number | null
  nombre: string
  descripcion?: string | null
  icono?: string | null
  color?: string | null
  visible: boolean
  orden: number
  created_at: string
  updated_at: string
  created_by_empresa_id?: number | null
  product_count?: number
  // Alias para mantener compatibilidad
  fecha_creacion?: string
  fecha_actualizacion?: string
  cantidad_productos?: number
}

export interface CreateCategoryEmpresaCommand {
  empresa_id: number
  codigo_rubro: number
  nombre: string
  descripcion?: string | null
  icono?: string | null
  color?: string | null
  visible: boolean
  orden: number
}

export interface CreateCategoryEmpresaCommandResult {
  id: number
  empresa_id: number
  codigo_rubro: number
  nombre: string
  descripcion?: string | null
  icono?: string | null
  color?: string | null
  visible: boolean
  orden: number
  fecha_creacion: string
}

export interface UpdateCategoryEmpresaCommand {
  id: number
  empresa_id: number
  codigo_rubro: number
  nombre: string
  descripcion?: string | null
  icono?: string | null
  color?: string | null
  visible: boolean
  orden: number
}

export interface UpdateCategoryEmpresaCommandResult {
  id: number
  empresa_id: number
  codigo_rubro: number
  nombre: string
  descripcion?: string | null
  icono?: string | null
  color?: string | null
  visible: boolean
  orden: number
  fecha_actualizacion: string
}

export interface GetCategoriesEmpresaQueryResult {
  categorias: CategoryEmpresaDto[]
  total: number
}

export interface GetCategoryEmpresaByIdQueryResult {
  categoria: CategoryEmpresaDto
}

// ===== TIPOS PARA FILTROS Y PARÁMETROS =====

export interface CategoryBaseFilters {
  visibleOnly?: boolean
  empresaId?: number
}

export interface CategoryEmpresaFilters {
  empresaId?: number
  visibleOnly?: boolean
}

// ===== TIPOS PARA COMPONENTES =====

export interface CategoryFormData {
  codigo_rubro?: number
  nombre: string
  descripcion: string
  icono?: string | null
  color?: string | null
  visible: boolean
  orden: number
  empresa_id?: number
}

export interface CategoryTableColumn {
  key: string
  label: string
  sortable?: boolean
}

// ===== TIPOS PARA SELECTS Y OPCIONES =====

export interface CategoryOption {
  value: number
  label: string
  descripcion?: string
}

export interface EmpresaOption {
  value: number
  label: string
  codigo?: string
}