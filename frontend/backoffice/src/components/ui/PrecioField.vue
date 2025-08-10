<template>
  <div class="relative">
    <UInput
      v-model="precioInput"
      :placeholder="placeholder"
      :disabled="disabled || saving"
      :variant="hasError ? 'error' : 'outline'"
      @blur="handleBlur"
      @keydown.enter.prevent="handleSave"
      @keydown.escape="handleCancel"
    >
      <template #leading>
        <span class="text-gray-500">$</span>
      </template>

      <template #trailing>
        <!-- Estado de guardado -->
        <UIcon 
          v-if="saving" 
          name="i-heroicons-arrow-path" 
          class="h-4 w-4 text-blue-500 animate-spin" 
        />
        
        <!-- Indicador de guardado exitoso -->
        <UIcon 
          v-else-if="showSavedIndicator" 
          name="i-heroicons-check-circle" 
          class="h-4 w-4 text-green-500" 
          title="Guardado"
        />
        
        <!-- Indicador de error -->
        <UIcon 
          v-else-if="hasError" 
          name="i-heroicons-exclamation-triangle" 
          class="h-4 w-4 text-red-500" 
          title="Error en el precio"
        />
      </template>
    </UInput>

    <!-- Mensaje de error -->
    <div v-if="errorMessage" class="mt-1 text-xs text-red-500">
      {{ errorMessage }}
    </div>

    <!-- Información adicional -->
    <div v-if="lista && showListaInfo" class="mt-1 text-xs text-gray-500">
      Lista: {{ lista.nombre }}
    </div>
    
    <!-- Instrucciones (solo si tiene cambios sin guardar) -->
    <div v-if="hasUnsavedChanges && !saving && !readonly" class="mt-1 text-xs text-gray-500">
      Presiona Enter para guardar o Esc para cancelar
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ListaPrecioInfo } from '~/types/productos'

interface Props {
  modelValue?: number | null
  lista?: ListaPrecioInfo
  placeholder?: string
  disabled?: boolean
  readonly?: boolean
  showListaInfo?: boolean
  autoSave?: boolean
  autoSaveDelay?: number
}

interface Emits {
  'update:modelValue': [value: number | null]
  'save': [value: number]
  'error': [error: string]
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: '0.00',
  autoSave: true,
  autoSaveDelay: 1000,
  showListaInfo: false
})

const emit = defineEmits<Emits>()

// Composables
const { formatPrecio, parsePrecio, validatePrecio } = useProductoPrecios()

// Estado reactivo
const precioInput = ref('')
const originalValue = ref<number | null>(null)
const saving = ref(false)
const hasError = ref(false)
const errorMessage = ref('')
const showSavedIndicator = ref(false)

// Auto-save timer
let autoSaveTimer: NodeJS.Timeout | null = null

// Computed
const hasUnsavedChanges = computed(() => {
  if (!precioInput.value.trim()) return false
  const currentValue = parsePrecio(precioInput.value)
  return currentValue !== originalValue.value
})

// Formatear valor inicial
const formatInitialValue = (value: number | null): string => {
  if (!value && value !== 0) return ''
  return value.toLocaleString('es-AR', { 
    minimumFractionDigits: 2,
    maximumFractionDigits: 2 
  })
}

// Validar precio actual
const validateCurrentPrice = (): boolean => {
  if (!precioInput.value.trim()) {
    hasError.value = false
    errorMessage.value = ''
    return true
  }

  const precio = parsePrecio(precioInput.value)
  
  if (!validatePrecio(precio)) {
    hasError.value = true
    errorMessage.value = 'Precio inválido'
    return false
  }
  
  if (precio < 0) {
    hasError.value = true
    errorMessage.value = 'El precio no puede ser negativo'
    return false
  }
  
  hasError.value = false
  errorMessage.value = ''
  return true
}

// Manejar guardado
const handleSave = async () => {
  if (!validateCurrentPrice()) return
  if (!hasUnsavedChanges.value) return
  if (props.readonly) return

  const precio = parsePrecio(precioInput.value)
  
  saving.value = true
  try {
    emit('save', precio)
    originalValue.value = precio
    
    // Mostrar indicador de guardado exitoso
    showSavedIndicator.value = true
    setTimeout(() => {
      showSavedIndicator.value = false
    }, 2000)
    
    // Formatear el valor guardado
    precioInput.value = formatInitialValue(precio)
    
  } catch (error: any) {
    hasError.value = true
    errorMessage.value = error.message || 'Error al guardar'
    emit('error', errorMessage.value)
  } finally {
    saving.value = false
  }
}

// Manejar cancelación
const handleCancel = () => {
  precioInput.value = formatInitialValue(originalValue.value)
  hasError.value = false
  errorMessage.value = ''
  
  // Limpiar timer de auto-save
  if (autoSaveTimer) {
    clearTimeout(autoSaveTimer)
    autoSaveTimer = null
  }
}

// Manejar blur (pérdida de foco)
const handleBlur = () => {
  validateCurrentPrice()
  
  if (props.autoSave && hasUnsavedChanges.value && !hasError.value) {
    handleSave()
  }
}

// Auto-save con delay
const scheduleAutoSave = () => {
  if (!props.autoSave) return
  if (props.readonly) return
  
  // Limpiar timer anterior
  if (autoSaveTimer) {
    clearTimeout(autoSaveTimer)
  }
  
  // Programar nuevo auto-save
  autoSaveTimer = setTimeout(() => {
    if (hasUnsavedChanges.value && !hasError.value) {
      handleSave()
    }
  }, props.autoSaveDelay)
}

// Watchers
watch(() => props.modelValue, (newValue) => {
  originalValue.value = newValue
  precioInput.value = formatInitialValue(newValue)
  hasError.value = false
  errorMessage.value = ''
}, { immediate: true })

watch(precioInput, () => {
  validateCurrentPrice()
  
  if (props.autoSave && !hasError.value) {
    scheduleAutoSave()
  }
})

// Cleanup
onUnmounted(() => {
  if (autoSaveTimer) {
    clearTimeout(autoSaveTimer)
  }
})
</script>