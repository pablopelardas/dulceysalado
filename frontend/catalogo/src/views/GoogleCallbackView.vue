<!-- GoogleCallbackView.vue - Página de callback para Google OAuth -->
<template>
  <div class="min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8" :style="{ background: `linear-gradient(135deg, var(--theme-primary), var(--theme-gray-dark))` }">
    <div class="max-w-md w-full space-y-8 text-center">
      <div>
        <!-- Logo de la empresa -->
        <div class="flex justify-center mb-6">
          <img 
            :src="EMPRESA_CONFIG.logoUrl" 
            :alt="EMPRESA_CONFIG.nombre"
            class="h-20 w-auto object-contain"
          />
        </div>
        
        <!-- Loading State -->
        <div v-if="loading" class="text-center">
          <svg class="animate-spin h-12 w-12 text-white mx-auto mb-4" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <h2 class="text-2xl font-bold text-white mb-2">
            Procesando autenticación...
          </h2>
          <p class="text-gray-300">
            Por favor espera mientras te autenticamos con Google
          </p>
        </div>

        <!-- Error State -->
        <div v-else-if="error" class="text-center">
          <div class="bg-red-100 rounded-full w-16 h-16 flex items-center justify-center mx-auto mb-4">
            <svg class="h-8 w-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </div>
          <h2 class="text-2xl font-bold text-white mb-2">
            Error de autenticación
          </h2>
          <p class="text-gray-300 mb-6">
            {{ error }}
          </p>
          <RouterLink 
            to="/login" 
            class="inline-flex items-center px-4 py-2 text-white font-medium rounded-lg transition-colors duration-200"
            :style="{ backgroundColor: 'var(--theme-accent)' }"
          >
            Volver al login
          </RouterLink>
        </div>

        <!-- Success State -->
        <div v-else class="text-center">
          <div class="bg-green-100 rounded-full w-16 h-16 flex items-center justify-center mx-auto mb-4">
            <svg class="h-8 w-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
            </svg>
          </div>
          <h2 class="text-2xl font-bold text-white mb-2">
            ¡Autenticación exitosa!
          </h2>
          <p class="text-gray-300">
            Redirigiendo...
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter, RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { EMPRESA_CONFIG } from '@/config/empresa.config'
import { applyTheme } from '@/utils/theme'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// State
const loading = ref(true)
const error = ref<string | null>(null)

// Process OAuth callback
const processCallback = async () => {
  try {
    // Obtener parámetros de la URL
    const token = route.query.token as string
    const refreshToken = route.query.refresh_token as string
    const isNew = route.query.is_new === 'true'
    const errorMessage = route.query.error as string

    if (errorMessage) {
      throw new Error(errorMessage)
    }

    if (!token || !refreshToken) {
      throw new Error('No se recibieron tokens de autenticación')
    }

    // Guardar tokens en el store
    authStore.token = token
    authStore.refreshToken = refreshToken
    
    // Guardar en localStorage
    localStorage.setItem('auth_token', token)
    localStorage.setItem('refresh_token', refreshToken)

    // Obtener perfil del usuario
    await authStore.fetchUserProfile()

    // Determinar a dónde redirigir
    let redirectTo = '/'
    
    if (isNew) {
      // Usuario nuevo: ir a completar perfil
      redirectTo = '/completar-perfil'
    } else {
      // Usuario existente: ir al home o página guardada
      redirectTo = route.query.redirect as string || '/'
    }
    
    setTimeout(() => {
      router.push(redirectTo)
    }, 1500)

  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al procesar la autenticación'
    loading.value = false
  }
}

// Lifecycle
onMounted(() => {
  applyTheme()
  processCallback()
})
</script>

<style scoped>
/* Estilos adicionales si son necesarios */
</style>