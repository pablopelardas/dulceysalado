<template>
  <UTable
    v-model:sorting="sorting"
    :data="productos"
    :columns="columns"
    :loading="loading"
    class="flex-1"
  />
</template>

<script setup lang="ts">
import { h, resolveComponent } from 'vue'
import type { ProductoBaseDto } from '~/types/productos'
import type { TableColumn } from '@nuxt/ui'

const UButton = resolveComponent('UButton')
const UBadge = resolveComponent('UBadge')
const UAvatar = resolveComponent('UAvatar')
const UTooltip = resolveComponent('UTooltip')

interface Props {
  productos: ProductoBaseDto[] | readonly ProductoBaseDto[]
  loading?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
  listaSeleccionada?: any
  readOnly?: boolean
}

interface Emits {
  edit: [producto: ProductoBaseDto]
  delete: [producto: ProductoBaseDto]
  sort: [column: string, order: 'asc' | 'desc']
  'edit-image': [producto: ProductoBaseDto]
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Composables
const { userPermissions } = useAuth()

// Estado del ordenamiento - inicializar desde props
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

// Función helper para verificar si un campo es sincronizado
const isSyncField = (fieldName: string): boolean => {
  const syncFields = [
    'descripcion',
    'codigo_rubro',
    'precio',
    'existencia',
    'grupo1',
    'grupo2',
    'grupo3',
    'fechaAlta',
    'fechaModi',
    'imputable',
    'disponible',
    'codigoUbicacion'
  ]
  return syncFields.includes(fieldName.toLowerCase()) || syncFields.includes(fieldName)
}

// Función helper para crear el botón de ordenamiento
const createSortButton = (columnId: string, label: string) => {
  return () => {
    // Determinar si esta columna está ordenada
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
          // Si ya está ordenado por esta columna, alternar dirección
          newDirection = currentDirection === 'asc' ? 'desc' : 'asc'
        } else {
          // Si es una nueva columna, empezar con ASC
          newDirection = 'asc'
        }
        
        // Actualizar estado local INMEDIATAMENTE
        currentSort.value = {
          column: columnId,
          direction: newDirection
        }
        
        // Actualizar estado de la tabla
        sorting.value = [{ id: columnId, desc: newDirection === 'desc' }]
        
        emit('sort', columnId, newDirection)
      }
    })
  }
}

// Función específica para crear el header de precio con tooltip
const createPriceHeader = () => {
  return () => {
    const sortButton = createSortButton('precio', 'Precio')()
    
    if (props.listaSeleccionada) {
      return h(UTooltip, {
        text: `Los precios corresponden a: ${props.listaSeleccionada.nombre}`
      }, () => sortButton)
    }
    
    return sortButton
  }
}

