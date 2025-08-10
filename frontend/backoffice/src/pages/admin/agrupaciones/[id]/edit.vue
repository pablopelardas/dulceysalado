<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Dashboard</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <NuxtLink to="/admin/agrupaciones" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Agrupaciones</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Editar</span>
        </nav>
        
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
              Editar Agrupación
            </h1>
            <p class="mt-2 text-gray-600 dark:text-gray-400">
              Modifica los datos editables de la agrupación
            </p>
          </div>
          
          <!-- Info sobre campos read-only -->
          <UBadge color="orange" variant="soft" size="lg">
            <UIcon name="i-heroicons-lock-closed" class="mr-1" />
            Código desde GECOM
          </UBadge>
        </div>
      </div>

      <!-- Loading state -->
      <div v-if="initialLoading" class="space-y-6">
        <UCard>
          <template #header>
            <USkeleton class="h-6 w-40" />
          </template>
          <div class="space-y-4">
            <USkeleton class="h-10 w-full" />
            <USkeleton class="h-24 w-full" />
            <USkeleton class="h-10 w-32" />
          </div>
        </UCard>
      </div>

      <!-- Error state -->
      <div v-else-if="error" class="text-center py-8">
        <UIcon name="i-heroicons-exclamation-triangle" class="h-12 w-12 text-red-500 mx-auto mb-4" />
        <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100 mb-2">Error al cargar</h2>
        <p class="text-red-600 dark:text-red-400 mb-4">{{ error }}</p>
        <div class="flex gap-3 justify-center">
          <UButton color="red" variant="ghost" @click="loadAgrupacion">
            Reintentar
          </UButton>
          <UButton variant="ghost" @click="router.push('/admin/agrupaciones')">
            Volver al listado
          </UButton>
        </div>
      </div>

      <!-- Formulario -->
      <UCard v-else-if="agrupacion">
        <template #header>
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">
              Información de la Agrupación
            </h2>
            <div class="flex items-center gap-2">
              <UBadge :color="agrupacion.activa ? 'green' : 'red'" variant="soft">
                {{ agrupacion.activa ? 'Activa' : 'Inactiva' }}
              </UBadge>
            </div>
          </div>
        </template>

        <form @submit.prevent="handleSubmit" class="space-y-6">
          <!-- Código (read-only) -->
          <UFormField label="Código" help="Este código proviene de GECOM y no se puede modificar">
            <UInput
              :model-value="agrupacion.codigo.toString()"
              disabled
              icon="i-heroicons-lock-closed"
            />
          </UFormField>

          <!-- Nombre -->
          <UFormField 
            label="Nombre" 
            required
            :error="formErrors.nombre"
            help="Nombre descriptivo para la agrupación"
          >
            <UInput
              v-model="formData.nombre"
              placeholder="Ej: Electrodomésticos"
              :disabled="saving"
              maxlength="100"
            />
          </UFormField>

          <!-- Descripción -->
          <UFormField 
            label="Descripción" 
            :error="formErrors.descripcion"
            help="Descripción opcional más detallada"
          >
            <UTextarea
              v-model="formData.descripcion"
              placeholder="Descripción detallada de la agrupación..."
              :disabled="saving"
              :rows="4"
              maxlength="500"
            />
          </UFormField>

          <!-- Estado activo -->
          <UFormField label="Estado">
            <UCheckbox
              v-model="formData.activa"
              label="Agrupación activa"
              help="Solo las agrupaciones activas aparecen en el catálogo"
              :disabled="saving"
            />
          </UFormField>

          <!-- Información adicional -->
          <div class="bg-gray-50 dark:bg-gray-800 p-4 rounded-lg space-y-2">
            <h3 class="font-medium text-gray-900 dark:text-gray-100 text-sm">Información del sistema</h3>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm text-gray-600 dark:text-gray-400">
              <div>
                <span class="font-medium">Creado:</span>
                {{ formatDate(agrupacion.created_at) }}
              </div>
              <div>
                <span class="font-medium">Última actualización:</span>
                {{ formatDate(agrupacion.updated_at) }}
              </div>
            </div>
          </div>

          <!-- Botones de acción -->
          <div class="flex justify-between items-center pt-6 border-t border-gray-200 dark:border-gray-700">
            <UButton
              variant="ghost"
              color="gray"
              @click="router.push('/admin/agrupaciones')"
              :disabled="saving"
            >
              Cancelar
            </UButton>
            
            <div class="flex gap-3">
              <UButton
                variant="ghost"
                @click="resetForm"
                :disabled="saving || !hasChanges"
              >
                Deshacer cambios
              </UButton>
              
              <UButton
                type="submit"
                color="primary"
                :loading="saving"
                :disabled="!hasChanges || hasValidationErrors"
              >
                Guardar cambios
              </UButton>
            </div>
          </div>
        </form>
      </UCard>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { AgrupacionDto } from '~/types/agrupaciones'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default',
  requiresEmpresaPrincipal: true
})

