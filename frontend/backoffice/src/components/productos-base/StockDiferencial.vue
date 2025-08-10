<template>
  <div class="stock-diferencial">
    <!-- Encabezado con información -->
    <div class="flex items-center justify-between mb-4">
      <div class="flex items-center gap-2">
        <Icon name="i-heroicons-cube" class="w-5 h-5 text-blue-600" />
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Stock por Empresa
        </h3>
        <UBadge
          v-if="empresaNombre"
          color="blue"
          variant="soft"
          size="sm"
        >
          {{ empresaNombre }}
        </UBadge>
      </div>
      
      <!-- Indicador de estado -->
      <div class="flex items-center gap-2">
        <UBadge
          :color="stockData?.esStockDiferencial ? 'green' : 'gray'"
          :variant="stockData?.esStockDiferencial ? 'solid' : 'soft'"
          size="sm"
        >
          {{ stockData?.esStockDiferencial ? 'Stock Diferencial' : 'Stock Base' }}
        </UBadge>
      </div>
    </div>
    
    <!-- Formulario de edición de stock -->
    <div class="space-y-4">
      <!-- Campo de stock actual -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Stock editable -->
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Stock Empresa
            <span v-if="required" class="text-red-500 ml-1">*</span>
          </label>
          <div class="relative">
            <UInput
              v-model.number="stockEmpresa"
              type="number"
              min="0"
              step="1"
              :disabled="loading || disabled"
              :error="stockError !== null"
              placeholder="Ingrese cantidad..."
              size="lg"
              class="w-full"
            >
              <template #trailing>
                <Icon name="i-heroicons-cube" class="w-4 h-4 text-gray-400" />
              </template>
            </UInput>
            
            <!-- Error de validación -->
            <p v-if="stockError" class="mt-1 text-sm text-red-600">
              {{ stockError }}
            </p>
          </div>
        </div>
        
        <!-- Stock base (solo referencia) -->
        <div v-if="stockData?.stockBase !== undefined">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Stock Base (Referencia)
          </label>
          <UInput
            :model-value="stockData.stockBase"
            type="number"
            disabled
            size="lg"
            class="w-full"
          >
            <template #trailing>
              <Icon name="i-heroicons-archive-box" class="w-4 h-4 text-gray-400" />
            </template>
          </UInput>
        </div>
      </div>
      
      <!-- Botones de acciones -->
      <div class="flex items-center gap-3">
        <UButton
          @click="handleSave"
          :loading="loading"
          :disabled="!hasChanges || !isValidStock || disabled"
          color="blue"
          size="md"
        >
          <template #leading>
            <Icon name="i-heroicons-check" class="w-4 h-4" />
          </template>
          Guardar Stock
        </UButton>
        
        <UButton
          @click="handleReset"
          :loading="loading"
          :disabled="!stockData?.esStockDiferencial || disabled"
          color="gray"
          variant="soft"
          size="md"
        >
          <template #leading>
            <Icon name="i-heroicons-arrow-path" class="w-4 h-4" />
          </template>
          Resetear a Base
        </UButton>
        
        <UButton
          v-if="hasChanges"
          @click="handleCancel"
          :disabled="loading"
          color="red" 
          variant="ghost"
          size="md"
        >
          <template #leading>
            <Icon name="i-heroicons-x-mark" class="w-4 h-4" />
          </template>
          Cancelar
        </UButton>
      </div>
      
      <!-- Información adicional -->
      <div v-if="stockData" class="p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
        <div class="flex items-start gap-3">
          <Icon name="i-heroicons-information-circle" class="w-5 h-5 text-blue-500 mt-0.5" />
          <div class="flex-1 text-sm text-gray-600 dark:text-gray-400">
            <p class="mb-2">
              <strong>Stock actual:</strong> {{ stockEmpresa ?? 0 }} unidades
            </p>
            <p v-if="stockData.stockBase !== undefined" class="mb-2">
              <strong>Stock base del producto:</strong> {{ stockData.stockBase }} unidades
            </p>
            <p class="text-xs text-gray-500">
              {{ stockData.esStockDiferencial 
                ? 'Este producto tiene stock específico para la empresa seleccionada.' 
                : 'Este producto está usando el stock base del catálogo.'
              }}
            </p>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Estado de carga -->
    <div v-if="loading && !stockData" class="flex items-center justify-center py-8">
      <div class="text-center">
        <Icon name="i-heroicons-arrow-path" class="w-8 h-8 text-blue-500 animate-spin mx-auto mb-2" />
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Cargando stock...
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { StockDiferencialData } from '~/composables/useStockDiferencial'

