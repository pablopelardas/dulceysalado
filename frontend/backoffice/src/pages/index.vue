<script setup lang="ts">
import { computed, onMounted, ref, watchEffect } from 'vue'
import DashboardActionSkeleton from '~/components/ui/DashboardActionSkeleton.vue';
import { useSyncLogs } from '~/composables/useSyncLogs'

definePageMeta({
  middleware: 'auth'
})

useHead({
  title: 'Dashboard',
  meta: [
    { name: 'description', content: 'Panel de administración Dulce y Salado' }
  ]
})

const { user, empresa, isEmpresaPrincipal, userPermissions, isPermissionsLoaded } = useAuth()
const features = useFeatures()

// Función para obtener color del rol
const getRoleColor = (role: string | undefined) => {
  const colors: Record<string, 'primary' | 'error' | 'info' | 'neutral'> = {
    'admin': 'error',
    'editor': 'primary',
    'viewer': 'info',
    'default': 'neutral'
  }
  return colors[role || 'default'] || colors.default 
}

// Computed local para features específicos
const clienteAutenticacionEnabled = computed(() => {
  if (!empresa.value?.features) return false
  const feature = empresa.value.features.find(f => f.codigo === 'cliente_autenticacion')
  return feature?.habilitado || false
})

// Verificar sincronizaciones de hoy - solo para empresa principal
const syncLogs = ref([])
const syncLogsLoaded = ref(false)
const syncLogsAttempted = ref(false)

const hasSyncToday = computed(() => {
  if (empresa.value?.tipo_empresa !== 'principal') return true // No mostrar alerta si no es empresa principal
  
  const today = new Date()
  const todayStr = today.toLocaleDateString('es-AR')
  const todayLogs = syncLogs.value.filter(log => {
    const logDate = new Date(log.fecha_procesamiento).toLocaleDateString('es-AR')
    return logDate === todayStr
  })
  return todayLogs.length > 0
})

// Cargar logs solo si es empresa principal
const loadSyncLogs = async () => {
  if (syncLogsAttempted.value) return // Evitar múltiples ejecuciones
  
  syncLogsAttempted.value = true
  
  if (empresa.value?.tipo_empresa === 'principal') {
    const { logs, fetchLogs } = useSyncLogs()
    try {
      await fetchLogs(100)
      syncLogs.value = logs.value
      syncLogsLoaded.value = true
    } catch (error) {
      console.error('Error loading sync logs:', error)
      syncLogsLoaded.value = true // Marcar como cargado aunque falle
    }
  } else {
    // Si no es empresa principal, marcar como cargado sin hacer petición
    syncLogsLoaded.value = true
  }
}

