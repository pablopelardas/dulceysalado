<template>
  <UTable
    v-model:sorting="sorting"
    :data="listas"
    :columns="columns"
    :loading="loading"
    class="flex-1"
  />
</template>

<script setup lang="ts">
import { h, resolveComponent } from 'vue'
import type { ListaPrecioDto } from '~/types/listas-precios'
import type { TableColumn } from '@nuxt/ui'

const UButton = resolveComponent('UButton')
const UBadge = resolveComponent('UBadge')
const UIcon = resolveComponent('UIcon')
const UTooltip = resolveComponent('UTooltip')

interface Props {
  listas: ListaPrecioDto[] | readonly ListaPrecioDto[]
  loading?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

interface Emits {
  edit: [lista: ListaPrecioDto]
  delete: [lista: ListaPrecioDto]
  sort: [column: string, order: 'asc' | 'desc']
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Composables
const { userPermissions } = useAuth()

// Estado del ordenamiento
const sorting = ref([
  {
    id: props.sortBy || 'orden',
    desc: props.sortOrder === 'desc'
  }
])

// Estado independiente para tracking del ordenamiento actual
const currentSort = ref({
  column: props.sortBy || 'orden',
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
const columns: TableColumn<ListaPrecioDto>[] = [
  {
    accessorKey: 'codigo',
    header: createSortButton('codigo', 'Código'),
    cell: ({ row }) => {
      const lista = row.original
      return h('div', { class: 'font-mono text-sm font-medium text-gray-900 dark:text-gray-100' }, 
        lista.codigo || '-')
    }
  },
  {
    accessorKey: 'nombre',
    header: createSortButton('nombre', 'Nombre'),
    cell: ({ row }) => {
      const lista = row.original
      return h('div', { class: 'flex items-center gap-2' }, [
        h('p', { class: 'font-medium text-gray-900 dark:text-gray-100' }, 
          lista.nombre || 'Sin nombre'),
        lista.es_predeterminada ? h(UBadge, {
          color: 'blue',
          variant: 'soft',
          size: 'xs'
        }, () => 'Predeterminada') : null
      ])
    }
  },
  {
    id: 'actions',
    header: 'Acciones',
    cell: ({ row }) => {
      const lista = row.original
      const buttons = []
      
      if (canEdit()) {
        buttons.push(h(UButton, {
          icon: 'i-heroicons-pencil',
          size: 'xs',
          color: 'primary',
          variant: 'soft',
          title: 'Editar lista',
          onClick: () => emit('edit', lista)
        }))
      }
      
      if (canDelete() && !lista.es_predeterminada) {
        buttons.push(h(UButton, {
          icon: 'i-heroicons-trash',
          size: 'xs',
          color: 'red',
          variant: 'soft',
          title: 'Eliminar lista',
          onClick: () => emit('delete', lista)
        }))
      } else if (canDelete() && lista.es_predeterminada) {
        buttons.push(h(UTooltip, {
          text: 'No se puede eliminar la lista predeterminada'
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

const canDelete = () => {
  return userPermissions.value?.canManageProductosBase || false
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