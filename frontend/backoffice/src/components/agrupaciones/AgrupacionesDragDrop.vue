<template>
  <div class="space-y-6">
    <!-- Header con información de la empresa -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
      <div class="flex items-center justify-between">
        <div>
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            {{ title }}
          </h3>
          <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
            {{ description }}
          </p>
        </div>
        <div class="flex items-center space-x-4">
          <UButton
            v-if="hasChanges"
            color="primary"
            variant="solid"
            :loading="loading"
            @click="saveChanges"
          >
            <UIcon name="i-heroicons-check" class="mr-2" />
            Guardar Cambios
          </UButton>
          <UButton
            v-if="hasChanges"
            color="gray"
            variant="outline"
            @click="resetChanges"
          >
            <UIcon name="i-heroicons-x-mark" class="mr-2" />
            Descartar
          </UButton>
        </div>
      </div>
    </div>

    <!-- Estadísticas -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <UCard>
        <div class="flex items-center">
          <UIcon name="i-heroicons-squares-2x2" class="h-8 w-8 text-blue-600 mr-3" />
          <div>
            <p class="text-sm text-gray-600 dark:text-gray-400">Total</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-gray-100">
              {{ totalAgrupaciones }}
            </p>
          </div>
        </div>
      </UCard>
      <UCard>
        <div class="flex items-center">
          <UIcon name="i-heroicons-eye" :class="`h-8 w-8 text-${visibleColor}-600 mr-3`" />
          <div>
            <p class="text-sm text-gray-600 dark:text-gray-400">Visibles</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-gray-100">
              {{ visibleAgrupaciones.length }}
            </p>
          </div>
        </div>
      </UCard>
      <UCard>
        <div class="flex items-center">
          <UIcon name="i-heroicons-eye-slash" :class="`h-8 w-8 text-${hiddenColor}-600 mr-3`" />
          <div>
            <p class="text-sm text-gray-600 dark:text-gray-400">Ocultas</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-gray-100">
              {{ noVisibleAgrupaciones.length }}
            </p>
          </div>
        </div>
      </UCard>
    </div>

    <!-- Paneles de Drag & Drop -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Panel Visible -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm">
        <div class="p-4 border-b border-gray-200 dark:border-gray-700">
          <div class="flex items-center justify-between">
            <div class="flex items-center">
              <h4 :class="`text-md font-semibold text-${visibleColor}-900 dark:text-${visibleColor}-100`">
                <UIcon name="i-heroicons-eye" class="mr-2" />
                {{ visibleLabel }}
              </h4>
              <UBadge :color="visibleColor" variant="soft" class="ml-2">
                {{ visibleAgrupaciones.length }}
              </UBadge>
            </div>
            <div class="flex items-center space-x-2">
              <UButton
                v-if="noVisibleAgrupaciones.length > 0"
                :color="visibleColor"
                variant="ghost"
                size="xs"
                @click="moveAllToVisible"
              >
                <UIcon name="i-heroicons-chevron-double-left" class="mr-1" />
                Todas
              </UButton>
              <UButton
                v-if="visibleAgrupaciones.length > 0"
                :color="hiddenColor"
                variant="ghost"
                size="xs"
                @click="moveAllToHidden"
              >
                <UIcon name="i-heroicons-chevron-double-right" class="mr-1" />
                Ninguna
              </UButton>
            </div>
          </div>
          <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
            {{ visibleDescription }}
          </p>
        </div>
        <div
          ref="visibleDropZone"
          class="min-h-[300px] p-4 space-y-2"
          :class="[
            'transition-colors duration-200',
            isDraggingOver.visible ? `bg-${visibleColor}-50 dark:bg-${visibleColor}-900/20` : ''
          ]"
          @drop="onDrop('visible', $event)"
          @dragover.prevent
          @dragenter.prevent="onDragEnter('visible')"
          @dragleave.prevent="onDragLeave('visible')"
        >
          <div
            v-for="agrupacion in visibleAgrupaciones"
            :key="agrupacion.id"
            :draggable="true"
            :class="`drag-item bg-${visibleColor}-50 dark:bg-${visibleColor}-900/20 border border-${visibleColor}-200 dark:border-${visibleColor}-800 rounded-lg p-3 cursor-pointer hover:shadow-md transition-all hover:bg-${visibleColor}-100 dark:hover:bg-${visibleColor}-900/30`"
            @dragstart="onDragStart(agrupacion, 'visible')"
            @click="moveItem(agrupacion, 'visible', 'noVisible')"
          >
            <div class="flex items-center justify-between">
              <div class="flex-1">
                <div class="flex items-center space-x-2">
                  <UBadge :color="visibleColor" variant="soft" size="xs">
                    {{ agrupacion.codigo }}
                  </UBadge>
                  <span :class="`font-medium text-${visibleColor}-900 dark:text-${visibleColor}-100`">
                    {{ agrupacion.nombre }}
                  </span>
                </div>
                <p v-if="agrupacion.descripcion" :class="`text-sm text-${visibleColor}-700 dark:text-${visibleColor}-300 mt-1`">
                  {{ agrupacion.descripcion }}
                </p>
              </div>
              <div class="flex items-center space-x-1">
                <UIcon name="i-heroicons-bars-3" :class="`h-4 w-4 text-${visibleColor}-600 dark:text-${visibleColor}-400`" />
                <UIcon name="i-heroicons-arrow-right" :class="`h-4 w-4 text-${visibleColor}-600 dark:text-${visibleColor}-400`" />
              </div>
            </div>
          </div>
          <div
            v-if="visibleAgrupaciones.length === 0"
            class="flex flex-col items-center justify-center h-32 text-gray-500 dark:text-gray-400"
          >
            <UIcon name="i-heroicons-eye-slash" class="h-12 w-12 mb-2" />
            <p class="text-sm">No hay agrupaciones visibles</p>
            <p class="text-xs">Arrastra elementos desde el panel derecho</p>
          </div>
        </div>
      </div>

      <!-- Panel No Visible -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm">
        <div class="p-4 border-b border-gray-200 dark:border-gray-700">
          <div class="flex items-center justify-between">
            <div class="flex items-center">
              <h4 :class="`text-md font-semibold text-${hiddenColor}-900 dark:text-${hiddenColor}-100`">
                <UIcon name="i-heroicons-eye-slash" class="mr-2" />
                {{ hiddenLabel }}
              </h4>
              <UBadge :color="hiddenColor" variant="soft" class="ml-2">
                {{ noVisibleAgrupaciones.length }}
              </UBadge>
            </div>
            <div class="flex items-center space-x-2">
              <UButton
                v-if="visibleAgrupaciones.length > 0"
                :color="visibleColor"
                variant="ghost"
                size="xs"
                @click="moveAllToHidden"
              >
                <UIcon name="i-heroicons-chevron-double-left" class="mr-1" />
                Todas
              </UButton>
              <UButton
                v-if="noVisibleAgrupaciones.length > 0"
                :color="hiddenColor"
                variant="ghost"
                size="xs"
                @click="moveAllToVisible"
              >
                <UIcon name="i-heroicons-chevron-double-right" class="mr-1" />
                Mostrar
              </UButton>
            </div>
          </div>
          <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
            {{ hiddenDescription }}
          </p>
        </div>
        <div
          ref="noVisibleDropZone"
          class="min-h-[300px] p-4 space-y-2"
          :class="[
            'transition-colors duration-200',
            isDraggingOver.noVisible ? `bg-${hiddenColor}-50 dark:bg-${hiddenColor}-900/20` : ''
          ]"
          @drop="onDrop('noVisible', $event)"
          @dragover.prevent
          @dragenter.prevent="onDragEnter('noVisible')"
          @dragleave.prevent="onDragLeave('noVisible')"
        >
          <div
            v-for="agrupacion in noVisibleAgrupaciones"
            :key="agrupacion.id"
            :draggable="true"
            :class="`drag-item bg-${hiddenColor}-50 dark:bg-${hiddenColor}-900/20 border border-${hiddenColor}-200 dark:border-${hiddenColor}-800 rounded-lg p-3 cursor-pointer hover:shadow-md transition-all hover:bg-${hiddenColor}-100 dark:hover:bg-${hiddenColor}-900/30`"
            @dragstart="onDragStart(agrupacion, 'noVisible')"
            @click="moveItem(agrupacion, 'noVisible', 'visible')"
          >
            <div class="flex items-center justify-between">
              <div class="flex-1">
                <div class="flex items-center space-x-2">
                  <UBadge :color="hiddenColor" variant="soft" size="xs">
                    {{ agrupacion.codigo }}
                  </UBadge>
                  <span :class="`font-medium text-${hiddenColor}-900 dark:text-${hiddenColor}-100`">
                    {{ agrupacion.nombre }}
                  </span>
                </div>
                <p v-if="agrupacion.descripcion" :class="`text-sm text-${hiddenColor}-700 dark:text-${hiddenColor}-300 mt-1`">
                  {{ agrupacion.descripcion }}
                </p>
              </div>
              <div class="flex items-center space-x-1">
                <UIcon name="i-heroicons-arrow-left" :class="`h-4 w-4 text-${hiddenColor}-600 dark:text-${hiddenColor}-400`" />
                <UIcon name="i-heroicons-bars-3" :class="`h-4 w-4 text-${hiddenColor}-600 dark:text-${hiddenColor}-400`" />
              </div>
            </div>
          </div>
          <div
            v-if="noVisibleAgrupaciones.length === 0"
            class="flex flex-col items-center justify-center h-32 text-gray-500 dark:text-gray-400"
          >
            <UIcon name="i-heroicons-eye" class="h-12 w-12 mb-2" />
            <p class="text-sm">Todas las agrupaciones están visibles</p>
            <p class="text-xs">Arrastra elementos desde el panel izquierdo</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { DragDropAgrupacion } from '~/types/agrupaciones'

