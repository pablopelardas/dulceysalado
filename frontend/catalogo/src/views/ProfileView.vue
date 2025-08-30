<!-- ProfileView.vue - Página de perfil de usuario -->
<template>
  <div class="min-h-screen bg-black">
    <!-- Header spacer -->
    <div class="h-14 lg:h-16"></div>
    
    <div class="max-w-4xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-white">Mi Perfil</h1>
        <p class="mt-2 text-gray-300">Gestiona tu información personal y preferencias</p>
      </div>

      <div class="bg-white/95 shadow-lg rounded-lg">

        <!-- Content -->
        <div class="p-6">
          <!-- Información Personal -->
            <!-- Información de cuenta -->
            <div class="mb-8">
              <h3 class="text-lg font-medium text-gray-900 mb-4">Información de cuenta</h3>
              <div class="bg-gray-50 rounded-lg p-4 mb-6 border border-gray-200">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-600">Código de cliente</label>
                    <p class="text-gray-900 font-mono">{{ authStore.user?.codigo }}</p>
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-600">Lista de precios</label>
                    <p class="text-gray-900">{{ (authStore.user as any)?.lista_precio?.nombre || 'No asignada' }}</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Formulario de datos personales -->
            <form @submit.prevent="handleSubmit">
              <div class="space-y-6">
                <h3 class="text-lg font-medium text-gray-900">Datos personales</h3>
                
                <!-- Nombre y Email -->
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <label for="nombre" class="block text-sm font-medium text-gray-700 mb-2">
                      Nombre *
                    </label>
                    <input
                      id="nombre"
                      v-model="form.nombre"
                      type="text"
                      required
                      class="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                      placeholder="Tu nombre completo"
                      :disabled="loading"
                    />
                  </div>
                  <div>
                    <label for="email" class="block text-sm font-medium text-gray-700 mb-2">
                      Email *
                    </label>
                    <input
                      id="email"
                      v-model="form.email"
                      type="email"
                      required
                      class="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                      placeholder="tu@email.com"
                      :disabled="loading"
                    />
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
                    class="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                    placeholder="Ej: +54 11 1234-5678"
                    :disabled="loading"
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
                    class="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                    placeholder="Ej: Av. Corrientes 1234"
                    :disabled="loading"
                  />
                </div>

                <!-- Localidad, Provincia y Altura -->
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div>
                    <label for="localidad" class="block text-sm font-medium text-gray-700 mb-2">
                      Localidad
                    </label>
                    <input
                      id="localidad"
                      v-model="form.localidad"
                      type="text"
                      class="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
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
                      class="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
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
                  <div>
                    <label for="altura" class="block text-sm font-medium text-gray-700 mb-2">
                      Altura
                    </label>
                    <input
                      id="altura"
                      v-model="form.altura"
                      type="text"
                      class="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                      placeholder="Ej: 1234"
                      :disabled="loading"
                    />
                  </div>
                </div>

                <!-- CUIT y Tipo IVA -->
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label for="cuit" class="block text-sm font-medium text-gray-700 mb-2">
                      CUIT
                    </label>
                    <input
                      id="cuit"
                      v-model="form.cuit"
                      type="text"
                      class="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
                      placeholder="Ej: 20-12345678-9"
                      :disabled="loading"
                      maxlength="13"
                      @input="formatCuit"
                    />
                  </div>
                  <div>
                    <label for="tipoIva" class="block text-sm font-medium text-gray-700 mb-2">
                      Condición ante IVA
                    </label>
                    <select
                      id="tipoIva"
                      v-model="form.tipo_iva"
                      class="w-full px-3 py-2 border border-gray-300 bg-white text-gray-900 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200"
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
                </div>

                <!-- Error message -->
                <div v-if="error" class="rounded-md bg-red-50 p-4 border border-red-200">
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

                <!-- Success message -->
                <div v-if="successMessage" class="rounded-md bg-green-50 p-4 border border-green-200">
                  <div class="flex">
                    <div class="flex-shrink-0">
                      <svg class="h-5 w-5 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
                      </svg>
                    </div>
                    <div class="ml-3">
                      <p class="text-sm font-medium text-green-800">
                        {{ successMessage }}
                      </p>
                    </div>
                  </div>
                </div>

                <!-- Action buttons -->
                <div class="flex justify-between items-center pt-6 border-t border-gray-200">
                  <RouterLink 
                    to="/catalogo"
                    class="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-600 hover:text-gray-900 transition-colors duration-200"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/>
                    </svg>
                    Volver al Catálogo
                  </RouterLink>
                  
                  <div class="flex space-x-4">
                    <button
                      type="button"
                      @click="resetForm"
                      :disabled="loading"
                      class="px-4 py-2 border border-gray-300 text-sm font-medium rounded-lg text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors duration-200 disabled:opacity-50 cursor-pointer disabled:cursor-not-allowed"
                    >
                      Restablecer
                    </button>
                    <button
                      type="submit"
                      :disabled="loading || !hasChanges"
                      class="px-6 py-2 border border-transparent text-sm font-medium rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-all duration-200 disabled:opacity-50 cursor-pointer disabled:cursor-not-allowed"
                      :style="{ backgroundColor: 'var(--theme-accent)' }"
                    >
                      <span v-if="loading" class="flex items-center">
                        <svg class="animate-spin -ml-1 mr-3 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                        Guardando...
                      </span>
                      <span v-else>Guardar cambios</span>
                    </button>
                  </div>
                </div>
              </div>
            </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
