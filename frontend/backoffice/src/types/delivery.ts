export enum SlotType {
  Morning = 0,
  Afternoon = 1
}

export interface DeliverySettings {
  id: number
  empresa_id: number
  min_slots_ahead: number
  max_capacity_morning: number
  max_capacity_afternoon: number
  
  // Configuración por día - Lunes
  monday_enabled: boolean
  monday_morning_start: string | null
  monday_morning_end: string | null
  monday_afternoon_start: string | null
  monday_afternoon_end: string | null
  
  // Configuración por día - Martes
  tuesday_enabled: boolean
  tuesday_morning_start: string | null
  tuesday_morning_end: string | null
  tuesday_afternoon_start: string | null
  tuesday_afternoon_end: string | null
  
  // Configuración por día - Miércoles
  wednesday_enabled: boolean
  wednesday_morning_start: string | null
  wednesday_morning_end: string | null
  wednesday_afternoon_start: string | null
  wednesday_afternoon_end: string | null
  
  // Configuración por día - Jueves
  thursday_enabled: boolean
  thursday_morning_start: string | null
  thursday_morning_end: string | null
  thursday_afternoon_start: string | null
  thursday_afternoon_end: string | null
  
  // Configuración por día - Viernes
  friday_enabled: boolean
  friday_morning_start: string | null
  friday_morning_end: string | null
  friday_afternoon_start: string | null
  friday_afternoon_end: string | null
  
  // Configuración por día - Sábado
  saturday_enabled: boolean
  saturday_morning_start: string | null
  saturday_morning_end: string | null
  saturday_afternoon_start: string | null
  saturday_afternoon_end: string | null
  
  // Configuración por día - Domingo
  sunday_enabled: boolean
  sunday_morning_start: string | null
  sunday_morning_end: string | null
  sunday_afternoon_start: string | null
  sunday_afternoon_end: string | null
  
  created_at: string
  updated_at: string
  schedules?: DeliverySchedule[]
  slots?: DeliverySlot[]
}

export interface DeliverySchedule {
  id: number
  delivery_settings_id: number
  date: string
  morning_enabled: boolean
  afternoon_enabled: boolean
  custom_max_capacity_morning?: number | null
  custom_max_capacity_afternoon?: number | null
  custom_morning_start_time?: string | null
  custom_morning_end_time?: string | null
  custom_afternoon_start_time?: string | null
  custom_afternoon_end_time?: string | null
  created_at: string
  updated_at: string
  slots?: DeliverySlot[]
}

export interface DeliverySlot {
  id: number
  delivery_settings_id: number
  delivery_schedule_id?: number
  date: string
  slot_type: SlotType
  slot_type_name: string
  current_capacity: number
  max_capacity: number
  is_available: boolean
  created_at: string
  updated_at: string
}

export interface AvailableDeliverySlot {
  date: string
  morning_slots: SlotAvailability[]
  afternoon_slots: SlotAvailability[]
}

export interface SlotAvailability {
  slot_type: SlotType
  slot_type_name: string
  time_range: string
  is_available: boolean
  current_capacity: number
  max_capacity: number
  remaining_capacity: number
}

export interface CreateDeliverySettingsRequest {
  empresa_id: number
  min_slots_ahead?: number
  max_capacity_morning?: number
  max_capacity_afternoon?: number
  
  // Configuración por día - Lunes
  monday_enabled?: boolean
  monday_morning_start?: string | null
  monday_morning_end?: string | null
  monday_afternoon_start?: string | null
  monday_afternoon_end?: string | null
  
  // Configuración por día - Martes
  tuesday_enabled?: boolean
  tuesday_morning_start?: string | null
  tuesday_morning_end?: string | null
  tuesday_afternoon_start?: string | null
  tuesday_afternoon_end?: string | null
  
