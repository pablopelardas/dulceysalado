<script setup lang="ts">
import type { DeliverySettings, CreateDeliverySettingsRequest, UpdateDeliverySettingsRequest } from '~/types/delivery'
import { z } from 'zod'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Configuración de Entregas',
  meta: [
    { name: 'description', content: 'Configuración del sistema de entregas' }
  ]
})

const { user, empresa } = useAuth()
const { 
  deliverySettings, 
  deliverySchedules,
  loading, 
  error, 
  fetchDeliverySettings, 
  createDeliverySettings, 
  updateDeliverySettings,
  fetchDeliverySchedules,
  createDeliverySchedule,
  updateDeliverySchedule,
  deleteDeliverySchedule
} = useDelivery()

const toast = useToast()

// Esquema de validación
const schema = z.object({
  min_slots_ahead: z.number().min(0).max(30),
  max_capacity_morning: z.number().min(1).max(100),
  max_capacity_afternoon: z.number().min(1).max(100),
  
  // Configuración por día - Lunes
  monday_enabled: z.boolean(),
  monday_morning_start: z.string().nullable(),
  monday_morning_end: z.string().nullable(),
  monday_afternoon_start: z.string().nullable(),
  monday_afternoon_end: z.string().nullable(),
  
  // Configuración por día - Martes
  tuesday_enabled: z.boolean(),
  tuesday_morning_start: z.string().nullable(),
  tuesday_morning_end: z.string().nullable(),
  tuesday_afternoon_start: z.string().nullable(),
  tuesday_afternoon_end: z.string().nullable(),
  
  // Configuración por día - Miércoles
  wednesday_enabled: z.boolean(),
  wednesday_morning_start: z.string().nullable(),
  wednesday_morning_end: z.string().nullable(),
  wednesday_afternoon_start: z.string().nullable(),
  wednesday_afternoon_end: z.string().nullable(),
  
  // Configuración por día - Jueves
  thursday_enabled: z.boolean(),
  thursday_morning_start: z.string().nullable(),
  thursday_morning_end: z.string().nullable(),
  thursday_afternoon_start: z.string().nullable(),
  thursday_afternoon_end: z.string().nullable(),
  
  // Configuración por día - Viernes
  friday_enabled: z.boolean(),
  friday_morning_start: z.string().nullable(),
  friday_morning_end: z.string().nullable(),
  friday_afternoon_start: z.string().nullable(),
  friday_afternoon_end: z.string().nullable(),
  
  // Configuración por día - Sábado
  saturday_enabled: z.boolean(),
  saturday_morning_start: z.string().nullable(),
  saturday_morning_end: z.string().nullable(),
  saturday_afternoon_start: z.string().nullable(),
  saturday_afternoon_end: z.string().nullable(),
  
  // Configuración por día - Domingo
  sunday_enabled: z.boolean(),
  sunday_morning_start: z.string().nullable(),
  sunday_morning_end: z.string().nullable(),
  sunday_afternoon_start: z.string().nullable(),
  sunday_afternoon_end: z.string().nullable()
})

// Estados del formulario - usar reactive para UForm
const formData = reactive<CreateDeliverySettingsRequest | UpdateDeliverySettingsRequest>({
  min_slots_ahead: 2,
  max_capacity_morning: 10,
  max_capacity_afternoon: 10,
  
  // Configuración por día - Lunes
  monday_enabled: true,
  monday_morning_start: '09:00',
  monday_morning_end: '13:00',
  monday_afternoon_start: '14:00',
  monday_afternoon_end: '18:00',
  
  // Configuración por día - Martes
  tuesday_enabled: true,
  tuesday_morning_start: '09:00',
  tuesday_morning_end: '13:00',
  tuesday_afternoon_start: '14:00',
  tuesday_afternoon_end: '18:00',
  
  // Configuración por día - Miércoles
  wednesday_enabled: true,
  wednesday_morning_start: '09:00',
  wednesday_morning_end: '13:00',
  wednesday_afternoon_start: '14:00',
  wednesday_afternoon_end: '18:00',
  
  // Configuración por día - Jueves
  thursday_enabled: true,
  thursday_morning_start: '09:00',
  thursday_morning_end: '13:00',
  thursday_afternoon_start: '14:00',
  thursday_afternoon_end: '18:00',
  
  // Configuración por día - Viernes
  friday_enabled: true,
  friday_morning_start: '09:00',
  friday_morning_end: '13:00',
  friday_afternoon_start: '14:00',
  friday_afternoon_end: '18:00',
  
  // Configuración por día - Sábado
  saturday_enabled: false,
  saturday_morning_start: '09:00',
  saturday_morning_end: '12:00',
  saturday_afternoon_start: null,
  saturday_afternoon_end: null,
  
  // Configuración por día - Domingo
  sunday_enabled: false,
  sunday_morning_start: null,
  sunday_morning_end: null,
  sunday_afternoon_start: null,
  sunday_afternoon_end: null
})