// authApiService import removed - not used

const router = useRouter()
const authStore = useAuthStore()

// State
const loading = ref(false)
const error = ref<string | null>(null)
const successMessage = ref<string | null>(null)

// Form data
const form = ref({
  nombre: '',
  email: '',
  telefono: '',
  direccion: '',
  localidad: '',
  provincia: '',
  cuit: '',
  tipo_iva: '',
  altura: ''
})


// Original data for comparison
const originalData = ref<typeof form.value>({} as any)

// Computed
const hasChanges = computed(() => {
  return JSON.stringify(form.value) !== JSON.stringify(originalData.value)
})

// Methods
const loadUserData = async () => {
  if (!authStore.token) {
    router.push('/login')
    return
  }

  loading.value = true
  error.value = null

  try {
    // Usar la función del store para refrescar el perfil desde el servidor
    const success = await authStore.refreshUserProfile()
    
    if (!success) {
      error.value = 'Error al cargar los datos del perfil'
      router.push('/login')
      return
    }
    
    // Obtener los datos del usuario actualizado del store
    const user = authStore.user
    if (!user) {
      error.value = 'No se pudieron cargar los datos del usuario'
      return
    }
    
    const userData = {
      nombre: user.nombre || '',
      email: user.email || '',
      telefono: user.telefono || '',
      direccion: user.direccion || '',
      localidad: (user as any).localidad || '',
      provincia: (user as any).provincia || '',
      cuit: (user as any).cuit || '',
      tipo_iva: (user as any).tipo_iva || '',
      altura: (user as any).altura || ''
    }
    
    form.value = { ...userData }
    originalData.value = { ...userData }
    
  } catch (err) {
    console.error('Error al cargar perfil:', err)
    error.value = 'Error al cargar los datos del perfil'
    router.push('/login')
  } finally {
    loading.value = false
  }
}

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

const resetForm = () => {
  form.value = { ...originalData.value }
  error.value = null
  successMessage.value = null
}

const handleSubmit = async () => {
  loading.value = true
  error.value = null
  successMessage.value = null
  
  try {
    // Filtrar campos que han cambiado
    const changes = Object.entries(form.value)
      .filter(([key, value]) => value !== (originalData.value as any)[key])
      .reduce((acc, [key, value]) => {
        if (value.trim() !== '') {
          acc[key] = value.trim()
        }
        return acc
      }, {} as Record<string, string>)
    
    if (Object.keys(changes).length === 0) {
      successMessage.value = 'No hay cambios para guardar'
      return
    }
    
    const success = await authStore.updateProfile(changes)
    
    if (success) {
      successMessage.value = 'Perfil actualizado correctamente'
      // Recargar datos frescos del servidor
      await loadUserData()
      
      // Clear success message after 3 seconds
      setTimeout(() => {
        successMessage.value = null
      }, 3000)
    } else {
      throw new Error('Error al actualizar el perfil')
    }
    
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al guardar los cambios'
  } finally {
    loading.value = false
  }
}


// Lifecycle
onMounted(async () => {
  // Verificar autenticación
  if (!authStore.isAuthenticated) {
    router.push('/login')
    return
  }
  
  await loadUserData()
})
</script>

<style scoped>
/* Estilos adicionales si son necesarios */
</style>