  // Configuración por día - Miércoles
  wednesday_enabled?: boolean
  wednesday_morning_start?: string | null
  wednesday_morning_end?: string | null
  wednesday_afternoon_start?: string | null
  wednesday_afternoon_end?: string | null
  
  // Configuración por día - Jueves
  thursday_enabled?: boolean
  thursday_morning_start?: string | null
  thursday_morning_end?: string | null
  thursday_afternoon_start?: string | null
  thursday_afternoon_end?: string | null
  
  // Configuración por día - Viernes
  friday_enabled?: boolean
  friday_morning_start?: string | null
  friday_morning_end?: string | null
  friday_afternoon_start?: string | null
  friday_afternoon_end?: string | null
  
  // Configuración por día - Sábado
  saturday_enabled?: boolean
  saturday_morning_start?: string | null
  saturday_morning_end?: string | null
  saturday_afternoon_start?: string | null
  saturday_afternoon_end?: string | null
  
  // Configuración por día - Domingo
  sunday_enabled?: boolean
  sunday_morning_start?: string | null
  sunday_morning_end?: string | null
  sunday_afternoon_start?: string | null
  sunday_afternoon_end?: string | null
}

export interface UpdateDeliverySettingsRequest {
  min_slots_ahead: number
  max_capacity_morning: number
  max_capacity_afternoon: number
  
  // Configuración por día - Lunes
  monday_enabled: boolean
  monday_morning_start: string | null
  monday_morning_end: string | null
  monday_afternoon_start: string | null
  monday_afternoon_end: string | null
  
  // Configuración por día - Martes
  tuesday_enabled: boolean
  tuesday_morning_start: string | null
  tuesday_morning_end: string | null
  tuesday_afternoon_start: string | null
  tuesday_afternoon_end: string | null
  
  // Configuración por día - Miércoles
  wednesday_enabled: boolean
  wednesday_morning_start: string | null
  wednesday_morning_end: string | null
  wednesday_afternoon_start: string | null
  wednesday_afternoon_end: string | null
  
  // Configuración por día - Jueves
  thursday_enabled: boolean
  thursday_morning_start: string | null
  thursday_morning_end: string | null
  thursday_afternoon_start: string | null
  thursday_afternoon_end: string | null
  
  // Configuración por día - Viernes
  friday_enabled: boolean
  friday_morning_start: string | null
  friday_morning_end: string | null
  friday_afternoon_start: string | null
  friday_afternoon_end: string | null
  
  // Configuración por día - Sábado
  saturday_enabled: boolean
  saturday_morning_start: string | null
  saturday_morning_end: string | null
  saturday_afternoon_start: string | null
  saturday_afternoon_end: string | null
  
  // Configuración por día - Domingo
  sunday_enabled: boolean
  sunday_morning_start: string | null
  sunday_morning_end: string | null
  sunday_afternoon_start: string | null
  sunday_afternoon_end: string | null
}

export interface CreateDeliveryScheduleRequest {
  delivery_settings_id: number
  date: string
  morning_enabled?: boolean
  afternoon_enabled?: boolean
  custom_max_capacity_morning?: number | null
  custom_max_capacity_afternoon?: number | null
  custom_morning_start_time?: string | null
  custom_morning_end_time?: string | null
  custom_afternoon_start_time?: string | null
  custom_afternoon_end_time?: string | null
}

export interface UpdateDeliveryScheduleRequest {
  morning_enabled: boolean
  afternoon_enabled: boolean
  custom_max_capacity_morning?: number | null
  custom_max_capacity_afternoon?: number | null
  custom_morning_start_time?: string | null
  custom_morning_end_time?: string | null
  custom_afternoon_start_time?: string | null
  custom_afternoon_end_time?: string | null
}

export interface ReserveSlotRequest {
  date: string
  slot_type: SlotType
  pedido_id: number
}

export interface SlotAvailabilityCheck {
  is_available: boolean
  remaining_capacity: number
  max_capacity: number
  message: string
}