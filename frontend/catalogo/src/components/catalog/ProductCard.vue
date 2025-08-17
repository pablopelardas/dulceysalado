<!-- ProductCard.vue - Tarjeta de producto con Tailwind CSS -->
<template>
  <article 
    :class="[
      'group relative bg-white rounded-lg shadow-sm hover:shadow-lg transition-all duration-200 cursor-pointer overflow-hidden h-full',
      viewMode === 'grid' ? 'flex flex-col' : 'flex flex-row items-center gap-4 p-4',
      product.destacado ? 'border-2 hover:shadow-xl hover:-translate-y-1.5' : 'hover:-translate-y-1'
    ]"
    :style="product.destacado ? { borderColor: 'var(--theme-accent)' } : {}"
    @click="handleCardClick"
  >
    <!-- Badge de destacado -->
    <div 
      v-if="product.destacado" 
      class="absolute top-3 left-3 z-10 flex items-center gap-1 px-2 py-1 rounded text-xs font-semibold text-white shadow-md"
      :style="{ backgroundColor: 'var(--theme-accent)' }"
    >
      <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"/>
      </svg>
      Destacado
    </div>

    <!-- Contenedor de imagen -->
    <div :class="[
      'relative overflow-hidden bg-gray-50',
      viewMode === 'grid' ? 'w-full h-64 md:h-48 rounded-t-lg' : 'w-32 h-32 flex-shrink-0 rounded-lg'
    ]">
      <img 
        v-if="product.imagen_urls?.length" 
        :src="product.imagen_urls[0]" 
        :alt="product.imagen_alt || product.nombre"
        class="w-full h-full object-cover transition-transform duration-200 hover:scale-105"
        loading="lazy"
      />
      <div v-else class="flex items-center justify-center w-full h-full text-gray-400">
        <svg class="w-12 h-12" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"/>
        </svg>
      </div>

      <!-- Overlay con indicador de agregar al carrito -->
      <div 
        class="absolute inset-0 bg-black/0 group-hover:bg-black/40 transition-all duration-200 flex items-center justify-center opacity-0 group-hover:opacity-100"
      >
        <div class="flex flex-col items-center gap-2 text-white">
          <div class="w-12 h-12 rounded-full bg-white/20 backdrop-blur-sm flex items-center justify-center">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z"/>
            </svg>
          </div>
          <span class="text-sm font-medium">Agregar al carrito</span>
        </div>
      </div>
    </div>

    <!-- Información del producto -->
    <div :class="[
      'flex-1 flex flex-col gap-2',
      viewMode === 'grid' ? 'p-4 text-center' : 'text-left'
    ]">
      <!-- Código del producto -->
      <div v-if="showCode" class="text-xs text-gray-500 font-medium tracking-wide">
        Código: {{ product.codigo }}
      </div>

      <!-- Nombre del producto -->
      <h3 class="font-semibold text-lg text-gray-900 leading-tight line-clamp-2">
        {{ product.nombre }}
      </h3>

      <!-- Descripción corta -->
      <p v-if="product.descripcion_corta" class="text-sm text-gray-600 leading-normal line-clamp-2">
        {{ product.descripcion_corta }}
      </p>

      <!-- Marca y unidad -->
      <div :class="[
        'flex gap-2',
        viewMode === 'grid' ? 'justify-center' : 'justify-start'
      ]">
        <span v-if="product.marca" class="text-xs text-gray-600 bg-gray-100 px-2 py-1 rounded font-medium">
          {{ product.marca }}
        </span>
        <span v-if="product.unidad" class="text-xs text-gray-600 bg-gray-100 px-2 py-1 rounded font-medium">
          {{ product.unidad }}
        </span>
      </div>

      <!-- Tags -->
      <div 
        v-if="product.tags?.length" 
        :class="[
          'flex gap-1 flex-wrap',
          viewMode === 'grid' ? 'justify-center' : 'justify-start'
        ]"
      >
        <span 
          v-for="tag in product.tags.slice(0, 3)" 
          :key="tag"
          class="text-xs text-white px-2 py-1 rounded-full font-medium"
          :style="{ backgroundColor: 'var(--theme-accent)' }"
        >
          {{ tag }}
        </span>
      </div>

      <!-- Precio y información de lista -->
      <div class="mt-auto pt-2">
        <div 
          v-if="showPrice && product.precio" 
          :class="[
            'flex flex-col gap-1',
            viewMode === 'grid' ? 'items-center' : 'items-start'
          ]"
        >
          <span class="text-xl font-bold" :style="{ color: 'var(--theme-accent)' }">
            ${{ formatPrice(product.precio) }}
          </span>
          <span v-if="product.lista_precio_nombre" class="text-xs text-gray-500 font-medium">
            {{ product.lista_precio_nombre }}
          </span>
        </div>

        <!-- Stock (si está habilitado) -->
        <div 
          v-if="showStock && product.stock !== null" 
          :class="[
            'mt-2',
            viewMode === 'grid' ? 'text-center' : 'text-left'
          ]"
        >
          <span 
            :class="[
              'text-xs px-2 py-1 rounded font-semibold',
              product.stock > 0 ? 'bg-green-500 text-white' : 'bg-red-500 text-white'
            ]"
          >
            {{ product.stock > 0 ? `${product.stock} disponibles` : 'Sin stock' }}
          </span>
        </div>
      </div>
    </div>


    <!-- Indicador de carga -->
    <div v-if="loading" class="absolute inset-0 bg-white/80 backdrop-blur-sm z-30 flex items-center justify-center">
      <div 
        class="w-8 h-8 border-3 border-gray-200 rounded-full animate-spin"
        :style="{ borderTopColor: 'var(--theme-accent)' }"
      ></div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Product } from '@/services/api'
import EMPRESA_CONFIG from '@/config/empresa.config'

interface Props {
  product: Product
  viewMode?: 'grid' | 'list'
  showCode?: boolean
  showQuickView?: boolean
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  viewMode: 'grid',
  showCode: false,
  showQuickView: true,
  loading: false
})

const emit = defineEmits<{
  openCart: [product: Product]
  quickView: [product: Product]
  cardClick: [product: Product]
}>()

// Computed
const showPrice = computed(() => EMPRESA_CONFIG.mostrarPrecios)
const showStock = computed(() => EMPRESA_CONFIG.mostrarStock)

// Methods
const formatPrice = (price: number): string => {
  return new Intl.NumberFormat('es-AR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(price)
}

const handleAddToCart = () => {
  emit('openCart', props.product)
}

const handleQuickView = () => {
  emit('quickView', props.product)
}

const handleCardClick = () => {
  // Al hacer click en cualquier parte de la tarjeta, abre el modal de agregar al carrito
  emit('openCart', props.product)
}
</script>

