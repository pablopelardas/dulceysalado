<!-- ProductGrid.vue - Grid de productos con mejor spacing -->
<template>
  <div class="py-6">
    <!-- Novedades y Ofertas Section -->
    <div class="mb-8 space-y-6">
      <!-- Novedades Section -->
      <section class="rounded-xl shadow-sm p-6" style="background-color: #1E1E1E;">
        <div class="text-center mb-12">
          <h2 class="text-3xl md:text-4xl font-bold text-white mb-4">
            🆕 Novedades
          </h2>
          <p class="text-lg text-gray-300">
            Los productos más nuevos en nuestro catálogo
          </p>
        </div>
        
        <ProductCarousel
          title=""
          :products="mockNovedades"
          :icon="null"
          :modal-open="false"
          @open-cart="openAddToCartModal"
        />
      </section>

      <!-- Ofertas Section -->
      <section class="rounded-xl shadow-sm p-6" style="background-color: #2A2A2A;">
        <div class="text-center mb-12">
          <h2 class="text-3xl md:text-4xl font-bold text-white mb-4">
            🔥 Ofertas Especiales
          </h2>
          <p class="text-lg text-gray-300">
            Los mejores precios que no podés dejar pasar
          </p>
        </div>
        
        <ProductCarousel
          title=""
          :products="mockOfertas"
          :icon="null"
          :modal-open="false"
          @open-cart="openAddToCartModal"
        />
      </section>
    </div>

    <!-- Categories Section -->
    <div class="categories-section mb-8 py-4 bg-white/10 backdrop-blur-sm rounded-xl">
      <div class="container py-6">
        <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-2">
          <!-- Show skeleton when loading categories -->
          <template v-if="catalogStore.loadingCategories">
            <CategoryChipSkeleton v-for="i in 12" :key="`chip-skeleton-${i}`" />
          </template>
          
          <!-- Show actual categories -->
          <template v-else>
            <button
              class="category-pill"
              :class="{ active: selectedCategory === null }"
              @click="setCategory(null)"
            >
              <span class="text-gray-800 font-medium">Todos</span>
              <span class="count">({{ catalogStore.totalCount }})</span>
            </button>
            
            <button
              v-for="category in catalogStore.displayCategories"
              :key="category.id"
              class="category-pill"
              :class="{ active: selectedCategory === category.codigo_rubro }"
              @click="setCategory(category.codigo_rubro)"
            >
              <span class="text-gray-800 font-medium">{{ category.nombre }}</span>
              <span class="count">({{ category.product_count || 0 }})</span>
            </button>
          </template>
        </div>
      </div>
    </div>
    
    <!-- Toolbar simplificado -->
    <div class="mb-6 pt-6 products-toolbar">
      <!-- Primera fila: Contador con página y controles -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-4">
        <!-- Contador y página -->
        <div class="flex items-center gap-6">
          <h2 class="text-xl font-semibold text-white">
            {{ displayProductCount }} productos
          </h2>
          
          <!-- Page indicator -->
          <div v-if="totalPages > 1" class="text-white/80 text-sm font-medium">
            Página {{ currentPage }} de {{ totalPages }}
          </div>
        </div>
        
        <!-- Controles -->
        <div class="flex items-center gap-3">
          <select 
            v-model="sortOrder"
            @change="handleSortChange"
            class="px-4 py-2 rounded-lg bg-white/90 text-gray-800 text-sm font-medium border-0 focus:ring-2 focus:ring-white/50 transition-all cursor-pointer"
          >
            <option value="nombre_asc">Alfabéticamente A-Z</option>
            <option value="nombre_desc">Alfabéticamente Z-A</option>
            <option value="precio_asc">Precio: menor a mayor</option>
            <option value="precio_desc">Precio: mayor a menor</option>
          </select>
          
          <div class="flex gap-1 bg-white/10 rounded-lg p-1">
            <button 
              @click="setViewMode('grid')"
              :class="viewMode === 'grid' ? 'bg-white/20' : ''"
              class="p-2 rounded transition-all cursor-pointer"
            >
              <ViewGridIcon class="h-5 w-5 text-white" />
            </button>
            <button 
              @click="setViewMode('list')"
              :class="viewMode === 'list' ? 'bg-white/20' : ''"
              class="p-2 rounded transition-all cursor-pointer"
            >
              <ListBulletIcon class="h-5 w-5 text-white" />
            </button>
          </div>
        </div>
      </div>
      
      <!-- Segunda fila: Filtros activos -->
      <div v-if="hasActiveFilters" class="space-y-3">
        <!-- Categoría destacada -->
        <div v-if="selectedCategory !== null" class="flex flex-col sm:flex-row sm:items-center gap-2 sm:gap-3">
          <span class="text-sm text-white/80 font-medium">Categoría:</span>
          <div class="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-white/95 shadow-lg max-w-full">
            <div class="w-2 h-2 rounded-full" :style="{ background: 'var(--theme-accent)' }"></div>
            <span class="text-base font-semibold text-gray-800">
              {{ getCategoryName(selectedCategory) }}
            </span>
            <button 
              @click="clearCategory" 
              class="ml-2 p-1 hover:bg-gray-200 rounded-full transition-colors cursor-pointer"
              title="Quitar filtro de categoría"
            >
              <XMarkIcon class="h-4 w-4 text-gray-600" />
            </button>
          </div>
        </div>
        
        <!-- Búsqueda activa -->
        <div v-if="searchQuery" class="flex flex-col sm:flex-row sm:items-center gap-2 sm:gap-3">
          <span class="text-sm text-white/80 font-medium">Búsqueda:</span>
          <div class="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-white/95 shadow-lg max-w-full">
            <svg class="w-4 h-4 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <span class="text-base font-medium text-gray-800">
              "{{ searchQuery }}"
            </span>
            <button 
              @click="clearSearch" 
              class="ml-2 p-1 hover:bg-gray-200 rounded-full transition-colors cursor-pointer"
              title="Quitar búsqueda"
            >
              <XMarkIcon class="h-4 w-4 text-gray-600" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Content -->
    <div class="products-grid-section">
      <!-- Loading state -->
      <div v-if="isLoading">
        <div 
          :class="viewMode === 'grid' 
            ? 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6' 
            : 'space-y-4'"
        >
          <ProductSkeleton v-for="i in 12" :key="`loading-product-${i}`" />
        </div>
      </div>
      
      <!-- Error state -->
      <div v-else-if="error" class="flex justify-center py-20">
        <div class="glass max-w-md p-8 rounded-xl text-center">
          <ExclamationTriangleIcon class="h-12 w-12 text-red-500 mx-auto mb-4" />
          <p class="text-gray-800 mb-4">{{ error }}</p>
          <button @click="retry" class="btn btn-primary cursor-pointer">
            Intentar nuevamente
          </button>
        </div>
      </div>
      
      <!-- Empty state -->
      <div v-else-if="showEmptyState" class="flex justify-center py-20">
        <div class="glass max-w-md p-8 rounded-xl text-center">
          <ShoppingBagIcon class="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <h3 class="text-lg font-semibold text-gray-800 mb-2">No se encontraron productos</h3>
          <p class="text-gray-600 mb-4">
            {{ hasActiveFilters ? 'Intenta con otros filtros' : 'No hay productos disponibles' }}
          </p>
          <button v-if="hasActiveFilters" @click="clearAllFilters" class="btn btn-primary cursor-pointer">
            Limpiar filtros
          </button>
        </div>
      </div>
      
      <!-- Products grid -->
      <div v-else>
        <div 
          :class="viewMode === 'grid' 
            ? 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6' 
            : 'grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4'"
        >
          <ProductCard
            v-for="product in products"
            :key="product.codigo"
            :product="product"
            :view-mode="viewMode"
            @open-cart="openAddToCartModal"
          />
        </div>
      </div>
      <!-- Paginación -->
      <div v-if="totalPages > 1" class="flex justify-center mt-10">
        <div class="glass px-6 py-4 rounded-xl">
          <Pagination
            :current-page="currentPage"
            :total-pages="totalPages"
            :has-next="hasNext"
            :has-prev="hasPrev"
            @next="nextPage"
            @prev="prevPage"
            @goto="goToPage"
          />
        </div>
      </div>
    </div>
    
    <!-- Modal is now handled globally in AppLayout -->
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch, inject } from 'vue'
// useRouter import removed - not used
import { useCatalogStore } from '@/stores/catalog'
import { 
  XMarkIcon,
  Squares2X2Icon as ViewGridIcon,
  ListBulletIcon,
  ExclamationTriangleIcon,
  ShoppingBagIcon
} from '@heroicons/vue/24/outline'
import ProductCard from './ProductCard.vue'
import ProductCarousel from './ProductCarousel.vue'
import ProductSkeleton from '@/components/ui/ProductSkeleton.vue'
import CategoryChipSkeleton from '@/components/ui/CategoryChipSkeleton.vue'
import Pagination from '@/components/ui/Pagination.vue'
// Modal is now handled globally
import type { Product } from '@/services/api'

