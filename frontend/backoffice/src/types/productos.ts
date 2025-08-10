// Tipos para Listas de Precios
export interface ListaPrecioInfo {
  id: number
  codigo: string | null
  nombre: string | null
  es_predeterminada: boolean
}

export interface ProductoPrecioInfo {
  lista_precio_id: number
  lista_precio_codigo: string | null
  lista_precio_nombre: string | null
  precio: number
}

// Tipos para ProductosBase
export interface ProductoBaseDto {
  id: number
  codigo: number
  descripcion: string | null
  codigo_rubro: number | null
  existencia: number | null
  visible: boolean | null
  destacado: boolean | null
  orden_categoria: number | null
  imagen_url: string | null
  imagen_alt: string | null
  descripcion_corta: string | null
  descripcion_larga: string | null
  tags: string | null
  codigo_barras: string | null
  marca: string | null
  unidad_medida: string | null
  administrado_por_empresa_id: number
  created_at: string | null
  updated_at: string | null
  precio_seleccionado: number | null // Precio de la lista seleccionada
  configuraciones_faltantes?: string[]
}

// Response de la consulta GetAllProductosBase
export interface GetAllProductosBaseQueryResult {
  productos: ProductoBaseDto[] | null
  total: number
  page: number
  page_size: number
  total_pages: number
  lista_seleccionada: ListaPrecioInfo | null
  listas_disponibles: ListaPrecioInfo[] | null
}

// Response de la consulta GetProductoBaseById
export interface GetProductoBaseByIdQueryResult {
  id: number
  codigo: number
  descripcion: string | null
  codigo_rubro: number | null
  existencia: number | null
  visible: boolean | null
  destacado: boolean | null
  orden_categoria: number | null
  imagen_url: string | null
  imagen_alt: string | null
  descripcion_corta: string | null
  descripcion_larga: string | null
  tags: string | null
  codigo_barras: string | null
  marca: string | null
  unidad_medida: string | null
  administrado_por_empresa_id: number
  created_at: string | null
  updated_at: string | null
  precios: ProductoPrecioInfo[] | null // Lista de precios del producto
}

// Response de la consulta GetProductoBaseByCodigo
export interface GetProductoBaseByCodigoQueryResult {
  id: number
  codigo: number
  descripcion: string | null
  codigo_rubro: number | null
  precio: number | null
  existencia: number | null
  visible: boolean | null
  destacado: boolean | null
  orden_categoria: number | null
  imagen_url: string | null
  imagen_alt: string | null
  descripcion_corta: string | null
  descripcion_larga: string | null
  tags: string | null
  codigo_barras: string | null
  marca: string | null
  unidad_medida: string | null
  administrado_por_empresa_id: number
  created_at: string | null
  updated_at: string | null
}

// Command para crear un ProductoBase
export interface CreateProductoBaseCommand {
  codigo: number
  descripcion: string | null
  codigo_rubro?: number | null
  existencia?: number | null
  visible?: boolean | null
  destacado?: boolean | null
  orden_categoria?: number | null
  imagen_url?: string | null
  imagen_alt?: string | null
  descripcion_corta?: string | null
  descripcion_larga?: string | null
  tags?: string | null
  codigo_barras?: string | null
  marca?: string | null
  unidad_medida?: string | null
}

// Response al crear un ProductoBase
export interface CreateProductoBaseCommandResult {
  id: number
  codigo: number
  descripcion: string | null
  codigo_rubro: number | null
  precio: number | null
  existencia: number | null
  visible: boolean | null
  destacado: boolean | null
  orden_categoria: number | null
  imagen_url: string | null
  imagen_alt: string | null
  descripcion_corta: string | null
  descripcion_larga: string | null
  tags: string | null
  codigo_barras: string | null
  marca: string | null
  unidad_medida: string | null
  administrado_por_empresa_id: number
  created_at: string | null
  updated_at: string | null
}

// Command para actualizar un ProductoBase
export interface UpdateProductoBaseCommand {
  id: number
  descripcion?: string | null
  codigo_rubro?: number | null
  existencia?: number | null
  visible?: boolean | null
  destacado?: boolean | null
  orden_categoria?: number | null
  imagen_url?: string | null
  imagen_alt?: string | null
  descripcion_corta?: string | null
  descripcion_larga?: string | null
  tags?: string | null
  codigo_barras?: string | null
  marca?: string | null
  unidad_medida?: string | null
}

// Response al actualizar un ProductoBase
export interface UpdateProductoBaseCommandResult {
  id: number
  codigo: number
  descripcion: string | null
  codigo_rubro: number | null
  precio: number | null
  existencia: number | null
  visible: boolean | null
  destacado: boolean | null
  orden_categoria: number | null
  imagen_url: string | null
  imagen_alt: string | null
  descripcion_corta: string | null
  descripcion_larga: string | null
  tags: string | null
  codigo_barras: string | null
  marca: string | null
  unidad_medida: string | null
  administrado_por_empresa_id: number
  created_at: string | null
  updated_at: string | null
}

// Filtros para consultas de ProductosBase
export interface ProductosBaseFilters {
  visible?: boolean
  destacado?: boolean
  codigoRubro?: number
  busqueda?: string
  soloSinConfiguracion?: boolean
  incluirSinExistencia?: boolean
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
  listaPrecioId?: number // ID de la lista de precios a mostrar
  empresaId?: number // ID de la empresa para stock diferencial
}


// Tipo para producto del catálogo público
export interface ProductoCatalogoDto {
  codigo: string | null
  nombre: string | null
  descripcion: string | null
  precio: number | null
  precio_especial: number | null
  stock: number | null
  imagen_url: string | null
  categoria: string | null
  destacado: boolean
  mostrar_precio: boolean
  mostrar_stock: boolean
}

// Tipo para categoría
export interface CategoriaPublicaDto {
  codigo: number
  nombre: string | null
  icono: string | null
  color: string | null
  product_count: number
}

// Tipos para gestión de precios
export interface UpsertPrecioProductoCommand {
  producto_id?: number
  producto_empresa_id?: number // Opcional para productos empresa
  lista_precio_id: number
  precio: number
}

export interface UpsertPrecioProductoCommandResult {
  producto_id: number
  lista_precio_id: number
  precio: number
  was_created: boolean
  message: string | null
}

export interface UpdatePrecioProductoRequest {
  precio: number
}

export interface UpdatePrecioProductoCommandResult {
  producto_id: number
  lista_precio_id: number
  precio: number
  message: string | null
}

export interface GetPreciosPorProductoQueryResult {
  producto_id: number
  precios: PrecioListaDto[] | null
}

export interface PrecioListaDto {
  lista_precio_id: number
  lista_precio_codigo: string | null
  lista_precio_nombre: string | null
  precio: number
  ultima_actualizacion: string | null
}

export interface GetPrecioPorProductoYListaQueryResult {
  producto_id: number
  lista_precio_id: number
  lista_precio_codigo: string | null
  lista_precio_nombre: string | null
  precio: number
  ultima_actualizacion: string | null
}

// Tipo para endpoints dinámicos
export type ProductType = 'base' | 'empresa'