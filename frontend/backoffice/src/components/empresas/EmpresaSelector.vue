<template>
  <div v-if="showSelector" class="mb-6">
    <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
      <Icon name="i-heroicons-building-office" class="w-4 h-4 inline mr-1" />
      Empresa para gestionar stock
    </label>
    
    <!-- Selector de empresa -->
    <div class="relative">
      <USelectMenu
        v-model="selectedEmpresa"
        :items="empresasOptions"
        :loading="loading"
        :disabled="loading || empresasOptions.length === 0"
        placeholder="Seleccionar empresa..."
        class="w-full max-w-md"
        size="lg"
      >
        <template #label>
          <div v-if="selectedEmpresa" class="flex items-center gap-2">
            <div 
              class="w-3 h-3 rounded-full"
              :class="getEmpresaTypeClass(selectedEmpresa?.tipo)"
            />
            <span class="font-medium">{{ selectedEmpresa?.label }}</span>
            <UBadge 
              :color="selectedEmpresa?.tipo === 'principal' ? 'blue' : 'gray'"
              size="xs"
              variant="soft"
            >
              {{ selectedEmpresa?.tipo === 'principal' ? 'Principal' : 'Cliente' }}
            </UBadge>
          </div>
          <span v-else class="text-gray-500">
            Seleccionar empresa...
          </span>
        </template>
        
        <template #option="{ option }">
          <div class="flex items-center justify-between w-full">
            <div class="flex items-center gap-2">
              <div 
                class="w-3 h-3 rounded-full"
                :class="getEmpresaTypeClass(option.tipo)"
              />
              <span class="font-medium">{{ option.label }}</span>
            </div>
            <UBadge 
              :color="option.tipo === 'principal' ? 'blue' : 'gray'"
              size="xs"
              variant="soft"
            >
              {{ option.tipo === 'principal' ? 'Principal' : 'Cliente' }}
            </UBadge>
          </div>
        </template>
      </USelectMenu>
      
      <!-- Información adicional -->
      <div v-if="selectedEmpresa" class="mt-2 text-xs text-gray-500 dark:text-gray-400">
        <div class="flex items-center gap-4">
          <span>
            <Icon name="i-heroicons-hashtag" class="w-3 h-3 inline mr-1" />
            Código: {{ selectedEmpresa.codigo }}
          </span>
          <span v-if="selectedEmpresa.email">
            <Icon name="i-heroicons-envelope" class="w-3 h-3 inline mr-1" />
            {{ selectedEmpresa.email }}
          </span>
        </div>
      </div>
    </div>
    
    <!-- Estado de carga o error -->
    <div v-if="error" class="mt-2">
      <UAlert
        color="red"
        variant="soft"
        :title="'Error al cargar empresas'"
        :description="error"
        :close-button="{ icon: 'i-heroicons-x-mark-20-solid', color: 'red', variant: 'link' }"
        @close="clearError"
      />
    </div>
    
    <!-- Helper text -->
    <p v-if="!error" class="mt-2 text-xs text-gray-500 dark:text-gray-400">
      El stock que visualices y edites será específico para la empresa seleccionada
    </p>
  </div>
</template>

<script setup lang="ts">
import type { Empresa } from '~/types/auth'

interface Props {
  modelValue?: number | null
  disabled?: boolean
  required?: boolean
}

interface Emits {
  (e: 'update:modelValue', value: number | null): void
  (e: 'empresa-changed', empresa: Empresa | null): void
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: null,
  disabled: false,
  required: false
})

const emit = defineEmits<Emits>()

// Composables
const { companies, loading, error, fetchCompanies } = useCompanies()
const { user } = useAuth()

// Estado local
const selectedEmpresa = ref<any>(null)

// Computadas
const showSelector = computed(() => {
  // Solo mostrar selector si el usuario es de empresa principal
  return user.value?.empresa?.tipo_empresa === 'principal'
})

const empresasOptions = computed(() => {
  if (!companies.value || companies.value.length === 0) return []
  
  // Incluir empresa principal y todas las empresas cliente
  const options = companies.value.map(empresa => ({
    label: empresa.nombre,
    value: empresa.id,
    tipo: empresa.tipo_empresa,
    empresa: empresa,
    // Para el template
    id: empresa.id,
    nombre: empresa.nombre,
    codigo: empresa.codigo,
    email: empresa.email
  }))
  
  // Ordenar: empresa principal primero, luego clientes alfabéticamente
  return options.sort((a, b) => {
    if (a.tipo === 'principal' && b.tipo !== 'principal') return -1
    if (a.tipo !== 'principal' && b.tipo === 'principal') return 1
    return a.label.localeCompare(b.label)
  })
})

const selectedEmpresaData = computed(() => {
  if (!selectedEmpresa.value) return null
  return selectedEmpresa.value.empresa || selectedEmpresa.value
})

// Métodos
const getEmpresaTypeClass = (tipo?: string) => {
  switch (tipo) {
    case 'principal':
      return 'bg-blue-500'
    case 'cliente':
      return 'bg-gray-400'
    default:
      return 'bg-gray-300'
  }
}

const clearError = () => {
  // Si el composable useCompanies tiene método para limpiar error
  if (error.value) {
    error.value = null
  }
}

// Watchers
watch(selectedEmpresa, (newValue) => {
  const empresaId = newValue?.value || newValue?.id || null
  const empresaData = newValue?.empresa || newValue || null
  
  emit('update:modelValue', empresaId)
  emit('empresa-changed', empresaData)
})

watch(() => props.modelValue, (newValue) => {
  if (newValue && companies.value.length > 0) {
    // Buscar la opción correspondiente al ID
    const option = empresasOptions.value.find(opt => opt.value === newValue)
    selectedEmpresa.value = option || null
  } else {
    selectedEmpresa.value = null
  }
})

// Inicialización
onMounted(async () => {
  if (showSelector.value) {
    // Cargar lista de empresas
    await fetchCompanies()
    
    // Si hay un modelValue inicial, seleccionarlo
    if (props.modelValue && companies.value.length > 0) {
      const option = empresasOptions.value.find(opt => opt.value === props.modelValue)
      selectedEmpresa.value = option || null
    } else if (!selectedEmpresa.value && user.value?.empresa) {
      // Si no hay empresa seleccionada, usar la empresa del usuario actual
      const option = empresasOptions.value.find(opt => opt.value === user.value.empresa.id)
      selectedEmpresa.value = option || null
    }
  } else {
    // Si el usuario es de empresa cliente, usar automáticamente su empresa
    if (user.value?.empresa?.tipo_empresa === 'cliente') {
      // Emitir directamente ya que no se muestra el selector
      emit('update:modelValue', user.value.empresa.id)
      emit('empresa-changed', user.value.empresa)
    }
  }
})

// Validación (si es requerido)
const isValid = computed(() => {
  if (!props.required) return true
  return selectedEmpresa.value !== null && selectedEmpresa.value !== undefined
})

// Exponer métodos/propiedades si es necesario
defineExpose({
  selectedEmpresa: readonly(selectedEmpresa),
  selectedEmpresaData: readonly(selectedEmpresaData),
  isValid: readonly(isValid),
  clearSelection: () => {
    selectedEmpresa.value = null
  }
})
</script>

<style scoped>
/* Estilos adicionales si son necesarios */
.empresa-selector {
  /* Estilos personalizados */
}
</style>