// Stores
const catalogStore = useCatalogStore()
// router removed - not used currently

// Mock data para las secciones
const mockNovedades = ref<Product[]>([
  {
    codigo: 'NOV001',
    nombre: 'Alfajores de Maicena Premium',
    precio: 1200,
    imagen_urls: ['/placeholder.png'],
    descripcion: 'Deliciosos alfajores artesanales de maicena con dulce de leche',
    stock: 50,
    lista: 'minorista',
    marca: 'Dulce & Salado MAX',
    categoria: 'Alfajores',
    destacado: true,
    novedad: true
  },
  {
    codigo: 'NOV002',
    nombre: 'Chocolates Artesanales Mix',
    precio: 2800,
    imagen_urls: ['/placeholder.png'],
    descripcion: 'Variedad de chocolates artesanales premium',
    stock: 25,
    lista: 'minorista',
    marca: 'Dulce & Salado MAX',
    categoria: 'Chocolates',
    destacado: true,
    novedad: true
  },
  {
    codigo: 'NOV003',
    nombre: 'Galletitas Dulces Surtidas',
    precio: 850,
    imagen_urls: ['/placeholder.png'],
    descripcion: 'Mezcla de galletitas dulces variadas',
    stock: 40,
    lista: 'minorista',
    marca: 'Dulce & Salado MAX',
    categoria: 'Galletitas',
    destacado: true,
    novedad: true
  },
  {
    codigo: 'NOV004',
    nombre: 'Snacks Salados Premium',
    precio: 1500,
    imagen_urls: ['/placeholder.png'],
    descripcion: 'Selección de snacks salados gourmet',
    stock: 35,
    lista: 'minorista',
    marca: 'Dulce & Salado MAX',
    categoria: 'Snacks',
    destacado: true,
    novedad: true
  },
  {
    codigo: 'NOV005',
    nombre: 'Turrones Artesanales',
    precio: 2200,
    imagen_urls: ['/placeholder.png'],
    descripcion: 'Turrones caseros de almendra y miel',
    stock: 15,
    lista: 'minorista',
    marca: 'Dulce & Salado MAX',
    categoria: 'Turrones',
    destacado: true,
    novedad: true
  },
  {
    codigo: 'NOV006',
    nombre: 'Bombones Premium',
    precio: 3500,
    imagen_urls: ['/placeholder.png'],
    descripcion: 'Exquisitos bombones rellenos de licor',
    stock: 20,
    lista: 'minorista',
    marca: 'Dulce & Salado MAX',
    categoria: 'Bombones',
    destacado: true,
    novedad: true
  }
])

