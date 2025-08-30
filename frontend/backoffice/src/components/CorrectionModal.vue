<template>
  <UModal 
    v-model:open="showModal"
    prevent-close
  >
    <template #header>
      <div class="flex items-center justify-between">
        <h3 class="text-lg font-semibold">Corregir Pedido</h3>
        <UButton 
          color="gray" 
          variant="ghost" 
          icon="i-heroicons-x-mark-20-solid" 
          class="-my-1" 
          @click="closeModal" 
        />
      </div>
    </template>
    
    <template #body>
      <div class="space-y-6">
        <div class="bg-blue-50 dark:bg-blue-900/20 p-4 rounded-lg">
          <p class="text-sm text-blue-800 dark:text-blue-200">
            <strong>Pedido #{{ pedido?.numero }}</strong><br>
            Ajusta las cantidades de los productos según disponibilidad de stock.
          </p>
        </div>

        <!-- Lista de items para corregir -->
        <div class="space-y-4">
          <h4 class="font-medium text-gray-900 dark:text-gray-100">Items del Pedido</h4>
          
          <div v-for="(item, index) in correctionItems" :key="item.codigo_producto" 
               class="border dark:border-gray-700 rounded-lg p-4 space-y-3">
            <div class="flex items-center justify-between">
              <div>
                <p class="font-medium text-gray-900 dark:text-gray-100">{{ item.nombre_producto }}</p>
                <p class="text-sm text-gray-500">Código: {{ item.codigo_producto }}</p>
                <p class="text-sm text-gray-500">Precio: ${{ item.precio_unitario.toLocaleString() }}</p>
              </div>
              
              <div class="text-right">
                <p class="text-sm text-gray-500">Original: {{ item.cantidad_original }} unidades</p>
                <p class="font-medium">${{ (item.cantidad_original * item.precio_unitario).toLocaleString() }}</p>
              </div>
            </div>
            
            <div class="grid grid-cols-2 gap-4">
              <UFormField label="Nueva cantidad">
                <UInput
                  v-model.number="correctionItems[index].nueva_cantidad"
                  type="number"
                  min="0"
                  :max="item.cantidad_original"
                />
              </UFormField>
              
              <UFormField label="Motivo (opcional)">
                <div class="space-y-2">
                  <USelectMenu
                    v-model="itemMotivoSelections[index]"
                    :items="[
                      'Falta de stock',
                      'Compra mínima del producto',
                      'Producto descontinuado',
                      'Otro motivo'
                    ]"
                    @update:model-value="updateItemMotivo(index, $event)"
                    placeholder="Seleccionar motivo..."
                  />
                  <UInput
                    v-if="itemMotivoSelections[index] === 'Otro motivo'"
                    v-model="correctionItems[index].motivo"
                    placeholder="Especifica el motivo..."
                    class="w-full"
                  />
                </div>
              </UFormField>
            </div>
            
            <!-- Mostrar cambio si hay diferencia -->
            <div v-if="item.nueva_cantidad !== item.cantidad_original" 
                 class="bg-yellow-50 dark:bg-yellow-900/20 p-3 rounded">
              <div class="flex items-center justify-between text-sm">
                <span class="text-yellow-800 dark:text-yellow-200">
                  <template v-if="item.nueva_cantidad === 0">
                    <UIcon name="i-heroicons-x-mark" class="inline w-4 h-4" />
                    Producto eliminado
                  </template>
                  <template v-else>
                    <UIcon name="i-heroicons-arrow-right" class="inline w-4 h-4" />
                    {{ item.cantidad_original }} → {{ item.nueva_cantidad }} unidades
                  </template>
                </span>
                <span class="font-medium text-yellow-800 dark:text-yellow-200">
                  ${{ (item.nueva_cantidad * item.precio_unitario).toLocaleString() }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Resumen del pedido corregido -->
        <div class="border-t pt-4">
          <div class="bg-gray-50 dark:bg-gray-800 p-4 rounded-lg">
            <div class="flex items-center justify-between mb-2">
              <span class="font-medium">Total Original:</span>
              <span>${{ pedido?.monto_total?.toLocaleString() }}</span>
            </div>
            <div class="flex items-center justify-between font-bold text-lg">
              <span>Total Corregido:</span>
              <span class="text-green-600 dark:text-green-400">
                ${{ getTotalCorregido().toLocaleString() }}
              </span>
            </div>
            <div v-if="getTotalCorregido() !== pedido?.monto_total" 
                 class="text-sm text-yellow-600 dark:text-yellow-400 mt-1">
              Diferencia: ${{ Math.abs(getTotalCorregido() - (pedido?.monto_total || 0)).toLocaleString() }}
              {{ getTotalCorregido() < (pedido?.monto_total || 0) ? 'menos' : 'más' }}
            </div>
          </div>
        </div>

        <!-- Motivo general y opciones -->
        <div class="space-y-4">
          <UFormField label="Motivo general de la corrección (opcional)">
            <div class="space-y-3">
              <!-- Opciones predefinidas -->
              <div class="grid gap-2">
                <label class="flex items-center space-x-3 cursor-pointer">
                  <input 
                    type="radio" 
                    name="correction-reason" 
                    value="Falta de stock"
                    v-model="selectedCorrectionReason"
                    class="w-4 h-4 text-primary-600 border-gray-300 focus:ring-primary-500"
                  />
                  <span class="text-sm text-gray-700 dark:text-gray-300">Falta de stock</span>
                </label>
                
                <label class="flex items-center space-x-3 cursor-pointer">
                  <input 
                    type="radio" 
                    name="correction-reason" 
                    value="Compra mínima del producto"
                    v-model="selectedCorrectionReason"
                    class="w-4 h-4 text-primary-600 border-gray-300 focus:ring-primary-500"
                  />
                  <span class="text-sm text-gray-700 dark:text-gray-300">Compra mínima del producto</span>
                </label>
                
                <label class="flex items-center space-x-3 cursor-pointer">
                  <input 
                    type="radio" 
                    name="correction-reason" 
                    value="Producto descontinuado"
                    v-model="selectedCorrectionReason"
                    class="w-4 h-4 text-primary-600 border-gray-300 focus:ring-primary-500"
                  />
                  <span class="text-sm text-gray-700 dark:text-gray-300">Producto descontinuado</span>
                </label>
                
                <label class="flex items-center space-x-3 cursor-pointer">
                  <input 
                    type="radio" 
                    name="correction-reason" 
                    value="otro"
                    v-model="selectedCorrectionReason"
                    class="w-4 h-4 text-primary-600 border-gray-300 focus:ring-primary-500"
                  />
                  <span class="text-sm text-gray-700 dark:text-gray-300">Otro motivo</span>
                </label>
              </div>
              
              <!-- Campo de texto personalizado -->
              <div v-if="selectedCorrectionReason === 'otro'" class="mt-3 w-full">
                <UTextarea
                  v-model="customCorrectionReason"
                  placeholder="Especifica el motivo de la corrección..."
                  :rows="2"
                  class="w-full"
                />
              </div>
            </div>
          </UFormField>

          <div class="flex items-center space-x-3">
            <UCheckbox v-model="sendToClient" />
            <label class="text-sm text-gray-700 dark:text-gray-300">
              Enviar corrección al cliente para aprobación
            </label>
          </div>
          
          <div v-if="!sendToClient" class="bg-orange-50 dark:bg-orange-900/20 p-3 rounded-lg">
            <p class="text-sm text-orange-800 dark:text-orange-200">
              ⚠️ La corrección quedará en estado "En Corrección" para continuar editando después.
            </p>
          </div>
        </div>
      </div>
    </template>

    <template #footer>
      <div class="flex justify-end gap-3">
        <UButton @click="closeModal" color="gray" variant="ghost">
          Cancelar
        </UButton>
        <UButton
          @click="confirmCorrection"
          color="yellow"
          :disabled="!hasChanges"
          :loading="loading"
        >
          {{ sendToClient ? 'Enviar Corrección' : 'Guardar Cambios' }}
        </UButton>
      </div>
    </template>
  </UModal>
</template>

<script setup lang="ts">
import type { Pedido } from '~/types/pedidos'

interface Props {
  show: boolean
  pedido: Pedido | null
  loading?: boolean
}

interface Emits {
  (e: 'close'): void
  (e: 'confirm', data: {
    items: Array<{
      codigo_producto: string
      nueva_cantidad: number
      motivo: string | null
    }>
    motivo_correccion: string | null
    enviar_al_cliente: boolean
    motivo_final: string
  }): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Variables reactivas
const correctionItems = ref<Array<{
  codigo_producto: string
  nombre_producto: string
  cantidad_original: number
  nueva_cantidad: number
  precio_unitario: number
  motivo: string
}>>([])

const itemMotivoSelections = ref<Array<string>>([])
const selectedCorrectionReason = ref('')
const customCorrectionReason = ref('')
const sendToClient = ref(true)

// Computed
const showModal = computed({
  get: () => props.show,
  set: (value) => {
    if (!value) emit('close')
  }
})

const hasChanges = computed(() => {
  return correctionItems.value.some(item => 
    item.nueva_cantidad !== item.cantidad_original || item.motivo
  )
})

// Funciones
const closeModal = () => {
  emit('close')
  resetForm()
}

const resetForm = () => {
  correctionItems.value = []
  itemMotivoSelections.value = []
  selectedCorrectionReason.value = ''
  customCorrectionReason.value = ''
  sendToClient.value = true
}

const initializeItems = () => {
  if (!props.pedido) return
  
  correctionItems.value = props.pedido.items.map(item => ({
    codigo_producto: item.codigo_producto,
    nombre_producto: item.nombre_producto,
    cantidad_original: item.cantidad,
    nueva_cantidad: item.cantidad,
    precio_unitario: item.precio_unitario,
    motivo: ''
  }))
  
  itemMotivoSelections.value = new Array(props.pedido.items.length).fill('')
}

const updateItemMotivo = (index: number, value: string) => {
  if (value === 'Otro motivo') {
    correctionItems.value[index].motivo = ''
  } else {
    correctionItems.value[index].motivo = value
  }
}

const getTotalCorregido = () => {
  return correctionItems.value.reduce((total, item) => {
    return total + (item.nueva_cantidad * item.precio_unitario)
  }, 0)
}

const getFinalCorrectionReason = () => {
  if (selectedCorrectionReason.value === 'otro') {
    return customCorrectionReason.value.trim()
  }
  return selectedCorrectionReason.value
}

const confirmCorrection = () => {
  if (!props.pedido) return

  const items = correctionItems.value
    .filter(item => item.nueva_cantidad !== item.cantidad_original || item.motivo)
    .map(item => ({
      codigo_producto: item.codigo_producto,
      nueva_cantidad: item.nueva_cantidad,
      motivo: item.motivo || null
    }))

  if (items.length === 0) {
    closeModal()
    return
  }

  const motivoFinal = getFinalCorrectionReason()

  emit('confirm', {
    items,
    motivo_correccion: motivoFinal || null,
    enviar_al_cliente: sendToClient.value,
    motivo_final: motivoFinal
  })
}

// Watchers
watch(() => props.show, (newShow) => {
  if (newShow && props.pedido) {
    initializeItems()
  }
})
</script>