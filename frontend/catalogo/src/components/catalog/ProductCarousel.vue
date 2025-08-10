<template>
  <div class="carousel-container py-6 mb-4 md:mb-0">
    <!-- Title -->
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-2xl md:text-3xl font-bold text-white flex items-center gap-3">
        <component 
          :is="icon" 
          class="h-8 w-8 text-yellow-400"
        />
        {{ title }}
      </h2>
      
      <!-- Manual Controls -->
      <div v-if="products.length > visibleProducts" class="hidden sm:flex items-center gap-2">
        <button
          @click="prevSlide"
          :disabled="currentIndex === 0"
          class="control-btn w-12 h-12 cursor-pointer"
          :class="{ 'opacity-50 cursor-not-allowed': currentIndex === 0 }"
          aria-label="Anterior"
        >
          <ChevronLeftIcon class="h-5 w-5" />
        </button>
        <button
          @click="nextSlide"
          :disabled="currentIndex >= maxIndex"
          class="control-btn w-12 h-12 cursor-pointer"
          :class="{ 'opacity-50 cursor-not-allowed': currentIndex >= maxIndex }"
          aria-label="Siguiente"
        >
          <ChevronRightIcon class="h-5 w-5" />
        </button>
      </div>
    </div>
    
    <!-- Carousel -->
    <div 
      class="relative overflow-hidden rounded-xl"
      @mouseenter="handleMouseEnter"
      @mouseleave="handleMouseLeave"
      @touchstart="handleTouchStart"
      @touchmove="handleTouchMove"
      @touchend="handleTouchEnd"
    >
      <!-- Products Track -->
      <div 
        class="flex transition-transform duration-500 ease-in-out"
        :style="{ transform: `translateX(-${(currentIndex * 100) / visibleProducts}%)` }"
      >
        <div
          v-for="product in products"
          :key="product.codigo"
          :class="[
            'flex-shrink-0 px-3',
            isMobile ? 'w-full' : 'w-1/4'
          ]"
        >
          <ProductCard 
            :product="product"
            :view-mode="'grid'"
            @open-cart="openAddToCartModal"
          />
        </div>
      </div>
    </div>
    
    <!-- Mobile Navigation Indicator -->
    <div v-if="isMobile && products.length > 1" class="flex justify-center items-center gap-3 mt-4">
      <button
        @click="prevSlide"
        :disabled="currentIndex === 0"
        class="control-btn w-10 h-10 cursor-pointer"
        :class="{ 'opacity-50 cursor-not-allowed': currentIndex === 0 }"
        aria-label="Anterior"
      >
        <ChevronLeftIcon class="h-4 w-4" />
      </button>
      
      <div class="bg-white/20 backdrop-blur-sm rounded-full px-4 py-2 text-white font-medium">
        {{ currentIndex + 1 }} / {{ products.length }}
      </div>
      
      <button
        @click="nextSlide"
        :disabled="currentIndex >= maxIndex"
        class="control-btn w-10 h-10 cursor-pointer"
        :class="{ 'opacity-50 cursor-not-allowed': currentIndex >= maxIndex }"
        aria-label="Siguiente"
      >
        <ChevronRightIcon class="h-4 w-4" />
      </button>
    </div>
    
    <!-- Desktop Pagination Indicators -->
    <div v-if="!isMobile && products.length > visibleProducts" class="flex justify-center mt-6 gap-2">
      <button
        v-for="(_, index) in Array(Math.ceil(products.length / visibleProducts))"
        :key="index"
        @click="goToSlide(index * visibleProducts)"
        class="dot cursor-pointer"
        :class="{ 'active': Math.floor(currentIndex / visibleProducts) === index }"
        :aria-label="`Ir a la página ${index + 1}`"
      ></button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { ChevronLeftIcon, ChevronRightIcon } from '@heroicons/vue/24/outline'
import ProductCard from './ProductCard.vue'
import type { Product } from '@/services/api'

interface Props {
  title: string
  products: Product[]
  icon: unknown
  autoplayInterval?: number
  modalOpen?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  autoplayInterval: 5000,
  modalOpen: false
})

const emit = defineEmits<{
  openCart: [product: Product]
}>()

// Reactive data
const currentIndex = ref(0)
const autoplayTimer = ref<number | null>(null)
const isPaused = ref(false)

// Touch/swipe data
const touchStartX = ref(0)
const touchEndX = ref(0)
const touchStartY = ref(0)
const touchEndY = ref(0)
const isSwiping = ref(false)

// Responsive behavior
const isMobile = ref(false)
const visibleProducts = computed(() => isMobile.value ? 1 : 4)
const maxIndex = computed(() => Math.max(0, props.products.length - visibleProducts.value))

