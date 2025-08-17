<!-- CompleteProfileView.vue - Página para completar perfil de usuario nuevo -->
<template>
  <div class="min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8" :style="{ background: `linear-gradient(135deg, var(--theme-primary), var(--theme-gray-dark))` }">
    <div class="max-w-2xl w-full space-y-8">
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
            {{ isNewUser ? '¡Bienvenido/a' : '¡Hola' }} {{ authStore.user?.nombre || 'a nuestra plataforma' }}!
          </h2>
          <p class="mt-2 text-lg text-gray-300">
            {{ isNewUser ? 'Tu cuenta se creó exitosamente. Ahora podés' : 'Podés' }} completar tu perfil para personalizar tu experiencia
          </p>
          <p class="mt-1 text-sm text-gray-400">
            Todos los campos son opcionales, podés completarlos ahora o más tarde
          </p>
        </div>
      </div>

      <div class="bg-white rounded-xl shadow-2xl p-8">
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <!-- Información básica precargada -->
          <div class="bg-gray-50 rounded-lg p-4 mb-6">
            <h3 class="text-lg font-semibold text-gray-900 mb-3">Información de Google</h3>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700">Nombre</label>
                <p class="text-gray-900">{{ authStore.user?.nombre || 'No disponible' }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Email</label>
                <p class="text-gray-900">{{ authStore.user?.email || 'No disponible' }}</p>
              </div>
            </div>
          </div>

          <!-- Información adicional -->
          <div class="space-y-6">
            <h3 class="text-lg font-semibold text-gray-900">Información adicional</h3>
            
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
                placeholder="Ej: Av. Corrientes 1234"
                :disabled="loading"
              />
            </div>

            <!-- Localidad y Provincia -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label for="localidad" class="block text-sm font-medium text-gray-700 mb-2">
                  Localidad
                </label>
                <input
                  id="localidad"
                  v-model="form.localidad"
                  type="text"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                  placeholder="Ej: CABA"
                  :disabled="loading"
                />
              </div>
              <div>
                <label for="provincia" class="block text-sm font-medium text-gray-700 mb-2">
                  Provincia
                </label>
                <select
                  id="provincia"
                  v-model="form.provincia"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                  :disabled="loading"
                >
                  <option value="">Seleccionar provincia</option>
                  <option value="Buenos Aires">Buenos Aires</option>
                  <option value="CABA">Ciudad Autónoma de Buenos Aires</option>
                  <option value="Catamarca">Catamarca</option>
                  <option value="Chaco">Chaco</option>
                  <option value="Chubut">Chubut</option>
                  <option value="Córdoba">Córdoba</option>
                  <option value="Corrientes">Corrientes</option>
                  <option value="Entre Ríos">Entre Ríos</option>
                  <option value="Formosa">Formosa</option>
                  <option value="Jujuy">Jujuy</option>
                  <option value="La Pampa">La Pampa</option>
                  <option value="La Rioja">La Rioja</option>
                  <option value="Mendoza">Mendoza</option>
                  <option value="Misiones">Misiones</option>
                  <option value="Neuquén">Neuquén</option>
                  <option value="Río Negro">Río Negro</option>
                  <option value="Salta">Salta</option>
                  <option value="San Juan">San Juan</option>
                  <option value="San Luis">San Luis</option>
                  <option value="Santa Cruz">Santa Cruz</option>
                  <option value="Santa Fe">Santa Fe</option>
                  <option value="Santiago del Estero">Santiago del Estero</option>
                  <option value="Tierra del Fuego">Tierra del Fuego</option>
                  <option value="Tucumán">Tucumán</option>
                </select>
              </div>
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
                :disabled="loading"
              />
            </div>

            <!-- CUIT -->
            <div>
              <label for="cuit" class="block text-sm font-medium text-gray-700 mb-2">
                CUIT
              </label>
              <input
                id="cuit"
                v-model="form.cuit"
                type="text"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                placeholder="Ej: 20-12345678-9"
                :disabled="loading"
                maxlength="13"
                @input="formatCuit"
              />
            </div>

            <!-- Tipo IVA -->
            <div>
              <label for="tipoIva" class="block text-sm font-medium text-gray-700 mb-2">
                Condición ante IVA
              </label>
              <select
                id="tipoIva"
                v-model="form.tipo_iva"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                :disabled="loading"
              >
                <option value="">Seleccionar condición</option>
                <option value="Responsable Inscripto">Responsable Inscripto</option>
                <option value="Monotributista">Monotributista</option>
                <option value="Exento">Exento</option>
                <option value="Consumidor Final">Consumidor Final</option>
                <option value="Responsable No Inscripto">Responsable No Inscripto</option>
              </select>
            </div>

            <!-- Altura -->
            <div>
              <label for="altura" class="block text-sm font-medium text-gray-700 mb-2">
                Altura
              </label>
              <input
                id="altura"
                v-model="form.altura"
                type="text"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                placeholder="Ej: 1234"
                :disabled="loading"
              />
            </div>
          </div>

          <!-- Error message -->
          <div v-if="error" class="rounded-md bg-red-50 p-4">
            <div class="flex">
              <div class="flex-shrink-0">
                <svg class="h-5 w-5 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
                </svg>
              </div>
              <div class="ml-3">
                <p class="text-sm font-medium text-red-800">
                  {{ error }}
                </p>
              </div>
            </div>
          </div>

          <!-- Action buttons -->
          <div class="flex flex-col sm:flex-row gap-3 pt-6">
            <button
              type="submit"
              :disabled="loading"
              class="flex-1 flex justify-center py-3 px-4 border border-transparent text-sm font-medium rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
              :style="{ backgroundColor: 'var(--theme-accent)' }"
            >
              <span v-if="loading" class="flex items-center">
                <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Guardando...
              </span>
              <span v-else>Guardar información</span>
            </button>
            
            <button
              type="button"
              @click="skipProfile"
              :disabled="loading"
              class="flex-1 sm:flex-initial px-4 py-3 border border-gray-300 text-sm font-medium rounded-lg text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors duration-200 disabled:opacity-50 cursor-pointer disabled:cursor-not-allowed"
            >
              Completar más tarde
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { EMPRESA_CONFIG } from '@/config/empresa.config'
import { applyTheme } from '@/utils/theme'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

// Computed
const isNewUser = computed(() => route.query.new === 'true')

// State
const loading = ref(false)
const error = ref<string | null>(null)

// Form data
const form = ref({
  direccion: '',
  localidad: '',
  provincia: '',
  telefono: '',
  cuit: '',
  tipo_iva: '',
  altura: ''
})

// Methods
const formatCuit = (event: Event) => {
  const target = event.target as HTMLInputElement
  let value = target.value.replace(/\D/g, '') // Solo números
  
  if (value.length >= 2) {
    value = value.substring(0, 2) + '-' + value.substring(2)
  }
  if (value.length >= 11) {
    value = value.substring(0, 11) + '-' + value.substring(11, 12)
  }
  
  form.value.cuit = value
}

const handleSubmit = async () => {
  loading.value = true
  error.value = null
  
  try {
    // Filtrar campos vacíos
    const updateData = Object.entries(form.value)
      .filter(([, value]) => value.trim() !== '')
      .reduce((acc, [key, value]) => {
        acc[key] = value.trim()
        return acc
      }, {} as Record<string, string>)
    
    // Solo enviar si hay datos para actualizar
    if (Object.keys(updateData).length > 0) {
      const success = await authStore.updateProfile(updateData)
      
      if (!success) {
        throw new Error('Error al actualizar el perfil')
      }
    }
    
    // Redirigir al home
    router.push('/')
    
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al guardar los datos'
  } finally {
    loading.value = false
  }
}

const skipProfile = () => {
  router.push('/')
}

// Lifecycle
onMounted(() => {
  applyTheme()
  
  // Verificar que el usuario esté autenticado
  if (!authStore.isAuthenticated) {
    router.push('/login')
    return
  }
  
  // Pre-llenar campos si ya existen
  if (authStore.user) {
    form.value.telefono = authStore.user.telefono || ''
    form.value.direccion = authStore.user.direccion || ''
  }
})
</script>

<style scoped>
/* Estilos adicionales si son necesarios */
</style>