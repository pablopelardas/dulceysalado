import { authApiService } from './api'

export interface ItemCorreccion {
  codigo_producto: string
  nombre_producto: string
  cantidad: number
  precio_unitario: number
  subtotal: number
  motivo?: string
}

export interface PedidoData {
  numero: string
  items: ItemCorreccion[]
  monto_total: number
  motivo_correccion?: string
}

export interface CorreccionData {
  token: string
  pedido_id: number
  pedido_numero: string
  fecha_expiracion: string
  es_valido: boolean
  cliente_nombre: string
  cliente_email: string
  pedido_original: {
    items: ItemCorreccion[]
    monto_total: number
  }
  pedido_corregido: {
    items: ItemCorreccion[]
    monto_total: number
    motivo_correccion?: string
  }
  historial_correcciones: CorreccionToken[]
}

export interface CorreccionToken {
  id: number
  token: string
  fecha_creacion: string
  fecha_expiracion: string
  utilizado: boolean
  fecha_uso?: string
  motivo_correccion?: string
  pedido_original_json?: string
}

export interface RespuestaCorreccion {
  comentario?: string
}

class CorreccionService {
  async obtenerCorreccion(token: string): Promise<CorreccionData> {
    try {
      const response = await authApiService.publicFetch(`/api/public/correccion/${token}`)
      return response
    } catch (error: any) {
      if (error.status === 404) {
        throw new Error('Token de corrección no válido o expirado')
      }
      throw new Error('Error al obtener la corrección')
    }
  }

  async aprobarCorreccion(token: string, comentario?: string): Promise<void> {
    try {
      const payload: RespuestaCorreccion = {}
      if (comentario?.trim()) {
        payload.comentario = comentario.trim()
      }

      await authApiService.publicFetch(`/api/public/correccion/${token}/aprobar`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      })
    } catch (error: any) {
      if (error.status === 400) {
        throw new Error('No se pudo procesar la respuesta')
      }
      throw new Error('Error al aprobar la corrección')
    }
  }

  async rechazarCorreccion(token: string, comentario?: string): Promise<void> {
    try {
      const payload: RespuestaCorreccion = {}
      if (comentario?.trim()) {
        payload.comentario = comentario.trim()
      }

      await authApiService.publicFetch(`/api/public/correccion/${token}/rechazar`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      })
    } catch (error: any) {
      if (error.status === 400) {
        throw new Error('No se pudo procesar la respuesta')
      }
      throw new Error('Error al rechazar la corrección')
    }
  }
}

export const correccionService = new CorreccionService()