<template>
  <div class="sync-stats">
    <div v-if="loading" class="flex justify-center items-center h-64">
      <UIcon name="i-heroicons-arrow-path" class="animate-spin text-3xl text-gray-400" />
    </div>

    <div v-else-if="error" class="text-center text-red-500 py-8">
      {{ error }}
    </div>

    <div v-else-if="stats?.estadisticas" class="space-y-6">
      <!-- Resumen General -->
      <div class="stats-overview">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Resumen de los últimos {{ stats.periodo_dias }} días
        </h3>
        
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <UCard>
            <div class="text-center">
              <UIcon name="i-heroicons-arrow-path" class="text-3xl text-blue-500 mb-2" />
              <p class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ stats.estadisticas.total_syncs }}
              </p>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                Sincronizaciones
              </p>
            </div>
          </UCard>

          <UCard>
            <div class="text-center">
              <UIcon name="i-heroicons-check-circle" class="text-3xl text-green-500 mb-2" />
              <p class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ stats.estadisticas.tasa_exito_promedio }}%
              </p>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                Tasa de Éxito
              </p>
            </div>
          </UCard>

          <UCard>
            <div class="text-center">
              <UIcon name="i-heroicons-cube" class="text-3xl text-purple-500 mb-2" />
              <p class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ formatNumber(stats.estadisticas.productos_totales) }}
              </p>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                Productos Procesados
              </p>
            </div>
          </UCard>

          <UCard>
            <div class="text-center">
              <UIcon name="i-heroicons-clock" class="text-3xl text-orange-500 mb-2" />
              <p class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ formatTime(stats.estadisticas.tiempo_promedio_ms) }}
              </p>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                Tiempo Promedio
              </p>
            </div>
          </UCard>
        </div>
      </div>

      <!-- Métricas Detalladas -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Performance -->
        <UCard>
          <template #header>
            <h4 class="font-semibold text-gray-900 dark:text-white">
              Rendimiento
            </h4>
          </template>
          
          <div class="space-y-3">
            <div class="flex justify-between items-center">
              <span class="text-sm text-gray-600 dark:text-gray-400">
                Productos por segundo
              </span>
              <span class="font-medium text-gray-900 dark:text-white">
                {{ stats.estadisticas.productos_por_segundo_promedio.toFixed(2) }}
              </span>
            </div>
            
            <div class="flex justify-between items-center">
              <span class="text-sm text-gray-600 dark:text-gray-400">
                Tiempo mínimo
              </span>
              <span class="font-medium text-gray-900 dark:text-white">
                {{ formatTime(stats.estadisticas.performance.tiempo_minimo_ms) }}
              </span>
            </div>
            
            <div class="flex justify-between items-center">
              <span class="text-sm text-gray-600 dark:text-gray-400">
                Tiempo máximo
              </span>
              <span class="font-medium text-gray-900 dark:text-white">
                {{ formatTime(stats.estadisticas.performance.tiempo_maximo_ms) }}
              </span>
            </div>
            
            <div class="flex justify-between items-center">
              <span class="text-sm text-gray-600 dark:text-gray-400">
                Sesiones con advertencias
              </span>
              <span class="font-medium" :class="{
                'text-orange-600': stats.estadisticas.performance.sesiones_con_advertencias > 0,
                'text-green-600': stats.estadisticas.performance.sesiones_con_advertencias === 0
              }">
                {{ stats.estadisticas.performance.sesiones_con_advertencias }}
              </span>
            </div>
          </div>
        </UCard>

        <!-- Distribución de Productos -->
        <UCard>
          <template #header>
            <h4 class="font-semibold text-gray-900 dark:text-white">
              Distribución de Productos
            </h4>
          </template>
          
          <div class="space-y-3">
            <div class="flex justify-between items-center">
              <span class="text-sm text-gray-600 dark:text-gray-400">
                Productos actualizados
              </span>
              <span class="font-medium text-gray-900 dark:text-white">
                {{ formatNumber(stats.estadisticas.productos_actualizados) }}
              </span>
            </div>
            
            <div class="flex justify-between items-center">
              <span class="text-sm text-gray-600 dark:text-gray-400">
                Productos nuevos
              </span>
              <span class="font-medium text-gray-900 dark:text-white">
                {{ formatNumber(stats.estadisticas.productos_nuevos) }}
              </span>
            </div>
            
            <div class="flex justify-between items-center">
              <span class="text-sm text-gray-600 dark:text-gray-400">
                Sincronizaciones exitosas
              </span>
              <span class="font-medium text-green-600">
                {{ stats.estadisticas.syncs_exitosos }}
              </span>
            </div>
            
            <div class="flex justify-between items-center">
              <span class="text-sm text-gray-600 dark:text-gray-400">
                Sincronizaciones con errores
              </span>
              <span class="font-medium" :class="{
                'text-red-600': stats.estadisticas.syncs_con_errores > 0,
                'text-gray-900 dark:text-white': stats.estadisticas.syncs_con_errores === 0
              }">
                {{ stats.estadisticas.syncs_con_errores }}
              </span>
            </div>
          </div>
        </UCard>
      </div>

      <!-- Última Sincronización -->
      <div class="text-center text-sm text-gray-600 dark:text-gray-400">
        Última sincronización: {{ formatFullDate(stats.estadisticas.ultima_sincronizacion) }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  stats: any
  loading: boolean
  error: string | null
}

const props = defineProps<Props>()


// Funciones de formato
const formatNumber = (num: number) => {
  if (num >= 1000000) {
    return (num / 1000000).toFixed(1) + 'M'
  } else if (num >= 1000) {
    return (num / 1000).toFixed(1) + 'k'
  }
  return num.toString()
}

const formatTime = (ms: number) => {
  if (ms < 1000) {
    return ms + 'ms'
  } else if (ms < 60000) {
    return (ms / 1000).toFixed(1) + 's'
  } else {
    return (ms / 60000).toFixed(1) + 'min'
  }
}

const formatFullDate = (dateStr: string) => {
  const date = new Date(dateStr)
  return date.toLocaleString('es-AR', { 
    day: 'numeric',
    month: 'long',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>

