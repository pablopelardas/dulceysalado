<template>
  <div class="carousel-container py-6">
    <!-- Title Skeleton -->
    <div class="flex items-center gap-3 mb-6">
      <div class="w-8 h-8 bg-white/20 rounded animate-pulse"></div>
      <div class="w-32 h-8 bg-white/20 rounded animate-pulse"></div>
    </div>
    
    <!-- Carousel Skeleton -->
    <div class="relative overflow-hidden rounded-xl">
      <div class="flex gap-6">
        <!-- Desktop: 4 cards, Mobile: 1 card -->
        <div
          v-for="i in isMobile ? 1 : 4"
          :key="i"
          :class="[
            'flex-shrink-0',
            isMobile ? 'w-full' : 'w-1/4'
          ]"
        >
          <div class="bg-white/90 rounded-xl shadow-lg p-4">
            <!-- Image skeleton -->
            <div class="bg-gray-200 rounded-lg mb-4 animate-pulse" style="height: 280px;"></div>
            
            <!-- Title skeleton -->
            <div class="space-y-2 mb-4">
              <div class="w-3/4 h-4 bg-gray-200 rounded animate-pulse"></div>
              <div class="w-1/2 h-4 bg-gray-200 rounded animate-pulse"></div>
            </div>
            
            <!-- Price and button skeleton -->
            <div class="flex items-center justify-between">
              <div class="w-20 h-6 bg-gray-200 rounded animate-pulse"></div>
              <div class="w-12 h-12 bg-gray-200 rounded-full animate-pulse"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Mobile indicator skeleton -->
    <div v-if="isMobile" class="flex justify-center items-center gap-3 mt-4">
      <div class="w-10 h-10 bg-white/20 rounded-full animate-pulse"></div>
      <div class="w-16 h-8 bg-white/20 rounded-full animate-pulse"></div>
      <div class="w-10 h-10 bg-white/20 rounded-full animate-pulse"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

// Responsive behavior
const isMobile = ref(false)

const checkMobile = () => {
  isMobile.value = window.innerWidth < 768
}

onMounted(() => {
  checkMobile()
  window.addEventListener('resize', checkMobile)
})

onUnmounted(() => {
  window.removeEventListener('resize', checkMobile)
})
</script>

<style scoped>
@reference "tailwindcss";

.carousel-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
}
</style>