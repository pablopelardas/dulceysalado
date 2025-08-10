<template>
  <AppLayout>
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Page Header -->
      <div class="mb-8">
        <nav class="flex mb-4" aria-label="Breadcrumb">
          <ol class="flex items-center space-x-2">
            <li>
              <RouterLink to="/" class="text-gray-500 hover:text-[--theme-primary] transition-colors">
                Inicio
              </RouterLink>
            </li>
            <li class="flex items-center">
              <ChevronRightIcon class="flex-shrink-0 h-4 w-4 text-gray-400 mx-2" />
              <RouterLink to="/catalog" class="text-gray-500 hover:text-[--theme-primary] transition-colors">
                Catálogo
              </RouterLink>
            </li>
            <li class="flex items-center">
              <ChevronRightIcon class="flex-shrink-0 h-4 w-4 text-gray-400 mx-2" />
              <span class="text-gray-900 font-medium">{{ categoryName }}</span>
            </li>
          </ol>
        </nav>
        
        <div v-if="category" class="flex items-center space-x-4">
          <div
            class="w-12 h-12 rounded-full flex items-center justify-center text-white font-bold text-lg"
            :style="{ backgroundColor: category.color }"
          >
            {{ category.nombre.charAt(0).toUpperCase() }}
          </div>
          <div>
            <h1 class="text-3xl font-bold text-gray-900">{{ category.nombre }}</h1>
            <p v-if="category.descripcion" class="text-gray-600 mt-1">
              {{ category.descripcion }}
            </p>
            <p class="text-sm text-gray-500 mt-1">
              {{ category.product_count }} producto{{ category.product_count !== 1 ? 's' : '' }} disponible{{ category.product_count !== 1 ? 's' : '' }}
            </p>
          </div>
        </div>
        
        <div v-else class="animate-pulse">
          <div class="flex items-center space-x-4">
            <div class="w-12 h-12 bg-gray-300 rounded-full"></div>
            <div>
              <div class="h-8 bg-gray-300 rounded w-48 mb-2"></div>
              <div class="h-4 bg-gray-300 rounded w-64"></div>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Products -->
      <ProductGrid />
    </div>
  </AppLayout>
</template>

<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useCompanyStore } from '@/stores/company'
import { useCatalogStore } from '@/stores/catalog'
import { useSeo } from '@/composables/useSeo'
import { ChevronRightIcon } from '@heroicons/vue/24/outline'
import { RouterLink } from 'vue-router'
import AppLayout from '@/components/layout/AppLayout.vue'
import ProductGrid from '@/components/catalog/ProductGrid.vue'

// Composables
const route = useRoute()
const companyStore = useCompanyStore()
const catalogStore = useCatalogStore()
const { setCategorySeo } = useSeo()

// Computed
const categoryCode = computed(() => {
  const code = route.params.code as string
  return code ? parseInt(code) : null
})

const category = computed(() => {
  if (!categoryCode.value) return null
  return catalogStore.getCategoryByCode(categoryCode.value)
})

const categoryName = computed(() => {
  return category.value?.nombre || 'Categoría'
})

// Methods
const loadCategory = async () => {
  if (!categoryCode.value) return
  
  // Set category filter
  catalogStore.setCategory(categoryCode.value)
  
  // Load categories if not loaded
  if (!catalogStore.hasCategories) {
    await catalogStore.fetchCategories()
  }
  
  // Update page title and SEO
  const categoryName = category.value?.nombre || 'Categoría'
  companyStore.updateTitle(categoryName)
  
  // Update SEO for category
  if (category.value && companyStore.company) {
    setCategorySeo(category.value.nombre, companyStore.company)
  }
}

// Watch for route changes
watch(() => route.params.code, () => {
  loadCategory()
}, { immediate: true })

// Initialize
onMounted(async () => {
  await companyStore.init()
  await loadCategory()
})
</script>