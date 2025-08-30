export interface UserNotificationPreferences {
  id?: number
  user_id: number
  notificacion_nuevos_pedidos: boolean
  notificacion_correcciones_aprobadas: boolean
  notificacion_correcciones_rechazadas: boolean
  notificacion_pedidos_cancelados: boolean
  created_at?: string
  updated_at?: string
  created_by?: string
  updated_by?: string
}

export const useNotificationPreferences = () => {
  const api = useApi()

  // Obtener las preferencias del usuario actual
  const getMyPreferences = async (): Promise<UserNotificationPreferences> => {
    const response = await api.get<UserNotificationPreferences>('/api/usernotificationpreferences')
    return response
  }

  // Actualizar las preferencias del usuario actual
  const updateMyPreferences = async (preferences: Partial<UserNotificationPreferences>): Promise<UserNotificationPreferences> => {
    const payload = {
      notificacion_nuevos_pedidos: preferences.notificacion_nuevos_pedidos,
      notificacion_correcciones_aprobadas: preferences.notificacion_correcciones_aprobadas,
      notificacion_correcciones_rechazadas: preferences.notificacion_correcciones_rechazadas,
      notificacion_pedidos_cancelados: preferences.notificacion_pedidos_cancelados
    }

    const response = await api.put<UserNotificationPreferences>('/api/usernotificationpreferences', payload)
    return response
  }

  return {
    getMyPreferences,
    updateMyPreferences
  }
}