const mockOfertas = ref<Product[]>([
  {
    codigo: 'OFF001',
    nombre: 'Pack Golosinas x12',
    precio: 1800,
    precio_anterior: 2400,
    imagen_urls: ['/placeholder.png'],
    descripcion: 'Pack especial de 12 golosinas variadas - 25% OFF',
    stock: 20,
    lista: 'minorista',
    marca: 'Dulce & Salado MAX',
    categoria: 'Golosinas',
    destacado: true,
    oferta: true
  },
  {
    codigo: 'OFF002',
    nombre: 'Combo Dulce & Salado',
    precio: 3200,
    precio_anterior: 4000,
    imagen_urls: ['/placeholder.png'],
    descripcion: 'Combo especial mitad dulce, mitad salado - 20% OFF',
    stock: 15,
    lista: 'minorista',
    marca: 'Dulce & Salado MAX',
    categoria: 'Combos',
    destacado: true,
    oferta: true
  },
  {
    codigo: 'OFF003',
    nombre: 'Caramelos Premium x100',
    precio: 950,
    precio_anterior: 1200,
    imagen_urls: ['/placeholder.png'],
    descripcion: 'Bolsa de 100 caramelos premium surtidos',
    stock: 60,
    lista: 'minorista',
    marca: 'Dulce & Salado MAX',
    categoria: 'Caramelos',
    destacado: true,
    oferta: true
  },
  {
    codigo: 'OFF004',
    nombre: 'Frutos Secos Mix 500g',
    precio: 2100,
    precio_anterior: 2800,
    imagen_urls: ['/placeholder.png'],
    descripcion: 'Mezcla premium de frutos secos 500g - 25% OFF',
    stock: 30,
    lista: 'minorista',
    marca: 'Dulce & Salado MAX',
    categoria: 'Frutos Secos',
    destacado: true,
    oferta: true
  },
  {
    codigo: 'OFF005',
    nombre: 'Mega Pack Dulces x24',
    precio: 2700,
    precio_anterior: 3600,
    imagen_urls: ['/placeholder.png'],
    descripcion: 'Pack familiar de 24 dulces variados - 25% OFF',
    stock: 12,
    lista: 'minorista',
    marca: 'Dulce & Salado MAX',
    categoria: 'Packs',
    destacado: true,
    oferta: true
  },
  {
    codigo: 'OFF006',
    nombre: 'Chocolates Premium 1kg',
    precio: 4500,
    precio_anterior: 6000,
    imagen_urls: ['/placeholder.png'],
    descripcion: 'Caja de chocolates premium 1kg - 25% OFF',
    stock: 8,
    lista: 'minorista',
    marca: 'Dulce & Salado MAX',
    categoria: 'Chocolates',
    destacado: true,
    oferta: true
  }
])


