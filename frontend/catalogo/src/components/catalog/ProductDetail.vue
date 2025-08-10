<template>
  <div v-if="loading" class="flex justify-center py-12">
    <LoadingSpinner size="lg" text="Cargando producto..." />
  </div>
  
  <div v-else-if="error" class="text-center py-12">
    <div class="text-red-600 mb-4">{{ error }}</div>
    <button
      @click="retry"
      class="bg-[--theme-primary] text-white px-4 py-2 rounded-md hover:bg-[--theme-primary-dark] transition-colors"
    >
      Intentar nuevamente
    </button>
  </div>
  
  <div v-else-if="product" class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 p-6">
      <!-- Galería de imágenes -->
      <div class="space-y-4">
        <div class="aspect-square bg-gray-50 rounded-lg overflow-hidden">
          <img
            v-if="currentImage"
            :src="currentImage"
            :alt="product.imagen_alt || product.nombre"
            class="w-full h-full object-cover"
            @error="handleImageError"
          />
          <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
            <PhotoIcon class="h-16 w-16" />
          </div>
        </div>
        
        <!-- Thumbnails -->
        <div v-if="product.imagen_urls && product.imagen_urls.length > 1" class="flex space-x-2 overflow-x-auto">
          <button
            v-for="(url, index) in product.imagen_urls"
            :key="index"
            @click="currentImageIndex = index"
            class="flex-shrink-0 w-16 h-16 bg-gray-50 rounded-md overflow-hidden border-2 transition-colors"
            :class="currentImageIndex === index ? 'border-[--theme-primary]' : 'border-gray-200 hover:border-gray-300'"
          >
            <img
              :src="url"
              :alt="`${product.nombre} - ${index + 1}`"
              class="w-full h-full object-cover"
            />
          </button>
        </div>
      </div>
      
      <!-- Información del producto -->
      <div class="space-y-6">
        <!-- Header -->
        <div>
          <div class="flex items-center justify-between mb-2">
            <span v-if="product.marca" class="text-sm text-gray-500 font-medium">
              {{ product.marca }}
            </span>
            <span class="text-sm text-gray-400">
              #{{ product.codigo }}
            </span>
          </div>
          
          <h1 class="text-2xl font-bold text-gray-900 mb-2">
            {{ product.nombre }}
          </h1>
          
          <div v-if="product.destacado" class="mb-4">
            <span class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-[--theme-primary] text-white">
              <StarIcon class="h-4 w-4 mr-1" />
              Producto Destacado
            </span>
          </div>
        </div>
        
        <!-- Precio -->
        <div v-if="showPrices && product.precio !== null" class="space-y-2">
          <div class="text-3xl font-bold text-[--theme-primary]">
            ${{ formatPrice(product.precio) }}
          </div>
          <div v-if="product.lista_precio_nombre" class="text-sm text-gray-600">
            {{ product.lista_precio_nombre }} ({{ product.lista_precio_codigo }})
          </div>
        </div>
        
        <!-- Stock -->
        <div v-if="showStock && product.stock !== null" class="flex items-center space-x-2">
          <span class="text-gray-600">Stock disponible:</span>
          <span class="font-medium" :class="stockColorClass">
            {{ product.stock }} {{ product.unidad }}
          </span>
        </div>
        
        <!-- Descripción -->
        <div v-if="product.descripcion" class="space-y-2">
          <h3 class="text-lg font-semibold text-gray-900">Descripción</h3>
          <p class="text-gray-700 leading-relaxed">{{ product.descripcion }}</p>
        </div>
        
        <!-- Detalles adicionales -->
        <div class="space-y-3">
          <h3 class="text-lg font-semibold text-gray-900">Detalles</h3>
          <dl class="grid grid-cols-1 gap-2 text-sm">
            <div v-if="product.unidad" class="flex justify-between">
              <dt class="text-gray-600">Unidad:</dt>
              <dd class="font-medium">{{ product.unidad }}</dd>
            </div>
            <div v-if="product.codigo_barras" class="flex justify-between">
              <dt class="text-gray-600">Código de barras:</dt>
              <dd class="font-medium">{{ product.codigo_barras }}</dd>
            </div>
            <div class="flex justify-between">
              <dt class="text-gray-600">Tipo:</dt>
              <dd class="font-medium capitalize">{{ product.tipo_producto }}</dd>
            </div>
          </dl>
        </div>
        
        <!-- Tags -->
        <div v-if="product.tags && product.tags.length > 0" class="space-y-2">
          <h3 class="text-lg font-semibold text-gray-900">Etiquetas</h3>
          <div class="flex flex-wrap gap-2">
            <span
              v-for="tag in product.tags"
              :key="tag"
              class="px-3 py-1 bg-gray-100 text-gray-700 rounded-full text-sm"
            >
              {{ tag }}
            </span>
          </div>
        </div>
        
        <!-- Botones de acción -->
        <div class="space-y-3 pt-4 border-t">
          <button
            v-if="allowOrders && product.stock !== 0"
            @click="addToCart"
            class="w-full bg-[--theme-primary] text-white py-3 px-6 rounded-md text-lg font-medium hover:bg-[--theme-primary-dark] transition-colors"
          >
            Agregar al carrito
          </button>
          
          <div v-if="allowOrders && product.stock === 0" class="text-center py-3">
            <span class="text-red-600 font-medium">Producto sin stock</span>
          </div>
          
          <!-- Botón de WhatsApp -->
          <button
            v-if="whatsappUrl"
            @click="contactWhatsApp"
            class="w-full bg-green-500 text-white py-3 px-6 rounded-md font-medium hover:bg-green-600 transition-colors flex items-center justify-center"
          >
            <ChatBubbleLeftRightIcon class="h-5 w-5 mr-2" />
            Consultar por WhatsApp
          </button>
          
          <button
            @click="goBack"
            class="w-full border border-gray-300 text-gray-700 py-2 px-6 rounded-md font-medium hover:bg-gray-50 transition-colors"
          >
            Volver al catálogo
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useCatalogStore } from '@/stores/catalog'
import { useCompanyStore } from '@/stores/company'
import { PhotoIcon, StarIcon, ChatBubbleLeftRightIcon } from '@heroicons/vue/24/outline'
import LoadingSpinner from '@/components/ui/Loading.vue'

