<template>
  <UCard>
    <template #header>
      <div class="flex items-center justify-between">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Permisos del Usuario
        </h3>
        <UBadge :color="hasCustomPermissions ? 'warning' : 'neutral'" variant="subtle">
          {{ hasCustomPermissions ? 'Permisos Personalizados' : 'Permisos por Rol' }}
        </UBadge>
      </div>
      <p class="mt-1 text-sm text-gray-600 dark:text-gray-400">
        Selecciona los permisos específicos para este usuario
      </p>
    </template>

    <div class="space-y-6">
      <!-- Permisos de Productos -->
      <div class="border-b border-gray-200 dark:border-gray-700 pb-6">
        <h4 class="text-md font-medium text-gray-900 dark:text-gray-100 mb-4 flex items-center">
          <UIcon name="i-heroicons-cube" class="mr-2 text-blue-500" />
          Gestión de Productos
        </h4>
        
        <div class="grid grid-cols-1 gap-4">
          <!-- Productos Base (solo empresa principal) -->
          <div v-if="isMainCompany" class="flex items-start space-x-3">
            <UCheckbox
              :id="'perm-productos-base'"
              v-model="localPermissions.puede_gestionar_productos_base"
              :disabled="disabled"
              @change="updatePermissions"
            />
            <div class="flex-1">
              <label :for="'perm-productos-base'" class="text-sm font-medium text-gray-900 dark:text-gray-100 cursor-pointer">
                Gestionar Productos Base
              </label>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                Crear, editar y eliminar productos del catálogo base del sistema
              </p>
              <UBadge color="info" variant="subtle" size="xs" class="mt-1">
                Solo Empresa Principal
              </UBadge>
            </div>
          </div>
          
          <!-- Productos Empresa -->
          <div v-if="features.empresaProducts" class="flex items-start space-x-3">
            <UCheckbox
              :id="'perm-productos-empresa'"
              v-model="localPermissions.puede_gestionar_productos_empresa"
              :disabled="disabled"
              @change="updatePermissions"
            />
            <div class="flex-1">
              <label :for="'perm-productos-empresa'" class="text-sm font-medium text-gray-900 dark:text-gray-100 cursor-pointer">
                Gestionar Productos Propios
              </label>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                Crear, editar y eliminar productos específicos de la empresa
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Permisos de Categorías -->
      <div class="border-b border-gray-200 dark:border-gray-700 pb-6">
        <h4 class="text-md font-medium text-gray-900 dark:text-gray-100 mb-4 flex items-center">
          <UIcon name="i-heroicons-tag" class="mr-2 text-green-500" />
          Gestión de Categorías
        </h4>
        
        <div class="grid grid-cols-1 gap-4">
          <!-- Categorías Base (solo empresa principal) -->
          <div v-if="isMainCompany" class="flex items-start space-x-3">
            <UCheckbox
              :id="'perm-categorias-base'"
              v-model="localPermissions.puede_gestionar_categorias_base"
              :disabled="disabled"
              @change="updatePermissions"
            />
            <div class="flex-1">
              <label :for="'perm-categorias-base'" class="text-sm font-medium text-gray-900 dark:text-gray-100 cursor-pointer">
                Gestionar Categorías Base
              </label>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                Crear, editar y eliminar categorías del catálogo base
              </p>
              <UBadge color="info" variant="subtle" size="xs" class="mt-1">
                Solo Empresa Principal
              </UBadge>
            </div>
          </div>
          
          <!-- Categorías Empresa -->
          <div v-if="features.empresaCategories" class="flex items-start space-x-3">
            <UCheckbox
              :id="'perm-categorias-empresa'"
              v-model="localPermissions.puede_gestionar_categorias_empresa"
              :disabled="disabled"
              @change="updatePermissions"
            />
            <div class="flex-1">
              <label :for="'perm-categorias-empresa'" class="text-sm font-medium text-gray-900 dark:text-gray-100 cursor-pointer">
                Gestionar Categorías Propias
              </label>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                Crear, editar y eliminar categorías específicas de la empresa
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Permisos Administrativos -->
      <div>
        <h4 class="text-md font-medium text-gray-900 dark:text-gray-100 mb-4 flex items-center">
          <UIcon name="i-heroicons-cog-6-tooth" class="mr-2 text-orange-500" />
          Administración
        </h4>
        
        <div class="grid grid-cols-1 gap-4">
          <!-- Gestionar Usuarios -->
          <div class="flex items-start space-x-3">
            <UCheckbox
              :id="'perm-usuarios'"
              v-model="localPermissions.puede_gestionar_usuarios"
              :disabled="disabled"
              @change="updatePermissions"
            />
            <div class="flex-1">
              <label :for="'perm-usuarios'" class="text-sm font-medium text-gray-900 dark:text-gray-100 cursor-pointer">
                Gestionar Usuarios
              </label>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                Crear, editar y eliminar usuarios de la empresa
              </p>
            </div>
          </div>
          
          <!-- Ver Estadísticas -->
          <div class="flex items-start space-x-3">
            <UCheckbox
              :id="'perm-estadisticas'"
              v-model="localPermissions.puede_ver_estadisticas"
              :disabled="disabled"
              @change="updatePermissions"
            />
            <div class="flex-1">
              <label :for="'perm-estadisticas'" class="text-sm font-medium text-gray-900 dark:text-gray-100 cursor-pointer">
                Ver Estadísticas
              </label>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                Acceder a reportes y estadísticas del sistema
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Resumen de permisos -->
    <template #footer v-if="activePermissions.length > 0">
      <div class="bg-gray-50 dark:bg-gray-800/50 px-4 py-3 rounded-lg">
        <p class="text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">
          Permisos Activos ({{ activePermissions.length }})
        </p>
        <div class="flex flex-wrap gap-2">
          <UBadge
            v-for="permission in activePermissions"
            :key="permission"
            color="success"
            variant="subtle"
            size="sm"
          >
            {{ getPermissionLabel(permission) }}
          </UBadge>
        </div>
      </div>
    </template>
  </UCard>
