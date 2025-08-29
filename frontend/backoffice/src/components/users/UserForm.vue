<template>
  <UForm 
    ref="form"
    :schema="schema" 
    :state="formData"
    @submit="onSubmit"
    @error="onError"
    class="space-y-6"
  >
    <!-- Información básica -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Información Personal
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Nombre" name="nombre" required>
          <UInput 
            v-model="formData.nombre"
            placeholder="Ingresa el nombre"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Apellido" name="apellido" required>
          <UInput 
            v-model="formData.apellido"
            placeholder="Ingresa el apellido"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Email" name="email" required class="md:col-span-2">
          <UInput 
            v-model="formData.email"
            type="email"
            placeholder="usuario@ejemplo.com"
            :disabled="loading"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Contraseña (solo para crear) -->
    <UCard v-if="mode === 'create'">
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Credenciales de Acceso
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Contraseña" name="password" required>
          <UInput 
            v-model="formData.password"
            type="password"
            placeholder="Contraseña segura"
            :disabled="loading"
          />
          <template #help>
            <span class="text-sm text-gray-500 dark:text-gray-400">
              Mínimo 8 caracteres, incluir mayúsculas, minúsculas y números
            </span>
          </template>
        </UFormField>
        
        <UFormField label="Confirmar Contraseña" name="confirmPassword" required>
          <UInput 
            v-model="formData.confirmPassword"
            type="password"
            placeholder="Repite la contraseña"
            :disabled="loading"
          />
        </UFormField>
      </div>
    </UCard>


    <!-- Rol -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Rol del Usuario
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Rol" name="rol" required>
          <USelect
            v-model="formData.rol"
            :items="roleOptions"
            option-attribute="label"
            value-attribute="value"
            placeholder="Selecciona un rol"
            :disabled="loading"
            @update:modelValue="onRoleChange"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Permisos -->
    <UsersPermissions
      v-model="formData"
      :is-main-company="isMainCompany"
      :disabled="loading"
      @update:modelValue="updateFormData"
    />

    <!-- Botones de acción -->
    <div class="flex justify-end space-x-3 pt-6">
      <UButton
        type="button"
        color="neutral"
        variant="soft"
        :disabled="loading"
        @click="$emit('cancel')"
      >
        Cancelar
      </UButton>
      
      <UButton
        type="submit"
        :loading="loading"
        :disabled="!isFormValid"
      >
        <UIcon :name="mode === 'create' ? 'i-heroicons-plus' : 'i-heroicons-check'" class="mr-2" />
        {{ mode === 'create' ? 'Crear Usuario' : 'Actualizar Usuario' }}
      </UButton>
    </div>
  </UForm>
</template>

<script setup lang="ts">
import type { UserFormData } from '~/types/users'
import { ROLE_PERMISSIONS } from '~/types/users'

// Props y emits
interface Props {
  mode: 'create' | 'edit'
  initialData?: Partial<UserFormData>
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  mode: 'create',
  initialData: () => ({}),
  loading: false
})

const emit = defineEmits<{
  submit: [data: UserFormData]
  cancel: []
}>()

// Composables
const { user: currentUser } = useAuth()
const { validateCreateUser, validateUpdateUser, createUserSchema, updateUserSchema } = useUserValidation()
const api = useApi()

// Estado reactivo
const form = ref()
const formData = reactive<UserFormData>({
  nombre: '',
  apellido: '',
  email: '',
  password: '',
  confirmPassword: '',
  rol: 'viewer',
  activo: true,
  puede_gestionar_productos_base: false,
  puede_gestionar_categorias_base: false,
  puede_gestionar_usuarios: false,
  puede_ver_estadisticas: false,
  ...props.initialData
})

// Computed

const schema = computed(() => {
  return props.mode === 'create' ? createUserSchema : updateUserSchema
})

const roleOptions = [
  { label: 'Administrador', value: 'admin' },
  { label: 'Editor', value: 'editor' },
  { label: 'Visualizador', value: 'viewer' }
]

const isFormValid = computed(() => {
  if (props.mode === 'create') {
    return formData.nombre && 
           formData.apellido && 
           formData.email && 
           formData.password && 
           formData.confirmPassword &&
           formData.rol
  }
  return formData.nombre && formData.apellido && formData.email && formData.rol
})

// Métodos
const onRoleChange = (role: string) => {
  // Aplicar permisos predeterminados según el rol
  const defaultPermissions = ROLE_PERMISSIONS[role] || {}
  
  // Usar updateFormData para aplicar permisos correctamente
  const updatedData = {
    ...formData,
    ...defaultPermissions
  }
  updateFormData(updatedData)
}

const onSubmit = async () => {
  try {
    const validation = props.mode === 'create' 
      ? validateCreateUser(formData)
      : validateUpdateUser(formData)
    
    if (!validation.success) {
      console.error('Errores de validación:', validation.errors)
      return
    }

    emit('submit', formData)
  } catch (error) {
    console.error('Error en submit:', error)
  }
}

const onError = (event: any) => {
  console.error('Errores de formulario:', event)
}

// Método para actualizar formData manualmente
const updateFormData = (updatedData: UserFormData) => {
  // Actualizar solo los permisos
  formData.puede_gestionar_productos_base = updatedData.puede_gestionar_productos_base
  formData.puede_gestionar_categorias_base = updatedData.puede_gestionar_categorias_base
  formData.puede_gestionar_usuarios = updatedData.puede_gestionar_usuarios
  formData.puede_ver_estadisticas = updatedData.puede_ver_estadisticas
}


// Variable para controlar si es la primera vez que se aplican permisos
const permissionsInitialized = ref(false)

// Watchers para aplicar permisos iniciales solo una vez
watchEffect(() => {
  if (props.mode === 'create' && formData.rol && !permissionsInitialized.value) {
    onRoleChange(formData.rol)
    permissionsInitialized.value = true
  }
})

</script>