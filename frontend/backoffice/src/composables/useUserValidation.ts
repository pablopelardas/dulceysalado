import { z } from 'zod'
import type { UserFormData } from '~/types/users'

// Esquema base para validación de usuarios
const baseUserSchema = z.object({
  email: z.string()
    .min(1, 'El email es requerido')
    .email('Email inválido'),
  
  nombre: z.string()
    .min(1, 'El nombre es requerido')
    .min(2, 'El nombre debe tener al menos 2 caracteres')
    .max(50, 'El nombre no puede exceder 50 caracteres'),
  
  apellido: z.string()
    .min(1, 'El apellido es requerido')
    .min(2, 'El apellido debe tener al menos 2 caracteres')
    .max(50, 'El apellido no puede exceder 50 caracteres'),
  
  rol: z.enum(['admin', 'editor', 'viewer'], {
    required_error: 'El rol es requerido',
    invalid_type_error: 'Rol inválido'
  }),
  
  activo: z.boolean().optional().default(true),
  
  // Permisos
  puede_gestionar_productos_base: z.boolean().optional().default(false),
  puede_gestionar_productos_empresa: z.boolean().optional().default(false),
  puede_gestionar_categorias_base: z.boolean().optional().default(false),
  puede_gestionar_categorias_empresa: z.boolean().optional().default(false),
  puede_gestionar_usuarios: z.boolean().optional().default(false),
  puede_ver_estadisticas: z.boolean().optional().default(false)
})

// Esquema para crear usuario
export const createUserSchema = baseUserSchema.extend({
  password: z.string()
    .min(1, 'La contraseña es requerida')
    .min(8, 'La contraseña debe tener al menos 8 caracteres')
    .regex(/[A-Z]/, 'La contraseña debe contener al menos una mayúscula')
    .regex(/[a-z]/, 'La contraseña debe contener al menos una minúscula')
    .regex(/[0-9]/, 'La contraseña debe contener al menos un número'),
  
  confirmPassword: z.string()
    .min(1, 'Confirma la contraseña')
}).refine((data) => data.password === data.confirmPassword, {
  message: "Las contraseñas no coinciden",
  path: ["confirmPassword"]
})

// Esquema para actualizar usuario (todos los campos opcionales)
export const updateUserSchema = baseUserSchema.partial()

// Esquema para cambiar contraseña
export const changePasswordSchema = z.object({
  password: z.string()
    .min(1, 'La contraseña es requerida')
    .min(8, 'La contraseña debe tener al menos 8 caracteres')
    .regex(/[A-Z]/, 'La contraseña debe contener al menos una mayúscula')
    .regex(/[a-z]/, 'La contraseña debe contener al menos una minúscula')
    .regex(/[0-9]/, 'La contraseña debe contener al menos un número'),
  
  password_confirmation: z.string()
    .min(1, 'Confirma la contraseña')
}).refine((data) => data.password === data.password_confirmation, {
  message: "Las contraseñas no coinciden",
  path: ["password_confirmation"]
})

// Composable para validación de usuarios
export const useUserValidation = () => {
  
  // Validar datos de creación
  const validateCreateUser = (data: UserFormData) => {
    try {
      return {
        success: true,
        data: createUserSchema.parse(data)
      }
    } catch (error) {
      if (error instanceof z.ZodError) {
        return {
          success: false,
          errors: error.errors
        }
      }
      throw error
    }
  }
  
  // Validar datos de actualización
  const validateUpdateUser = (data: Partial<UserFormData>) => {
    try {
      return {
        success: true,
        data: updateUserSchema.parse(data)
      }
    } catch (error) {
      if (error instanceof z.ZodError) {
        return {
          success: false,
          errors: error.errors
        }
      }
      throw error
    }
  }
  
  // Validar cambio de contraseña
  const validateChangePassword = (data: { password: string, password_confirmation: string }) => {
    try {
      return {
        success: true,
        data: changePasswordSchema.parse(data)
      }
    } catch (error) {
      if (error instanceof z.ZodError) {
        return {
          success: false,
          errors: error.errors
        }
      }
      throw error
    }
  }
  
  // Helpers para validación de campos individuales
  const isValidEmail = (email: string) => {
    return z.string().email().safeParse(email).success
  }
  
  const isStrongPassword = (password: string) => {
    return password.length >= 8 &&
           /[A-Z]/.test(password) &&
           /[a-z]/.test(password) &&
           /[0-9]/.test(password)
  }
  
  // Formatear errores para mostrar en UI
  const formatErrors = (errors: z.ZodError['errors']) => {
    return errors.reduce((acc, error) => {
      const path = error.path.join('.')
      acc[path] = error.message
      return acc
    }, {} as Record<string, string>)
  }
  
  return {
    // Esquemas
    createUserSchema,
    updateUserSchema,
    changePasswordSchema,
    
    // Funciones de validación
    validateCreateUser,
    validateUpdateUser,
    validateChangePassword,
    
    // Helpers
    isValidEmail,
    isStrongPassword,
    formatErrors
  }
}