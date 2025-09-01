<!-- RegisterView.vue - Página de registro -->
<template>
  <div class="min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8" :style="{ background: `linear-gradient(135deg, var(--theme-primary), var(--theme-gray-dark))` }">
    <div class="max-w-md w-full space-y-8">
      <div>
        <!-- Logo de la empresa -->
        <div class="flex justify-center mb-6">
          <img 
            :src="EMPRESA_CONFIG.logoUrl" 
            :alt="EMPRESA_CONFIG.nombre"
            class="h-20 w-auto object-contain"
          />
        </div>
        
        <div class="text-center">
          <h2 class="text-3xl font-extrabold text-white">
            Crear Cuenta
          </h2>
          <p class="mt-2 text-sm text-gray-300">
            O 
            <RouterLink 
              to="/login" 
              class="font-medium text-red-400 hover:text-red-300 transition-colors duration-200"
            >
              iniciar sesión con tu cuenta existente
            </RouterLink>
          </p>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-2xl p-8">
        <!-- Google Registration -->
        <div class="mb-6">
          <button
            type="button"
            @click="handleGoogleRegister"
            :disabled="authStore.loading"
            class="w-full flex justify-center items-center gap-3 px-4 py-3 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
          >
            <svg class="w-5 h-5" viewBox="0 0 24 24">
              <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
              <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
              <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
              <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
            </svg>
            Registrarse con Google
          </button>
        </div>

        <!-- Divider -->
        <div class="relative mb-6">
          <div class="absolute inset-0 flex items-center">
            <div class="w-full border-t border-gray-300"></div>
          </div>
          <div class="relative flex justify-center text-sm">
            <span class="px-2 bg-white text-gray-500">O registrarse con email</span>
          </div>
        </div>

        <form @submit.prevent="handleSubmit" class="space-y-6">
          <!-- Nombre y Apellido -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label for="nombre" class="block text-sm font-medium text-gray-700 mb-2">
                Nombre *
              </label>
              <input
                id="nombre"
                v-model="form.nombre"
                type="text"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                placeholder="Tu nombre"
                :disabled="authStore.loading"
              />
            </div>
            <div>
              <label for="apellido" class="block text-sm font-medium text-gray-700 mb-2">
                Apellido *
              </label>
              <input
                id="apellido"
                v-model="form.apellido"
                type="text"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                placeholder="Tu apellido"
                :disabled="authStore.loading"
              />
            </div>
          </div>

          <!-- Email -->
          <div>
            <label for="email" class="block text-sm font-medium text-gray-700 mb-2">
              Correo Electrónico *
            </label>
            <input
              id="email"
              v-model="form.email"
              type="email"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
              placeholder="tu@email.com"
              :disabled="authStore.loading"
            />
          </div>

          <!-- Teléfono -->
          <PhoneInput
            v-model="form.telefono"
            label="Teléfono"
            :disabled="authStore.loading"
          />

          <!-- Dirección -->
          <div>
            <label for="direccion" class="block text-sm font-medium text-gray-700 mb-2">
              Dirección
            </label>
            <input
              id="direccion"
              v-model="form.direccion"
              type="text"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
              placeholder="Ej: Av. Corrientes 1234, CABA, Buenos Aires"
              :disabled="authStore.loading"
            />
          </div>

          <!-- Password -->
          <div>
            <label for="password" class="block text-sm font-medium text-gray-700 mb-2">
              Contraseña *
            </label>
            <div class="relative">
              <input
                id="password"
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                required
                minlength="6"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                placeholder="Mínimo 6 caracteres"
                :disabled="authStore.loading"
              />
              <button
                type="button"
                @click="showPassword = !showPassword"
                class="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 transition-colors duration-200 cursor-pointer disabled:cursor-not-allowed"
                :disabled="authStore.loading"
              >
                <svg v-if="showPassword" class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
                </svg>
                <svg v-else class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21"/>
                </svg>
              </button>
            </div>
            <p class="mt-1 text-xs text-gray-500">
              La contraseña debe tener al menos 6 caracteres
            </p>
          </div>

          <!-- Confirm Password -->
          <div>
            <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-2">
              Confirmar Contraseña *
            </label>
            <div class="relative">
              <input
                id="confirmPassword"
                v-model="form.confirmPassword"
                :type="showConfirmPassword ? 'text' : 'password'"
                required
                class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                :class="{ 'border-red-500': form.confirmPassword && form.password !== form.confirmPassword }"
                placeholder="Repetir contraseña"
                :disabled="authStore.loading"
              />
              <button
                type="button"
                @click="showConfirmPassword = !showConfirmPassword"
                class="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 transition-colors duration-200 cursor-pointer disabled:cursor-not-allowed"
                :disabled="authStore.loading"
              >
                <svg v-if="showConfirmPassword" class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
                </svg>
                <svg v-else class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.878 9.878L3 3m6.878 6.878L21 21"/>
                </svg>
              </button>
            </div>
            <p v-if="form.confirmPassword && form.password !== form.confirmPassword" class="mt-1 text-xs text-red-600">
              Las contraseñas no coinciden
            </p>
          </div>

          <!-- Error message -->
          <div v-if="authStore.error" class="rounded-md bg-red-50 p-4">
            <div class="flex">
              <div class="flex-shrink-0">
                <svg class="h-5 w-5 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
                </svg>
              </div>
              <div class="ml-3">
                <p class="text-sm font-medium text-red-800">
                  {{ authStore.error }}
                </p>
              </div>
            </div>
          </div>

          <!-- Submit button -->
          <div>
            <button
              type="submit"
              :disabled="authStore.loading || !isFormValid"
              class="group relative w-full flex justify-center py-3 px-4 border border-transparent text-sm font-medium rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
              :style="{ backgroundColor: 'var(--theme-accent)' }"
            >
              <span v-if="authStore.loading" class="absolute left-0 inset-y-0 flex items-center pl-3">
                <svg class="h-5 w-5 text-white animate-spin" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
              </span>
              {{ authStore.loading ? 'Creando cuenta...' : 'Crear Cuenta' }}
            </button>
          </div>
        </form>
      </div>

      <!-- Link to home -->
      <div class="text-center">
        <RouterLink 
          to="/" 
          class="text-sm text-gray-300 hover:text-white transition-colors duration-200 flex items-center justify-center gap-2"
        >
          <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
          </svg>
          Volver al inicio
        </RouterLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { EMPRESA_CONFIG } from '@/config/empresa.config'