// Check if screen is mobile
const checkMobile = () => {
  isMobile.value = window.innerWidth < 768
}

// Navigation methods
const nextSlide = () => {
  if (currentIndex.value < maxIndex.value) {
    currentIndex.value += isMobile.value ? 1 : visibleProducts.value
  } else {
    currentIndex.value = 0 // Loop back to start
  }
}

const prevSlide = () => {
  if (currentIndex.value > 0) {
    currentIndex.value -= isMobile.value ? 1 : visibleProducts.value
  } else {
    currentIndex.value = maxIndex.value // Loop to end
  }
}

const goToSlide = (index: number) => {
  currentIndex.value = Math.min(index, maxIndex.value)
}

// Autoplay methods
const startAutoplay = () => {
  if (props.products.length <= visibleProducts.value || props.modalOpen) return
  
  autoplayTimer.value = setInterval(() => {
    if (!isPaused.value && !props.modalOpen) {
      nextSlide()
    }
  }, props.autoplayInterval)
}

const stopAutoplay = () => {
  if (autoplayTimer.value) {
    clearInterval(autoplayTimer.value)
    autoplayTimer.value = null
  }
}

const pauseAutoplay = () => {
  isPaused.value = true
}

const resumeAutoplay = () => {
  isPaused.value = false
}

const handleMouseEnter = () => {
  console.log('Mouse enter - pausing autoplay')
  isPaused.value = true
}

const handleMouseLeave = () => {
  console.log('Mouse leave - resuming autoplay')
  isPaused.value = false
}

// Touch/swipe handlers
const handleTouchStart = (event: TouchEvent) => {
  if (!isMobile.value) return
  
  touchStartX.value = event.touches[0].clientX
  touchStartY.value = event.touches[0].clientY
  isSwiping.value = true
  isPaused.value = true // Pause autoplay during swipe
}

const handleTouchMove = (event: TouchEvent) => {
  if (!isMobile.value || !isSwiping.value) return
  
  touchEndX.value = event.touches[0].clientX
  touchEndY.value = event.touches[0].clientY
  
  // Prevent default scrolling if we're swiping horizontally
  const deltaX = Math.abs(touchEndX.value - touchStartX.value)
  const deltaY = Math.abs(touchEndY.value - touchStartY.value)
  
  if (deltaX > deltaY && deltaX > 10) {
    event.preventDefault()
  }
}

const handleTouchEnd = () => {
  if (!isMobile.value || !isSwiping.value) return
  
  const deltaX = touchStartX.value - touchEndX.value
  const deltaY = Math.abs(touchStartY.value - touchEndY.value)
  const minSwipeDistance = 50
  
  // Only trigger swipe if horizontal movement is greater than vertical
  if (Math.abs(deltaX) > minSwipeDistance && Math.abs(deltaX) > deltaY) {
    if (deltaX > 0) {
      // Swiped left - go to next
      nextSlide()
    } else {
      // Swiped right - go to previous
      prevSlide()
    }
  }
  
  isSwiping.value = false
  isPaused.value = false // Resume autoplay
}

// Cart modal handler
const openAddToCartModal = (product: Product) => {
  // Pause autoplay when user interacts
  pauseAutoplay()
  setTimeout(resumeAutoplay, 2000) // Resume after 2 seconds
  
  emit('openCart', product)
}

// Watch for products changes to reset carousel
watch(() => props.products, () => {
  currentIndex.value = 0
  stopAutoplay()
  if (props.products.length > 0) {
    startAutoplay()
  }
})

// Watch for modal state changes
watch(() => props.modalOpen, (isOpen) => {
  if (isOpen) {
    // Modal opened - stop autoplay
    stopAutoplay()
  } else {
    // Modal closed - restart autoplay if we have products
    if (props.products.length > 0) {
      startAutoplay()
    }
  }
})

// Lifecycle
onMounted(() => {
  checkMobile()
  window.addEventListener('resize', checkMobile)
  
  if (props.products.length > 0) {
    startAutoplay()
  }
})

onUnmounted(() => {
  window.removeEventListener('resize', checkMobile)
  stopAutoplay()
})
</script>

<style scoped>
@reference "tailwindcss";

.carousel-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
}

.control-btn {
  @apply rounded-full flex items-center justify-center text-white transition-all duration-200;
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(4px);
  border: 1px solid rgba(255, 255, 255, 0.3);
}

.control-btn:hover {
  background: rgba(255, 255, 255, 0.3);
}

.dot {
  @apply w-3 h-3 rounded-full transition-all duration-200;
  background: rgba(255, 255, 255, 0.4);
}

.dot:hover {
  background: rgba(255, 255, 255, 0.6);
}

.dot.active {
  @apply transform scale-125;
  background: white;
}
</style>