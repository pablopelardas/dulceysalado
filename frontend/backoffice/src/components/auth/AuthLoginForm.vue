<template>
  <UCard class="max-w-md mx-auto p-6 bg-white dark:bg-gray-800 shadow-md rounded-lg border border-gray-200 dark:border-gray-700">
    <template #header>
      <h3 class="text-lg font-medium text-gray-900 dark:text-gray-100">Iniciar Sesión</h3>
    </template>

    <!-- Cuentas guardadas - Solo en desarrollo -->
    <div v-if="isDevelopment && savedAccounts.length > 0" class="mb-4">
      <UFormField label="Cuentas Guardadas" hint="Solo en desarrollo">
        <USelectMenu
          v-model="selectedAccount"
          :items="savedAccountOptions"
          placeholder="Selecciona una cuenta guardada"
          icon="i-heroicons-user-circle"
          size="lg"
          @update:modelValue="onSelectAccount"
        />
      </UFormField>
    </div>

    <UForm
      :schema="loginSchema"
      :state="formState"
      @submit="onSubmit"
      class="space-y-4 flex flex-col gap-0.5"
    >
      <UFormField label="Email o Usuario" name="email" required>
        <UInput
          v-model="formState.email"
          type="text"
          placeholder="email@empresa.com o tu_usuario"
          icon="i-heroicons-user"
          size="lg"
          class="w-full"
          :disabled="isLoading"
        />
      </UFormField>

      <UFormField label="Contraseña" name="password" required>
        <UInput
          v-model="formState.password"
          type="password"
          placeholder="••••••••"
          icon="i-heroicons-lock-closed"
          size="lg"
          class="w-full"
          :disabled="isLoading"
        />
      </UFormField>

      <UButton 
        type="submit" 
        :loading="isLoading"
        :disabled="isLoading"
        size="lg"
        class="mx-auto"
      >
        {{ isLoading ? 'Iniciando sesión...' : 'Iniciar Sesión' }}
      </UButton>
    </UForm>

    <UAlert
      v-if="error"
      icon="i-heroicons-exclamation-triangle"
      color="error"
      variant="subtle"
      :title="error"
      class="mt-4"
    />
  </UCard>
</template>

<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '#ui/types'
import { UFormField } from '#components'

interface Props {
  isLoading?: boolean
  error?: string | null
}

interface Emits {
  submit: [credentials: { email: string; password: string; shouldSave?: boolean }]
}

const props = withDefaults(defineProps<Props>(), {
  isLoading: false,
  error: null
})

const emit = defineEmits<Emits>()

// Schema de validación
const loginSchema = z.object({
  email: z.string().min(1, 'Email o usuario es requerido'),
  password: z.string().min(1, 'La contraseña es requerida')
})

type LoginSchema = z.output<typeof loginSchema>

// Interfaces para cuentas guardadas
interface SavedAccount {
  email: string
  password: string
  lastUsed: string
  displayName?: string
}

// Variables de desarrollo
const isDevelopment = process.env.NODE_ENV === 'development'
const STORAGE_KEY = 'dev-login-accounts'

// Estado del formulario
const formState = reactive({
  email: '',
  password: ''
})

// Estado para cuentas guardadas
const savedAccounts = ref<SavedAccount[]>([])
const selectedAccount = ref<{ label: string; value: string; description: string } | undefined>(undefined)

// Computed para opciones del select
const savedAccountOptions = computed(() => {
  return savedAccounts.value.map(account => ({
    label: account.displayName || account.email,
    value: account.email,
    description: `Último uso: ${new Date(account.lastUsed).toLocaleDateString()}`
  }))
})

// Cargar cuentas guardadas al montar el componente
const loadSavedAccounts = () => {
  if (!isDevelopment || typeof window === 'undefined') return
  
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored) {
      savedAccounts.value = JSON.parse(stored)
    }
  } catch (error) {
    console.error('Error loading saved accounts:', error)
    savedAccounts.value = []
  }
}

// Guardar cuenta en localStorage
const saveAccount = (email: string, password: string) => {
  if (!isDevelopment || typeof window === 'undefined') return
  
  try {
    const existingIndex = savedAccounts.value.findIndex(acc => acc.email === email)
    const accountData: SavedAccount = {
      email,
      password,
      lastUsed: new Date().toISOString(),
      displayName: email.split('@')[0] // Usar parte antes del @ como nombre
    }
    
    if (existingIndex >= 0) {
      // Actualizar cuenta existente
      savedAccounts.value[existingIndex] = accountData
    } else {
      // Agregar nueva cuenta
      savedAccounts.value.push(accountData)
    }
    
    // Mantener solo las últimas 5 cuentas
    savedAccounts.value = savedAccounts.value
      .sort((a, b) => new Date(b.lastUsed).getTime() - new Date(a.lastUsed).getTime())
      .slice(0, 5)
    
    localStorage.setItem(STORAGE_KEY, JSON.stringify(savedAccounts.value))
  } catch (error) {
    console.error('Error saving account:', error)
  }
}

// Seleccionar cuenta guardada
const onSelectAccount = (value: { label: string; value: string; description: string } | undefined) => {
  if (!value) return
  const email = value.value
  const account = savedAccounts.value.find(acc => acc.email === email)
  if (account) {
    formState.email = account.email
    formState.password = account.password
    selectedAccount.value = value
  }
}

// Manejar envío del formulario
const onSubmit = (event: FormSubmitEvent<LoginSchema>) => {
  // Guardar credenciales si el login es exitoso (se maneja en el componente padre)
  emit('submit', { 
    ...event.data, 
    shouldSave: isDevelopment // Flag para indicar si se debe guardar
  })
}

// Exponer función para guardar desde el componente padre
defineExpose({
  saveCredentials: (email: string, password: string) => {
    if (isDevelopment) {
      saveAccount(email, password)
    }
  }
})

// Resetear formulario cuando hay error
watch(() => props.error, (newError) => {
  if (newError) {
    formState.password = ''
    selectedAccount.value = undefined
  }
})

// Cargar cuentas al montar
onMounted(() => {
  loadSavedAccounts()
})
</script>