// State
const searchQuery = ref('')
const selectedCategory = ref<number | null>(null)
const showFeaturedOnly = ref(false)

// Load view mode from localStorage or default to 'grid'
const getStoredViewMode = (): 'grid' | 'list' => {
  try {
    const stored = localStorage.getItem('catalog-view-mode')
    return (stored === 'grid' || stored === 'list') ? stored : 'grid'
  } catch {
    return 'grid'
  }
}

const viewMode = ref<'grid' | 'list'>(getStoredViewMode())
const sortOrder = ref('nombre_asc')

// Inject global cart modal
const openCartModal = inject('openCartModal') as ((product: any) => void) | undefined

// State
const isClearing = ref(false)
const isInitialMount = ref(true)

// Computed
const products = computed(() => catalogStore.filteredProducts)

const loading = computed(() => catalogStore.loadingProducts)
const error = computed(() => catalogStore.productsError)
const hasProducts = computed(() => catalogStore.hasProducts)
const currentPage = computed(() => catalogStore.currentPage)
const totalPages = computed(() => catalogStore.totalPages)
// totalCount computed property removed - not used

// Display the current filtered count (what's actually being shown)
const displayProductCount = computed(() => 
  catalogStore.searchQuery ? catalogStore.filteredProductsCount : catalogStore.totalCount
)
const hasNext = computed(() => catalogStore.hasNextPage)
const hasPrev = computed(() => catalogStore.hasPrevPage)