// Composables
const route = useRoute()
const router = useRouter()
const { fetchAgrupacionById, updateAgrupacion, validateAgrupacion } = useAgrupacionesCrud()

// Estado reactivo
const agrupacion = ref<AgrupacionDto | null>(null)
const initialLoading = ref(true)
const saving = ref(false)
const error = ref<string | null>(null)

const formData = reactive({
  nombre: '',
  descripcion: '',
  activa: true
})

const formErrors = reactive({
  nombre: '',
  descripcion: ''
})

// Computed
const agrupacionId = computed(() => {
  const id = route.params.id
  return Array.isArray(id) ? parseInt(id[0]) : parseInt(id as string)
})

const hasChanges = computed(() => {
  if (!agrupacion.value) return false
  
  return (
    formData.nombre !== agrupacion.value.nombre ||
    formData.descripcion !== (agrupacion.value.descripcion || '') ||
    formData.activa !== agrupacion.value.activa
  )
})

const hasValidationErrors = computed(() => {
  return Object.values(formErrors).some(error => error !== '')
})

// Métodos
const loadAgrupacion = async () => {
  initialLoading.value = true
  error.value = null
  
  try {
    const data = await fetchAgrupacionById(agrupacionId.value)
    agrupacion.value = data
    
    // Inicializar formulario
    formData.nombre = data.nombre
    formData.descripcion = data.descripcion || ''
    formData.activa = data.activa
    
    // Actualizar título de la página
    useHead({
      title: `Editar ${data.nombre} - Agrupaciones`
    })
  } catch (err: any) {
    error.value = err.message || 'Error al cargar la agrupación'
  } finally {
    initialLoading.value = false
  }
}

const validateForm = () => {
  // Limpiar errores previos
  formErrors.nombre = ''
  formErrors.descripcion = ''
  
  // Validar usando el composable
  const errors = validateAgrupacion(formData)
  
  // Mapear errores a campos específicos
  errors.forEach(errorMsg => {
    if (errorMsg.includes('nombre')) {
      formErrors.nombre = errorMsg
    } else if (errorMsg.includes('descripción')) {
      formErrors.descripcion = errorMsg
    }
  })
  
  return errors.length === 0
}

const handleSubmit = async () => {
  if (!agrupacion.value || !validateForm()) return
  
  saving.value = true
  
  try {
    await updateAgrupacion(agrupacion.value.id, {
      nombre: formData.nombre.trim(),
      descripcion: formData.descripcion.trim() || null,
      activa: formData.activa
    })
    
    // Redirigir al listado
    router.push('/admin/agrupaciones')
  } catch (err: any) {
    // El error ya se maneja en el composable
  } finally {
    saving.value = false
  }
}

const resetForm = () => {
  if (!agrupacion.value) return
  
  formData.nombre = agrupacion.value.nombre
  formData.descripcion = agrupacion.value.descripcion || ''
  formData.activa = agrupacion.value.activa
  
  // Limpiar errores
  formErrors.nombre = ''
  formErrors.descripcion = ''
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleString('es-AR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Watchers
watch(() => formData.nombre, () => {
  if (formErrors.nombre) {
    validateForm()
  }
})

watch(() => formData.descripcion, () => {
  if (formErrors.descripcion) {
    validateForm()
  }
})

// Cargar datos al montar
onMounted(async () => {
  await loadAgrupacion()
})
</script>