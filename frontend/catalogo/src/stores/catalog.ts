import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import { apiService, type Product, type Category, type CatalogFilters } from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import EMPRESA_CONFIG from '@/config/empresa.config'

export const useCatalogStore = defineStore('catalog', () => {
  // Get auth store and ensure it's initialized
  const authStore = useAuthStore()
  
  // Initialize auth on store creation if not already done
  if (!authStore.token && !authStore.user) {
    console.log('🔍 catalogStore: Initializing auth store')
    authStore.initializeAuth()
  }
  
  // State
  const products = ref<Product[]>([])
  const featuredProducts = ref<Product[]>([])
  const novedades = ref<Product[]>([])
  const ofertas = ref<Product[]>([])
  const categories = ref<Category[]>([])
  const filteredCategories = ref<Category[]>([])
  const currentProduct = ref<Product | null>(null)
  
  // Pagination
  const currentPage = ref(1)
  const pageSize = ref(EMPRESA_CONFIG.productosPorPagina)
  const totalCount = ref(0)
  const totalPages = ref(0)
  const originalTotalCount = ref(0) // Total original sin filtros
  
  // Filters
  const selectedCategory = ref<number | null>(null)
  const searchQuery = ref('')
  const selectedPriceList = ref<string | null>(null)
  const showFeaturedOnly = ref(false)
  const sortBy = ref<'precio_asc' | 'precio_desc' | 'nombre_asc' | 'nombre_desc' | null>('nombre_asc')
  
  // Loading states
  const loadingProducts = ref(false)
  const loadingCategories = ref(false)
  const loadingFeatured = ref(false)
  const loadingProduct = ref(false)
  const loadingNovedades = ref(false)
  const loadingOfertas = ref(false)
  const initializing = ref(true) // Global initialization state
  
  // AbortController for cancelling requests
  let currentProductsController: AbortController | null = null
  
  // Cache system for search results
  const searchCache = ref<Map<string, CatalogResponse>>(new Map())
  const CACHE_SIZE_LIMIT = 50 // Maximum number of cached searches
  
  // Errors
  const productsError = ref<string | null>(null)
  const categoriesError = ref<string | null>(null)
  const featuredError = ref<string | null>(null)
  const productError = ref<string | null>(null)

  // Getters
  const hasProducts = computed(() => filteredProducts.value.length > 0)
  const hasCategories = computed(() => categories.value.length > 0)
  
  // Client-side filtered products when there's a search query
  const filteredProducts = computed(() => {
    // If there's no search query, return all products
    if (!searchQuery.value) {
      return products.value
    }
    
    // If there's a search query, filter by category on client-side
    if (selectedCategory.value) {
      return products.value.filter(product => product.codigo_rubro === selectedCategory.value)
    }
    
    return products.value
  })
  
  // Update product count based on filtered products
  const filteredProductsCount = computed(() => {
    if (!searchQuery.value) {
      return totalCount.value
    }
    return filteredProducts.value.length
  })
  const hasFeaturedProducts = computed(() => featuredProducts.value.length > 0)
  const hasNextPage = computed(() => currentPage.value < totalPages.value)
  const hasPrevPage = computed(() => currentPage.value > 1)
  
  // Get categories to display - filtered only when searching, all categories for other filters
  const displayCategories = computed(() => {
    return searchQuery.value 
      ? filteredCategories.value 
      : categories.value
  })
  
  // Computed para determinar qué lista de precios usar
  const effectivePriceList = computed(() => {
    console.log('🔍 effectivePriceList check:')
    console.log('  - isAuthenticated:', authStore.isAuthenticated)
    console.log('  - user.lista_precio:', authStore.user?.lista_precio)
    console.log('  - user.lista_precio?.codigo:', authStore.user?.lista_precio?.codigo)
    
    // Si el usuario está autenticado y tiene lista de precios, usarla
    if (authStore.isAuthenticated && authStore.user?.lista_precio?.codigo) {
      const listCodigo = authStore.user.lista_precio.codigo
      console.log('🔍 effectivePriceList: Using user price list:', listCodigo, 'for user:', authStore.user.codigo)
      return listCodigo
    }
    // Si no, dejar que el backend use la predeterminada (no enviar parámetro)
    console.log('🔍 effectivePriceList: No user authenticated or no price list, using default')
    return null
  })
  
  const currentFilters = computed((): CatalogFilters => {
    const filters = {
      listaPrecioCodigo: effectivePriceList.value || undefined,
      busqueda: searchQuery.value || undefined,
      destacados: showFeaturedOnly.value || undefined,
      codigoRubro: selectedCategory.value || undefined,
      ordenarPor: sortBy.value || undefined,
      page: currentPage.value,
      pageSize: pageSize.value
    }
    console.log('🔍 currentFilters:', filters)
    return filters
  })

  // Get category by ID
  const getCategoryById = computed(() => {
    return (id: number) => categories.value.find(cat => cat.id === id)
  })

  // Get category by code
  const getCategoryByCode = computed(() => {
    return (code: number) => categories.value.find(cat => cat.codigo_rubro === code)
  })

  // Helper function to generate cache key from filters
  const generateCacheKey = (filters: CatalogFilters): string => {
    const sortedEntries = Object.entries(filters)
      .filter(([, value]) => value !== undefined && value !== null)
      .sort(([a], [b]) => a.localeCompare(b))
    return JSON.stringify(sortedEntries)
  }
  
  // Helper function to manage cache size
  const manageCacheSize = () => {
    if (searchCache.value.size >= CACHE_SIZE_LIMIT) {
      // Remove oldest entries (first ones added)
      const keysToRemove = Array.from(searchCache.value.keys()).slice(0, 10)
      keysToRemove.forEach(key => searchCache.value.delete(key))
    }
  }
  
  // Actions
  const fetchOriginalTotalCount = async () => {
    try {
      const response = await apiService.getCatalog({
        listaPrecioCodigo: effectivePriceList.value || undefined,
        page: 1,
        pageSize: 1 // Solo necesitamos el total_count
      })
      
      if (response.data) {
        originalTotalCount.value = response.data.total_count
      }
    } catch (err) {
      console.error('Error fetching original total count:', err)
    }
  }

  const fetchProducts = async (filters: CatalogFilters = {}) => {
    const mergedFilters = { ...currentFilters.value, ...filters }
    console.log('fetchProducts called with mergedFilters:', mergedFilters)
    const cacheKey = generateCacheKey(mergedFilters)
    
    // Check cache first
    if (searchCache.value.has(cacheKey)) {
      const cachedData = searchCache.value.get(cacheKey) as CatalogResponse
      
      // Update state with cached data
      products.value = cachedData.productos
      totalCount.value = cachedData.total_count
      totalPages.value = cachedData.total_pages
      currentPage.value = cachedData.page
      pageSize.value = cachedData.page_size
      
      // Update filtered categories only when there's a search query
      if (cachedData.categorias && searchQuery.value) {
        filteredCategories.value = cachedData.categorias.filter(category => category.product_count > 0)
      }
      
      return // Return early, no need to make API call
    }
    
    // Cancel previous request if still pending
    if (currentProductsController) {
      currentProductsController.abort()
    }
    
    // Create new AbortController for this request
    currentProductsController = new AbortController()
    const signal = currentProductsController.signal
    
    loadingProducts.value = true
    productsError.value = null
    
    try {
      const response = await apiService.getCatalog(mergedFilters, signal)
      
      // Check if request was cancelled
      if (signal.aborted) {
        return
      }
      
      if (response.error) {
        productsError.value = response.error
        return
      }
      
      if (response.data) {
        // Store in cache before updating state
        manageCacheSize()
        searchCache.value.set(cacheKey, response.data)
        
        products.value = response.data.productos
        totalCount.value = response.data.total_count
        totalPages.value = response.data.total_pages
        currentPage.value = response.data.page
        pageSize.value = response.data.page_size
        
        // Store original total count only on first load (no filters) or if not set yet
        if ((!selectedCategory.value && !searchQuery.value && !showFeaturedOnly.value) || originalTotalCount.value === 0) {
          originalTotalCount.value = response.data.total_count
        }
        
        // Update filtered categories only when there's a search query
        if (response.data.categorias && searchQuery.value) {
          // Filter to show only categories with products (product_count > 0)
          filteredCategories.value = response.data.categorias.filter(category => category.product_count > 0)
        }
      }
    } catch (err) {
      // Don't show error if request was cancelled
      if (err instanceof Error && err.name === 'AbortError') {
        return
      }
      productsError.value = err instanceof Error ? err.message : 'Error loading products'
    } finally {
      // Only clear loading if this request wasn't cancelled
      if (!signal.aborted) {
        loadingProducts.value = false
      }
    }
  }

  const fetchCategories = async () => {
    loadingCategories.value = true
    categoriesError.value = null
    
    try {
      const response = await apiService.getCategories()
      
      if (response.error) {
        categoriesError.value = response.error
        return
      }
      
      if (response.data) {
        categories.value = response.data.categorias
      }
    } catch (err) {
      categoriesError.value = err instanceof Error ? err.message : 'Error loading categories'
    } finally {
      loadingCategories.value = false
    }
  }

  const fetchFeaturedProducts = async (limit: number = 10) => {
    loadingFeatured.value = true
    featuredError.value = null
    
    try {
      const response = await apiService.getFeaturedProducts(effectivePriceList.value || undefined, limit)
      
      if (response.error) {
        featuredError.value = response.error
        return
      }
      
      if (response.data) {
        featuredProducts.value = response.data.productos
      }
    } catch (err) {
      featuredError.value = err instanceof Error ? err.message : 'Error loading featured products'
    } finally {
      loadingFeatured.value = false
    }
  }

  const fetchProduct = async (codigo: string) => {
    loadingProduct.value = true
    productError.value = null
    
    try {
      const response = await apiService.getProduct(codigo, effectivePriceList.value || undefined)
      
      if (response.error) {
        productError.value = response.error
        return
      }
      
      if (response.data) {
        currentProduct.value = response.data
      }
    } catch (err) {
      productError.value = err instanceof Error ? err.message : 'Error loading product'
    } finally {
      loadingProduct.value = false
    }
  }

  // Filter actions
  const setCategory = async (categoryId: number | null, resetPage: boolean = true) => {
    console.log('setCategory called with:', categoryId, resetPage)
    selectedCategory.value = categoryId
    if (resetPage) {
      currentPage.value = 1 // Reset to first page when filtering
    }
    
    // If we don't have original total count, fetch it first
    if (originalTotalCount.value === 0) {
      await fetchOriginalTotalCount()
    }
    
    // Only fetch products if there's no active search query
    // When searching, the category filter will be applied client-side
    if (!searchQuery.value) {
      await fetchProducts() // Fetch products with new filter
    }
  }

  const setSearch = async (query: string, resetPage: boolean = true) => {
    searchQuery.value = query
    if (resetPage) {
      currentPage.value = 1 // Reset to first page when searching
    }
    
    // If we don't have original total count, fetch it first
    if (originalTotalCount.value === 0) {
      await fetchOriginalTotalCount()
    }
    
    await fetchProducts() // Fetch products with new search
  }

  const setPriceList = async (_priceListId: string | null) => {
    // Esta función ya no se usa, pero la mantenemos por compatibilidad
    console.log('setPriceList deprecated - price list is now automatic based on user authentication')
  }

  const setFeaturedOnly = async (featured: boolean) => {
    showFeaturedOnly.value = featured
    currentPage.value = 1
    await fetchProducts()
  }

  const setSortBy = async (sort: 'precio_asc' | 'precio_desc' | 'nombre_asc' | 'nombre_desc' | null) => {
    sortBy.value = sort
    currentPage.value = 1
    await fetchProducts()
  }

  const clearFilters = async () => {
    selectedCategory.value = null
    searchQuery.value = ''
    showFeaturedOnly.value = false
    selectedPriceList.value = null
    sortBy.value = 'nombre_asc' // Mantener ordenamiento alfabético por defecto
    currentPage.value = 1
    filteredCategories.value = [] // Clear filtered categories
    await fetchProducts()
  }
  
  // Cache management functions
  const clearCache = () => {
    searchCache.value.clear()
  }
  
  const getCacheSize = () => {
    return searchCache.value.size
  }


  // Pagination actions
  const nextPage = () => {
    if (hasNextPage.value) {
      currentPage.value++
    }
  }

  const prevPage = () => {
    if (hasPrevPage.value) {
      currentPage.value--
    }
  }

  const goToPage = (page: number) => {
    if (page >= 1 && page <= totalPages.value) {
      currentPage.value = page
    }
  }


  // Fetch novedades
  const fetchNovedades = async (): Promise<void> => {
    if (loadingNovedades.value) return
    
    loadingNovedades.value = true
    
    try {
      const result = await apiService.getNovedades()
      if (result.data) {
        novedades.value = result.data
      }
    } catch (error) {
      console.error('Error fetching novedades:', error)
      novedades.value = []
    } finally {
      loadingNovedades.value = false
    }
  }

  // Fetch ofertas
  const fetchOfertas = async (): Promise<void> => {
    if (loadingOfertas.value) return
    
    loadingOfertas.value = true
    
    try {
      const result = await apiService.getOfertas()
      if (result.data) {
        ofertas.value = result.data
      }
    } catch (error) {
      console.error('Error fetching ofertas:', error)
      ofertas.value = []
    } finally {
      loadingOfertas.value = false
    }
  }

  // Watcher para actualizar cuando cambia el usuario (login/logout)
  watch(
    () => authStore.user,
    async (newUser, oldUser) => {
      console.log('🔍 User watcher triggered - oldUser:', oldUser?.codigo, 'newUser:', newUser?.codigo)
      
      // Refrescar el catálogo si la lista de precios del usuario cambió
      const oldListCodigo = oldUser?.lista_precio?.codigo
      const newListCodigo = newUser?.lista_precio?.codigo
      
      if (oldListCodigo !== newListCodigo) {
        console.log('🔍 User price list changed from', oldListCodigo, 'to', newListCodigo, '- refreshing catalog')
        await fetchProducts()
      }
    }
  )
  
  // Watcher adicional para detectar cuando el usuario se autentica inicialmente
  watch(
    () => authStore.isAuthenticated,
    async (isAuth, wasAuth) => {
      console.log('🔍 Auth status changed from', wasAuth, 'to', isAuth)
      if (isAuth && !wasAuth && authStore.user?.lista_precio?.codigo) {
        console.log('🔍 User just logged in, refreshing catalog with price list:', authStore.user.lista_precio.codigo)
        await fetchProducts()
      }
    }
  )
  
  // Initialize all required data (except products which are fetched separately)
  const initializeAll = async (): Promise<void> => {
    initializing.value = true
    
    try {
      // Fetch all initial data in parallel
      await Promise.all([
        fetchCategories(),
        fetchNovedades(),
        fetchOfertas()
      ])
    } catch (error) {
      console.error('Error during initialization:', error)
    } finally {
      initializing.value = false
    }
  }

  // Initialize with company's default page size
  const initWithCompany = () => {
    // Ya se inicializa con el valor de EMPRESA_CONFIG
    pageSize.value = EMPRESA_CONFIG.productosPorPagina
  }

  return {
    // State
    products,
    filteredProducts,
    featuredProducts,
    novedades,
    ofertas,
    categories,
    filteredCategories,
    displayCategories,
    currentProduct,
    currentPage,
    pageSize,
    totalCount,
    filteredProductsCount,
    totalPages,
    originalTotalCount,
    selectedCategory,
    searchQuery,
    selectedPriceList,
    showFeaturedOnly,
    sortBy,
    
    // Loading states
    loadingProducts,
    loadingCategories,
    loadingFeatured,
    loadingProduct,
    loadingNovedades,
    loadingOfertas,
    initializing,
    
    // Errors
    productsError,
    categoriesError,
    featuredError,
    productError,
    
    // Getters
    hasProducts,
    hasCategories,
    hasFeaturedProducts,
    hasNextPage,
    hasPrevPage,
    currentFilters,
    getCategoryById,
    getCategoryByCode,
    
    // Actions
    fetchProducts,
    fetchCategories,
    fetchFeaturedProducts,
    fetchProduct,
    fetchOriginalTotalCount,
    fetchNovedades,
    fetchOfertas,
    initializeAll,
    setCategory,
    setSearch,
    setPriceList,
    setFeaturedOnly,
    setSortBy,
    nextPage,
    prevPage,
    goToPage,
    clearFilters,
    clearCache,
    getCacheSize,
    initWithCompany
  }
})