// Columnas de la tabla
const columns: TableColumn<ProductoBaseDto>[] = [
  {
    accessorKey: 'imagen_url',
    header: 'Imagen',
    cell: ({ row }) => {
      const producto = row.original
      if (props.readOnly) {
        return h(UAvatar, {
          src: producto.imagen_url || undefined,
          alt: producto.imagen_alt || producto.descripcion || 'Producto',
          size: 'lg',
          class: 'rounded-lg'
        })
      }
      return h('div', {
        class: 'cursor-pointer hover:opacity-75 transition-opacity',
        title: 'Click para editar imagen',
        onClick: () => emit('edit-image', producto)
      }, [
        h(UAvatar, {
          src: producto.imagen_url || undefined,
          alt: producto.imagen_alt || producto.descripcion || 'Producto',
          size: 'lg',
          class: 'rounded-lg'
        })
      ])
    }
  },
  {
    accessorKey: 'codigo',
    header: createSortButton('codigo', 'Código'),
    cell: ({ row }) => {
      const producto = row.original
      return h('div', { class: 'font-mono text-sm font-medium text-gray-900 dark:text-gray-100' }, 
        producto.codigo?.toString() || '-')
    }
  },
  {
    accessorKey: 'descripcion',
    header: createSortButton('descripcion', 'Descripción'),
    cell: ({ row }) => {
      const producto = row.original
      const isSynced = isSyncField('descripcion')
      
      return h('div', { class: 'flex flex-col gap-1' }, [
        h('div', { class: 'flex items-center gap-2' }, [
          h('p', { class: 'font-medium text-gray-900 dark:text-gray-100 truncate max-w-xs' }, 
            producto.descripcion || 'Sin descripción'),
          isSynced ? h(UBadge, {
            color: 'orange',
            variant: 'soft',
            size: 'xs',
            title: 'Campo sincronizado con SIGMA'
          }, () => 'SYNC') : null
        ]),
        producto.descripcion_corta ? h('p', { class: 'text-sm text-gray-500 dark:text-gray-400 truncate max-w-xs' }, 
          producto.descripcion_corta) : null
      ])
    }
  },
  {
    accessorKey: 'codigo_rubro',
    header: createSortButton('codigorubro', 'Rubro'),
    cell: ({ row }) => {
      const producto = row.original
      const isSynced = isSyncField('codigo_rubro')
      
      return h('div', { class: 'flex items-center gap-2' }, [
        h('span', { class: 'text-sm text-gray-900 dark:text-gray-100' }, 
          producto.codigo_rubro?.toString() || '-'),
        isSynced ? h(UBadge, {
          color: 'orange',
          variant: 'soft',
          size: 'xs',
          title: 'Campo sincronizado con SIGMA'
        }, () => 'SYNC') : null
      ])
    }
  },
  {
    accessorKey: 'precio_seleccionado',
    header: createPriceHeader(),
    cell: ({ row }) => {
      const producto = row.original
      const isSynced = isSyncField('precio')
      
      return h('div', { class: 'flex items-center gap-2' }, [
        h('span', { class: 'font-medium text-gray-900 dark:text-gray-100' }, 
          producto.precio_seleccionado ? `$${producto.precio_seleccionado.toLocaleString('es-AR', { minimumFractionDigits: 2 })}` : '-'),
        isSynced ? h(UBadge, {
          color: 'orange',
          variant: 'soft',
          size: 'xs',
          title: 'Campo sincronizado con SIGMA'
        }, () => 'SYNC') : null
      ])
    }
  },
  {
    accessorKey: 'existencia',
    header: createSortButton('existencia', 'Stock'),
    cell: ({ row }) => {
      const producto = row.original
      const isSynced = isSyncField('existencia')
      
      return h('div', { class: 'flex items-center gap-2' }, [
        h('span', { 
          class: `font-medium ${getStockColor(producto.existencia)}` 
        }, producto.existencia?.toString() || '0'),
        isSynced ? h(UBadge, {
          color: 'orange',
          variant: 'soft',
          size: 'xs',
          title: 'Campo sincronizado con SIGMA'
        }, () => 'SYNC') : null
      ])
    }
  },
  {
    accessorKey: 'configuraciones_faltantes',
    header: 'Configuración',
    cell: ({ row }) => {
      const producto = row.original
      const configuracionesFaltantes = producto.configuraciones_faltantes || []
      const isCompleto = configuracionesFaltantes.length === 0
      
      return h(UTooltip, {
        text: isCompleto 
          ? 'Configuración completa' 
          : `Falta: ${configuracionesFaltantes.join(', ')}`
      }, () => h(UBadge, {
        color: isCompleto ? 'green' : 'orange',
        variant: 'soft',
        size: 'xs'
      }, () => isCompleto ? 'Completo' : `${configuracionesFaltantes.length} pendiente${configuracionesFaltantes.length !== 1 ? 's' : ''}`))
    }
  },
  {
    accessorKey: 'visible',
    header: createSortButton('visible', 'Visible'),
    cell: ({ row }) => {
      const producto = row.original
      return h(UBadge, {
        color: producto.visible ? 'green' : 'red',
        variant: 'soft',
        size: 'xs'
      }, () => producto.visible ? 'Visible' : 'Oculto')
    }
  },
  ...(!props.readOnly ? [{
    id: 'actions',
    header: 'Acciones',
    cell: ({ row }) => {
      const producto = row.original
      const buttons = []
      
      if (canEdit()) {
        buttons.push(h(UButton, {
          icon: 'i-heroicons-pencil',
          size: 'xs',
          color: 'primary',
          variant: 'soft',
          title: 'Editar producto',
          onClick: () => emit('edit', producto)
        }))
      }
      
      if (canDelete()) {
        buttons.push(h(UButton, {
          icon: 'i-heroicons-trash',
          size: 'xs',
          color: 'red',
          variant: 'soft',
          title: 'Eliminar producto',
          onClick: () => emit('delete', producto)
        }))
      }
      
      return h('div', { class: 'flex items-center gap-2' }, buttons)
    }
  }] : [])
]

// Permisos de acciones
const canEdit = () => {
  return userPermissions.value?.canManageProductosBase || false
}

const canDelete = () => {
  return userPermissions.value?.canManageProductosBase || false
}

// Helpers
const getStockColor = (stock: number | null | undefined) => {
  if (!stock || stock === 0) return 'text-red-600 dark:text-red-400'
  if (stock <= 10) return 'text-orange-600 dark:text-orange-400'
  return 'text-green-600 dark:text-green-400'
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