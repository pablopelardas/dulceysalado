<!-- AppFooter.vue - Footer simplificado con información disponible -->
<template>
  <footer class="mt-auto py-8 bg-white/10 backdrop-blur-md border-t border-white/20">
    <div class="container py-8">
      <div class="flex flex-col md:flex-row md:justify-between gap-8">
        <!-- Info de la empresa -->
        <div>
          <div class="flex items-center gap-3 mb-4">
            <div 
              v-if="companyLogo"
              class="w-10 h-10 rounded-lg overflow-hidden"
            >
              <img 
                :src="companyLogo" 
                :alt="companyName"
                class="w-full h-full object-cover"
              />
            </div>
            <div 
              v-else 
              class="w-10 h-10 rounded-lg flex items-center justify-center text-white font-bold"
              :style="{ background: 'var(--theme-accent)' }"
            >
              {{ companyName.charAt(0) }}
            </div>
            <div>
              <h3 class="font-semibold text-white">{{ companyName }}</h3>
              <p v-if="company?.razonSocial" class="text-sm text-white/70">{{ company.razonSocial }}</p>
            </div>
          </div>
          
          <p class="text-sm text-white/70 mb-2">
            Tu catálogo online confiable. Encuentra todos nuestros productos disponibles las 24 horas.
          </p>
          
          <p class="text-xs text-white/60 mb-4 italic">
            * Los precios mostrados están sujetos a cambios por parte del vendedor. Los precios en esta página web pueden no reflejar los precios finales del local.
          </p>
          
          <!-- Social links - Solo mostrar si existen -->
          <div v-if="hasSocialLinks" class="flex gap-2">
            <a 
              v-if="company?.facebook"
              :href="company.facebook" 
              target="_blank"
              rel="noopener noreferrer"
              class="p-2 rounded-lg bg-white/10 text-white hover:bg-white/20 transition-colors"
              aria-label="Facebook"
            >
              <svg class="h-5 w-5" fill="currentColor" viewBox="0 0 24 24">
                <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/>
              </svg>
            </a>
            <a 
              v-if="company?.instagram"
              :href="company.instagram"
              target="_blank"
              rel="noopener noreferrer"
              class="p-2 rounded-lg bg-white/10 text-white hover:bg-white/20 transition-colors"
              aria-label="Instagram"
            >
              <svg class="h-5 w-5" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12.315 2c2.43 0 2.784.013 3.808.06 1.064.049 1.791.218 2.427.465a4.902 4.902 0 011.772 1.153 4.902 4.902 0 011.153 1.772c.247.636.416 1.363.465 2.427.048 1.067.06 1.407.06 4.123v.08c0 2.643-.012 2.987-.06 4.043-.049 1.064-.218 1.791-.465 2.427a4.902 4.902 0 01-1.153 1.772 4.902 4.902 0 01-1.772 1.153c-.636.247-1.363.416-2.427.465-1.067.048-1.407.06-4.123.06h-.08c-2.643 0-2.987-.012-4.043-.06-1.064-.049-1.791-.218-2.427-.465a4.902 4.902 0 01-1.772-1.153 4.902 4.902 0 01-1.153-1.772c-.247-.636-.416-1.363-.465-2.427-.047-1.024-.06-1.379-.06-3.808v-.63c0-2.43.013-2.784.06-3.808.049-1.064.218-1.791.465-2.427a4.902 4.902 0 011.153-1.772A4.902 4.902 0 015.45 2.525c.636-.247 1.363-.416 2.427-.465C8.901 2.013 9.256 2 11.685 2h.63zm-.081 1.802h-.468c-2.456 0-2.784.011-3.807.058-.975.045-1.504.207-1.857.344-.467.182-.8.398-1.15.748-.35.35-.566.683-.748 1.15-.137.353-.3.882-.344 1.857-.047 1.023-.058 1.351-.058 3.807v.468c0 2.456.011 2.784.058 3.807.045.975.207 1.504.344 1.857.182.466.399.8.748 1.15.35.35.683.566 1.15.748.353.137.882.3 1.857.344 1.054.048 1.37.058 4.041.058h.08c2.597 0 2.917-.01 3.96-.058.976-.045 1.505-.207 1.858-.344.466-.182.8-.398 1.15-.748.35-.35.566-.683.748-1.15.137-.353.3-.882.344-1.857.048-1.055.058-1.37.058-4.041v-.08c0-2.597-.01-2.917-.058-3.96-.045-.976-.207-1.505-.344-1.858a3.097 3.097 0 00-.748-1.15 3.098 3.098 0 00-1.15-.748c-.353-.137-.882-.3-1.857-.344-1.023-.047-1.351-.058-3.807-.058zM12 6.865a5.135 5.135 0 110 10.27 5.135 5.135 0 010-10.27zm0 1.802a3.333 3.333 0 100 6.666 3.333 3.333 0 000-6.666zm5.338-3.205a1.2 1.2 0 110 2.4 1.2 1.2 0 010-2.4z"/>
              </svg>
            </a>
          </div>
        </div>
        
        <!-- Contacto - Solo mostrar si hay información -->
        <div v-if="hasContactInfo">
          <h4 class="font-semibold text-white mb-4">Contacto</h4>
          <div class="space-y-3">
            <div v-if="company?.direccion" class="flex items-start gap-3">
              <MapPinIcon class="h-5 w-5 text-white/70 mt-0.5" />
              <span class="text-sm text-white/70">{{ company.direccion }}</span>
            </div>
            <div v-if="company?.telefono" class="flex items-center gap-3">
              <PhoneIcon class="h-5 w-5 text-white/70" />
              <a 
                :href="`tel:${company.telefono}`"
                class="text-sm text-white/70 hover:text-white transition-colors"
              >
                {{ company.telefono }}
              </a>
            </div>
            <div v-if="company?.email" class="flex items-center gap-3">
              <EnvelopeIcon class="h-5 w-5 text-white/70" />
              <a 
                :href="`mailto:${company.email}`"
                class="text-sm text-white/70 hover:text-white transition-colors"
              >
                {{ company.email }}
              </a>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Bottom bar -->
      <div class="border-t border-white/10 mt-8 pt-6">
        <div class="flex flex-col sm:flex-row justify-between items-center gap-4 text-sm text-white/60">
          <p>© {{ currentYear }} {{ companyName }}. Todos los derechos reservados.</p>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { EMPRESA_CONFIG } from '@/config/empresa.config'
import { 
  MapPinIcon,
  PhoneIcon,
  EnvelopeIcon
} from '@heroicons/vue/24/outline'


// Computed
const company = computed(() => EMPRESA_CONFIG)
const companyName = computed(() => EMPRESA_CONFIG.nombre)
const companyLogo = computed(() => EMPRESA_CONFIG.logoUrl)
const currentYear = computed(() => new Date().getFullYear())

// Check if we have social links
const hasSocialLinks = computed(() => {
  return company.value?.facebook || company.value?.instagram
})

// Check if we have contact info
const hasContactInfo = computed(() => {
  return company.value?.direccion || company.value?.telefono || company.value?.email
})
</script>