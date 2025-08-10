<template>
  <UTable
    v-model:sorting="sorting"
    :data="agrupaciones"
    :columns="columns"
    :loading="loading"
    class="flex-1"
  />
</template>

<script setup lang="ts">
import { h, resolveComponent } from 'vue'
import type { AgrupacionDto } from '~/types/agrupaciones'
import type { TableColumn } from '@nuxt/ui'

const UButton = resolveComponent('UButton')
const UBadge = resolveComponent('UBadge')
const UIcon = resolveComponent('UIcon')
const UTooltip = resolveComponent('UTooltip')

interface Props {
  agrupaciones: AgrupacionDto[] | readonly AgrupacionDto[]
  loading?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

interface Emits {
  edit: [agrupacion: AgrupacionDto]
  sort: [column: string, order: 'asc' | 'desc']
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Composables
const { userPermissions } = useAuth()

// Estado del ordenamiento
const sorting = ref([
  {
    id: props.sortBy || 'codigo',
    desc: props.sortOrder === 'desc'
  }
])

// Estado independiente para tracking del ordenamiento actual
const currentSort = ref({
  column: props.sortBy || 'codigo',
  direction: props.sortOrder || 'asc'
})

// Función helper para crear el botón de ordenamiento
const createSortButton = (columnId: string, label: string) => {
  return () => {
    const isCurrentlySorted = currentSort.value.column === columnId
    const currentDirection = isCurrentlySorted ? currentSort.value.direction : null
    
    return h(UButton, {
      color: 'neutral',
      variant: 'ghost',
      label: label,
      icon: isCurrentlySorted
        ? currentDirection === 'asc'
          ? 'i-heroicons-arrow-up'
          : 'i-heroicons-arrow-down'
        : 'i-heroicons-arrows-up-down',
      class: '-mx-2.5 font-semibold',
      onClick: () => {
        let newDirection: 'asc' | 'desc' = 'asc'
        
        if (isCurrentlySorted) {
          newDirection = currentDirection === 'asc' ? 'desc' : 'asc'
        } else {
          newDirection = 'asc'
        }
        
        currentSort.value = {
          column: columnId,
          direction: newDirection
        }
        
        sorting.value = [{ id: columnId, desc: newDirection === 'desc' }]
        emit('sort', columnId, newDirection)
      }
    })
  }
}

// Columnas de la tabla
const columns: TableColumn<AgrupacionDto>[] = [
  {
    accessorKey: 'codigo',
    header: createSortButton('codigo', 'Código'),
    cell: ({ row }) => {
      const agrupacion = row.original
      return h('div', { class: 'font-mono text-sm font-medium text-gray-900 dark:text-gray-100' }, 
        agrupacion.codigo?.toString() || '-')
    }
  },
  {
    accessorKey: 'nombre',
    header: createSortButton('nombre', 'Nombre'),
    cell: ({ row }) => {
      const agrupacion = row.original
      return h('div', { class: 'flex flex-col gap-1' }, [
        h('p', { class: 'font-medium text-gray-900 dark:text-gray-100' }, 
          agrupacion.nombre || 'Sin nombre'),
        agrupacion.descripcion ? h('p', { class: 'text-sm text-gray-500 dark:text-gray-400 truncate max-w-xs' }, 
          agrupacion.descripcion) : null
      ])
    }
  },
  {
    accessorKey: 'activa',
    header: createSortButton('activa', 'Estado'),
    cell: ({ row }) => {
      const agrupacion = row.original
      return h(UBadge, {
        color: agrupacion.activa ? 'green' : 'red',
        variant: 'soft',
        size: 'xs'
      }, () => agrupacion.activa ? 'Activa' : 'Inactiva')
    }
  },
  {
    accessorKey: 'updated_at',
    header: createSortButton('updated_at', 'Última Actualización'),
    cell: ({ row }) => {
      const agrupacion = row.original
      const date = new Date(agrupacion.updated_at)
      return h('div', { class: 'text-sm text-gray-600 dark:text-gray-400' }, [
        h('p', {}, date.toLocaleDateString('es-AR')),
        h('p', { class: 'text-xs' }, date.toLocaleTimeString('es-AR', { 
          hour: '2-digit', 
          minute: '2-digit' 
        }))
      ])
    }
  },
  {
    id: 'actions',
    header: 'Acciones',
    cell: ({ row }) => {
      const agrupacion = row.original
      const buttons = []
      
      if (canEdit()) {
        buttons.push(h(UButton, {
          icon: 'i-heroicons-pencil',
          size: 'xs',
          color: 'primary',
          variant: 'soft',
          title: 'Editar agrupación',
          onClick: () => emit('edit', agrupacion)
        }))
      }
      
      // Tooltip informativo sobre por qué no se puede eliminar
      if (canEdit()) {
        buttons.push(h(UTooltip, {
          text: 'Las agrupaciones se crean automáticamente desde GECOM y no se pueden eliminar'
        }, () => h(UButton, {
          icon: 'i-heroicons-trash',
          size: 'xs',
          color: 'gray',
          variant: 'soft',
          disabled: true
        })))
      }
      
      return h('div', { class: 'flex items-center gap-2' }, buttons)
    }
  }
]

// Permisos de acciones
const canEdit = () => {
  return userPermissions.value?.canManageProductosBase || false
}

// Helpers para formateo
const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('es-AR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  })
}

const formatTime = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleTimeString('es-AR', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Watcher para sincronizar con props cuando cambien externamente
watch(() => [props.sortBy, props.sortOrder], ([newSortBy, newSortOrder]) => {
  if (newSortBy && newSortOrder) {
    currentSort.value = {
      column: newSortBy,
      direction: newSortOrder
    }
    sorting.value = [{
      id: newSortBy,
      desc: newSortOrder === 'desc'
    }]
  }
}, { immediate: true })
</script>