import { ref, computed, watch } from 'vue'
import { defineStore } from 'pinia'
import { EMPRESA_CONFIG } from '@/config/empresa.config'
import { authApiService } from '@/services/api'

export interface CartItem {
  codigo: string
  nombre: string
  precio: number
  cantidad: number
  imagen_urls?: string[]
  lista?: string
}

export interface OrderData {
  observaciones?: string
  direccion_entrega?: string
  fecha_entrega?: string
  horario_entrega?: string
  delivery_slot?: string
}

export interface OrderItem {
  codigo_producto: string
  cantidad: number
  precio_unitario: number
  nombre_producto?: string
  observaciones?: string | null
}

export interface CreateOrderRequest {
  items: OrderItem[]
  observaciones?: string
  direccion_entrega?: string
  fecha_entrega?: string
  horario_entrega?: string
  delivery_slot?: string
}

export interface OrderResponse {
  id: number
  cliente_id: number
  empresa_id: number
  numero: string
  fecha_pedido: string
  fecha_entrega?: string
  horario_entrega?: string
  direccion_entrega?: string
  observaciones?: string
  monto_total: number
  estado: string
  motivo_rechazo?: string | null
  usuario_gestion_id?: number | null
  fecha_gestion?: string | null
  created_at: string
  updated_at: string
  cliente_nombre: string
  cliente_email: string
  cliente_telefono: string
  items: {
    id: number
    pedido_id: number
    codigo_producto: string
    nombre_producto: string
    cantidad: number
    precio_unitario: number
    subtotal: number
    observaciones?: string | null
  }[]
}

