<template>
  <div v-if="showCarousel">
    <ProductCarousel
      title="Novedades"
      :products="products"
      :icon="SparklesIcon"
      :autoplay-interval="5000"
      :modal-open="props.modalOpen"
      @open-cart="$emit('openCart', $event)"
    />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { SparklesIcon } from '@heroicons/vue/24/outline'
import ProductCarousel from './ProductCarousel.vue'
import { useCatalogStore } from '@/stores/catalog'
import type { Product } from '@/services/api'

interface Props {
  modalOpen?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  modalOpen: false
})

defineEmits<{
  openCart: [product: Product]
}>()

// Use catalog store
const catalogStore = useCatalogStore()

// Get data from store
const products = computed(() => catalogStore.novedades)
const loading = computed(() => catalogStore.loadingNovedades)

// Only show carousel if we have products
const showCarousel = computed(() => products.value.length > 0)

// Expose loading state for parent component
defineExpose({
  loading,
  hasProducts: showCarousel,
  finished: computed(() => !loading.value) // Request is finished (regardless of results)
})
</script>