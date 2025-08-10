<!-- AppHeader.vue - Header mejorado con mejor spacing -->
<template>
  <div>
    <!-- Main Header -->
    <header 
      class="fixed top-0 left-0 right-0 z-50 shadow-lg glass-header"
      style="height: 80px;"
    >
      <div class="container h-full">
        <div 
          class="flex items-center justify-between h-full py-3"
        >
          <!-- Logo y nombre con mejor proporción -->
          <RouterLink to="/" class="group flex items-center gap-3">
            <div class="relative">
              <div 
                v-if="companyLogo"
                class="w-10 h-10 rounded-xl overflow-hidden border-2 border-white/20 shadow-md group-hover:shadow-lg transition-all"
              >
                <img 
                  :src="companyLogo" 
                  :alt="companyName"
                  class="w-full h-full object-cover"
                />
              </div>
              <div 
                v-else 
                class="w-10 h-10 rounded-xl flex items-center justify-center text-white font-bold text-sm shadow-md"
                :style="{ background: 'var(--theme-accent)' }"
              >
                {{ companyName.charAt(0) }}
              </div>
            </div>
            
            <div class="text-white hidden sm:block">
              <h1 class="font-semibold text-base">
                {{ companyName }}
              </h1>
            </div>
          </RouterLink>
          
          <!-- Search Bar -->
          <div class="flex-1 max-w-lg mx-4">
            <SearchBar 
              v-model="searchQuery"
              @search="handleSearch"
              @searchWithScroll="handleSearchWithScroll"
              placeholder="Buscar productos..."
              compact
            />
          </div>
          
          <!-- Cart Button -->
          <FloatingCart 
            :always-show="true" 
            :compact="true"
            @open-summary="$emit('openCartSummary')"
            @open-export="$emit('openExportOptions')"
          />
        </div>
      </div>
    </header>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { useCompanyStore } from '@/stores/company'
import { useCatalogStore } from '@/stores/catalog'
import SearchBar from '@/components/catalog/SearchBar.vue'
import FloatingCart from '@/components/cart/FloatingCart.vue'

// Emits
const emit = defineEmits<{
  openCartSummary: []
  openExportOptions: []
}>()

// Stores
const companyStore = useCompanyStore()
const catalogStore = useCatalogStore()

// State
const searchQuery = ref('')

// Computed
const companyName = computed(() => companyStore.companyName)
const companyLogo = computed(() => companyStore.companyLogo)

// Methods
const handleSearch = async () => {
  await catalogStore.setSearch(searchQuery.value)
}

const handleSearchWithScroll = async () => {
  await catalogStore.setSearch(searchQuery.value)
  // Always scroll to products when actively searching
  requestAnimationFrame(() => {
    const toolbarElement = document.querySelector('.products-toolbar')
    if (toolbarElement) {
      const yOffset = -100; // Account for fixed header and better positioning
      const y = toolbarElement.getBoundingClientRect().top + window.pageYOffset + yOffset;
      
      // Custom easing function
      const easeInOutCubic = (t: number): number => {
        return t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
      };
      
      // Animate scroll with 300ms duration
      const duration = 300;
      const startY = window.pageYOffset;
      const distance = y - startY;
      const startTime = performance.now();
      
      const animateScroll = (currentTime: number) => {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);
        const easedProgress = easeInOutCubic(progress);
        
        window.scrollTo(0, startY + distance * easedProgress);
        
        if (progress < 1) {
          requestAnimationFrame(animateScroll);
        }
      };
      
      requestAnimationFrame(animateScroll);
    }
  })
}


watch(() => catalogStore.searchQuery, (newQuery) => {
  searchQuery.value = newQuery
})

onMounted(() => {
  // Sync initial state from store
  searchQuery.value = catalogStore.searchQuery
})
</script>

<style scoped>
 @reference "tailwindcss";
.category-pill {
  @apply flex items-center gap-2 px-3 py-2 rounded-full text-white text-sm font-medium transition-all justify-center min-h-[2.5rem] cursor-pointer;
  background: var(--theme-secondary);
}

.category-pill:hover {
  filter: brightness(1.1);
}

.category-pill.active {
  filter: brightness(1.2);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
}

.category-pill .count {
  @apply text-xs opacity-70;
}

.category-icon-text {
  @apply text-lg font-bold flex-shrink-0;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.3);
}
</style>