export interface Pedido {
  id: number
  numero: string
  fecha_pedido: string
  fecha_entrega: string // Fecha solicitada de entrega
  horario_entrega: string // Franja horaria de entrega
  direccion_entrega: string // Dirección de entrega
  estado: string // Backend envía string, no enum
  cliente_id: number
  cliente_nombre: string
  cliente_email: string
  cliente_telefono?: string
  monto_total: number // Backend usa monto_total, no total
  observaciones?: string // Backend usa observaciones, no comentarios
  fecha_aceptado?: string
  fecha_rechazado?: string
  fecha_completado?: string
  motivo_rechazo?: string
  usuario_gestion_id?: number
  usuario_gestion_nombre?: string
  items: PedidoItem[]
  correcciones: CorreccionToken[]
  created_at: string
  updated_at: string
}

export interface PedidoItem {
  id: number
  pedido_id: number
  codigo_producto: string // Backend usa codigo_producto
  nombre_producto: string // Backend usa nombre_producto
  cantidad: number
  precio_unitario: number
  subtotal: number
  observaciones?: string // Backend incluye observaciones
}

export enum PedidoEstado {
  Pendiente = "Pendiente",
  Aceptado = "Aceptado", 
  Rechazado = "Rechazado",
  Completado = "Completado",
  Cancelado = "Cancelado",
  EnCorreccion = "EnCorreccion",
  CorreccionPendiente = "CorreccionPendiente",
  CorreccionRechazada = "CorreccionRechazada"
}

export interface PedidosPagedResult {
  items: Pedido[]
  total_count: number
  page: number
  page_size: number
  total_pages: number
}

export interface PedidoEstadisticas {
  total_pedidos?: number // Calculado del total
  total_pendientes: number // Backend usa total_pendientes
  total_aceptados: number // Backend usa total_aceptados
  total_rechazados: number // Backend usa total_rechazados
  total_completados: number // Backend usa total_completados
  total_cancelados: number // Backend incluye cancelados
  monto_total_hoy: number // Backend usa monto_total_*
  monto_total_semana: number
  monto_total_mes: number
}

export interface PedidoFiltros {
  page?: number
  pageSize?: number
  estado?: string // Cambiar a string para coincidir con backend
  fechaDesde?: string
  fechaHasta?: string
  clienteId?: number
  numeroContiene?: string
}

export interface RechazarPedidoRequest {
  motivo: string
}

export interface CorregirPedidoRequest {
  items: ItemCorreccion[]
  motivo_correccion?: string
  enviar_al_cliente: boolean
}

export interface ItemCorreccion {
  codigo_producto: string
  nueva_cantidad: number
  motivo?: string
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

// Helpers para mostrar estados
export const ESTADOS_PEDIDO = {
  [PedidoEstado.Pendiente]: { label: 'Pendiente', color: 'orange' as const, icon: 'i-heroicons-clock' },
  [PedidoEstado.Aceptado]: { label: 'Aceptado', color: 'blue' as const, icon: 'i-heroicons-check-circle' },
  [PedidoEstado.Rechazado]: { label: 'Rechazado', color: 'red' as const, icon: 'i-heroicons-x-circle' },
  [PedidoEstado.Completado]: { label: 'Completado', color: 'green' as const, icon: 'i-heroicons-check-badge' },
  [PedidoEstado.Cancelado]: { label: 'Cancelado', color: 'gray' as const, icon: 'i-heroicons-x-circle' },
  [PedidoEstado.EnCorreccion]: { label: 'En Corrección', color: 'yellow' as const, icon: 'i-heroicons-pencil' },
  [PedidoEstado.CorreccionPendiente]: { label: 'Corrección Pendiente', color: 'purple' as const, icon: 'i-heroicons-clock' },
  [PedidoEstado.CorreccionRechazada]: { label: 'Corrección Rechazada', color: 'red' as const, icon: 'i-heroicons-x-mark' }
}

export const formatEstadoPedido = (estado: string) => {
  return ESTADOS_PEDIDO[estado as keyof typeof ESTADOS_PEDIDO] || { label: 'Desconocido', color: 'gray' as const, icon: 'i-heroicons-question-mark-circle' }
}