interface Props {
  empresaId: number
  agrupaciones: DragDropAgrupacion[]
  visibleIds: number[]
  loading?: boolean
  // Configuración personalizable
  title?: string
  description?: string
  visibleLabel?: string
  hiddenLabel?: string
  visibleDescription?: string
  hiddenDescription?: string
  visibleColor?: string
  hiddenColor?: string
}

interface Emits {
  (e: 'save', visibleIds: number[]): void
  (e: 'reset'): void
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  title: 'Configuración de Agrupaciones',
  description: 'Arrastra las agrupaciones entre los paneles para configurar su visibilidad',
  visibleLabel: 'Agrupaciones Visibles',
  hiddenLabel: 'Agrupaciones Ocultas',
  visibleDescription: 'Estas agrupaciones serán visibles en el catálogo. Click para mover o arrastra entre paneles.',
  hiddenDescription: 'Estas agrupaciones no serán visibles en el catálogo. Click para mover o arrastra entre paneles.',
  visibleColor: 'green',
  hiddenColor: 'red'
})

const emit = defineEmits<Emits>()

// Estado reactivo
const visibleAgrupaciones = ref<DragDropAgrupacion[]>([])
const noVisibleAgrupaciones = ref<DragDropAgrupacion[]>([])
const originalVisibleIds = ref<number[]>([])
const isDraggingOver = reactive({
  visible: false,
  noVisible: false
})