// Esperar a que empresa esté disponible
watchEffect(() => {
  if (empresa.value && !syncLogsAttempted.value) {
    loadSyncLogs()
  }
})
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-7xl mx-auto">
      <!-- Alerta de sincronización pendiente (solo empresa principal) -->
      <ClientOnly>
        <template #default>
          <UAlert
            v-if="empresa?.tipo_empresa === 'principal' && syncLogsLoaded && !hasSyncToday"
            color="red"
            variant="soft"
            class="mb-6"
          >
            <template #icon>
              <UIcon name="i-heroicons-exclamation-triangle" />
            </template>
            <template #title>
              Sincronización SIGMA Pendiente
            </template>
            <template #description>
              <div class="space-y-2">
                <p>No se han detectado sincronizaciones hoy. Recuerda ejecutar la sincronización diaria con SIGMA para mantener actualizado el catálogo.</p>
                <UButton
                  to="/admin/sincronizaciones"
                  variant="solid"
                  color="white"
                  size="sm"
                >
                  <UIcon name="i-heroicons-calendar" class="mr-2" />
                  Ver Historial de Sincronizaciones
                </UButton>
              </div>
            </template>
          </UAlert>
        </template>
      </ClientOnly>

      <!-- Header del dashboard -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Dashboard
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          <ClientOnly>
            <template #default>
              Bienvenido {{ user?.nombre || 'usuario' }}, administra tu catálogo desde aquí
            </template>
            <template #fallback>
              Bienvenido usuario, administra tu catálogo desde aquí
            </template>
          </ClientOnly>
        </p>
      </div>

      <!-- Bienvenida con rol -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <div>
            <h2 class="text-xl font-medium text-gray-900 dark:text-gray-100">
              <ClientOnly>
                <template #default>
                  Bienvenido, {{ user?.nombre || 'usuario' }}
                </template>
                <template #fallback>
                  Bienvenido, usuario
                </template>
              </ClientOnly>
            </h2>
            <div class="flex items-center gap-2 mt-2">
              <ClientOnly>
                <template #default>
                  <UBadge 
                    :color="getRoleColor(user?.rol)" 
                    variant="subtle" 
                    size="sm"
                  >
                    {{ user?.rol?.toUpperCase() || 'USUARIO' }}
                  </UBadge>
                </template>
                <template #fallback>
                  <USkeleton class="h-6 w-16 rounded" />
                </template>
              </ClientOnly>
            </div>
          </div>
        </div>
      </div>

      <!-- Acciones rápidas -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <!-- Gestión de productos -->
        <UCard>
          <template #header>
            <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Gestión de Productos</h2>
          </template>

          <div class="space-y-4">
            <div class="flex items-center justify-between p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
              <div>
                <h3 class="font-medium text-blue-900 dark:text-blue-100">Productos Base</h3>
                <ClientOnly>
                  <template #default>
                    <p class="text-sm text-blue-700 dark:text-blue-300">
                      {{ userPermissions?.canManageProductosBase ? 'Gestiona el catálogo principal' : 'Ver catálogo principal' }}
                    </p>
                  </template>
                  <template #fallback>
                    <p class="text-sm text-blue-700 dark:text-blue-300">Ver catálogo principal</p>
                  </template>
                </ClientOnly>
              </div>
              <ClientOnly>
                <template #default>
                  <UButton color="blue" variant="solid" to="/admin/productos-base">
                    {{ userPermissions?.canManageProductosBase ? 'Gestionar' : 'Ver' }}
                  </UButton>
                </template>
                <template #fallback>
                  <UButton color="blue" variant="solid" to="/admin/productos-base">
                    Ver
                  </UButton>
                </template>
              </ClientOnly>
            </div>

          </div>
        </UCard>

        <!-- Gestión de Pedidos -->
        <UCard>
          <template #header>
            <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Gestión de Pedidos</h2>
          </template>

          <div class="space-y-4">
            <div class="flex items-center justify-between p-4 bg-orange-50 dark:bg-orange-900/20 rounded-lg">
              <div>
                <h3 class="font-medium text-orange-900 dark:text-orange-100">Pedidos del Sistema</h3>
                <p class="text-sm text-orange-700 dark:text-orange-300">Gestiona pedidos, acepta y rechaza solicitudes</p>
              </div>
              <UButton color="orange" variant="solid" to="/admin/pedidos">
                <UIcon name="i-heroicons-shopping-bag" class="mr-2" />
                Gestionar
              </UButton>
            </div>

            <div class="flex items-center justify-between p-4 bg-amber-50 dark:bg-amber-900/20 rounded-lg">
              <div>
                <h3 class="font-medium text-amber-900 dark:text-amber-100">Configuración de Entregas</h3>
                <p class="text-sm text-amber-700 dark:text-amber-300">Configura horarios y capacidad de entrega</p>
              </div>
              <UButton color="amber" variant="solid" to="/admin/delivery-config">
                <UIcon name="i-heroicons-truck" class="mr-2" />
                Configurar
              </UButton>
            </div>
          </div>
        </UCard>

        <!-- Gestión de categorías (solo mostrar si tiene permisos) -->
        <ClientOnly>
          <template #default>
            <UCard v-if="!isPermissionsLoaded">
              <template #header>
                <div class="h-6 bg-gray-200 dark:bg-gray-600 rounded w-1/2 animate-pulse"></div>
              </template>
              <div class="space-y-4">
                <DashboardActionSkeleton />
                <DashboardActionSkeleton />
              </div>
            </UCard>
            <UCard v-else-if="userPermissions?.canManageCategoriasBase || (features.empresaCategories && userPermissions?.canManageCategoriasEmpresa)">
              <template #header>
                <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Gestión de Categorías</h2>
              </template>

              <div class="space-y-4">
                <div v-if="userPermissions?.canManageCategoriasBase" class="flex items-center justify-between p-4 bg-purple-50 dark:bg-purple-900/20 rounded-lg">
                  <div>
                    <h3 class="font-medium text-purple-900 dark:text-purple-100">Categorías Base</h3>
                    <p class="text-sm text-purple-700 dark:text-purple-300">Gestiona las categorías principales</p>
                  </div>
                  <UButton color="purple" variant="solid" to="/admin/categorias-base">
                    Gestionar
                  </UButton>
                </div>

              </div>
            </UCard>
          </template>
          <template #fallback>
            <UCard>
              <template #header>
                <div class="h-6 bg-gray-200 dark:bg-gray-600 rounded w-1/2 animate-pulse"></div>
              </template>
              <div class="space-y-4">
                <DashboardActionSkeleton />
                <DashboardActionSkeleton />
              </div>
            </UCard>
          </template>
        </ClientOnly>


        <!-- Gestión de listas de precios (solo empresa principal con permisos) -->
        <ClientOnly>
          <template #default>
            <UCard v-if="!isPermissionsLoaded">
              <template #header>
                <div class="h-6 bg-gray-200 dark:bg-gray-600 rounded w-1/2 animate-pulse"></div>
              </template>
              <DashboardActionSkeleton />
            </UCard>
            <UCard v-else-if="isEmpresaPrincipal && userPermissions?.canManageProductosBase">
              <template #header>
                <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Listas de Precios</h2>
              </template>

              <div class="space-y-4">
                <div class="flex items-center justify-between p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
                  <div>
                    <h3 class="font-medium text-blue-900 dark:text-blue-100">Gestión de Listas</h3>
                    <p class="text-sm text-blue-700 dark:text-blue-300">Administra las listas de precios del sistema</p>
                  </div>
                  <UButton color="blue" variant="solid" to="/admin/listas-precios">
                    Gestionar
                  </UButton>
                </div>
              </div>
            </UCard>
          </template>
          <template #fallback>
            <UCard>
              <template #header>
                <div class="h-6 bg-gray-200 dark:bg-gray-600 rounded w-1/2 animate-pulse"></div>
              </template>
              <DashboardActionSkeleton />
            </UCard>
          </template>
        </ClientOnly>

        <!-- Gestión de usuarios (solo si tiene permisos) -->
        <ClientOnly>
          <template #default>
            <UCard v-if="!isPermissionsLoaded">
              <template #header>
                <div class="h-6 bg-gray-200 dark:bg-gray-600 rounded w-1/2 animate-pulse"></div>
              </template>
              <DashboardActionSkeleton />
            </UCard>
            <UCard v-else-if="userPermissions?.canManageUsuarios">
              <template #header>
                <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Gestión de Usuarios</h2>
              </template>

              <div class="space-y-4">
                <div class="flex items-center justify-between p-4 bg-purple-50 dark:bg-purple-900/20 rounded-lg">
                  <div>
                    <h3 class="font-medium text-purple-900 dark:text-purple-100">Usuarios</h3>
                    <p class="text-sm text-purple-700 dark:text-purple-300">Administra usuarios de tu empresa</p>
                  </div>
                  <UButton color="purple" variant="solid" to="/users">
                    Gestionar
                  </UButton>
                </div>
              </div>
            </UCard>
          </template>
          <template #fallback>
            <UCard>
              <template #header>
                <div class="h-6 bg-gray-200 dark:bg-gray-600 rounded w-1/2 animate-pulse"></div>
              </template>
              <DashboardActionSkeleton />
            </UCard>
          </template>
        </ClientOnly>
      </div>

      <!-- Mi Perfil -->
      <div class="mt-8">
        <UCard>
          <template #header>
            <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Mi Cuenta</h2>
          </template>

          <div class="space-y-4">
            <div class="flex items-center justify-between p-4 bg-green-50 dark:bg-green-900/20 rounded-lg">
              <div>
                <h3 class="font-medium text-green-900 dark:text-green-100">Mi Perfil</h3>
                <p class="text-sm text-green-700 dark:text-green-300">Gestiona tu información personal y contraseña</p>
              </div>
              <UButton color="green" variant="solid" to="/profile">
                <UIcon name="i-heroicons-user" class="mr-2" />
                Ver Perfil
              </UButton>
            </div>
          </div>
        </UCard>
      </div>

      <!-- Administración (solo empresa principal) -->
      <ClientOnly>
        <template #default>
          <div v-if="!isPermissionsLoaded" class="mt-8">
            <UCard>
              <template #header>
                <div class="h-6 bg-gray-200 dark:bg-gray-600 rounded w-1/2 animate-pulse"></div>
              </template>
              <DashboardActionSkeleton />
            </UCard>
          </div>
          <div v-else-if="isEmpresaPrincipal" class="mt-8">
            <UCard>
              <template #header>
                <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Administración</h2>
              </template>
              <div class="space-y-4">

                <div v-if="clienteAutenticacionEnabled" class="flex items-center justify-between p-4 bg-emerald-50 dark:bg-emerald-900/20 rounded-lg">
                  <div>
                    <h3 class="font-medium text-emerald-900 dark:text-emerald-100">Clientes</h3>
                    <p class="text-sm text-emerald-700 dark:text-emerald-300">Gestiona clientes y sus credenciales de acceso</p>
                  </div>
                  <UButton color="emerald" variant="solid" to="/clientes">
                    <UIcon name="i-heroicons-user-group" class="mr-2" />
                    Gestionar
                  </UButton>
                </div>

                <div class="flex items-center justify-between p-4 bg-cyan-50 dark:bg-cyan-900/20 rounded-lg">
                  <div>
                    <h3 class="font-medium text-cyan-900 dark:text-cyan-100">Sincronizaciones SIGMA</h3>
                    <p class="text-sm text-cyan-700 dark:text-cyan-300">Monitorea el historial de sincronizaciones con SIGMA</p>
                  </div>
                  <UButton color="cyan" variant="solid" to="/admin/sincronizaciones">
                    <UIcon name="i-heroicons-calendar" class="mr-2" />
                    Ver Historial
                  </UButton>
                </div>
              </div>
            </UCard>
          </div>
        </template>
        <template #fallback>
          <div class="mt-8">
            <UCard>
              <template #header>
                <div class="h-6 bg-gray-200 dark:bg-gray-600 rounded w-1/2 animate-pulse"></div>
              </template>
              <DashboardActionSkeleton />
            </UCard>
          </div>
        </template>
      </ClientOnly>
    </div>
  </div>
</template>
