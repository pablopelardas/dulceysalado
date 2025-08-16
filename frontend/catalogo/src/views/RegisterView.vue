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
          <div>
            <label for="telefono" class="block text-sm font-medium text-gray-700 mb-2">
              Teléfono
            </label>
            <input
              id="telefono"
              v-model="form.telefono"
              type="tel"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
              placeholder="Ej: +54 11 1234-5678"
              :disabled="authStore.loading"
            />
          </div>

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
              placeholder="Tu dirección completa"
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
                class="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 transition-colors duration-200"
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
                class="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 transition-colors duration-200"
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

          <!-- Terms and conditions -->
          <div class="flex items-start">
            <div class="flex items-center h-5">
              <input
                id="acceptTerms"
                v-model="form.acceptTerms"
                type="checkbox"
                required
                class="h-4 w-4 text-red-600 focus:ring-red-500 border-gray-300 rounded"
                :disabled="authStore.loading"
              />
            </div>
            <div class="ml-3 text-sm">
              <label for="acceptTerms" class="text-gray-700">
                Acepto los 
                <a href="#" class="font-medium text-red-600 hover:text-red-500 transition-colors duration-200">
                  términos y condiciones
                </a>
                y la 
                <a href="#" class="font-medium text-red-600 hover:text-red-500 transition-colors duration-200">
                  política de privacidad
                </a>
              </label>
            </div>
          </div>

          <!-- Newsletter -->
          <div class="flex items-center">
            <input
              id="newsletter"
              v-model="form.newsletter"
              type="checkbox"
              class="h-4 w-4 text-red-600 focus:ring-red-500 border-gray-300 rounded"
              :disabled="authStore.loading"
            />
            <label for="newsletter" class="ml-2 block text-sm text-gray-700">
              Quiero recibir ofertas y novedades por email
            </label>
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
              class="group relative w-full flex justify-center py-3 px-4 border border-transparent text-sm font-medium rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
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
  confirmPassword: '',
  acceptTerms: false,
  newsletter: false
})

const showPassword = ref(false)
const showConfirmPassword = ref(false)

// Computed
const isFormValid = computed(() => {
  return form.value.nombre.trim() &&
         form.value.apellido.trim() &&
         form.value.email.trim() &&
         form.value.password.length >= 6 &&
         form.value.password === form.value.confirmPassword &&
         form.value.acceptTerms
})

// Methods
const handleSubmit = async () => {
  if (!isFormValid.value) return
  
  authStore.clearError()
  
  const success = await authStore.register({
    nombre: form.value.nombre.trim(),
    apellido: form.value.apellido.trim(),
    email: form.value.email.trim(),
    password: form.value.password,
    telefono: form.value.telefono.trim() || undefined,
    direccion: form.value.direccion.trim() || undefined
  })

  if (success) {
    // Redirigir al home o a la página anterior
    const redirectTo = router.currentRoute.value.query.redirect as string || '/'
    router.push(redirectTo)
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