<template>
  <UTable
    :data="empresas"
    :columns="columns"
    :loading="loading"
  />
</template>

<script setup lang="ts">
import { h, computed } from 'vue'
import { UBadge, UButton } from '#components'
import type { Empresa } from '~/types/auth'

interface Props {
  empresas: Empresa[]
  loading?: boolean
}

interface Emits {
  edit: [empresa: Empresa]
  delete: [empresa: Empresa]
}

defineProps<Props>()
const emit = defineEmits<Emits>()

const columns = computed(() => [
  {
    accessorKey: 'nombre',
    header: 'Información',
    cell: ({ row }: any) => {
      const empresa = row.original
      return h('div', { class: 'flex flex-col' }, [
        h('div', { class: 'flex items-center gap-2' }, [
          h('span', { class: 'text-xs text-gray-500 dark:text-gray-400 font-mono' }, empresa.codigo),
          h('span', { class: 'font-medium text-gray-900 dark:text-gray-100' }, empresa.nombre)
        ]),
        empresa.razon_social ? h('span', { class: 'text-sm text-gray-600 dark:text-gray-400' }, empresa.razon_social) : null
      ].filter(Boolean))
    }
  },
  {
    accessorKey: 'activa',
    header: 'Estado',
    cell: ({ row }: any) => {
      const empresa = row.original
      return h(UBadge, {
        color: empresa.activa ? 'success' : 'error',
        variant: 'soft',
        size: 'sm'
      }, () => empresa.activa ? 'Activa' : 'Inactiva')
    }
  },
  {
    accessorKey: 'fecha_vencimiento',
    header: 'Vencimiento',
    cell: ({ row }: any) => {
      const empresa = row.original
      if (!empresa.fecha_vencimiento) {
        return h('span', { class: 'text-gray-400 dark:text-gray-500' }, 'Sin vencimiento')
      }
      
      const date = new Date(empresa.fecha_vencimiento)
      const today = new Date()
      const isExpired = date < today
      const isNearExpiration = date <= new Date(today.getTime() + 30 * 24 * 60 * 60 * 1000) && date > today
      
      return h('div', { class: 'flex flex-col' }, [
        h('span', { 
          class: `text-sm ${isExpired ? 'text-red-600 dark:text-red-400' : isNearExpiration ? 'text-orange-600 dark:text-orange-400' : 'text-gray-900 dark:text-gray-100'}`
        }, formatDate(empresa.fecha_vencimiento)),
        isExpired ? h('span', { class: 'text-xs text-red-500' }, 'Vencida') :
        isNearExpiration ? h('span', { class: 'text-xs text-orange-500' }, 'Próxima a vencer') : null
      ].filter(Boolean))
    }
  },
  {
    accessorKey: 'permisos',
    header: 'Permisos',
    cell: ({ row }: any) => {
      const empresa = row.original
      const badges = []
      
      if (empresa.puede_agregar_productos) {
        badges.push(h(UBadge, {
          color: 'blue',
          variant: 'soft',
          size: 'sm',
          title: 'Puede agregar productos'
        }, () => 'P'))
      }
      
      if (empresa.puede_agregar_categorias) {
        badges.push(h(UBadge, {
          color: 'green',
          variant: 'soft',
          size: 'sm',
          title: 'Puede agregar categorías'
        }, () => 'C'))
      }
      
      return h('div', { class: 'flex gap-1' }, badges)
    }
  },
  {
    id: 'acciones',
    header: 'Acciones',
    enableHiding: false,
    cell: ({ row }: any) => {
      const empresa = row.original
      return h('div', { class: 'flex items-center gap-2' }, [
        h(UButton, {
          icon: 'i-heroicons-pencil',
          size: 'sm',
          class: 'cursor-pointer',
          color: 'primary',
          variant: 'soft',
          title: 'Editar empresa',
          onClick: () => emit('edit', empresa)
        }),
        h(UButton, {
          icon: 'i-heroicons-trash',
          size: 'sm',
          class: 'cursor-pointer',
          color: 'error',
          variant: 'soft',
          title: 'Desactivar empresa',
          onClick: () => emit('delete', empresa)
        })
      ])
    }
  }
])

// Función para formatear fechas
function formatDate(dateString: string) {
  const date = new Date(dateString)
  return date.toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}
</script>
