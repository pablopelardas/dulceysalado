<template>
  <div class="flex justify-center items-center" :class="containerClass">
    <div class="animate-spin rounded-full border-b-2" :class="spinnerClass">
    </div>
    <span v-if="text" class="ml-3 text-gray-600" :class="textClass">{{ text }}</span>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

// Props
interface Props {
  size?: 'sm' | 'md' | 'lg'
  text?: string
  color?: string
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  color: 'var(--theme-primary)'
})

// Computed
const containerClass = computed(() => {
  switch (props.size) {
    case 'sm':
      return 'py-2'
    case 'lg':
      return 'py-8'
    default:
      return 'py-4'
  }
})

const spinnerClass = computed(() => {
  const sizeClass = {
    sm: 'h-4 w-4',
    md: 'h-8 w-8',
    lg: 'h-12 w-12'
  }[props.size]
  
  return `${sizeClass} border-[--theme-primary]`
})

const textClass = computed(() => {
  switch (props.size) {
    case 'sm':
      return 'text-sm'
    case 'lg':
      return 'text-lg'
    default:
      return 'text-base'
  }
})
</script>