interface Props {
  productoId: number
  empresaId: number
  empresaNombre?: string
  initialStock?: number
  disabled?: boolean
  required?: boolean
}

interface Emits {
  (e: 'stock-updated', data: StockDiferencialData): void
  (e: 'stock-reset', data: StockDiferencialData): void
  (e: 'error', error: string): void
}

const props = withDefaults(defineProps<Props>(), {
  initialStock: 0,
  disabled: false,
  required: false
})

const emit = defineEmits<Emits>()

// Composables
const { 
  getStockByEmpresa, 
  updateStockByEmpresa, 
  resetStockDiferencial,
  loading,
  error 
} = useStockDiferencial()

const { validateStockUpdate, isValidStock } = useStockDiferencialValidation()

// Estado local
const stockEmpresa = ref<number>(props.initialStock)
const stockData = ref<StockDiferencialData | null>(null)
const originalStock = ref<number>(props.initialStock)
const stockError = ref<string | null>(null)

// Computadas
const hasChanges = computed(() => {
  return stockEmpresa.value !== originalStock.value
})

const isValidStockValue = computed(() => {
  return isValidStock(stockEmpresa.value)
})

// Validación en tiempo real
const validateCurrentStock = () => {
  if (stockEmpresa.value === null || stockEmpresa.value === undefined) {
    stockError.value = props.required ? 'El stock es requerido' : null
    return false
  }
  
  if (!isValidStockValue.value) {
    stockError.value = 'El stock debe ser un número entero mayor o igual a 0'
    return false
  }
  
  stockError.value = null
  return true
}

// Métodos
const loadStockData = async () => {
  if (!props.empresaId || !props.productoId) return
  
  try {
    const data = await getStockByEmpresa({
      empresaId: props.empresaId,
      productoId: props.productoId
    })
    
    if (data) {
      stockData.value = data
      stockEmpresa.value = data.existencia
      originalStock.value = data.existencia
    }
  } catch (err: any) {
    emit('error', err.message || 'Error al cargar stock')
  }
}

const handleSave = async () => {
  if (!validateCurrentStock()) return
  
  const validation = validateStockUpdate({
    empresaId: props.empresaId,
    productoId: props.productoId,
    existencia: stockEmpresa.value
  })
  
  if (!validation.isValid) {
    stockError.value = validation.errors[0]
    return
  }
  
  try {
    const response = await updateStockByEmpresa({
      empresaId: props.empresaId,
      productoId: props.productoId,
      existencia: stockEmpresa.value
    })
    
    if (response.success && response.data) {
      stockData.value = response.data
      originalStock.value = stockEmpresa.value
      emit('stock-updated', response.data)
    } else {
      emit('error', response.error || 'Error al actualizar stock')
    }
  } catch (err: any) {
    emit('error', err.message || 'Error al actualizar stock')
  }
}

const handleReset = async () => {
  try {
    const response = await resetStockDiferencial({
      empresaId: props.empresaId,
      productoId: props.productoId
    })
    
    if (response.success && response.data) {
      stockData.value = response.data
      stockEmpresa.value = response.data.existencia
      originalStock.value = response.data.existencia
      emit('stock-reset', response.data)
    } else {
      emit('error', response.error || 'Error al resetear stock')
    }
  } catch (err: any) {
    emit('error', err.message || 'Error al resetear stock')
  }
}

const handleCancel = () => {
  stockEmpresa.value = originalStock.value
  stockError.value = null
}

// Watchers
watch(stockEmpresa, () => {
  validateCurrentStock()
})

watch(() => [props.empresaId, props.productoId], () => {
  if (props.empresaId && props.productoId) {
    loadStockData()
  }
}, { immediate: true })

watch(() => props.initialStock, (newValue) => {
  if (!hasChanges.value) {
    stockEmpresa.value = newValue
    originalStock.value = newValue
  }
})

// Inicialización
onMounted(() => {
  if (props.empresaId && props.productoId) {
    loadStockData()
  }
})

// Exponer métodos si es necesario
defineExpose({
  loadStockData,
  stockData: readonly(stockData),
  hasChanges: readonly(hasChanges),
  isValid: computed(() => validateCurrentStock())
})
</script>

<style scoped>
.stock-diferencial {
  @apply bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-700 rounded-lg p-6;
}
</style>