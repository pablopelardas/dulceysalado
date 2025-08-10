<template>
  <AgrupacionesDragDrop
    :empresa-id="empresaId"
    :agrupaciones="agrupacionesData"
    :visible-ids="visibleIds"
    :loading="loading"
    title="Configuración de Novedades"
    description="Arrastra las agrupaciones entre los paneles para configurar cuáles aparecen como novedades"
    visible-label="Agrupaciones en Novedades"
    hidden-label="Agrupaciones Disponibles"
    visible-description="Estas agrupaciones aparecerán como novedades en el catálogo público. Click para mover o arrastra entre paneles."
    hidden-description="Estas agrupaciones están disponibles pero no aparecen como novedades. Click para mover o arrastra entre paneles."
    visible-color="green"
    hidden-color="red"
    @save="handleSave"
    @reset="handleReset"
  />
</template>

<script setup lang="ts">
import type { DragDropAgrupacion } from '~/types/agrupaciones'

interface Props {
  empresaId: number
  agrupaciones: DragDropAgrupacion[]
  visibleIds: number[]
  loading?: boolean
}

interface Emits {
  (e: 'save', visibleIds: number[]): void
  (e: 'reset'): void
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<Emits>()

// Usar las agrupaciones proporcionadas
const agrupacionesData = computed(() => props.agrupaciones)

const handleSave = (visibleIds: number[]) => {
  emit('save', visibleIds)
}

const handleReset = () => {
  emit('reset')
}
</script>