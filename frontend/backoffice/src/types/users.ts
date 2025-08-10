import type { Usuario, Empresa } from './auth'

// Request types
export interface CreateUserRequest {
  empresa_id?: number // Solo para empresa principal
  email: string
  password: string
  nombre: string
  apellido: string
  rol: 'admin' | 'editor' | 'viewer'
  
  // Permisos específicos
  puede_gestionar_productos_base?: boolean
  puede_gestionar_productos_empresa?: boolean
  puede_gestionar_categorias_base?: boolean
  puede_gestionar_categorias_empresa?: boolean
  puede_gestionar_usuarios?: boolean
  puede_ver_estadisticas?: boolean
  
  activo?: boolean
}

export interface UpdateUserRequest {
  email?: string
  nombre?: string
  apellido?: string
  rol?: 'admin' | 'editor' | 'viewer'
  
  // Permisos específicos
  puede_gestionar_productos_base?: boolean
  puede_gestionar_productos_empresa?: boolean
  puede_gestionar_categorias_base?: boolean
  puede_gestionar_categorias_empresa?: boolean
  puede_gestionar_usuarios?: boolean
  puede_ver_estadisticas?: boolean
  
  activo?: boolean
}

export interface ChangePasswordRequest {
  NewPassword: string
  CurrentPassword: string
}

export interface AdminChangePasswordRequest {
  password: string
  confirmPassword: string
}

// Response types
export interface UserListResponse {
  usuarios: Usuario[]
  pagination: {
    page: number
    limit: number
    total: number
    pages: number
  }
}

export interface UserResponse {
  user: Usuario
}

export interface CreateUserResponse {
  message: string
  user: Usuario
}

export interface UpdateUserResponse {
  message: string
  user: Usuario
}

// Filter types
export interface UserFilters {
  search?: string
  rol?: 'admin' | 'editor' | 'viewer' | ''
  activo?: boolean | ''
  empresa_id?: number // Solo para empresa principal
  page?: number
  limit?: number
}

// Permission helpers
export interface UserPermission {
  key: keyof PermissionSet
  label: string
  description: string
  adminOnly?: boolean // Solo para empresa principal
}

export interface PermissionSet {
  puede_gestionar_productos_base: boolean
  puede_gestionar_productos_empresa: boolean
  puede_gestionar_categorias_base: boolean
  puede_gestionar_categorias_empresa: boolean
  puede_gestionar_usuarios: boolean
  puede_ver_estadisticas: boolean
}

// Constantes de permisos
export const USER_PERMISSIONS: UserPermission[] = [
  {
    key: 'puede_gestionar_productos_base',
    label: 'Gestionar Productos Base',
    description: 'Crear, editar y eliminar productos del catálogo base',
    adminOnly: true
  },
  {
    key: 'puede_gestionar_productos_empresa',
    label: 'Gestionar Productos Propios',
    description: 'Crear, editar y eliminar productos de la empresa'
  },
  {
    key: 'puede_gestionar_categorias_base',
    label: 'Gestionar Categorías Base',
    description: 'Crear, editar y eliminar categorías del catálogo base',
    adminOnly: true
  },
  {
    key: 'puede_gestionar_categorias_empresa',
    label: 'Gestionar Categorías Propias',
    description: 'Crear, editar y eliminar categorías de la empresa'
  },
  {
    key: 'puede_gestionar_usuarios',
    label: 'Gestionar Usuarios',
    description: 'Crear, editar y eliminar usuarios de la empresa'
  },
  {
    key: 'puede_ver_estadisticas',
    label: 'Ver Estadísticas',
    description: 'Acceder a reportes y estadísticas del sistema'
  }
]

// Roles predefinidos con permisos
export const ROLE_PERMISSIONS: Record<string, Partial<PermissionSet>> = {
  admin: {
    puede_gestionar_productos_empresa: true,
    puede_gestionar_categorias_empresa: true,
    puede_gestionar_usuarios: true,
    puede_ver_estadisticas: true
  },
  editor: {
    puede_gestionar_productos_empresa: true,
    puede_gestionar_categorias_empresa: true,
    puede_ver_estadisticas: false
  },
  viewer: {
    puede_ver_estadisticas: false
  }
}

// Utility types
export interface UserFormData extends CreateUserRequest {
  confirmPassword?: string
}

export interface UserTableItem extends Usuario {
  empresa: Empresa
}