import type { ListaPrecioInfo } from './productos'

// ==================== TIPOS PRINCIPALES ====================

// Tipo para lista de precios anidada en cliente
export interface ClienteListaPrecio {
  id: number
  codigo: string
  nombre: string
  descripcion: string | null
  activo: boolean
}

// Cliente básico DTO (datos del cliente sin credenciales)
export interface ClienteDto {
  id: number
  empresa_id: number
  codigo: string
  nombre: string
  direccion: string | null
  localidad: string | null
  telefono: string | null
  cuit: string | null
  altura: string | null
  provincia: string | null
  email: string
  tipo_iva: string | null
  lista_precio: ClienteListaPrecio | null
  created_by: string | null
  created_at: string | null
  updated_at: string | null
  // Información de credenciales (si existe)
  tiene_acceso: boolean // Renombrado de has_credentials para coincidir con API
  username: string | null
  activo: boolean // Renombrado de is_active para coincidir con API
  last_login: string | null // Renombrado de ultimo_login para coincidir con API
}

// DTO para credenciales del cliente
export interface ClienteCredentialDto {
  id: number
  cliente_id: number
  empresa_id: number
  username: string
  is_active: boolean
  created_by: string | null
  created_at: string | null
  updated_at: string | null
  ultimo_login: string | null
}

// ==================== RESPONSES DE CONSULTAS ====================

// Response para obtener todos los clientes
export interface GetAllClientesQueryResult {
  clientes: ClienteDto[]
  total: number
  page: number
  page_size: number
  total_pages: number
  listas_disponibles: ListaPrecioInfo[]
}

// Response para obtener cliente por ID
export interface GetClienteByIdQueryResult extends ClienteDto {}

// Response para obtener credenciales de cliente
export interface GetClienteCredentialsQueryResult extends ClienteCredentialDto {}

// ==================== COMMANDS CRUD ====================

// Command para crear cliente (datos básicos)
export interface CreateClienteCommand {
  empresa_id?: number // Opcional, se puede inferir del JWT
  codigo: string
  nombre: string
  direccion?: string | null
  localidad?: string | null
  telefono?: string | null
  cuit?: string | null
  altura?: string | null
  provincia?: string | null
  email: string
  tipo_iva?: string | null
  lista_precio_id?: number | null // Se envía como ID, pero se recibe como objeto
  created_by?: string | null
}

// Response al crear cliente
export interface CreateClienteCommandResult extends ClienteDto {}

// Command para actualizar cliente
export interface UpdateClienteCommand {
  id: number
  codigo?: string
  nombre?: string
  direccion?: string | null
  localidad?: string | null
  telefono?: string | null
  cuit?: string | null
  altura?: string | null
  provincia?: string | null
  email?: string
  tipo_iva?: string | null
  lista_precio_id?: number | null // Se envía como ID, pero se recibe como objeto
}

// Response al actualizar cliente
export interface UpdateClienteCommandResult extends ClienteDto {}

// ==================== COMMANDS PARA CREDENCIALES ====================

// Command para crear credenciales de cliente
export interface CreateClienteCredentialsCommand {
  cliente_id: number
  empresa_id?: number // Opcional, se puede inferir del JWT
  username: string
  password: string
  is_active?: boolean
  created_by?: string | null
}

// Response al crear credenciales
export interface CreateClienteCredentialsCommandResult extends ClienteCredentialDto {}

// Command para actualizar credenciales
export interface UpdateClienteCredentialsCommand {
  username?: string
  password?: string
  is_active?: boolean
}

// Response al actualizar credenciales
export interface UpdateClienteCredentialsCommandResult extends ClienteCredentialDto {}

// Command para reset de password
export interface ResetPasswordClienteCommand {
  password: string
}

// Response al reset de password
export interface ResetPasswordClienteCommandResult {
  success: boolean
  message: string
}

// ==================== FILTROS Y PAGINACIÓN ====================

// Filtros para consultas de clientes
export interface ClientesFilters {
  search?: string // Busca en nombre, email, codigo
  lista_precio_id?: number | null
  has_credentials?: boolean
  is_active?: boolean
  localidad?: string
  provincia?: string
  include_deleted?: boolean
  page?: number
  page_size?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

// ==================== VALIDACIÓN Y FORMULARIOS ====================

// Tipos para validación de formularios - datos básicos del cliente
export interface ClienteFormData {
  codigo: string
  nombre: string
  direccion: string
  localidad: string
  telefono: string
  cuit: string
  altura: string
  provincia: string
  email: string
  tipo_iva: string
  lista_precio_id: number | null
}

// Tipos para formulario de credenciales
export interface ClienteCredentialsFormData {
  username: string
  password: string
  is_active: boolean
}

// ==================== HELPERS ====================

// Enum para estados de credenciales
export enum ClienteCredentialEstado {
  ACTIVO = true,
  INACTIVO = false
}

// Enum para campos de ordenamiento
export enum ClienteSortFields {
  CODIGO = 'codigo',
  NOMBRE = 'nombre',
  EMAIL = 'email',
  LOCALIDAD = 'localidad',
  USERNAME = 'username',
  ULTIMO_LOGIN = 'ultimo_login',
  CREATED_AT = 'created_at'
}

// Tipo combinado para el formulario completo (cliente + credenciales)
export interface ClienteCompleteFormData extends ClienteFormData {
  // Datos de credenciales opcionales
  create_credentials: boolean
  credentials?: ClienteCredentialsFormData
}