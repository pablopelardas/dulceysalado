<template>
  <div class="flex items-center justify-between px-4 py-3 rounded-lg" :style="{ background: 'rgba(255, 255, 255, 0.95)', backdropFilter: 'blur(10px)', border: '1px solid rgba(255, 255, 255, 0.2)' }">
    <!-- Mobile -->
    <div class="flex items-center justify-between sm:hidden">
      <!-- Prev Arrow -->
      <button
        @click="$emit('prev')"
        :disabled="!hasPrev"
        class="mobile-arrow-btn"
        :style="{ color: hasPrev ? 'var(--theme-primary)' : 'var(--theme-text)', opacity: hasPrev ? 1 : 0.3 }"
      >
        <ChevronLeftIcon class="w-6 h-6" />
      </button>
      
      <!-- Page indicator with more space -->
      <div class="flex-1 text-center">
        <div class="text-lg font-semibold" :style="{ color: 'var(--theme-text)' }">
          Página <span class="text-xl" :style="{ color: 'var(--theme-primary)' }">{{ currentPage }}</span> de <span class="text-xl" :style="{ color: 'var(--theme-primary)' }">{{ totalPages }}</span>
        </div>
      </div>
      
      <!-- Next Arrow -->
      <button
        @click="$emit('next')"
        :disabled="!hasNext"
        class="mobile-arrow-btn"
        :style="{ color: hasNext ? 'var(--theme-primary)' : 'var(--theme-text)', opacity: hasNext ? 1 : 0.3 }"
      >
        <ChevronRightIcon class="w-6 h-6" />
      </button>
    </div>

    <!-- Desktop -->
    <div class="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between sm:gap-5">
      <p class="text-sm" :style="{ color: 'var(--theme-text)' }">
        Página <span class="font-semibold" :style="{ color: 'var(--theme-primary)' }">{{ currentPage }}</span> de
        <span class="font-semibold" :style="{ color: 'var(--theme-primary)' }">{{ totalPages }}</span>
      </p>

      <nav class="flex items-center space-x-1" aria-label="Pagination">
        <!-- Prev -->
        <button
          @click="$emit('prev')"
          :disabled="!hasPrev"
          class="nav-arrow"
          :style="{ color: hasPrev ? 'var(--theme-primary)' : 'var(--theme-text)', opacity: hasPrev ? 1 : 0.3 }"
        >
          <ChevronLeftIcon class="w-5 h-5" />
        </button>

        <!-- Page Numbers -->
        <template v-for="page in visiblePages" :key="page">
          <button
            v-if="page !== '...'"
            @click="$emit('goto', page)"
            class="page-number"
            :class="{ 'active': page === currentPage }"
            :style="getPageButtonStyle(page === currentPage)"
          >
            {{ page }}
          </button>
          <span
            v-else
            class="px-3 py-1.5 text-sm select-none"
            :style="{ color: 'var(--theme-text)', opacity: 0.4 }"
          >
            ...
          </span>
        </template>

        <!-- Next -->
        <button
          @click="$emit('next')"
          :disabled="!hasNext"
          class="nav-arrow"
          :style="{ color: hasNext ? 'var(--theme-primary)' : 'var(--theme-text)', opacity: hasNext ? 1 : 0.3 }"
        >
          <ChevronRightIcon class="w-5 h-5" />
        </button>
      </nav>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { ChevronLeftIcon, ChevronRightIcon } from '@heroicons/vue/20/solid'
import { useTheme } from '@/composables/useTheme'

// Props
interface Props {
  currentPage: number
  totalPages: number
  hasNext: boolean
  hasPrev: boolean
  maxVisible?: number
}

const props = withDefaults(defineProps<Props>(), {
  maxVisible: 7
})

// Emits
defineEmits<{
  next: []
  prev: []
  goto: [page: number | string]
}>()

// Theme
const { secondaryTextColor } = useTheme()

// Methods
const getPageButtonStyle = (isActive: boolean) => {
  if (isActive) {
    return {
      background: 'var(--theme-primary)',
      color: '#ffffff',
      boxShadow: '0 2px 8px rgba(0, 0, 0, 0.15)'
    }
  }
  return {
    background: 'rgba(255, 255, 255, 0.8)',
    color: 'var(--theme-text)',
    border: '1px solid rgba(0, 0, 0, 0.1)'
  }
}

// Computed
const visiblePages = computed(() => {
  const pages: (number | string)[] = []
  const { currentPage, totalPages, maxVisible } = props
  
  if (totalPages <= maxVisible) {
    // Mostrar todas las páginas
    for (let i = 1; i <= totalPages; i++) {
      pages.push(i)
    }
  } else {
    // Mostrar páginas con elipsis
    const halfVisible = Math.floor(maxVisible / 2)
    
    // Siempre mostrar la primera página
    pages.push(1)
    
    let start = Math.max(2, currentPage - halfVisible)
    let end = Math.min(totalPages - 1, currentPage + halfVisible)
    
    // Ajustar para mantener maxVisible páginas
    if (end - start + 1 < maxVisible - 2) {
      if (start === 2) {
        end = Math.min(totalPages - 1, start + maxVisible - 3)
      } else {
        start = Math.max(2, end - maxVisible + 3)
      }
    }
    
    // Agregar elipsis antes si es necesario
    if (start > 2) {
      pages.push('...')
    }
    
    // Agregar páginas del medio
    for (let i = start; i <= end; i++) {
      pages.push(i)
    }
    
    // Agregar elipsis después si es necesario
    if (end < totalPages - 1) {
      pages.push('...')
    }
    
    // Siempre mostrar la última página
    if (totalPages > 1) {
      pages.push(totalPages)
    }
  }
  
  return pages
})
</script>

<style scoped>
 @reference "tailwindcss";

.mobile-nav-btn {
  @apply rounded-md px-3 py-2 text-sm font-medium transition duration-200 cursor-pointer;
}

.mobile-nav-btn:hover {
  filter: brightness(1.1);
}

.mobile-nav-btn:disabled {
  @apply opacity-50 cursor-not-allowed;
}

.mobile-nav-btn:disabled:hover {
  filter: none;
}

.nav-arrow {
  @apply p-2 rounded-md transition duration-200 cursor-pointer;
}

.nav-arrow:hover:not(:disabled) {
  background: rgba(0, 0, 0, 0.05);
}

.nav-arrow:disabled {
  @apply cursor-not-allowed;
}

.page-number {
  @apply px-3 py-1.5 rounded-md text-sm font-medium transition duration-200 cursor-pointer;
}

.page-number:hover {
  filter: brightness(0.95);
}

.page-number.active {
  transform: scale(1.05);
}

.mobile-arrow-btn {
  @apply p-3 rounded-full transition duration-200 cursor-pointer;
  min-width: 48px;
  min-height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.mobile-arrow-btn:hover:not(:disabled) {
  background: rgba(0, 0, 0, 0.05);
}

.mobile-arrow-btn:disabled {
  @apply cursor-not-allowed;
}
</style>