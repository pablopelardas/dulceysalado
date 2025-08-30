<template>
  <UTable
    :data="users"
    :columns="columns"
    :loading="loading"
  />
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { Usuario } from '~/types/auth'
import type { TableColumn } from '@nuxt/ui'
import { UBadge, UButton } from '#components'

interface Props {
  users: Usuario[]
  loading?: boolean
}

interface Emits {
  edit: [user: Usuario]
  delete: [user: Usuario]
  'change-password': [user: Usuario]
}

defineProps<Props>()
const emit = defineEmits<Emits>()

// Composables
const { user: currentUser } = useAuth()

// Columnas de la tabla usando la estructura correcta de Nuxt UI
const columns = computed<TableColumn<Usuario>[]>(() => {
  const baseColumns: TableColumn<Usuario>[] = [
    {
      accessorKey: 'nombre',
      header: 'Nombre',
      cell: ({ row }) => {
        const user = row.original
        return h('div', { class: 'flex items-center gap-2' }, [
          h('div', {}, [
            h('p', { class: 'font-medium text-gray-900 dark:text-gray-100' }, `${user.nombre} ${user.apellido}`),
            h('p', { class: 'text-sm text-gray-500 dark:text-gray-400' }, user.email)
          ])
        ])
      }
    },
    {
      accessorKey: 'rol',
      header: 'Rol',
      cell: ({ row }) => {
        const user = row.original
        return h(UBadge, {
          color: getRoleColor(user.rol),
          variant: 'outline',
          size: 'sm'
        }, () => getRoleLabel(user.rol))
      }
    },
    {
      accessorKey: 'activo',
      header: 'Estado',
      cell: ({ row }) => {
        const user = row.original
        return h(UBadge, {
          color: user.activo ? 'success' : 'error',
          variant: 'soft',
          size: 'sm'
        }, () => user.activo ? 'Activo' : 'Inactivo')
      }
    }
  ]

  return [
    ...baseColumns,
    {
      accessorKey: 'ultimo_login',
      header: 'Último Login',
      cell: ({ row }) => {
        const user = row.original
        return h('div', { class: 'text-sm' }, [
          h('p', { class: 'text-gray-900 dark:text-gray-100' }, formatDate(user.ultimo_login)),
          h('p', { class: 'text-gray-500 dark:text-gray-400' }, formatTime(user.ultimo_login))
        ])
      }
    },
    {
      accessorKey: 'permisos',
      header: 'Permisos',
      cell: ({ row }) => {
        const user = row.original
        const badges = []
        
        if (user.puede_gestionar_productos_base) {
          badges.push(h(UBadge, {
            color: 'primary',
            variant: 'soft',
            size: 'md',
            title: 'Puede gestionar productos base'
          }, () => 'PB'))
        }
        
        if (user.puede_gestionar_productos_empresa) {
          badges.push(h(UBadge, {
            color: 'success',
            variant: 'soft',
            size: 'md',
            title: 'Puede gestionar productos empresa'
          }, () => 'PE'))
        }
        
        if (user.puede_gestionar_categorias_base) {
          badges.push(h(UBadge, {
            color: 'secondary',
            variant: 'soft',
            size: 'md',
            title: 'Puede gestionar categorías base'
          }, () => 'CB'))
        }
        
        if (user.puede_gestionar_categorias_empresa) {
          badges.push(h(UBadge, {
            color: 'warning',
            variant: 'soft',
            size: 'md',
            title: 'Puede gestionar categorías empresa'
          }, () => 'CE'))
        }
        
        if (user.puede_gestionar_usuarios) {
          badges.push(h(UBadge, {
            color: 'error',
            variant: 'soft',
            size: 'md',
            title: 'Puede gestionar usuarios'
          }, () => 'U'))
        }
        
        if (user.puede_ver_estadisticas) {
          badges.push(h(UBadge, {
            color: 'info',
            variant: 'soft',
            size: 'md',
            title: 'Puede ver estadísticas'
          }, () => 'E'))
        }
        
        return h('div', { class: 'flex flex-wrap gap-1' }, badges)
      }
    },
    {
      id: 'actions',
      header: 'Acciones',
      enableHiding: false,
      cell: ({ row }) => {
        const user = row.original
        const buttons = []
        
        if (canEdit(user)) {
          buttons.push(h(UButton, {
            icon: 'i-heroicons-pencil',
            size: 'md',
            class: 'cursor-pointer',
            color: 'primary',
            variant: 'soft',
            title: 'Editar usuario',
            onClick: () => emit('edit', user)
          }))
          
          buttons.push(h(UButton, {
            icon: 'i-heroicons-key',
            size: 'md',
            class: 'cursor-pointer',
            color: 'warning',
            variant: 'soft',
            title: 'Cambiar contraseña',
            onClick: () => emit('change-password', user)
          }))
        }
        
        if (canDelete(user)) {
          buttons.push(h(UButton, {
            icon: 'i-heroicons-trash',
            size: 'md',
            class: 'cursor-pointer',
            color: 'error',
            variant: 'soft',
            title: 'Desactivar usuario',
            onClick: () => emit('delete', user)
          }))
        }
        
        return h('div', { class: 'flex items-center gap-2' }, buttons)
      }
    }
  ]
})

// Helpers para formatear datos
const getRoleColor = (rol: string) => {
  switch (rol) {
    case 'admin': return 'error'
    case 'editor': return 'primary'
    case 'viewer': return 'secondary'
    default: return 'secondary'
  }
}

const getRoleLabel = (rol: string) => {
  switch (rol) {
    case 'admin': return 'Administrador'
    case 'editor': return 'Editor'
    case 'viewer': return 'Viewer'
    default: return rol
  }
}

const formatDate = (dateString: string | null | undefined) => {
  if (!dateString) return 'Nunca'
  const date = new Date(dateString)
  return date.toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

const formatTime = (dateString: string | null | undefined) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleTimeString('es-ES', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Permisos de acciones
const canEdit = (user: Usuario) => {
  // No puede editar su propio usuario
  if (user.id === currentUser.value?.id) {
    return false
  }
  
  // En sistema single-tenant, puede editar cualquier usuario (excepto el propio)
  return true
}

const canDelete = (user: Usuario) => {
  // No puede eliminar su propio usuario
  if (user.id === currentUser.value?.id) {
    return false
  }
  
  return canEdit(user)
}
</script>