const hasActiveFilters = computed(() => 
  selectedCategory.value !== null || 
  searchQuery.value.length > 0 || 
  showFeaturedOnly.value
)

// Add a computed to track if we should show empty state
const showEmptyState = computed(() => 
  !catalogStore.initializing && !loading.value && !error.value && !hasProducts.value
)

// Check if we're in initial loading state (show skeletons for everything)
// isInitialLoading computed property removed - not used

// Update loading to include initial loading
const isLoading = computed(() => loading.value || catalogStore.initializing)



// Methods
const setViewMode = (mode: 'grid' | 'list') => {
  viewMode.value = mode
  try {
    localStorage.setItem('catalog-view-mode', mode)
  } catch (error) {
    console.warn('Could not save view mode to localStorage:', error)
  }
}

const getCategoryName = (categoryId: number) => {
  const category = catalogStore.getCategoryByCode(categoryId)
  return category?.nombre || 'Categoría'
}

const clearCategory = () => {
  isClearing.value = true
  selectedCategory.value = null
  catalogStore.setCategory(null, false) // Don't reset page when clearing
  fetchProducts()
  // Scroll to categories when clearing category filter
  requestAnimationFrame(scrollToCategories)
  // Reset flag after a short delay to allow for state changes
  setTimeout(() => {
    isClearing.value = false
  }, 100)
}

const clearSearch = () => {
  isClearing.value = true
  searchQuery.value = ''
  catalogStore.setSearch('', false) // Don't reset page when clearing
  fetchProducts()
  // Scroll to categories when clearing search
  requestAnimationFrame(scrollToCategories)
  // Reset flag after a short delay to allow for state changes
  setTimeout(() => {
    isClearing.value = false
  }, 100)
}

const clearAllFilters = () => {
  isClearing.value = true
  searchQuery.value = ''
  selectedCategory.value = null
  showFeaturedOnly.value = false
  catalogStore.clearFilters()
  fetchProducts()
  // Scroll to categories when clearing all filters
  requestAnimationFrame(scrollToCategories)
  // Reset flag after a short delay to allow for state changes
  setTimeout(() => {
    isClearing.value = false
  }, 100)
}

const scrollToProducts = () => {
  // Find the products toolbar (contains count and sorting)
  const toolbarElement = document.querySelector('.products-toolbar')
  if (toolbarElement) {
    const yOffset = -100; // Scroll a bit higher to account for fixed header and better visibility
    const y = toolbarElement.getBoundingClientRect().top + window.pageYOffset + yOffset;
    
    // Use the same fast custom scroll animation
    const startY = window.pageYOffset;
    const distance = y - startY;
    const duration = 300; // Same 300ms for consistency
    let start: number | null = null;
    
    const step = (timestamp: number) => {
      if (!start) start = timestamp;
      const progress = Math.min((timestamp - start) / duration, 1);
      
      // Same easing function for consistency
      const easeInOutCubic = progress < 0.5
        ? 4 * progress * progress * progress
        : 1 - Math.pow(-2 * progress + 2, 3) / 2;
      
      window.scrollTo(0, startY + distance * easeInOutCubic);
      
      if (progress < 1) {
        requestAnimationFrame(step);
      }
    };
    
    requestAnimationFrame(step);
  }
}

