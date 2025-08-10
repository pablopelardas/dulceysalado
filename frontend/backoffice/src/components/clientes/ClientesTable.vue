<template>
  <UTable
    v-model:sorting="sorting"
    :data="clientes"
    :columns="columns"
    :loading="loading"
    class="flex-1"
  />
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { ClienteDto } from '~/types/clientes'

interface Props {
  clientes: ClienteDto[] | readonly ClienteDto[]
  loading?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

interface Emits {
  edit: [cliente: ClienteDto]
  delete: [cliente: ClienteDto]
  sort: [column: string, order: 'asc' | 'desc']
  'manage-credentials': [cliente: ClienteDto]
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Estado del ordenamiento - inicializar desde props
const sorting = ref([
  {
    id: props.sortBy || 'nombre',
    desc: props.sortOrder === 'desc'
  }
])

// Estado independiente para tracking del ordenamiento actual
const currentSort = ref({
  column: props.sortBy || 'nombre',
  direction: props.sortOrder || 'asc'
})

// Función helper para crear el botón de ordenamiento
const createSortButton = (columnId: string, label: string) => {
  return () => {
    // Determinar si esta columna está ordenada
    const isCurrentlySorted = currentSort.value.column === columnId
    const currentDirection = isCurrentlySorted ? currentSort.value.direction : null
    
    return h('button', {
      class: 'flex items-center space-x-1 font-semibold text-gray-900 dark:text-gray-100 hover:text-blue-600 dark:hover:text-blue-400 transition-colors',
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
    }, [
      h('span', label),
      h('svg', {
        class: 'w-4 h-4',
        fill: 'none',
        stroke: 'currentColor',
        viewBox: '0 0 24 24'
      }, [
        h('path', {
          'stroke-linecap': 'round',
          'stroke-linejoin': 'round',
          'stroke-width': '2',
          d: isCurrentlySorted
            ? currentDirection === 'asc'
              ? 'M5 15l7-7 7 7'
              : 'M19 9l-7 7-7-7'
            : 'M8 9l4-4 4 4m0 6l-4 4-4-4'
        })
      ])
    ])
  }
}

// Columnas de la tabla
const columns: TableColumn<ClienteDto>[] = [
  {
    accessorKey: 'codigo',
    header: createSortButton('codigo', 'Código'),
    cell: ({ row }) => {
      const cliente = row.original
      return h('div', { class: 'font-mono text-sm font-medium text-gray-900 dark:text-gray-100' }, 
        cliente.codigo || '-')
    }
  },
  {
    accessorKey: 'nombre',
    header: createSortButton('nombre', 'Nombre'),
    cell: ({ row }) => {
      const cliente = row.original
      return h('div', { class: 'flex flex-col gap-1' }, [
        h('p', { class: 'font-medium text-gray-900 dark:text-gray-100' }, 
          cliente.nombre),
        cliente.localidad ? h('p', { class: 'text-sm text-gray-500 dark:text-gray-400' }, 
          `${cliente.localidad}${cliente.provincia ? `, ${cliente.provincia}` : ''}`) : null
      ])
    }
  },
  {
    accessorKey: 'email',
    header: createSortButton('email', 'Email'),
    cell: ({ row }) => {
      const cliente = row.original
      return h('div', { class: 'text-gray-900 dark:text-gray-100' }, cliente.email)
    }
  },
  {
    accessorKey: 'telefono',
    header: 'Teléfono',
    cell: ({ row }) => {
      const cliente = row.original
      return h('div', { class: 'text-gray-600 dark:text-gray-400' }, 
        cliente.telefono || '-')
    }
  },
  {
    accessorKey: 'lista_precio',
    header: 'Lista de Precios',
    cell: ({ row }) => {
      const cliente = row.original
      if (!cliente.lista_precio) {
        return h('span', {
          class: 'inline-flex items-center rounded-md bg-gray-50 px-2 py-1 text-xs font-medium text-gray-600 ring-1 ring-inset ring-gray-500/10 dark:bg-gray-400/10 dark:text-gray-400 dark:ring-gray-400/20'
        }, 'Sin lista')
      }
      return h('span', {
        class: 'inline-flex items-center rounded-md bg-blue-50 px-2 py-1 text-xs font-medium text-blue-700 ring-1 ring-inset ring-blue-700/10 dark:bg-blue-400/10 dark:text-blue-400 dark:ring-blue-400/30'
      }, cliente.lista_precio.nombre)
    }
  },
  {
    accessorKey: 'tiene_acceso',
    header: 'Credenciales',
    cell: ({ row }) => {
      const cliente = row.original
      const hasCredentials = cliente.tiene_acceso
      
      if (!hasCredentials) {
        return h('span', {
          class: 'inline-flex items-center rounded-md bg-gray-50 px-2 py-1 text-xs font-medium text-gray-600 ring-1 ring-inset ring-gray-500/10 dark:bg-gray-400/10 dark:text-gray-400 dark:ring-gray-400/20'
        }, 'Sin credenciales')
      }
      
      const isActive = cliente.activo
      return h('div', { class: 'flex flex-col gap-1' }, [
        h('span', {
          class: 'inline-flex items-center rounded-md bg-green-50 px-2 py-1 text-xs font-medium text-green-700 ring-1 ring-inset ring-green-600/20 dark:bg-green-400/10 dark:text-green-400 dark:ring-green-400/20'
        }, `@${cliente.username}`),
        isActive ? h('span', {
          class: 'inline-flex items-center rounded-md bg-green-50 px-2 py-1 text-xs font-medium text-green-700 ring-1 ring-inset ring-green-600/20 dark:bg-green-400/10 dark:text-green-400 dark:ring-green-400/20'
        }, 'ACTIVO') : h('span', {
          class: 'inline-flex items-center rounded-md bg-red-50 px-2 py-1 text-xs font-medium text-red-700 ring-1 ring-inset ring-red-600/10 dark:bg-red-400/10 dark:text-red-400 dark:ring-red-400/20'
        }, 'INACTIVO')
      ])
    }
  },
  {
    accessorKey: 'last_login',
    header: createSortButton('last_login', 'Último Login'),
    cell: ({ row }) => {
      const cliente = row.original
      if (!cliente.last_login) {
        return h('div', { class: 'text-gray-500 dark:text-gray-400' }, 'Nunca')
      }
      const fecha = new Date(cliente.last_login).toLocaleDateString('es-ES')
      return h('div', { class: 'text-gray-600 dark:text-gray-400 text-sm' }, fecha)
    }
  },
  {
    accessorKey: 'actions',
    header: 'Acciones',
    cell: ({ row }) => {
      const cliente = row.original
      
      return h('div', { class: 'flex items-center space-x-2' }, [
        // Botón Ver
        h('button', {
          class: 'inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 dark:hover:bg-gray-800 dark:hover:text-gray-300',
          title: 'Ver detalles',
          onClick: () => navigateTo(`/clientes/${cliente.id}`)
        }, [
          h('svg', {
            class: 'h-4 w-4',
            fill: 'none',
            stroke: 'currentColor',
            viewBox: '0 0 24 24'
          }, [
            h('path', {
              'stroke-linecap': 'round',
              'stroke-linejoin': 'round',
              'stroke-width': '2',
              d: 'M15 12a3 3 0 11-6 0 3 3 0 016 0z'
            }),
            h('path', {
              'stroke-linecap': 'round',
              'stroke-linejoin': 'round',
              'stroke-width': '2',
              d: 'M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z'
            })
          ])
        ]),
        
        // Botón Editar
        h('button', {
          class: 'inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 dark:hover:bg-gray-800 dark:hover:text-gray-300',
          title: 'Editar cliente',
          onClick: () => emit('edit', cliente)
        }, [
          h('svg', {
            class: 'h-4 w-4',
            fill: 'none',
            stroke: 'currentColor',
            viewBox: '0 0 24 24'
          }, [
            h('path', {
              'stroke-linecap': 'round',
              'stroke-linejoin': 'round',
              'stroke-width': '2',
              d: 'M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z'
            })
          ])
        ]),
        
        // Botón Credenciales - Solo mostrar si el cliente está activo
        ...(cliente.activo ? [h('button', {
          class: 'inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 dark:hover:bg-gray-800 dark:hover:text-gray-300',
          title: cliente.tiene_acceso ? 'Gestionar credenciales' : 'Crear credenciales',
          onClick: () => emit('manage-credentials', cliente)
        }, [
          h('svg', {
            class: 'h-4 w-4',
            fill: 'none',
            stroke: 'currentColor',
            viewBox: '0 0 24 24'
          }, [
            h('path', {
              'stroke-linecap': 'round',
              'stroke-linejoin': 'round',
              'stroke-width': '2',
              d: cliente.tiene_acceso ? 'M15 7a2 2 0 012 2m0 0a2 2 0 012 2 2 2 0 002-2m-2-2h10m-9 2a2 2 0 00-2 2v6a2 2 0 002 2h6a2 2 0 002-2v-6a2 2 0 00-2-2m-2-2V7a2 2 0 00-2-2H9a2 2 0 00-2 2v2m4 0V7a1 1 0 011-1h2a1 1 0 011 1v2M9 7h6' : 'M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z'
            })
          ])
        ])] : []),
        
        // Botón Eliminar
        h('button', {
          class: 'inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:bg-red-100 hover:text-red-500 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 dark:hover:bg-red-900/20 dark:hover:text-red-400',
          title: 'Eliminar cliente',
          onClick: () => emit('delete', cliente)
        }, [
          h('svg', {
            class: 'h-4 w-4',
            fill: 'none',
            stroke: 'currentColor',
            viewBox: '0 0 24 24'
          }, [
            h('path', {
              'stroke-linecap': 'round',
              'stroke-linejoin': 'round',
              'stroke-width': '2',
              d: 'M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16'
            })
          ])
        ])
      ])
    }
  }
]

// Watcher para sincronizar sorting cuando cambien las props
watch(() => [props.sortBy, props.sortOrder], ([newSortBy, newSortOrder]) => {
  if (newSortBy && newSortOrder) {
    currentSort.value = {
      column: newSortBy,
      direction: newSortOrder
    }
    sorting.value = [{ id: newSortBy, desc: newSortOrder === 'desc' }]
  }
}, { immediate: true })
</script>