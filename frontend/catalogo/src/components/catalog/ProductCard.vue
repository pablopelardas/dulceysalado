<template>
  <article 
    :class="[
      'card-product group rounded-lg shadow-sm bg-white h-full transition-all hover:shadow-md relative cursor-pointer',
      viewMode === 'list' 
        ? 'flex items-center gap-4 px-3 py-2' 
        : 'flex flex-col p-3'
    ]"
    @click="openAddToCartModal"
  >
    <!-- Imagen -->
    <div 
      :class="viewMode === 'list' ? 'w-32 h-32 flex-shrink-0' : 'w-full mb-2'"
      :style="viewMode === 'grid' ? 'height: 350px;' : ''"
    >
      <div class="w-full h-full bg-gray-50 rounded-md overflow-hidden">
        <img 
          v-if="product.imagen_urls?.length" 
          :src="product.imagen_urls[0]" 
          :alt="product.nombre"
          class="w-full h-full object-cover"
        />
        <div v-else class="w-full h-full flex items-center justify-center">
          <ShoppingBagIcon class="h-10 w-10 text-gray-300" />
        </div>
      </div>
    </div>

    <!-- Contenido -->
    <div :class="[
      viewMode === 'list' ? 'flex-1 pr-12 sm:pr-0' : 'w-full text-center flex flex-col flex-1 pb-12 sm:pb-0'
    ]">
      <h3 class="text-sm font-medium text-gray-800 mb-1 line-clamp-2 leading-snug flex-shrink-0">
        {{ product.nombre }}
      </h3>

      <div class="mt-auto">
        <div class="text-base font-semibold text-gray-900">
          ${{ product.precio }}
        </div>

        <div v-if="product.lista" class="text-xs text-gray-500 mt-0.5">
          {{ product.lista }}
        </div>
      </div>
    </div>
    
    <!-- Add to cart button (mobile only) -->
    <button
      @click="openAddToCartModal"
      :class="[
        'sm:hidden rounded-full shadow-lg cursor-pointer',
        viewMode === 'list' 
          ? 'absolute right-3 top-1/2 -translate-y-1/2 p-2'
          : 'absolute bottom-3 right-3 p-3'
      ]"
      :style="{ background: 'var(--theme-accent)' }"
      aria-label="Agregar a mi lista"
    >
      <PlusIcon :class="viewMode === 'list' ? 'w-4 h-4' : 'w-5 h-5'" class="text-white" />
    </button>
    
    <!-- Desktop hover caption -->
    <div class="hidden sm:block absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-100 pointer-events-none">
      <div class="absolute inset-0 bg-black/10 rounded-lg flex items-center justify-center">
        <div class="bg-white/90 text-gray-800 px-3 py-1.5 rounded-md shadow-md text-sm font-medium">
          Agregar a mi lista
        </div>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { ShoppingBagIcon, PlusIcon } from '@heroicons/vue/24/outline'

interface Props {
  product: any
  viewMode?: 'grid' | 'list'
}

const props = withDefaults(defineProps<Props>(), {
  viewMode: 'grid'
})

const emit = defineEmits<{
  openCart: [product: any]
}>()

// Methods
const openAddToCartModal = () => {
  emit('openCart', props.product)
}
</script>