const scrollToCategories = () => {
  // Find the categories section in the ProductGrid
  const categoriesSection = document.querySelector('.categories-section')
  if (categoriesSection) {
    const yOffset = -120; // Account for fixed header (80px) + extra breathing room
    const y = categoriesSection.getBoundingClientRect().top + window.pageYOffset + yOffset;
    
    // Use the same animation as scrollToProducts for consistency
    const startY = window.pageYOffset;
    const distance = y - startY;
    const duration = 300;
    let start: number | null = null;
    
    const step = (timestamp: number) => {
      if (!start) start = timestamp;
      const progress = Math.min((timestamp - start) / duration, 1);
      
      const easeInOutCubic = progress < 0.5
        ? 4 * progress * progress * progress
        : 1 - Math.pow(-2 * progress + 2, 3) / 2;
      
      window.scrollTo(0, startY + distance * easeInOutCubic);
      
      if (progress < 1) {
        requestAnimationFrame(step);
      }
    };
    
    requestAnimationFrame(step);
  }
}

const nextPage = () => {
  catalogStore.nextPage()
  fetchProducts()
  requestAnimationFrame(scrollToProducts)
}

const prevPage = () => {
  catalogStore.prevPage()
  fetchProducts()
  requestAnimationFrame(scrollToProducts)
}

const goToPage = (page: number | string) => {
  const pageNum = typeof page === 'string' ? parseInt(page) : page
  catalogStore.goToPage(pageNum)
  fetchProducts()
  requestAnimationFrame(scrollToProducts)
}

// Cart modal methods
const openAddToCartModal = (product: any) => {
  if (openCartModal) {
    openCartModal(product)
  }
}

// handleProductClick removed - using direct navigation in template

const fetchProducts = async () => {
  await catalogStore.fetchProducts()
}

const retry = () => {
  fetchProducts()
}

const handleSortChange = async () => {
  const sortValue = sortOrder.value || null
  await catalogStore.setSortBy(sortValue as any)
}

const setCategory = async (categoryId: number | null) => {
  selectedCategory.value = categoryId
  await catalogStore.setCategory(categoryId)
}

const syncFiltersFromStore = () => {
  searchQuery.value = catalogStore.searchQuery
  selectedCategory.value = catalogStore.selectedCategory
  showFeaturedOnly.value = catalogStore.showFeaturedOnly
  sortOrder.value = catalogStore.sortBy || 'nombre_asc'
}

// Watch for store changes to keep UI in sync
watch([
  () => catalogStore.searchQuery,
  () => catalogStore.selectedCategory,
  () => catalogStore.showFeaturedOnly,
  () => catalogStore.sortBy
], ([newSearch, newCategory, newFeatured], [, oldCategory]) => {
  syncFiltersFromStore()
  
  // Scroll behavior when category changes
  if (newCategory !== oldCategory && !isClearing.value && !isInitialMount.value) {
    requestAnimationFrame(() => {
      console.log('Watcher new category', newCategory, oldCategory)
      if (newCategory === null) {
        // When going to "Todos", scroll to categories
        scrollToCategories();
      } else {
        // When selecting a specific category, scroll to products
        scrollToProducts();
      }
    });
  } else if (isInitialMount.value && (newCategory !== null || newSearch || newFeatured)) {
    // Only scroll on initial mount if there are active filters from URL
    requestAnimationFrame(() => {
      scrollToProducts();
    });
  }
  
  // Clear initial mount flag after first watch execution
  if (isInitialMount.value) {
    isInitialMount.value = false
  }
}, { immediate: true })

// Products loading is now handled by the global initializing state



// Initialize
onMounted(async () => {
  // Company and categories are already initialized in App.vue
  // Force sync from store first
  syncFiltersFromStore()
  // Don't fetch products here - let Catalog.vue handle it
})

onUnmounted(() => {
  // No cleanup needed for carousels since we use ProductCarousel component
})
</script>

<style scoped>
@reference "tailwindcss";

.category-pill {
  @apply flex items-center gap-2 px-3 py-2 rounded-full text-gray-800 text-sm font-medium transition-all justify-center min-h-[2.5rem] cursor-pointer;
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
  @apply text-lg font-bold flex-shrink-0 text-white;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.3);
}

</style>