const isEditing = ref(false)
const isInitialized = ref(false)

// Cargar configuración existente
const loadSettings = async () => {
  console.log('loadSettings - empresa:', empresa.value)
  if (!empresa.value?.id) {
    console.log('No hay empresa o ID de empresa')
    return
  }
  
  try {
    console.log('Llamando fetchDeliverySettings con empresaId:', empresa.value.id)
    const settings = await fetchDeliverySettings(empresa.value.id)
    console.log('Settings recibidos del backend:', settings)
    console.log('deliverySettings del composable:', deliverySettings.value)
    
    if (settings) {
      isEditing.value = true
      // Actualizar configuración general
      formData.min_slots_ahead = settings.min_slots_ahead
      formData.max_capacity_morning = settings.max_capacity_morning
      formData.max_capacity_afternoon = settings.max_capacity_afternoon
      
      // Actualizar configuración por día - Lunes
      formData.monday_enabled = settings.monday_enabled
      formData.monday_morning_start = settings.monday_morning_start
      formData.monday_morning_end = settings.monday_morning_end
      formData.monday_afternoon_start = settings.monday_afternoon_start
      formData.monday_afternoon_end = settings.monday_afternoon_end
      
      // Actualizar configuración por día - Martes
      formData.tuesday_enabled = settings.tuesday_enabled
      formData.tuesday_morning_start = settings.tuesday_morning_start
      formData.tuesday_morning_end = settings.tuesday_morning_end
      formData.tuesday_afternoon_start = settings.tuesday_afternoon_start
      formData.tuesday_afternoon_end = settings.tuesday_afternoon_end
      
      // Actualizar configuración por día - Miércoles
      formData.wednesday_enabled = settings.wednesday_enabled
      formData.wednesday_morning_start = settings.wednesday_morning_start
      formData.wednesday_morning_end = settings.wednesday_morning_end
      formData.wednesday_afternoon_start = settings.wednesday_afternoon_start
      formData.wednesday_afternoon_end = settings.wednesday_afternoon_end
      
      // Actualizar configuración por día - Jueves
      formData.thursday_enabled = settings.thursday_enabled
      formData.thursday_morning_start = settings.thursday_morning_start
      formData.thursday_morning_end = settings.thursday_morning_end
      formData.thursday_afternoon_start = settings.thursday_afternoon_start
      formData.thursday_afternoon_end = settings.thursday_afternoon_end
      
      // Actualizar configuración por día - Viernes
      formData.friday_enabled = settings.friday_enabled
      formData.friday_morning_start = settings.friday_morning_start
      formData.friday_morning_end = settings.friday_morning_end
      formData.friday_afternoon_start = settings.friday_afternoon_start
      formData.friday_afternoon_end = settings.friday_afternoon_end
      
      // Actualizar configuración por día - Sábado
      formData.saturday_enabled = settings.saturday_enabled
      formData.saturday_morning_start = settings.saturday_morning_start
      formData.saturday_morning_end = settings.saturday_morning_end
      formData.saturday_afternoon_start = settings.saturday_afternoon_start
      formData.saturday_afternoon_end = settings.saturday_afternoon_end
      
      // Actualizar configuración por día - Domingo
      formData.sunday_enabled = settings.sunday_enabled
      formData.sunday_morning_start = settings.sunday_morning_start
      formData.sunday_morning_end = settings.sunday_morning_end
      formData.sunday_afternoon_start = settings.sunday_afternoon_start
      formData.sunday_afternoon_end = settings.sunday_afternoon_end
      
      console.log('FormData después de mapear:', formData)
      
      // Cargar fechas especiales si existe configuración
      try {
        await fetchDeliverySchedules(settings.id, true)
        // Convertir schedules del backend a formato UI
        fechasEspeciales.value = deliverySchedules.value.map(schedule => ({
          id: schedule.id,
          date: schedule.date,
          enabled: schedule.morning_enabled || schedule.afternoon_enabled,
          morning_enabled: schedule.morning_enabled,
          morning_start: schedule.custom_morning_start_time || '09:00',
          morning_end: schedule.custom_morning_end_time || '13:00',
          max_capacity_morning: schedule.custom_max_capacity_morning || formData.max_capacity_morning,
          afternoon_enabled: schedule.afternoon_enabled,
          afternoon_start: schedule.custom_afternoon_start_time || '14:00',
          afternoon_end: schedule.custom_afternoon_end_time || '18:00',
          max_capacity_afternoon: schedule.custom_max_capacity_afternoon || formData.max_capacity_afternoon
        }))
      } catch (err) {
        console.log('Error cargando fechas especiales:', err)
      }
    } else {
      console.log('Settings es undefined - verificar composable')
    }
  } catch (err) {
    // Si es 404, simplemente no hay configuración aún
    console.log('Error cargando settings:', err)
  } finally {
    isInitialized.value = true
  }
}

