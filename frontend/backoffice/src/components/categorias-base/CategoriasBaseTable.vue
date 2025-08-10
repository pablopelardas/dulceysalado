<template>
  <UTable
    v-model:sorting="sorting"
    :data="categories"
    :columns="columns"
    :loading="loading"
    class="flex-1"
  />
</template>

<script setup lang="ts">
import { h, resolveComponent } from 'vue'
import type { CategoryBaseDto } from '~/types/categorias'
import type { TableColumn } from '@nuxt/ui'

const UButton = resolveComponent('UButton')
const UBadge = resolveComponent('UBadge')
const UIcon = resolveComponent('UIcon')

interface Props {
  categories: CategoryBaseDto[] | readonly CategoryBaseDto[]
  loading?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

interface Emits {
  edit: [category: CategoryBaseDto]
  delete: [category: CategoryBaseDto]
  sort: [column: string, order: 'asc' | 'desc']
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Composables
const { userPermissions } = useAuth()

// Estado del ordenamiento
const sorting = ref([
  {
    id: props.sortBy || 'nombre',
    desc: props.sortOrder === 'desc'
  }
])

const currentSort = ref({
  column: props.sortBy || 'nombre',
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
const columns: TableColumn<CategoryBaseDto>[] = [
  {
    accessorKey: 'icono',
    header: 'Icono',
    cell: ({ row }) => {
      const category = row.original
      // No mostrar nada si no hay icono o si es la cajita por defecto
      const shouldShowIcon = !!category.icono
      
      return h('div', { class: 'flex items-center justify-center min-h-[2rem]' }, [
        shouldShowIcon ? h('span', { 
          class: 'text-2xl',
          title: category.nombre 
        }, category.icono!) : h('span', { class: 'text-gray-400' }, '—')
      ])
    }
  },
  {
    accessorKey: 'nombre',
    header: createSortButton('nombre', 'Nombre'),
    cell: ({ row }) => {
      const category = row.original
      return h('div', { class: 'flex flex-col gap-1' }, [
        h('p', { class: 'font-medium text-gray-900 dark:text-gray-100' }, 
          category.nombre || 'Sin nombre'),
        category.descripcion ? h('p', { class: 'text-sm text-gray-500 dark:text-gray-400 truncate max-w-xs' }, 
          category.descripcion) : null
      ])
    }
  },
  {
    accessorKey: 'codigo_rubro',
    header: createSortButton('codigo_rubro', 'Código'),
    cell: ({ row }) => {
      const category = row.original
      return h('span', { class: 'font-mono text-sm text-gray-900 dark:text-gray-100' }, 
        category.codigo_rubro?.toString() || '-')
    }
  },
  {
    accessorKey: 'visible',
    header: 'Visibilidad',
    cell: ({ row }) => {
      const category = row.original
      return h(UBadge, {
        color: category.visible ? 'green' : 'red',
        variant: 'soft',
        size: 'xs'
      }, () => category.visible ? 'Visible' : 'Oculto')
    }
  },
  {
    accessorKey: 'orden',
    header: createSortButton('orden', 'Orden'),
    cell: ({ row }) => {
      const category = row.original
      return h('span', { class: 'text-gray-600 dark:text-gray-400' }, 
        category.orden?.toString() || '0')
    }
  },
  {
    accessorKey: 'cantidad_productos',
    header: 'Productos',
    cell: ({ row }) => {
      const category = row.original
      return h('span', { class: 'text-gray-600 dark:text-gray-400' }, 
        `${category.cantidad_productos || 0} productos`)
    }
  },
  {
    accessorKey: 'fecha_creacion',
    header: createSortButton('fecha_creacion', 'Creada'),
    cell: ({ row }) => {
      const category = row.original
      return h('span', { class: 'text-gray-600 dark:text-gray-400' }, 
        category.fecha_creacion ? formatDate(category.fecha_creacion) : '-')
    }
  },
  {
    id: 'actions',
    header: 'Acciones',
    cell: ({ row }) => {
      const category = row.original
      const buttons = []
      
      if (canEdit()) {
        buttons.push(h(UButton, {
          icon: 'i-heroicons-pencil',
          size: 'xs',
          color: 'primary',
          variant: 'soft',
          title: 'Editar categoría',
          onClick: () => emit('edit', category)
        }))
      }
      
      if (canDelete()) {
        buttons.push(h(UButton, {
          icon: 'i-heroicons-trash',
          size: 'xs',
          color: 'red',
          variant: 'soft',
          title: 'Eliminar categoría',
          onClick: () => emit('delete', category)
        }))
      }
      
      return h('div', { class: 'flex items-center gap-2' }, buttons)
    }
  }
]

// Permisos de acciones
const canEdit = () => {
  return userPermissions.value?.canManageCategoriasBase || false
}

const canDelete = () => {
  return userPermissions.value?.canManageCategoriasBase || false
}

// Helper para formatear fechas
const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('es-AR')
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