// Estado de drag & drop
const draggedItem = ref<DragDropAgrupacion | null>(null)
const draggedFrom = ref<'visible' | 'noVisible' | null>(null)

// Métodos (declarados antes de watchers para evitar errores de hoisting)
const initializeData = () => {
  originalVisibleIds.value = [...props.visibleIds]
  
  visibleAgrupaciones.value = props.agrupaciones
    .filter(agrupacion => props.visibleIds.includes(agrupacion.id))
    .sort((a, b) => a.codigo - b.codigo)
  
  noVisibleAgrupaciones.value = props.agrupaciones
    .filter(agrupacion => !props.visibleIds.includes(agrupacion.id))
    .sort((a, b) => a.codigo - b.codigo)
}

// Computed
const totalAgrupaciones = computed(() => props.agrupaciones.length)

const hasChanges = computed(() => {
  const currentVisibleIds = visibleAgrupaciones.value.map(a => a.id).sort()
  const originalIds = originalVisibleIds.value.sort()
  return JSON.stringify(currentVisibleIds) !== JSON.stringify(originalIds)
})

// Watchers
watch(
  () => [props.agrupaciones, props.visibleIds],
  () => {
    initializeData()
  },
  { immediate: true, deep: true }
)

const onDragStart = (agrupacion: DragDropAgrupacion, from: 'visible' | 'noVisible') => {
  draggedItem.value = agrupacion
  draggedFrom.value = from
}