// Guardar configuración
const saveSettings = async (event: any) => {
  if (!empresa.value?.id) return

  try {
    console.log('Guardando configuración:', event.data)
    if (deliverySettings.value) {
      // Si ya existe configuración, actualizar
      await updateDeliverySettings(deliverySettings.value.id, event.data as UpdateDeliverySettingsRequest)
    } else {
      // Si no existe configuración, crear nueva
      const createData: CreateDeliverySettingsRequest = {
        empresa_id: empresa.value.id,
        ...event.data
      }
      await createDeliverySettings(createData)
      isEditing.value = true
    }

    // Guardar también las fechas especiales pendientes
    const fechasPendientes = fechasEspeciales.value.filter(fecha => 
      fecha.date && fecha.enabled && !fecha.id
    )
    
    for (const fecha of fechasPendientes) {
      try {
        await guardarFechaEspecial(fecha)
      } catch (err) {
        console.error('Error guardando fecha especial:', fecha.date, err)
      }
    }

    if (fechasPendientes.length > 0) {
      toast.add({
        title: 'Éxito',
        description: `Configuración actualizada y ${fechasPendientes.length} fecha(s) especial(es) guardada(s)`,
        color: 'green'
      })
    }
  } catch (err) {
    // El error ya se maneja en el composable
    console.error('Error guardando configuración:', err)
  }
}

// Fechas especiales
interface FechaEspecial {
  id?: number
  date: string
  enabled: boolean
  morning_enabled: boolean
  morning_start: string | null
  morning_end: string | null
  max_capacity_morning: number | null
  afternoon_enabled: boolean
  afternoon_start: string | null
  afternoon_end: string | null
  max_capacity_afternoon: number | null
}

const fechasEspeciales = ref<FechaEspecial[]>([])
const today = new Date().toISOString().split('T')[0]

const agregarFechaEspecial = () => {
  fechasEspeciales.value.push({
    date: '',
    enabled: true,
    morning_enabled: false,
    morning_start: '09:00',
    morning_end: '13:00',
    max_capacity_morning: formData.max_capacity_morning,
    afternoon_enabled: false,
    afternoon_start: '14:00',
    afternoon_end: '18:00',
    max_capacity_afternoon: formData.max_capacity_afternoon
  })
}

