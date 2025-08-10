<template>
  <div class="sync-logs-calendar">
    <div class="calendar-header">
      <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
        Historial de Sincronizaciones
      </h3>
      <div class="calendar-controls">
        <UButton
          variant="ghost"
          size="sm"
          icon="i-heroicons-chevron-left"
          @click="previousMonth"
        />
        <span class="text-sm font-medium text-gray-700 dark:text-gray-300 mx-4">
          {{ monthYearDisplay }}
        </span>
        <UButton
          variant="ghost"
          size="sm"
          icon="i-heroicons-chevron-right"
          @click="nextMonth"
        />
      </div>
    </div>

    <div v-if="loading" class="flex justify-center items-center h-64">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin text-3xl text-gray-400" />
    </div>

    <div v-else-if="error" class="text-center text-red-500 py-8">
      {{ error }}
    </div>

    <div v-else class="calendar-grid">
      <!-- Días de la semana -->
      <div v-for="day in weekDays" :key="day" class="calendar-weekday">
        {{ day }}
      </div>

      <!-- Días del mes -->
      <div
        v-for="(day, index) in calendarDays"
        :key="index"
        class="calendar-day"
        :class="getDayClasses(day)"
        @click="day && selectDay(day)"
      >
        <template v-if="day">
          <div class="day-content">
            <span class="day-number" :class="{
              'has-syncs': getDayLogs(day).length > 0
            }">{{ day.getDate() }}</span>
            <div v-if="getDayLogs(day).length > 0" class="day-info">
              <div class="sync-dots">
                <div 
                  class="sync-dot"
                  :class="{
                    'bg-green-500': getDayStatus(day.toLocaleDateString('es-AR')) === 'success',
                    'bg-red-500': getDayStatus(day.toLocaleDateString('es-AR')) === 'error'
                  }"
                />
                <span class="sync-count text-xs" :class="{
                  'text-green-600 dark:text-green-400': getDayStatus(day.toLocaleDateString('es-AR')) === 'success',
                  'text-red-600 dark:text-red-400': getDayStatus(day.toLocaleDateString('es-AR')) === 'error'
                }">
                  {{ getDayLogs(day).length }}
                </span>
              </div>
            </div>
          </div>
        </template>
      </div>
    </div>

    <!-- Modal de detalles del día -->
    <UModal v-model:open="showDayDetails">
        <template #header>
          <h3 class="text-lg font-semibold">
            Sincronizaciones - {{ formatDate(selectedDay) }}
          </h3>
        </template>
        <template #body>
          <div class="space-y-4">
            <!-- Resumen del día -->
            <div class="summary-grid">
              <div class="summary-item">
                <span class="summary-label">Total de sincronizaciones</span>
                <span class="summary-value">{{ daySummary.totalSyncs }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Sincronizaciones exitosas</span>
                <span class="summary-value text-green-600">{{ daySummary.successfulSyncs }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Productos actualizados</span>
                <span class="summary-value">{{ daySummary.totalProductsUpdated }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Productos nuevos</span>
                <span class="summary-value">{{ daySummary.totalProductsNew }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Errores totales</span>
                <span class="summary-value" :class="{ 'text-red-600': daySummary.totalErrors > 0 }">
                  {{ daySummary.totalErrors }}
                </span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Tiempo promedio</span>
                <span class="summary-value">{{ daySummary.avgProcessingTime }}ms</span>
              </div>
            </div>
  
            <!-- Lista de sincronizaciones -->
            <div class="logs-list">
              <h4 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Detalle de sincronizaciones
              </h4>
              <div class="space-y-2">
                <div
                  v-for="log in daySummary.logs"
                  :key="log.id"
                  class="log-item"
                  :class="{
                    'border-green-200 dark:border-green-800': log.estado === 'Exitoso' && log.errores === 0,
                    'border-red-200 dark:border-red-800': log.estado !== 'Exitoso' || log.errores > 0
                  }"
                >
                  <div class="flex justify-between items-start">
                    <div>
                      <p class="text-sm font-medium text-gray-900 dark:text-white">
                        {{ formatTime(log.fecha_procesamiento) }}
                      </p>
                      <p class="text-xs text-gray-500 dark:text-gray-400">
                        {{ log.archivo_nombre }}
                      </p>
                    </div>
                    <UBadge
                      :color="log.estado === 'Exitoso' && log.errores === 0 ? 'green' : 'red'"
                      variant="subtle"
                    >
                      {{ log.estado }}
                    </UBadge>
                  </div>
                  <div class="mt-2 text-xs text-gray-600 dark:text-gray-400">
                    <span>Actualizados: {{ log.productos_actualizados }}</span>
                    <span class="mx-2">•</span>
                    <span>Nuevos: {{ log.productos_nuevos }}</span>
                    <span class="mx-2">•</span>
                    <span>Tiempo: {{ log.tiempo_procesamiento_ms }}ms</span>
                    <span v-if="log.errores > 0" class="text-red-600 dark:text-red-400">
                      <span class="mx-2">•</span>
                      Errores: {{ log.errores }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </template>
    </UModal>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { useSyncLogs } from '~/composables/useSyncLogs'

const { logs, loading, error, fetchLogs, getDayStatus, getDaySummary } = useSyncLogs()

// Estado del calendario
const currentMonth = ref(new Date())
const selectedDay = ref<Date | null>(null)
const showDayDetails = ref(false)

// Días de la semana
const weekDays = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb']

// Computed properties
const monthYearDisplay = computed(() => {
  const month = currentMonth.value.toLocaleDateString('es-AR', { month: 'long' })
  const year = currentMonth.value.getFullYear()
  return `${month.charAt(0).toUpperCase() + month.slice(1)} ${year}`
})

const calendarDays = computed(() => {
  const year = currentMonth.value.getFullYear()
  const month = currentMonth.value.getMonth()
  
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  const startPadding = firstDay.getDay()
  
  const days: (Date | null)[] = []
  
  // Agregar días vacíos al inicio
  for (let i = 0; i < startPadding; i++) {
    days.push(null)
  }
  
  // Agregar días del mes
  for (let i = 1; i <= lastDay.getDate(); i++) {
    days.push(new Date(year, month, i))
  }
  
  return days
})

const daySummary = computed(() => {
  if (!selectedDay.value) return { totalSyncs: 0, successfulSyncs: 0, totalProductsUpdated: 0, totalProductsNew: 0, totalErrors: 0, avgProcessingTime: 0, logs: [] }
  return getDaySummary(selectedDay.value.toLocaleDateString('es-AR'))
})

// Métodos
const previousMonth = () => {
  currentMonth.value = new Date(currentMonth.value.getFullYear(), currentMonth.value.getMonth() - 1, 1)
}

const nextMonth = () => {
  currentMonth.value = new Date(currentMonth.value.getFullYear(), currentMonth.value.getMonth() + 1, 1)
}

const getDayLogs = (date: Date) => {
  const dateStr = date.toLocaleDateString('es-AR')
  return logs.value.filter(log => {
    const logDate = new Date(log.fecha_procesamiento).toLocaleDateString('es-AR')
    return logDate === dateStr
  })
}

const getDayClasses = (day: Date | null) => {
  if (!day) return 'calendar-day-empty'
  
  const today = new Date()
  const isToday = day.toDateString() === today.toDateString()
  const isCurrentMonth = day.getMonth() === currentMonth.value.getMonth()
  const hasLogs = getDayLogs(day).length > 0
  
  return {
    'calendar-day-today': isToday,
    'calendar-day-other-month': !isCurrentMonth,
    'calendar-day-has-logs': hasLogs,
    'cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800': hasLogs
  }
}

const selectDay = (day: Date) => {
  if (getDayLogs(day).length === 0) return
  
  selectedDay.value = day
  showDayDetails.value = true
}

const formatDate = (date: Date) => {
  return date.toLocaleDateString('es-AR', { 
    weekday: 'long', 
    year: 'numeric', 
    month: 'long', 
    day: 'numeric' 
  })
}

const formatTime = (dateStr: string) => {
  return new Date(dateStr).toLocaleTimeString('es-AR', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
}

// Cargar logs al montar
onMounted(() => {
  fetchLogs(1000) // Cargar más logs para tener un historial más completo
})
</script>

<style scoped>
@reference 'tailwindcss';
.sync-logs-calendar {
  @apply bg-white dark:bg-gray-900 rounded-lg shadow-sm border border-gray-200 dark:border-gray-800 p-6;
}

.calendar-header {
  @apply flex justify-between items-center mb-6;
}

.calendar-controls {
  @apply flex items-center;
}

.calendar-grid {
  @apply grid grid-cols-7 gap-1;
}

.calendar-weekday {
  @apply text-xs font-medium text-gray-500 dark:text-gray-400 text-center py-2;
}

.calendar-day {
  @apply relative min-h-[60px] border border-gray-200 dark:border-gray-800 p-1 transition-colors;
}

.calendar-day-empty {
  @apply border-transparent;
}

.calendar-day-today {
  @apply bg-blue-50 dark:bg-blue-900/20;
}

.calendar-day-other-month {
  @apply opacity-50;
}

.day-content {
  @apply flex flex-col h-full;
}

.day-number {
  @apply text-sm font-medium text-gray-700 dark:text-gray-300;
}

.day-number.has-syncs {
  @apply font-semibold;
}

.day-info {
  @apply mt-1;
}

.sync-dots {
  @apply flex items-center justify-center gap-1;
}

.sync-dot {
  @apply w-2 h-2 rounded-full;
}

.sync-count {
  @apply font-medium;
}

.summary-grid {
  @apply grid grid-cols-2 md:grid-cols-3 gap-4 p-4 bg-gray-50 dark:bg-gray-800 rounded-lg;
}

.summary-item {
  @apply flex flex-col;
}

.summary-label {
  @apply text-xs text-gray-500 dark:text-gray-400;
}

.summary-value {
  @apply text-lg font-semibold text-gray-900 dark:text-white;
}

.logs-list {
  @apply max-h-96 overflow-y-auto;
}

.log-item {
  @apply p-3 border rounded-lg bg-gray-50 dark:bg-gray-800;
}

/* Responsive para móvil */
@media (max-width: 640px) {
  .calendar-day {
    @apply min-h-[50px] p-0.5;
  }
  
  .day-number {
    @apply text-xs;
  }
  
  .sync-dot {
    @apply w-1.5 h-1.5;
  }
  
  .sync-count {
    @apply text-[10px];
  }
}
</style>