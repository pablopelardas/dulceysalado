<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
              Sincronizaciones GECOM
            </h1>
            <p class="mt-2 text-gray-600 dark:text-gray-400">
              Monitoreo y estadísticas de sincronizaciones con el sistema GECOM
            </p>
          </div>
          <UButton
            variant="ghost"
            icon="i-heroicons-arrow-left"
            to="/"
          >
            Volver al Dashboard
          </UButton>
        </div>
      </div>

      <!-- Alerta de sincronización pendiente -->
      <UAlert
        v-if="dataLoaded && !hasSyncToday"
        color="red"
        variant="soft"
        class="mb-6"
      >
        <template #icon>
          <UIcon name="i-heroicons-exclamation-triangle" />
        </template>
        <template #title>
          Sincronización GECOM Pendiente
        </template>
        <template #description>
          No se han detectado sincronizaciones hoy. Recuerda ejecutar la sincronización diaria con GECOM para mantener actualizado el catálogo.
        </template>
      </UAlert>

      <!-- Estadísticas -->
      <div class="mb-8">
        <SyncStats :stats="stats" :loading="loadingStats" :error="error" />
      </div>

      <!-- Calendario de sincronizaciones -->
      <SyncLogsCalendar />
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import SyncLogsCalendar from '~/components/sync/SyncLogsCalendar.vue'
import SyncStats from '~/components/sync/SyncStats.vue'
import { useAuth } from '~/composables/useAuth'
import { useSyncLogs } from '~/composables/useSyncLogs'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Sincronizaciones GECOM',
  meta: [
    { name: 'description', content: 'Historial de sincronizaciones con GECOM' }
  ]
})

const { isEmpresaPrincipal } = useAuth()
const router = useRouter()
const { stats, logs, error, fetchStats, fetchLogs } = useSyncLogs()
const loadingStats = ref(false)
const dataLoaded = ref(false)

// Verificar si hay sincronizaciones hoy
const hasSyncToday = computed(() => {
  const today = new Date()
  const todayStr = today.toLocaleDateString('es-AR')
  const todayLogs = logs.value.filter(log => {
    const logDate = new Date(log.fecha_procesamiento).toLocaleDateString('es-AR')
    return logDate === todayStr
  })
  return todayLogs.length > 0
})

// Cargar estadísticas y logs
const loadData = async () => {
  loadingStats.value = true
  await Promise.all([
    fetchStats(30),
    fetchLogs(100)
  ])
  loadingStats.value = false
  dataLoaded.value = true
}

// Redirigir si no es empresa principal y cargar datos
onMounted(() => {
  if (!isEmpresaPrincipal) {
    router.push('/')
  } else {
    loadData()
  }
})
</script>