const eliminarFechaEspecial = async (index: number) => {
  const fecha = fechasEspeciales.value[index]
  
  if (fecha.id) {
    // Si tiene ID, eliminar del backend
    try {
      await deleteDeliverySchedule(fecha.id)
    } catch (err) {
      console.error('Error eliminando fecha especial:', err)
      return
    }
  }
  
  // Eliminar de la lista local
  fechasEspeciales.value.splice(index, 1)
}

// Guardar una fecha especial específica
const guardarFechaEspecial = async (fecha: FechaEspecial) => {
  if (!deliverySettings.value) return
  
  try {
    if (fecha.id) {
      // Actualizar existente
      const updateData: UpdateDeliveryScheduleRequest = {
        morning_enabled: fecha.morning_enabled,
        afternoon_enabled: fecha.afternoon_enabled,
        custom_max_capacity_morning: fecha.morning_enabled ? fecha.max_capacity_morning : null,
        custom_max_capacity_afternoon: fecha.afternoon_enabled ? fecha.max_capacity_afternoon : null,
        custom_morning_start_time: fecha.morning_enabled ? fecha.morning_start : null,
        custom_morning_end_time: fecha.morning_enabled ? fecha.morning_end : null,
        custom_afternoon_start_time: fecha.afternoon_enabled ? fecha.afternoon_start : null,
        custom_afternoon_end_time: fecha.afternoon_enabled ? fecha.afternoon_end : null
      }
      await updateDeliverySchedule(fecha.id, updateData)
    } else {
      // Crear nuevo
      const createData: CreateDeliveryScheduleRequest = {
        delivery_settings_id: deliverySettings.value.id,
        date: fecha.date,
        morning_enabled: fecha.morning_enabled,
        afternoon_enabled: fecha.afternoon_enabled,
        custom_max_capacity_morning: fecha.morning_enabled ? fecha.max_capacity_morning : null,
        custom_max_capacity_afternoon: fecha.afternoon_enabled ? fecha.max_capacity_afternoon : null,
        custom_morning_start_time: fecha.morning_enabled ? fecha.morning_start : null,
        custom_morning_end_time: fecha.morning_enabled ? fecha.morning_end : null,
        custom_afternoon_start_time: fecha.afternoon_enabled ? fecha.afternoon_start : null,
        custom_afternoon_end_time: fecha.afternoon_enabled ? fecha.afternoon_end : null
      }
      const created = await createDeliverySchedule(createData)
      // Actualizar el ID en la lista local
      fecha.id = created.id
    }
  } catch (err) {
    console.error('Error guardando fecha especial:', err)
    throw err
  }
}

// Watchers para guardado automático cuando se cambian los switches de días
const setupDayWatchers = () => {
  const dayKeys = [
    'monday_enabled', 'tuesday_enabled', 'wednesday_enabled', 'thursday_enabled',
    'friday_enabled', 'saturday_enabled', 'sunday_enabled'
  ]

  dayKeys.forEach(key => {
    watch(
      () => formData[key as keyof typeof formData],
      async (newValue, oldValue) => {
        // Solo guardar si ya se inicializó y el valor realmente cambió
        if (isInitialized.value && newValue !== oldValue && deliverySettings.value) {
          try {
            // Crear objeto con solo los campos necesarios para el update
            const updateData: UpdateDeliverySettingsRequest = { ...formData } as UpdateDeliverySettingsRequest
            await updateDeliverySettings(deliverySettings.value.id, updateData)
            
            toast.add({
              title: 'Configuración guardada',
              description: `Día ${getDayName(key)} ${newValue ? 'habilitado' : 'deshabilitado'}`,
              color: 'green'
            })
          } catch (err) {
            console.error('Error guardando configuración automáticamente:', err)
            toast.add({
              title: 'Error',
              description: 'No se pudo guardar la configuración automáticamente',
              color: 'red'
            })
          }
        }
      }
    )
  })
}

// Función auxiliar para obtener el nombre del día
const getDayName = (key: string): string => {
  const dayMap: Record<string, string> = {
    'monday_enabled': 'Lunes',
    'tuesday_enabled': 'Martes', 
    'wednesday_enabled': 'Miércoles',
    'thursday_enabled': 'Jueves',
    'friday_enabled': 'Viernes',
    'saturday_enabled': 'Sábado',
    'sunday_enabled': 'Domingo'
  }
  return dayMap[key] || ''
}