const onDragEnter = (zone: 'visible' | 'noVisible') => {
  if (draggedFrom.value !== zone) {
    isDraggingOver[zone] = true
  }
}

const onDragLeave = (zone: 'visible' | 'noVisible') => {
  isDraggingOver[zone] = false
}

const onDrop = (zone: 'visible' | 'noVisible', event: DragEvent) => {
  event.preventDefault()
  isDraggingOver[zone] = false
  
  if (!draggedItem.value || !draggedFrom.value || draggedFrom.value === zone) {
    return
  }
  
  const item = draggedItem.value
  
  // Remover del panel origen
  if (draggedFrom.value === 'visible') {
    visibleAgrupaciones.value = visibleAgrupaciones.value.filter(a => a.id !== item.id)
  } else {
    noVisibleAgrupaciones.value = noVisibleAgrupaciones.value.filter(a => a.id !== item.id)
  }
  
  // Agregar al panel destino
  if (zone === 'visible') {
    visibleAgrupaciones.value.push(item)
    visibleAgrupaciones.value.sort((a, b) => a.codigo - b.codigo)
  } else {
    noVisibleAgrupaciones.value.push(item)
    noVisibleAgrupaciones.value.sort((a, b) => a.codigo - b.codigo)
  }
  
  // Limpiar estado drag
  draggedItem.value = null
  draggedFrom.value = null
}

const saveChanges = () => {
  const newVisibleIds = visibleAgrupaciones.value.map(a => a.id)
  emit('save', newVisibleIds)
}

const resetChanges = () => {
  initializeData()
  emit('reset')
}

// Métodos para click directo (soporte mobile)
const moveItem = (agrupacion: DragDropAgrupacion, from: 'visible' | 'noVisible', to: 'visible' | 'noVisible') => {
  // Remover del panel origen
  if (from === 'visible') {
    visibleAgrupaciones.value = visibleAgrupaciones.value.filter(a => a.id !== agrupacion.id)
  } else {
    noVisibleAgrupaciones.value = noVisibleAgrupaciones.value.filter(a => a.id !== agrupacion.id)
  }
  
  // Agregar al panel destino
  if (to === 'visible') {
    visibleAgrupaciones.value.push(agrupacion)
    visibleAgrupaciones.value.sort((a, b) => a.codigo - b.codigo)
  } else {
    noVisibleAgrupaciones.value.push(agrupacion)
    noVisibleAgrupaciones.value.sort((a, b) => a.codigo - b.codigo)
  }
}

// Métodos para mover todas las agrupaciones
const moveAllToVisible = () => {
  // Mover todas las agrupaciones ocultas a visibles
  const allItems = [...noVisibleAgrupaciones.value]
  visibleAgrupaciones.value = [...visibleAgrupaciones.value, ...allItems]
  visibleAgrupaciones.value.sort((a, b) => a.codigo - b.codigo)
  noVisibleAgrupaciones.value = []
}

const moveAllToHidden = () => {
  // Mover todas las agrupaciones visibles a ocultas
  const allItems = [...visibleAgrupaciones.value]
  noVisibleAgrupaciones.value = [...noVisibleAgrupaciones.value, ...allItems]
  noVisibleAgrupaciones.value.sort((a, b) => a.codigo - b.codigo)
  visibleAgrupaciones.value = []
}

// Cleanup on unmount
onBeforeUnmount(() => {
  draggedItem.value = null
  draggedFrom.value = null
})
</script>

<style scoped>
.drag-item {
  user-select: none;
}

.drag-item:hover {
  transform: translateY(-1px);
}

.drag-item.dragging {
  opacity: 0.5;
}
</style>