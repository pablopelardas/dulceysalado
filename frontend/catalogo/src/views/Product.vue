<template>
  <AppLayout>
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- Breadcrumb -->
      <nav class="flex mb-8" aria-label="Breadcrumb">
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
          <li v-if="productCategory" class="flex items-center">
            <ChevronRightIcon class="flex-shrink-0 h-4 w-4 text-gray-400 mx-2" />
            <RouterLink 
              :to="`/category/${productCategory.codigo_rubro}`"
              class="text-gray-500 hover:text-[--theme-primary] transition-colors"
            >
              {{ productCategory.nombre }}
            </RouterLink>
          </li>
          <li class="flex items-center">
            <ChevronRightIcon class="flex-shrink-0 h-4 w-4 text-gray-400 mx-2" />
            <span class="text-gray-900 font-medium">
              {{ product?.nombre || 'Producto' }}
            </span>
          </li>
        </ol>
      </nav>
      
      <!-- Product Detail -->
      <ProductDetail />
      
      <!-- Productos relacionados -->
      <div v-if="relatedProducts.length > 0" class="mt-16">
        <h2 class="text-2xl font-bold text-gray-900 mb-6">Productos relacionados</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          <ProductCard
            v-for="relatedProduct in relatedProducts"
            :key="relatedProduct.codigo"
            :product="relatedProduct"
          />
        </div>
      </div>
    </div>
  </AppLayout>
</template>

<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { RouterLink } from 'vue-router'
import { useCompanyStore } from '@/stores/company'
import { useCatalogStore } from '@/stores/catalog'
import { useSeo } from '@/composables/useSeo'
import { ChevronRightIcon } from '@heroicons/vue/24/outline'
import AppLayout from '@/components/layout/AppLayout.vue'
import ProductDetail from '@/components/catalog/ProductDetail.vue'
import ProductCard from '@/components/catalog/ProductCard.vue'

// Composables
const route = useRoute()
const companyStore = useCompanyStore()
const catalogStore = useCatalogStore()
const { setProductSeo } = useSeo()

// Computed
const product = computed(() => catalogStore.currentProduct)

const productCategory = computed(() => {
  if (!product.value?.codigo_rubro) return null
  return catalogStore.getCategoryByCode(product.value.codigo_rubro)
})

const relatedProducts = computed(() => {
  if (!product.value) return []
  
  // Get products from the same category, excluding current product
  return catalogStore.products
    .filter(p => 
      p.codigo_rubro === product.value?.codigo_rubro && 
      p.codigo !== product.value?.codigo
    )
    .slice(0, 4)
})

// Methods
const loadRelatedProducts = async () => {
  if (!product.value?.codigo_rubro) return
  
  // If we don't have products from this category, fetch them
  const categoryProducts = catalogStore.products.filter(p => 
    p.codigo_rubro === product.value?.codigo_rubro
  )
  
  if (categoryProducts.length === 0) {
    await catalogStore.fetchProducts({
      codigoRubro: product.value.codigo_rubro,
      pageSize: 12
    })
  }
}

// Watch for product changes to load related products and update SEO
watch(product, async (newProduct) => {
  if (newProduct && companyStore.company) {
    await loadRelatedProducts()
    // Update SEO for product
    setProductSeo(newProduct.nombre, companyStore.company, newProduct.imagen_urls[0])
  }
}, { immediate: true })

// Initialize
onMounted(async () => {
  await companyStore.init()
  
  // Load categories if not loaded
  if (!catalogStore.hasCategories) {
    await catalogStore.fetchCategories()
  }
})
</script>