import { applyTheme } from '@/utils/theme'
import PhoneInput from '@/components/ui/PhoneInput.vue'

const router = useRouter()
const authStore = useAuthStore()

// Form state
const form = ref({
  nombre: '',
  apellido: '',
  email: '',
  telefono: '',
  direccion: '',
  password: '',
  confirmPassword: ''
})

const showPassword = ref(false)
const showConfirmPassword = ref(false)

// Computed
const isFormValid = computed(() => {
  return form.value.nombre.trim() &&
         form.value.apellido.trim() &&
         form.value.email.trim() &&
         form.value.password.length >= 6 &&
         form.value.password === form.value.confirmPassword
})

// Methods
const handleSubmit = async () => {
  if (!isFormValid.value) return
  
  authStore.clearError()
  
  const success = await authStore.register({
    nombre: `${form.value.nombre.trim()} ${form.value.apellido.trim()}`.trim(),
    email: form.value.email.trim(),
    password: form.value.password,
    telefono: form.value.telefono.trim() || undefined,
    direccion: form.value.direccion.trim() || undefined
  })

  if (success) {
    // Redirigir a completar perfil para que puedan agregar más datos
    console.log('Registro exitoso, redirigiendo a completar perfil')
    router.push('/completar-perfil?new=true')
  } else {
    console.log('Registro falló, no redirigiendo')
  }
}

const handleGoogleRegister = async () => {
  authStore.clearError()
  
  const success = await authStore.loginWithGoogle()
  
  if (success) {
    // La redirección se maneja automáticamente en el auth store
    // El callback de Google manejará si es un usuario nuevo o existente
  }
}

// Lifecycle
onMounted(() => {
  applyTheme()
  
  // Si ya está autenticado, redirigir
  if (authStore.isAuthenticated) {
    router.push('/')
  }
})

onUnmounted(() => {
  authStore.clearError()
})
</script>

<style scoped>
/* Estilos adicionales si son necesarios */
</style>