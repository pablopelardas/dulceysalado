/**
 * Utility for parsing API error responses into user-friendly messages
 */

export interface ApiErrorResponse {
  type?: string
  title?: string
  status?: number
  errors?: Record<string, string[]>
  error?: string
  message?: string
  traceId?: string
}

export interface ParsedError {
  message: string
  details?: string[]
  originalError?: any
}

/**
 * Parse API error response and extract meaningful error messages
 */
export function parseApiError(error: any): ParsedError {
  // Handle different error structures
  const errorData = error?.data || error

  // Handle validation errors with detailed field errors
  if (errorData?.errors && typeof errorData.errors === 'object') {
    const fieldErrors: string[] = []
    
    for (const [field, messages] of Object.entries(errorData.errors)) {
      if (Array.isArray(messages)) {
        // Add field name context to each message
        const fieldName = getFieldDisplayName(field)
        messages.forEach(msg => {
          fieldErrors.push(`${fieldName}: ${msg}`)
        })
      }
    }
    
    if (fieldErrors.length > 0) {
      return {
        message: fieldErrors[0], // Show first error as main message
        details: fieldErrors.length > 1 ? fieldErrors.slice(1) : undefined,
        originalError: error
      }
    }
  }

  // Handle single error message
  if (errorData?.message) {
    return {
      message: errorData.message,
      originalError: error
    }
  }

  // Handle error field
  if (errorData?.error) {
    return {
      message: errorData.error,
      originalError: error
    }
  }

  // Handle title field (common in validation responses)
  if (errorData?.title) {
    return {
      message: errorData.title,
      originalError: error
    }
  }

  // Handle network errors
  if (error?.name === 'FetchError' || error?.message?.includes('fetch')) {
    return {
      message: 'Error de conexión. Verifica tu conexión a internet.',
      originalError: error
    }
  }

  // Handle timeout errors
  if (error?.message?.includes('timeout')) {
    return {
      message: 'La petición tardó demasiado tiempo. Inténtalo de nuevo.',
      originalError: error
    }
  }

  // Handle status-specific errors
  if (error?.status || errorData?.status) {
    const status = error?.status || errorData?.status
    switch (status) {
      case 400:
        return {
          message: 'Solicitud inválida. Verifica los datos enviados.',
          originalError: error
        }
      case 401:
        return {
          message: 'No tienes permisos para realizar esta acción.',
          originalError: error
        }
      case 403:
        return {
          message: 'Acceso denegado.',
          originalError: error
        }
      case 404:
        return {
          message: 'El recurso solicitado no se encontró.',
          originalError: error
        }
      case 409:
        return {
          message: 'El recurso ya existe o hay un conflicto.',
          originalError: error
        }
      case 413:
        return {
          message: 'El archivo es demasiado grande.',
          originalError: error
        }
      case 415:
        return {
          message: 'Formato de archivo no soportado.',
          originalError: error
        }
      case 422:
        return {
          message: 'Los datos enviados no son válidos.',
          originalError: error
        }
      case 429:
        return {
          message: 'Demasiadas peticiones. Inténtalo más tarde.',
          originalError: error
        }
      case 500:
        return {
          message: 'Error interno del servidor. Inténtalo más tarde.',
          originalError: error
        }
      case 502:
      case 503:
      case 504:
        return {
          message: 'Servicio temporalmente no disponible. Inténtalo más tarde.',
          originalError: error
        }
    }
  }

  // Fallback to generic error
  return {
    message: 'Ha ocurrido un error inesperado. Inténtalo de nuevo.',
    originalError: error
  }
}

/**
 * Get user-friendly field names for display
 */
function getFieldDisplayName(fieldName: string): string {
  const fieldMappings: Record<string, string> = {
    'imagen_url': 'URL de imagen',
    'imagen_alt': 'Texto alternativo',
    'codigo': 'Código',
    'descripcion': 'Descripción',
    'codigo_rubro': 'Código de rubro',
    'precio': 'Precio',
    'existencia': 'Stock',
    'nombre': 'Nombre',
    'email': 'Email',
    'telefono': 'Teléfono',
    'direccion': 'Dirección',
    'cuit': 'CUIT',
    'razon_social': 'Razón social',
    'dominio_personalizado': 'Dominio personalizado',
    'fecha_vencimiento': 'Fecha de vencimiento'
  }

  return fieldMappings[fieldName] || fieldName
}

/**
 * Format error message for toast notifications
 */
export function formatErrorForToast(error: any): { title: string; description?: string } {
  const parsed = parseApiError(error)
  
  return {
    title: 'Error',
    description: parsed.message
  }
}

/**
 * Get all error messages as a single string
 */
export function getAllErrorMessages(error: any): string {
  const parsed = parseApiError(error)
  
  if (parsed.details && parsed.details.length > 0) {
    return [parsed.message, ...parsed.details].join('\n')
  }
  
  return parsed.message
}