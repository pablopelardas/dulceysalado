<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { RouterView } from 'vue-router'
import { useCompanyStore } from '@/stores/company'
import { useCatalogStore } from '@/stores/catalog'

const companyStore = useCompanyStore()
const catalogStore = useCatalogStore()
const appReady = ref(false)

onMounted(async () => {
  // Initialize company and theme first
  await companyStore.init()
  
  // Initialize catalog store with company data
  catalogStore.initWithCompany()
  
  // Load categories once for the entire app
  if (!catalogStore.hasCategories) {
    await catalogStore.fetchCategories()
  }

  // Small delay to ensure theme is applied
  setTimeout(() => {
    appReady.value = true
  }, 100)
})
</script>

<template>
  <!-- Global App Loader -->
  <Transition name="app-loader">
    <div 
      v-if="!appReady"
      class="fixed inset-0 z-[9999] flex items-center justify-center min-h-screen"
      style="background: linear-gradient(135deg, #6b7280 0%, #4b5563 100%)"
    >
      <div class="text-center flex flex-col justify-center items-center gap-3 px-6">
        <!-- Logo/Brand -->
        <div class="w-24 h-24 mx-auto mb-8 rounded-3xl bg-white/10 backdrop-blur-lg border border-white/20 flex items-center justify-center shadow-2xl">
          <div 
            v-if="companyStore.companyLogo"
            class="w-16 h-16 rounded-2xl overflow-hidden"
          >
            <img 
              :src="companyStore.companyLogo" 
              :alt="companyStore.companyName"
              class="w-full h-full object-cover"
            />
          </div>
          <div 
            v-else-if="companyStore.companyName"
            class="w-14 h-14 rounded-2xl bg-white/20 flex items-center justify-center text-white font-bold text-2xl"
          >
            {{ companyStore.companyName.charAt(0) }}
          </div>
          <div v-else class="w-14 h-14 rounded-2xl bg-white/20 flex items-center justify-center">
            <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"></path>
            </svg>
          </div>
        </div>
        
        <!-- Loading animation -->
        <div class="flex justify-center mb-6">
          <div class="loading-spinner"></div>
        </div>
        
        <!-- Loading text -->
        <div class="space-y-2">
          <p class="text-white text-xl font-semibold">
            {{ companyStore.companyName ? `Cargando ${companyStore.companyName}` : 'Cargando catálogo' }}
          </p>
          <p class="text-white/60 text-sm">Preparando tu experiencia de compra...</p>
        </div>
      </div>
    </div>
  </Transition>

  <!-- Main App Content -->
  <router-view v-if="appReady" v-slot="{ Component }">
    <transition name="app-content">
      <component :is="Component" />
    </transition>
  </router-view>
</template>

<style scoped>
/* Loading Spinner */
.loading-spinner {
  width: 2rem;
  height: 2rem;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top: 3px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* App Loader Transitions */
.app-loader-enter-active,
.app-loader-leave-active {
  transition: all 0.5s ease;
}

.app-loader-enter-from,
.app-loader-leave-to {
  opacity: 0;
}

/* App Content Transitions */
.app-content-enter-active {
  transition: all 0.4s ease 0.1s;
}

.app-content-enter-from {
  opacity: 0;
  transform: translateY(10px);
}
</style>