</template>

<script setup lang="ts">
import type { UserFormData, PermissionSet } from '~/types/users'
import { USER_PERMISSIONS, ROLE_PERMISSIONS } from '~/types/users'

// Composables
const features = useFeatures()

// Props
interface Props {
  modelValue: UserFormData
  isMainCompany?: boolean
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isMainCompany: false,
  disabled: false
})

const emit = defineEmits<{
  'update:modelValue': [value: UserFormData]
}>()

// Estado local para permisos
const localPermissions = reactive<PermissionSet>({
  puede_gestionar_productos_base: props.modelValue.puede_gestionar_productos_base || false,
  puede_gestionar_productos_empresa: props.modelValue.puede_gestionar_productos_empresa || false,
  puede_gestionar_categorias_base: props.modelValue.puede_gestionar_categorias_base || false,
  puede_gestionar_categorias_empresa: props.modelValue.puede_gestionar_categorias_empresa || false,
  puede_gestionar_usuarios: props.modelValue.puede_gestionar_usuarios || false,
  puede_ver_estadisticas: props.modelValue.puede_ver_estadisticas || false
})

// Computed
const activePermissions = computed(() => {
  return Object.entries(localPermissions)
    .filter(([_, value]) => value)
    .map(([key]) => key)
})

const rolePermissions = computed(() => {
  const role = props.modelValue.rol
  if (!role) return {}
  
  let permissions = { ...ROLE_PERMISSIONS[role] }
  
  // Si no es empresa principal, remover permisos base
  if (!props.isMainCompany) {
    delete permissions.puede_gestionar_productos_base
    delete permissions.puede_gestionar_categorias_base
  }
  
  return permissions
})

const hasCustomPermissions = computed(() => {
  const defaultPerms = rolePermissions.value
  
  return Object.keys(localPermissions).some(key => {
    const permKey = key as keyof PermissionSet
    return localPermissions[permKey] !== (defaultPerms[permKey] || false)
  })
})

// Métodos
const getPermissionLabel = (permissionKey: string) => {
  const permission = USER_PERMISSIONS.find(p => p.key === permissionKey)
  return permission?.label || permissionKey
}

const updatePermissions = () => {
  // Emitir cambios al componente padre
  const updatedData = {
    ...props.modelValue,
    ...localPermissions
  }
  
  emit('update:modelValue', updatedData)
}

// Watcher para sincronizar cambios cuando el padre actualiza los permisos
watch(() => props.modelValue, (newValue) => {
  // Solo actualizar los permisos, no otros campos
  localPermissions.puede_gestionar_productos_base = newValue.puede_gestionar_productos_base || false
  localPermissions.puede_gestionar_productos_empresa = newValue.puede_gestionar_productos_empresa || false
  localPermissions.puede_gestionar_categorias_base = newValue.puede_gestionar_categorias_base || false
  localPermissions.puede_gestionar_categorias_empresa = newValue.puede_gestionar_categorias_empresa || false
  localPermissions.puede_gestionar_usuarios = newValue.puede_gestionar_usuarios || false
  localPermissions.puede_ver_estadisticas = newValue.puede_ver_estadisticas || false
}, { deep: true })
</script>