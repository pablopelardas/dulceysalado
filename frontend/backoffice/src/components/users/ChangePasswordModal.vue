<template>
  <UModal 
    v-model:open="isOpen" 
    variant="subtle"
    :ui="{ 
      title: 'Cambiar Contraseña',
      description: `Cambiar la contraseña de ${user?.nombre || ''} ${user?.apellido || ''}`
    }"
  >
    <template #header>
      <div class="flex items-center">
        <UIcon name="i-heroicons-key" class="h-6 w-6 text-blue-500 mr-2" />
        <h3 id="modal-title" class="text-lg font-semibold text-gray-900 dark:text-gray-100">Cambiar Contraseña</h3>
      </div>
    </template>

    <template #body>
 <div class="space-y-4" aria-describedby="modal-description">
        <p id="modal-description" class="text-gray-600 dark:text-gray-400 text-sm">
          <span v-if="mode === 'self'">Cambiar tu contraseña</span>
          <span v-else>Cambiar la contraseña de <strong>{{ user?.nombre }} {{ user?.apellido }}</strong></span>
        </p>

        <UForm
          ref="form"
          :schema="schema"
          :state="formData"
          @submit="onSubmit"
          @error="onError"
          class="space-y-4"
        >
          <!-- Campo de contraseña actual (solo para modo self) -->
          <UFormField v-if="mode === 'self'" label="Contraseña Actual" name="currentPassword" required>
            <UInput
              v-model="formData.currentPassword"
              type="password"
              placeholder="Tu contraseña actual"
              :disabled="loading"
              autocomplete="current-password"
            />
          </UFormField>

          <UFormField label="Nueva Contraseña" name="password" required>
            <UInput
              v-model="formData.password"
              type="password"
              placeholder="Nueva contraseña segura"
              :disabled="loading"
              autocomplete="new-password"
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
              placeholder="Repite la nueva contraseña"
              :disabled="loading"
              autocomplete="new-password"
            />
          </UFormField>

          <!-- Mostrar errores de validación -->
          <UAlert
            v-if="validationError"
            icon="i-heroicons-exclamation-triangle"
            color="red"
            variant="soft"
            :title="validationError"
            class="mt-4"
          />
        </UForm>
      </div>
    </template>
      <template #footer>
        <div class="flex justify-end space-x-3">
          <UButton
            variant="ghost"
            color="gray"
            :disabled="loading"
            @click="closeModal"
          >
            Cancelar
          </UButton>
          <UButton
            color="primary"
            :loading="loading"
            :disabled="!isFormValid"
            @click="handleSubmit"
          >
            <UIcon name="i-heroicons-check" class="mr-2" />
            Cambiar Contraseña
          </UButton>
        </div>
      </template>
  </UModal>
</template>

<script setup lang="ts">
import { z } from 'zod'
import type { Usuario } from '~/types/auth'

// Props
interface Props {
  modelValue: boolean
  user: Usuario | null
  mode?: 'admin' | 'self' // admin: admin cambiando contraseña de otro, self: usuario cambiando su propia contraseña
}

const props = withDefaults(defineProps<Props>(), {
  mode: 'admin'
})

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'success': []
}>()

// Composables
const { adminChangePassword, changePassword } = useUsers()
const toast = useToast()

// Estado reactivo
const form = ref()
const loading = ref(false)
const validationError = ref<string | null>(null)

// Datos del formulario
const formData = reactive({
  password: '',
  confirmPassword: '',
  currentPassword: ''
})

// Schemas de validación
const adminSchema = z.object({
  password: z
    .string()
    .min(8, 'La contraseña debe tener al menos 8 caracteres')
    .regex(/[A-Z]/, 'Debe contener al menos una mayúscula')
    .regex(/[a-z]/, 'Debe contener al menos una minúscula')
    .regex(/[0-9]/, 'Debe contener al menos un número'),
  confirmPassword: z.string().min(1, 'Debe confirmar la contraseña')
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Las contraseñas no coinciden',  
  path: ['confirmPassword']
})

const selfSchema = z.object({
  password: z
    .string()
    .min(8, 'La contraseña debe tener al menos 8 caracteres')
    .regex(/[A-Z]/, 'Debe contener al menos una mayúscula')
    .regex(/[a-z]/, 'Debe contener al menos una minúscula')
    .regex(/[0-9]/, 'Debe contener al menos un número'),
  confirmPassword: z.string().min(1, 'Debe confirmar la contraseña'),
  currentPassword: z.string().min(1, 'Debe ingresar su contraseña actual')
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Las contraseñas no coinciden',
  path: ['confirmPassword']
})

// Schema dinámico según el modo
const schema = computed(() => {
  return props.mode === 'self' ? selfSchema : adminSchema
})

// Computed
const isOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const isFormValid = computed(() => {
  const baseValid = formData.password.length >= 8 && 
                   formData.confirmPassword.length >= 8 &&
                   formData.password === formData.confirmPassword
  
  // Si es modo self, también validar que tenga contraseña actual
  if (props.mode === 'self') {
    return baseValid && formData.currentPassword.length > 0
  }
  
  return baseValid
})

// Métodos
const resetForm = () => {
  formData.password = ''
  formData.confirmPassword = ''
  formData.currentPassword = ''
  validationError.value = null
}

const closeModal = () => {
  resetForm()
  isOpen.value = false
}

const onSubmit = async () => {
  await handleSubmit()
}

const handleSubmit = async () => {
  if (!props.user) {
    validationError.value = 'No se ha seleccionado un usuario'
    return
  }

  // Validar formulario
  try {
    schema.value.parse(formData)
    validationError.value = null
  } catch (error: any) {
    if (error.errors && error.errors.length > 0) {
      validationError.value = error.errors[0].message
    }
    return
  }

  loading.value = true

  try {
    if (props.mode === 'self') {
      // Usuario cambiando su propia contraseña
      await changePassword(props.user.id, {
        CurrentPassword: formData.currentPassword,
        NewPassword: formData.password
      })
      
      toast.add({
        title: 'Contraseña actualizada',
        description: 'Tu contraseña ha sido actualizada exitosamente',
        color: 'success'
      })
    } else {
      // Admin cambiando contraseña de otro usuario
      await adminChangePassword(props.user.id, {
        password: formData.password,
        confirmPassword: formData.confirmPassword
      })

      toast.add({
        title: 'Contraseña actualizada',
        description: `La contraseña de ${props.user.nombre} ${props.user.apellido} ha sido actualizada exitosamente`,
        color: 'success'
      })
    }

    emit('success')
    closeModal()

  } catch (error: any) {
    validationError.value = error.message || 'Error al cambiar la contraseña'
    console.error('Error cambiando contraseña:', error)
  } finally {
    loading.value = false
  }
}

const onError = (event: any) => {
  console.error('Errores de formulario:', event)
  if (event.errors && event.errors.length > 0) {
    validationError.value = event.errors[0].message
  }
}

// Resetear formulario cuando se abre el modal
watch(() => props.modelValue, (isOpen) => {
  if (isOpen) {
    resetForm()
  }
})
</script>