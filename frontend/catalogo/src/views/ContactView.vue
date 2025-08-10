<!-- ContactView.vue - Página de contacto -->
<template>
  <div class="min-h-screen pt-20 bg-gray-50">
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <!-- Header -->
      <div class="text-center mb-12">
        <h1 class="text-4xl font-bold text-gray-900 mb-4">
          Contactanos
        </h1>
        <p class="text-lg text-gray-600">
          Estamos aquí para ayudarte con cualquier consulta sobre nuestros productos
        </p>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-2 gap-12">
        <!-- Información de contacto -->
        <div class="bg-white rounded-lg shadow-md p-8">
          <h2 class="text-2xl font-semibold text-gray-900 mb-6">
            {{ EMPRESA_CONFIG.nombre }}
          </h2>
          
          <div class="space-y-6">
            <!-- Dirección -->
            <div class="flex items-start gap-4">
              <div class="w-6 h-6 text-red-600 mt-1">
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"/>
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"/>
                </svg>
              </div>
              <div>
                <h3 class="font-semibold text-gray-900">Dirección</h3>
                <p class="text-gray-600">{{ EMPRESA_CONFIG.direccion }}</p>
              </div>
            </div>

            <!-- Teléfono -->
            <div v-if="EMPRESA_CONFIG.telefono" class="flex items-start gap-4">
              <div class="w-6 h-6 text-red-600 mt-1">
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"/>
                </svg>
              </div>
              <div>
                <h3 class="font-semibold text-gray-900">Teléfono</h3>
                <p class="text-gray-600">{{ EMPRESA_CONFIG.telefono }}</p>
              </div>
            </div>

            <!-- Email -->
            <div v-if="EMPRESA_CONFIG.email" class="flex items-start gap-4">
              <div class="w-6 h-6 text-red-600 mt-1">
                <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/>
                </svg>
              </div>
              <div>
                <h3 class="font-semibold text-gray-900">Email</h3>
                <p class="text-gray-600">{{ EMPRESA_CONFIG.email }}</p>
              </div>
            </div>
          </div>

          <!-- Redes sociales -->
          <div class="mt-8">
            <h3 class="font-semibold text-gray-900 mb-4">Seguinos en redes</h3>
            <div class="flex gap-4">
              <a 
                v-if="EMPRESA_CONFIG.facebook" 
                :href="EMPRESA_CONFIG.facebook"
                target="_blank"
                class="w-10 h-10 bg-blue-600 text-white rounded-full flex items-center justify-center hover:bg-blue-700 transition-colors"
              >
                <span class="text-sm font-bold">f</span>
              </a>
              <a 
                v-if="EMPRESA_CONFIG.instagram" 
                :href="EMPRESA_CONFIG.instagram"
                target="_blank"
                class="w-10 h-10 bg-pink-600 text-white rounded-full flex items-center justify-center hover:bg-pink-700 transition-colors"
              >
                <span class="text-sm font-bold">ig</span>
              </a>
            </div>
          </div>
        </div>

        <!-- WhatsApp Contact -->
        <div class="bg-green-50 border border-green-200 rounded-lg p-8">
          <div class="text-center">
            <div class="w-16 h-16 bg-green-600 text-white rounded-full mx-auto mb-6 flex items-center justify-center">
              <svg class="w-8 h-8" fill="currentColor" viewBox="0 0 24 24">
                <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893A11.821 11.821 0 0020.485 3.097"/>
              </svg>
            </div>
            <h3 class="text-2xl font-semibold text-gray-900 mb-4">
              ¡Consultanos por WhatsApp!
            </h3>
            <p class="text-gray-600 mb-6">
              La forma más rápida de contactarnos es a través de WhatsApp. Te respondemos al instante.
            </p>
            <a 
              :href="whatsappLink"
              target="_blank"
              class="inline-flex items-center gap-2 px-6 py-3 bg-green-600 text-white font-semibold rounded-lg hover:bg-green-700 transition-colors duration-200"
            >
              <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893A11.821 11.821 0 0020.485 3.097"/>
              </svg>
              Escribir por WhatsApp
            </a>
          </div>
        </div>
      </div>
    </div>

    <!-- WhatsApp Button -->
    <WhatsAppButton />
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import EMPRESA_CONFIG, { getWhatsAppLink } from '@/config/empresa.config'
import WhatsAppButton from '@/components/common/WhatsAppButton.vue'
import { applyTheme } from '@/utils/theme'

// Computed
const whatsappLink = computed(() => 
  getWhatsAppLink('¡Hola! Me gustaría hacer una consulta sobre sus productos.')
)

// Lifecycle
onMounted(() => {
  applyTheme()
  document.title = `Contacto - ${EMPRESA_CONFIG.nombre}`
})
</script>