// Composables
const router = useRouter()
const route = useRoute()
const catalogStore = useCatalogStore()
const companyStore = useCompanyStore()

// State
const currentImageIndex = ref(0)

// Computed
const product = computed(() => catalogStore.currentProduct)
const loading = computed(() => catalogStore.loadingProduct)
const error = computed(() => catalogStore.productError)
const showPrices = computed(() => companyStore.showPrices)
const showStock = computed(() => companyStore.showStock)
const allowOrders = computed(() => companyStore.allowOrders)
const whatsappUrl = computed(() => companyStore.whatsappUrl)

const currentImage = computed(() => {
  if (!product.value?.imagen_urls || product.value.imagen_urls.length === 0) {
    return null
  }
  return product.value.imagen_urls[currentImageIndex.value]
})

const stockColorClass = computed(() => {
  if (!product.value || product.value.stock === null) return 'text-gray-600'
  if (product.value.stock === 0) return 'text-red-600'
  if (product.value.stock < 10) return 'text-yellow-600'
  return 'text-green-600'
})

// Methods
const formatPrice = (price: number) => {
  return new Intl.NumberFormat('es-AR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(price)
}

const handleImageError = (event: Event) => {
  const target = event.target as HTMLImageElement
  target.style.display = 'none'
}

const addToCart = () => {
  // TODO: Implementar funcionalidad de carrito
  console.log('Agregar al carrito:', product.value?.codigo)
}

const contactWhatsApp = () => {
  if (!whatsappUrl.value || !product.value) return
  
  const message = encodeURIComponent(
    `Hola, me interesa el producto: ${product.value.nombre} (Código: ${product.value.codigo})`
  )
  
  const url = whatsappUrl.value.includes('?') 
    ? `${whatsappUrl.value}&text=${message}`
    : `${whatsappUrl.value}?text=${message}`
  
  window.open(url, '_blank')
}

const goBack = () => {
  router.back()
}

const retry = () => {
  const codigo = route.params.codigo as string
  if (codigo) {
    catalogStore.fetchProduct(codigo)
  }
}

const loadProduct = async () => {
  const codigo = route.params.codigo as string
  if (codigo) {
    await catalogStore.fetchProduct(codigo)
    // Update page title
    if (product.value) {
      companyStore.updateTitle(product.value.nombre)
    }
  }
}

// Watch for route changes
watch(() => route.params.codigo, () => {
  currentImageIndex.value = 0
  loadProduct()
})

// Initialize
onMounted(() => {
  loadProduct()
})
</script>