// Configurar watchers después de montar
onMounted(async () => {
  await loadSettings()
  setupDayWatchers()
})

// Días de la semana para el formulario
const diasSemana = [
  { 
    day: 'monday', 
    label: 'Lunes',
    enabled_key: 'monday_enabled',
    morning_start_key: 'monday_morning_start',
    morning_end_key: 'monday_morning_end', 
    afternoon_start_key: 'monday_afternoon_start',
    afternoon_end_key: 'monday_afternoon_end'
  },
  { 
    day: 'tuesday', 
    label: 'Martes',
    enabled_key: 'tuesday_enabled',
    morning_start_key: 'tuesday_morning_start',
    morning_end_key: 'tuesday_morning_end',
    afternoon_start_key: 'tuesday_afternoon_start', 
    afternoon_end_key: 'tuesday_afternoon_end'
  },
  { 
    day: 'wednesday', 
    label: 'Miércoles',
    enabled_key: 'wednesday_enabled',
    morning_start_key: 'wednesday_morning_start',
    morning_end_key: 'wednesday_morning_end',
    afternoon_start_key: 'wednesday_afternoon_start',
    afternoon_end_key: 'wednesday_afternoon_end'
  },
  { 
    day: 'thursday', 
    label: 'Jueves',
    enabled_key: 'thursday_enabled',
    morning_start_key: 'thursday_morning_start',
    morning_end_key: 'thursday_morning_end',
    afternoon_start_key: 'thursday_afternoon_start',
    afternoon_end_key: 'thursday_afternoon_end'
  },
  { 
    day: 'friday', 
    label: 'Viernes',
    enabled_key: 'friday_enabled',
    morning_start_key: 'friday_morning_start',
    morning_end_key: 'friday_morning_end',
    afternoon_start_key: 'friday_afternoon_start',
    afternoon_end_key: 'friday_afternoon_end'
  },
  { 
    day: 'saturday', 
    label: 'Sábado',
    enabled_key: 'saturday_enabled',
    morning_start_key: 'saturday_morning_start',
    morning_end_key: 'saturday_morning_end',
    afternoon_start_key: 'saturday_afternoon_start',
    afternoon_end_key: 'saturday_afternoon_end'
  },
  { 
    day: 'sunday', 
    label: 'Domingo',
    enabled_key: 'sunday_enabled',
    morning_start_key: 'sunday_morning_start',
    morning_end_key: 'sunday_morning_end',
    afternoon_start_key: 'sunday_afternoon_start',
    afternoon_end_key: 'sunday_afternoon_end'
  }
]
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
              Configuración de Entregas
            </h1>
            <p class="mt-2 text-gray-600 dark:text-gray-400">
              Configura horarios, capacidades y disponibilidad para el sistema de entregas
            </p>
          </div>
          <UButton 
            variant="outline" 
            to="/admin/pedidos"
            icon="i-heroicons-arrow-left"
          >
            Volver a Pedidos
          </UButton>
        </div>
      </div>

      <!-- Loading initial -->
      <div v-if="!isInitialized" class="flex justify-center py-12">
        <UIcon name="i-heroicons-arrow-path" class="w-8 h-8 animate-spin" />
      </div>

      <!-- Formulario de configuración -->
      <div v-else>
        <UForm :schema="schema" :state="formData" @submit="saveSettings" class="space-y-6">
          <!-- Configuración básica -->
          <UCard>
            <template #header>
              <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
                Configuración Básica
              </h3>
            </template>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6 items-end">
              <UFormField label="Franjas mínimas de anticipación" name="min_slots_ahead" required>
                <UInput 
                  v-model.number="formData.min_slots_ahead"
                  type="number"
                  :min="0"
                  :max="30"
                  placeholder="2"
                />
              </UFormField>

              <UFormField label="Capacidad máxima - Mañana" name="max_capacity_morning" required>
                <UInput 
                  v-model.number="formData.max_capacity_morning"
                  type="number"
                  :min="1"
                  :max="100"
                  placeholder="10"
                />
              </UFormField>

              <UFormField label="Capacidad máxima - Tarde" name="max_capacity_afternoon" required>
                <UInput 
                  v-model.number="formData.max_capacity_afternoon"
                  type="number"
                  :min="1"
                  :max="100"
                  placeholder="10"
                />
              </UFormField>
            </div>
          </UCard>

          <!-- Configuración por días -->
          <UCard>
            <template #header>
              <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
                Horarios por Día
              </h3>
            </template>
            
            <div class="space-y-6">
              <p class="text-sm text-gray-600 dark:text-gray-400">
                Configura los horarios específicos para cada día de la semana
              </p>
              
              <div class="space-y-4">
                <div v-for="dia in diasSemana" :key="dia.day" class="border border-gray-200 dark:border-gray-700 rounded-lg p-4">
                  <div class="flex items-center justify-between mb-4">
                    <div class="flex items-center gap-3">
                      <USwitch
                        v-model="formData[dia.enabled_key as keyof typeof formData]"
                        :id="`switch-${dia.day}`"
                        size="sm"
                      />
                      <label :for="`switch-${dia.day}`" class="text-base font-medium text-gray-900 dark:text-gray-100 cursor-pointer">
                        {{ dia.label }}
                      </label>
                    </div>
                  </div>
                  
                  <div v-if="formData[dia.enabled_key as keyof typeof formData]" class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <!-- Horario de mañana -->
                    <div class="space-y-2">
                      <label class="text-sm font-medium text-gray-700 dark:text-gray-300">Mañana</label>
                      <div class="flex gap-2 items-center">
                        <UFormField name="`${dia.morning_start_key}`">
                          <UInput
                            v-model="formData[dia.morning_start_key as keyof typeof formData]"
                            type="time"
                            placeholder="09:00"
                          />
                        </UFormField>
                        <span class="text-gray-500">a</span>
                        <UFormField name="`${dia.morning_end_key}`">
                          <UInput
                            v-model="formData[dia.morning_end_key as keyof typeof formData]"
                            type="time"
                            placeholder="13:00"
                          />
                        </UFormField>
                      </div>
                    </div>
                    
                    <!-- Horario de tarde -->
                    <div class="space-y-2">
                      <label class="text-sm font-medium text-gray-700 dark:text-gray-300">Tarde</label>
                      <div class="flex gap-2 items-center">
                        <UFormField name="`${dia.afternoon_start_key}`">
                          <UInput
                            v-model="formData[dia.afternoon_start_key as keyof typeof formData]"
                            type="time"
                            placeholder="14:00"
                          />
                        </UFormField>
                        <span class="text-gray-500">a</span>
                        <UFormField name="`${dia.afternoon_end_key}`">
                          <UInput
                            v-model="formData[dia.afternoon_end_key as keyof typeof formData]"
                            type="time"
                            placeholder="18:00"
                          />
                        </UFormField>
                      </div>
                      <p class="text-xs text-gray-500">Deja vacío si no hay horario de tarde</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </UCard>

          <!-- Fechas especiales -->
          <UCard>
            <template #header>
              <div class="flex items-center justify-between">
                <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
                  Fechas Especiales
                </h3>
                <UButton 
                  variant="soft" 
                  size="sm"
                  @click="agregarFechaEspecial"
                  icon="i-heroicons-plus"
                >
                  Agregar Fecha
                </UButton>
              </div>
            </template>
            
            <div class="space-y-4">
              <p class="text-sm text-gray-600 dark:text-gray-400">
                Configura horarios y capacidades específicas para fechas particulares que sobrescriban la configuración semanal
              </p>
              
              <div v-if="fechasEspeciales.length === 0" class="text-center py-8 text-gray-500">
                No hay fechas especiales configuradas
              </div>
              
              <div v-else class="space-y-4">
                <div 
                  v-for="(fecha, index) in fechasEspeciales" 
                  :key="index"
                  class="border border-gray-200 dark:border-gray-700 rounded-lg p-4"
                >
                  <div class="flex items-center justify-between mb-4">
                    <div class="flex items-center gap-3">
                      <UInput 
                        v-model="fecha.date"
                        type="date"
                        :min="today"
                        placeholder="Seleccionar fecha"
                      />
                      <USwitch 
                        v-model="fecha.enabled"
                        size="sm"
                      />
                      <span class="text-sm font-medium">{{ fecha.enabled ? 'Habilitado' : 'Deshabilitado' }}</span>
                    </div>
                    <div class="flex items-center gap-2">
                      <UButton 
                        v-if="fecha.date && fecha.enabled"
                        variant="soft" 
                        size="sm"
                        @click="guardarFechaEspecial(fecha)"
                        :loading="loading"
                        icon="i-heroicons-check"
                      >
                        Guardar
                      </UButton>
                      <UButton 
                        variant="ghost" 
                        color="red"
                        size="sm"
                        @click="eliminarFechaEspecial(index)"
                        icon="i-heroicons-trash"
                      >
                      </UButton>
                    </div>
                  </div>
                  
                  <div v-if="fecha.enabled" class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <!-- Configuración de mañana -->
                    <div class="space-y-3">
                      <div class="flex items-center gap-2">
                        <USwitch v-model="fecha.morning_enabled" size="sm" />
                        <label class="text-sm font-medium">Mañana</label>
                      </div>
                      <div v-if="fecha.morning_enabled" class="space-y-2 pl-6">
                        <div class="flex gap-2 items-center">
                          <UInput 
                            v-model="fecha.morning_start"
                            type="time"
                            placeholder="09:00"
                          />
                          <span class="text-gray-500">a</span>
                          <UInput 
                            v-model="fecha.morning_end"
                            type="time"
                            placeholder="13:00"
                          />
                        </div>
                        <UInput 
                          v-model.number="fecha.max_capacity_morning"
                          type="number"
                          :min="1"
                          :max="100"
                          placeholder="Capacidad máxima mañana"
                        />
                      </div>
                    </div>
                    
                    <!-- Configuración de tarde -->
                    <div class="space-y-3">
                      <div class="flex items-center gap-2">
                        <USwitch v-model="fecha.afternoon_enabled" size="sm" />
                        <label class="text-sm font-medium">Tarde</label>
                      </div>
                      <div v-if="fecha.afternoon_enabled" class="space-y-2 pl-6">
                        <div class="flex gap-2 items-center">
                          <UInput 
                            v-model="fecha.afternoon_start"
                            type="time"
                            placeholder="14:00"
                          />
                          <span class="text-gray-500">a</span>
                          <UInput 
                            v-model="fecha.afternoon_end"
                            type="time"
                            placeholder="18:00"
                          />
                        </div>
                        <UInput 
                          v-model.number="fecha.max_capacity_afternoon"
                          type="number"
                          :min="1"
                          :max="100"
                          placeholder="Capacidad máxima tarde"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </UCard>

          <!-- Información adicional -->
          <div class="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-4">
            <div class="flex">
              <UIcon name="i-heroicons-information-circle" class="w-5 h-5 text-blue-500 mt-0.5 mr-3" />
              <div class="text-sm text-blue-700 dark:text-blue-300">
                <p class="font-medium mb-1">Información importante:</p>
                <ul class="list-disc list-inside space-y-1">
                  <li>Los horarios por día definen la configuración general semanal</li>
                  <li>Las fechas especiales sobrescriben la configuración del día correspondiente</li>
                  <li>Los clientes podrán reservar slots según la anticipación mínima configurada</li>
                  <li>Solo se pueden configurar fechas futuras para evitar historial innecesario</li>
                </ul>
              </div>
            </div>
          </div>

          <!-- Botones de acción -->
          <div class="flex justify-end gap-3 pt-6">
            <UButton 
              variant="outline" 
              to="/admin/pedidos"
              :disabled="loading"
            >
              Cancelar
            </UButton>
            <UButton 
              type="submit"
              :loading="loading"
              :disabled="loading"
            >
              {{ deliverySettings ? 'Actualizar Configuración' : 'Crear Configuración' }}
            </UButton>
          </div>
        </UForm>

      </div>
    </div>
  </div>
</template>