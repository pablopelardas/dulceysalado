<template>
  <div class="stock-indicator">
    <!-- Indicador principal de stock -->
    <div class="flex items-center gap-2">
      <!-- Valor del stock -->
      <span 
        class="font-semibold text-lg"
        :class="getStockColorClass()"
      >
        {{ displayStock }}
      </span>
      
      <!-- Badge de estado -->
      <UBadge
        :color="getBadgeColor()"
        :variant="getBadgeVariant()"
        size="xs"
      >
        {{ getStockStatus() }}
      </UBadge>
      
      <!-- Icono de tipo de stock -->
      <UTooltip :text="getTooltipText()">
        <Icon 
          :name="getStockIcon()"
          class="w-4 h-4"
          :class="getIconColorClass()"
        />
      </UTooltip>
    </div>
    
    <!-- Información adicional (mostrada en hover/click) -->
    <div v-if="showDetails" class="mt-2 text-xs text-gray-600 dark:text-gray-400">
      <div class="space-y-1">
        <div v-if="stockBase !== undefined && stockBase !== stockEmpresa">
          <span class="font-medium">Stock base:</span> {{ stockBase }}
        </div>
        <div v-if="empresaNombre">
          <span class="font-medium">Empresa:</span> {{ empresaNombre }}
        </div>
        <div v-if="lastUpdated">
          <span class="font-medium">Actualizado:</span> {{ formatDate(lastUpdated) }}
        </div>
      </div>
    </div>
    
    <!-- Barra de progreso para stock bajo -->
    <div v-if="showProgressBar && stockThreshold" class="mt-2">
      <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-1.5">
        <div 
          class="h-1.5 rounded-full transition-all duration-300"
          :class="getProgressBarColor()"
          :style="{ width: `${getStockPercentage()}%` }"
        />
      </div>
      <p class="text-xs text-gray-500 mt-1">
        {{ stockEmpresa }} / {{ stockThreshold }} ({{ Math.round(getStockPercentage()) }}%)
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  stockEmpresa: number
  stockBase?: number
  empresaNombre?: string
  esStockDiferencial?: boolean
  lastUpdated?: string | Date
  stockThreshold?: number // Umbral para considerar stock bajo
  showDetails?: boolean
  showProgressBar?: boolean
  size?: 'sm' | 'md' | 'lg'
}

const props = withDefaults(defineProps<Props>(), {
  stockEmpresa: 0,
  esStockDiferencial: false,
  showDetails: false,
  showProgressBar: false,
  size: 'md'
})

// Computadas
const displayStock = computed(() => {
  return props.stockEmpresa?.toString() || '0'
})

const getStockColorClass = () => {
  const stock = props.stockEmpresa
  
  if (stock === 0) {
    return 'text-red-600 dark:text-red-400'
  } else if (props.stockThreshold && stock <= props.stockThreshold) {
    return 'text-yellow-600 dark:text-yellow-400'
  } else {
    return 'text-green-600 dark:text-green-400'
  }
}

const getBadgeColor = () => {
  const stock = props.stockEmpresa
  
  if (stock === 0) {
    return 'red'
  } else if (props.stockThreshold && stock <= props.stockThreshold) {
    return 'yellow'
  } else {
    return 'green'
  }
}

const getBadgeVariant = () => {
  return props.esStockDiferencial ? 'solid' : 'soft'
}

const getStockStatus = () => {
  const stock = props.stockEmpresa
  
  if (stock === 0) {
    return 'Sin stock'
  } else if (props.stockThreshold && stock <= props.stockThreshold) {
    return 'Stock bajo'
  } else {
    return 'Disponible'
  }
}

const getStockIcon = () => {
  if (props.esStockDiferencial) {
    return 'i-heroicons-adjustments-horizontal'
  } else {
    return 'i-heroicons-cube'
  }
}

const getIconColorClass = () => {
  if (props.esStockDiferencial) {
    return 'text-blue-500'
  } else {
    return 'text-gray-400'
  }
}

const getTooltipText = () => {
  if (props.esStockDiferencial) {
    return `Stock diferencial para ${props.empresaNombre || 'empresa seleccionada'}`
  } else {
    return 'Stock base del producto'
  }
}

const getProgressBarColor = () => {
  const stock = props.stockEmpresa
  
  if (stock === 0) {
    return 'bg-red-500'
  } else if (props.stockThreshold && stock <= props.stockThreshold) {
    return 'bg-yellow-500'
  } else {
    return 'bg-green-500'
  }
}

const getStockPercentage = () => {
  if (!props.stockThreshold) return 100
  
  const percentage = (props.stockEmpresa / props.stockThreshold) * 100
  return Math.min(percentage, 100)
}

// Métodos
const formatDate = (date: string | Date) => {
  if (!date) return ''
  
  const dateObj = typeof date === 'string' ? new Date(date) : date
  
  return new Intl.DateTimeFormat('es-ES', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(dateObj)
}

// Emits para interactividad
interface Emits {
  (e: 'click'): void
  (e: 'details-toggle', show: boolean): void
}

const emit = defineEmits<Emits>()

const handleClick = () => {
  emit('click')
}

const toggleDetails = () => {
  emit('details-toggle', !props.showDetails)
}
</script>

<style scoped>
.stock-indicator {
  @apply inline-flex flex-col;
}

/* Variantes de tamaño */
.stock-indicator.size-sm {
  @apply text-sm;
}

.stock-indicator.size-md {
  @apply text-base;
}

.stock-indicator.size-lg {
  @apply text-lg;
}
</style>