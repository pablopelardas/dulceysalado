<!-- Catalog.vue - Soporte para lista de precios y query params -->
<template>
  <AppLayout>
    <ProductGrid />
  </AppLayout>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { EMPRESA_CONFIG } from '@/config/empresa.config'
import { useCatalogStore } from '@/stores/catalog'
import AppLayout from '@/components/layout/AppLayout.vue'
import ProductGrid from '@/components/catalog/ProductGrid.vue'

const route = useRoute()
const router = useRouter()
const catalogStore = useCatalogStore()

// Function to update URL with current state
const updateURL = () => {
  const query: Record<string, string> = {}
  
  if (catalogStore.searchQuery) query.q = catalogStore.searchQuery
  if (catalogStore.selectedCategory !== null && catalogStore.selectedCategory !== undefined) {
    query.categoria = catalogStore.selectedCategory.toString()
  }
  if (catalogStore.sortBy && catalogStore.sortBy !== 'nombre_asc') query.orden = catalogStore.sortBy
  if (catalogStore.currentPage > 1) query.page = catalogStore.currentPage.toString()
  if (catalogStore.showFeaturedOnly) query.destacados = 'true'
  
  // Only update if query actually changed
  const currentQuery = JSON.stringify(route.query)
  const newQuery = JSON.stringify(query)
  
  if (currentQuery !== newQuery) {
    // Save current scroll position before updating URL
    const scrollY = window.scrollY
    
    router.replace({ query }).then(() => {
      // Restore scroll position after URL update
      requestAnimationFrame(() => {
        window.scrollTo(0, scrollY)
      })
    })
  }
}

// Function to restore state from URL
const restoreFromURL = async () => {
  const query = route.query
  
  // Restore search
  if (query.q && typeof query.q === 'string') {
    catalogStore.searchQuery = query.q
  }
  
  // Restore category
  if (query.categoria && typeof query.categoria === 'string') {
    const categoryId = parseInt(query.categoria)
    if (!isNaN(categoryId)) {
      catalogStore.selectedCategory = categoryId
    }
  }
  
  // Restore sort
  if (query.orden && typeof query.orden === 'string') {
    const validSorts = ['precio_asc', 'precio_desc', 'nombre_asc', 'nombre_desc']
    if (validSorts.includes(query.orden)) {
      catalogStore.sortBy = query.orden as any
    }
  }
  
  // Restore page
  if (query.page && typeof query.page === 'string') {
    const page = parseInt(query.page)
    if (!isNaN(page) && page > 0) {
      catalogStore.currentPage = page
    }
  }
  
  // Restore featured
  if (query.destacados === 'true') {
    catalogStore.showFeaturedOnly = true
  }
}

// Flag to prevent double fetch when we're updating URL from store changes
let isUpdatingFromStore = false

// Watch for store changes and update URL
watch([
  () => catalogStore.searchQuery,
  () => catalogStore.selectedCategory, 
  () => catalogStore.sortBy,
  () => catalogStore.currentPage,
  () => catalogStore.showFeaturedOnly
], () => {
  isUpdatingFromStore = true
  updateURL()
  // Reset flag after a short delay to allow URL update to complete
  setTimeout(() => {
    isUpdatingFromStore = false
  }, 100)
}, { deep: true })

// Watch for route changes (back/forward navigation)
watch(() => route.query, async (newQuery, oldQuery) => {
  // Only respond to external navigation (not our own updates)
  // Skip if we're currently updating from store changes
  if (JSON.stringify(newQuery) !== JSON.stringify(oldQuery) && !isUpdatingFromStore) {
    console.log('Route query changed externally, restoring from URL')
    await restoreFromURL()
    await catalogStore.fetchProducts()
  }
}, { deep: true })

// Handle browser back/forward
const handlePopState = async () => {
  await restoreFromURL()
  await catalogStore.fetchProducts()
}

onMounted(async () => {
  console.log('Route query:', route.query)
  
  // Add popstate listener for browser navigation
  window.addEventListener('popstate', handlePopState)
  
  // Initialize catalog data
  await catalogStore.initializeAll() // This will fetch categories, novedades, and ofertas
  
  // Set document title
  document.title = `Catálogo - ${EMPRESA_CONFIG.nombre}`
  
  // Restore state from URL query params
  await restoreFromURL()
  
  // Fetch products with restored state - single call
  await catalogStore.fetchProducts()
  
  // If we have filters from URL, fetch original total count in parallel (doesn't affect loading state)
  if (catalogStore.selectedCategory !== null || catalogStore.searchQuery || catalogStore.showFeaturedOnly) {
    catalogStore.fetchOriginalTotalCount() // No await - parallel execution
  }
})

onUnmounted(() => {
  // Clean up event listener
  window.removeEventListener('popstate', handlePopState)
})
</script>