export const useCartStore = defineStore('cart', () => {
  // State
  const items = ref<CartItem[]>([])
  
  // Load from localStorage on initialization
  const loadFromStorage = () => {
    try {
      const stored = localStorage.getItem('shopping-cart')
      if (stored) {
        items.value = JSON.parse(stored)
      }
    } catch (error) {
      console.error('Error loading cart from storage:', error)
      items.value = []
    }
  }
  
  // Save to localStorage whenever items change
  const saveToStorage = () => {
    try {
      localStorage.setItem('shopping-cart', JSON.stringify(items.value))
    } catch (error) {
      console.error('Error saving cart to storage:', error)
    }
  }
  
  // Watch for changes and auto-save
  watch(items, saveToStorage, { deep: true })
  
  // Getters
  const totalItems = computed(() => 
    items.value.reduce((total, item) => total + item.cantidad, 0)
  )
  
  const totalAmount = computed(() => 
    items.value.reduce((total, item) => total + (item.precio * item.cantidad), 0)
  )
  
  const itemCount = computed(() => items.value.length)
  
  const isEmpty = computed(() => items.value.length === 0)
  
  const getItemByCode = (codigo: string) => {
    return items.value.find(item => item.codigo === codigo)
  }
  
  // Actions
  const addItem = (product: any, cantidad: number = 1) => {
    if (cantidad <= 0) return
    
    const existingItem = getItemByCode(product.codigo)
    
    if (existingItem) {
      // Update existing item quantity
      existingItem.cantidad += cantidad
    } else {
      // Add new item
      const newItem: CartItem = {
        codigo: product.codigo,
        nombre: product.nombre,
        precio: product.precio || 0,
        cantidad,
        imagen_urls: product.imagen_urls,
        lista: product.lista
      }
      items.value.push(newItem)
    }
  }
  
  const updateQuantity = (codigo: string, cantidad: number) => {
    if (cantidad <= 0) {
      removeItem(codigo)
      return
    }
    
    const item = getItemByCode(codigo)
    if (item) {
      item.cantidad = cantidad
    }
  }
  
  const removeItem = (codigo: string) => {
    const index = items.value.findIndex(item => item.codigo === codigo)
    if (index > -1) {
      items.value.splice(index, 1)
    }
  }
  
  const clearCart = () => {
    items.value = []
  }
  
  const incrementItem = (codigo: string, amount: number = 1) => {
    const item = getItemByCode(codigo)
    if (item) {
      item.cantidad += amount
    }
  }
  
  const decrementItem = (codigo: string, amount: number = 1) => {
    const item = getItemByCode(codigo)
    if (item) {
      const newQuantity = item.cantidad - amount
      if (newQuantity <= 0) {
        removeItem(codigo)
      } else {
        item.cantidad = newQuantity
      }
    }
  }
  
  // Export functions
  const exportToText = () => {
    if (isEmpty.value) return ''
    
    const companyName = EMPRESA_CONFIG.nombre
    
    let text = `Lista de Compras - ${companyName}\n`
    text += '==========================================\n\n'
    
    items.value.forEach(item => {
      const total = item.precio * item.cantidad
      text += `${item.nombre}\n`
      text += `  Código: ${item.codigo}\n`
      text += `  Cantidad: ${item.cantidad}\n`
      text += `  Precio unitario: $${item.precio.toFixed(2)}\n`
      text += `  Subtotal: $${total.toFixed(2)}\n\n`
    })
    
    text += `Total items: ${totalItems.value}\n`
    text += `Total general: $${totalAmount.value.toFixed(2)}\n\n`
    text += `* Los precios mostrados están sujetos a cambios por parte del vendedor. Los precios en esta página web pueden no reflejar los precios finales del local.\n`
    
    return text
  }
  
  const exportForWhatsApp = () => {
    if (isEmpty.value) return ''
    
    const companyName = EMPRESA_CONFIG.nombre
    
    let message = `*Lista de Compras - ${companyName}*\n\n`
    
    items.value.forEach(item => {
      const total = item.precio * item.cantidad
      message += `• *${item.nombre}*\n`
      message += `   Código: ${item.codigo}\n`
      message += `   Cantidad: ${item.cantidad}\n`
      message += `   Precio: $${item.precio.toFixed(2)} c/u\n`
      message += `   Subtotal: $${total.toFixed(2)}\n\n`
    })
    
    message += `*Total: ${totalItems.value} productos*\n`
    message += `*Total general: $${totalAmount.value.toFixed(2)}*\n\n`
    message += `_* Los precios mostrados están sujetos a cambios por parte del vendedor. Los precios en esta página web pueden no reflejar los precios finales del local._`
    
    return encodeURIComponent(message)
  }
  
  const exportForEmail = () => {
    if (isEmpty.value) return ''
    
    const companyName = EMPRESA_CONFIG.nombre
    
    let body = `Lista de Compras - ${companyName}\n\n`
    
    items.value.forEach(item => {
      const total = item.precio * item.cantidad
      body += `${item.nombre}\n`
      body += `Código: ${item.codigo}\n`
      body += `Cantidad: ${item.cantidad}\n`
      body += `Precio unitario: $${item.precio.toFixed(2)}\n`
      body += `Subtotal: $${total.toFixed(2)}\n\n`
    })
    
    body += `Total items: ${totalItems.value}\n`
    body += `Total general: $${totalAmount.value.toFixed(2)}\n\n`
    body += `* Los precios mostrados están sujetos a cambios por parte del vendedor. Los precios en esta página web pueden no reflejar los precios finales del local.`
    
    return encodeURIComponent(body)
  }
  
  const exportForWhatsAppPedido = (companyName: string, messageTemplate?: string, customerData?: any) => {
    if (isEmpty.value) return ''
    
    // Default template if not provided
    const defaultTemplate = 'Hola, quiero hacer el siguiente pedido:\n{{items}}\nTotal: ${{total}}'
    const template = messageTemplate || defaultTemplate
    
    // Build customer data header if provided
    let customerInfo = ''
    if (customerData && Object.keys(customerData).length > 0) {
      // Field mapping with emojis for known fields
      const fieldIcons: Record<string, string> = {
        nombre: '👤',
        numero_cliente: '🆔',
        telefono: '📱',
        direccion_entrega: '📍',
        direccion: '📍',
        email: '📧',
        observaciones: '💬',
        dni: '🪪',
        cuit: '🏢',
        empresa: '🏢',
        horario: '🕐',
        fecha_entrega: '📅'
      }
      
      // Field display names
      const fieldLabels: Record<string, string> = {
        nombre: 'Nombre',
        numero_cliente: 'N° Cliente',
        telefono: 'Teléfono',
        direccion_entrega: 'Dirección',
        direccion: 'Dirección',
        email: 'Email',
        observaciones: 'Observaciones',
        dni: 'DNI',
        cuit: 'CUIT',
        empresa: 'Empresa',
        horario: 'Horario',
        fecha_entrega: 'Fecha de entrega'
      }
      
      customerInfo = '📋 *DATOS DEL CLIENTE*\n'
      
      // Process all fields dynamically
      Object.entries(customerData).forEach(([key, value]) => {
        if (value) {
          const icon = fieldIcons[key] || '▪️'
          const label = fieldLabels[key] || key.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase())
          customerInfo += `${icon} ${label}: ${value}\n`
        }
      })
      
      customerInfo += '\n'
    }
    
    // Build items list
    let itemsList = ''
    items.value.forEach((item, index) => {
      const total = item.precio * item.cantidad
      itemsList += `*${index + 1}. ${item.nombre}*\n`
      itemsList += `   📦 Código: ${item.codigo}\n`
      itemsList += `   🔢 Cantidad: ${item.cantidad}\n`
      itemsList += `   💰 Precio: $${item.precio.toFixed(2)} c/u\n`
      itemsList += `   💵 Subtotal: $${total.toFixed(2)}\n\n`
    })
    
    // Replace placeholders in template
    let message = template
      .replace('{{items}}', itemsList.trim())
      .replace('{{total}}', totalAmount.value.toFixed(2))
      .replace('{{empresa}}', companyName)
      .replace('{{total_productos}}', totalItems.value.toString())
    
    // Add customer data at the beginning of the message
    if (customerInfo) {
      message = customerInfo + message
    }
    
    return encodeURIComponent(message)
  }

  // Order functions
  const createOrder = async (orderData: OrderData, accessToken: string): Promise<OrderResponse> => {
    if (isEmpty.value) {
      throw new Error('El carrito está vacío')
    }

    // Convert cart items to order items with product names
    const orderItems: OrderItem[] = items.value.map(item => ({
      codigo_producto: item.codigo,
      cantidad: item.cantidad,
      precio_unitario: item.precio,
      nombre_producto: item.nombre,
      observaciones: null
    }))

    const orderRequest: CreateOrderRequest = {
      items: orderItems,
      observaciones: orderData.observaciones,
      direccion_entrega: orderData.direccion_entrega,
      fecha_entrega: orderData.fecha_entrega,
      horario_entrega: orderData.horario_entrega,
      delivery_slot: orderData.delivery_slot
    }

    try {
      const response = await authApiService.createOrder(orderRequest, accessToken)
      
      // Clear cart after successful order
      clearCart()
      
      return response
    } catch (error) {
      console.error('Error creating order:', error)
      throw error
    }
  }

  const getOrderHistory = async (accessToken: string): Promise<OrderResponse[]> => {
    try {
      const paginatedResponse = await authApiService.getOrderHistory(accessToken)
      return paginatedResponse.items
    } catch (error) {
      console.error('Error fetching order history:', error)
      throw error
    }
  }
  
  // Initialize from storage
  loadFromStorage()
  
  return {
    // State
    items,
    
    // Getters
    totalItems,
    totalAmount,
    itemCount,
    isEmpty,
    getItemByCode,
    
    // Actions
    addItem,
    updateQuantity,
    removeItem,
    clearCart,
    incrementItem,
    decrementItem,
    
    // Export functions
    exportToText,
    exportForWhatsApp,
    exportForWhatsAppPedido,
    exportForEmail,
    
    // Order functions
    createOrder,
    getOrderHistory,
    
    // Utility
    loadFromStorage,
    saveToStorage
  }
})