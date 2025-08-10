<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 py-12 px-4 sm:px-6 lg:px-8 transition-colors duration-200">
    <div class="max-w-md w-full space-y-8">
      <div>
        <div class="mx-auto h-12 w-12 flex items-center justify-center rounded-full bg-blue-100 dark:bg-blue-900/20">
          <UIcon name="i-heroicons-building-office" class="h-6 w-6 text-blue-600 dark:text-blue-400" />
        </div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900 dark:text-gray-100">
          DistriCatalogo Admin
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600 dark:text-gray-400">
          Ingresa a tu cuenta para administrar tu catálogo
        </p>
      </div>

      <AuthLoginForm
        ref="loginFormRef"
        :is-loading="isLoading"
        :error="error"
        @submit="onSubmit"
        class="mt-8"
      />

      <div class="text-center">
        <p class="text-sm text-gray-600 dark:text-gray-400">
          ¿Problemas para acceder? Contacta al administrador
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: false,
  middleware: ['guest']
})

// Composables
const { login, isLoading, error } = useAuth()
const router = useRouter()

// Referencia al componente del formulario
const loginFormRef = ref()

// Manejar envío del formulario
const onSubmit = async (credentials: { email: string; password: string; shouldSave?: boolean }) => {
  try {
    await login({ email: credentials.email, password: credentials.password })
    
    // Guardar credenciales en localStorage si es desarrollo y login exitoso
    if (credentials.shouldSave && loginFormRef.value?.saveCredentials) {
      loginFormRef.value.saveCredentials(credentials.email, credentials.password)
    }
    
    // Redireccionar al dashboard después del login exitoso
    await router.push('/')
    
  } catch (err: any) {
    // El error ya se maneja en el store
    console.error('Error en login:', err)
  }
}

// Redireccionar si ya está autenticado
const { isAuthenticated } = useAuth()

watch(isAuthenticated, (authenticated) => {
  if (authenticated) {
    router.push('/')
  }
}, { immediate: true })
</script>